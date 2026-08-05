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

    // The index heading already names the type, so the title column carries only what distinguishes
    // one record from another. The last capture group is the title in every pattern the schema
    // declares — whether that group is the only one or the second of two.
    [Theory]
    [InlineData("^Policy: (.+)$", "Policy: Secrets are managed", "Secrets are managed")]
    [InlineData(@"^ADR-(\d{4}): (.+)$", "ADR-0001: Knowledge as code", "Knowledge as code")]
    [InlineData(null, "A type that declares no pattern", "A type that declares no pattern")]
    public void TitleText_strips_the_boilerplate_the_h1_pattern_declares(string? pattern, string h1, string expected)
    {
        var schema = new Schema();
        schema.ByFolder["policies"] = new TypeSchema { H1Pattern = pattern };
        var doc = Doc.Parse("policies/scrt-a-title.md", $"---\nid: pol-SCRT\n---\n\n# {h1}\n", schema);

        Assert.NotNull(doc);
        Assert.Equal(expected, doc.TitleText());
    }
}
