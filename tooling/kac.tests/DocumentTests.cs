using kac.core;
using Markdig;
using Markdig.Syntax;

namespace kac.tests;

public class DocumentTests
{
    [Fact]
    public void Md_PlainText_flattens_literals_and_inline_code()
    {
        var h1 = Markdown.Parse("# Hello `code` world").Descendants<HeadingBlock>().First();
        Assert.Equal("Hello code world", Md.PlainText(h1.Inline));
    }

    [Fact]
    public void Doc_Parse_reads_frontmatter_keys_scalars_and_h1()
    {
        const string text = "---\nid: adr-0001\nstatus: accepted\n---\n\n# ADR-0001: A title\n";
        var doc = Doc.Parse("adrs/0001-a-title.md", text, new Schema());

        Assert.NotNull(doc);
        Assert.Equal(["id", "status"], doc.FrontKeys);
        Assert.Equal("adr-0001", doc.FrontScalar("id"));
        Assert.Equal("ADR-0001: A title", doc.H1);
    }

    [Fact]
    public void Doc_Parse_returns_null_without_frontmatter()
        => Assert.Null(Doc.Parse("notes.md", "# Just a heading, no frontmatter\n", new Schema()));

    [Fact]
    public void Doc_Parse_reads_a_complex_frontmatter_key_as_the_empty_key()
    {
        const string text = "---\nid: adr-0001\n? [a, b]\n: value\n---\n\n# ADR-0001: A title\n";
        var doc = Doc.Parse("adrs/0001-a-title.md", text, new Schema());

        Assert.NotNull(doc);
        Assert.NotNull(doc.Front);
        Assert.Equal(["id", ""], doc.FrontKeys);
    }

    [Fact]
    public void Doc_Parse_leaves_frontmatter_null_where_the_yaml_will_not_read()
    {
        const string text = "---\nid: \"unterminated\nstatus: accepted\n---\n\n# A title\n";
        var doc = Doc.Parse("adrs/0001-a-title.md", text, new Schema());

        Assert.NotNull(doc);
        Assert.Null(doc.Front);
        Assert.Equal("A title", doc.H1);
    }

    [Theory]
    [InlineData("`Policy: pol-SCRT` `DRAFT`", new[] { "Policy: pol-SCRT", "DRAFT" })]
    [InlineData("`pol-SCRT` `DRAFT`", new[] { "pol-SCRT", "DRAFT" })]
    [InlineData("`Policy: pol-SCRT`", new[] { "Policy: pol-SCRT" })]
    [InlineData("`Policy: pol-SCRT` `DRAFT` `EXTRA`", new[] { "Policy: pol-SCRT", "DRAFT", "EXTRA" })]
    public void Identity_line_yields_its_code_spans_in_order(string line, string[] expected)
    {
        var doc = Doc.Parse("policies/scrt-a-title.md",
            $"---\nid: pol-SCRT\n---\n\n# Secrets are managed\n\n{line}\n", new Schema());

        Assert.NotNull(doc);
        Assert.Equal(expected, doc.IdentitySpans);
        Assert.Equal(7, doc.IdentityLine);
    }

    [Theory]
    [InlineData("## Purpose\n")]                                        // straight into a section
    [InlineData("Some opening prose.\n\n`Policy: pol-SCRT` `DRAFT`\n")] // line, but not first
    [InlineData("> A Y-statement block-quote.\n")]                      // the wrong kind of block
    [InlineData("")]                                                    // nothing at all after the H1
    public void No_identity_line_when_the_block_after_the_h1_is_something_else(string after)
    {
        var doc = Doc.Parse("policies/scrt-a-title.md",
            $"---\nid: pol-SCRT\n---\n\n# Secrets are managed\n\n{after}", new Schema());

        Assert.NotNull(doc);
        Assert.Null(doc.IdentitySpans);
    }

