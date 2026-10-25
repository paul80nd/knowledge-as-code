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

    [Fact]
    public void The_gate_runs_after_a_job_it_needs_has_failed()
    {
        var gate = (YamlMappingNode)Jobs().Children[new YamlScalarNode(Gate)];

        Assert.True(gate.Children.TryGetValue(new YamlScalarNode("if"), out var condition),
            $"'{Gate}' states no `if:`, so a failed job skips it and GitHub counts the skipped check as passing.");
        Assert.Equal("always()", Text(condition));
    }

    [Fact]
    public void The_gate_reads_the_result_of_every_job_it_needs()
    {
        var gate = (YamlMappingNode)Jobs().Children[new YamlScalarNode(Gate)];
        var steps = (YamlSequenceNode)gate.Children[new YamlScalarNode("steps")];

        Assert.Contains(steps.Children, step => step.ToString().Contains("needs.*.result", StringComparison.Ordinal));
    }

    private static string Text(YamlNode node) =>
        (node as YamlScalarNode)?.Value
        ?? throw new XunitException($"a node in kac.yml holds no text: {node}");
}
