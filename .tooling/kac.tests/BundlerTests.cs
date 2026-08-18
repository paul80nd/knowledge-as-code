using System.Text;
using System.Text.Json;
using kac.core;

// In-process unit tests for the bundle.
//
// The golden fixtures hold two plugins between them and pin what the CLI assembles from each. They
// cannot hold a third cheaply, and several rules need one: a manifest may be malformed in several
// ways, a component may name a path that is a file rather than a directory, and a corpus may write
// keys into `plugin.json` that this tool has never heard of. That is what these build.

namespace kac.tests;

public class BundlerTests
{
    // -- what stops a run --

    [Fact]
    public void A_plugin_tree_with_no_manifest_is_refused()
        => Assert.Contains(".claude-plugin/plugin.json",
            Assert.Single(Plan(plugin: [("README.md", "# nothing")], export: [Manifest()]).Problems));

    [Fact]
    public void A_manifest_that_is_not_JSON_is_refused()
        => Assert.Contains("not a JSON object",
            Assert.Single(Plan(plugin: [(Bundler.ManifestFile, "not json at all")], export: [Manifest()]).Problems));

    // The name is what a marketplace installs by and what a user types for it afterwards. Inventing one
    // here would produce a plugin that installs under a name nobody chose.
    [Fact]
    public void A_manifest_with_no_name_is_refused()
        => Assert.Contains("states no name",
            Assert.Single(Plan(
                plugin: [(Bundler.ManifestFile, """{"metadata":{"corpusRoot":"corpus"}}""")],
                export: [Manifest()]).Problems));

    // The export is copied under `corpusRoot`, so a plugin tree already holding that directory would
    // lose files to the copy or take files from it. Whichever won, the loser would be missing from an
    // artefact nobody reviews.
    [Fact]
    public void A_corpus_root_the_plugin_tree_already_uses_is_refused()
        => Assert.Contains("would overwrite the other",
            Assert.Single(Plan(
                plugin: [(Bundler.ManifestFile, Source()), ("corpus/notes.md", "mine")],
                export: [Manifest()]).Problems));

    // The skills address the export as `${CLAUDE_PLUGIN_ROOT}/<corpusRoot>/…` by name. A default here
    // would be the tool quietly disagreeing with the words the corpus wrote in its own skill, and the
    // disagreement would only show up when someone asked the installed plugin a question.
    [Fact]
    public void A_manifest_with_no_corpus_root_is_refused()
        => Assert.Contains("metadata.corpusRoot",
            Assert.Single(Plan(plugin: [(Bundler.ManifestFile, """{"name":"p"}""")],
                export: [Manifest()]).Problems));

    [Fact]
    public void An_export_with_no_manifest_is_refused()
        => Assert.Contains("Run the export first",
            Assert.Single(Plan(plugin: [(Bundler.ManifestFile, Source())], export: [("glossary/terms.jsonl", "{}")])
                .Problems));

    // A refused plan writes nothing. The alternative is a plugin assembled around a missing answer,
    // which installs and fails later somewhere less obvious.
    [Fact]
    public void A_refused_plan_names_no_files()
        => Assert.Empty(Plan(plugin: [("README.md", "# nothing")], export: [Manifest()]).Files);

    // -- trimming --

    [Fact]
    public void A_component_whose_types_the_export_carries_is_included()
    {
        var plan = Plan(plugin: [(Bundler.ManifestFile, Source(Component("skills/look", "glossary")))],
            export: [Manifest("glossary")]);

        Assert.Equal(["skills/look"], plan.Included.Select(c => c.Path));
        Assert.Empty(plan.Trimmed);
    }

    [Fact]
    public void A_component_whose_type_the_export_does_not_carry_is_trimmed_and_says_why()
    {
        var trimmed = Assert.Single(Plan(
            plugin: [(Bundler.ManifestFile, Source(Component("skills/decide", "adrs")))],
            export: [Manifest("glossary")]).Trimmed);

        Assert.Equal("skills/decide", trimmed.Path);
        Assert.Equal("the export carries no adrs", trimmed.Reason);
    }

    // A component requiring two types where the export carries one is trimmed, and the reason names the
    // half that was missing rather than both. The author's next move is to export the type it named.
    [Fact]
    public void A_component_needing_two_types_is_trimmed_on_the_one_that_is_missing()
        => Assert.Equal("the export carries no adrs",
            Assert.Single(Plan(
                plugin: [(Bundler.ManifestFile, Source(Component("skills/both", "glossary", "adrs")))],
                export: [Manifest("glossary")]).Trimmed).Reason);

    // A component naming no type reads nothing the corpus decides, so nothing decides whether it
    // travels. `bundle.json` still records it, because a reader asking what is in the plugin wants it
    // listed beside the ones that had to earn their place.
    [Fact]
    public void A_component_requiring_nothing_always_travels()
    {
        var plan = Plan(plugin: [(Bundler.ManifestFile, Source(Component("hooks/hooks.json")))],
            export: [Manifest()]);

        Assert.Equal(["hooks/hooks.json"], plan.Included.Select(c => c.Path));
    }

