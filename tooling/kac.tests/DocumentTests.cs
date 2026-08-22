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

    // The identity line's code spans are handed to the validator raw and in order. The parser makes
    // no judgement about how many there should be or what they should say, so a malformed line still
    // arrives as data the validator can quote back.
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

    // Anchored on the block directly after the H1, not on the first paragraph of code spans anywhere.
    // Without that anchor a document that opened with prose would borrow an identity line from further
    // down the page and the missing-line check could never fire.
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

    // A section runs to the next heading at its own level or above, so a sub-heading and everything
    // under it belong to the section above them, and a second H1 ends the section it follows.
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

    // Every H2 in document order, each carrying the line it sits on, so a check reporting one can point
    // at the heading rather than at the document.
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
    // Position decides a checkbox: `[ ]` opening a list item is a marker, and the same brackets in
    // the middle of a sentence are a candidate reference like any other.
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

    // Rows arrive as written, not as they should be: an id that is not a single code span reports no
    // span but keeps its text, and a clause that opens with no bold run reports no lead. Every one of
    // those is a finding the validator words, and it can only word them if the parser declines to fix
    // them on the way past.
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

    // A table under some other heading is not the clause table, however much it looks like one. The
    // section the schema names is what makes it one.
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

    // Citations are collected from code spans anywhere in the document, and left unjudged: case and
    // width are the validator's to rule on, so a mis-cased citation is one it can report as unresolved
    // rather than one the parser silently never saw.
    [Fact]
    public void Part_citations_are_collected_from_code_spans()
    {
        var doc = ParseWithClauses("Cites `pol-VURM.TIMEBOX`, `pol-vurm.lower`, `pol-VURM` and `DRAFT`.\n");

        Assert.NotNull(doc);
        Assert.Equal(["pol-VURM.TIMEBOX", "pol-vurm.lower"], doc.PartRefs.Select(r => r.Ref));
    }

    // A record named by a slug carries hyphens on both sides of the dot, and a filename written as a
    // code span has every feature of that shape. The prefix is what tells them apart: `pol` is a type's
    // and `vurm` is not, so the filename is passed over and never reported as a citation of nothing.
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

    // Each field's own section, gathered in the one walk. A field mirroring `Dependencies` sees the
    // links under `## Dependencies` and nothing from the section beside it.
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

    // A link in a paragraph is a link. Inlines hang off leaf blocks, so a walk that descends from the
    // top-level block finds the ones in a list and silently misses the ones in prose. It is the same
    // link to whoever wrote it, and the difference between a section that reconciles and one that
    // seems to.
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

    // A section the document does not carry is an empty list rather than an absent key, so the
    // validator reports every id in the field as missing from it rather than skipping the field.
    [Fact]
    public void A_mirrored_section_the_document_lacks_is_collected_as_empty()
    {
        var doc = ParseWithMirrors("## Environments\n\n* [svc-search](search.md)\n", "Dependencies");

        Assert.NotNull(doc);
        Assert.Empty(doc.MirroredSectionLinks["Dependencies"]);
    }
}
