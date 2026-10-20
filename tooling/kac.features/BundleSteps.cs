using System.Diagnostics;
using System.Text.Json.Nodes;
using kac.core;
using Reqnroll;
using Reqnroll.UnitTestProvider;
using Xunit;

namespace kac.features;

[Binding]
public sealed class BundleSteps(Workspace ws, IUnitTestRuntimeProvider runtime)
{
    private const string PluginManifest = ".plugin/.claude-plugin/plugin.json";

    private const string Breadcrumb = ".dist/plugin/hooks/breadcrumb.txt";

    private readonly List<string> _hookRuns = [];

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

    [Given(@"the descriptor publishes to github at ""(.*)""")]
    public void GivenTheDescriptorPublishesToGithubAt(string url)
    {
        GivenTheDescriptorStatesNo("publishing-target");
        ws.Write(".corpus.yaml",
            ws.Read(".corpus.yaml") + $"\npublishing-target: github\npublishing:\n  base: {url}\n");
    }

    /// <summary>Copies of <c>glossary/estate.md</c>, each under its own id and title.</summary>
    [Given(@"the glossary holds (\d+) records")]
    public void GivenTheGlossaryHoldsRecords(int count)
    {
        var held = Directory.GetFiles(ws.PathOf("glossary"), "*.md")
            .Count(f => !f.EndsWith("_template.md", StringComparison.Ordinal));
        var estate = ws.Read("glossary/estate.md");
        for (var n = held + 1; n <= count; n++)
            ws.Write($"glossary/extra-{n}.md", estate
                .Replace("gls-estate", $"gls-extra-{n}", StringComparison.Ordinal)
                .Replace("# Estate", $"# Extra {n}", StringComparison.Ordinal));
    }

    /// <summary>Over the fixture's stub, so the scenario runs the script every corpus here ships.</summary>
    [Given("the plugin tree has the template's build-index hook")]
    public void GivenThePluginTreeHasTheTemplatesBuildIndexHook() => File.Copy(
        Path.Combine(Harness.RepoRoot, "template", ".plugin", "hooks", "build-index"),
        ws.PathOf(".plugin/hooks/build-index"), overwrite: true);

    [Given("the corpus keeps the bundle fixture's plugin tree")]
    public void GivenTheCorpusKeepsTheBundleFixturesPluginTree()
    {
        var from = Path.Combine(Harness.RepoRoot, "tooling", "tests", "fixtures", "bundle", "plugin");
        foreach (var file in Directory.EnumerateFiles(from, "*", SearchOption.AllDirectories))
        {
            var to = ws.PathOf($".plugin/{Path.GetRelativePath(from, file).Replace('\\', '/')}");
            Directory.CreateDirectory(Path.GetDirectoryName(to) ?? ws.Root);
            File.Copy(file, to);
        }
    }

    /// <summary>
    /// Moves every file of <c>.plugin/</c> to <paramref name="from"/>, and copies the manifest, so a scenario can tell
    /// which of the two the bundle read.
    /// </summary>
    [Given(@"the plugin tree is shared from ""(.*)""")]
    public void GivenThePluginTreeIsSharedFrom(string from)
    {
        foreach (var path in ws.Snapshot(".plugin").Keys)
        {
            var to = ws.PathOf($"{from}/{path}");
            Directory.CreateDirectory(Path.GetDirectoryName(to) ?? ws.Root);
            File.Copy(ws.PathOf($".plugin/{path}"), to);
            if (path != Bundler.ManifestFile) File.Delete(ws.PathOf($".plugin/{path}"));
        }

        GivenTheDescriptorReadsThePluginTreeFrom(from);
    }

    [Given(@"the corpus keeps its own ""(.*)""")]
    public void GivenTheCorpusKeepsItsOwn(string path) => ws.Write($".plugin/{path}", "This corpus's own copy.\n");

    [Given(@"the component ""(.*)"" requires ""(.*)""")]
    public void GivenTheComponentRequires(string path, string requires)
    {
        var manifest = ws.ReadJson(PluginManifest);
        var component = Components(manifest).OfType<JsonObject>().Single(c => JsonRead.Str(c["path"]) == path);
        component["requires"] = new JsonArray(requires);
        ws.WriteJson(PluginManifest, manifest);
    }

    [Given(@"the plugin tree has a file ""(.*)""")]
    public void GivenThePluginTreeHasAFile(string path) => ws.Write($".plugin/{path}", "A file no component claims.\n");

    [Given(@"the plugin tree has a component ""(.*)"" reading no type")]
    public void GivenThePluginTreeHasAComponentReadingNoType(string path) => AddComponent(path, standalone: false);

    [Given(@"the plugin tree has a component ""(.*)"" reading ""(.*)""")]
    public void GivenThePluginTreeHasAComponentReading(string path, string type)
        => AddComponent(path, standalone: false, type);

    /// <summary>Writes no file, because <c>bundle</c> generates a view and the plugin tree contains none.</summary>
    [Given(@"the plugin tree declares the view ""(.*)"" as ""(.*)"" reading ""(.*)""")]
    public void GivenThePluginTreeDeclaresTheView(string path, string view, string type)
    {
        var manifest = ws.ReadJson(PluginManifest);
        Components(manifest).Add(new JsonObject
        {
            ["path"] = path,
            ["view"] = view,
            ["requires"] = new JsonArray(type)
        });
        ws.WriteJson(PluginManifest, manifest);
    }

