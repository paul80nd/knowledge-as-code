using kac.core;

namespace kac.tests;

public class TreeTests
{
    private static Tree Corpus() => new(
        new HashSet<string>(StringComparer.Ordinal)
        {
            "README.md",
            "adrs.md",
            "adrs/0001-a.md",
            "adrs/_template.md",
            "knowledge-as-code/style.md",
            "knowledge-as-code/taxonomy.md"
        },
        rel => $"the text of {rel}");

    [Fact]
    public void The_listing_is_what_the_corpus_holds()
    {
        Assert.True(Corpus().Exists("adrs/0001-a.md"));
        Assert.False(Corpus().Exists("adrs/0099-gone.md"));
    }

    [Fact]
    public void A_file_the_corpus_does_not_hold_may_still_be_on_disk()
    {
        var tree = new Tree(
            new HashSet<string>(["adrs/0001-a.md"], StringComparer.Ordinal),
            rel => rel,
            rel => rel is "adrs/0001-a.md" or "adrs/_template.md");

        Assert.False(tree.Exists("adrs/_template.md"));
        Assert.True(tree.OnDisk("adrs/_template.md"));
    }

    [Fact]
    public void A_corpus_built_from_values_answers_on_disk_from_its_listing()
    {
        var tree = new Tree(new HashSet<string>(["adrs.md"], StringComparer.Ordinal), rel => rel);

        Assert.True(tree.OnDisk("adrs.md"));
        Assert.False(tree.OnDisk("policies.md"));
    }

    [Fact]
    public void A_folder_is_held_where_the_corpus_holds_something_inside_it()
    {
        Assert.True(Corpus().HasFolder("adrs"));
        Assert.False(Corpus().HasFolder("policies"));
    }

    [Fact]
    public void A_glob_names_the_files_directly_inside_a_folder()
        => Assert.Equal(["knowledge-as-code/style.md", "knowledge-as-code/taxonomy.md"],
            Corpus().Match("knowledge-as-code/*.md"));

    [Fact]
    public void A_glob_spanning_segments_names_the_markdown_at_any_depth()
        => Assert.Equal(
            [
                "README.md", "adrs.md", "adrs/0001-a.md", "adrs/_template.md",
                "knowledge-as-code/style.md", "knowledge-as-code/taxonomy.md"
            ],
            Corpus().Match("**/*.md"));

    [Fact]
    public void The_matches_come_back_in_ordinal_order()
        => Assert.Equal(["adrs/0001-a.md", "adrs/_template.md"], Corpus().Match("adrs/*.md"));

    [Fact]
    public void A_glob_matching_nothing_names_nothing()
        => Assert.Empty(Corpus().Match("policies/*.md"));

    [Fact]
    public void A_windows_separator_names_the_same_file()
    {
        Assert.True(Corpus().Exists(@"adrs\0001-a.md"));
        Assert.Equal("the text of adrs/0001-a.md", Corpus().Read(@"adrs\0001-a.md"));
    }
}
