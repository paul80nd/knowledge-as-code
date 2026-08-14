using System.Text.Json;

// ---------------------------------------------------------------------------
// Subcommands — the orchestration behind each CLI verb. The entrypoint (.tooling/kac.cs) only wires
// System.CommandLine to these; all the work lives here and in the rest of kac.core.
// ---------------------------------------------------------------------------

namespace kac.core;

public static class Commands
{
    public static int Validate(string repoRoot, List<string> paths, bool json)
    {
        var corpus = Corpus.Load(repoRoot, paths);
        var findings = Validator.CheckAll(corpus);
        return Report(findings, corpus.Docs.Count, corpus.Templates.Count, corpus.SkippedNoFrontmatter, json);
    }

    public static int Index(string repoRoot, bool check)
    {
        var corpus = Corpus.Load(repoRoot, []);

        // What every generated file should hold. `GeneratedFiles` owns which files those are and which
        // blocks each carries, so that `validate` holds a corpus to the same list this writes.
        var targets = GeneratedFiles.Targets(corpus);

        if (check)
        {
            var stale = targets.Where(x => !File.Exists(x.Path) || Files.ReadLf(x.Path) != x.Content)
                .Select(x => Path.GetRelativePath(repoRoot, x.Path).Replace('\\', '/'))
                .ToList();
            if (stale.Count == 0)
            {
                Console.WriteLine($"index is up to date ({targets.Count} generated file(s)).");
                return 0;
            }

            Console.Error.WriteLine(
                "index is stale — the following generated files differ from the schema/frontmatter:");
            foreach (var s in stale) Console.Error.WriteLine($"  {s}");
            Console.Error.WriteLine("run:  dotnet run .tooling/kac.cs -- index");
            return 1;
        }

        ReportWritten(repoRoot, GeneratedFiles.Write(targets));
        return 0;
    }

    // What a regeneration wrote. Shared with `mechanism --sync`, which ends by regenerating, so that a
    // sync reports the files it rebuilt in the words `index` uses for the same work.
    private static void ReportWritten(string repoRoot, List<string> written)
    {
        foreach (var path in written)
            Console.WriteLine($"wrote {Path.GetRelativePath(repoRoot, path).Replace('\\', '/')}");

        Console.WriteLine(written.Count == 0
            ? "index already up to date; nothing written."
            : $"index updated {written.Count} file(s).");
    }

    private static int Report(List<Finding> findings, int validated, int templates, int skipped, bool json)
    {
        var errors = findings.Count(f => f.Severity == Sev.Error);
        var warnings = findings.Count(f => f.Severity == Sev.Warning);

        if (json)
        {
            // Through the source generator rather than reflection — see Json.cs.
            var report = new ValidateReport(
                new ValidateSummary(validated, templates, skipped, errors, warnings),
                [
                    .. findings
                        .OrderBy(f => f.File).ThenBy(f => f.Line ?? 0)
                        .Select(f => new ValidateFinding(
                            f.File, f.Line, f.Severity.ToString().ToLowerInvariant(), f.Check.Value, f.Message))
                ]);

            Console.WriteLine(JsonSerializer.Serialize(report, KacJson.Relaxed.ValidateReport));
            return errors > 0 ? 1 : 0;
        }

        foreach (var grp in findings.GroupBy(f => f.File).OrderBy(g => g.Key))
        {
            Console.WriteLine(grp.Key);
            foreach (var f in grp.OrderBy(f => f.Line ?? 0))
            {
                var tag = f.Severity == Sev.Error ? "error  " : "warning";
                var at = f.Line is { } ln ? $":{ln}" : "";
                Console.WriteLine($"  {tag}  [{f.Check}] {f.Message}{(at.Length > 0 ? $"  ({grp.Key}{at})" : "")}");
            }

            Console.WriteLine();
        }

        // Templates are counted apart from documents because they are checked apart from them: a reader
        // who sees a finding against a `_template.md` should find it accounted for in the tally, and one
        // who sees none should be able to tell that the templates were read rather than skipped.
        Console.WriteLine(
            $"validated {validated} document(s) and {templates} template(s), skipped {skipped} without "
            + $"frontmatter — {errors} error(s), {warnings} warning(s)");
        return errors > 0 ? 1 : 0;
    }

