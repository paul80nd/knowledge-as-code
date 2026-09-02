using kac.core;

// `Update.Plan` decides from two listings and a manifest, so every arm of it is a case here rather than
// a tree on disk. The golden `update` scenario is the layer above: it stands a corpus up from the real
// template, breaks it, and proves the round trip.
//
// `docs/cli/update.md` says what each layer means. Nothing below repeats that.

namespace kac.tests;

public class UpdateTests
{
    // A manifest holding one rule per layer, and one that relocates. Written here rather than read from
    // the repository, so a case says what it is about and a rule added upstream cannot rewrite it.
    private static Manifest Rules() => new()
    {
        Version = 4,
        Rules =
        [
            new ManifestRule([".schema/**"], Manifest.Overlay),
            new ManifestRule(["template/pages/**"], Manifest.Overlay, "pages/"),
            new ManifestRule(["template/*.md"], Manifest.Seed, ""),
            new ManifestRule(["template/policies/**"], Manifest.Seed, "policies/"),
            new ManifestRule(["template/ci.yml"], Manifest.Seed, "", "github"),
            new ManifestRule(["old/**"], Manifest.Removed, "legacy/"),
            new ManifestRule(["template/.plugin/.claude-plugin/plugin.json"], Manifest.Seed,
                ".plugin/.claude-plugin/"),
            new ManifestRule(["template/.plugin/**"], Manifest.Overlay, ".plugin/"),
            new ManifestRule(["**"], Manifest.Withheld)
        ]
    };

    private static HashSet<string> Files(params string[] paths) => new(paths, StringComparer.Ordinal);

    // No file carries an id unless a case says so, so a seed is matched by path alone as it always was.
    private static readonly RecordIds Anonymous = new(_ => null, _ => null);

    // One id on the template's side and one on the corpus's, which is all a case about a moved seed
    // needs. Every other file carries none.
    private static RecordIds Carrying(string seed, string path, string held) =>
        new(_ => seed, rel => rel.Equals(path, StringComparison.Ordinal) ? held : null);

    // Every type adopted, so nothing is declined unless a case says so.
    private static UpdateTypes Everything(params string[] declared) =>
        new(declared, declared, _ => false);

    private static UpdatePlan Plan(IReadOnlySet<string> template, IReadOnlySet<string> corpus,
        CorpusDescriptor? descriptor = null, UpdateTypes? types = null,
        string policy = CorpusDescriptor.Cautious, bool readInPlace = false, bool same = false,
        RecordIds? ids = null) =>
        Update.Plan(template, corpus, Rules(), descriptor ?? new CorpusDescriptor(),
            types ?? Everything(), policy, readInPlace, _ => same, ids ?? Anonymous);

    // A corpus sharing one plugin tree with its siblings holds none of the shared half, so an update
    // that wrote a copy here would put the tree back the moment it was taken away.
    [Fact]
    public void A_corpus_reading_the_plugin_tree_elsewhere_receives_none_of_it()
    {
        var plan = Plan(
            Files("template/.plugin/skills/glossary-lookup/SKILL.md", "template/.plugin/hooks/hooks.json"),
            Files(),
            descriptor: new CorpusDescriptor { PluginFrom = "../../template/.plugin" });

        Assert.Empty(plan.Copies);
        Assert.Equal(2, plan.DeclinedPlugin);
        Assert.False(plan.Changes);
    }

    // The manifest names the plugin and lists the components this corpus declares, so it is a seed and
    // arrives whatever the descriptor says about the rest of the tree.
    [Fact]
    public void Its_own_plugin_manifest_arrives_all_the_same()
    {
        var plan = Plan(
            Files("template/.plugin/.claude-plugin/plugin.json", "template/.plugin/hooks/hooks.json"),
            Files(),
            descriptor: new CorpusDescriptor { PluginFrom = "../../template/.plugin" });

        Assert.Equal([".plugin/.claude-plugin/plugin.json"], plan.Seeded.Select(f => f.To));
        Assert.Equal(1, plan.DeclinedPlugin);
    }

