using System.Text.Json;

// ---------------------------------------------------------------------------
// Subcommands — the orchestration behind each CLI verb. The entrypoint (.tooling/kac.cs) only wires
// System.CommandLine to these; all the work lives here and in the rest of kac.core.
// ---------------------------------------------------------------------------

namespace kac.core;

public static class Commands
{
    public static int Validate(string repoRoot, bool json)
    {
        var corpus = Corpus.Load(repoRoot);
        var findings = Validator.CheckAll(corpus);
        return Report(findings, corpus.Docs.Count, corpus.Templates.Count, corpus.SkippedNoFrontmatter, json);
    }

    // Build the export. The whole corpus is loaded whatever `type` names, because a narrowed load
    // answers questions about the set from some of its members and answers them wrongly; `type` narrows
    // what is written.
    public static int Export(string repoRoot, string? type)
    {
        var corpus = Corpus.Load(repoRoot);

        if (type is not null && corpus.Adopted.All(t => t.Key != type))
            return Fail($"export: this corpus has not adopted a type called '{type}'. "
                        + $"It holds {string.Join(", ", corpus.Adopted.Select(t => t.Key))}.");

        var unknown = corpus.Descriptor.ExportExclude
            .Where(e => !CorpusDescriptor.Excludable.Contains(e, StringComparer.Ordinal)).ToList();
        if (unknown.Count > 0)
            return Fail($"export: .corpus.yaml excludes {string.Join(", ", unknown)}, which an export cannot "
                        + $"act on. It excludes {string.Join(" or ", CorpusDescriptor.Excludable)}.");

        var commit = Git.Head(repoRoot);
        var dirty = Git.Dirty(repoRoot);
        var publishing = Publishing.For(corpus.Descriptor, commit);
        var now = DateTime.UtcNow;

        var plan = Exporter.Plan(corpus, publishing, type,
            new ExportRun(now.ToString("yyyy-MM-ddTHH:mm:ssZ"), DateOnly.FromDateTime(now), commit, dirty));

        var written = Exporter.Write(repoRoot, plan);
        foreach (var path in written) Console.WriteLine($"wrote {path}");

        // What the export cannot say is worth saying here, where someone is watching. Neither state is
        // an error: a corpus may publish nowhere, and an export built from a dirty tree is still an
        // export — it is only one that cannot be rebuilt from the commit it names.
        if (publishing is null)
            Console.WriteLine(
                "export: no published links — this corpus states no publishing target the tool can address, "
                + "or no bases for one. Records carry their paths and no URLs.");
        if (dirty is true)
            Console.WriteLine(
                "export: built from a dirty working tree, and the manifest says so. The commit it names "
                + "does not reproduce it.");

        // Named rather than counted. A record left behind is invisible in the output by definition, so
        // this run is the last place anyone sees which ones they were.
        if (plan.Withheld.Count > 0)
            Console.WriteLine(
                $"export: withheld {string.Join(", ", plan.Withheld)} — .corpus.yaml excludes "
                + $"{string.Join(" and ", corpus.Descriptor.ExportExclude)}.");

        // The same reasoning for a cross-reference the export could not read. It carries what a link
        // names, and a link naming a record rather than a term inside it leaves nothing to carry — so
        // the omission is stated here, where an author can act on it, instead of being guessed at.
        if (plan.Unread.Count > 0)
        {
            Console.WriteLine(
                $"export: {plan.Unread.Count} cross-reference(s) name a record and no part inside it, so "
                + "nothing was carried for them:");
            foreach (var u in plan.Unread) Console.WriteLine($"  {u}");
            Console.WriteLine("export: point each link at the part it means, as '<file>.md#<anchor>'.");
        }

        // An empty type list is a statement of what this corpus has, and not a failure: a corpus may
        // have adopted no type that declares an export, or have withheld everything the types it did
        // adopt would have carried. The line above says which, where it was the second.
        Console.WriteLine(plan.Types.Count == 0
            ? $"export: wrote {written.Count} file(s); no type contributed a record."
            : $"export: wrote {written.Count} file(s) for {string.Join(", ", plan.Types.Select(t => t.Type))}.");
        return 0;
    }

    // Assemble the plugin from the export and the `.plugin/` tree. Two trees in, one directory out, and
    // the corpus is never loaded: what a bundle has to decide is a fact about the export it was handed.
    public static int Bundle(string repoRoot)
    {
        var pluginTree = Bundler.Read(Path.Combine(repoRoot, Bundler.SourceDir));
        if (pluginTree is null)
            return Fail($"bundle: no plugin tree at {Bundler.SourceDir}/. It is the source a plugin is built from, "
                        + "and it arrives with the mechanism.");

        var export = Bundler.Read(Path.Combine(repoRoot, Dist.Export.Replace('/', Path.DirectorySeparatorChar)));
        if (export is null)
            return Fail($"bundle: no export at {Dist.Export}/. Run it first: ./kac export");

        var plan = Bundler.Plan(new BundleSource(pluginTree, export));

        if (plan.Problems.Count > 0)
        {
            foreach (var problem in plan.Problems) Console.Error.WriteLine($"bundle: {problem}");
            return 1;
        }

        var written = Bundler.Write(repoRoot, plan);
        foreach (var path in written) Console.WriteLine($"wrote {path}");

        // Named rather than counted, as the export names what it withheld. A component that was dropped
        // is invisible in the output by definition, and two corpora building one plugin name may drop
        // different ones — so the run says which, beside the `bundle.json` that will outlive it.
        foreach (var t in plan.Trimmed)
            Console.WriteLine($"bundle: trimmed {t.Path} — {t.Reason}.");

        foreach (var warning in plan.Warnings) Console.WriteLine($"bundle: {warning}");

        Console.WriteLine(
            $"bundle: wrote {written.Count} file(s) to {Dist.Plugin}/ as {plan.PluginName} "
            + $"{plan.Version ?? "(no version)"} — {plan.Included.Count} component(s) included, "
            + $"{plan.Trimmed.Count} trimmed.");
        Console.WriteLine(
            $"bundle: {Dist.Root}/ is a marketplace holding it. Install it from a path with:  "
            + $"claude plugin marketplace add ./{Dist.Root}");
        return 0;
    }

