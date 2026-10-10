using kac.core;

namespace kac.tests;

public class PageCheckTests
{
    [Fact]
    public void A_page_of_prose_and_links_is_silent()
        => Assert.Empty(Page("# ADRs\n\nThe [first](/adrs/0001-a.md).\n"));

    [Fact]
    public void Frontmatter_on_a_page_is_reported_and_names_where_it_belongs()
    {
        var finding = Assert.Single(Page("---\nid: adr-0001\n---\n\n# ADRs\n"));

        Assert.Equal("page-frontmatter", finding.Check.Value);
        Assert.Contains("Move what it holds into 'adrs/'", finding.Message);
    }

    [Fact]
    public void A_dead_link_on_a_page_is_reported()
        => Assert.Equal("link-resolves",
            Assert.Single(Page("# ADRs\n\nThe [missing](/adrs/0099-gone.md).\n")).Check.Value);

    [Fact]
    public void A_page_the_corpus_does_not_hold_is_left_to_type_setup()
        => Assert.Empty(Page("---\nid: adr-0001\n---\n\n# ADRs\n\n[gone](/adrs/0099-gone.md)\n", tracked: false));

    private static readonly TypeSchema Adrs = new()
    {
        Key = "adrs",
        TypeName = "adrs",
        Folder = "adrs",
        Page = "adrs.md"
    };

    private static List<Finding> Page(string page, bool tracked = true)
    {
        const string rel = "adrs.md";
        var held = tracked ? new[] { rel, "adrs/0001-a.md" } : ["adrs/0001-a.md"];

        var tree = new Tree(
            new HashSet<string>(held, StringComparer.Ordinal),
            path => path == rel ? page : "# A\n",
            path => held.Contains(path, StringComparer.Ordinal) || path == rel);

        var schema = new Schema
        {
            ByFolder = new Dictionary<string, TypeSchema>(StringComparer.Ordinal) { ["adrs"] = Adrs }
        };

        return
        [
            .. Validator.CheckAll(Corpus.Load(tree, schema, new CorpusDescriptor()), Required.Today)
                .Where(f => f is { File: rel, Check.Value: "page-frontmatter" or "link-resolves" })
        ];
    }
}