    public static int Checks(string repoRoot, bool json)
    {
        var schema = Schema.Load(repoRoot);
        var catalogue = CheckCatalogue.For(schema);

        // The catalogue is always valid data, so emit it either way; the reader-facing table's
        // fidelity to it is a separate signal, reported to stderr and via the exit code below. This
        // is the tie the test suite relies on: a new catalogue check with no table row (and no
        // explicit waiver), or a row naming a check that no longer exists, exits non-zero here.
        if (json)
        {
            var report = new ChecksReport(
            [
                .. catalogue.Select(c =>
                    new CheckInfo(c.Id.Value, c.Severity.ToString().ToLowerInvariant(), c.Summary))
            ]);
            Console.WriteLine(JsonSerializer.Serialize(report, KacJson.Relaxed.ChecksReport));
        }
        else
        {
            foreach (var c in catalogue)
            {
                var tag = c.Severity == Sev.Error ? "error  " : "warning";
                Console.WriteLine($"  {tag}  {c.Id,-24}  {c.Summary}");
            }

            Console.WriteLine();
            Console.WriteLine($"{catalogue.Count} checks.");
        }

        var problems = Generator.ChecksTableProblems(schema);
        if (problems.Count == 0) return 0;

        Console.Error.WriteLine("checks: the reader-facing checks table is out of step with the catalogue:");
        foreach (var p in problems) Console.Error.WriteLine($"  {p}");
        Console.Error.WriteLine(
            "fix Generator.DocRows (or IntentionallyUndocumented) in .tooling/kac.core/Generator.cs.");
        return 1;
    }

    public static int Mechanism(string repoRoot, bool check, bool sync, string? against)
    {
        if (check == sync)
            return Fail(check
                ? "mechanism: --check and --sync are the two halves of this command; ask for one."
                : "mechanism: specify --check to compare against a reference, or --sync to take from one.");

        var descriptor = CorpusDescriptor.Load(repoRoot);

        // A sync needs a declared upstream, and not just a directory it can read. `--against` says which
        // copy of the upstream to take from — a local checkout rather than the URL. `upstream.url` says
        // the corpus takes from an upstream at all. The corpus at the head of the chain names none:
        // changes leave it and none arrive, so a sync has nowhere to run from.
        if (sync && descriptor.UpstreamUrl is null)
            return Fail("mechanism: this corpus names no upstream, so there is nothing to sync from. "
                        + "A corpus that takes from another records it in upstream.url in .corpus.yaml.");

        var reference = against ?? descriptor.UpstreamUrl;
        if (string.IsNullOrWhiteSpace(reference))
            return Fail("mechanism: no reference to compare against. Pass --against <path>, "
                        + "or set upstream.url in .corpus.yaml.");

        var refRoot = Path.GetFullPath(reference, repoRoot);
        if (!Directory.Exists(refRoot))
            return Fail($"mechanism: reference corpus not found: {refRoot}");
        if (Path.GetFullPath(refRoot) == Path.GetFullPath(repoRoot))
            return Fail("mechanism: the reference is this corpus itself — nothing to compare.");

        // Whether two copies of a file say the same thing, which is the one question either engine asks of
        // the disk. Passed in, so each engine decides from listings and a predicate rather than from a tree.
        bool Same(string rel) => MechanismCheck.Same(repoRoot, refRoot, rel);

        var localFiles = MechanismCheck.ListFiles(repoRoot);
        var refFiles = MechanismCheck.ListFiles(refRoot);

        // Check reads this corpus's manifest, because it reports whether this corpus is in step with the
        // boundary it believes in. Sync reads the reference's, because it takes that boundary down along
        // with the files the boundary describes.
        if (check)
            return ReportMechanism(
                MechanismCheck.Classify(localFiles, refFiles, Manifest.Load(repoRoot), descriptor, Same),
                refRoot);

        var manifest = Manifest.Load(refRoot);
        var plan = MechanismSync.Plan(localFiles, refFiles, manifest, descriptor,
            MechanismSync.DeclinedTypePaths(refRoot, descriptor), Same);

        var today = DateTime.Today.ToString("yyyy-MM-dd");
        MechanismSync.Apply(plan, repoRoot, refRoot, manifest, reference, today);
        return ReportSync(plan, repoRoot, manifest.Version, reference, today);

        static int Fail(string message)
        {
            Console.Error.WriteLine(message);
            return 1;
        }
    }