    public static int Index(string repoRoot, bool check)
    {
        var corpus = Corpus.Load(repoRoot);

        // What every generated file should hold, beside what it holds now. `GeneratedFiles` owns which
        // files those are and which blocks each carries, so that `validate` holds a corpus to the same
        // list this writes.
        var plan = GeneratedFiles.Plan(corpus.Schema, corpus.Adopted, corpus.Docs, corpus.Tree);

        if (check)
        {
            var stale = plan.Where(f => f.Stale).Select(f => f.Path).ToList();
            if (stale.Count == 0)
            {
                Console.WriteLine($"index is up to date ({plan.Count} generated file(s)).");
                return 0;
            }

            Console.Error.WriteLine(
                "index is stale — the following generated files differ from the schema/frontmatter:");
            foreach (var s in stale) Console.Error.WriteLine($"  {s}");
            Console.Error.WriteLine("run:  dotnet run .tooling/kac.cs -- index");
            return 1;
        }

        ReportWritten(GeneratedFiles.Write(repoRoot, plan));
        return 0;
    }

    // What a regeneration wrote. Shared with `mechanism --sync`, which ends by regenerating, so that a
    // sync reports the files it rebuilt in the words `index` uses for the same work.
    private static void ReportWritten(List<string> written)
    {
        foreach (var path in written) Console.WriteLine($"wrote {path}");

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
            "fix Generator.DocRows in .tooling/kac.core/Generator.cs, or the check's 'on-type-page:' "
            + "in .schema/_checks.yaml.");
        return 1;
    }

    public static int Mechanism(string repoRoot, bool check, bool sync, string? against)
    {
        if (check == sync)
            return Fail(check
                ? "mechanism: --check and --sync are the two halves of this command; ask for one."
                : "mechanism: specify --check to compare against a reference, or --sync to take from one.");

        var descriptor = CorpusDescriptor.Load(repoRoot);

        // A descriptor still on a renamed key stops both halves. A check would report on a file it has
        // misread, and a sync would stamp beside a key it does not read.
        if (CorpusDescriptor.RenamedKeyInUse(repoRoot) is { } renamed) return Fail(renamed);

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

        var localFiles = MechanismCheck.ListFiles(repoRoot);
        var refFiles = MechanismCheck.ListFiles(refRoot);

        // Check reads this corpus's manifest, because it reports whether this corpus is in step with the
        // boundary it believes in. Sync reads the reference's, because it takes that boundary down along
        // with the files the boundary describes.
        if (check)
            return ReportMechanism(
                MechanismCheck.Classify(localFiles, refFiles, Manifest.Load(repoRoot), descriptor, Same),
                descriptor, refRoot);

        var manifest = Manifest.Load(refRoot);
        var plan = MechanismSync.Plan(localFiles, refFiles, manifest, descriptor,
            MechanismSync.DeclinedTypePaths(refRoot, descriptor), Same);

        var today = DateTime.Today.ToString("yyyy-MM-dd");
        MechanismSync.Apply(plan, repoRoot, refRoot, manifest, reference, today);
        return ReportSync(plan, repoRoot, manifest.Version, reference, today);

        // Whether two copies of a file say the same thing, which is the one question either engine asks of
        // the disk. Passed in, so each engine decides from listings and a predicate rather than from a tree.
        bool Same(string rel) => MechanismCheck.Same(repoRoot, refRoot, rel);
    }

    // A command stopping on something the caller asked for and cannot have. The message goes to stderr
    // and the exit code says so, which is the same bargain every verb strikes.
    private static int Fail(string message)
    {
        Console.Error.WriteLine(message);
        return 1;
    }

    private static int ReportMechanism(MechanismReport report, CorpusDescriptor descriptor, string refRoot)
    {
        // Where the corpus says it stands, before what the comparison found. Three versions answering
        // three questions, so a reader can tell which one moved. See CorpusDescriptor for why an unstated
        // one is reported and not filled in.
        Console.WriteLine(
            $"mechanism: content version {Stated(descriptor.ContentVersion)}, "
            + $"descriptor format {Stated(descriptor.DescriptorVersion)}, "
            + $"mechanism version {Stated(descriptor.MechanismVersion)}.");
        Console.WriteLine($"mechanism: comparing the synced layer against {refRoot}");
        Section("DRIFT — synced files differ from the reference", report.Drift);
        Section("MISSING LOCALLY — synced files in the reference but not here", report.MissingLocally);
        Section("MISSING UPSTREAM — synced files here but not in the reference", report.MissingUpstream);
        Section("UNCLASSIFIED — files matching no manifest rule", report.Unclassified);

        if (report.ResolvedDivergence.Count > 0)
        {
            Console.WriteLine(
                "RESOLVED — accepted divergences that are now identical again (delete them from .corpus.yaml):");
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

        static string Stated(object? version) => version?.ToString() is { Length: > 0 } v ? v : "not declared";
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
            Console.Error.WriteLine(
                "mechanism sync: the reference's manifest does not resolve its own tree — fix it there.");
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
            var corpus = Corpus.Load(repoRoot);
            ReportWritten(GeneratedFiles.Write(repoRoot,
                GeneratedFiles.Plan(corpus.Schema, corpus.Adopted, corpus.Docs, corpus.Tree)));
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