    // A corpus that adopted `plugin.from` with the old copies still on disk. `Merge` gives its own file
    // priority, so a leftover would win over the shared tree for good and no upstream change would reach
    // it. Reported as a file nothing sends to, which is what it now is.
    [Fact]
    public void A_copy_left_behind_by_adopting_the_key_is_reported()
    {
        var plan = Plan(
            Files("template/.plugin/skills/glossary-lookup/SKILL.md"),
            Files(".plugin/skills/glossary-lookup/SKILL.md"),
            descriptor: new CorpusDescriptor { PluginFrom = "../../template/.plugin" });

        Assert.Equal([".plugin/skills/glossary-lookup/SKILL.md"], plan.Unshared);
        Assert.True(plan.Changes);
    }

    // The ordinary case, and the one a corpus standing on its own is always in.
    [Fact]
    public void A_corpus_saying_nothing_receives_the_plugin_tree()
    {
        var plan = Plan(Files("template/.plugin/hooks/hooks.json"), Files());

        Assert.Equal([".plugin/hooks/hooks.json"], plan.Written.Select(f => f.To));
        Assert.Equal(0, plan.DeclinedPlugin);
    }

    [Fact]
    public void An_overlay_file_whose_copies_differ_is_written()
    {
        var plan = Plan(Files(".schema/adrs.yaml"), Files(".schema/adrs.yaml"));

        Assert.Equal([".schema/adrs.yaml"], plan.Written.Select(f => f.To));
        Assert.Equal(0, plan.InStep);
        Assert.True(plan.Changes);
    }

    [Fact]
    public void An_overlay_file_already_matching_is_left_alone()
    {
        var plan = Plan(Files(".schema/adrs.yaml"), Files(".schema/adrs.yaml"), same: true);

        Assert.Empty(plan.Written);
        Assert.Equal(1, plan.InStep);
        Assert.False(plan.Changes);
    }

    [Fact]
    public void An_overlay_file_the_corpus_lacks_is_written()
    {
        var plan = Plan(Files(".schema/adrs.yaml"), Files(), same: true);

        Assert.Equal([".schema/adrs.yaml"], plan.Written.Select(f => f.To));
    }

    [Fact]
    public void A_relocated_file_is_planned_against_where_it_lands()
    {
        var plan = Plan(Files("template/pages/taxonomy.md"), Files());

        var file = Assert.Single(plan.Written);
        Assert.Equal("template/pages/taxonomy.md", file.From);
        Assert.Equal("pages/taxonomy.md", file.To);
    }

    [Fact]
    public void A_seed_the_corpus_holds_is_left_alone_under_cautious()
    {
        var plan = Plan(Files("template/CLAUDE.md"), Files("CLAUDE.md"));

        Assert.Empty(plan.Seeded);
        Assert.Equal(1, plan.InStep);
    }

    [Fact]
    public void A_seed_the_corpus_lacks_is_written_under_cautious()
    {
        var plan = Plan(Files("template/CLAUDE.md"), Files());

        Assert.Equal(["CLAUDE.md"], plan.Seeded.Select(f => f.To));
    }

    [Fact]
    public void A_seed_the_corpus_holds_is_refreshed_under_full()
    {
        var plan = Plan(Files("template/CLAUDE.md"), Files("CLAUDE.md"), policy: CorpusDescriptor.Full);

        Assert.Equal(["CLAUDE.md"], plan.Seeded.Select(f => f.To));
    }

    // Filing a record in a folder is how a corpus sets its category, so this is the ordinary case.
    [Fact]
    public void A_seeded_record_the_corpus_filed_under_a_category_is_already_held()
    {
        var plan = Plan(
            Files("template/policies/devi-deviations-are-recorded.md"),
            Files("policies/governance/devi-deviations-are-recorded.md"),
            ids: Carrying("pol-DEVI", "policies/governance/devi-deviations-are-recorded.md", "pol-DEVI"));

        Assert.Empty(plan.Seeded);
        Assert.Equal(1, plan.InStep);
        Assert.False(plan.Changes);
    }

    // The corpus holds a record of that type, and the id says it is a different one.
    [Fact]
    public void A_seeded_record_the_corpus_has_none_of_is_still_written()
    {
        var plan = Plan(
            Files("template/policies/devi-deviations-are-recorded.md"),
            Files("policies/governance/know-knowledge-is-written-down.md"),
            ids: Carrying("pol-DEVI", "policies/governance/know-knowledge-is-written-down.md", "pol-KNOW"));

        Assert.Equal(["policies/devi-deviations-are-recorded.md"], plan.Seeded.Select(f => f.To));
    }

