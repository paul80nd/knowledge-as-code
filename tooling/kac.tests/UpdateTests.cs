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
            new ManifestRule(["template/ci.yml"], Manifest.Seed, "", "github"),
            new ManifestRule(["old/**"], Manifest.Removed, "legacy/"),
            new ManifestRule(["**"], Manifest.Withheld)
        ]
    };

    private static HashSet<string> Files(params string[] paths) => new(paths, StringComparer.Ordinal);

    // Every type adopted, so nothing is declined unless a case says so.
    private static UpdateTypes Everything(params string[] declared) =>
        new(declared, declared, _ => false);

    private static UpdatePlan Plan(IReadOnlySet<string> template, IReadOnlySet<string> corpus,
        CorpusDescriptor? descriptor = null, UpdateTypes? types = null,
        string policy = CorpusDescriptor.Cautious, bool readInPlace = false, bool same = false) =>
        Update.Plan(template, corpus, Rules(), descriptor ?? new CorpusDescriptor(),
            types ?? Everything(), policy, readInPlace, _ => same);

    // -- the overlay layer --

    // Framework property. An edit to one is drift rather than a customisation, and the next update takes
    // it back.
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

    // `to:` sends a file somewhere other than where it sat upstream, and everything after the plan reads
    // the destination. So a relocated file is compared and written where it lands, not where it was read.
    [Fact]
    public void A_relocated_file_is_planned_against_where_it_lands()
    {
        var plan = Plan(Files("template/pages/taxonomy.md"), Files());

        var file = Assert.Single(plan.Written);
        Assert.Equal("template/pages/taxonomy.md", file.From);
        Assert.Equal("pages/taxonomy.md", file.To);
    }

    // -- the seed layer --

    // A seed is the corpus's own words from the moment it lands. Refreshing every one of them would open
    // each update with three dozen files to revert by hand.
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

    // `full` hands the reconciliation to the diff, and holds a seed to the template like any other file.
    [Fact]
    public void A_seed_the_corpus_holds_is_refreshed_under_full()
    {
        var plan = Plan(Files("template/CLAUDE.md"), Files("CLAUDE.md"), policy: CorpusDescriptor.Full);

        Assert.Equal(["CLAUDE.md"], plan.Seeded.Select(f => f.To));
    }

    // Even under `full`, a seed that already matches is not rewritten. The policy widens what is compared
    // and never what is copied.
    [Fact]
    public void A_seed_that_already_matches_is_left_alone_under_full()
    {
        var plan = Plan(Files("template/CLAUDE.md"), Files("CLAUDE.md"), policy: CorpusDescriptor.Full,
            same: true);

        Assert.Empty(plan.Seeded);
        Assert.Equal(1, plan.InStep);
    }

    // -- the removed layer --

    // A tombstone names a file the template no longer holds, so it is matched against the corpus rather
    // than against the template. Nothing else would ever reach it.
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

    // -- the withheld layer --

    [Fact]
    public void A_withheld_file_reaches_no_corpus()
    {
        var plan = Plan(Files("tooling/kac/Program.cs"), Files());

        Assert.Empty(plan.Written);
        Assert.Empty(plan.Seeded);
        Assert.False(plan.Changes);
    }

    // -- what the corpus takes back --

    // The one way to say "I own this" about a file the overlay would otherwise reclaim on every run.
    [Fact]
    public void A_skipped_path_is_neither_read_nor_written()
    {
        var descriptor = new CorpusDescriptor();
        descriptor.Skipped.Add(new SkippedFile(".schema/adrs.yaml", "Patched for our proxy."));

        var plan = Plan(Files(".schema/adrs.yaml"), Files(".schema/adrs.yaml"), descriptor);

        Assert.Empty(plan.Written);
        Assert.Contains("Patched for our proxy.", Assert.Single(plan.Skipped));
    }

    // A tombstone does not reach a file the corpus has claimed either. `skip:` is stated in both
    // directions, and a deletion is the direction that cannot be undone by hand.
    [Fact]
    public void A_skipped_path_is_not_deleted_by_a_tombstone()
    {
        var descriptor = new CorpusDescriptor();
        descriptor.Skipped.Add(new SkippedFile("legacy/gone.md", "We still run this."));

        var plan = Plan(Files(), Files("legacy/gone.md"), descriptor);

        Assert.Empty(plan.Deleted);
        Assert.Single(plan.Skipped);
    }

    // -- a type the corpus did not adopt --

    [Fact]
    public void A_declined_types_files_are_withheld_and_counted()
    {
        var types = new UpdateTypes(["adrs", "runbooks"], ["adrs"],
            rel => rel.Equals(".schema/runbooks.yaml", StringComparison.Ordinal));

        var plan = Plan(Files(".schema/adrs.yaml", ".schema/runbooks.yaml"), Files(), types: types);

        Assert.Equal([".schema/adrs.yaml"], plan.Written.Select(f => f.To));
        Assert.Equal(1, plan.Declined);
    }

    // Reported, never adopted. Silence would hide a type the corpus could take, and adoption would put
    // folders in a corpus nobody asked for.
    [Fact]
    public void A_type_the_template_declares_and_the_corpus_has_not_adopted_is_offered()
    {
        var types = new UpdateTypes(["adrs", "runbooks"], ["adrs"], _ => false);

        Assert.Equal(["runbooks"], Plan(Files(), Files(), types: types).Offered);
    }

    // A corpus that has declared nothing is held to everything, as it is everywhere else the descriptor
    // is read. So there is nothing to offer it.
    [Fact]
    public void A_corpus_declaring_no_types_is_offered_none()
    {
        var types = new UpdateTypes(["adrs", "runbooks"], null, _ => false);

        Assert.Empty(Plan(Files(), Files(), types: types).Offered);
    }

    // -- a starter for a continuous integration system --

    // Which system builds a corpus is a fact about that repository. An update introducing a workflow
    // would hand it one that runs uninvited.
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
            Everything(), CorpusDescriptor.Cautious, readInPlace: false, _ => false);

        Assert.Equal(["jenkins"], plan.UnknownCi);
        Assert.True(plan.TemplateIsUnsound);
    }

    // -- a template this tool cannot read --

    [Fact]
    public void A_file_the_manifest_cannot_place_stops_the_run()
    {
        var manifest = new Manifest { Rules = [new ManifestRule([".schema/**"], Manifest.Overlay)] };

        var plan = Update.Plan(Files("stray.txt"), Files(), manifest, new CorpusDescriptor(),
            Everything(), CorpusDescriptor.Cautious, readInPlace: false, _ => false);

        Assert.Equal(["stray.txt"], plan.Unclassified);
        Assert.True(plan.TemplateIsUnsound);
    }

    // -- the other direction --

    // A framework change written in the wrong tree. It reaches no other corpus, and nothing in this one
    // reads as though anything is missing, so the check is the only place it surfaces.
    [Fact]
    public void An_overlay_file_the_template_does_not_send_is_reported()
    {
        var plan = Plan(Files(), Files("pages/invented.md"), same: true);

        Assert.Equal(["pages/invented.md"], plan.Unshared);
        Assert.True(plan.Changes);
        Assert.Empty(plan.Written);
    }

    // A seed the template stopped sending belongs to the corpus, so the corpus keeps it and hears
    // nothing. Only the overlay is the framework's to account for.
    [Fact]
    public void A_seed_the_template_does_not_send_is_left_unreported()
    {
        var plan = Plan(Files(), Files("NOTES.md"), same: true);

        Assert.Empty(plan.Unshared);
        Assert.False(plan.Changes);
    }

    // -- a corpus inside the repository serving its template --

    // `.schema/` and the travelling skills are authored once at this repository's root and read from
    // there by both corpora below it. A file whose destination is its source is that arrangement, and
    // there is no copy to compare or to write.
    [Fact]
    public void A_file_whose_destination_is_its_source_is_shared_rather_than_copied()
    {
        var plan = Plan(Files(".schema/adrs.yaml"), Files(), readInPlace: true);

        Assert.Empty(plan.Written);
        Assert.False(plan.Changes);
    }

    // And only where the corpus sits inside that repository. Everywhere else `.schema/` is a copy the
    // corpus receives, which is the ordinary case.
    [Fact]
    public void A_corpus_of_its_own_receives_that_file_as_a_copy()
    {
        var plan = Plan(Files(".schema/adrs.yaml"), Files(), readInPlace: false);

        Assert.Equal([".schema/adrs.yaml"], plan.Written.Select(f => f.To));
    }

    [Theory]
    [InlineData("/repo", "/repo/example", true)]
    [InlineData("/repo", "/repo", false)]   // the corpus is the repository
    [InlineData("/repo", "/other", false)]
    [InlineData("/repo", "/repo-two", false)] // a sibling whose name opens on the same characters
    public void A_corpus_is_inside_its_template_only_where_the_path_says_so(
        string template, string corpus, bool inside)
    {
        var separator = Path.DirectorySeparatorChar;
        Assert.Equal(inside, Update.ReadInPlace(
            template.Replace('/', separator), corpus.Replace('/', separator)));
    }

    // -- adopting and giving up a type --

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

    // Reading adoption off the folders is what a corpus that has declared nothing gets, and there is no
    // list there to change.
    [Fact]
    public void A_corpus_declaring_no_types_is_told_to_declare_before_changing_the_list()
    {
        var adoption = Adopt(Files(), null, add: "adrs");

        Assert.Contains("no `types:`", adoption.Problem);
    }

    // Everything else in a corpus exists to serve its records, so the records are the one thing a drop
    // will not take with it.
    [Fact]
    public void Giving_up_a_type_whose_folder_holds_records_is_refused_with_the_count()
    {
        var corpus = Files("adrs/0001-a-decision.md", "adrs/0002-another.md", "adrs/_template.md");

        var adoption = Adopt(corpus, ["adrs"], drop: "adrs");

        Assert.Null(adoption.Types);
        Assert.Contains("holds 2 record(s)", adoption.Problem);
    }

    // With no record to lose, a drop is the inverse of an adoption: the schema, the root page and the
    // folder all go, and `types:` stops naming it.
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

    // A run naming neither flag answers with the list the corpus already holds, so the caller reads one
    // list whether or not it asked for a change.
    [Fact]
    public void A_run_changing_no_type_answers_with_the_list_as_it_stands()
    {
        var adoption = Adopt(Files(), ["adrs"]);

        Assert.Equal(["adrs"], adoption.Types);
        Assert.Null(adoption.Account);
    }

    // -- what a type folder counts as a record --

    [Fact]
    public void The_two_files_a_type_folder_holds_that_are_not_records_are_not_counted()
    {
        var corpus = Files("adrs/_template.md", "adrs/_index.md", "adrs/0001-a-decision.md",
            "adrs/notes.txt", "policies/0001-elsewhere.md");

        Assert.Equal(["adrs/0001-a-decision.md"], Update.RecordsUnder(corpus, "adrs"));
    }
}
