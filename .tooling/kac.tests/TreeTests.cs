using kac.core;

// In-process unit tests for the listing every pass reads the corpus through.
//
// Three of the four questions it answers are about presence, and the differences between them are what
// the rest of the tool leans on: the listing is what git tracks, `OnDisk` is what is there, and a glob
// names a set. No fixture can show them apart, because a fixture corpus is a directory in which every
// file is both tracked and present.

namespace kac.tests;

public class TreeTests
{
    // What a corpus holds. Presence is left to the listing here; the two cases where it is not are built
    // their own corpus below, because the difference between them is the whole of what they assert.
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

    // -- presence --

    [Fact]
    public void The_listing_is_what_the_corpus_holds()
    {
        Assert.True(Corpus().Exists("adrs/0001-a.md"));
        Assert.False(Corpus().Exists("adrs/0099-gone.md"));
    }

    // The two questions come apart in the one direction that matters: a file git does not track is in no
    // clone, so it is present without being held.
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

    // A corpus assembled from values has no disk to ask, so the listing answers for both. That is what
    // makes a test corpus a set of paths rather than a set of paths and a second set beside it.
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

    // -- naming a set --

    // `*` stops at a slash, so a pattern names the markdown directly inside a folder. This is the
    // framework's own documentation, which is read as a set and reported against file by file.
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

    // Ordinal, so that a pass walking a set reports in one order however the listing arrived.
    [Fact]
    public void The_matches_come_back_in_ordinal_order()
        => Assert.Equal(["adrs/0001-a.md", "adrs/_template.md"], Corpus().Match("adrs/*.md"));

    [Fact]
    public void A_glob_matching_nothing_names_nothing()
        => Assert.Empty(Corpus().Match("policies/*.md"));

    // -- reading --

    // A path is normalised before it is looked up or read, so a caller that built one with the platform's
    // separator asks the same question as one that wrote it with a slash.
    [Fact]
    public void A_windows_separator_names_the_same_file()
    {
        Assert.True(Corpus().Exists(@"adrs\0001-a.md"));
        Assert.Equal("the text of adrs/0001-a.md", Corpus().Read(@"adrs\0001-a.md"));
    }
}