    [Fact]
    public void A_record_of_the_same_id_outside_the_type_folder_is_not_that_seed()
    {
        var plan = Plan(
            Files("template/policies/devi-deviations-are-recorded.md"),
            Files("standards/devi-deviations-are-recorded.md"),
            ids: Carrying("pol-DEVI", "standards/devi-deviations-are-recorded.md", "pol-DEVI"));

        Assert.Equal(["policies/devi-deviations-are-recorded.md"], plan.Seeded.Select(f => f.To));
    }

    // `full` refreshes a seed against the template, and a record's relative links are written for the
    // depth it was seeded at. There is no copy that would land correctly a folder down.
    [Fact]
    public void A_moved_seed_is_left_alone_under_full_too()
    {
        var plan = Plan(
            Files("template/policies/devi-deviations-are-recorded.md"),
            Files("policies/governance/devi-deviations-are-recorded.md"),
            policy: CorpusDescriptor.Full,
            ids: Carrying("pol-DEVI", "policies/governance/devi-deviations-are-recorded.md", "pol-DEVI"));

        Assert.Empty(plan.Seeded);
        Assert.Equal(1, plan.InStep);
        Assert.False(plan.Changes);
    }

    // `id-unique` reads two ids differing only in case as one, so the match in front of it has to.
    [Fact]
    public void A_moved_seed_is_matched_however_the_corpus_cased_its_id()
    {
        var plan = Plan(
            Files("template/policies/devi-deviations-are-recorded.md"),
            Files("policies/governance/devi-deviations-are-recorded.md"),
            ids: Carrying("pol-DEVI", "policies/governance/devi-deviations-are-recorded.md", "pol-Devi"));

        Assert.Empty(plan.Seeded);
        Assert.Equal(1, plan.InStep);
    }

    // The policy widens what is compared and never what is copied.
    [Fact]
    public void A_seed_that_already_matches_is_left_alone_under_full()
    {
        var plan = Plan(Files("template/CLAUDE.md"), Files("CLAUDE.md"), policy: CorpusDescriptor.Full,
            same: true);

        Assert.Empty(plan.Seeded);
        Assert.Equal(1, plan.InStep);
    }

    // The template no longer holds the file a tombstone names, so nothing but the corpus-side pass
    // reaches one.
    [Fact]
    public void A_tombstoned_file_the_corpus_still_holds_is_deleted()
    {
        var plan = Plan(Files(), Files("legacy/gone.md", "pages/taxonomy.md"));

        Assert.Equal(["legacy/gone.md"], plan.Deleted);
        Assert.True(plan.Changes);
    }

    [Fact]
    public void A_tombstone_naming_nothing_the_corpus_holds_deletes_nothing()
    {
        var plan = Plan(Files(), Files("pages/taxonomy.md"));

        Assert.Empty(plan.Deleted);
    }

    [Fact]
    public void A_withheld_file_reaches_no_corpus()
    {
        var plan = Plan(Files("tooling/kac/Program.cs"), Files());

        Assert.Empty(plan.Written);
        Assert.Empty(plan.Seeded);
        Assert.False(plan.Changes);
    }

    [Fact]
    public void A_skipped_path_is_neither_read_nor_written()
    {
        var descriptor = new CorpusDescriptor();
        descriptor.Skipped.Add(new SkippedFile(".schema/adrs.yaml", "Patched for our proxy."));

        var plan = Plan(Files(".schema/adrs.yaml"), Files(".schema/adrs.yaml"), descriptor);

        Assert.Empty(plan.Written);
        Assert.Contains("Patched for our proxy.", Assert.Single(plan.Skipped));
    }

    // A deletion is the direction that cannot be undone by hand, so `skip:` has to hold there too.
    [Fact]
    public void A_skipped_path_is_not_deleted_by_a_tombstone()
    {
        var descriptor = new CorpusDescriptor();
        descriptor.Skipped.Add(new SkippedFile("legacy/gone.md", "We still run this."));

        var plan = Plan(Files(), Files("legacy/gone.md"), descriptor);

        Assert.Empty(plan.Deleted);
        Assert.Single(plan.Skipped);
    }

