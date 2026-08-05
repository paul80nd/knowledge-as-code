using System.Text.Json;

// JsonSerializer for the --json output paths

// ---------------------------------------------------------------------------
// Subcommands — the orchestration behind each CLI verb. The entrypoint (.tooling/kac.cs) only wires
// System.CommandLine to these; all the work lives here and in the rest of kac.core.
// ---------------------------------------------------------------------------

namespace kac.core;

public static class Commands
{
    public static int Validate(string repoRoot, List<string> paths, bool json)
    {
        var schema = Schema.Load(repoRoot);
        var corpus = Corpus.Discover(repoRoot, schema, paths);

        var findings = new List<Finding>();
        var docs = new List<Doc>();
        var skippedNoFrontmatter = 0;

        // Parse pass: load every candidate, keep the ones that have opted into the
        // schema by carrying a frontmatter block.
        foreach (var rel in corpus)
        {
            var full = Path.Combine(repoRoot, rel);
            var text = File.ReadAllText(full);
            var doc = Doc.Parse(rel, text, schema);
            if (doc is null)
            {
                skippedNoFrontmatter++;
                continue;
            }

            docs.Add(doc);
        }

        // Per-document checks.
        foreach (var doc in docs)
            Validator.CheckDocument(doc, schema, repoRoot, findings);

        // Corpus-wide checks (uniqueness, reciprocity) need every doc in hand.
        Validator.CheckCorpus(docs, findings);

        return Report(findings, docs.Count, skippedNoFrontmatter, json);
    }

