using kac.core;

// In-process unit tests for where work about a corpus is filed.
//
// The derivation and the identity are both rules over strings, so they are proven here rather than
// through a corpus. The golden fixtures prove that what these rules answer reaches the manifest, and the
// export tests prove which block each answer lands in.

namespace kac.tests;

public class TrackerTests
{
    private const string GitHubBase = "https://github.com/example/corpus";
    private const string RepoBase = "https://dev.azure.com/acme/Standards/_git/corpus";
    private const string WikiBase = "https://dev.azure.com/acme/Standards/_wiki/wikis/KaC";
    private const string Project = "https://dev.azure.com/acme/Standards";

    private static CorpusDescriptor Descriptor(
        string? target = Publishing.GitHub, string? published = GitHubBase,
        string? trackerTarget = null, string? trackerBase = null, string? trackerArea = null) =>
        new()
        {
            PublishingTarget = target, Base = published,
            TrackerTarget = trackerTarget, TrackerBase = trackerBase, TrackerArea = trackerArea
        };

    // The case the derivation exists for: one repository, one issue list, and nothing for the corpus to
    // configure.
    [Fact]
    public void A_github_corpus_files_where_it_publishes()
    {
        var tracker = Tracker.Own(Descriptor());

        Assert.Equal(Publishing.GitHub, tracker.Target);
        Assert.Equal(GitHubBase, tracker.Base);
    }

    // One Azure DevOps project holds the backlog, the repositories and the wikis, so both bases below
    // reach the same place and neither of them is it.
    [Theory]
    [InlineData(Publishing.AzureDevOps, RepoBase)]
    [InlineData(Publishing.AzureDevOpsWiki, WikiBase)]
    public void An_azure_devops_corpus_files_on_the_project_holding_what_it_publishes(
        string target, string published)
    {
        var tracker = Tracker.Own(Descriptor(target, published));

        Assert.Equal(Publishing.AzureDevOps, tracker.Target);
        Assert.Equal(Project, tracker.Base);
    }

    // A documentation site and a corpus publishing nowhere both have a backlog somewhere, and nothing
    // about where the records are read says where it is.
    [Theory]
    [InlineData(Publishing.MkDocs, "https://example.github.io/corpus")]
    [InlineData(Publishing.None, null)]
    public void A_corpus_publishing_where_no_backlog_lives_states_no_tracker(string target, string? published)
    {
        Assert.Equal(Tracker.None, Tracker.Own(Descriptor(target, published)));
    }

    [Fact]
    public void A_stated_tracker_is_where_the_corpus_files_rather_than_where_it_publishes()
    {
        var tracker = Tracker.Own(Descriptor(trackerTarget: Publishing.AzureDevOps, trackerBase: Project));

        Assert.Equal(Publishing.AzureDevOps, tracker.Target);
        Assert.Equal(Project, tracker.Base);
    }

    // Each half falls back on its own, so a corpus moving its backlog inside the platform it publishes on
    // states the address and leaves the client to follow from where it publishes.
    [Fact]
    public void A_stated_base_takes_the_target_from_the_publishing_block()
    {
        var tracker = Tracker.Own(Descriptor(trackerBase: "https://github.com/example/tickets"));

        Assert.Equal(Publishing.GitHub, tracker.Target);
        Assert.Equal("https://github.com/example/tickets", tracker.Base);
    }

    // The framework's tracker is the third address a caller holds, and the corpus's own publishing block
    // says nothing about who maintains what it took.
    [Fact]
    public void The_framework_tracker_is_stated_and_never_derived()
    {
        var descriptor = Descriptor();
        descriptor.FrameworkTarget = Publishing.GitHub;
        descriptor.FrameworkBase = "https://github.com/example/framework";

        Assert.Equal("github:github.com/example/framework", Tracker.Framework(descriptor).Id);
        Assert.Equal(Tracker.None, Tracker.Framework(Descriptor()));
    }

    // A corpus that states `none` has said it has no backlog, and the block below it says where its
    // records are read. Carrying that across would state an address the corpus just declined.
    [Fact]
    public void A_corpus_stating_a_target_of_none_states_no_base_either()
    {
        Assert.Equal(Tracker.None, Tracker.Own(Descriptor(trackerTarget: Publishing.None)));
    }

    // A client the publishing block does not imply is a backlog on another platform, and only the corpus
    // knows the address there. Pairing the two would build an `az boards` identity out of a GitHub URL.
    [Fact]
    public void A_stated_target_the_publishing_block_disagrees_with_takes_no_base_from_it()
    {
        var tracker = Tracker.Own(Descriptor(trackerTarget: Publishing.AzureDevOps));

        Assert.Equal(Publishing.AzureDevOps, tracker.Target);
        Assert.Null(tracker.Base);
        Assert.Null(tracker.Id);
    }

    // A base with no client addresses nothing, so it is not written as though it did. The target stays
    // as stated, which is what makes a value nobody spelled right visible in the manifest.
    [Theory]
    [InlineData(Publishing.MkDocs)]
    [InlineData(Publishing.AzureDevOpsWiki)]
    [InlineData("GitHub")]
    public void A_target_that_files_nowhere_states_no_base(string target)
    {
        var tracker = Tracker.For(target, GitHubBase, null);

        Assert.Equal(target, tracker.Target);
        Assert.Null(tracker.Base);
        Assert.Null(tracker.Id);
    }