    [Theory]
    [InlineData("## Context\n\nWhy.\n", "Why.")]
    [InlineData("## Context\n\n### A sub-heading\n\nWhy.\n", "### A sub-heading\n\nWhy.")]
    [InlineData("## Context\n\nWhy.\n\n## Decision\n\nWhat.\n", "Why.")]
    [InlineData("## Context\n\nWhy.\n\n# A second title\n\nMore.\n", "Why.")]
    [InlineData("## Context\n", "")]
    [InlineData("## Context\n\n## Decision\n", "")]
    public void A_section_body_runs_to_the_next_heading_at_its_level_or_above(string body, string expected)
    {
        var doc = Doc.Parse("adrs/0001-a-title.md",
            $"---\nid: adr-0001\n---\n\n# A title\n\n{body}", new Schema());

        Assert.NotNull(doc);
        var section = doc.Sections[0];
        Assert.Equal(expected, doc.Text[section.BodyStart..section.BodyEnd].Trim());
    }

    [Fact]
    public void Sections_are_read_in_order_with_their_lines()
    {
        var doc = Doc.Parse("adrs/0001-a-title.md",
            "---\nid: adr-0001\n---\n\n# A title\n\n## Context\n\nWhy.\n\n## Decision\n\nWhat.\n", new Schema());

        Assert.NotNull(doc);
        Assert.Equal(["Context", "Decision"], doc.Sections.Select(s => s.Title));
        Assert.Equal([7, 11], doc.Sections.Select(s => s.Line));
    }

    [Theory]
    [InlineData("Why.", true)]
    [InlineData("### A sub-heading", true)]
    [InlineData("<!-- a note to whoever writes this -->", true)]
    [InlineData("", false)]
    [InlineData("\n\n", false)]
    [InlineData("---", false)]
    [InlineData("—", false)]
    [InlineData("*", false)]
    public void Content_is_a_letter_or_a_digit(string text, bool expected)
        => Assert.Equal(expected, Md.HasContent(text));

    [Fact]
    public void A_checkbox_is_read_as_a_marker_and_a_bracket_in_prose_as_a_candidate_reference()
    {
        var doc = Doc.Parse("adrs/0001-a-title.md", """
                                                    ---
                                                    id: adr-0001
                                                    ---

                                                    # A title

                                                    - [ ] a box to tick
                                                    - [x] one already ticked
                                                    - [ADR-0099] opening an item, and no checkbox
                                                    - a bullet mentioning [a placeholder] in passing

                                                    Prose marking a choice [x] in the middle of a line.
                                                    """, new Schema());

        Assert.NotNull(doc);
        Assert.Equal(["ADR-0099", "a placeholder", "x"], doc.BareBracketTokens.Select(t => t.inner));
    }

    /// <summary>
    /// A schema sourcing a type's parts from a table, for the parse tests below. The folder must map to a type carrying
    /// a PartSpec, since a type that declares none is never read for parts at all, and the prefix must be there, since
    /// that is what tells a citation from a filename of the same shape.
    /// </summary>
    private static Schema WithClauses()
    {
        return new Schema
        {
            ByFolder = new Dictionary<string, TypeSchema>
            {
                ["policies"] = new()
                {
                    IdPrefix = "pol",
                    Parts = new PartSpec(PartSpec.Table, "", ["MUST"], ["SHOULD"]) { Section = "Clauses" }
                }
            }
        };
    }

    private static Doc? ParseWithClauses(string body) =>
        Doc.Parse("policies/scrt-a-title.md",
            $"---\nid: pol-SCRT\n---\n\n# Secrets are managed\n\n{body}", WithClauses());

    [Fact]
    public void Clause_rows_are_read_as_written()
    {
        var doc = ParseWithClauses("""
                                   ## Clauses

                                   | Id      | Clause                  |
                                   |---------|-------------------------|
                                   | `STORE` | **MUST** hold secrets   |
                                   | PLAIN   | SHOULD rotate them      |
                                   """);

        Assert.NotNull(doc);
        Assert.Equal(["Id", "Clause"], doc.PartTableHeaders);
        Assert.Collection(doc.Parts,
            first =>
            {
                Assert.Equal("STORE", first.Id);
                Assert.Equal("MUST hold secrets", first.Text);
                Assert.Equal("MUST", first.BoldLead);
            },
            second =>
            {
                // Written as prose, so no span to read an id from.
                Assert.Null(second.Id);
                Assert.Equal("PLAIN", second.IdText);
                // …and no bold run opening the clause.
                Assert.Null(second.BoldLead);
            });
    }

