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
        var corpus = Corpus.Load(repoRoot, paths);
        var findings = Validator.CheckAll(corpus);
        return Report(findings, corpus.Docs.Count, corpus.Templates.Count, corpus.SkippedNoFrontmatter, json);
    }

    public static int Index(string repoRoot, bool check)
    {
        var corpus = Corpus.Load(repoRoot, []);
        var schema = corpus.Schema;

        // Grouped by type. A document whose folder maps to no schema has nothing to be indexed under;
        // validate is the voice that says so.
        var byType = new Dictionary<string, List<Doc>>();
        foreach (var doc in corpus.Docs)
        {
            if (doc.Type is null) continue;
            (byType.TryGetValue(doc.Type.Folder, out var list) ? list : byType[doc.Type.Folder] = []).Add(doc);
        }

        // Compute the full intended content of every affected file.
        var targets = new List<(string path, string content)>();

        // Every collection type gets an index, populated or not — each type page links to one, so a
        // withheld file is a dead link rather than a tidy absence. A single-document type is its own
        // index and has nothing to generate.
        //
        // A folder absent from disk is skipped rather than created: the generator populates structure
        // the corpus has declared, and never invents it. `validate` is the one voice that says a
        // declared type is not set up, so a missing folder is reported there rather than papered over
        // here.
        foreach (var (_, t) in schema.ByFolder.OrderBy(kv => kv.Key))
        {
            if (t.IsSingleDocument || string.IsNullOrEmpty(t.Folder)) continue;
            if (!Directory.Exists(Path.Combine(repoRoot, t.Folder))) continue;
            var docs = byType.TryGetValue(t.Folder, out var found) ? found : [];
            targets.Add((Path.Combine(repoRoot, t.Folder, Artefact.Index), Generator.IndexPage(t, docs)));
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

        // The two pages that describe the taxonomy to a reader rather than to the tool: the taxonomy
        // itself, and the corpus's front door. Both list types, and the list is the half that was wrong
        // in every corpus that adopted some of them — so both are generated from what this corpus has
        // stood up, and neither can name a type whose page is not there to open.
        var stoodUp = Corpus.StoodUp(schema, repoRoot);

        Splice(Path.Combine(repoRoot, "knowledge-as-code", "taxonomy.md"),
            ("types-placement", Generator.PlacementTable(stoodUp)),
            ("types-detail", Generator.TypeCatalogue(schema.Tiers, stoodUp)),
            ("types-versus", Generator.Disambiguations(stoodUp)));

        Splice(Path.Combine(repoRoot, "README.md"),
            ("types-index", Generator.TypesIndex(stoodUp, "knowledge-as-code/taxonomy.md")));

        // Every block a page carries, spliced into one text and offered as one target — a page is written
        // once, so two blocks in the same file cannot each overwrite the other's work.
        //
        // A page that is not there is skipped, and one carrying no marker resolves to itself: the generator
        // fills in structure the corpus has declared and never invents it, which is what lets a corpus
        // decline a block by deleting its markers rather than by arguing with the tool.
        void Splice(string path, params (string Block, string Inner)[] blocks)
        {
            if (!File.Exists(path)) return;

            var text = Files.ReadLf(path);
            foreach (var (block, inner) in blocks) text = Generator.SpliceBlock(text, block, inner);
            targets.Add((path, text));
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

    private static int Report(List<Finding> findings, int validated, int templates, int skipped, bool json)
    {
        var errors = findings.Count(f => f.Severity == Sev.Error);
        var warnings = findings.Count(f => f.Severity == Sev.Warning);

        if (json)
        {
            // Emitted through the source generator (KacJson), not reflection — the core is
            // AOT-friendly. See Json.cs for the output models.
            var report = new ValidateReport(
                new ValidateSummary(validated, templates, skipped, errors, warnings),
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
        var catalogue = CheckCatalogue.For(Schema.Load(repoRoot));

        // The catalogue is always valid data, so emit it either way; the reader-facing table's
        // fidelity to it is a separate signal, reported to stderr and via the exit code below. This
        // is the tie the test suite relies on: a new catalogue check with no table row (and no
        // explicit waiver), or a row naming a check that no longer exists, exits non-zero here.
        if (json)
        {
            var report = new ChecksReport(
            [
                .. catalogue.Select(c =>
                    new CheckInfo(c.Id, c.Severity.ToString().ToLowerInvariant(), c.Summary))
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
