using kac.core;

// In-process unit tests for the decision the link pass turns on: whether a target names something the
// corpus holds. It is fiddly, it is quiet when wrong, and the goldens can only reach it through a whole
// corpus. Whether a bracketed label is an id is asked here too, but answered in IdChecks and tested
// beside it.
//
// Driven through `LinkChecks.Check`, which is what the validator calls, against a `Tree` built from a
// listing. The resolver reads the corpus and never the disk, so the corpus a test needs is a set of
// paths and the text behind them.

namespace kac.tests;

public class LinkCheckTests
{
    // -- does this target resolve? --

    [Fact]
    public void A_target_resolves_absolute_from_the_root_or_relative_to_the_document()
    {
        Assert.Empty(Unresolved("adrs/0001-a.md", "[b](/adrs/0002-b.md)"));
        Assert.Empty(Unresolved("adrs/0001-a.md", "[b](0002-b.md)"));
        Assert.Empty(Unresolved("adrs/0001-a.md", "[up](../adrs.md)"));
        Assert.Single(Unresolved("adrs/0001-a.md", "[gone](/adrs/0099-gone.md)"));
    }

    // Azure DevOps resolves a link with the extension left off, so the corpus is written that way and
    // the check has to follow.
    [Fact]
    public void The_md_extension_may_be_omitted()
        => Assert.Empty(Unresolved("adrs/0001-a.md", "[b](/adrs/0002-b)"));

    // A directory is deliberately not a target: `/adrs` is a link to the page `adrs.md`.
    [Fact]
    public void A_directory_is_not_a_target_but_the_page_beside_it_is()
    {
        Assert.Empty(Unresolved("index.md", "[adrs](/adrs)"));      // resolves as adrs.md
        Assert.Single(Unresolved("index.md", "[pics](/pictures)")); // a folder with no page
    }

    // A file the repository ignores is not in the corpus. It is on the disk of whoever created it and in
    // no clone, so a link to one is dead everywhere the corpus is read.
    [Fact]
    public void A_target_the_corpus_does_not_hold_does_not_resolve()
        => Assert.Single(Unresolved("index.md", "[draft](/_plan/notes.md)"));

    // A fragment or a query addresses within a target. It never names a different one.
    [Theory]
    [InlineData("[b](/adrs/0002-b.md#context)")]
    [InlineData("[b](/adrs/0002-b.md?raw=1)")]
    public void A_fragment_or_query_is_stripped_before_the_target_is_looked_up(string markdown)
        => Assert.Empty(Unresolved("adrs/0001-a.md", markdown));

    // A bare fragment never reaches the resolver.
    [Fact]
    public void A_bare_fragment_names_a_heading_in_this_document()
    {
        Assert.Empty(Findings("adrs/0001-a.md", "# Context\n\n[here](#context)\n"));
        Assert.Single(Findings("adrs/0001-a.md", "# Context\n\n[there](#decision)\n"));
    }

    [Theory]
    [InlineData("https://example.com/a", true)]
    [InlineData("http://example.com/a", true)]
    [InlineData("mailto:someone@example.com", true)]
    [InlineData("/adrs/0002-b.md", false)]
    public void An_external_target_is_left_alone(string target, bool external)
        => Assert.Equal(external, LinkChecks.IsExternal(target));

    // -- the corpus these are asked against --

    private static List<string> Unresolved(string fromRel, string markdown) =>
    [
        .. Findings(fromRel, markdown).Where(f => f.Check.Value == "link-resolves").Select(f => f.Message)
    ];

    private static List<Finding> Findings(string fromRel, string markdown)
    {
        var schema = new Schema();
        var doc = Doc.Parse(fromRel, markdown, schema, requireFrontmatter: false)!;
        var findings = new List<Finding>();
        LinkChecks.Check(doc, schema, Corpus(), new Report(doc.Rel, findings));
        return findings;
    }

    // What the corpus holds, as a listing. `_plan/notes.md` is deliberately absent: it is the file on
    // disk that git ignores, and the test above turns on those being two questions.
    private static Tree Corpus() => new(
        new HashSet<string>(StringComparer.Ordinal)
        {
            "adrs.md",
            "index.md",
            "adrs/0001-a.md",
            "adrs/0002-b.md",
            "pictures/cat.png"
        },
        rel => rel switch
        {
            "adrs.md" => "# ADRs\n",
            "index.md" => "# Index\n",
            "adrs/0001-a.md" => "# A\n",
            "adrs/0002-b.md" => "# B\n\n## Context\n",
            _ => ""
        });
}
