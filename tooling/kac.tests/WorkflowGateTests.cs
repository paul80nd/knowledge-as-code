// The branch rule on `main` names one check, and `validate` is it. A job that rule does not reach runs,
// reports, and blocks nothing, which shows nowhere in the workflow file and nowhere on the pull request.
// `.github/workflows/kac.yml` carries why the gate is shaped this way.

using YamlDotNet.RepresentationModel;

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
        var declared = jobs.Children.Keys.Select(k => ((YamlScalarNode)k).Value!).ToList();

        var needs = (YamlSequenceNode)((YamlMappingNode)jobs.Children[new YamlScalarNode(Gate)])
            .Children[new YamlScalarNode("needs")];

        var gated = needs.Children.Select(n => ((YamlScalarNode)n).Value!).Append(Gate);

        Assert.Equal(declared.Order(StringComparer.Ordinal), gated.Order(StringComparer.Ordinal));
    }
}