    // A component may be a directory of skills or a single file, such as a hook definition, so the
    // trim matches the path itself as well as anything beneath it.
    [Fact]
    public void Trimming_a_component_that_is_one_file_removes_that_file()
    {
        var plan = Plan(
            plugin:
            [
                (Bundler.ManifestFile, Source(Component("hooks/hooks.json", "adrs"))),
                ("hooks/hooks.json", "{}")
            ],
            export: [Manifest("glossary")]);

        Assert.DoesNotContain(plan.Files, f => f.Path.EndsWith("hooks/hooks.json", StringComparison.Ordinal));
    }

    // A path beginning with a component's name but not beneath it is a different component. `skills/a`
    // must not take `skills/ab` with it.
    [Fact]
    public void Trimming_a_component_leaves_a_sibling_whose_name_starts_the_same_way()
    {
        var plan = Plan(
            plugin:
            [
                (Bundler.ManifestFile, Source(Component("skills/a", "adrs"))),
                ("skills/ab/SKILL.md", "kept")
            ],
            export: [Manifest("glossary")]);

        Assert.Contains(plan.Files, f => f.Path.EndsWith("skills/ab/SKILL.md", StringComparison.Ordinal));
    }

    [Fact]
    public void A_plugin_left_with_no_component_warns_and_is_still_assembled()
    {
        var plan = Plan(plugin: [(Bundler.ManifestFile, Source(Component("skills/look", "glossary")))],
            export: [Manifest()]);

        Assert.Contains(plan.Warnings, w => w.Contains("every component was trimmed"));
        Assert.Contains(plan.Files, f => f.Path.EndsWith(Bundler.ManifestFile, StringComparison.Ordinal));
    }

    // A plugin declaring none is equally useless and equally worth saying so, in different words: there
    // is nothing to blame the corpus for.
    [Fact]
    public void A_plugin_declaring_no_component_warns_in_its_own_words()
        => Assert.Contains("declares no components",
            Assert.Single(Plan(plugin: [(Bundler.ManifestFile, Source())],
                export: [Versioned("2.3.4", "glossary")]).Warnings));

    // -- the manifest that travels --

    [Fact]
    public void The_version_is_the_corpus_content_version()
        => Assert.Equal("2.3.4", Written(Plan(
            plugin: [(Bundler.ManifestFile, Source())],
            export: [Versioned("2.3.4")]), Bundler.ManifestFile).GetProperty("version").GetString());

    // The export format version is the shape of the data and the content version is what the corpus
    // knows. Stamping the first onto the plugin would tell a reader which parser to use, over data
    // whose meaning had changed underneath it.
    [Fact]
    public void The_export_format_version_is_not_the_plugin_version()
        => Assert.NotEqual("2", Written(Plan(
            plugin: [(Bundler.ManifestFile, Source())],
            export: [Versioned("2.3.4")]), Bundler.ManifestFile).GetProperty("version").GetString());

    [Fact]
    public void A_corpus_with_no_content_version_keeps_the_version_the_manifest_stated_and_says_so()
    {
        var plan = Plan(plugin: [(Bundler.ManifestFile, Source())], export: [Manifest()]);

        Assert.Equal("0.0.1", Written(plan, Bundler.ManifestFile).GetProperty("version").GetString());
        Assert.Contains(plan.Warnings, w => w.Contains("no contentVersion"));
    }

    // The manifest is the corpus's own file. Reading it into a shape known here and writing that back
    // would delete whatever the corpus had added, without a word.
    [Fact]
    public void A_key_this_tool_does_not_know_survives_the_rewrite()
    {
        const string theirs = """
                              {
                                "name": "example-libraries",
                                "version": "0.0.1",
                                "somethingOfTheirOwn": "theirs",
                                "metadata": { "corpusRoot": "corpus" }
                              }
                              """;

        Assert.Equal("theirs",
            Written(Plan(plugin: [(Bundler.ManifestFile, theirs)], export: [Manifest()]), Bundler.ManifestFile)
                .GetProperty("somethingOfTheirOwn").GetString());
    }

    [Fact]
    public void A_trimmed_component_leaves_the_manifest_it_was_declared_in()
    {
        var components = Written(Plan(
                plugin: [(Bundler.ManifestFile, Source(Component("skills/decide", "adrs")))],
                export: [Manifest("glossary")]), Bundler.ManifestFile)
            .GetProperty("metadata").GetProperty("components");

        Assert.Equal(0, components.GetArrayLength());
    }

    // -- the export inside the plugin --

    [Fact]
    public void The_export_is_copied_under_the_corpus_root_the_manifest_names()
        => Assert.Contains(
            Plan(plugin: [(Bundler.ManifestFile, Source())], export: [Manifest(), ("glossary/terms.jsonl", "{}")]).Files,
            f => f.Path == "plugin/corpus/glossary/terms.jsonl");