    [Fact]
    public void No_clause_table_when_the_section_holds_prose()
    {
        var doc = ParseWithClauses("## Clauses\n\n* We will hold secrets in a store.\n");

        Assert.NotNull(doc);
        Assert.Null(doc.PartTableHeaders);
        Assert.Empty(doc.Parts);
    }

    [Fact]
    public void A_table_outside_the_clause_section_is_not_read_as_clauses()
    {
        var doc = ParseWithClauses("""
                                   ## Alignment

                                   | Id      | Clause                |
                                   |---------|-----------------------|
                                   | `STORE` | **MUST** hold secrets |
                                   """);

        Assert.NotNull(doc);
        Assert.Null(doc.PartTableHeaders);
    }

    [Fact]
    public void Part_citations_are_collected_from_code_spans()
    {
        var doc = ParseWithClauses("Cites `pol-VURM.TIMEBOX`, `pol-vurm.lower`, `pol-VURM` and `DRAFT`.\n");

        Assert.NotNull(doc);
        Assert.Equal(["pol-VURM.TIMEBOX", "pol-vurm.lower"], doc.PartRefs.Select(r => r.Ref));
    }

    [Fact]
    public void Part_citations_are_collected_from_reference_link_labels()
    {
        var doc = ParseWithClauses("""
                                   See [pol-VURM.TIMEBOX], again in [pol-VURM.TIMEBOX], and [elsewhere].

                                   [elsewhere]: elsewhere.md
                                   [pol-VURM.TIMEBOX]: vurm-a-title.md#clauses
                                   """);

        Assert.NotNull(doc);
        Assert.Equal(["pol-VURM.TIMEBOX"], doc.PartRefs.Select(r => r.Ref));
    }

    [Fact]
    public void A_colon_citation_in_a_link_label_is_collected_as_one()
    {
        var doc = ParseWithClauses("""
                                   See [pol-VURM:TIMEBOX].

                                   [pol-VURM:TIMEBOX]: vurm-a-title.md#clauses
                                   """);

        Assert.NotNull(doc);
        Assert.Empty(doc.PartRefs);
        Assert.Equal(["pol-VURM:TIMEBOX"], doc.ColonCitations.Select(r => r.Ref));
    }

    [Fact]
    public void Part_citations_are_collected_from_inline_link_text()
    {
        var doc = ParseWithClauses(
            "See [pol-VURM.TIMEBOX](vurm-a-title.md#clauses) and [the table](vurm-a-title.md).\n");

        Assert.NotNull(doc);
        Assert.Equal(["pol-VURM.TIMEBOX"], doc.PartRefs.Select(r => r.Ref));
    }

    [Fact]
    public void A_part_id_hard_against_a_link_is_read_as_a_citation()
    {
        var doc = ParseWithClauses("""
                                   See [pol-VURM].TIMEBOX and [pol-VURM].WINDOW.

                                   [pol-VURM]: vurm-a-title.md
                                   """);

        Assert.NotNull(doc);
        Assert.Equal(["pol-VURM.TIMEBOX", "pol-VURM.WINDOW"], doc.PartRefs.Select(r => r.Ref));
    }

    [Fact]
    public void A_part_id_hard_against_an_inline_link_is_read_as_a_citation()
    {
        var doc = ParseWithClauses("See [pol-VURM](vurm-a-title.md).TIMEBOX for the window.\n");

        Assert.NotNull(doc);
        Assert.Equal(["pol-VURM.TIMEBOX"], doc.PartRefs.Select(r => r.Ref));
    }

    /// <summary>
    /// A pipe table re-parses its cells, and the part id is found by its offset in the document. A cell whose spans did
    /// not survive that re-parse would read the wrong character.
    /// </summary>
    [Fact]
    public void A_part_id_hard_against_a_link_in_a_table_cell_is_read_as_a_citation()
    {
        var doc = ParseWithClauses("""
                                   ## Clauses

                                   | Id      | Clause                                    |
                                   |---------|-------------------------------------------|
                                   | `STORE` | **MUST** hold secrets. See [pol-VURM].TIMEBOX |

                                   [pol-VURM]: vurm-a-title.md
                                   """);

        Assert.NotNull(doc);
        Assert.Equal(["pol-VURM.TIMEBOX"], doc.PartRefs.Select(r => r.Ref));
    }