    private static int ReportMechanism(MechanismReport report, string refRoot)
    {
        Console.WriteLine($"mechanism: comparing the synced layer against {refRoot}");
        Section("DRIFT — synced files differ from the reference", report.Drift);
        Section("MISSING LOCALLY — synced files in the reference but not here", report.MissingLocally);
        Section("MISSING UPSTREAM — synced files here but not in the reference", report.MissingUpstream);
        Section("UNCLASSIFIED — files matching no manifest rule", report.Unclassified);

        if (report.ResolvedDivergence.Count > 0)
        {
            Console.WriteLine("RESOLVED — accepted divergences that are now identical again (delete them from .corpus.yaml):");
            foreach (var p in report.ResolvedDivergence) Console.WriteLine($"  {p}");
        }

        Console.WriteLine(
            $"synced: {report.SyncedInStep} in step, {report.Drift.Count} drifted; "
            + $"forked: {report.ForkedShared} shared ({report.ForkedDiffer} differ, informational); "
            + $"accepted divergences: {report.AcceptedActive}.");

        // Held but not asked for: schema files for types this corpus did not adopt, or a fixture tree in
        // a corpus whose role declines the verification layer. Neither is drift, because nothing was
        // compared. Say so anyway — no sync will refresh these files, and the alternative is leaving the
        // reader to find them stale later.
        if (report.DeclinedButHeld > 0)
            Console.WriteLine(
                $"declined: {report.DeclinedButHeld} file(s) held here that this corpus's descriptor does not ask for. "
                + "They are not synced or compared; delete them, or adopt what they belong to.");

        if (report.Problems > 0)
        {
            Console.Error.WriteLine($"mechanism check failed — {report.Problems} synced-layer problem(s) above.");
            return 1;
        }

        Console.WriteLine("mechanism: synced layer in step.");
        return 0;

        static void Section(string heading, IReadOnlyList<string> paths)
        {
            if (paths.Count == 0) return;
            Console.Error.WriteLine($"{heading}:");
            foreach (var p in paths) Console.Error.WriteLine($"  {p}");
        }
    }

    private static int ReportSync(SyncPlan plan, string repoRoot, int mechanismVersion, string reference,
        string today)
    {
        Console.WriteLine($"mechanism: syncing the shared layers from {reference}");
        Section("UPDATED — brought down from the reference", plan.Updated);
        Section("SEEDED — the corpus's own from here on, copied because it had none", plan.Seeded);
        Section("SKIPPED — accepted divergences, left as they are", plan.Skipped);
        Section("HELD HERE, NOT UPSTREAM — shared files the reference does not have (sync never deletes)",
            plan.HeldHere);

        Console.WriteLine(
            $"synced: {plan.Updated.Count} updated, {plan.InStep} already in step; seeded {plan.Seeded.Count}; "
            + $"skipped {plan.Skipped.Count}; declined {plan.Declined}. "
            + $"Recorded in .corpus.yaml as mechanism version {mechanismVersion}, taken {today}.");

        if (plan.ReferenceIsUnsound)
        {
            Console.Error.WriteLine("UNCLASSIFIED — files in the reference matching no manifest rule, so not copied:");
            foreach (var p in plan.Unclassified) Console.Error.WriteLine($"  {p}");
            Console.Error.WriteLine("mechanism sync: the reference's manifest does not resolve its own tree — fix it there.");
            return 1;
        }

        // Every synced page may carry a generated block built from this corpus's own types, so the copies
        // above are only right once rebuilt against what this corpus holds. Regenerating here makes a
        // passing `index --check` sync's postcondition instead of the reader's next surprise.
        return Regenerate(repoRoot);

        static void Section(string heading, IReadOnlyList<string> paths)
        {
            if (paths.Count == 0) return;
            Console.WriteLine($"{heading}:");
            foreach (var p in paths) Console.WriteLine($"  {p}");
        }
    }

    // The corpus is loaded here rather than by the caller because a sync has just replaced the schema, and
    // a schema this corpus cannot yet read is the one failure worth surviving: the files are already in
    // place, and saying so is more use than a stack trace over a half-finished tree.
    private static int Regenerate(string repoRoot)
    {
        try
        {
            ReportWritten(repoRoot, GeneratedFiles.Write(GeneratedFiles.Targets(Corpus.Load(repoRoot, []))));
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"mechanism sync: regeneration failed — {ex.Message}");
            Console.Error.WriteLine(
                "mechanism sync: the files are in place but the generated blocks were not rebuilt. "
                + "Run ./kac validate to see what the corpus is missing, then ./kac index.");
            return 1;
        }
    }
}
