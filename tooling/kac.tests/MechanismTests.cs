using kac.core;

// In-process unit tests for the two mechanism engines.
//
// Both decide from file listings and a predicate rather than from a filesystem, so each arm of the
// classification is a case here rather than a tree on disk. The golden `mechanism` and `sync` scenarios
// prove the answers reach the CLI and that a sync leaves the tree it said it would; what they cannot do
// is put one file in one state and assert where it lands, because a fixture corpus states every file at
// once and reports the total.

namespace kac.tests;

public class MechanismTests
{
    // A manifest with no catch-all, so a path none of the rules names is unclassified. That is the
    // state the real manifest's final `**` rule exists to make impossible.
    private static Manifest Rules() => new()
    {
        Version = 3,
        Rules =
        [
            new ManifestRule(["tooling/kac.tests/**"], "verification"),
            new ManifestRule(["tooling/**", ".schema/**"], "synced"),
            new ManifestRule(["**/*.md"], "forked"),
            new ManifestRule(["notes/**"], "local")
        ]
    };

    private static CorpusDescriptor Descriptor(string role = "source", List<string>? types = null,
        params (string Path, string? Reason)[] accepted)
    {
        var descriptor = new CorpusDescriptor { Role = role, Types = types };
        foreach (var (path, reason) in accepted) descriptor.Skipped.Add(new SkippedFile(path, reason));
        return descriptor;
    }

    private static HashSet<string> Files(params string[] paths) => new(paths, StringComparer.Ordinal);

    private static readonly Func<string, bool> Identical = _ => true;
    private static readonly Func<string, bool> Differ = _ => false;

    // -- MechanismCheck.Classify --

    [Fact]
    public void A_shared_file_whose_copies_differ_is_drift()
    {
        var report = MechanismCheck.Classify(Files("tooling/kac/Program.cs"), Files("tooling/kac/Program.cs"),
            Rules(), Descriptor(), Differ);

        Assert.Equal(["tooling/kac/Program.cs"], report.Drift);
        Assert.Equal(0, report.SyncedInStep);
        Assert.Equal(1, report.Problems);
    }

    [Fact]
    public void A_shared_file_whose_copies_match_is_counted_and_not_named()
    {
        var report = MechanismCheck.Classify(Files("tooling/kac/Program.cs"), Files("tooling/kac/Program.cs"),
            Rules(), Descriptor(), Identical);

        Assert.Empty(report.Drift);
        Assert.Equal(1, report.SyncedInStep);
        Assert.Equal(0, report.Problems);
    }

    [Fact]
    public void A_shared_file_only_upstream_is_missing_locally()
    {
        var report = MechanismCheck.Classify(Files(), Files("tooling/kac/Program.cs"), Rules(), Descriptor(), Differ);

        Assert.Equal(["tooling/kac/Program.cs"], report.MissingLocally);
        Assert.Empty(report.MissingUpstream);
    }

    [Fact]
    public void A_shared_file_only_here_is_missing_upstream()
    {
        var report = MechanismCheck.Classify(Files("tooling/kac/Program.cs"), Files(), Rules(), Descriptor(), Differ);

        Assert.Equal(["tooling/kac/Program.cs"], report.MissingUpstream);
        Assert.Empty(report.MissingLocally);
    }

    [Fact]
    public void A_file_no_rule_places_is_unclassified()
    {
        var report = MechanismCheck.Classify(Files("stray.txt"), Files("stray.txt"), Rules(), Descriptor(), Identical);

        Assert.Equal(["stray.txt"], report.Unclassified);
        Assert.Equal(1, report.Problems);
    }

    // An accepted divergence is a decision the corpus recorded, so it is never drift. The two states it
    // can be in are told apart: still divergent is a count, identical again is a line asking for the
    // entry to be deleted.
    [Fact]
    public void An_accepted_divergence_that_still_differs_is_counted_rather_than_reported()
    {
        var report = MechanismCheck.Classify(Files("tooling/kac/Program.cs"), Files("tooling/kac/Program.cs"),
            Rules(), Descriptor(accepted: ("tooling/kac/Program.cs", "pinned to .NET 8")), Differ);

        Assert.Empty(report.Drift);
        Assert.Empty(report.ResolvedDivergence);
        Assert.Equal(1, report.AcceptedActive);
        Assert.Equal(0, report.Problems);
    }

    [Fact]
    public void An_accepted_divergence_that_matches_again_is_reported_as_resolved()
    {
        var report = MechanismCheck.Classify(Files("tooling/kac/Program.cs"), Files("tooling/kac/Program.cs"),
            Rules(), Descriptor(accepted: ("tooling/kac/Program.cs", null)), Identical);

        Assert.Equal(["tooling/kac/Program.cs"], report.ResolvedDivergence);
        Assert.Equal(0, report.AcceptedActive);

        // Reported, and not a fault: deleting the entry is the corpus's to do when it likes.
        Assert.Equal(0, report.Problems);
    }

    [Fact]
    public void A_forked_file_is_counted_and_never_drift()
    {
        var report = MechanismCheck.Classify(Files("adrs.md"), Files("adrs.md"), Rules(), Descriptor(), Differ);

        Assert.Empty(report.Drift);
        Assert.Equal(1, report.ForkedShared);
        Assert.Equal(1, report.ForkedDiffer);
        Assert.Equal(0, report.Problems);
    }

    [Fact]
    public void A_consumer_declines_the_verification_layer_and_is_told_what_it_still_holds()
    {
        var files = Files("tooling/kac.tests/x.cs");

        Assert.Equal(1, MechanismCheck.Classify(files, files, Rules(), Descriptor("consumer"), Differ).DeclinedButHeld);
        Assert.Empty(MechanismCheck.Classify(files, files, Rules(), Descriptor("consumer"), Differ).Drift);

        // A source answers for the tool, so the same file is compared like any other shared file.
        Assert.Equal(["tooling/kac.tests/x.cs"],
            MechanismCheck.Classify(files, files, Rules(), Descriptor(), Differ).Drift);
    }

