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
        Assert.Equal(["id", "status"], doc.FrontKeys);       // order preserved
        Assert.Equal("adr-0001", doc.FrontScalar("id"));
        Assert.Equal("ADR-0001: A title", doc.H1);
    }

    [Fact]
    public void Doc_Parse_returns_null_without_frontmatter()
        => Assert.Null(Doc.Parse("notes.md", "# Just a heading, no frontmatter\n", new Schema()));

    // The identity line's code spans are handed to the validator raw and in order — the parser makes
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
    [InlineData("## Purpose\n")]                                  // straight into a section
    [InlineData("Some opening prose.\n\n`Policy: pol-SCRT` `DRAFT`\n")]  // line, but not first
    [InlineData("> A Y-statement block-quote.\n")]                // the wrong kind of block
    [InlineData("")]                                              // nothing at all after the H1
    public void No_identity_line_when_the_block_after_the_h1_is_something_else(string after)
    {
        var doc = Doc.Parse("policies/scrt-a-title.md",
            $"---\nid: pol-SCRT\n---\n\n# Secrets are managed\n\n{after}", new Schema());

        Assert.NotNull(doc);
        Assert.Null(doc.IdentitySpans);
    }

    // A schema declaring clauses, for the parse tests below: the folder must map to a type carrying a
    // ClauseSpec, since a type that declares none is never read for a clause table at all.
    private static Schema WithClauses()
    {
        var schema = new Schema();
        schema.ByFolder["policies"] = new TypeSchema
        {
            Clauses = new ClauseSpec("", ["MUST"], ["SHOULD"]) { Section = "Clauses" }
        };
        return schema;
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
        Assert.Equal(["Id", "Clause"], doc.ClauseHeaders);
        Assert.Collection(doc.Clauses,
            first =>
            {
                Assert.Equal("STORE", first.IdSpan);
                Assert.Equal("MUST hold secrets", first.Text);
                Assert.Equal("MUST", first.BoldLead);
            },
            second =>
            {
                Assert.Null(second.IdSpan);       // written as prose, so no span to report
                Assert.Equal("PLAIN", second.IdText);
                Assert.Null(second.BoldLead);     // …and no bold run opening the clause
            });
    }

    // Null headers mean "the section holds no table", which is the finding. An empty list would say the
    // table is there and headed with nothing, and the two have different fixes.
    [Fact]
    public void No_clause_table_when_the_section_holds_prose()
    {
        var doc = ParseWithClauses("## Clauses\n\n* We will hold secrets in a store.\n");

        Assert.NotNull(doc);
        Assert.Null(doc.ClauseHeaders);
        Assert.Empty(doc.Clauses);
    }

    // A table under some other heading is not the clause table, however much it looks like one — the
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
        Assert.Null(doc.ClauseHeaders);
    }

    // Citations are collected from code spans anywhere in the document, and left unjudged: case and
    // width are the validator's to rule on, so a mis-cased citation is one it can report as unresolved
    // rather than one the parser silently never saw.
    [Fact]
    public void Clause_citations_are_collected_from_code_spans()
    {
        var doc = ParseWithClauses("Cites `pol-VURM.TIMEBOX`, `pol-vurm.lower`, `pol-VURM` and `DRAFT`.\n");

        Assert.NotNull(doc);
        Assert.Equal(["pol-VURM.TIMEBOX", "pol-vurm.lower"], doc.ClauseRefs.Select(r => r.Ref));
    }
}
