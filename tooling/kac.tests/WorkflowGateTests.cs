// The branch rule on `main` names one check, and `validate` is it. A job that rule does not reach runs,
// reports, and blocks nothing, which shows nowhere in the workflow file and nowhere on the pull request.
// `.github/workflows/kac.yml` carries why the gate is shaped this way.

using YamlDotNet.RepresentationModel;
using Xunit.Sdk;

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public class WorkflowGateTests
{
    private const string Gate = "validate";

    private static YamlMappingNode Jobs()
    {
        var stream = new YamlStream();
        using var reader = new StreamReader(Path.Combine(Repo.Root, ".github", "workflows", "kac.yml"));
        stream.Load(reader);

        var root = (YamlMappingNode)stream.Documents[0].RootNode;
        return (YamlMappingNode)root.Children[new YamlScalarNode("jobs")];
    }

    [Fact]
    public void Every_job_is_behind_the_one_name_the_branch_rule_requires()
    {
        var jobs = Jobs();
        var declared = jobs.Children.Keys.Select(Text).ToList();

        var needs = (YamlSequenceNode)((YamlMappingNode)jobs.Children[new YamlScalarNode(Gate)])
            .Children[new YamlScalarNode("needs")];

        var gated = needs.Children.Select(Text).Append(Gate);

        Assert.Equal(declared.Order(StringComparer.Ordinal), gated.Order(StringComparer.Ordinal));
    }
    // The text of a scalar the workflow writes. A node holding none is an empty key or an empty item,
    // and this file reads neither.
    private static string Text(YamlNode node) =>
        (node as YamlScalarNode)?.Value
        ?? throw new XunitException($"a node in kac.yml holds no text: {node}");

}