    [Fact]
    public void A_colon_hard_against_a_link_is_collected_as_a_colon_citation()
    {
        var doc = ParseWithClauses("""
                                   See [pol-VURM]:TIMEBOX.

                                   [pol-VURM]: vurm-a-title.md
                                   """);

        Assert.NotNull(doc);
        Assert.Empty(doc.PartRefs);
        Assert.Equal(["pol-VURM:TIMEBOX"], doc.ColonCitations.Select(r => r.Ref));
    }

    [Fact]
    public void A_label_that_already_carries_the_citation_takes_nothing_after_the_bracket()
    {
        var doc = ParseWithClauses("""
                                   See [pol-VURM.TIMEBOX].Also read the rest.

                                   [pol-VURM.TIMEBOX]: vurm-a-title.md#clauses
                                   """);

        Assert.NotNull(doc);
        Assert.Equal(["pol-VURM.TIMEBOX"], doc.PartRefs.Select(r => r.Ref));
    }

    [Theory]
    [InlineData("See [pol-VURM]. The policy times it out.")]
    [InlineData("See [pol-VURM].")]
    [InlineData("See [pol-VURM], which times it out.")]
    public void A_full_stop_ending_a_sentence_is_not_a_citation(string prose)
    {
        var doc = ParseWithClauses($"{prose}\n\n[pol-VURM]: vurm-a-title.md\n");

        Assert.NotNull(doc);
        Assert.Empty(doc.PartRefs);
        Assert.Empty(doc.ColonCitations);
    }

    [Fact]
    public void A_part_id_after_an_external_label_is_passed_over()
    {
        var doc = ParseWithClauses("""
                                   Aligns with [ISO 27001:2022].A.8.24.

                                   [ISO 27001:2022]: https://www.iso.org/standard/27001
                                   """);

        Assert.NotNull(doc);
        Assert.Empty(doc.PartRefs);
        Assert.Empty(doc.ColonCitations);
    }

    [Fact]
    public void A_filename_shaped_like_a_citation_is_not_one()
    {
        var doc = ParseWithClauses(
            "See `pol-secrets-at-rest.holding-store` in `vurm-vulnerability-remediation.md`.\n");

        Assert.NotNull(doc);
        Assert.Equal(["pol-secrets-at-rest.holding-store"], doc.PartRefs.Select(r => r.Ref));
    }

    private static Schema WithMirrors(params string[] sections) => new()
    {
        ByFolder = new Dictionary<string, TypeSchema>
        {
            ["services"] = new()
            {
                DeclaredFields =
                    [.. sections.Select(s => new FieldSpec { Name = s.ToLowerInvariant(), MirrorsSection = s })]
            }
        }
    };

    private static Doc? ParseWithMirrors(string body, params string[] sections) =>
        Doc.Parse("services/catalogue-web.md",
            $"---\nid: svc-catalogue-web\n---\n\n# The catalogue site\n\n{body}", WithMirrors(sections));

    [Fact]
    public void Links_are_collected_under_each_mirrored_section()
    {
        var doc = ParseWithMirrors("""
                                   ## Dependencies

                                   * [svc-search](search.md) — the search box.

                                   ### Not an edge

                                   * [svc-lending](lending.md) — over the bus, inside the same section.

                                   ## Data

                                   * [dat-catalogue](../data/catalogue.md) — the bibliographic record.
                                   """, "Dependencies", "Data");

        Assert.NotNull(doc);
        Assert.Equal(["search.md", "lending.md"],
            doc.MirroredSectionLinks["Dependencies"].Select(l => l.Target));
        Assert.Equal(["../data/catalogue.md"], doc.MirroredSectionLinks["Data"].Select(l => l.Target));
    }

    [Fact]
    public void Links_written_as_prose_are_collected_as_readily_as_bullets()
    {
        var doc = ParseWithMirrors("""
                                   ## Dependencies

                                   None, and the graph is wrong: it reaches [svc-search](search.md) all the same.
                                   """, "Dependencies");

        Assert.NotNull(doc);
        Assert.Equal(["search.md"], doc.MirroredSectionLinks["Dependencies"].Select(l => l.Target));
    }