    public static int Index(string repoRoot, bool check)
    {
        var schema = Schema.Load(repoRoot);
        var corpus = Corpus.Discover(repoRoot, schema, []);

        // Load every frontmatter-bearing doc, grouped by type.
        var byType = new Dictionary<string, List<Doc>>();
        foreach (var rel in corpus)
        {
            var doc = Doc.Parse(rel, File.ReadAllText(Path.Combine(repoRoot, rel)), schema);
            if (doc?.Type is null) continue;
            (byType.TryGetValue(doc.Type.Folder, out var list) ? list : byType[doc.Type.Folder] = []).Add(doc);
        }

        // Compute the full intended content of every affected file.
        var targets = new List<(string path, string content)>();

        // Every type with a folder gets an INDEX, populated or not — each type page links to one, so a
        // withheld file is a dead link rather than a tidy absence. Types without a folder (glossary is
        // the single-document type) have nothing to index, and a folder absent from disk is skipped so
        // the generator never conjures a directory.
        foreach (var (_, t) in schema.ByFolder.OrderBy(kv => kv.Key))
        {
            if (string.IsNullOrEmpty(t.Folder) || !Directory.Exists(Path.Combine(repoRoot, t.Folder))) continue;
            var docs = byType.TryGetValue(t.Folder, out var found) ? found : [];
            targets.Add((Path.Combine(repoRoot, t.Folder, "INDEX.md"), Generator.IndexPage(t, docs)));
        }

        // The schema and checks blocks derive from the schema alone, so every type gets them whether or
        // not it holds records yet. Restricting this to populated types would leave the markers on an
        // empty page holding hand-written text nothing checks, to be overwritten by whoever adds the
        // type's first record — surfacing the drift at the least convenient moment.
        foreach (var (key, t) in schema.ByFolder.OrderBy(kv => kv.Key))
        {
            var pagePath = Path.Combine(repoRoot, t.Page);
            if (!File.Exists(pagePath)) continue;

            var text = Files.ReadLf(pagePath);
            text = Generator.SpliceBlock(text, $"schema-{key}", Generator.SchemaTable(t, schema));
            text = Generator.SpliceBlock(text, $"checks-{key}", Generator.ChecksTable(t));
            targets.Add((pagePath, text));
        }

        // metadata.md documents the universal fields for the whole taxonomy. It is not a type page —
        // it has no records and no folder — but it is derived from the same schema, so it is generated
        // on the same pass rather than hand-maintained beside it.
        var metadataPath = Path.Combine(repoRoot, "knowledge-as-code", "metadata.md");
        if (File.Exists(metadataPath))
        {
            var text = Generator.SpliceBlock(Files.ReadLf(metadataPath), "schema-universal",
                Generator.UniversalSchemaTable(schema));
            targets.Add((metadataPath, text));
        }

        if (check)
        {
            var stale = targets.Where(x => !File.Exists(x.path) || Files.ReadLf(x.path) != x.content)
                .Select(x => Path.GetRelativePath(repoRoot, x.path).Replace('\\', '/'))
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

        var written = 0;
        foreach (var (path, content) in targets)
        {
            if (File.Exists(path) && Files.ReadLf(path) == content) continue;
            File.WriteAllText(path, content);
            Console.WriteLine($"wrote {Path.GetRelativePath(repoRoot, path).Replace('\\', '/')}");
            written++;
        }

        Console.WriteLine(written == 0
            ? "index already up to date; nothing written."
            : $"index updated {written} file(s).");
        return 0;
    }

    private static int Report(List<Finding> findings, int validated, int skipped, bool json)
    {
        var errors = findings.Count(f => f.Severity == Sev.Error);
        var warnings = findings.Count(f => f.Severity == Sev.Warning);

        if (json)
        {
            // Emitted through the source generator (KacJson), not reflection — the core is
            // AOT-friendly. See Json.cs for the output models.
            var report = new ValidateReport(
                new ValidateSummary(validated, skipped, errors, warnings),
                [
                    .. findings
                        .OrderBy(f => f.File).ThenBy(f => f.Line ?? 0)
                        .Select(f => new ValidateFinding(
                            f.File, f.Line, f.Severity.ToString().ToLowerInvariant(), f.Check, f.Message))
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

        Console.WriteLine(
            $"validated {validated} document(s), skipped {skipped} without frontmatter — {errors} error(s), {warnings} warning(s)");
        return errors > 0 ? 1 : 0;
    }

    public static int Checks(bool json)
    {
        // The catalogue is always valid data, so emit it either way; the reader-facing table's
        // fidelity to it is a separate signal, reported to stderr and via the exit code below. This
        // is the tie the test suite relies on: a new catalogue check with no table row (and no
        // explicit waiver), or a row naming a check that no longer exists, exits non-zero here.
        if (json)
        {
            var report = new ChecksReport(
            [
                .. CheckCatalogue.All.Select(c =>
                    new CheckInfo(c.Id, c.Severity.ToString().ToLowerInvariant(), c.Summary))
            ]);
            Console.WriteLine(JsonSerializer.Serialize(report, KacJson.Relaxed.ChecksReport));
        }
        else
        {
            foreach (var c in CheckCatalogue.All)
            {
                var tag = c.Severity == Sev.Error ? "error  " : "warning";
                Console.WriteLine($"  {tag}  {c.Id,-24}  {c.Summary}");
            }

            Console.WriteLine();
            Console.WriteLine($"{CheckCatalogue.All.Count} checks.");
        }

        var problems = Generator.ChecksTableProblems();
        if (problems.Count == 0) return 0;

        Console.Error.WriteLine("checks: the reader-facing checks table is out of step with the catalogue:");
        foreach (var p in problems) Console.Error.WriteLine($"  {p}");
        Console.Error.WriteLine(
            "fix Generator.DocRows (or IntentionallyUndocumented) in .tooling/kac.core/Generator.cs.");
        return 1;
    }

    public static int Mechanism(string repoRoot, bool check, string? against)
    {
        if (!check)
        {
            Console.Error.WriteLine(
                "mechanism: specify --check (mechanism --sync is not yet implemented — see issue #6).");
            return 1;
        }

        var manifest = Manifest.Load(repoRoot);
        var lockFile = MechanismLock.Load(repoRoot);

        var reference = against ?? lockFile.UpstreamUrl;
        if (string.IsNullOrWhiteSpace(reference))
            return Fail("mechanism: no reference to compare against. Pass --against <path>, "
                        + "or set upstream.url in .mechanism.lock.");

        var refRoot = Path.GetFullPath(reference, repoRoot);
        if (!Directory.Exists(refRoot))
            return Fail($"mechanism: reference corpus not found: {refRoot}");
        return Path.GetFullPath(refRoot) == Path.GetFullPath(repoRoot)
            ? Fail("mechanism: the reference is this corpus itself — nothing to compare.")
            : MechanismCheck.Run(repoRoot, refRoot, manifest, lockFile);

        static int Fail(string message)
        {
            Console.Error.WriteLine(message);
            return 1;
        }
    }
}
