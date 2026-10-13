using kac.core;
using Reqnroll;
using Xunit;
using YamlDotNet.RepresentationModel;

namespace kac.features;

[Binding]
public sealed class ReportSteps(Workspace ws)
{
    private const string ToolVersion = "9.9.9";

    private const string At = "2026-06-15T09:00:00Z";

    /// <summary>
    /// The path is passed rooted, because <c>--out</c> resolves against the process's working directory and a spec
    /// shares that with every other scenario.
    /// </summary>
    [When(@"I write the ""(.*)"" report to ""(.*)""")]
    public void WhenIWriteTheReportTo(string name, string path)
        => ws.Run(root => Commands.Report(root, name, ToolVersion, At, ws.PathOf(path)));

    [When(@"I ask for the ""(.*)"" report")]
    public void WhenIAskForTheReport(string name)
        => ws.Run(root => Commands.Report(root, name, ToolVersion, At, outPath: null));

    [Then(@"the report at ""(.*)"" states ""(.*)"" as ""(.*)""")]
    public void ThenTheReportStatesAs(string path, string key, string value)
        => Assert.Equal(value, Yaml.Str(Field(path, key)));

    /// <summary>A key written with nothing after it, as a run leaves the ones a person fills in.</summary>
    [Then(@"the report at ""(.*)"" leaves ""(.*)"" empty")]
    public void ThenTheReportLeavesEmpty(string path, string key)
    {
        var node = Field(path, key);
        Assert.True(node is YamlScalarNode { Value: null or "" }, $"{path} fills '{key}'");
    }

    [Then(@"the report at ""(.*)"" names these sources:")]
    public void ThenTheReportNamesTheseSources(string path, Table table)
    {
        var sources = Field(path, "sources") as YamlSequenceNode
                      ?? throw new InvalidOperationException($"{path} lists no sources");
        var actual = sources.Select(s => (Yaml.Str(Yaml.Get(s, "resource")), Yaml.Str(Yaml.Get(s, "version"))));
        var expected = table.Rows.Select(r => ((string?)r["resource"], (string?)r["version"]));
        Assert.Equal(expected, actual);
    }

    [Then(@"the reports at ""(.*)"" and ""(.*)"" are the same")]
    public void ThenTheReportsAreTheSame(string first, string second) => Assert.Equal(ws.Read(first), ws.Read(second));

    [Then(@"validating the corpus reports the report at ""(.*)"" missing ""(.*)""")]
    public void ThenValidatingReportsTheReportMissing(string path, string field)
        => Assert.Contains(Harness.Validate(ws.Root).Findings, f =>
            f.File == path && f.Check.Value == "required-field" && f.Message.Contains($"'{field}'"));

    /// <summary><paramref name="key"/> is a dotted path through the frontmatter's mappings.</summary>
    private YamlNode? Field(string path, string key)
    {
        var text = ws.Read(path);
        var end = text.IndexOf("\n---\n", StringComparison.Ordinal);
        Assert.True(text.StartsWith("---\n", StringComparison.Ordinal) && end > 0, $"{path} opens on no frontmatter");

        YamlNode? node = Yaml.Load(text[4..end]);
        foreach (var step in key.Split('.')) node = Yaml.Get(node, step);
        return node;
    }
}
