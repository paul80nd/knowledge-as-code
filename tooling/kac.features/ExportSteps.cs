using System.Text.Json.Nodes;
using kac.core;
using Reqnroll;
using Xunit;

namespace kac.features;

[Binding]
public sealed class ExportSteps(Workspace ws)
{
    private static readonly DateTime Now = new(2026, 6, 15, 9, 0, 0, DateTimeKind.Utc);

    private const string Commit = "0123456789abcdef0123456789abcdef01234567";

    [Given(@"the restored ""(.*)"" export states ""(.*)"" at shape version (\d+)")]
    public void GivenTheRestoredExportStatesAtShapeVersion(string shortcode, string type, int version)
    {
        var path = $".imports/{shortcode}/manifest.json";
        var manifest = ws.ReadJson(path);
        var entry = Types(manifest).OfType<JsonObject>().Single(t => JsonRead.Str(t["type"]) == type);
        entry["shapeVersion"] = version;
        ws.WriteJson(path, manifest);
    }

    [Given(@"the restored ""(.*)"" export states ""(.*)"" section ""(.*)"" at ""(.*)""")]
    public void GivenTheRestoredExportStatesSectionAt(string shortcode, string type, string section, string fidelity)
    {
        var path = $".imports/{shortcode}/manifest.json";
        var manifest = ws.ReadJson(path);
        var entry = Types(manifest).OfType<JsonObject>().Single(t => JsonRead.Str(t["type"]) == type);
        var sections = entry["sections"] as JsonObject
                       ?? throw new InvalidOperationException($"'{type}' states no sections");
        Assert.True(sections.ContainsKey(section), $"'{type}' exports no section '{section}'");
        sections[section] = fidelity;
        ws.WriteJson(path, manifest);
    }

    /// <summary>The time and the commit are fixed, so two runs over one corpus write the same bytes.</summary>
    internal static int Export(string root, string? type = null, bool? dirty = false) =>
        Commands.Export(root, type, Now, Commit, dirty);

    [When("I export")]
    public void WhenIExport() => ws.Run(root => Export(root));

    [When(@"I export the ""(.*)"" type")]
    public void WhenIExportTheType(string type) => ws.Run(root => Export(root, type));

    [When("^I export from a tree git reports as (dirty|clean|unknown)$")]
    public void WhenIExportFromATreeGitReportsAs(string tree)
    {
        bool? dirty = tree switch { "dirty" => true, "clean" => false, _ => null };
        ws.Run(root => Export(root, dirty: dirty));
    }

    [Then(@"the export lists the types ""(.*)""")]
    public void ThenTheExportListsTheTypes(string types)
    {
        var listed = Types(ws.ReadJson(".dist/export/manifest.json")).Select(t => JsonRead.Str(t?["type"]));
        Assert.Equal(types, string.Join(", ", listed));
    }

    private static JsonArray Types(JsonObject manifest) =>
        manifest["types"] as JsonArray ?? throw new InvalidOperationException("the manifest lists no types");
}
