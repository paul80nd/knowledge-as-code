using kac.core;
using Xunit.Sdk;

// In-process unit tests for how a publishing target addresses what it publishes.
//
// The link forms cannot be shown by a golden fixture: the harness assembles a corpus that is not a git
// repository, so no run there resolves a ref and no run there writes a link. That is the right shape
// for the fixture: it proves the no-links path, and leaves the rules themselves to be proven here.

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

    // The publishing a descriptor resolves to. Every descriptor above names a target and a base, so a
    // null is this file's own arrangement being wrong rather than the case a test is making.
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

    // The caller's question is whether it can write a link, and a null says no without asking it to tell
    // the cases apart.
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

    // Azure DevOps shows a page's numeric id in the address bar, and nothing derives the id of a second
    // page from the id of the first. A base copied from there addresses one record and misaddresses
    // every other, which is worse than addressing none.
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

    // A wiki addresses a page rather than a file, so the extension goes and the separators are encoded.
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

    // A wiki takes its anchor as a query parameter, because `?pagePath=` opened the query string first.
    // A fragment there is discarded and the reader lands at the top of the page.
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

    // A citation says what the agent read. A wiki is the one target that cannot keep this, because no
    // `?pagePath=` URL takes a commit.
    [Theory]
    [InlineData(Publishing.GitHub, GitHubBase)]
    [InlineData(Publishing.AzureDevOps, RepoBase)]
    public void A_link_resolves_against_the_ref_and_not_against_a_branch(string target, string published)
    {
        var link = For(Descriptor(target, published)).Link("glossary/search.md");

        Assert.Contains(Sha, link, StringComparison.Ordinal);
        Assert.DoesNotContain("/main/", link, StringComparison.Ordinal);
    }

    // A corpus should not have to know that the mechanism is about to append a slash.
    [Fact]
    public void A_trailing_slash_on_a_base_does_not_double()
    {
        var link = For(Descriptor(published: GitHubBase + "/")).Link("glossary/search.md");

        Assert.Equal($"{GitHubBase}/blob/{Sha}/glossary/search.md", link);
    }

    // The prefix lands between the commit and the record, which is the one place it can go.
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

    // Whether the descriptor wrote the prefix with slashes says nothing about where the corpus sits.
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

    // The link carries no empty segment where a prefix would sit, and the manifest states no prefix for
    // an agent to join.
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

    // The anchor rule is the target's, and GitHub's is the discarding form the corpus's own links
    // already use. That is what lets one citation name a term and a link land on it.
    [Fact]
    public void The_anchor_is_the_form_the_corpus_already_links_with()
    {
        Assert.Equal("identity-line", Publishing.Anchor("Identity line"));
        Assert.Equal(Md.Slug("Identity line"), Publishing.Anchor("Identity line"));
    }

    // The commit is settled too: a ref a reader has to copy is forty characters that can be mistyped
    // into a plausible 404. The wrong one answers as confidently as the right one.
    [Theory]
    [InlineData(Publishing.GitHub, GitHubBase, GitHubBase + "/blob/" + Sha + "/{path}#{anchor}")]
    [InlineData(Publishing.AzureDevOps, RepoBase, RepoBase + "?path=/{path}&version=GC" + Sha + "#{anchor}")]
    [InlineData(Publishing.AzureDevOpsWiki, WikiBase, WikiBase + "?pagePath=%2F{path}&anchor={anchor}")]
    public void A_template_leaves_the_path_and_the_anchor_and_settles_the_rest(
        string target, string published, string expected)
    {
        Assert.Equal(expected, For(Descriptor(target, published)).Template());
    }

    // A reader holding a record's path supplies the path alone, and cannot join it to the prefix in the
    // wrong order.
    [Fact]
    public void A_template_settles_the_subdirectory_too()
    {
        var template = For(Descriptor(prefix: "example")).Template();

        Assert.Equal($"{GitHubBase}/blob/{Sha}/example/{{path}}#{{anchor}}", template);
    }

    // One rule builds both, so a corpus reading the export cannot be handed two addresses for one part.
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
