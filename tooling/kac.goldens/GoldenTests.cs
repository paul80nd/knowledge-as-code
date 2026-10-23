using Xunit.Sdk;
using Xunit.v3;
using static kac.goldens.Suite;

[assembly: Parallelization(Mode = ParallelMode.All)]

namespace kac.goldens;

/// <summary>
/// One case per fixture, and one per gate. A gate asks for every scenario's outcome, so it never judges a partial run,
/// and a scenario a gate ran first is not run again for its own case.
/// </summary>
public class GoldenTests
{
    /// <summary>Each fixture, with a <c>Scenario</c> trait to run it alone by.</summary>
    public static IEnumerable<TheoryDataRow<string>> Fixtures() =>
        Scenarios.Select(name => new TheoryDataRow<string>(name).WithTrait("Scenario", name));

    [Theory]
    [MemberData(nameof(Fixtures))]
    public void Scenario(string name)
    {
        var outcome = OutcomeOf(name);
        TestContext.Current.TestOutputHelper?.Write(outcome.Log);

        Assert.False(outcome.Failed, outcome.Log);
    }

    [Fact]
    public void Every_reachable_check_has_a_fixture_and_every_golden_check_is_in_the_catalogue()
    {
        RunAll();

        // `type` fires only for a document in no type folder, and discovery never yields one, so no fixture reaches it.
        var unreachable = new HashSet<string>(["type"], StringComparer.Ordinal);

        var covered = CoveredChecks.Keys.ToHashSet(StringComparer.Ordinal);
        var catalogue = Runner.CheckCatalogue(Kac, LibraryRoot);
        var stale = covered.Where(c => !catalogue.Contains(c)).OrderBy(c => c).ToList();
        var uncovered = catalogue.Where(c => !covered.Contains(c) && !unreachable.Contains(c)).OrderBy(c => c).ToList();
        var unreachableSeen = catalogue.Where(unreachable.Contains).OrderBy(c => c).ToList();

        var report = new List<string> { $"coverage: {covered.Count}/{catalogue.Count} checks exercised by a fixture." };
        if (unreachableSeen.Count > 0)
            report.Add($"  unreachable (no fixture possible): {string.Join(", ", unreachableSeen)}");
        if (uncovered.Count > 0)
            report.Add($"  NOT COVERED, every reachable check needs a fixture: {string.Join(", ", uncovered)}");
        if (stale.Count > 0)
            report.Add($"  STALE (in a golden but kac no longer emits): {string.Join(", ", stale)}");

        TestContext.Current.TestOutputHelper?.WriteLine(string.Join('\n', report));
        Assert.True(uncovered.Count == 0 && stale.Count == 0, string.Join('\n', report));
    }

    [Fact]
    public void Every_verb_is_run_by_a_scenario_or_a_gate()
    {
        // An update skips the runs of a scenario with no golden to write, so it would report their verbs unrun.
        Assert.SkipWhen(Updating, "KAC_UPDATE_GOLDENS=1 skips the runs this gate counts.");

        RunAll();

        // `checks` counts as the coverage gate's, so the gate runs it here too and the tally is the same whatever ran.
        Runner.CheckCatalogue(Kac, LibraryRoot);
        var declared = Runner.DeclaredVerbs(Kac, LibraryRoot);
        var unrun = declared.Where(v => !Invoked.Verbs.ContainsKey(v)).ToList();

        var ran = declared.Count - unrun.Count;
        var report = $"coverage: {ran}/{declared.Count} verbs run by a scenario or a gate.";
        if (unrun.Count > 0) report += $"\n  NOT COVERED, every verb needs a scenario: {string.Join(", ", unrun)}";

        TestContext.Current.TestOutputHelper?.WriteLine(report);
        Assert.True(unrun.Count == 0, report);
    }

    [Fact]
    public void The_checks_table_agrees_with_the_catalogue()
    {
        // `kac checks` verifies its curated rows against the catalogue and exits non-zero on drift.
        var (_, checksErr, checksExit) = Suite.Run(LibraryRoot, "dotnet", Kac, "checks");

        Assert.True(checksExit == 0, $"CHECKS TABLE out of step with the catalogue:\n{Indent(checksErr)}");
    }

    [Fact]
    public void Version_and_help_answer_outside_a_corpus_and_a_verb_exits_2()
    {
        // A corpus lookup running before the parse passes every other test here.
        var nowhere = Directory.CreateTempSubdirectory("kac-tests-nowhere-").FullName;
        var problems = new List<string>();
        try
        {
            foreach (var flag in new[] { "--version", "--help" })
            {
                var (flagOut, _, flagExit) = Suite.Run(nowhere, "dotnet", Kac, flag);
                if (flagExit == 0 && flagOut.Trim().Length > 0) continue;
                problems.Add($"  {flag} outside a corpus: exit {flagExit}, printed {flagOut.Trim().Length} char(s)");
            }

            var (_, verbErr, verbExit) = Suite.Run(nowhere, "dotnet", Kac, "validate");
            if (verbExit != 2)
                problems.Add($"  validate outside a corpus: exit {verbExit}, expected 2\n{Indent(verbErr)}");
        }
        finally
        {
            Runner.TryDelete(nowhere);
        }

        Assert.True(problems.Count == 0, string.Join('\n', problems));
    }
}