    [Fact]
    public void A_mirrored_section_the_document_lacks_is_collected_as_empty()
    {
        var doc = ParseWithMirrors("## Environments\n\n* [svc-search](search.md)\n", "Dependencies");

        Assert.NotNull(doc);
        Assert.Empty(doc.MirroredSectionLinks["Dependencies"]);
    }

    private static Schema WithFootnotes(params string[] labels) => new()
    {
        ByFolder = new Dictionary<string, TypeSchema>
        {
            ["policies"] = new()
            {
                IdPrefix = "pol",
                DeclaredFields =
                [
                    .. labels.Select((label, i) =>
                        new FieldSpec { Name = $"field{i}", MirrorsCitations = label })
                ]
            }
        }
    };

    private static Doc? ParseWithFootnotes(string body, params string[] labels) =>
        Doc.Parse("policies/scrt-a-title.md",
            $"---\nid: pol-SCRT\n---\n\n# Secrets are managed\n\n{body}",
            WithFootnotes(labels.Length > 0 ? labels : ["Covers"]));

    [Fact]
    public void A_labelled_line_closing_a_section_gathers_its_citations()
    {
        var doc = ParseWithFootnotes("""
                                     ## A rule

                                     Something binding.

                                     _**Covers:** [pol-VURM].TIMEBOX, `eng:pol-VURM.WINDOW`_

                                     [pol-VURM]: vurm-a-title.md
                                     """);

        Assert.NotNull(doc);
        var footnote = Assert.Single(doc.CitationFootnotes["Covers"]);
        Assert.True(footnote.ClosesSection);
        Assert.Equal(["pol-VURM.TIMEBOX", "eng:pol-VURM.WINDOW"], footnote.Citations);
    }

    [Fact]
    public void An_emphasis_delimiter_is_not_read_into_the_part_id()
    {
        var doc = ParseWithFootnotes("""
                                     ## A rule

                                     _**Covers:** [pol-VURM].TIMEBOX_

                                     [pol-VURM]: vurm-a-title.md
                                     """);

        Assert.NotNull(doc);
        Assert.Equal(["pol-VURM.TIMEBOX"], doc.PartRefs.Select(r => r.Ref));
    }

    [Fact]
    public void A_bold_label_that_is_not_italic_is_not_a_footnote()
    {
        var doc = ParseWithFootnotes("""
                                     ## A rule

                                     **Covers:** [pol-VURM].TIMEBOX, and the rest of the sentence.

                                     [pol-VURM]: vurm-a-title.md
                                     """);

        Assert.NotNull(doc);
        Assert.Empty(doc.CitationFootnotes["Covers"]);
    }

    [Theory]
    [InlineData("_**Covers:** [pol-VURM].TIMEBOX_\n\nMore prose under the same heading.")]
    [InlineData("- A bullet, and beneath it:\n\n  _**Covers:** [pol-VURM].TIMEBOX_")]
    public void A_labelled_line_that_closes_no_section_is_collected_as_misplaced(string body)
    {
        var doc = ParseWithFootnotes($"## A rule\n\n{body}\n\n[pol-VURM]: vurm-a-title.md\n");

        Assert.NotNull(doc);
        var footnote = Assert.Single(doc.CitationFootnotes["Covers"]);
        Assert.False(footnote.ClosesSection);
        Assert.Equal(["pol-VURM.TIMEBOX"], footnote.Citations);
    }

    /// <summary>
    /// The definitions render as nothing and markdig gathers them at the end of every document, so a line closing the
    /// last section would read as standing in the middle of one.
    /// </summary>
    [Fact]
    public void A_line_closing_the_last_section_closes_it_despite_the_definitions_beneath()
    {
        var doc = ParseWithFootnotes("""
                                     ## The last rule

                                     _**Covers:** [pol-VURM].TIMEBOX_

                                     [pol-VURM]: vurm-a-title.md
                                     """);

        Assert.NotNull(doc);
        Assert.True(Assert.Single(doc.CitationFootnotes["Covers"]).ClosesSection);
    }

