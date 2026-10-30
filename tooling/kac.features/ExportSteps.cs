using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
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

    [Given(@"the corpus states content-version ""(.*)""")]
    public void GivenTheCorpusStatesContentVersion(string version)
        => ws.Write(".corpus.yaml", ws.Read(".corpus.yaml") + $"content-version: \"{version}\"\n");

    [Given(@"the corpus excludes ""(.*)"" from its export")]
    public void GivenTheCorpusExcludesFromItsExport(string value)
        => ws.Write(".corpus.yaml", ws.Read(".corpus.yaml") + $"\nexport:\n  exclude: [ {value} ]\n");

    [Given(@"the corpus publishes to ""(.*)"" at ""(.*)""")]
    public void GivenTheCorpusPublishesToAt(string target, string published)
        => ws.Write(".corpus.yaml",
            ws.Read(".corpus.yaml") + $"\npublishing-target: {target}\npublishing:\n  base: {published}\n");

    [Given(@"the schema declares ""(.*)"" at export version (\d+)")]
    public void GivenTheSchemaDeclaresAtExportVersion(string type, int version)
    {
        var path = $".schema/{type}.yaml";
        var schema = ws.Read(path);
        var declared = Regex.Replace(schema, @"(?m)^(export:\n  version: )\d+$", $"${{1}}{version}");
        Assert.NotEqual(schema, declared);
        ws.Write(path, declared);
    }

    [Given(@"the review date of ""(.*)"" passed on ""(.*)""")]
    public void GivenTheReviewDateOfPassedOn(string path, string date)
    {
        var record = ws.Read(path);
        var lapsed = Regex.Replace(record, @"(?m)^review-by: "".*""$", $"review-by: \"{date}\"");
        Assert.NotEqual(record, lapsed);
        ws.Write(path, lapsed);
    }

    /// <summary>
    /// Restores the grandparent the "eng" export already carries a second time, consumed directly at
    /// <paramref name="version"/> and addressed as that export states it.
    /// </summary>
    [Given(@"the corpus also consumes ""(.*)"" directly, restored at ""(.*)""")]
    public void GivenTheCorpusAlsoConsumesDirectlyRestoredAt(string shortcode, string version)
    {
        var manifest = ws.ReadJson(".imports/eng/manifest.json");
        var source = (manifest["sources"] as JsonArray ?? []).OfType<JsonObject>()
            .Single(s => JsonRead.Str(s["shortcode"]) == shortcode);
        var corpus = JsonRead.Str(source["corpus"]);

        manifest["shortcode"] = shortcode;
        manifest["corpus"] = corpus;
        manifest["contentVersion"] = version;
        manifest["publishing"] = source["publishing"]?.DeepClone();
        manifest["sources"] = new JsonArray();
        manifest["types"] = new JsonArray();
        ws.WriteJson($".imports/{shortcode}/manifest.json", manifest);

        var descriptor = ws.Read(".corpus.yaml");
        Assert.Contains("consumes:\n", descriptor);
        ws.Write(".corpus.yaml", descriptor.Replace("consumes:\n",
            $"consumes:\n  - corpus: {corpus}\n    shortcode: {shortcode}\n    version: ^{version}\n"
            + $"    source: ../{shortcode}/.dist/package\n    resolved: \"{version}\"\n"));
    }

    /// <summary>The time and the commit are fixed, so two runs over one corpus write the same bytes.</summary>
    internal static int Export(string root, string? type = null, bool? dirty = false) =>
        Commands.Export(root, type, Now, Commit, _ => dirty);

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

    [Then("the manifest states the export format this build writes")]
    public void ThenTheManifestStatesTheExportFormatThisBuildWrites()
        => Assert.Equal(Exporter.FormatVersion, (int?)ws.ReadJson(".dist/export/manifest.json")["formatVersion"]);

    [Then(@"the export states ""(.*)"" at shape version (\d+)")]
    public void ThenTheExportStatesAtShapeVersion(string type, int version)
        => Assert.Equal(version, ShapeVersion(type));

    [Then(@"the export states ""(.*)"" at the shape version its schema declares")]
    public void ThenTheExportStatesAtTheShapeVersionItsSchemaDeclares(string type)
    {
        var declared = Corpus.Load(ws.Root).Adopted.Single(t => t.Key == type).Export?.Version;
        Assert.Equal(declared, ShapeVersion(type));
    }

    /// <summary><paramref name="value"/> is JSON, as in the steps every command shares.</summary>
    [Then(@"the ""(.*)"" line of ""(.*)"" states ""(.*)"" as (.*)")]
    public void ThenTheLineOfStatesAs(string id, string path, string key, string value)
    {
        var line = ws.Read(path).Split('\n').Where(l => l.Length > 0).Select(JsonRead.Parse)
            .Single(l => JsonRead.Str(l?["id"]) == id);
        var actual = line?[key];
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(value), actual),
            $"{id} states '{key}' as {actual?.ToJsonString() ?? "null"}");
    }

    private int? ShapeVersion(string type) =>
        (int?)Types(ws.ReadJson(".dist/export/manifest.json")).OfType<JsonObject>()
            .Single(t => JsonRead.Str(t["type"]) == type)["shapeVersion"];

    private static JsonArray Types(JsonObject manifest) =>
        manifest["types"] as JsonArray ?? throw new InvalidOperationException("the manifest lists no types");
}
