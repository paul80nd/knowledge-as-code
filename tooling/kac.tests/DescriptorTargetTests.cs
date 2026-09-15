using kac.core;

// In-process unit tests for `descriptor-target`: every target `.corpus.yaml` names, held to the values
// the tool acts on.
//
// The `descriptor-targets` fixture covers a corpus that gets one wrong through the CLI, and covers the
// id once, which is all the coverage gate asks. Every other spelling lives here, where a case is one
// string.

namespace kac.tests;

public class DescriptorTargetTests
{
    [Fact]
    public void A_descriptor_naming_no_target_at_all_is_not_asked_anything()
        => Assert.Empty(Targets(new CorpusDescriptor()));

    [Theory]
    [InlineData(Publishing.AzureDevOps)]
    [InlineData(Publishing.AzureDevOpsWiki)]
    [InlineData(Publishing.GitHub)]
    [InlineData(Publishing.MkDocs)]
    [InlineData(Publishing.None)]
    public void Every_target_a_corpus_can_publish_to_is_silent(string target)
        => Assert.Empty(Targets(new CorpusDescriptor { PublishingTarget = target }));

    [Theory]
    [InlineData("gihtub")]
    [InlineData("GitHub")]
    [InlineData("gitlab")]
    public void A_publishing_target_kac_cannot_address_names_what_it_takes(string target)
    {
        var message = Assert.Single(Targets(new CorpusDescriptor { PublishingTarget = target })).Message;

        Assert.Contains($"publishing-target '{target}' is not a target kac can publish to.", message);
        Assert.Contains("azure-devops, azure-devops-wiki, github, mkdocs, none.", message);
    }

    [Theory]
    [InlineData(Publishing.AzureDevOps)]
    [InlineData(Publishing.GitHub)]
    [InlineData(Publishing.None)]
    public void Every_target_a_corpus_can_file_on_is_silent(string target)
        => Assert.Empty(Targets(new CorpusDescriptor { TrackerTarget = target, FrameworkTarget = target }));

    // A wiki and a documentation site publish records and hold no backlog, so a tracker naming either
    // addresses nothing. The corpus is told rather than left with a block that files nowhere.
    [Theory]
    [InlineData(Publishing.AzureDevOpsWiki)]
    [InlineData(Publishing.MkDocs)]
    [InlineData("GitHub")]
    public void A_tracker_target_that_files_nowhere_names_what_it_takes(string target)
    {
        var message = Assert.Single(Targets(new CorpusDescriptor { TrackerTarget = target })).Message;

        Assert.Contains($"tracker.target '{target}' is not a target kac can file on.", message);
        Assert.Contains("azure-devops, github, none.", message);
    }

    // The same vocabulary, and the key is named in the message, so an author fixing one block is not
    // sent to the other.
    [Fact]
    public void A_framework_target_is_held_to_the_same_values_and_named_apart()
    {
        var message = Assert.Single(Targets(new CorpusDescriptor { FrameworkTarget = Publishing.MkDocs })).Message;

        Assert.Contains("framework.target 'mkdocs'", message);
    }

    // Each key is asked on its own, so a corpus that got two wrong fixes both from one run.
    [Fact]
    public void Two_keys_spelled_wrong_are_reported_together()
    {
        var findings = Targets(new CorpusDescriptor
        {
            PublishingTarget = "gitlab", TrackerTarget = "jira"
        });

        Assert.Equal(2, findings.Count);
        Assert.Contains(findings, f => f.Message.Contains("publishing-target"));
        Assert.Contains(findings, f => f.Message.Contains("tracker.target"));
    }

    // Nothing here is stood up: the pass reads the descriptor and never the listing.
    private static List<Finding> Targets(CorpusDescriptor descriptor)
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
                .Where(f => f.Check.Value == "descriptor-target")
        ];
    }
}
