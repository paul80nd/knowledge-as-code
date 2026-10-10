using kac.core;
using Xunit.Sdk;

namespace kac.tests;

public class PublishingTests
{
    private const string GitHubBase = "https://github.com/example/corpus";
    private const string WikiBase = "https://dev.azure.com/acme/Standards/_wiki/wikis/KaC";
    private const string RepoBase = "https://dev.azure.com/acme/Standards/_git/corpus";
    private const string Sha = "0123456789abcdef0123456789abcdef01234567";

    private static CorpusDescriptor Descriptor(
        string? target = Publishing.GitHub, string? published = GitHubBase, string? prefix = null) =>
        new() { PublishingTarget = target, Base = published, PathPrefix = prefix };

    /// <summary>
    /// The publishing a descriptor resolves to. Every descriptor above names a target and a base, so a null is this
    /// file's own arrangement being wrong rather than the case a test is making.
    /// </summary>
    private static Publishing For(CorpusDescriptor descriptor) =>
        Publishing.For(descriptor, Sha)
        ?? throw new XunitException($"'{descriptor.PublishingTarget}' resolved to no publishing.");

    [Fact]
    public void A_github_corpus_with_a_base_and_a_ref_is_addressable()
    {
        var publishing = Publishing.For(Descriptor(), Sha);

        Assert.NotNull(publishing);
        Assert.Equal(Publishing.GitHub, publishing.Target);
        Assert.Equal(Sha, publishing.Ref);
    }

    [Theory]
    [InlineData(Publishing.AzureDevOps, RepoBase)]
    [InlineData(Publishing.AzureDevOpsWiki, WikiBase)]
    public void Both_azure_devops_targets_are_addressable(string target, string published)
    {
        Assert.NotNull(Publishing.For(Descriptor(target, published), Sha));
    }

    [Theory]
    [InlineData(Publishing.None, GitHubBase, Sha)]    // publishes nowhere
    [InlineData(Publishing.MkDocs, GitHubBase, Sha)]  // a target nothing addresses yet
    [InlineData(Publishing.GitHub, null, Sha)]        // a target, and no base to build on
    [InlineData(Publishing.GitHub, GitHubBase, null)] // no ref, so no stable address
    public void A_corpus_the_tool_cannot_address_resolves_to_nothing(
        string? target, string? published, string? gitRef)
    {
        Assert.Null(Publishing.For(Descriptor(target, published), gitRef));
    }

    [Fact]
    public void A_descriptor_stating_no_target_resolves_to_nothing()
    {
        Assert.Null(Publishing.For(Descriptor(target: null), Sha));
    }

    /// <summary>
    /// Azure DevOps shows a page's numeric id in the address bar, and nothing derives the id of a second page from the
    /// id of the first. A base copied from there addresses one record and misaddresses every other, which is worse than
    /// addressing none.
    /// </summary>
    [Theory]
    [InlineData(WikiBase + "/880/Knowledge-as-code")]
    [InlineData(WikiBase + "/880")]
    [InlineData("https://dev.azure.com/acme/Standards/_git/corpus")]
    public void A_wiki_base_naming_a_page_rather_than_the_wiki_resolves_to_nothing(string published)
    {
        Assert.Null(Publishing.For(Descriptor(Publishing.AzureDevOpsWiki, published), Sha));
    }

    [Fact]
    public void A_github_record_is_read_at_a_blob_url_under_the_commit()
    {
        var link = For(Descriptor()).Link("glossary/search.md");

        Assert.Equal($"{GitHubBase}/blob/{Sha}/glossary/search.md", link);
    }

    /// <summary>
    /// A wiki addresses a page rather than a file, so the extension goes and the separators are encoded.
    /// </summary>
    [Fact]
    public void A_wiki_record_is_read_at_an_encoded_page_path()
    {
        var link = For(Descriptor(Publishing.AzureDevOpsWiki, WikiBase))
            .Link("glossary/search.md");

        Assert.Equal($"{WikiBase}?pagePath=%2Fglossary%2Fsearch", link);
    }

    [Fact]
    public void An_azure_repos_record_is_read_at_a_path_pinned_to_the_commit()
    {
        var link = For(Descriptor(Publishing.AzureDevOps, RepoBase))
            .Link("glossary/search.md");

        Assert.Equal($"{RepoBase}?path=/glossary/search.md&version=GC{Sha}", link);
    }

    /// <summary>
    /// A wiki takes its anchor as a query parameter, because <c>?pagePath=</c> opened the query string first. A
    /// fragment there is discarded and the reader lands at the top of the page.
    /// </summary>
    [Theory]
    [InlineData(Publishing.GitHub, GitHubBase, "#query")]
    [InlineData(Publishing.AzureDevOps, RepoBase, "#query")]
    [InlineData(Publishing.AzureDevOpsWiki, WikiBase, "&anchor=query")]
    public void A_cited_part_anchors_the_link_the_way_its_target_spells_an_anchor(
        string target, string published, string expected)
    {
        var link = For(Descriptor(target, published)).Link("glossary/search.md", "query");

        Assert.EndsWith(expected, link, StringComparison.Ordinal);
    }

