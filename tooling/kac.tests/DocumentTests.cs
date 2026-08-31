using kac.core;
using Markdig;
using Markdig.Syntax;

// In-process unit tests for the markdown layer (Md plain-text flattening and Doc.Parse). The full
// parse over the real corpus is exercised by the golden suite; these pin the tricky bits directly.

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
        Assert.Equal(["id", "status"], doc.FrontKeys); // order preserved
        Assert.Equal("adr-0001", doc.FrontScalar("id"));
        Assert.Equal("ADR-0001: A title", doc.H1);
    }

    [Fact]
    public void Doc_Parse_returns_null_without_frontmatter()
        => Assert.Null(Doc.Parse("notes.md", "# Just a heading, no frontmatter\n", new Schema()));

    // A key that is itself a sequence is legal YAML and names no field. The validator reports it as the
    // empty key, rather than the parse stopping.
    [Fact]
    public void Doc_Parse_reads_a_complex_frontmatter_key_as_the_empty_key()
    {
        const string text = "---\nid: adr-0001\n? [a, b]\n: value\n---\n\n# ADR-0001: A title\n";
        var doc = Doc.Parse("adrs/0001-a-title.md", text, new Schema());

        Assert.NotNull(doc);
        Assert.NotNull(doc.Front);
        Assert.Equal(["id", ""], doc.FrontKeys);
    }

    // The parse still runs, so the document is asked about its prose. `frontmatter-parses` is what
    // reports the frontmatter.
    [Fact]
    public void Doc_Parse_leaves_frontmatter_null_where_the_yaml_will_not_read()
    {
        const string text = "---\nid: \"unterminated\nstatus: accepted\n---\n\n# A title\n";
        var doc = Doc.Parse("adrs/0001-a-title.md", text, new Schema());

        Assert.NotNull(doc);
        Assert.Null(doc.Front);
        Assert.Equal("A title", doc.H1);
    }

    // The parser makes no judgement about how many spans there should be or what they should say, so a
    // malformed line still arrives as data the validator can quote back.
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

    // Without an anchor on the block directly after the H1, a document opening with prose would borrow
    // an identity line from further down the page. The missing-line check could then never fire.
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

    // A sub-heading and everything under it belong to the section above them, and a second H1 ends the
    // section it follows.
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

    // A check reporting a section can point at the heading rather than at the document.
    [Fact]
    public void Sections_are_read_in_order_with_their_lines()
    {
        var doc = Doc.Parse("adrs/0001-a-title.md",
            "---\nid: adr-0001\n---\n\n# A title\n\n## Context\n\nWhy.\n\n## Decision\n\nWhat.\n", new Schema());

        Assert.NotNull(doc);
        Assert.Equal(["Context", "Decision"], doc.Sections.Select(s => s.Title));
        Assert.Equal([7, 11], doc.Sections.Select(s => s.Line));
    }

    // What `empty-section` reads a section's body for. A comment counts, because someone wrote it for
    // the next author; a rule, a bullet marker left behind and an em dash standing in for the words do
    // not, and the rendered blocks would offer all three as content.
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

    // What the bracket scan hands the validator to report as `bracket-literal` or `undefined-label`.
    // Position, not the brackets, is what decides a checkbox.
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

    // A schema sourcing a type's parts from a table, for the parse tests below. The folder must map to a
    // type carrying a PartSpec, since a type that declares none is never read for parts at all, and the
    // prefix must be there, since that is what tells a citation from a filename of the same shape.
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

    // A malformed row is a finding the validator words, and it can only word it if the parser declines
    // to fix the row on the way past.
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
                Assert.Null(second.Id); // written as prose, so no span to read an id from
                Assert.Equal("PLAIN", second.IdText);
                Assert.Null(second.BoldLead); // …and no bold run opening the clause
            });
    }

    // Null headers mean "the section holds no table", which is the finding. An empty list would say the
    // table is there and headed with nothing, and the two have different fixes.
    [Fact]
    public void No_clause_table_when_the_section_holds_prose()
    {
        var doc = ParseWithClauses("## Clauses\n\n* We will hold secrets in a store.\n");

        Assert.NotNull(doc);
        Assert.Null(doc.PartTableHeaders);
        Assert.Empty(doc.Parts);
    }

    // The section the schema names is what makes a table the clause table.
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

    // Case and width are the validator's to rule on, so a mis-cased citation is one it can report as
    // unresolved rather than one the parser silently never saw.
    [Fact]
    public void Part_citations_are_collected_from_code_spans()
    {
        var doc = ParseWithClauses("Cites `pol-VURM.TIMEBOX`, `pol-vurm.lower`, `pol-VURM` and `DRAFT`.\n");

        Assert.NotNull(doc);
        Assert.Equal(["pol-VURM.TIMEBOX", "pol-vurm.lower"], doc.PartRefs.Select(r => r.Ref));
    }

    // The other form a citation takes. One entry per label, however often the prose reaches for it.
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

    // The colon form reaches no part, so it has to be seen where it is written. A label carries it as
    // readily as a code span does.
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

    // An inline link carries its citation as text, where a reference carries it as a label.
    [Fact]
    public void Part_citations_are_collected_from_inline_link_text()
    {
        var doc = ParseWithClauses(
            "See [pol-VURM.TIMEBOX](vurm-a-title.md#clauses) and [the table](vurm-a-title.md).\n");

        Assert.NotNull(doc);
        Assert.Equal(["pol-VURM.TIMEBOX"], doc.PartRefs.Select(r => r.Ref));
    }

    // Two clauses of one policy, cited off one definition, which is what this form is for.
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

    // An inline link ends on its target, so the part id follows the parenthesis rather than a bracket.
    [Fact]
    public void A_part_id_hard_against_an_inline_link_is_read_as_a_citation()
    {
        var doc = ParseWithClauses("See [pol-VURM](vurm-a-title.md).TIMEBOX for the window.\n");

        Assert.NotNull(doc);
        Assert.Equal(["pol-VURM.TIMEBOX"], doc.PartRefs.Select(r => r.Ref));
    }

    // A clause table is where a cross-reference is written, and a pipe table re-parses its cells. The
    // part id is found by its offset in the document, so a cell whose spans did not survive that
    // re-parse would read the wrong character.
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

    // The separator is asked of this form as of the others, so a colon is named rather than passed over.
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

    // A label already carrying the whole citation ends it, so whatever follows the bracket is prose.
    // Reading a second part id onto it would build `pol-VURM.TIMEBOX.Also`, which matches no citation
    // form and would drop a citation the parser collects today.
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

    // A sentence ending on a link is ordinary prose, and reading its full stop as a separator would
    // report a part called 'The'.
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

    // A label with no type prefix is external, and the half after the dot answers to nothing here. This
    // is the Alignment column's form, and it stays as it was.
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

    // The prefix tells a citation from a filename: `pol` is a type's and `vurm` is not, so the filename
    // is passed over and never reported as a citation of nothing.
    [Fact]
    public void A_filename_shaped_like_a_citation_is_not_one()
    {
        var doc = ParseWithClauses(
            "See `pol-secrets-at-rest.holding-store` in `vurm-vulnerability-remediation.md`.\n");

        Assert.NotNull(doc);
        Assert.Equal(["pol-secrets-at-rest.holding-store"], doc.PartRefs.Select(r => r.Ref));
    }

    // A schema whose service type mirrors two fields against two sections, for the parse tests below.
    // Which sections are collected is decided by the fields, so a type declaring none is never walked
    // for one.
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

    // Inlines hang off leaf blocks, so a walk that descends from the top-level block finds the ones in
    // a list and silently misses the ones in prose. A link in prose is the same link to whoever wrote
    // it, and the difference between a section that reconciles and one that seems to.
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

    // The validator reports every id in the field as missing rather than skipping the field.
    [Fact]
    public void A_mirrored_section_the_document_lacks_is_collected_as_empty()
    {
        var doc = ParseWithMirrors("## Environments\n\n* [svc-search](search.md)\n", "Dependencies");

        Assert.NotNull(doc);
        Assert.Empty(doc.MirroredSectionLinks["Dependencies"]);
    }

    // A schema whose policy type declares a field reconciling against a labelled line, for the parse
    // tests below. The label is the field's to name, and the prefix is what tells a citation from prose,
    // so both come off the same declaration the real corpus reads.
    private static Schema WithFootnotes(string label) => new()
    {
        ByFolder = new Dictionary<string, TypeSchema>
        {
            ["policies"] = new()
            {
                IdPrefix = "pol",
                DeclaredFields = [new FieldSpec { Name = "implements", MirrorsCitations = label }]
            }
        }
    };

    private static Doc? ParseWithFootnotes(string body, string label = "Covers") =>
        Doc.Parse("policies/scrt-a-title.md",
            $"---\nid: pol-SCRT\n---\n\n# Secrets are managed\n\n{body}", WithFootnotes(label));

    // The form the corpus writes, in both notations: a link with the part id against the bracket for a
    // record the corpus holds, and a code span for one it imports.
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

    // The underscore closing the emphasis is markup, and a part id may carry one. Read off the source
    // the last citation on the line comes out as `TIMEBOX_`, which reaches no clause anybody wrote.
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

    // The two labelled forms are told apart by the italic and by the position. A bold label standing in
    // among the prose is the other one, and gathers nothing.
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

    // Collected rather than dropped, so the validator reports where the line sits instead of reporting
    // every id it named as absent from the document.
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

    // The definitions render as nothing and markdig gathers them at the end of every document, so a line
    // closing the last section would read as standing in the middle of one.
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

    // The validator reports every id in the field rather than skipping the field.
    [Fact]
    public void A_label_no_line_carries_is_collected_as_empty()
    {
        var doc = ParseWithFootnotes("## A rule\n\n_**Notes:** nothing to see_\n");

        Assert.NotNull(doc);
        Assert.Empty(doc.CitationFootnotes["Covers"]);
    }

    // The colon belongs to the form, so a schema writing it into the label still matches the line every
    // author writes. Left alone, the declaration would match nothing and say nothing about why.
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

    // A field derived from where a record sits. The path arithmetic is what these pin: the type's own
    // folder comes off the front, the filename comes off the back, and whatever is left is the value.
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
    [InlineData("policies/flat.md", "")]                       // filed flat: no category, and none wanted
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

    // The derived value wins over a written one, so the index, the sort and the export cannot disagree
    // with the folder while `derived-key` is reporting the line.
    [Fact]
    public void Doc_FrontScalar_prefers_the_derivation_to_a_hand_written_value()
    {
        var doc = Doc.Parse("policies/security/nested.md",
            "---\nid: pol-AAAA\ncategory: delivery\n---\n\n# A title\n", DerivedCategory());

        Assert.NotNull(doc);
        Assert.Equal("security", doc.FrontScalar("category"));
    }

    // `field()` and `present()` are two facts about one field, so a rule guarding on the second before
    // reading the first has to see them agree. `FrontList` is what `Facts.Present` asks.
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

    // An empty derivation reaches the export as an absent field rather than as an empty string, so a
    // flat record and one whose field was never declared read the same way to a consumer.
    [Fact]
    public void Doc_FrontNode_is_null_where_the_derivation_is_empty()
    {
        var doc = Doc.Parse("policies/flat.md", "---\nid: pol-AAAA\n---\n\n# A title\n", DerivedCategory());

        Assert.NotNull(doc);
        Assert.Null(doc.FrontNode("category"));
    }
}