    [Fact]
    public void A_label_no_line_carries_is_collected_as_empty()
    {
        var doc = ParseWithFootnotes("## A rule\n\n_**Notes:** nothing to see_\n");

        Assert.NotNull(doc);
        Assert.Empty(doc.CitationFootnotes["Covers"]);
    }

    [Fact]
    public void A_declared_label_carrying_the_colon_matches_the_line_anyway()
    {
        var doc = ParseWithFootnotes("""
                                     ## A rule

                                     _**Covers:** [pol-VURM].TIMEBOX_

                                     [pol-VURM]: vurm-a-title.md
                                     """, "Covers:");

        Assert.NotNull(doc);
        Assert.Equal(["pol-VURM.TIMEBOX"], Assert.Single(doc.CitationFootnotes["Covers:"]).Citations);
    }

    [Fact]
    public void Two_fields_spelling_one_label_two_ways_see_the_same_lines()
    {
        var doc = ParseWithFootnotes("""
                                     ## A rule

                                     _**Covers:** [pol-VURM].TIMEBOX_

                                     [pol-VURM]: vurm-a-title.md
                                     """, "Covers", "Covers:");

        Assert.NotNull(doc);
        Assert.Equal(["pol-VURM.TIMEBOX"], Assert.Single(doc.CitationFootnotes["Covers"]).Citations);
        Assert.Equal(["pol-VURM.TIMEBOX"], Assert.Single(doc.CitationFootnotes["Covers:"]).Citations);
    }

    /// <summary>
    /// A field derived from where a record sits. The path arithmetic is what these pin: the type's own folder comes off
    /// the front, the filename comes off the back, and whatever is left is the value.
    /// </summary>
    private static Schema DerivedCategory() => new()
    {
        ByFolder = new Dictionary<string, TypeSchema>
        {
            ["policies"] = new()
            {
                TypeName = "policy",
                Folder = "policies",
                Fields = new Dictionary<string, FieldSpec>
                {
                    ["category"] = new() { Name = "category", From = "sub-path" }
                }
            }
        }
    };

    [Theory]
    [InlineData("policies/flat.md", "")] // filed flat: no category, and none wanted
    [InlineData("policies/security/nested.md", "security")]
    [InlineData("policies/platform/node/deep.md", "platform/node")]
    public void Doc_Derived_reads_the_sub_path_under_the_type_folder(string rel, string expected)
    {
        var doc = Doc.Parse(rel, "---\nid: pol-AAAA\n---\n\n# A title\n", DerivedCategory());

        Assert.NotNull(doc);
        Assert.Equal(expected, doc.Derived("category"));
    }

    [Fact]
    public void Doc_Derived_is_null_for_a_field_the_type_does_not_derive()
    {
        var doc = Doc.Parse("policies/security/nested.md", "---\nid: pol-AAAA\n---\n\n# A title\n",
            DerivedCategory());

        Assert.NotNull(doc);
        Assert.Null(doc.Derived("id"));
    }

    [Fact]
    public void Doc_FrontScalar_prefers_the_derivation_to_a_hand_written_value()
    {
        var doc = Doc.Parse("policies/security/nested.md",
            "---\nid: pol-AAAA\ncategory: delivery\n---\n\n# A title\n", DerivedCategory());

        Assert.NotNull(doc);
        Assert.Equal("security", doc.FrontScalar("category"));
    }

    [Theory]
    [InlineData("policies/security/nested.md", true)]
    [InlineData("policies/flat.md", false)]
    public void Doc_FrontList_carries_a_derived_value_as_its_one_entry(string rel, bool present)
    {
        var doc = Doc.Parse(rel, "---\nid: pol-AAAA\n---\n\n# A title\n", DerivedCategory());

        Assert.NotNull(doc);
        Assert.Equal(present, doc.FrontList("category").Count > 0);
        Assert.Equal(doc.FrontScalar("category")?.Length > 0, doc.FrontList("category").Count > 0);
    }

    [Fact]
    public void Doc_FrontNode_is_null_where_the_derivation_is_empty()
    {
        var doc = Doc.Parse("policies/flat.md", "---\nid: pol-AAAA\n---\n\n# A title\n", DerivedCategory());

        Assert.NotNull(doc);
        Assert.Null(doc.FrontNode("category"));
    }
}