    [Fact]
    public void A_declined_types_files_are_withheld_and_counted()
    {
        var types = new UpdateTypes(["adrs", "runbooks"], ["adrs"],
            rel => rel.Equals(".schema/runbooks.yaml", StringComparison.Ordinal));

        var plan = Plan(Files(".schema/adrs.yaml", ".schema/runbooks.yaml"), Files(), types: types);

        Assert.Equal([".schema/adrs.yaml"], plan.Written.Select(f => f.To));
        Assert.Equal(1, plan.Declined);
    }

    [Fact]
    public void A_type_the_template_declares_and_the_corpus_has_not_adopted_is_offered()
    {
        var types = new UpdateTypes(["adrs", "runbooks"], ["adrs"], _ => false);

        Assert.Equal(["runbooks"], Plan(Files(), Files(), types: types).Offered);
    }

    [Fact]
    public void A_corpus_declaring_no_types_is_offered_none()
    {
        var types = new UpdateTypes(["adrs", "runbooks"], null, _ => false);

        Assert.Empty(Plan(Files(), Files(), types: types).Offered);
    }

    [Fact]
    public void A_ci_starter_the_corpus_does_not_hold_is_never_introduced()
    {
        var plan = Plan(Files("template/ci.yml"), Files());

        Assert.Empty(plan.Seeded);
        Assert.Equal(1, plan.DeclinedCi);
        Assert.Equal(0, plan.Declined);
    }

    [Fact]
    public void A_ci_starter_the_corpus_holds_is_treated_as_any_other_seed()
    {
        var plan = Plan(Files("template/ci.yml"), Files("ci.yml"), policy: CorpusDescriptor.Full);

        Assert.Equal(["ci.yml"], plan.Seeded.Select(f => f.To));
        Assert.Equal(0, plan.DeclinedCi);
    }

    [Fact]
    public void A_starter_for_a_system_the_tool_cannot_offer_stops_the_run()
    {
        var manifest = new Manifest
        {
            Rules = [new ManifestRule(["template/ci.yml"], Manifest.Seed, "", "jenkins")]
        };

        var plan = Update.Plan(Files("template/ci.yml"), Files(), manifest, new CorpusDescriptor(),
            Everything(), CorpusDescriptor.Cautious, readInPlace: false, _ => false, Anonymous);

        Assert.Equal(["jenkins"], plan.Faults.UnknownCi);
        Assert.True(plan.Faults.Unsound);
    }

    [Fact]
    public void A_file_the_manifest_cannot_place_stops_the_run()
    {
        var manifest = new Manifest { Rules = [new ManifestRule([".schema/**"], Manifest.Overlay)] };

        var plan = Update.Plan(Files("stray.txt"), Files(), manifest, new CorpusDescriptor(),
            Everything(), CorpusDescriptor.Cautious, readInPlace: false, _ => false, Anonymous);

        Assert.Equal(["stray.txt"], plan.Faults.Unclassified);
        Assert.True(plan.Faults.Unsound);
    }

    [Fact]
    public void An_overlay_file_the_template_does_not_send_is_reported()
    {
        var plan = Plan(Files(), Files("pages/invented.md"), same: true);

        Assert.Equal(["pages/invented.md"], plan.Unshared);
        Assert.True(plan.Changes);
        Assert.Empty(plan.Written);
    }

    // Only the overlay is the framework's to account for.
    [Fact]
    public void A_seed_the_template_does_not_send_is_left_unreported()
    {
        var plan = Plan(Files(), Files("NOTES.md"), same: true);

        Assert.Empty(plan.Unshared);
        Assert.False(plan.Changes);
    }

    [Fact]
    public void A_file_whose_destination_is_its_source_is_shared_rather_than_copied()
    {
        var plan = Plan(Files(".schema/adrs.yaml"), Files(), readInPlace: true);

        Assert.Empty(plan.Written);
        Assert.False(plan.Changes);
    }

    [Fact]
    public void A_corpus_of_its_own_receives_that_file_as_a_copy()
    {
        var plan = Plan(Files(".schema/adrs.yaml"), Files(), readInPlace: false);

        Assert.Equal([".schema/adrs.yaml"], plan.Written.Select(f => f.To));
    }

    [Fact]
    public void A_relative_template_path_is_resolved_against_the_corpus_and_not_the_working_directory()
    {
        var corpus = Directory.CreateTempSubdirectory().FullName;
        var template = Directory.CreateDirectory(Path.Combine(corpus, "framework")).FullName;

        Assert.Equal(template, Update.TemplatePath("framework", corpus));
    }