    [Fact]
    public void A_type_the_corpus_did_not_adopt_takes_its_schema_file_with_it()
    {
        var files = Files(".schema/adrs.yaml", ".schema/runbooks.yaml");
        var report = MechanismCheck.Classify(files, files, Rules(), Descriptor(types: ["adrs"]), Differ);

        Assert.Equal([".schema/adrs.yaml"], report.Drift);
        Assert.Equal(1, report.DeclinedButHeld);
    }

    // -- MechanismSync.Plan --

    private static readonly DeclinedPaths NothingDeclined = new([], []);

    private static SyncPlan Plan(HashSet<string> local, HashSet<string> reference, CorpusDescriptor? descriptor = null,
        Func<string, bool>? same = null, DeclinedPaths? declined = null) =>
        MechanismSync.Plan(local, reference, Rules(), descriptor ?? Descriptor(), declined ?? NothingDeclined,
            same ?? Differ);

    [Fact]
    public void A_shared_file_whose_copies_differ_is_updated()
    {
        var plan = Plan(Files("tooling/kac/Program.cs"), Files("tooling/kac/Program.cs"));

        Assert.Equal(["tooling/kac/Program.cs"], plan.Updated);
        Assert.Equal(0, plan.InStep);
    }

    [Fact]
    public void A_shared_file_whose_copies_match_is_already_in_step()
    {
        var plan = Plan(Files("tooling/kac/Program.cs"), Files("tooling/kac/Program.cs"), same: Identical);

        Assert.Empty(plan.Updated);
        Assert.Equal(1, plan.InStep);
    }

    [Fact]
    public void A_shared_file_the_corpus_lacks_is_brought_down()
    {
        var plan = Plan(Files(), Files("tooling/kac/Program.cs"));

        Assert.Equal(["tooling/kac/Program.cs"], plan.Updated);
    }

    // A forked file is seeded once and never reconciled afterwards, so the two cases are opposite: absent
    // means copy it, present means leave it alone however far it has since travelled.
    [Fact]
    public void A_forked_file_the_corpus_lacks_is_seeded()
    {
        var plan = Plan(Files(), Files("adrs.md"));

        Assert.Equal(["adrs.md"], plan.Seeded);
        Assert.Empty(plan.Updated);
    }

    [Fact]
    public void A_forked_file_the_corpus_already_has_is_left_alone()
    {
        var plan = Plan(Files("adrs.md"), Files("adrs.md"));

        Assert.Empty(plan.Seeded);
        Assert.Empty(plan.Updated);
        Assert.Equal(0, plan.InStep);
    }

    [Fact]
    public void An_accepted_divergence_is_skipped_and_names_its_reason()
    {
        var plan = Plan(Files("tooling/kac/Program.cs"), Files("tooling/kac/Program.cs"),
            Descriptor(accepted: ("tooling/kac/Program.cs", "pinned to .NET 8")));

        Assert.Equal(["tooling/kac/Program.cs  (pinned to .NET 8)"], plan.Skipped);
        Assert.Empty(plan.Updated);
    }

    // Sync copies and never deletes, so a shared file the reference has lost is named and kept.
    [Fact]
    public void A_shared_file_the_reference_does_not_have_is_held_here()
    {
        var plan = Plan(Files("tooling/kac/Program.cs"), Files());

        Assert.Equal(["tooling/kac/Program.cs"], plan.HeldHere);
        Assert.Empty(plan.Copies);
    }

    [Fact]
    public void A_forked_file_the_reference_does_not_have_is_the_corpus_own_business()
    {
        var plan = Plan(Files("adrs.md"), Files());

        Assert.Empty(plan.HeldHere);
        Assert.Empty(plan.Copies);
    }

    // Sync answers for the reference's tree alone. A local file the manifest cannot place is `--check`'s
    // to report, and failing the sync over it would block a corpus from taking a fix for its own manifest.
    [Fact]
    public void An_unplaceable_file_is_syncs_business_only_when_the_reference_holds_it()
    {
        Assert.True(Plan(Files(), Files("stray.txt")).ReferenceIsUnsound);
        Assert.False(Plan(Files("stray.txt"), Files()).ReferenceIsUnsound);
    }

    [Fact]
    public void A_declined_type_is_counted_and_nothing_of_it_is_copied()
    {
        var plan = Plan(Files(), Files("runbooks.md", "runbooks/0001-x.md", ".schema/runbooks.yaml"),
            Descriptor(types: ["adrs"]),
            declined: new DeclinedPaths(new HashSet<string>(["runbooks.md"], StringComparer.Ordinal), ["runbooks/"]));

        Assert.Empty(plan.Copies);
        Assert.Equal(3, plan.Declined);
    }

    // What a sync touches is stated once. Anything reading the plan for the files to copy reads the two
    // lists it already reports, so no third list can drift out of step with them.
    [Fact]
    public void The_files_to_copy_are_exactly_the_updated_and_the_seeded()
    {
        var plan = Plan(Files("tooling/old.cs"), Files("tooling/kac/Program.cs", "adrs.md"));

        Assert.Equal(["tooling/kac/Program.cs"], plan.Updated);
        Assert.Equal(["adrs.md"], plan.Seeded);
        Assert.Equal(["tooling/kac/Program.cs", "adrs.md"], plan.Copies);
        Assert.Equal(["tooling/old.cs"], plan.HeldHere);
    }
}
