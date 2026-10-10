using kac.core;

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

    [Fact]
    public void A_github_corpus_files_where_it_publishes()
    {
        var tracker = Tracker.Own(Descriptor());

        Assert.Equal(Publishing.GitHub, tracker.Target);
        Assert.Equal(GitHubBase, tracker.Base);
    }

    /// <summary>
    /// One Azure DevOps project holds the backlog, the repositories and the wikis, so both bases below reach the same
    /// place and neither of them is it.
    /// </summary>
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

    [Fact]
    public void A_stated_base_takes_the_target_from_the_publishing_block()
    {
        var tracker = Tracker.Own(Descriptor(trackerBase: "https://github.com/example/tickets"));

        Assert.Equal(Publishing.GitHub, tracker.Target);
        Assert.Equal("https://github.com/example/tickets", tracker.Base);
    }

    [Fact]
    public void The_framework_tracker_is_stated_and_never_derived()
    {
        var descriptor = Descriptor();
        descriptor.FrameworkTarget = Publishing.GitHub;
        descriptor.FrameworkBase = "https://github.com/example/framework";

        Assert.Equal("github:github.com/example/framework", Tracker.Framework(descriptor).Id);
        Assert.Equal(Tracker.None, Tracker.Framework(Descriptor()));
    }

    [Fact]
    public void A_corpus_stating_a_target_of_none_states_no_base_either()
    {
        Assert.Equal(Tracker.None, Tracker.Own(Descriptor(trackerTarget: Publishing.None)));
    }

    [Fact]
    public void A_stated_target_the_publishing_block_disagrees_with_takes_no_base_from_it()
    {
        var tracker = Tracker.Own(Descriptor(trackerTarget: Publishing.AzureDevOps));

        Assert.Equal(Publishing.AzureDevOps, tracker.Target);
        Assert.Null(tracker.Base);
        Assert.Null(tracker.Id);
    }

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

    [Fact]
    public void A_stated_azure_devops_base_is_cut_back_to_the_project()
    {
        var tracker = Tracker.Own(
            Descriptor(trackerTarget: Publishing.AzureDevOps, trackerBase: RepoBase));

        Assert.Equal(Project, tracker.Base);
    }

    [Theory]
    [InlineData("https://github.com/Example/Corpus")]
    [InlineData("https://github.com/example/corpus.git")]
    [InlineData("https://www.github.com/example/corpus/")]
    [InlineData("github.com/example/corpus")]
    public void Two_spellings_of_one_repository_carry_one_identity(string published)
    {
        Assert.Equal(Tracker.Own(Descriptor()).Id, Tracker.Own(Descriptor(published: published)).Id);
    }

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

    [Fact]
    public void Two_repositories_under_one_owner_are_two_trackers()
    {
        Assert.NotEqual(Tracker.Own(Descriptor()).Id,
            Tracker.Own(Descriptor(published: "https://github.com/example/other")).Id);
    }

    [Theory]
    [InlineData(Publishing.MkDocs, GitHubBase)]
    [InlineData(Publishing.GitHub, null)]
    public void A_pair_reaching_no_backlog_carries_no_identity(string target, string? published)
    {
        Assert.Null(Tracker.For(target, published, null).Id);
    }

    [Fact]
    public void An_azure_devops_corpus_files_under_the_area_it_states()
    {
        var tracker = Tracker.Own(Descriptor(Publishing.AzureDevOps, RepoBase, trackerArea: "kac-it-swdev"));

        Assert.Equal(Project, tracker.Base);
        Assert.Equal("kac-it-swdev", tracker.Area);
    }

    [Fact]
    public void A_nested_area_travels_as_one_value()
    {
        Assert.Equal("kac-it-swdev/subteam",
            Tracker.For(Publishing.AzureDevOps, RepoBase, "kac-it-swdev/subteam").Area);
    }

    [Theory]
    [InlineData("  kac-it-swdev  ")]
    [InlineData("/kac-it-swdev")]
    [InlineData("kac-it-swdev/")]
    public void An_area_is_written_without_its_padding(string stated)
    {
        Assert.Equal("kac-it-swdev", Tracker.For(Publishing.AzureDevOps, RepoBase, stated).Area);
    }

    [Theory]
    [InlineData(Publishing.GitHub)]
    [InlineData(Publishing.MkDocs)]
    [InlineData(Publishing.None)]
    public void A_target_with_no_area_paths_states_no_area(string target)
    {
        Assert.Null(Tracker.For(target, GitHubBase, "kac-it-swdev").Area);
    }

    [Fact]
    public void An_area_with_no_backlog_beside_it_is_dropped()
    {
        Assert.Null(Tracker.For(Publishing.AzureDevOps, null, "kac-it-swdev").Area);
    }

    [Fact]
    public void A_derived_tracker_states_no_area()
    {
        Assert.Null(Tracker.Own(Descriptor(Publishing.AzureDevOps, RepoBase)).Area);
    }

    [Fact]
    public void Two_areas_in_one_project_are_one_tracker()
    {
        Assert.Equal(
            Tracker.For(Publishing.AzureDevOps, RepoBase, "kac-it-swdev").Id,
            Tracker.For(Publishing.AzureDevOps, RepoBase, "kac-framework").Id);
    }
}
