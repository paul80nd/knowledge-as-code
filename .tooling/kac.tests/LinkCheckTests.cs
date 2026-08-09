using kac.core;

// In-process unit tests for the decision the link pass turns on: whether a target resolves on disk. It
// is fiddly, it is quiet when wrong, and the goldens can only reach it through a whole corpus. Whether a
// bracketed label is an id is asked here too, but answered in IdChecks and tested beside it.

namespace kac.tests;

public class LinkCheckTests
{
    // -- does this target resolve? --

    [Fact]
    public void A_target_resolves_absolute_from_the_root_or_relative_to_the_document()
    {
        var root = TempCorpus();

        Assert.True(LinkChecks.ResolveTarget(root, "adrs/0001-a.md", "/adrs/0002-b.md"));
        Assert.True(LinkChecks.ResolveTarget(root, "adrs/0001-a.md", "0002-b.md"));
        Assert.True(LinkChecks.ResolveTarget(root, "adrs/0001-a.md", "../adrs.md"));
        Assert.False(LinkChecks.ResolveTarget(root, "adrs/0001-a.md", "/adrs/0099-gone.md"));
    }

    // Azure DevOps resolves a link with the extension left off, so the corpus is written that way and
    // the check has to follow.
    [Fact]
    public void The_md_extension_may_be_omitted()
        => Assert.True(LinkChecks.ResolveTarget(TempCorpus(), "adrs/0001-a.md", "/adrs/0002-b"));

    // A directory is deliberately not a target: `/adrs` is a link to the page `adrs.md`, and accepting
    // the folder as well would resolve a link to a type whose page has gone. Git cannot track an empty
    // directory either, so the same link would pass locally and fail in CI.
    [Fact]
    public void A_directory_is_not_a_target_but_the_page_beside_it_is()
    {
        var root = TempCorpus();
        Assert.True(LinkChecks.ResolveTarget(root, "index.md", "/adrs"));      // resolves as adrs.md
        Assert.False(LinkChecks.ResolveTarget(root, "index.md", "/pictures")); // a folder with no page
    }

    // A fragment or a query is addressing within a target, not a different one.
    [Theory]
    [InlineData("/adrs/0002-b.md#context")]
    [InlineData("/adrs/0002-b.md?raw=1")]
    [InlineData("#a-heading-in-this-document")]
    public void A_fragment_or_query_is_stripped_before_the_target_is_looked_up(string target)
        => Assert.True(LinkChecks.ResolveTarget(TempCorpus(), "adrs/0001-a.md", target));

    [Theory]
    [InlineData("https://example.com/a", true)]
    [InlineData("http://example.com/a", true)]
    [InlineData("mailto:someone@example.com", true)]
    [InlineData("/adrs/0002-b.md", false)]
    public void An_external_target_is_left_alone(string target, bool external)
        => Assert.Equal(external, LinkChecks.IsExternal(target));

    // A corpus on disk, because the resolver asks the filesystem and mocking that would only pin the
    // mock. Built once per class and left for the runner to clear.
    private static string TempCorpus()
    {
        var root = Path.Combine(Path.GetTempPath(), "kac-link-tests");
        Directory.CreateDirectory(Path.Combine(root, "adrs"));
        Directory.CreateDirectory(Path.Combine(root, "pictures"));
        File.WriteAllText(Path.Combine(root, "adrs.md"), "# ADRs\n");
        File.WriteAllText(Path.Combine(root, "index.md"), "# Index\n");
        File.WriteAllText(Path.Combine(root, "adrs", "0001-a.md"), "# A\n");
        File.WriteAllText(Path.Combine(root, "adrs", "0002-b.md"), "# B\n");
        return root;
    }
}
