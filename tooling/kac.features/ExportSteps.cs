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

    [Given(@"""(.*)"" is left from an earlier run")]
    public void GivenIsLeftFromAnEarlierRun(string path) => ws.Write(path, "{}\n");

    [When("I export")]
    public void WhenIExport() => ws.Run(root => Commands.Export(root, null, Now, Commit, dirty: false));

    [When(@"I export the ""(.*)"" type")]
    public void WhenIExportTheType(string type) => ws.Run(root => Commands.Export(root, type, Now, Commit, false));

    [When("^I export from a tree git reports as (dirty|clean|unknown)$")]
    public void WhenIExportFromATreeGitReportsAs(string tree)
    {
        bool? dirty = tree switch { "dirty" => true, "clean" => false, _ => null };
        ws.Run(root => Commands.Export(root, null, Now, Commit, dirty));
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
