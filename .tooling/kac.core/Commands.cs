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

        // The types this corpus took, which is what everything below is generated from. The schema
        // declares the framework's full range and a corpus adopts as much of it as it has use for, so
        // generating per schema type would write pages for types the corpus declined — files no list of
        // its types names, and which `index --check` then holds it to keeping fresh.
        var adopted = Corpus.Adopted(schema, repoRoot, corpus.Descriptor);

        // Every adopted type gets an index, populated or not — each type page links to one, so a
        // withheld file is a dead link rather than a tidy absence.
        //
        // A folder absent from disk is skipped rather than created: the generator populates structure
        // the corpus has declared, and never invents it. `validate` is the one voice that says an adopted
        // type is not set up, so a missing folder is reported there rather than papered over here.
        foreach (var t in adopted)
        {
            if (string.IsNullOrEmpty(t.Folder)) continue;
            if (!Directory.Exists(Path.Combine(repoRoot, t.Folder))) continue;
            var docs = byType.TryGetValue(t.Folder, out var found) ? found : [];
            targets.Add((Path.Combine(repoRoot, t.Folder, Artefact.Index), Generator.IndexPage(t, docs)));
        }

        // The schema and checks blocks derive from the schema alone, so every adopted type gets them
        // whether or not it holds records yet. Restricting this to populated types would leave the markers
        // on an empty page holding hand-written text nothing checks, to be overwritten by whoever adds the
        // type's first record — surfacing the drift at the least convenient moment.
        foreach (var t in adopted)
        {
            var pagePath = Path.Combine(repoRoot, t.Page);
            if (!File.Exists(pagePath)) continue;

            var text = Files.ReadLf(pagePath);
            text = Generator.SpliceBlock(text, $"schema-{t.Key}", Generator.SchemaTable(t, schema));
            text = Generator.SpliceBlock(text, $"checks-{t.Key}", Generator.ChecksTable(t));
            targets.Add((pagePath, text));
        }

        // The pages that describe the taxonomy to a reader rather than to the tool. Each lists types, and
        // the list is the half that was wrong in every corpus that adopted some of them — so none can name
        // a type whose page is not there to open. `metadata.md` also carries the universal field table,
        // which is the schema's alone.
        Splice(Path.Combine(repoRoot, "knowledge-as-code", "metadata.md"),
            ("schema-universal", Generator.UniversalSchemaTable(schema)),
            ("types-metadata", Generator.MetadataStrip(adopted)));

        Splice(Path.Combine(repoRoot, "knowledge-as-code", "taxonomy.md"),
            ("types-placement", Generator.PlacementTable(adopted)),
            ("types-detail", Generator.TypeCatalogue(schema.Tiers, adopted)),
            ("types-versus", Generator.Disambiguations(adopted)),
            ("types-graph", Generator.RelationDiagram(adopted)),
            ("types-edges", Generator.RelationTable(adopted)));

        Splice(Path.Combine(repoRoot, "knowledge-as-code", "lineage.md"),
            ("types-lineage", Generator.LineageTable(adopted)),
            ("types-collisions", Generator.Collisions(adopted)));

        Splice(Path.Combine(repoRoot, "README.md"),
            ("types-index", Generator.TypesIndex(adopted, "knowledge-as-code/taxonomy.md")));

        // Every block a page carries, spliced into one text and offered as one target — a page is written
        // once, so two blocks in the same file cannot each overwrite the other's work.
        //
        // A page that is not there is skipped, and one carrying no marker resolves to itself: the generator
        // fills in structure the corpus has declared and never invents it. Deleting the markers is how a
        // corpus declines a block, rather than by arguing with the tool.
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

        // Check reads this corpus's manifest, because it reports whether this corpus is in step with the
        // boundary it believes in. Sync reads the reference's, because it takes that boundary down along
        // with the files the boundary describes.
        return check
            ? MechanismCheck.Run(repoRoot, refRoot, Manifest.Load(repoRoot), descriptor)
            : MechanismSync.Run(repoRoot, refRoot, Manifest.Load(refRoot), descriptor, reference,
                DateTime.Today.ToString("yyyy-MM-dd"));

        static int Fail(string message)
        {
            Console.Error.WriteLine(message);
            return 1;
        }
    }
}