    [Theory]
    [InlineData("https://github.com/paul80nd/knowledge-as-code")]
    [InlineData("git@github.com:paul80nd/knowledge-as-code.git")]
    [InlineData("nowhere-on-this-machine")]
    public void Anything_that_is_not_a_folder_is_handed_back_unchanged(string from)
        => Assert.Equal(from, Update.TemplatePath(from, Directory.CreateTempSubdirectory().FullName));

    [Theory]
    [InlineData("/repo", "/repo/example", true)]
    [InlineData("/repo", "/repo", false)] // the corpus is the repository
    [InlineData("/repo", "/other", false)]
    [InlineData("/repo", "/repo-two", false)] // a sibling whose name opens on the same characters
    public void A_corpus_is_inside_its_template_only_where_the_path_says_so(
        string template, string corpus, bool inside)
    {
        var separator = Path.DirectorySeparatorChar;
        Assert.Equal(inside, Update.ReadInPlace(
            template.Replace('/', separator), corpus.Replace('/', separator)));
    }

    private static Schema TypeSchema() => Schema.Load(Repo.Root);

    private static Adoption Adopt(IReadOnlySet<string> corpusFiles, IReadOnlyList<string>? held,
        string? add = null, string? drop = null) =>
        Update.Adopt(corpusFiles, new CorpusDescriptor { Types = held?.ToList() }, TypeSchema(),
            [.. TypeSchema().ByFolder.Keys], add, drop);

    [Fact]
    public void Adopting_a_type_adds_it_to_the_list_in_order()
    {
        var adoption = Adopt(Files(), ["policies"], add: "adrs");

        Assert.Equal(["adrs", "policies"], adoption.Types);
        Assert.Null(adoption.Problem);
        Assert.Empty(adoption.Deleted);
    }

    [Theory]
    [InlineData("nonesuch", null, "declares no type")]
    [InlineData("policies", null, "already adopted")]
    [InlineData(null, "adrs", "not adopted")]
    public void An_adoption_the_corpus_cannot_make_is_refused(string? add, string? drop, string says)
    {
        var adoption = Adopt(Files(), ["policies"], add, drop);

        Assert.Null(adoption.Types);
        Assert.Contains(says, adoption.Problem);
    }

    [Fact]
    public void A_corpus_declaring_no_types_is_told_to_declare_before_changing_the_list()
    {
        var adoption = Adopt(Files(), null, add: "adrs");

        Assert.Contains("no `types:`", adoption.Problem);
    }

    [Fact]
    public void Giving_up_a_type_whose_folder_holds_records_is_refused_with_the_count()
    {
        var corpus = Files("adrs/0001-a-decision.md", "adrs/0002-another.md", "adrs/_template.md");

        var adoption = Adopt(corpus, ["adrs"], drop: "adrs");

        Assert.Null(adoption.Types);
        Assert.Contains("holds 2 record(s)", adoption.Problem);
    }

    [Fact]
    public void Giving_up_an_empty_type_takes_its_schema_page_and_folder()
    {
        var corpus = Files(".schema/adrs.yaml", "adrs.md", "adrs/_template.md", "adrs/_index.md",
            ".schema/policies.yaml", "policies.md");

        var adoption = Adopt(corpus, ["adrs", "policies"], drop: "adrs");

        Assert.Equal(["policies"], adoption.Types);
        Assert.Equal([".schema/adrs.yaml", "adrs.md", "adrs/_index.md", "adrs/_template.md"],
            adoption.Deleted);
    }

    [Fact]
    public void A_run_changing_no_type_answers_with_the_list_as_it_stands()
    {
        var adoption = Adopt(Files(), ["adrs"]);

        Assert.Equal(["adrs"], adoption.Types);
        Assert.Null(adoption.Account);
    }

    [Fact]
    public void The_two_files_a_type_folder_holds_that_are_not_records_are_not_counted()
    {
        var corpus = Files("adrs/_template.md", "adrs/_index.md", "adrs/0001-a-decision.md",
            "adrs/notes.txt", "policies/0001-elsewhere.md");

        Assert.Equal(["adrs/0001-a-decision.md"], Update.RecordsUnder(corpus, "adrs"));
    }
}