    [Given(@"the plugin tree has a standalone component ""(.*)""")]
    public void GivenThePluginTreeHasAStandaloneComponent(string path) => AddComponent(path, standalone: true);

    [When("I bundle")]
    public void WhenIBundle() => ws.Run(Commands.Bundle);

    /// <summary>
    /// Ignored on Windows, which puts no <c>sh</c> on the path. The <c>round-trip</c> job in <c>kac.yml</c> runs the
    /// hook there under Git Bash.
    /// </summary>
    [When(@"the build-index hook runs twice in session ""(.*)""")]
    public void WhenTheBuildIndexHookRunsTwiceInSession(string session)
    {
        if (OperatingSystem.IsWindows())
        {
            runtime.TestIgnore("Windows puts no sh on the path.");
            return;
        }

        var input = $"{{\"session_id\":\"{session}\",\"hook_event_name\":\"PreToolUse\",\"tool_name\":\"Bash\"}}\n";
        _hookRuns.Add(RunHook(input));
        _hookRuns.Add(RunHook(input));
    }

    [Then("the first run printed the build index")]
    public void ThenTheFirstRunPrintedTheBuildIndex()
        => Assert.Equal(ws.Read($"{Dist.Plugin}/{BuildIndexHook.RenderedFile}"), _hookRuns[0]);

    [Then("the second run printed nothing")]
    public void ThenTheSecondRunPrintedNothing() => Assert.Equal("", _hookRuns[1]);

    [Then(@"the bundle includes ""(.*)""")]
    public void ThenTheBundleIncludes(string path) => Assert.Contains(path, Paths("included"));

    [Then(@"the bundle trims ""(.*)"" because ""(.*)""")]
    public void ThenTheBundleTrimsBecause(string path, string reason)
    {
        var trimmed = Entries("trimmed").Single(c => JsonRead.Str(c["path"]) == path);
        Assert.Equal(reason, JsonRead.Str(trimmed["reason"]));
    }

    [Then(@"the plugin manifest lists the components ""(.*)""")]
    public void ThenThePluginManifestListsTheComponents(string paths)
    {
        var listed = Components(ws.ReadJson($"{Dist.Plugin}/{Bundler.ManifestFile}"))
            .OfType<JsonObject>().Select(c => JsonRead.Str(c["path"]));
        Assert.Equal(paths, string.Join(", ", listed));
    }

    [Then(@"the breadcrumb has the line ""(.*)""")]
    public void ThenTheBreadcrumbHasTheLine(string line) => Assert.Contains(line, ws.Read(Breadcrumb).Split('\n'));

    [Then(@"the breadcrumb does not mention ""(.*)""")]
    public void ThenTheBreadcrumbDoesNotMention(string text) => Assert.DoesNotContain(text, ws.Read(Breadcrumb));

    /// <summary>Ignored on Windows, which has no permission bit for a bundle to keep.</summary>
    [Then(@"""(.*)"" is executable")]
    public void ThenIsExecutable(string path)
    {
        if (OperatingSystem.IsWindows())
        {
            runtime.TestIgnore("Windows has no permission bit.");
            return;
        }

        Assert.True((File.GetUnixFileMode(ws.PathOf(path)) & UnixFileMode.UserExecute) != 0,
            $"{path} is not executable");
    }

    /// <summary>With a data directory inside the workspace, so the marker dies with the scenario.</summary>
    private string RunHook(string input)
    {
        var start = new ProcessStartInfo("sh")
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            Environment = { ["CLAUDE_PLUGIN_DATA"] = ws.PathOf(".hook-data") }
        };
        start.ArgumentList.Add(ws.PathOf($"{Dist.Plugin}/hooks/build-index"));

        using var hook = Process.Start(start) ?? throw new InvalidOperationException("sh did not start");
        hook.StandardInput.Write(input);
        hook.StandardInput.Close();
        var printed = hook.StandardOutput.ReadToEnd();
        hook.WaitForExit();
        Assert.Equal(0, hook.ExitCode);
        return printed;
    }

    private IEnumerable<string?> Paths(string list) => Entries(list).Select(c => JsonRead.Str(c["path"]));

    private IEnumerable<JsonObject> Entries(string list) =>
        (ws.ReadJson(".dist/plugin/bundle.json")[list] as JsonArray ?? []).OfType<JsonObject>();

    private void AddComponent(string path, bool standalone, params string[] requires)
    {
        ws.Write($".plugin/{path}/SKILL.md", "A skill.\n");
        var manifest = ws.ReadJson(PluginManifest);
        var component = new JsonObject
        {
            ["path"] = path,
            ["requires"] = new JsonArray([.. requires.Select(r => (JsonNode?)JsonValue.Create(r))])
        };
        if (standalone) component["standalone"] = true;
        Components(manifest).Add(component);
        ws.WriteJson(PluginManifest, manifest);
    }

    private static JsonArray Components(JsonObject manifest) =>
        manifest["metadata"]?["components"] as JsonArray
        ?? throw new InvalidOperationException("the plugin manifest declares no components");
}