    // The base a caller files against, whoever supplied it. A stated repository is cut back the way a
    // derived one is, so two corpora on one project state one address.
    [Fact]
    public void A_stated_azure_devops_base_is_cut_back_to_the_project()
    {
        var tracker = Tracker.Own(
            Descriptor(trackerTarget: Publishing.AzureDevOps, trackerBase: RepoBase));

        Assert.Equal(Project, tracker.Base);
    }

    // What a caller compares. Two corpora on one repository are one backlog however each of them spelled
    // the URL, because every part this drops is a part that never told two backlogs apart.
    [Theory]
    [InlineData("https://github.com/Example/Corpus")]
    [InlineData("https://github.com/example/corpus.git")]
    [InlineData("https://www.github.com/example/corpus/")]
    [InlineData("github.com/example/corpus")]
    public void Two_spellings_of_one_repository_carry_one_identity(string published)
    {
        Assert.Equal(Tracker.Own(Descriptor()).Id, Tracker.Own(Descriptor(published: published)).Id);
    }

    // The case the identity is built for. A corpus published on Azure Repos, an import published in the
    // wiki of the same project, and a framework whose tracker was written as the repository all file on
    // one backlog, and a caller told to open three tickets opens two it did not need.
    [Fact]
    public void One_backlog_reached_three_ways_carries_one_identity()
    {
        Assert.Equal("azure-devops:dev.azure.com/acme/standards",
            Tracker.Own(Descriptor(Publishing.AzureDevOps, RepoBase)).Id);

        Assert.Equal(Tracker.Own(Descriptor(Publishing.AzureDevOps, RepoBase)).Id,
            Tracker.Own(Descriptor(Publishing.AzureDevOpsWiki, WikiBase)).Id);

        Assert.Equal(Tracker.Own(Descriptor(Publishing.AzureDevOps, RepoBase)).Id,
            Tracker.For(Publishing.AzureDevOps, RepoBase, null).Id);
    }

    // Two repositories under one organisation are two issue lists, which is the comparison that has to
    // keep saying no.
    [Fact]
    public void Two_repositories_under_one_owner_are_two_trackers()
    {
        Assert.NotEqual(Tracker.Own(Descriptor()).Id,
            Tracker.Own(Descriptor(published: "https://github.com/example/other")).Id);
    }

    // A target that files nowhere and a base nobody supplied both leave a caller with nothing to compare,
    // and an identity built from either would compare equal to the next corpus that stated as little.
    [Theory]
    [InlineData(Publishing.MkDocs, GitHubBase)]
    [InlineData(Publishing.GitHub, null)]
    public void A_pair_reaching_no_backlog_carries_no_identity(string target, string? published)
    {
        Assert.Null(Tracker.For(target, published, null).Id);
    }

    // The case `area` exists for: one Azure DevOps project, several corpora, and a stated area telling
    // this corpus's work from the rest of the backlog's.
    [Fact]
    public void An_azure_devops_corpus_files_under_the_area_it_states()
    {
        var tracker = Tracker.Own(Descriptor(Publishing.AzureDevOps, RepoBase, trackerArea: "kac-it-swdev"));

        Assert.Equal(Project, tracker.Base);
        Assert.Equal("kac-it-swdev", tracker.Area);
    }

    // A nested area is one value with a separator in it. The descriptor writes `/` the way every base
    // here does, and the client that files converts it.
    [Fact]
    public void A_nested_area_travels_as_one_value()
    {
        Assert.Equal("kac-it-swdev/subteam",
            Tracker.For(Publishing.AzureDevOps, RepoBase, "kac-it-swdev/subteam").Area);
    }

    // The separators a hand-written value picks up, dropped before the manifest carries it. A client
    // joining the project to this would otherwise build a path with an empty segment in it.
    [Theory]
    [InlineData("  kac-it-swdev  ")]
    [InlineData("/kac-it-swdev")]
    [InlineData("kac-it-swdev/")]
    public void An_area_is_written_without_its_padding(string stated)
    {
        Assert.Equal("kac-it-swdev", Tracker.For(Publishing.AzureDevOps, RepoBase, stated).Area);
    }

    // A target with no area paths states no area, whatever the descriptor wrote. `validate` reports it
    // under `descriptor-area`, and an area beside a client that cannot set one would read as an address.
    [Theory]
    [InlineData(Publishing.GitHub)]
    [InlineData(Publishing.MkDocs)]
    [InlineData(Publishing.None)]
    public void A_target_with_no_area_paths_states_no_area(string target)
    {
        Assert.Null(Tracker.For(target, GitHubBase, "kac-it-swdev").Area);
    }

    // An area stated with nothing to file against is dropped with the base it would have qualified.
    [Fact]
    public void An_area_with_no_backlog_beside_it_is_dropped()
    {
        Assert.Null(Tracker.For(Publishing.AzureDevOps, null, "kac-it-swdev").Area);
    }

    // Nothing derives an area. A publishing block says where the corpus is read, and two corpora in one
    // project publish from two repositories and file under two areas neither block mentions.
    [Fact]
    public void A_derived_tracker_states_no_area()
    {
        Assert.Null(Tracker.Own(Descriptor(Publishing.AzureDevOps, RepoBase)).Area);
    }

    // One project is one backlog however many areas divide it, so the area is left out of the identity.
    // A caller comparing two blocks is asking which tracker to file on, not where inside it to land.
    [Fact]
    public void Two_areas_in_one_project_are_one_tracker()
    {
        Assert.Equal(
            Tracker.For(Publishing.AzureDevOps, RepoBase, "kac-it-swdev").Id,
            Tracker.For(Publishing.AzureDevOps, RepoBase, "kac-framework").Id);
    }
}
