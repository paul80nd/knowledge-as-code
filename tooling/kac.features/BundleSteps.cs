using System.Text.Json.Nodes;
using kac.core;
using Reqnroll;
using Xunit;

namespace kac.features;

[Binding]
public sealed class BundleSteps(Workspace ws)
{
    private const string PluginManifest = ".plugin/.claude-plugin/plugin.json";

    /// <summary>Outside <see cref="Workspace.Run"/>, so the export is part of what a scenario starts from.</summary>
    [Given("it has been exported")]
    public void GivenItHasBeenExported() => Assert.Equal(0, ExportSteps.Export(ws.Root));

    [Given(@"the descriptor reads the plugin tree from ""(.*)""")]
    public void GivenTheDescriptorReadsThePluginTreeFrom(string from)
        => ws.Write(".corpus.yaml", ws.Read(".corpus.yaml") + $"\nplugin:\n  from: {from}\n");

    [Given(@"the descriptor states no ""(.*)""")]
    public void GivenTheDescriptorStatesNo(string key)
    {
        var lines = ws.Read(".corpus.yaml").Split('\n');
        var kept = lines.Where(l => !l.StartsWith(key + ":", StringComparison.Ordinal)).ToList();
        Assert.NotEqual(lines.Length, kept.Count);
        ws.Write(".corpus.yaml", string.Join('\n', kept));
    }

    [Given(@"the component ""(.*)"" requires ""(.*)""")]
    public void GivenTheComponentRequires(string path, string requires)
    {
        var manifest = ws.ReadJson(PluginManifest);
        var component = Components(manifest).OfType<JsonObject>().Single(c => JsonRead.Str(c["path"]) == path);
        component["requires"] = new JsonArray(requires);
        ws.WriteJson(PluginManifest, manifest);
    }

    [When("I bundle")]
    public void WhenIBundle() => ws.Run(Commands.Bundle);

    [Then(@"the bundle includes ""(.*)""")]
    public void ThenTheBundleIncludes(string path) => Assert.Contains(path, Paths("included"));

    [Then(@"the bundle trims ""(.*)""")]
    public void ThenTheBundleTrims(string path) => Assert.Contains(path, Paths("trimmed"));

    private IEnumerable<string?> Paths(string list) =>
        (ws.ReadJson(".dist/plugin/bundle.json")[list] as JsonArray ?? [])
        .Select(c => JsonRead.Str(c?["path"]));

    private static JsonArray Components(JsonObject manifest) =>
        manifest["metadata"]?["components"] as JsonArray
        ?? throw new InvalidOperationException("the plugin manifest declares no components");
}