    /// <summary>
    /// A citation says what the agent read. A wiki is the one target that cannot keep this, because no
    /// <c>?pagePath=</c> URL takes a commit.
    /// </summary>
    [Theory]
    [InlineData(Publishing.GitHub, GitHubBase)]
    [InlineData(Publishing.AzureDevOps, RepoBase)]
    public void A_link_resolves_against_the_ref_and_not_against_a_branch(string target, string published)
    {
        var link = For(Descriptor(target, published)).Link("glossary/search.md");

        Assert.Contains(Sha, link, StringComparison.Ordinal);
        Assert.DoesNotContain("/main/", link, StringComparison.Ordinal);
    }

    [Fact]
    public void A_trailing_slash_on_a_base_does_not_double()
    {
        var link = For(Descriptor(published: GitHubBase + "/")).Link("glossary/search.md");

        Assert.Equal($"{GitHubBase}/blob/{Sha}/glossary/search.md", link);
    }

    [Theory]
    [InlineData(Publishing.GitHub, GitHubBase,
        GitHubBase + "/blob/" + Sha + "/example/glossary/search.md#query")]
    [InlineData(Publishing.AzureDevOps, RepoBase,
        RepoBase + "?path=/example/glossary/search.md&version=GC" + Sha + "#query")]
    [InlineData(Publishing.AzureDevOpsWiki, WikiBase,
        WikiBase + "?pagePath=%2Fexample%2Fglossary%2Fsearch&anchor=query")]
    public void A_corpus_in_a_subdirectory_is_addressed_under_it(
        string target, string published, string expected)
    {
        var link = For(Descriptor(target, published, "example"))
            .Link("glossary/search.md", "query");

        Assert.Equal(expected, link);
    }

    [Theory]
    [InlineData("example")]
    [InlineData("/example")]
    [InlineData("example/")]
    public void A_prefix_written_with_slashes_addresses_the_same_folder(string prefix)
    {
        var publishing = For(Descriptor(prefix: prefix));

        Assert.Equal($"{GitHubBase}/blob/{Sha}/example/glossary/search.md",
            publishing.Link("glossary/search.md"));
        Assert.Equal("example", publishing.PathPrefix);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("/")]
    public void A_corpus_that_is_the_repository_is_addressed_at_its_root(string? prefix)
    {
        var publishing = For(Descriptor(prefix: prefix));

        Assert.Equal($"{GitHubBase}/blob/{Sha}/glossary/search.md", publishing.Link("glossary/search.md"));
        Assert.Null(publishing.PathPrefix);
    }

    [Fact]
    public void The_anchor_is_the_form_the_corpus_already_links_with()
    {
        Assert.Equal("identity-line", Publishing.Anchor("Identity line"));
        Assert.Equal(Md.Slug("Identity line"), Publishing.Anchor("Identity line"));
    }

    [Theory]
    [InlineData(Publishing.GitHub, GitHubBase, GitHubBase + "/blob/" + Sha + "/{path}#{anchor}")]
    [InlineData(Publishing.AzureDevOps, RepoBase, RepoBase + "?path=/{path}&version=GC" + Sha + "#{anchor}")]
    [InlineData(Publishing.AzureDevOpsWiki, WikiBase, WikiBase + "?pagePath=%2F{path}&anchor={anchor}")]
    public void A_template_leaves_the_path_and_the_anchor_and_settles_the_rest(
        string target, string published, string expected)
    {
        Assert.Equal(expected, For(Descriptor(target, published)).Template());
    }

    [Fact]
    public void A_template_settles_the_subdirectory_too()
    {
        var template = For(Descriptor(prefix: "example")).Template();

        Assert.Equal($"{GitHubBase}/blob/{Sha}/example/{{path}}#{{anchor}}", template);
    }

    [Theory]
    [InlineData(Publishing.GitHub, GitHubBase, "query")]
    [InlineData(Publishing.GitHub, GitHubBase, null)]
    [InlineData(Publishing.AzureDevOps, RepoBase, "query")]
    [InlineData(Publishing.AzureDevOps, RepoBase, null)]
    [InlineData(Publishing.AzureDevOpsWiki, WikiBase, "query")]
    [InlineData(Publishing.AzureDevOpsWiki, WikiBase, null)]
    public void A_substituted_template_is_the_link_this_class_resolves(
        string target, string published, string? anchor)
    {
        var publishing = For(Descriptor(target, published));
        var wiki = target == Publishing.AzureDevOpsWiki;
        var mark = wiki ? "&anchor=" : "#";
        var path = wiki ? "glossary%2Fsearch" : "glossary/search.md";

        var human = publishing.Template().Replace(Publishing.PathToken, path, StringComparison.Ordinal);
        human = anchor is null
            ? human.Replace($"{mark}{Publishing.AnchorToken}", "", StringComparison.Ordinal)
            : human.Replace(Publishing.AnchorToken, anchor, StringComparison.Ordinal);

        Assert.Equal(publishing.Link("glossary/search.md", anchor), human);
    }
}
