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

    [Fact]
    public void A_plugin_tree_with_no_manifest_is_refused()
        => Assert.Contains(".claude-plugin/plugin.json",
            Assert.Single(Plan(plugin: [("README.md", "# nothing")], export: [Manifest()]).Problems));

    [Fact]
    public void A_manifest_that_is_not_JSON_is_refused()
        => Assert.Contains("not a JSON object",
            Assert.Single(Plan(plugin: [(Bundler.ManifestFile, "not json at all")], export: [Manifest()]).Problems));

    // The name is what a marketplace installs by and what a user types for it afterwards. It is the
    // corpus's own name rather than a second one beside it, so an export naming no corpus has no plugin
    // to build.
    [Fact]
    public void An_export_naming_no_corpus_is_refused()
        => Assert.Contains("names no corpus",
            Assert.Single(Plan(
                plugin: [(Bundler.ManifestFile, Source())],
                export: [(Exporter.ManifestFile,
                    $$"""{"formatVersion": {{Exporter.FormatVersion}}, "types": []}""")]).Problems));

    // The export is copied under `corpusRoot`, so the only other answer is to merge it into the plugin
    // tree.
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

    [Fact]
    public void A_refused_plan_names_no_files()
        => Assert.Empty(Plan(plugin: [("README.md", "# nothing")], export: [Manifest()]).Files);

    // Both numbers are named, because the reader's next move differs.
    [Fact]
    public void An_export_whose_format_version_this_tool_does_not_read_is_refused()
    {
        var problem = Assert.Single(Plan(
            plugin: [(Bundler.ManifestFile, Source())],
            export: [(Exporter.ManifestFile, """{"formatVersion":99,"corpus":"c","types":[]}""")]).Problems);

        Assert.Contains("99", problem);
        Assert.Contains(Exporter.FormatVersion.ToString(), problem);
    }

    // A document that does not say which contract it is written to is not one to guess about.
    [Fact]
    public void An_export_declaring_no_format_version_is_refused()
        => Assert.Contains("format version none",
            Assert.Single(Plan(
                plugin: [(Bundler.ManifestFile, Source())],
                export: [(Exporter.ManifestFile, """{"corpus":"c","types":[]}""")]).Problems));

    // The hook is one `cat`, and the consumer's shell is asked for nothing else.
    [Fact]
    public void The_breadcrumb_travels_beside_the_hook_that_prints_it()
        => Assert.Contains("example-libraries", Text(Plan(
            plugin:
            [
                (Bundler.ManifestFile, Source(Component("hooks", "glossary"))),
                ("hooks/hooks.json", "{}")
            ],
            export: [Manifest("glossary")]), Breadcrumb.RenderedFile));

    // The file exists to be printed, and one nothing prints is weight in an artefact nobody reviews.
    [Fact]
    public void A_plugin_with_no_hook_carries_no_breadcrumb()
        => Assert.DoesNotContain(
            $"{Dist.PluginDir}/{Breadcrumb.RenderedFile}",
            Plan(plugin: [(Bundler.ManifestFile, Source(Component("skills/look", "glossary")))],
                export: [Manifest("glossary")]).Files.Select(f => f.Path));

    // The other answer is a hook printing a file that says the corpus knows nothing.
    [Fact]
    public void A_trimmed_hook_takes_the_breadcrumb_with_it()
    {
        var plan = Plan(
            plugin:
            [
                (Bundler.ManifestFile, Source(Component("hooks", "glossary"))),
                ("hooks/hooks.json", "{}")
            ],
            export: [Manifest("adrs")]);

        Assert.Equal("hooks", Assert.Single(plan.Trimmed).Path);
        Assert.DoesNotContain($"{Dist.PluginDir}/{Breadcrumb.RenderedFile}", plan.Files.Select(f => f.Path));
        Assert.DoesNotContain($"{Dist.PluginDir}/hooks/hooks.json", plan.Files.Select(f => f.Path));
    }

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

    // The author's next move is to export the type the reason names.
    [Fact]
    public void A_component_needing_two_types_is_trimmed_on_the_one_that_is_missing()
        => Assert.Equal("the export carries no adrs",
            Assert.Single(Plan(
                plugin: [(Bundler.ManifestFile, Source(Component("skills/both", "glossary", "adrs")))],
                export: [Manifest("glossary")]).Trimmed).Reason);

    [Fact]
    public void A_component_reading_the_shape_the_export_carries_is_included()
    {
        var plan = Plan(plugin: [(Bundler.ManifestFile, Source(Component("skills/look", "glossary@1")))],
            export: [Manifest("glossary")]);

        Assert.Equal(["skills/look"], plan.Included.Select(c => c.Path));
        Assert.Empty(plan.Problems);
    }

    // Both numbers are named, because the author cannot tell from one whether to rebuild the export or
    // rewrite the component.
    [Fact]
    public void A_component_reading_a_shape_the_export_does_not_carry_stops_the_run()
    {
        var problem = Assert.Single(Plan(
            plugin: [(Bundler.ManifestFile, Source(Component("skills/look", "glossary@2")))],
            export: [Manifest("glossary")]).Problems);

        Assert.Contains("shape version 2", problem);
        Assert.Contains("carries version 1", problem);
    }

    // A bare name needs the type present and opens none of its files, which is what a breadcrumb hook does.
    [Fact]
    public void A_component_naming_no_shape_is_untouched_by_one()
        => Assert.Empty(Plan(
            plugin: [(Bundler.ManifestFile, Source(Component("hooks", "glossary")))],
            export: [Manifest("glossary")]).Problems);

    // A plugin that does less is the answer here, and refusing the run would be the wrong one.
    [Fact]
    public void A_component_reading_a_shape_of_a_type_the_export_omits_is_trimmed()
    {
        var plan = Plan(plugin: [(Bundler.ManifestFile, Source(Component("skills/look", "glossary@1")))],
            export: [Manifest("adrs")]);

        Assert.Empty(plan.Problems);
        Assert.Equal("the export carries no glossary", Assert.Single(plan.Trimmed).Reason);
    }

    [Fact]
    public void A_shape_that_is_not_a_number_is_reported_against_the_manifest()
        => Assert.Contains("a shape version is a whole number", Assert.Single(Plan(
            plugin: [(Bundler.ManifestFile, Source(Component("skills/look", "glossary@one")))],
            export: [Manifest("glossary")]).Problems));

    // `bundle.json` still records a component requiring nothing, because a reader asking what is in the
    // plugin wants it listed beside the ones that had to earn their place.
    [Fact]
    public void A_component_requiring_nothing_always_travels()
    {
        var plan = Plan(plugin: [(Bundler.ManifestFile, Source(Component("hooks/hooks.json")))],
            export: [Manifest()]);

        Assert.Equal(["hooks/hooks.json"], plan.Included.Select(c => c.Path));
    }

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

    // A path beginning with a component's name but not beneath it is a different component.
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

    // A plugin declaring none is equally useless, and there is nothing to blame the corpus for.
    [Fact]
    public void A_plugin_declaring_no_component_warns_in_its_own_words()
        => Assert.Contains("declares no components",
            Assert.Single(Plan(plugin: [(Bundler.ManifestFile, Source())],
                export: [Versioned("2.3.4", "glossary")]).Warnings));

    [Fact]
    public void The_version_is_the_corpus_content_version()
        => Assert.Equal("2.3.4", Written(Plan(
            plugin: [(Bundler.ManifestFile, Source())],
            export: [Versioned("2.3.4")]), Bundler.ManifestFile).GetProperty("version").GetString());

    // Stamping the format version onto the plugin would tell a reader which parser to use, over data
    // whose meaning had changed underneath it.
    [Fact]
    public void The_export_format_version_is_not_the_plugin_version()
        => Assert.NotEqual("2", Written(Plan(
            plugin: [(Bundler.ManifestFile, Source())],
            export: [Versioned("2.3.4")]), Bundler.ManifestFile).GetProperty("version").GetString());

    // There is nothing to fall back to. The version is the corpus's `content-version` and no second copy
    // of it exists, so a corpus that states none builds a manifest nothing will install, and is told.
    [Fact]
    public void A_corpus_with_no_content_version_builds_a_manifest_with_none_and_says_so()
    {
        var plan = Plan(plugin: [(Bundler.ManifestFile, Source())], export: [Manifest()]);

        Assert.False(Written(plan, Bundler.ManifestFile).TryGetProperty("version", out _));
        Assert.Contains(plan.Warnings, w => w.Contains("no contentVersion"));
    }

    // The fault this generation exists to prevent. A corpus copies its manifest from a template, and a
    // template naming an author hands every copy somebody else's identity to publish under.
    [Fact]
    public void An_identity_key_the_corpus_never_declared_is_removed_rather_than_inherited()
    {
        const string inherited = """
                                 {
                                   "name": "knowledge-as-code",
                                   "author": { "name": "Somebody Else" },
                                   "license": "MIT",
                                   "keywords": [ "glossary" ],
                                   "metadata": { "corpusRoot": "corpus", "components": [] }
                                 }
                                 """;

        var written = Written(
            Plan(plugin: [(Bundler.ManifestFile, inherited)], export: [Manifest()]), Bundler.ManifestFile);

        Assert.Equal("example-libraries", written.GetProperty("name").GetString());
        Assert.False(written.TryGetProperty("author", out _));
        Assert.False(written.TryGetProperty("license", out _));
        Assert.False(written.TryGetProperty("keywords", out _));
    }

    [Fact]
    public void The_identity_the_corpus_declared_is_what_travels()
    {
        var written = Written(
            Plan(plugin: [(Bundler.ManifestFile, Source())], export: [About()]), Bundler.ManifestFile);

        Assert.Equal("Example Libraries", written.GetProperty("displayName").GetString());
        Assert.Equal("A worked example.", written.GetProperty("description").GetString());
        Assert.Equal("Paul Law", written.GetProperty("author").GetProperty("name").GetString());
        Assert.Equal("MIT", written.GetProperty("license").GetString());
        Assert.Equal("https://example.com/corpus", written.GetProperty("homepage").GetString());
        Assert.Equal("https://example.com/corpus", written.GetProperty("repository").GetString());
    }

    // A plugin advertising a type its corpus declined sends a reader looking for records that are not
    // in it, so the words come from what the export actually carried.
    [Fact]
    public void The_keywords_are_the_types_the_export_carried()
        => Assert.Equal(["knowledge-as-code", "glossary", "policies"],
            Written(Plan(plugin: [(Bundler.ManifestFile, Source())], export: [Manifest("glossary", "policies")]),
                    Bundler.ManifestFile)
                .GetProperty("keywords").EnumerateArray().Select(k => k.GetString()!).ToArray());

    // A reader meets the plugin's identity before its declarations, whatever order the keys were added
    // in. `metadata` is last of the ones named, and anything this tool never heard of follows it.
    [Fact]
    public void The_manifest_is_written_identity_first()
    {
        var written = Written(
            Plan(plugin: [(Bundler.ManifestFile, Source())], export: [About()]), Bundler.ManifestFile);

        Assert.Equal(
            ["name", "displayName", "version", "description", "author", "homepage", "repository",
             "license", "keywords", "metadata"],
            written.EnumerateObject().Select(p => p.Name).ToArray());
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

    [Fact]
    public void The_export_is_copied_under_the_corpus_root_the_manifest_names()
        => Assert.Contains(
            Plan(plugin: [(Bundler.ManifestFile, Source())], export: [Manifest(), ("glossary/terms.jsonl", "{}")])
                .Files,
            f => f.Path == "plugin/corpus/glossary/terms.jsonl");

    // The copy is the seam between the two commands. A bundle that edited what it copied would make a
    // difference between the two copies something to interpret rather than a defect.
    [Fact]
    public void The_export_is_copied_byte_for_byte()
    {
        const string line = """{"id":"gls-a.term","title":"Term"}""";
        var copied = Assert.Single(
            Plan(plugin: [(Bundler.ManifestFile, Source())], export: [Manifest(), ("glossary/terms.jsonl", line)])
                .Files,
            f => f.Path.EndsWith("terms.jsonl", StringComparison.Ordinal));

        Assert.Equal(line, Encoding.UTF8.GetString(copied.Content));
    }

    // A marketplace refuses a source path containing `..`. `Dist.Root` says what follows from that.
    [Fact]
    public void The_marketplace_names_the_plugin_as_a_path_beneath_itself()
    {
        var source = Written(Plan(plugin: [(Bundler.ManifestFile, Source())], export: [Manifest()]),
                Dist.MarketplaceRel, underPlugin: false)
            .GetProperty("plugins")[0].GetProperty("source").GetString();

        Assert.Equal($"./{Dist.PluginDir}", source);
        Assert.DoesNotContain("..", source);
    }

    // The marketplace and the plugin it offers are the two words a reader types to install. Nothing in
    // the name says where this copy sits: the same file is what gets published, so a name qualified by
    // the path it was built at would be wrong the moment it moved.
    [Fact]
    public void The_marketplace_takes_the_name_of_the_plugin_it_offers()
    {
        var marketplace = Written(Plan(plugin: [(Bundler.ManifestFile, Source())], export: [Manifest()]),
            Dist.MarketplaceRel, underPlugin: false);

        Assert.Equal("example-libraries", marketplace.GetProperty("name").GetString());
        Assert.Equal("example-libraries", marketplace.GetProperty("plugins")[0].GetProperty("name").GetString());
    }

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

    private static BundlePlan Plan(
        (string Path, string Content)[] plugin,
        (string Path, string Content)[] export) =>
        Bundler.Plan(new BundleSource([.. plugin.Select(File)], [.. export.Select(File)]));

    private static BundleFile File((string Path, string Content) f) =>
        new(f.Path, Encoding.UTF8.GetBytes(f.Content));

    // A plugin manifest as a corpus writes one: the corpus root the skills address the export through,
    // and whatever components the case needs. Identity is generated from the export, so a corpus writes
    // none of it and this carries none.
    private static string Source(params string[] components) =>
        $$"""
          {
            "metadata": { "corpusRoot": "corpus", "components": [{{string.Join(",", components)}}] }
          }
          """;

    private static string Component(string path, params string[] requires) =>
        $$"""{"path":"{{path}}","requires":[{{string.Join(",", requires.Select(r => $"\"{r}\""))}}]}""";

    // An export manifest as `kac export` writes one, carrying the four keys a bundle reads from it.
    private static (string, string) Manifest(params string[] types) => Versioned(null, types);

    // An export from a corpus that said who it is, so the generated half has something to carry.
    private static (string, string) About() =>
        (Exporter.ManifestFile,
            $$"""
              {
                "formatVersion": {{Exporter.FormatVersion}},
                "corpus": "example-libraries",
                "contentVersion": "1.2.3",
                "about": {
                  "displayName": "Example Libraries",
                  "description": "A worked example.",
                  "author": { "name": "Paul Law", "url": "https://example.com/paul" },
                  "license": "MIT"
                },
                "publishing": { "base": "https://example.com/corpus" },
                "types": [{"type":"glossary","shapeVersion":1,"records":1}]
              }
              """);

    private static (string, string) Versioned(string? contentVersion, params string[] types) =>
        (Exporter.ManifestFile,
            $$"""
              {
                "formatVersion": {{Exporter.FormatVersion}},
                "corpus": "example-libraries",
                "contentVersion": {{(contentVersion is null ? "null" : $"\"{contentVersion}\"")}},
                "types": [{{Entries(types)}}]
              }
              """);

    // The manifest's type entries. Every type is at shape 1, which is what a component naming a shape
    // is held against.
    private static string Entries(string[] types) =>
        string.Join(",", types.Select(t => $$"""{"type":"{{t}}","shapeVersion":1,"records":1}"""));

    // Several corpora in one repository share a plugin tree, and each may still write a component of
    // its own. The corpus's copy is the one that travels.
    [Fact]
    public void A_file_the_corpus_holds_wins_over_the_shared_tree()
    {
        var merged = Bundler.Merge(
            [File("skills/glossary-lookup/SKILL.md", "shared"), File("hooks/hooks.json", "{}")],
            [File("skills/glossary-lookup/SKILL.md", "ours")]);

        Assert.Equal(["hooks/hooks.json", "skills/glossary-lookup/SKILL.md"], merged.Select(f => f.Path));
        Assert.Equal("ours", Encoding.UTF8.GetString(merged.Single(f => f.Path.EndsWith("SKILL.md")).Content));
    }

    // The manifest carries the name a plugin installs under, so two corpora sharing a tree would ship
    // one name between them. A corpus that wrote none of its own is refused by `Plan` instead.
    [Fact]
    public void The_shared_tree_s_manifest_is_left_behind()
    {
        var merged = Bundler.Merge(
            [File(Bundler.ManifestFile, """{"name":"theirs"}"""), File("hooks/hooks.json", "{}")],
            [File(Bundler.ManifestFile, """{"name":"ours"}""")]);

        Assert.Equal("""{"name":"ours"}""",
            Encoding.UTF8.GetString(merged.Single(f => f.Path == Bundler.ManifestFile).Content));
    }

    // A corpus taking the whole of a shared tree writes nothing of its own bar the manifest, so the
    // merge has to stand up to an empty second tree.
    [Fact]
    public void A_corpus_holding_nothing_takes_the_shared_tree_whole()
        => Assert.Equal(["hooks/hooks.json"],
            Bundler.Merge([File("hooks/hooks.json", "{}")], []).Select(f => f.Path));

    private static BundleFile File(string path, string content) =>
        new(path, Encoding.UTF8.GetBytes(content));

    // One file the plan would write, as text. The breadcrumb is prose rather than a document, so it is
    // read whole rather than picked apart.
    private static string Text(BundlePlan plan, string path)
    {
        var full = $"{Dist.PluginDir}/{path}";
        var file = plan.Files.SingleOrDefault(f => f.Path == full)
                   ?? throw new InvalidOperationException(
                       $"the plan writes no {full}; it writes {string.Join(", ", plan.Files.Select(f => f.Path))}");

        return Encoding.UTF8.GetString(file.Content);
    }

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
