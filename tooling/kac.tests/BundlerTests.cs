using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using kac.core;

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

    [Fact]
    public void An_export_naming_no_corpus_is_refused()
        => Assert.Contains("names no corpus",
            Assert.Single(Plan(
                plugin: [(Bundler.ManifestFile, Source())],
                export:
                [
                    (Exporter.ManifestFile,
                        $$"""{"formatVersion": {{Exporter.FormatVersion}}, "types": []}""")
                ]).Problems));

    [Fact]
    public void A_corpus_root_the_plugin_tree_already_uses_is_refused()
        => Assert.Contains("would overwrite the other",
            Assert.Single(Plan(
                plugin: [(Bundler.ManifestFile, Source()), ("corpus/notes.md", "mine")],
                export: [Manifest()]).Problems));

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

    [Fact]
    public void An_export_whose_format_version_this_tool_does_not_read_is_refused()
    {
        var problem = Assert.Single(Plan(
            plugin: [(Bundler.ManifestFile, Source())],
            export: [(Exporter.ManifestFile, """{"formatVersion":99,"corpus":"c","types":[]}""")]).Problems);

        Assert.Contains("99", problem);
        Assert.Contains(Exporter.FormatVersion.ToString(), problem);
    }

    [Fact]
    public void An_export_declaring_no_format_version_is_refused()
        => Assert.Contains("format version none",
            Assert.Single(Plan(
                plugin: [(Bundler.ManifestFile, Source())],
                export: [(Exporter.ManifestFile, """{"corpus":"c","types":[]}""")]).Problems));

    [Fact]
    public void The_breadcrumb_travels_beside_the_hook_that_prints_it()
        => Assert.Contains("example-libraries", Text(Plan(
            plugin:
            [
                (Bundler.ManifestFile, Source(Component("hooks", "glossary"))),
                ("hooks/hooks.json", "{}")
            ],
            export: [Manifest("glossary")]), Breadcrumb.RenderedFile));

    [Fact]
    public void A_plugin_with_no_hook_carries_no_breadcrumb()
        => Assert.DoesNotContain(
            $"{Dist.PluginDir}/{Breadcrumb.RenderedFile}",
            Plan(plugin: [(Bundler.ManifestFile, Source(Component("skills/look", "glossary")))],
                export: [Manifest("glossary")]).Files.Select(f => f.Path));

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

    [Fact]
    public void A_component_reading_a_shape_the_export_does_not_carry_stops_the_run()
    {
        var problem = Assert.Single(Plan(
            plugin: [(Bundler.ManifestFile, Source(Component("skills/look", "glossary@2")))],
            export: [Manifest("glossary")]).Problems);

        Assert.Contains("shape version 2", problem);
        Assert.Contains("carries version 1", problem);
    }

    [Fact]
    public void A_component_naming_no_shape_is_untouched_by_one()
        => Assert.Empty(Plan(
            plugin: [(Bundler.ManifestFile, Source(Component("hooks", "glossary")))],
            export: [Manifest("glossary")]).Problems);

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

    [Fact]
    public void A_component_requiring_nothing_travels_beside_one_that_earned_its_place()
    {
        var plan = Plan(
            plugin:
            [
                (Bundler.ManifestFile,
                    Source(Component("skills/support"), Component("skills/look", "glossary")))
            ],
            export: [Manifest("glossary")]);

        Assert.Equal(["skills/support", "skills/look"], plan.Included.Select(c => c.Path));
        Assert.Empty(plan.Trimmed);
    }

    [Fact]
    public void A_component_requiring_nothing_follows_the_last_one_it_supports_out()
    {
        var plan = Plan(
            plugin:
            [
                (Bundler.ManifestFile,
                    Source(Component("skills/support"), Component("skills/look", "glossary")))
            ],
            export: [Manifest()]);

        Assert.Empty(plan.Included);
        Assert.Equal(["skills/look", "skills/support"], plan.Trimmed.Select(c => c.Path));
        Assert.Equal("no component it supports survived",
            plan.Trimmed.Single(c => c.Path == "skills/support").Reason);
    }

    [Fact]
    public void A_standalone_component_stays_when_every_component_reading_a_type_has_gone()
    {
        var plan = Plan(
            plugin:
            [
                (Bundler.ManifestFile,
                    Source(Standalone("skills/alone"), Component("skills/support"),
                        Component("skills/look", "glossary")))
            ],
            export: [Manifest()]);

        Assert.Equal(["skills/alone"], plan.Included.Select(c => c.Path));
        Assert.Equal(["skills/look", "skills/support"], plan.Trimmed.Select(c => c.Path));
    }

    [Fact]
    public void A_standalone_component_naming_a_type_the_export_left_out_is_still_trimmed()
    {
        var plan = Plan(
            plugin:
            [
                (Bundler.ManifestFile,
                    Source($$"""{"path":"skills/alone","requires":["glossary"],"standalone":true}"""))
            ],
            export: [Manifest()]);

        Assert.Empty(plan.Included);
        Assert.Equal("the export carries no glossary", plan.Trimmed.Single().Reason);
    }

    [Fact]
    public void A_component_requiring_nothing_and_standing_alone_is_trimmed()
    {
        var plan = Plan(plugin: [(Bundler.ManifestFile, Source(Component("hooks/hooks.json")))],
            export: [Manifest()]);

        Assert.Empty(plan.Included);
        Assert.Equal(["hooks/hooks.json"], plan.Trimmed.Select(c => c.Path));
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

    [Fact]
    public void Trimming_a_component_leaves_a_sibling_whose_name_starts_the_same_way()
    {
        var plan = Plan(
            plugin:
            [
                (Bundler.ManifestFile, Source(Component("skills/a", "adrs"), Component("skills/ab", "glossary"))),
                ("skills/ab/SKILL.md", "kept")
            ],
            export: [Manifest("glossary")]);

        Assert.Contains(plan.Files, f => f.Path.EndsWith("skills/ab/SKILL.md", StringComparison.Ordinal));
    }

    [Fact]
    public void A_skill_directory_no_component_claims_is_refused()
        => Assert.Contains("'skills/stray'", Assert.Single(Plan(
            plugin:
            [
                (Bundler.ManifestFile, Source(Component("skills/look", "glossary"))),
                ("skills/look/SKILL.md", "declared"),
                ("skills/stray/SKILL.md", "undeclared"),
                ("skills/stray/references/notes.md", "undeclared")
            ],
            export: [Manifest("glossary")]).Problems));

    [Fact]
    public void A_hook_directory_no_component_claims_is_refused()
        => Assert.Contains("'hooks'", Assert.Single(Plan(
            plugin:
            [
                (Bundler.ManifestFile, Source(Component("skills/look", "glossary"))),
                ("hooks/hooks.json", "{}"),
                ("hooks/breadcrumb", "#!/bin/sh")
            ],
            export: [Manifest("glossary")]).Problems));

    [Fact]
    public void A_file_beside_a_component_declared_as_one_file_is_refused_by_its_own_path()
        => Assert.Contains("'hooks/breadcrumb'", Assert.Single(Plan(
            plugin:
            [
                (Bundler.ManifestFile, Source(Component("hooks/hooks.json", "glossary"))),
                ("hooks/hooks.json", "{}"),
                ("hooks/breadcrumb", "#!/bin/sh")
            ],
            export: [Manifest("glossary")]).Problems));

    [Fact]
    public void A_file_outside_the_skill_and_hook_directories_needs_no_component()
    {
        var plan = Plan(
            plugin: [(Bundler.ManifestFile, Source(Component("skills/look", "glossary"))), ("LICENSE", "MIT")],
            export: [Manifest("glossary")]);

        Assert.Empty(plan.Problems);
        Assert.Contains(plan.Files, f => f.Path.EndsWith("/LICENSE", StringComparison.Ordinal));
    }

    [Fact]
    public void A_skill_directory_a_trimmed_component_claims_is_not_refused()
        => Assert.Empty(Plan(
            plugin: [(Bundler.ManifestFile, Source(Component("skills/decide", "adrs"))), ("skills/decide/SKILL.md", "")],
            export: [Manifest("glossary")]).Problems);

    [Fact]
    public void A_plugin_left_with_no_component_warns_and_is_still_assembled()
    {
        var plan = Plan(plugin: [(Bundler.ManifestFile, Source(Component("skills/look", "glossary")))],
            export: [Manifest()]);

        Assert.Contains(plan.Warnings, w => w.Contains("every component was trimmed"));
        Assert.Contains(plan.Files, f => f.Path.EndsWith(Bundler.ManifestFile, StringComparison.Ordinal));
    }

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

    [Fact]
    public void The_export_format_version_is_not_the_plugin_version()
        => Assert.NotEqual("2", Written(Plan(
            plugin: [(Bundler.ManifestFile, Source())],
            export: [Versioned("2.3.4")]), Bundler.ManifestFile).GetProperty("version").GetString());

    [Fact]
    public void A_corpus_with_no_content_version_builds_a_manifest_with_none_and_says_so()
    {
        var plan = Plan(plugin: [(Bundler.ManifestFile, Source())], export: [Manifest()]);

        Assert.False(Written(plan, Bundler.ManifestFile).TryGetProperty("version", out _));
        Assert.Contains(plan.Warnings, w => w.Contains("no contentVersion"));
    }

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
        Assert.False(written.TryGetProperty("license", out _));
        Assert.False(written.TryGetProperty("keywords", out _));

        // `author` stays, because the format asks for one. What matters is that the template's name is gone.
        Assert.Equal("example-libraries", written.GetProperty("author").GetProperty("name").GetString());
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

    [Fact]
    public void The_keywords_are_the_types_the_export_carried()
        => Assert.Equal(["knowledge-as-code", "glossary", "policies"],
            Written(Plan(plugin: [(Bundler.ManifestFile, Source())], export: [Manifest("glossary", "policies")]),
                    Bundler.ManifestFile)
                .GetProperty("keywords").EnumerateArray().Select(k => k.Text()).ToArray());

    [Fact]
    public void The_manifest_is_written_identity_first()
    {
        var written = Written(
            Plan(plugin: [(Bundler.ManifestFile, Source())], export: [About()]), Bundler.ManifestFile);

        Assert.Equal(
            [
                "name", "displayName", "version", "description", "author", "homepage", "repository",
                "license", "keywords", "metadata"
            ],
            written.EnumerateObject().Select(p => p.Name).ToArray());
    }

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

    /// <summary>
    /// A marketplace refuses a source path containing <c>..</c>. <c>Dist.Root</c> says what follows from that.
    /// </summary>
    [Fact]
    public void The_marketplace_names_the_plugin_as_a_path_beneath_itself()
    {
        var source = Written(Plan(plugin: [(Bundler.ManifestFile, Source())], export: [Manifest()]),
                Dist.MarketplaceRel, underPlugin: false)
            .GetProperty("plugins")[0].GetProperty("source").GetString();

        Assert.Equal($"./{Dist.PluginDir}", source);
        Assert.DoesNotContain("..", source);
    }

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

    [Fact]
    public void The_record_carries_no_clock()
    {
        var record = Written(Plan(plugin: [(Bundler.ManifestFile, Source())], export: [Manifest()]),
            Bundler.RecordFile);

        Assert.False(record.TryGetProperty("generatedAt", out _));
        Assert.False(record.TryGetProperty("commit", out _));
    }

    [Fact]
    public void A_build_index_lists_every_rule_by_title_id_and_status()
    {
        var index = Text(Plan(
            plugin: [(Bundler.ManifestFile, Source(ViewComponent("views/build-index.md", View.BuildIndex, "standards@2")))],
            export: Standards(controls: false)), "views/build-index.md");

        Assert.Contains("- An artefact records its commit (`std-DEPLOY.commit`, draft)", index);
        Assert.Contains("- A call forwards the trace (`std-OBS.trace`, active)", index);
        Assert.DoesNotContain("**MUST**", index);
    }

    [Fact]
    public void A_build_index_names_the_lookup_skill_that_survived()
        => Assert.Contains("Ask the standards-lookup skill", Text(Plan(
            plugin:
            [
                (Bundler.ManifestFile,
                    Source(Component("skills/standards-lookup", "standards@2"),
                        ViewComponent("views/build-index.md", View.BuildIndex, "standards@2")))
            ],
            export: Standards(controls: false)), "views/build-index.md"));

    [Fact]
    public void A_build_index_with_no_lookup_skill_names_the_rules_file()
        => Assert.Contains("`corpus/standards/rules.jsonl`", Text(Plan(
            plugin: [(Bundler.ManifestFile, Source(ViewComponent("views/build-index.md", View.BuildIndex, "standards@2")))],
            export: Standards(controls: false)), "views/build-index.md"));

    [Fact]
    public void A_review_view_lists_each_rule_s_obligations_and_its_controls()
    {
        var review = Text(Plan(
            plugin: [(Bundler.ManifestFile, Source(ReviewView()))],
            export: Standards(controls: true)), "views/review.md");

        Assert.Contains("## std-DEPLOY: Deployment", review);
        Assert.Contains("Cite as `std-DEPLOY.commit`. Status: draft.", review);
        Assert.Contains("- An artefact **MUST** record the commit it was built from.", review);
        Assert.Contains("- A service **SHOULD** report the commit on its health endpoint.", review);
        Assert.Contains("Controls: `ctl-0002` Health reports commit (`not-enforced`, active).", review);
    }

    [Fact]
    public void A_review_view_says_where_no_control_checks_a_rule()
    {
        var review = Text(Plan(
            plugin: [(Bundler.ManifestFile, Source(ReviewView()))],
            export: Standards(controls: true)), "views/review.md");

        var trace = review[review.IndexOf("`std-OBS.trace`", StringComparison.Ordinal)..];
        Assert.Contains("Controls: none.", trace);
    }

    [Fact]
    public void A_review_view_renders_where_the_export_carries_no_controls()
    {
        var plan = Plan(plugin: [(Bundler.ManifestFile, Source(ReviewView()))], export: Standards(controls: false));
        var review = Text(plan, "views/review.md");

        Assert.Equal(["views/review.md"], plan.Included.Select(c => c.Path));
        Assert.Contains("exports no controls", review);
        Assert.DoesNotContain("Controls:", review);
    }

    [Fact]
    public void A_control_a_consumed_corpus_wrote_checks_that_corpus_s_rule()
    {
        const string inherited =
            """{"id":"eng:std-GATES.skip","title":"A check is skipped only under a deviation","obligations":[{"level":"MUST NOT","text":"A team **MUST NOT** skip a check."}],"record":"eng:std-GATES","part":"skip","status":"draft","shortcode":"eng"}""";

        (string, string)[] export =
        [
            .. Standards(controls: true).Where(f => f.Item1 != "standards/rules.jsonl"),
            ("standards/rules.jsonl", Rules() + "\n" + inherited),
            ("controls/eng/ctl-0004.json",
                """{"fields":{"id":"ctl-0004","title":"A skip cites a deviation","status":"active","verifies":["std-GATES"],"mechanism":"ci"}}""")
        ];

        var review = Text(Plan(plugin: [(Bundler.ManifestFile, Source(ReviewView()))], export: export),
            "views/review.md");

        Assert.Contains("Controls: `eng:ctl-0004` A skip cites a deviation (`ci`, active).", review);
    }

    [Fact]
    public void A_view_whose_types_the_export_lacks_is_trimmed_and_written_nowhere()
    {
        var plan = Plan(plugin: [(Bundler.ManifestFile, Source(ReviewView()))], export: [Manifest("glossary")]);

        Assert.Equal("the export carries no standards", Assert.Single(plan.Trimmed).Reason);
        Assert.DoesNotContain(plan.Files, f => f.Path.EndsWith("views/review.md", StringComparison.Ordinal));
    }

    [Fact]
    public void A_type_a_view_reads_where_present_at_another_shape_stops_the_run()
    {
        var problem = Assert.Single(Plan(
            plugin:
            [
                (Bundler.ManifestFile,
                    Source($$"""{"path":"views/review.md","view":"review","requires":["standards@2"],"optional":["controls@2"]}"""))
            ],
            export: Standards(controls: true)).Problems);

        Assert.Contains("'controls' shape version 2", problem);
    }

    [Fact]
    public void A_view_this_tool_does_not_write_is_refused()
        => Assert.Contains("'summary'", Assert.Single(Plan(
            plugin: [(Bundler.ManifestFile, Source(ViewComponent("views/summary.md", "summary", "standards@2")))],
            export: Standards(controls: false)).Problems));

    [Fact]
    public void A_view_path_the_plugin_tree_already_uses_is_refused()
        => Assert.Contains("views/review.md", Assert.Single(Plan(
            plugin: [(Bundler.ManifestFile, Source(ReviewView())), ("views/review.md", "mine")],
            export: Standards(controls: true)).Problems));

    [Fact]
    public void A_view_path_inside_the_corpus_root_is_refused()
        => Assert.Contains("corpus/review.md", Assert.Single(Plan(
            plugin: [(Bundler.ManifestFile, Source(ViewComponent("corpus/review.md", View.Review, "standards@2")))],
            export: Standards(controls: false)).Problems));

    private static string ViewComponent(string path, string view, params string[] requires) =>
        $$"""{"path":"{{path}}","view":"{{view}}","requires":[{{string.Join(",", requires.Select(r => $"\"{r}\""))}}]}""";

    private static string ReviewView() =>
        """{"path":"views/review.md","view":"review","requires":["standards@2"],"optional":["controls@1"]}""";

    private static string Rules() =>
        """
        {"id":"std-DEPLOY.commit","title":"An artefact records its commit","obligations":[{"level":"MUST","text":"An artefact **MUST** record the commit it was built from."},{"level":"SHOULD","text":"A service **SHOULD** report the commit on its health endpoint."}],"record":"std-DEPLOY","part":"commit","status":"draft"}
        {"id":"std-OBS.trace","title":"A call forwards the trace","obligations":[{"level":"MUST","text":"A service **MUST** forward the inbound traceparent."}],"record":"std-OBS","part":"trace","status":"active"}
        """;

    /// <summary>An export of two standards at shape 2, and of one control at shape 1 where <c>controls</c> is set.</summary>
    private static (string, string)[] Standards(bool controls)
    {
        var types = """
                    {"type":"standards","shapeVersion":2,"records":2,"parts":2,"dir":"standards",
                     "partsFile":"standards/rules.jsonl","recordKey":"record","partKey":"part","idKey":"id"}
                    """;
        if (controls) types += """,{"type":"controls","shapeVersion":1,"records":1,"parts":0,"dir":"controls"}""";

        (string, string)[] files =
        [
            (Exporter.ManifestFile,
                $$"""{"formatVersion": {{Exporter.FormatVersion}}, "corpus": "example-engineering", "types": [{{types}}]}"""),
            ("standards/rules.jsonl", Rules()),
            ("standards/std-DEPLOY.json", """{"fields":{"id":"std-DEPLOY","title":"Deployment"}}"""),
            ("standards/std-OBS.json", """{"fields":{"id":"std-OBS","title":"Observability"}}""")
        ];

        return controls
            ?
            [
                .. files,
                ("controls/ctl-0002.json",
                    """{"fields":{"id":"ctl-0002","title":"Health reports commit","status":"active","verifies":["std-DEPLOY"],"mechanism":"not-enforced"}}""")
            ]
            : files;
    }

    private static BundlePlan Plan(
        (string Path, string Content)[] plugin,
        (string Path, string Content)[] export) =>
        Bundler.Plan(new BundleSource([.. plugin.Select(File)], [.. export.Select(File)]));

    private static BundleFile File((string Path, string Content) f) =>
        new(f.Path, Encoding.UTF8.GetBytes(f.Content));

    /// <summary>
    /// A plugin manifest as a corpus writes one. Identity is generated from the export, so a corpus writes none of it
    /// and this carries none.
    /// </summary>
    private static string Source(params string[] components) =>
        $$"""
          {
            "metadata": { "corpusRoot": "corpus", "components": [{{string.Join(",", components)}}] }
          }
          """;

    private static string Component(string path, params string[] requires) =>
        $$"""{"path":"{{path}}","requires":[{{string.Join(",", requires.Select(r => $"\"{r}\""))}}]}""";

    private static string Standalone(string path) =>
        $$"""{"path":"{{path}}","requires":[],"standalone":true}""";

    [Fact]
    public void An_author_with_no_address_travels_with_the_key_the_export_states()
    {
        var plan = Plan(
            plugin: [(Bundler.ManifestFile, Source())],
            export:
            [
                (Exporter.ManifestFile,
                    $$"""
                      {
                        "formatVersion": {{Exporter.FormatVersion}},
                        "corpus": "example-libraries",
                        "about": {
                          "displayName": null, "description": null, "license": null,
                          "author": { "name": "Paul Law", "url": null }
                        },
                        "publishing": { "target": "none" },
                        "sources": [],
                        "types": []
                      }
                      """)
            ]);

        var author = Required.Json(Text(plan, Bundler.ManifestFile))["author"] as JsonObject;

        Assert.NotNull(author);
        Assert.Equal("Paul Law", author["name"]?.GetValue<string>());
        Assert.True(author.ContainsKey("url") && author["url"] is null);
    }

    private static (string, string) Manifest(params string[] types) => Versioned(null, types);

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

    /// <summary>Every type is at shape 1, which is what a component naming a shape is held against.</summary>
    private static string Entries(string[] types) =>
        string.Join(",", types.Select(t => $$"""{"type":"{{t}}","shapeVersion":1,"records":1}"""));

    [Fact]
    public void A_file_the_corpus_holds_wins_over_the_shared_tree()
    {
        var merged = Bundler.Merge(
            [File("skills/glossary-lookup/SKILL.md", "shared"), File("hooks/hooks.json", "{}")],
            [File("skills/glossary-lookup/SKILL.md", "ours")]);

        Assert.Equal(["hooks/hooks.json", "skills/glossary-lookup/SKILL.md"], merged.Select(f => f.Path));
        Assert.Equal("ours", Encoding.UTF8.GetString(merged.Single(f => f.Path.EndsWith("SKILL.md")).Content));
    }

    [Fact]
    public void The_shared_tree_s_manifest_is_left_behind()
    {
        var merged = Bundler.Merge(
            [File(Bundler.ManifestFile, """{"name":"theirs"}"""), File("hooks/hooks.json", "{}")],
            [File(Bundler.ManifestFile, """{"name":"ours"}""")]);

        Assert.Equal("""{"name":"ours"}""",
            Encoding.UTF8.GetString(merged.Single(f => f.Path == Bundler.ManifestFile).Content));
    }

    [Fact]
    public void A_corpus_holding_nothing_takes_the_shared_tree_whole()
        => Assert.Equal(["hooks/hooks.json"],
            Bundler.Merge([File("hooks/hooks.json", "{}")], []).Select(f => f.Path));

    private static BundleFile File(string path, string content) =>
        new(path, Encoding.UTF8.GetBytes(content));

    private static string Text(BundlePlan plan, string path)
    {
        var full = $"{Dist.PluginDir}/{path}";
        var file = plan.Files.SingleOrDefault(f => f.Path == full)
                   ?? throw new InvalidOperationException(
                       $"the plan writes no {full}; it writes {string.Join(", ", plan.Files.Select(f => f.Path))}");

        return Encoding.UTF8.GetString(file.Content);
    }

    /// <summary>
    /// Paths are relative to <c>.dist/</c>, so a file inside the plugin is addressed through the plugin directory and
    /// the marketplace is not.
    /// </summary>
    private static JsonElement Written(BundlePlan plan, string path, bool underPlugin = true)
    {
        var full = underPlugin ? $"{Dist.PluginDir}/{path}" : path;
        var file = plan.Files.SingleOrDefault(f => f.Path == full)
                   ?? throw new InvalidOperationException(
                       $"the plan writes no {full}; it writes {string.Join(", ", plan.Files.Select(f => f.Path))}");

        return JsonDocument.Parse(file.Content).RootElement;
    }
}
