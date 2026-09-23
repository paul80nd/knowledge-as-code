using kac.core;

// In-process unit tests for `descriptor-area`: every area path `.corpus.yaml` states, held to a target
// that has area paths.
//
// The `descriptor-area` fixture covers the id once through the CLI, which is all the coverage gate asks.
// Every other combination lives here, where a case is one string. `TrackerTests` holds what the exporter
// then writes.

namespace kac.tests;

public class DescriptorAreaTests
{
    private const string Project = "https://dev.azure.com/acme/Standards";
    private const string Repo = "https://github.com/acme/standards";

    [Fact]
    public void A_descriptor_stating_no_area_is_not_asked_anything()
        => Assert.Empty(Areas(new CorpusDescriptor()));

    // The case the key exists for: one Azure DevOps project, and an area telling this corpus's work from
    // the rest of the backlog's.
    [Fact]
    public void An_area_beside_an_azure_devops_tracker_is_silent()
        => Assert.Empty(Areas(new CorpusDescriptor
        {
            TrackerTarget = Publishing.AzureDevOps, TrackerBase = Project, TrackerArea = "kac-it-swdev"
        }));

    // A corpus publishing to Azure Repos states its area and no target, because the target follows from
    // `publishing:`. Reading the stated key alone would fail the ordinary case, so the check asks the
    // effective target.
    [Fact]
    public void An_area_beside_a_derived_azure_devops_tracker_is_silent()
        => Assert.Empty(Areas(new CorpusDescriptor
        {
            PublishingTarget = Publishing.AzureDevOps,
            Base = $"{Project}/_git/corpus",
            TrackerArea = "kac-it-swdev"
        }));

    // GitHub divides one repository's issue list with labels, so it has nothing an area path names.
    [Fact]
    public void An_area_beside_a_github_tracker_names_the_target_it_files_on()
    {
        var message = Assert.Single(Areas(new CorpusDescriptor
        {
            PublishingTarget = Publishing.GitHub, Base = Repo, TrackerArea = "kac-it-swdev"
        })).Message;

        Assert.Contains("tracker.area 'kac-it-swdev' names an area path", message);
        Assert.Contains("tracker files on 'github', which has no area paths.", message);
        Assert.Contains("only azure-devops does.", message);
    }

    // A tracker that files nowhere has nowhere to put an area either.
    [Fact]
    public void An_area_beside_a_tracker_that_files_nowhere_is_reported()
    {
        var message = Assert.Single(Areas(new CorpusDescriptor
        {
            TrackerTarget = Publishing.None, TrackerArea = "kac-it-swdev"
        })).Message;

        Assert.Contains("tracker files on 'none'", message);
    }

    // The same question, and the key is named in the message, so an author fixing one block is not sent
    // to the other. `framework:` is never derived, so its stated target is the effective one.
    [Fact]
    public void A_framework_area_is_asked_the_same_question_and_named_apart()
    {
        var message = Assert.Single(Areas(new CorpusDescriptor
        {
            FrameworkTarget = Publishing.GitHub, FrameworkBase = Repo, FrameworkArea = "kac-framework"
        })).Message;

        Assert.Contains("framework.area 'kac-framework'", message);
        Assert.Contains("framework files on 'github'", message);
    }

    // Each block is asked on its own, so a corpus that got both wrong fixes both from one run.
    [Fact]
    public void Both_blocks_are_reported_together()
    {
        var findings = Areas(new CorpusDescriptor
        {
            PublishingTarget = Publishing.GitHub, Base = Repo, TrackerArea = "kac-it-swdev",
            FrameworkTarget = Publishing.GitHub, FrameworkBase = Repo, FrameworkArea = "kac-framework"
        });

        Assert.Equal(2, findings.Count);
        Assert.Contains(findings, f => f.Message.Contains("tracker.area"));
        Assert.Contains(findings, f => f.Message.Contains("framework.area"));
    }

    // A key holding only spaces is the same absence as no key at all, so it is not reported as a value.
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void An_area_nobody_filled_in_is_not_reported(string area)
        => Assert.Empty(Areas(new CorpusDescriptor
        {
            PublishingTarget = Publishing.GitHub, Base = Repo, TrackerArea = area
        }));

    // Nothing here is stood up: the pass reads the descriptor and never the listing.
    private static List<Finding> Areas(CorpusDescriptor descriptor)
    {
        var schema = new Schema
        {
            ByFolder = new Dictionary<string, TypeSchema>(StringComparer.Ordinal)
            {
                ["adrs"] = new() { Key = "adrs", TypeName = "adrs", Folder = "adrs", IdPrefix = "adr" }
            }
        };

        var tree = new Tree(new HashSet<string>(StringComparer.Ordinal), _ => "", _ => false);

        return
        [
            .. Validator.CheckAll(Corpus.Load(tree, schema, descriptor), Required.Today)
                .Where(f => f.Check.Value == "descriptor-area")
        ];
    }
}