    // The copy is the seam between the two commands. A bundle that edited what it copied would make a
    // difference between the two copies something to interpret rather than a defect.
    [Fact]
    public void The_export_is_copied_byte_for_byte()
    {
        const string line = """{"id":"gls-a.term","title":"Term"}""";
        var copied = Assert.Single(
            Plan(plugin: [(Bundler.ManifestFile, Source())], export: [Manifest(), ("glossary/terms.jsonl", line)]).Files,
            f => f.Path.EndsWith("terms.jsonl", StringComparison.Ordinal));

        Assert.Equal(line, Encoding.UTF8.GetString(copied.Content));
    }

    // -- the marketplace --

    // A marketplace resolves a plugin source against the directory holding `.claude-plugin/`, and
    // refuses one containing `..`. So the plugin has to sit beneath the marketplace, which is what
    // makes `.dist/` the marketplace rather than a directory beside the plugin.
    [Fact]
    public void The_marketplace_names_the_plugin_as_a_path_beneath_itself()
    {
        var source = Written(Plan(plugin: [(Bundler.ManifestFile, Source())], export: [Manifest()]),
                Dist.MarketplaceRel, underPlugin: false)
            .GetProperty("plugins")[0].GetProperty("source").GetString();

        Assert.Equal($"./{Dist.PluginDir}", source);
        Assert.DoesNotContain("..", source);
    }

    // -- what the bundle records about itself --

    [Fact]
    public void The_record_names_what_was_included_and_what_was_trimmed()
    {
        var record = Written(Plan(
            plugin:
            [
                (Bundler.ManifestFile, Source(Component("skills/look", "glossary"), Component("skills/decide", "adrs")))
            ],
            export: [Manifest("glossary")]), Bundler.RecordFile);

        Assert.Equal("skills/look", record.GetProperty("included")[0].GetProperty("path").GetString());
        Assert.Equal("skills/decide", record.GetProperty("trimmed")[0].GetProperty("path").GetString());
        Assert.Equal("the export carries no adrs", record.GetProperty("trimmed")[0].GetProperty("reason").GetString());
    }

    // The export inside the plugin states when it was taken and from which commit. A second clock here
    // would be a second answer to one question, and the two would disagree the first time one was
    // rebuilt without the other.
    [Fact]
    public void The_record_carries_no_clock()
    {
        var record = Written(Plan(plugin: [(Bundler.ManifestFile, Source())], export: [Manifest()]),
            Bundler.RecordFile);

        Assert.False(record.TryGetProperty("generatedAt", out _));
        Assert.False(record.TryGetProperty("commit", out _));
    }

    // -- helpers --

    private static BundlePlan Plan(
        (string Path, string Content)[] plugin,
        (string Path, string Content)[] export) =>
        Bundler.Plan(new BundleSource([.. plugin.Select(File)], [.. export.Select(File)]));

    private static BundleFile File((string Path, string Content) f) =>
        new(f.Path, Encoding.UTF8.GetBytes(f.Content));

    // A plugin manifest as a corpus writes one: a name, a version to be replaced, the corpus root the
    // skills address the export through, and whatever components the case needs.
    private static string Source(params string[] components) =>
        $$"""
          {
            "name": "example-libraries",
            "version": "0.0.1",
            "description": "A plugin.",
            "metadata": { "corpusRoot": "corpus", "components": [{{string.Join(",", components)}}] }
          }
          """;

    private static string Component(string path, params string[] requires) =>
        $$"""{"path":"{{path}}","requires":[{{string.Join(",", requires.Select(r => $"\"{r}\""))}}]}""";

    // An export manifest as `kac export` writes one, reduced to the two things a bundle reads from it.
    private static (string, string) Manifest(params string[] types) => Versioned(null, types);

    private static (string, string) Versioned(string? contentVersion, params string[] types) =>
        (Exporter.ManifestFile,
            $$"""
              {
                "formatVersion": 2,
                "corpus": "example-libraries",
                "contentVersion": {{(contentVersion is null ? "null" : $"\"{contentVersion}\"")}},
                "types": [{{string.Join(",", types.Select(t => $$"""{"type":"{{t}}","records":1}"""))}}]
              }
              """);

    // One file the plan would write, parsed. Paths are relative to `.dist/`, so a file inside the
    // plugin is addressed through the plugin directory and the marketplace is not.
    private static JsonElement Written(BundlePlan plan, string path, bool underPlugin = true)
    {
        var full = underPlugin ? $"{Dist.PluginDir}/{path}" : path;
        var file = plan.Files.SingleOrDefault(f => f.Path == full)
                   ?? throw new InvalidOperationException(
                       $"the plan writes no {full}; it writes {string.Join(", ", plan.Files.Select(f => f.Path))}");

        return JsonDocument.Parse(file.Content).RootElement;
    }
}
