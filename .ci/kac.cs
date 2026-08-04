#:package YamlDotNet@18.*
#:package Markdig@1.*
#:package System.CommandLine@2.0.*
#:property TargetFramework=net10.0
#:property Nullable=enable

// kac — the knowledge-as-code validator and generator.
//
// One tool, several subcommands, sharing a schema-loading and markdown-parsing core:
//
//   validate   check the corpus against knowledge-as-code/schema/*.yaml
//   index      regenerate INDEX.md and the generated blocks in <type>.md
//   mechanism  enforce the portability manifest: synced layer vs a reference
//
// The tool is deliberately free of type-specific rules: everything it enforces is
// read from the YAML schema, so adding a type is adding a YAML file, not editing C#.
// See .ci/README.md for what fails versus warns and how each check maps to the schema.

using System.CommandLine;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

var repoRoot = FindRepoRoot(Directory.GetCurrentDirectory());
if (repoRoot is null)
{
    Console.Error.WriteLine("kac: could not locate the repo root (no knowledge-as-code/schema above the cwd).");
    return 2;
}

// validate — check the corpus against the schema.
var pathsArg = new Argument<string[]>("paths")
{
    Arity = ArgumentArity.ZeroOrMore,
    Description = "Subtrees or files to validate (default: the whole repo)."
};
var jsonOpt = new Option<bool>("--json") { Description = "Emit the summary and findings as JSON." };
var validate = new Command("validate", "Check the corpus against knowledge-as-code/schema/*.yaml.")
{
    pathsArg,
    jsonOpt
};
validate.SetAction(pr => Commands.Validate(repoRoot, [.. pr.GetValue(pathsArg) ?? []], pr.GetValue(jsonOpt)));

// index — regenerate INDEX.md and the generated blocks in <type>.md.
var checkOpt = new Option<bool>("--check") { Description = "Fail if a generated file is stale instead of writing it." };
var index = new Command("index", "Regenerate INDEX.md and the generated blocks in <type>.md.")
{
    checkOpt
};
index.SetAction(pr => Commands.Index(repoRoot, pr.GetValue(checkOpt)));

// checks — list every check the validator implements. Its purpose is machinery, not humans:
// the test suite reads `checks --json` to assert every rule is exercised by a fixture, so a
// new rule cannot ship without a golden covering it.
var checksJsonOpt = new Option<bool>("--json") { Description = "Emit the check catalogue as JSON." };
var checks = new Command("checks", "List every check the validator implements.")
{
    checksJsonOpt
};
checks.SetAction(pr => Commands.Checks(pr.GetValue(checksJsonOpt)));

// mechanism — enforce the portability manifest. `--check` compares this corpus's synced layer
// against a reference copy and reports drift, following the same discipline as `index --check`:
// recompute, compare, name what is stale, exit non-zero, never write. `--sync` (the write half)
// is not implemented yet — see issue #6.
var mechCheckOpt = new Option<bool>("--check") { Description = "Compare the synced layer against a reference and report drift; never writes." };
var againstOpt = new Option<string?>("--against") { Description = "Reference corpus to compare against (a path). Defaults to upstream.url in mechanism.lock." };
var mechanism = new Command("mechanism", "Enforce the portability manifest: compare the synced layer against a reference.")
{
    mechCheckOpt,
    againstOpt
};
mechanism.SetAction(pr => Commands.Mechanism(repoRoot, pr.GetValue(mechCheckOpt), pr.GetValue(againstOpt)));

var root = new RootCommand("kac — the knowledge-as-code validator and generator.") { validate, index, checks, mechanism };

// Bad arguments exit 1 (System.CommandLine's default) — the printed error makes it
// obvious it was a usage problem rather than corpus errors. Exit 2 is reserved for the
// pre-flight failure above (no repo root), where the tool never got as far as parsing.
return root.Parse(args).Invoke();

static string? FindRepoRoot(string start)
{
    var dir = new DirectoryInfo(start);
    while (dir is not null)
    {
        if (Directory.Exists(Path.Combine(dir.FullName, "knowledge-as-code", "schema")))
            return dir.FullName;
        dir = dir.Parent;
    }

    return null;
}

// ---------------------------------------------------------------------------
// Subcommands
// ---------------------------------------------------------------------------

internal static class Commands
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
        Validator.CheckCorpus(docs, schema, findings);

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

        // Compute the full intended content of every affected file. A type is
        // regenerated only if it has at least one record, so unmigrated types (empty
        // INDEX.md, other <type>.md pages) are never touched.
        var targets = new List<(string path, string content)>();
        foreach (var (folder, docs) in byType.OrderBy(kv => kv.Key))
        {
            var t = schema.ByFolder[folder];

            var indexPath = Path.Combine(repoRoot, folder, "INDEX.md");
            targets.Add((indexPath, Generator.IndexPage(t, docs)));

            var pagePath = Path.Combine(repoRoot, t.Page);
            if (File.Exists(pagePath))
            {
                var text = Read(pagePath);
                text = Generator.SpliceBlock(text, $"schema-{folder}", Generator.SchemaTable(t));
                text = Generator.SpliceBlock(text, $"checks-{folder}", Generator.ChecksTable());
                targets.Add((pagePath, text));
            }
        }

        if (check)
        {
            var stale = targets.Where(x => !File.Exists(x.path) || Read(x.path) != x.content)
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
            Console.Error.WriteLine("run:  dotnet run .ci/kac.cs -- index");
            return 1;
        }

        var written = 0;
        foreach (var (path, content) in targets)
        {
            if (File.Exists(path) && Read(path) == content) continue;
            File.WriteAllText(path, content);
            Console.WriteLine($"wrote {Path.GetRelativePath(repoRoot, path).Replace('\\', '/')}");
            written++;
        }

        Console.WriteLine(written == 0
            ? "index already up to date; nothing written."
            : $"index updated {written} file(s).");
        return 0;
    }

    // Read a file as UTF-8 with LF line endings, so generation is line-ending stable
    // regardless of what the working copy happens to carry.
    private static string Read(string path) => File.ReadAllText(path).Replace("\r\n", "\n");

    private static int Report(List<Finding> findings, int validated, int skipped, bool json)
    {
        var errors = findings.Count(f => f.Severity == Sev.Error);
        var warnings = findings.Count(f => f.Severity == Sev.Warning);

        if (json)
        {
            // Emitted through the source generator (KacJson), not reflection — reflection-based
            // serialization is disabled in file-based apps. See the JSON output models section.
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
                [.. CheckCatalogue.All.Select(c => new CheckInfo(c.Id, c.Severity.ToString().ToLowerInvariant(), c.Summary))]);
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
        Console.Error.WriteLine("fix Generator.DocRows (or IntentionallyUndocumented) in .ci/kac.cs.");
        return 1;
    }

    public static int Mechanism(string repoRoot, bool check, string? against)
    {
        if (!check)
        {
            Console.Error.WriteLine("mechanism: specify --check (mechanism --sync is not yet implemented — see issue #6).");
            return 1;
        }

        var manifest = Manifest.Load(repoRoot);
        var lockFile = MechanismLock.Load(repoRoot);

        var reference = against ?? lockFile.UpstreamUrl;
        if (string.IsNullOrWhiteSpace(reference))
            return Fail("mechanism: no reference to compare against. Pass --against <path>, "
                + "or set upstream.url in knowledge-as-code/mechanism.lock.");

        var refRoot = Path.GetFullPath(reference, repoRoot);
        if (!Directory.Exists(refRoot))
            return Fail($"mechanism: reference corpus not found: {refRoot}");
        if (Path.GetFullPath(refRoot) == Path.GetFullPath(repoRoot))
            return Fail("mechanism: the reference is this corpus itself — nothing to compare.");

        return MechanismCheck.Run(repoRoot, refRoot, manifest, lockFile);

        static int Fail(string message)
        {
            Console.Error.WriteLine(message);
            return 1;
        }
    }
}

// ---------------------------------------------------------------------------
// Findings
// ---------------------------------------------------------------------------

internal enum Sev
{
    Error,
    Warning
}

internal record Finding(string File, int? Line, Sev Severity, string Check, string Message);

// The registry of every check the validator can emit. It exists so the test suite can assert
// coverage (every id here must be triggered by a fixture) and so a human can `kac checks` to see
// the ruleset. Adding a new Err/Warn check to the validator means adding its id here — the
// coverage meta-test fails on any id emitted in a fixture golden that is missing from this list,
// which keeps the two honest. Ordered roughly as a document is checked, then corpus-wide.
internal readonly record struct CheckDef(string Id, Sev Severity, string Summary);

internal static class CheckCatalogue
{
    public static readonly IReadOnlyList<CheckDef> All =
    [
        new("type", Sev.Error, "The document's folder maps to a type schema."),
        new("frontmatter-parses", Sev.Error, "The frontmatter block is valid YAML and a mapping."),
        new("unknown-key", Sev.Error, "Every frontmatter key is a known field or reserved key."),
        new("key-order", Sev.Error, "Key order is a topological extension of the schema's field orders."),
        new("required-field", Sev.Error, "Every required (and required-when) field is present."),
        new("bare-key", Sev.Error, "An absent value is a bare key, not null / ~ / \"\" / —."),
        new("date-quoted", Sev.Error, "A date field is a quoted string."),
        new("date-format", Sev.Error, "A date field is YYYY-MM-DD in shape."),
        new("enum", Sev.Error, "An enum value is a scalar in the declared range."),
        new("enum-lowercase", Sev.Error, "An enum value is lowercase."),
        new("list", Sev.Error, "A list field is a YAML sequence."),
        new("tier-matches-type", Sev.Error, "tier equals the tier the type declares."),
        new("id-prefix", Sev.Error, "id carries the type's prefix."),
        new("id-format", Sev.Error, "id has the type's prefix and numeric width."),
        new("id-matches-filename", Sev.Error, "id's number matches the filename's number."),
        new("filename-pattern", Sev.Error, "The filename matches the type's filename pattern."),
        new("slug-length", Sev.Error, "The slug is within the type's slug-max."),
        new("h1", Sev.Error, "The document has an H1."),
        new("h1-pattern", Sev.Error, "The H1 matches the type's h1-pattern."),
        new("h1-matches-id", Sev.Error, "The H1's number matches the id."),
        new("required-section", Sev.Error, "Every required section heading is present."),
        new("link-resolves", Sev.Error, "Every internal link resolves."),
        new("undefined-label", Sev.Error, "A shortcut reference has a link definition."),
        new("related-matches-section", Sev.Error, "A mirrors-section field reconciles with its section."),
        new("id-unique", Sev.Error, "id is unique across the whole wiki."),
        new("reciprocal", Sev.Error, "A reciprocal field agrees in both directions."),
        new("unused-definition", Sev.Warning, "A link definition that nothing references."),
        new("bracket-literal", Sev.Warning, "A [...] in prose that looks like a broken reference."),
        new("y-statement", Sev.Warning, "A short Y-statement block-quote follows the H1."),
        new("alternatives-verdict", Sev.Warning, "Each Alternatives Considered bullet states an outcome."),
    ];
}

// ---------------------------------------------------------------------------
// JSON output models
//
// Every JSON document kac emits is a record serialized through the source generator
// (KacJson) — reflection-based serialization is disabled in file-based apps. To add a
// new document (e.g. `digest` or `drift` output): declare its record shape here and add
// one `[JsonSerializable(typeof(...))]` line to KacJson; no other plumbing is needed.
//
// Property names are PascalCase and emitted camelCase; `int?` line is written as `null`.
// ---------------------------------------------------------------------------

internal record ValidateReport(ValidateSummary Summary, IReadOnlyList<ValidateFinding> Findings);

internal record ValidateSummary(int Validated, int Skipped, int Errors, int Warnings);

internal record ValidateFinding(string File, int? Line, string Severity, string Check, string Message);

internal record ChecksReport(IReadOnlyList<CheckInfo> Checks);

internal record CheckInfo(string Check, string Severity, string Summary);

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
[JsonSerializable(typeof(ValidateReport))]
[JsonSerializable(typeof(ChecksReport))]
internal partial class KacJson : JsonSerializerContext
{
    private static KacJson? _relaxed;

    // Shared context for CLI output: the source-generated metadata from Default, plus
    // relaxed escaping so quotes and dashes in messages stay human-readable rather than
    // becoming ' / —. Lazily initialised so it does not touch the generator's Default
    // during static construction (their init order across the partial is unspecified).
    public static KacJson Relaxed => _relaxed ??= new KacJson(new JsonSerializerOptions(Default.Options)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });
}

// ---------------------------------------------------------------------------
// Generation — INDEX pages and the generated blocks in <type>.md
// ---------------------------------------------------------------------------

internal static class Generator
{
    private const string Banner =
        "<!-- Generated by `dotnet run .ci/kac.cs -- index` — do not edit by hand; run that command to regenerate. -->";

    public static string IndexPage(TypeSchema t, List<Doc> docs)
    {
        var headers = t.IndexColumns.Select(Humanize).ToList();
        var sort = string.IsNullOrEmpty(t.IndexSort) ? "id" : t.IndexSort;
        var rows = docs
            .OrderBy(d => d.FrontScalar(sort) ?? "", StringComparer.Ordinal)
            .Select(d => t.IndexColumns.Select(c => Cell(d, c)).ToList())
            .ToList();

        var title = t.IdPrefix.ToUpperInvariant() + " Index";
        return $"{Banner}\n\n# {title}\n\n{RenderTable(headers, rows)}\n";
    }

    private static string Cell(Doc d, string col)
    {
        if (col == "title")
        {
            var file = Path.GetFileName(d.Rel);
            return $"[{Escape(d.TitleText())}]({file})";
        }

        return Escape(d.FrontScalar(col) ?? "");
    }

    public static string SchemaTable(TypeSchema t)
    {
        List<string> headers = ["Field", "Req", "Type", "Notes"];
        var rows = t.FieldOrder.Select(name =>
        {
            var f = t.Fields[name];
            return new List<string> { $"`{name}`", f.Required ? "●" : "", f.Type, NotesFor(f) };
        }).ToList();
        return RenderTable(headers, rows);
    }

    private static string NotesFor(FieldSpec f)
    {
        var parts = new List<string>();
        if (f is { Type: "enum", Values.Count: > 0 })
            parts.Add(string.Join(" · ", f.Values.Select(v => $"`{v}`")));
        if (!string.IsNullOrEmpty(f.Notes)) parts.Add(f.Notes);
        if (!string.IsNullOrEmpty(f.RequiredWhen))
            parts.Add($"Required once `{f.RequiredWhen.Split("==").Last().Trim()}`.");
        return Escape(string.Join(" ", parts));
    }

    // The reader-facing "What CI checks" table: a curated, grouped view of the catalogue that
    // `kac index` splices into every type page. It is deliberately NOT the raw catalogue — related
    // checks are folded into one row (e.g. the three `id-*` checks read as one `id` row) and worded
    // for a human skim. Each row therefore names the catalogue ids it stands for, so the table's
    // coverage stays verifiable (ChecksTableProblems) even though its presentation is hand-tuned.
    private static readonly (string Label, string[] Ids, string Description)[] DocRows =
    [
        ("frontmatter-parses", ["frontmatter-parses"], "Frontmatter is present and is a valid YAML mapping."),
        ("unknown-key", ["unknown-key"], "Every frontmatter key is a schema field or a reserved ADO key."),
        ("key-order", ["key-order"], "Key order is a topological extension of the schema's field order."),
        ("required-field", ["required-field"], "Required and conditionally-required fields are present."),
        ("bare-key", ["bare-key"], "An absent value is a bare key, never `null`, `~`, `\"\"` or `—`."),
        ("date-quoted / date-format", ["date-quoted", "date-format"], "Date fields are quoted `YYYY-MM-DD`."),
        ("enum", ["enum", "enum-lowercase"], "Enum values are in range and lowercase."),
        ("tier-matches-type", ["tier-matches-type"], "`tier` matches the tier the type declares."),
        ("id", ["id-prefix", "id-format", "id-matches-filename"],
            "`id` has the type's prefix and width and matches the filename number."),
        ("id-unique", ["id-unique"], "`id` is unique across the whole wiki."),
        ("filename / slug-length", ["filename-pattern", "slug-length"],
            "Filename matches the pattern; the slug is within 30 characters."),
        ("h1", ["h1", "h1-pattern", "h1-matches-id"], "The H1 matches the title pattern and its number matches the `id`."),
        ("required-section", ["required-section"], "Every required section heading is present."),
        ("link-resolves", ["link-resolves"], "Every internal link resolves (all link forms, `.md` optional)."),
        ("undefined-label", ["undefined-label"], "Every `[ADR-NNNN]` shortcut reference has a link definition."),
        ("related-matches-section", ["related-matches-section"],
            "`related` reconciles with the ids in the `## Related` section."),
        ("reciprocal", ["reciprocal"], "`supersedes` / `superseded-by` agree in both directions."),
        ("unused-definition", ["unused-definition"], "A link definition that nothing references."),
        ("y-statement", ["y-statement"], "A Y-statement block-quote follows the H1 and is within 60 words."),
        ("alternatives-verdict", ["alternatives-verdict"], "Each Alternatives Considered bullet states a verdict.")
    ];

    // Catalogue checks the reader-facing table deliberately does not surface: `type` (an internal
    // folder→schema guard a well-formed document never trips), `list` (a low-level YAML-shape check
    // subsumed by the field descriptions) and `bracket-literal` (a heuristic sibling of
    // undefined-label). Every OTHER catalogue id must appear in DocRows — ChecksTableProblems fails
    // otherwise, so a new check cannot silently go undocumented.
    private static readonly HashSet<string> IntentionallyUndocumented =
        new(["type", "list", "bracket-literal"], StringComparer.Ordinal);

    public static string ChecksTable()
    {
        var severity = CheckCatalogue.All.ToDictionary(c => c.Id, c => c.Severity);
        List<string> headers = ["Check", "Level", "What it verifies"];
        var rows = DocRows.Select(r => new List<string>
        {
            $"`{r.Label}`",
            (severity.TryGetValue(r.Ids[0], out var s) ? s : Sev.Error).ToString().ToLowerInvariant(),
            r.Description
        }).ToList();
        return RenderTable(headers, rows);
    }

    // Reconcile the curated table with the catalogue. Empty means the reader-facing table is a
    // faithful, complete view of what the validator enforces; any entry is a drift a human must
    // resolve. `kac checks` calls this and fails on a non-empty result, which the test suite asserts.
    public static IReadOnlyList<string> ChecksTableProblems()
    {
        var catalogue = CheckCatalogue.All.Select(c => c.Id).ToHashSet(StringComparer.Ordinal);
        var documented = DocRows.SelectMany(r => r.Ids).ToHashSet(StringComparer.Ordinal);
        var problems = new List<string>();

        foreach (var id in documented.Where(id => !catalogue.Contains(id)).Order(StringComparer.Ordinal))
            problems.Add($"the checks table documents '{id}', which is not a catalogue check (stale row).");
        foreach (var id in catalogue.Where(id => !documented.Contains(id) && !IntentionallyUndocumented.Contains(id))
                     .Order(StringComparer.Ordinal))
            problems.Add($"catalogue check '{id}' is neither in the checks table nor waived in IntentionallyUndocumented.");
        foreach (var id in IntentionallyUndocumented.Where(id => documented.Contains(id) || !catalogue.Contains(id))
                     .Order(StringComparer.Ordinal))
            problems.Add($"'{id}' is waived in IntentionallyUndocumented but is documented or unknown — drop the waiver.");

        return problems;
    }

    public static string SpliceBlock(string text, string name, string inner)
    {
        var begin = $"<!-- BEGIN GENERATED: {name} -->";
        var end = $"<!-- END GENERATED: {name} -->";
        var bi = text.IndexOf(begin, StringComparison.Ordinal);
        if (bi < 0) return text;
        var ei = text.IndexOf(end, bi, StringComparison.Ordinal);
        if (ei < 0) return text;
        return text[..(bi + begin.Length)] + "\n\n" + inner + "\n\n" + text[ei..];
    }

    // Deterministic GFM table: fixed column widths, single-space padding, LF joins,
    // no trailing newline (callers add their own).
    private static string RenderTable(List<string> headers, List<List<string>> rows)
    {
        var n = headers.Count;
        var w = new int[n];
        for (var i = 0; i < n; i++) w[i] = headers[i].Length;
        foreach (var row in rows)
            for (var i = 0; i < n; i++)
                w[i] = Math.Max(w[i], row[i].Length);

        List<string> lines =
        [
            Row(headers, w), Sep(w),
            .. rows.Select(r => Row(r, w))
        ];
        return string.Join("\n", lines);
    }

    private static string Row(List<string> cells, int[] w)
    {
        var sb = new StringBuilder("|");
        for (var i = 0; i < cells.Count; i++)
            sb.Append(' ').Append(cells[i].PadRight(w[i])).Append(" |");
        return sb.ToString();
    }

    private static string Sep(int[] w)
    {
        var sb = new StringBuilder("|");
        foreach (var width in w)
            sb.Append(' ').Append(new string('-', Math.Max(3, width))).Append(" |");
        return sb.ToString();
    }

    private static string Humanize(string col)
    {
        if (col == "id") return "ID";
        var s = col.Replace('-', ' ');
        return char.ToUpperInvariant(s[0]) + s[1..];
    }

    private static string Escape(string cell)
        => cell.Replace("\r", " ").Replace("\n", " ").Replace("|", "\\|");
}

// ---------------------------------------------------------------------------
// Schema model — loaded from knowledge-as-code/schema/*.yaml
// ---------------------------------------------------------------------------

internal class FieldSpec
{
    public required string Name;
    public bool Required;
    public string? RequiredWhen; // e.g. "status == accepted"
    public string Type = "string"; // string|date|enum|id|list|bool|int
    public string? Of; // element type when Type == list
    public List<string>? Values; // enum values (resolved)
    public string? Ref; // folder the id must belong to
    public string? Reciprocal; // field on the target that must point back
    public string? Pattern;
    public string? MirrorsSection; // section whose ids this field must mirror
    public string? Notes; // prose, rendered into the generated Metadata table
}

internal class TypeSchema
{
    public string TypeName = "", Folder = "", Page = "", Tier = "", Lifecycle = "";
    public string IdPrefix = "", IdStyle = "";
    public int IdWidth;
    public string? FilenamePattern;
    public int SlugMax = 30;
    public string? H1Pattern;
    public bool TitleMatchesId;
    public List<string> FieldOrder = [];
    public Dictionary<string, FieldSpec> Fields = [];
    public List<string> RequiredSections = [];
    public List<string> OptionalSections = [];
    public List<string> IndexColumns = [];
    public string? IndexSort;
    public List<Dictionary<string, object>> Rules = [];
}

internal class Schema
{
    public List<string> UniversalOrder = [];
    public Dictionary<string, FieldSpec> Universal = [];
    public List<string> Reserved = [];
    public Dictionary<string, List<string>> Enums = [];
    public Dictionary<string, TypeSchema> ByFolder = [];

    public static Schema Load(string repoRoot)
    {
        var dir = Path.Combine(repoRoot, "knowledge-as-code", "schema");
        var s = new Schema();

        var enumsRoot = Yaml.LoadFile(Path.Combine(dir, "_enums.yaml"));
        foreach (var (name, node) in Yaml.Map(Yaml.Get(enumsRoot, "enums")))
            s.Enums[name] = Yaml.StrList(Yaml.Get(node, "values"));

        var uni = Yaml.LoadFile(Path.Combine(dir, "_universal.yaml"));
        foreach (var (name, node) in Yaml.Map(Yaml.Get(uni, "fields")))
        {
            s.UniversalOrder.Add(name);
            s.Universal[name] = ParseField(name, node, s);
        }

        s.Reserved = Yaml.StrList(Yaml.Get(uni, "reserved"));

        foreach (var file in Directory.GetFiles(dir, "*.yaml").OrderBy(f => f))
        {
            var baseName = Path.GetFileNameWithoutExtension(file);
            if (baseName.StartsWith('_')) continue; // _universal, _enums
            s.ByFolder[baseName] = ParseType(Yaml.LoadFile(file), s);
        }

        return s;
    }

    private static TypeSchema ParseType(YamlNode root, Schema s)
    {
        var t = new TypeSchema
        {
            TypeName = Yaml.Str(Yaml.Get(root, "type")) ?? "",
            Folder = Yaml.Str(Yaml.Get(root, "folder")) ?? "",
            Page = Yaml.Str(Yaml.Get(root, "page")) ?? "",
            Tier = Yaml.Str(Yaml.Get(root, "tier")) ?? "",
            Lifecycle = Yaml.Str(Yaml.Get(root, "lifecycle")) ?? ""
        };

        var id = Yaml.Get(root, "id");
        if (id is not null)
        {
            t.IdPrefix = Yaml.Str(Yaml.Get(id, "prefix")) ?? "";
            t.IdStyle = Yaml.Str(Yaml.Get(id, "style")) ?? "";
            t.IdWidth = Yaml.Int(Yaml.Get(id, "width"), 4);
        }

        var fn = Yaml.Get(root, "filename");
        if (fn is not null)
        {
            t.FilenamePattern = Yaml.Str(Yaml.Get(fn, "pattern"));
            t.SlugMax = Yaml.Int(Yaml.Get(fn, "slug-max"), 30);
        }

        var title = Yaml.Get(root, "title");
        if (title is not null)
        {
            t.H1Pattern = Yaml.Str(Yaml.Get(title, "h1-pattern"));
            t.TitleMatchesId = Yaml.Bool(Yaml.Get(title, "matches-id"));
        }

        foreach (var (name, node) in Yaml.Map(Yaml.Get(root, "fields")))
        {
            t.FieldOrder.Add(name);
            t.Fields[name] = ParseField(name, node, s);
        }

        var sections = Yaml.Get(root, "sections");
        if (sections is not null)
        {
            t.RequiredSections = Yaml.StrList(Yaml.Get(sections, "required"));
            t.OptionalSections = Yaml.StrList(Yaml.Get(sections, "optional"));
        }

        var index = Yaml.Get(root, "index");
        if (index is not null)
        {
            t.IndexColumns = Yaml.StrList(Yaml.Get(index, "columns"));
            t.IndexSort = Yaml.Str(Yaml.Get(index, "sort"));
        }

        if (Yaml.Get(root, "rules") is YamlSequenceNode rules)
            foreach (var r in rules.Children)
                t.Rules.Add(Yaml.Map(r).ToDictionary(x => x.Item1, object (x) => x.Item2));

        return t;
    }

    private static FieldSpec ParseField(string name, YamlNode node, Schema s)
    {
        var f = new FieldSpec
        {
            Name = name,
            Required = Yaml.Bool(Yaml.Get(node, "required")),
            RequiredWhen = Yaml.Str(Yaml.Get(node, "required-when")),
            Type = Yaml.Str(Yaml.Get(node, "type")) ?? "string",
            Of = Yaml.Str(Yaml.Get(node, "of")),
            Ref = Yaml.Str(Yaml.Get(node, "ref")),
            Reciprocal = Yaml.Str(Yaml.Get(node, "reciprocal")),
            Pattern = Yaml.Str(Yaml.Get(node, "pattern")),
            MirrorsSection = Yaml.Str(Yaml.Get(node, "mirrors-section")),
            Notes = Collapse(Yaml.Str(Yaml.Get(node, "notes")))
        };

        var values = Yaml.Get(node, "values");
        switch (values)
        {
            case YamlScalarNode sc when (sc.Value?.StartsWith("$enums.") == true):
                s.Enums.TryGetValue(sc.Value["$enums.".Length..], out f.Values);
                break;
            case YamlSequenceNode:
                f.Values = Yaml.StrList(values);
                break;
        }
        return f;
    }

    // Merge the universal status enum (per-type override) into the type's status field.
    public FieldSpec? EffectiveField(TypeSchema t, string name) =>
        t.Fields.TryGetValue(name, out var tf) ? tf : Universal.GetValueOrDefault(name);

    public IEnumerable<string> KnownKeys(TypeSchema t) =>
        UniversalOrder.Concat(t.FieldOrder).Concat(Reserved).Distinct();

    private static string? Collapse(string? s) =>
        s is null ? null : string.Join(" ", s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}

// ---------------------------------------------------------------------------
// File listing — shared by corpus discovery and the mechanism check
// ---------------------------------------------------------------------------

internal static class GitFiles
{
    // Tracked + untracked-but-not-ignored files, relative and forward-slashed, or null when git is
    // unavailable or this is not a repo (the caller then falls back to Walk). git ls-files respects
    // .gitignore, .git/info/exclude and global excludes, and never lists .git/ itself.
    public static List<string>? Tracked(string root)
    {
        try
        {
            var psi = new ProcessStartInfo("git", "ls-files --cached --others --exclude-standard")
            {
                WorkingDirectory = root,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            using var p = Process.Start(psi);
            if (p is null) return null;
            // Drain both streams concurrently before waiting: reading one to end while the other's
            // pipe buffer fills would deadlock.
            var stdout = p.StandardOutput.ReadToEndAsync();
            _ = p.StandardError.ReadToEndAsync();
            p.WaitForExit();
            if (p.ExitCode != 0) return null;
            return
                [.. stdout.Result.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
        }
        catch
        {
            return null;
        }
    }

    // Fallback for a non-git tree (the test harness assembles one): every file matching `pattern`,
    // relative and forward-slashed, dropping any path at or under one of `skipDirs`.
    public static List<string> Walk(string root, string pattern, params string[] skipDirs) =>
    [
        .. Directory
            .EnumerateFiles(root, pattern, SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(root, f).Replace('\\', '/'))
            .Where(rel => !skipDirs.Any(d => rel == d || rel.StartsWith(d + "/")))
    ];
}

// ---------------------------------------------------------------------------
// Corpus discovery
// ---------------------------------------------------------------------------

internal static class Corpus
{
    private static readonly string[] SkipDirs = [".git", ".idea", ".claude"];

    public static List<string> Discover(string repoRoot, Schema schema, List<string> paths)
    {
        // git ls-files respects .gitignore, .git/info/exclude and global excludes, and never lists
        // .git/ itself — exactly the "respect .gitignore" requirement; the walk is the non-git fallback.
        var files = GitFiles.Tracked(repoRoot) ?? GitFiles.Walk(repoRoot, "*.md", SkipDirs);

        // Type pages at the repo root — adrs.md, services.md, glossary.md, data.md, …
        var typePages = new HashSet<string>(
            schema.ByFolder.Values.Select(t => t.Page).Where(p => !string.IsNullOrEmpty(p)),
            StringComparer.OrdinalIgnoreCase);

        var typeFolders = new HashSet<string>(schema.ByFolder.Keys, StringComparer.OrdinalIgnoreCase);

        var pathFilter = paths.Select(p => p.Replace('\\', '/').TrimEnd('/')).ToList();

        var result = new List<string>();
        foreach (var raw in files)
        {
            var rel = raw.Replace('\\', '/');
            if (!rel.EndsWith(".md", StringComparison.OrdinalIgnoreCase)) continue;
            if (IsExcluded(rel, typePages, typeFolders)) continue;
            if (pathFilter.Count > 0 && !pathFilter.Any(p => rel == p || rel.StartsWith(p + "/"))) continue;
            result.Add(rel);
        }

        return [.. result.OrderBy(r => r, StringComparer.Ordinal)];
    }

    private static bool IsExcluded(string rel, HashSet<string> typePages, HashSet<string> typeFolders)
    {
        var parts = rel.Split('/');
        var top = parts[0];

        if (SkipDirs.Contains(top)) return true;
        if (top is "knowledge-as-code" or "_plan" or "_reports") return true;

        var name = parts[^1];
        if (name.Equals("template.md", StringComparison.OrdinalIgnoreCase)) return true;
        if (name.Equals("INDEX.md", StringComparison.OrdinalIgnoreCase)) return true;

        // Root-level orientation / generated / type pages.
        if (parts.Length == 1)
        {
            if (name.Equals("README.md", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.Equals("CLAUDE.md", StringComparison.OrdinalIgnoreCase)) return true;
            if (typePages.Contains(name)) return true;
        }

        // Only look inside folders that map to a type; everything else (legacy root
        // docs being reconciled elsewhere) is left alone.
        return !typeFolders.Contains(top);
    }
}

// ---------------------------------------------------------------------------
// Portability manifest & mechanism sync state
// ---------------------------------------------------------------------------

// One rule from knowledge-as-code/manifest.yaml: a set of path globs that all resolve to one layer.
internal record ManifestRule(IReadOnlyList<string> Patterns, string Layer);

internal class Manifest
{
    public List<ManifestRule> Rules = [];

    public static Manifest Load(string repoRoot)
    {
        var m = new Manifest();
        var root = Yaml.LoadFile(Path.Combine(repoRoot, "knowledge-as-code", "manifest.yaml"));
        if (Yaml.Get(root, "rules") is YamlSequenceNode rules)
            foreach (var rule in rules.Children)
            {
                var pathNode = Yaml.Get(rule, "path");
                var patterns = pathNode is YamlSequenceNode
                    ? Yaml.StrList(pathNode)
                    : Yaml.Str(pathNode) is { } single ? [single] : [];
                var layer = Yaml.Str(Yaml.Get(rule, "layer"));
                if (patterns.Count > 0 && layer is not null)
                    m.Rules.Add(new ManifestRule(patterns, layer));
            }
        return m;
    }

    // First rule with a matching glob wins, mirroring the manifest's own "evaluated in order"
    // contract. Returns null when nothing matches — which the check reports as an error, since the
    // manifest is meant to resolve every file (its final rule is a catch-all).
    public string? Resolve(string relPath)
    {
        foreach (var rule in Rules)
            foreach (var pattern in rule.Patterns)
                if (Glob.IsMatch(relPath, pattern))
                    return rule.Layer;
        return null;
    }
}

internal record AcceptedDivergence(string Path, string? Reason);

internal class MechanismLock
{
    public string Role = "";
    public string? UpstreamUrl;
    public List<AcceptedDivergence> Accepted = [];

    public static MechanismLock Load(string repoRoot)
    {
        var path = Path.Combine(repoRoot, "knowledge-as-code", "mechanism.lock");
        var lockFile = new MechanismLock();
        if (!File.Exists(path)) return lockFile;

        var root = Yaml.LoadFile(path);
        lockFile.Role = Yaml.Str(Yaml.Get(root, "role")) ?? "";
        var url = Yaml.Str(Yaml.Get(Yaml.Get(root, "upstream"), "url"));
        lockFile.UpstreamUrl = string.IsNullOrWhiteSpace(url) ? null : url;

        if (Yaml.Get(root, "accepted-divergences") is YamlSequenceNode seq)
            foreach (var item in seq.Children)
                if (Yaml.Str(Yaml.Get(item, "path")) is { } p)
                    lockFile.Accepted.Add(new AcceptedDivergence(p, Yaml.Str(Yaml.Get(item, "reason"))));

        return lockFile;
    }
}

// Minimal glob → regex for manifest patterns: `**` spans path segments, `*` stays within one, and a
// leading `**/` also matches at the root. Enough for the manifest's vocabulary, not a full gitignore.
internal static class Glob
{
    private static readonly Dictionary<string, Regex> Cache = [];

    public static bool IsMatch(string path, string pattern) =>
        (Cache.TryGetValue(pattern, out var re) ? re : Cache[pattern] = Compile(pattern)).IsMatch(path);

    private static Regex Compile(string glob)
    {
        var sb = new StringBuilder("^");
        var i = 0;
        if (glob.StartsWith("**/")) { sb.Append("(?:.*/)?"); i = 3; }
        for (; i < glob.Length; i++)
        {
            var c = glob[i];
            if (c == '*')
            {
                if (i + 1 < glob.Length && glob[i + 1] == '*') { sb.Append(".*"); i++; }
                else sb.Append("[^/]*");
            }
            else if (c == '/') sb.Append('/');
            else if ("\\.+?()[]{}|^$".IndexOf(c) >= 0) sb.Append('\\').Append(c);
            else sb.Append(c);
        }
        sb.Append('$');
        return new Regex(sb.ToString(), RegexOptions.CultureInvariant);
    }
}

// The `mechanism --check` engine: resolve every file against the manifest and compare the synced
// layer against the reference. Read-only — it classifies and reports, and never writes.
internal static class MechanismCheck
{
    public static int Run(string localRoot, string refRoot, Manifest manifest, MechanismLock lockFile)
    {
        var accepted = lockFile.Accepted.Select(a => a.Path).ToHashSet(StringComparer.Ordinal);
        var localFiles = ListFiles(localRoot);
        var refFiles = ListFiles(refRoot);

        var drift = new List<string>();           // synced, both present, content differs
        var missingLocally = new List<string>();  // synced, in the reference but not here
        var missingUpstream = new List<string>(); // synced, here but not in the reference
        var unclassified = new List<string>();    // matches no manifest rule
        var resolvedDivergence = new List<string>(); // accepted, but now identical again
        var syncedInStep = 0;
        var forkedShared = 0;
        var forkedDiffer = 0;
        var acceptedActive = 0;

        foreach (var rel in localFiles.Union(refFiles).OrderBy(r => r, StringComparer.Ordinal))
        {
            var layer = manifest.Resolve(rel);
            if (layer is null) { unclassified.Add(rel); continue; }

            var inLocal = localFiles.Contains(rel);
            var inRef = refFiles.Contains(rel);

            switch (layer)
            {
                case "synced":
                    var identical = inLocal && inRef
                        && ReadLf(Path.Combine(localRoot, rel)) == ReadLf(Path.Combine(refRoot, rel));

                    if (accepted.Contains(rel))
                    {
                        if (identical) resolvedDivergence.Add(rel);
                        else acceptedActive++;
                        break;
                    }

                    if (!inRef) missingUpstream.Add(rel);
                    else if (!inLocal) missingLocally.Add(rel);
                    else if (identical) syncedInStep++;
                    else drift.Add(rel);
                    break;

                case "forked":
                    if (inLocal && inRef)
                    {
                        forkedShared++;
                        if (ReadLf(Path.Combine(localRoot, rel)) != ReadLf(Path.Combine(refRoot, rel)))
                            forkedDiffer++;
                    }
                    break;

                // generated / local / ignored: each corpus owns these; nothing to compare.
            }
        }

        Console.WriteLine($"mechanism: comparing the synced layer against {refRoot}");
        var errors = Section("DRIFT — synced files differ from the reference", drift)
            + Section("MISSING LOCALLY — synced files in the reference but not here", missingLocally)
            + Section("MISSING UPSTREAM — synced files here but not in the reference", missingUpstream)
            + Section("UNCLASSIFIED — files matching no manifest rule", unclassified);

        if (resolvedDivergence.Count > 0)
        {
            Console.WriteLine("RESOLVED — accepted divergences that are now identical again (delete them from mechanism.lock):");
            foreach (var p in resolvedDivergence) Console.WriteLine($"  {p}");
        }

        Console.WriteLine(
            $"synced: {syncedInStep} in step, {drift.Count} drifted; "
            + $"forked: {forkedShared} shared ({forkedDiffer} differ, informational); "
            + $"accepted divergences: {acceptedActive}.");

        if (errors > 0)
        {
            Console.Error.WriteLine($"mechanism check failed — {errors} synced-layer problem(s) above.");
            return 1;
        }

        Console.WriteLine("mechanism: synced layer in step.");
        return 0;

        static int Section(string heading, List<string> paths)
        {
            if (paths.Count == 0) return 0;
            Console.Error.WriteLine($"{heading}:");
            foreach (var p in paths) Console.Error.WriteLine($"  {p}");
            return paths.Count;
        }
    }

    private static string ReadLf(string path) => File.ReadAllText(path).Replace("\r\n", "\n");

    // Every tracked (and not-ignored) file, relative and forward-slashed. The walk lets the check
    // run in a non-git tree (the test harness assembles one), skipping only .git.
    private static HashSet<string> ListFiles(string root) =>
        new(GitFiles.Tracked(root) ?? GitFiles.Walk(root, "*", ".git"), StringComparer.Ordinal);
}

// ---------------------------------------------------------------------------
// Parsed document
// ---------------------------------------------------------------------------

internal class LinkRef
{
    public string Target = ""; // raw url/target after the label resolves
    public int Line;
    public bool IsReference; // reference or shortcut link (has a label/definition)
    public string? Label;
}

internal class Doc
{
    public required string Rel;
    public required string Folder;
    public required TypeSchema? Type;
    public required string Text;
    public required MarkdownDocument Ast;

    public YamlMappingNode? Front; // frontmatter mapping (representation model)
    public List<string> FrontKeys = [];
    public int FrontStartLine; // 1-based line where the frontmatter block begins

    public string? H1;
    public int H1Line;
    public List<string> H2 = [];
    public List<LinkRef> Links = [];
    public List<string> DefinedLabels = [];
    public HashSet<string> UsedLabels = new(StringComparer.OrdinalIgnoreCase);
    public List<(string inner, int line)> BareBracketTokens = [];
    public List<LinkRef> RelatedSectionLinks = [];
    public QuoteBlock? YStatement;

    public static Doc? Parse(string rel, string text, Schema schema)
    {
        var pipeline = new MarkdownPipelineBuilder().UseYamlFrontMatter().UsePipeTables().Build();
        var ast = Markdown.Parse(text, pipeline);

        var fmBlock = ast.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
        if (fmBlock is null) return null; // not migrated — caller counts and skips

        var top = rel.Split('/')[0];
        schema.ByFolder.TryGetValue(top, out var type);

        var doc = new Doc { Rel = rel, Folder = top, Type = type, Text = text, Ast = ast };

        // Frontmatter: strip the --- fences and parse with the representation model so
        // key order and scalar quoting survive.
        var yamlText = StripFences(text, fmBlock);
        doc.FrontStartLine = fmBlock.Line + 1;
        try
        {
            var stream = new YamlStream();
            stream.Load(new StringReader(yamlText));
            if (stream.Documents.Count > 0 && stream.Documents[0].RootNode is YamlMappingNode map)
            {
                doc.Front = map;
                foreach (var kv in map.Children)
                    doc.FrontKeys.Add(((YamlScalarNode)kv.Key).Value ?? "");
            }
        }
        catch
        {
            doc.Front = null;
        } // signalled as a parse error downstream

        // Headings.
        foreach (var h in ast.Descendants<HeadingBlock>())
        {
            var txt = Md.PlainText(h.Inline);
            switch (h.Level)
            {
                case 1 when doc.H1 is null:
                    doc.H1 = txt;
                    doc.H1Line = h.Line + 1;
                    break;
                case 2:
                    doc.H2.Add(txt);
                    break;
            }
        }

        // Links (inline + resolved reference/shortcut). Iterating LinkInline naturally
        // excludes code — code spans and fenced/indented blocks carry no LinkInline.
        foreach (var link in ast.Descendants<LinkInline>())
        {
            if (link.IsImage) continue;
            doc.Links.Add(new LinkRef
            {
                Target = link.Url ?? "",
                Line = link.Line + 1,
                IsReference = link.Reference is not null,
                Label = link.Reference?.Label ?? link.Label
            });
            if (link.Reference is not null) doc.UsedLabels.Add(link.Reference.Label ?? "");
        }

        // Link reference definitions.
        foreach (var def in ast.Descendants<LinkReferenceDefinition>())
            if (def.Label is not null)
                doc.DefinedLabels.Add(def.Label);

        // Bare [bracket] tokens left in prose (an undefined shortcut ref like [ADR-0099] renders as
        // literal text). Markdig emits a failed link opener '[' as its own literal inline and leaves
        // the ']' in the following sibling, so the brackets never survive in one literal — the run
        // of consecutive literal siblings must be rejoined before scanning. A real link or code span
        // is a non-literal inline and so naturally breaks the run, which is what keeps code and
        // resolved links excluded. Driven from each leaf block's root inline, because Descendants
        // <ContainerInline> yields only nested containers, never the root that holds the run.
        foreach (var leaf in ast.Descendants<LeafBlock>())
            if (leaf.Inline is not null)
                ScanContainer(leaf.Inline, doc.BareBracketTokens);

        // Y-statement — first block-quote after the H1.
        doc.YStatement = ast.Descendants<QuoteBlock>().FirstOrDefault(q => q.Line > (doc.H1Line - 1));

        // Related section links.
        doc.RelatedSectionLinks = SectionLinks(ast, "Related");

        return doc;
    }

    public string? FrontScalar(string key) => Front is null
        ? null
        : (from kv in Front.Children
            where ((YamlScalarNode)kv.Key).Value == key
            select (kv.Value as YamlScalarNode)?.Value).FirstOrDefault();

    public string TitleText()
    {
        if (Type?.H1Pattern is null || H1 is null) return H1 ?? "";
        var m = System.Text.RegularExpressions.Regex.Match(H1, Type.H1Pattern);
        return m is { Success: true, Groups.Count: > 2 } ? m.Groups[2].Value : H1;
    }

    private static string StripFences(string text, YamlFrontMatterBlock block)
    {
        // block.Span covers the whole frontmatter incl. the --- fences; drop the first
        // and last fence lines and keep the YAML body.
        var slice = text.Substring(block.Span.Start, block.Span.Length);
        var lines = slice.Replace("\r\n", "\n").Split('\n').ToList();
        if (lines.Count > 0 && lines[0].TrimEnd() == "---") lines.RemoveAt(0);
        if (lines.Count > 0 && lines[^1].TrimEnd() == "---") lines.RemoveAt(lines.Count - 1);
        return string.Join("\n", lines);
    }

    // Rejoin the run of consecutive literal children of a container (breaking on any non-literal
    // inline) and scan each run for bare brackets, then recurse into nested containers (emphasis, a
    // link label). Only direct children are joined at each level, so a literal is scanned once.
    private static void ScanContainer(ContainerInline container, List<(string, int)> outp)
    {
        var run = new StringBuilder();
        var segments = new List<(int offset, int line)>();

        void Flush()
        {
            if (run.Length > 0) ScanBrackets(run.ToString(), segments, outp);
            run.Clear();
            segments.Clear();
        }

        for (var child = container.FirstChild; child is not null; child = child.NextSibling)
            if (child is LiteralInline lit)
            {
                segments.Add((run.Length, lit.Line + 1));
                run.Append(lit.Content.ToString());
            }
            else
            {
                Flush();
                if (child is ContainerInline nested) ScanContainer(nested, outp);
            }

        Flush();
    }

    private static void ScanBrackets(string s, List<(int offset, int line)> segments, List<(string, int)> outp)
    {
        for (var i = 0; i < s.Length; i++)
        {
            if (s[i] != '[') continue;
            var close = s.IndexOf(']', i + 1);
            if (close < 0) break;
            var inner = s.Substring(i + 1, close - i - 1);
            // A real inline/collapsed/full link would not survive as a literal, so a
            // literal '[x]' is a candidate undefined reference. Skip if immediately
            // followed by '(' or '[' (still part of a link the AST handled elsewhere).
            var next = close + 1 < s.Length ? s[close + 1] : ' ';
            // Map the line from the closing bracket: Markdig gives a failed link-opener '[' its own
            // literal with no source line (0), while the ']' always sits in a real-text literal that
            // carries the correct line.
            if (inner.Length > 0 && inner.IndexOf(']') < 0 && next != '(' && next != '[' && next != ':')
                outp.Add((inner, LineAt(segments, close)));
            i = close;
        }
    }

    // The source line of the literal segment that character `index` of a rejoined run falls in.
    private static int LineAt(List<(int offset, int line)> segments, int index)
    {
        var line = segments.Count > 0 ? segments[0].line : 1;
        foreach (var (offset, l) in segments)
        {
            if (offset > index) break;
            line = l;
        }

        return line;
    }

    private static List<LinkRef> SectionLinks(MarkdownDocument ast, string sectionTitle)
    {
        var result = new List<LinkRef>();
        var inSection = false;
        foreach (var block in ast)
        {
            if (block is HeadingBlock h)
            {
                inSection = h.Level switch
                {
                    2 => string.Equals(Md.PlainText(h.Inline), sectionTitle, StringComparison.OrdinalIgnoreCase),
                    < 2 => false,
                    _ => inSection
                };
            }

            if (inSection && block is not HeadingBlock)
                result.AddRange(from link in block.Descendants<LinkInline>()
                    where !link.IsImage
                    select new LinkRef
                        { Target = link.Url ?? "", Line = link.Line + 1, Label = link.Reference?.Label ?? link.Label });
        }

        return result;
    }
}

// ---------------------------------------------------------------------------
// The checks
// ---------------------------------------------------------------------------

internal static class Validator
{
    public static void CheckDocument(Doc d, Schema schema, string repoRoot, List<Finding> f)
    {
        if (d.Type is null)
        {
            Err("type", $"folder '{d.Folder}' has no schema.");
            return;
        }

        var t = d.Type;

        // -- frontmatter parses --
        if (d.Front is null)
        {
            Err("frontmatter-parses", "frontmatter is not a valid YAML mapping.");
            return;
        }

        var present = new Dictionary<string, YamlNode>();
        foreach (var kv in d.Front.Children)
            present[((YamlScalarNode)kv.Key).Value ?? ""] = kv.Value;

        // -- unknown keys --
        var known = new HashSet<string>(schema.KnownKeys(t), StringComparer.Ordinal);
        foreach (var k in d.FrontKeys)
            if (!known.Contains(k))
                Err("unknown-key", $"unknown frontmatter key '{k}'.", d.FrontStartLine);

        // -- key order --
        // The schema specifies order across two files (_universal + the type), sharing
        // the `status` key. Rather than invent one arbitrary total order, enforce that
        // the actual order is a topological extension of both declared chains: every
        // pair the schema orders must hold; genuinely unconstrained pairs are free.
        CheckKeyOrder(d, t, schema, Err);

        // -- required fields (universal + type), incl. required-when --
        foreach (var name in schema.UniversalOrder.Concat(t.FieldOrder).Distinct())
        {
            var spec = schema.EffectiveField(t, name);
            if (spec is null) continue;
            var req = spec.Required || RequiredWhenHolds(spec.RequiredWhen, present);
            var absent = !present.ContainsKey(name) || IsAbsentValue(present[name]);
            if (req && absent)
            {
                var why = spec.Required ? "" : $" (required when {spec.RequiredWhen})";
                Err("required-field", $"missing required field '{name}'{why}.", d.FrontStartLine);
            }
        }

        // -- per-field value checks --
        foreach (var (name, node) in present)
        {
            var spec = schema.EffectiveField(t, name);
            if (spec is null) continue; // unknown key already reported

            // absent values must be bare keys, never null / ~ / "" / —
            if (IsAbsentValue(node))
            {
                if (!IsBareKey(node))
                    Err("bare-key",
                        $"'{name}' is absent but not a bare key — use '{name}:' with no value (not null, ~, \"\", or —).",
                        Line(node, d));
                continue;
            }

            switch (spec.Type)
            {
                case "date": CheckDate(name, node, d, Err); break;
                case "enum": CheckEnum(name, node, spec, d, Err); break;
                case "list": CheckList(name, node, spec, d, Err); break;
            }
        }

        // -- tier matches type --
        if (present.TryGetValue("tier", out var tierNode) && Scalar(tierNode) is { } tier && tier != t.Tier)
            Err("tier-matches-type", $"tier '{tier}' does not match the '{t.TypeName}' type tier '{t.Tier}'.",
                Line(tierNode, d));

        // -- id: prefix, width, matches filename number --
        CheckId(d, t, present, Err);

        // -- filename pattern + slug length --
        CheckFilename(d, t, Err);

        // -- H1 pattern + number matches id --
        CheckH1(d, t, present, Err);

        // -- required sections --
        foreach (var sec in t.RequiredSections)
            if (!d.H2.Any(h => string.Equals(h, sec, StringComparison.OrdinalIgnoreCase)))
                Err("required-section", $"missing required section '## {sec}'.");

        // -- links resolve --
        CheckLinks(d, repoRoot, Err, Warn);

        // -- related mirrors ## Related --
        CheckMirrorsSection(d, t, schema, present, Err);

        // -- warning rules --
        CheckWarnings(d, t, Warn);
        return;

        void Warn(string check, string msg, int? line = null) =>
            f.Add(new Finding(d.Rel, line, Sev.Warning, check, msg));

        void Err(string check, string msg, int? line = null) => f.Add(new Finding(d.Rel, line, Sev.Error, check, msg));
    }

    public static void CheckCorpus(List<Doc> docs, Schema schema, List<Finding> f)
    {
        // id uniqueness across the whole wiki.
        var byId = new Dictionary<string, Doc>(StringComparer.OrdinalIgnoreCase);
        foreach (var d in docs)
        {
            var id = FrontScalar(d, "id");
            if (id is null) continue;
            if (byId.TryGetValue(id, out var other))
                f.Add(new Finding(d.Rel, d.FrontStartLine, Sev.Error, "id-unique",
                    $"id '{id}' is also used by {other.Rel}."));
            else
                byId[id] = d;
        }

        // reciprocal fields (e.g. supersedes / superseded-by).
        foreach (var d in docs)
        {
            if (d.Type is null) continue;
            foreach (var name in d.Type.FieldOrder)
            {
                var spec = d.Type.Fields[name];
                if (spec.Reciprocal is null || spec.Ref is null) continue;
                foreach (var targetId in FrontIdList(d, name))
                {
                    if (!byId.TryGetValue(targetId, out var target))
                    {
                        f.Add(new Finding(d.Rel, d.FrontStartLine, Sev.Error, "reciprocal",
                            $"'{name}' points at '{targetId}', which does not exist."));
                        continue;
                    }

                    var back = FrontIdList(target, spec.Reciprocal);
                    var selfId = FrontScalar(d, "id");
                    if (!back.Any(b => string.Equals(b, selfId, StringComparison.OrdinalIgnoreCase)))
                        f.Add(new Finding(d.Rel, d.FrontStartLine, Sev.Error, "reciprocal",
                            $"'{name}: {targetId}' is not reciprocated — {target.Rel} must list '{spec.Reciprocal}: {selfId}'."));
                }
            }
        }
    }

    // -- helpers for individual checks --

    private static void CheckDate(string name, YamlNode node, Doc d, Action<string, string, int?> err)
    {
        var sc = node as YamlScalarNode;
        var v = sc?.Value ?? "";
        var quoted = sc?.Style is ScalarStyle.DoubleQuoted or ScalarStyle.SingleQuoted;
        if (!quoted)
            err("date-quoted", $"'{name}' date must be quoted, e.g. \"{v}\".", Line(node, d));
        if (!IsIsoDate(v))
            err("date-format", $"'{name}' must be a YYYY-MM-DD date, got '{v}'.", Line(node, d));
    }

    private static void CheckEnum(string name, YamlNode node, FieldSpec spec, Doc d, Action<string, string, int?> err)
    {
        var v = Scalar(node);
        if (v is null)
        {
            err("enum", $"'{name}' must be a scalar.", Line(node, d));
            return;
        }

        if (spec.Values is not null && !spec.Values.Contains(v))
            err("enum", $"'{name}' value '{v}' is not one of: {string.Join(", ", spec.Values)}.", Line(node, d));
        if (v != v.ToLowerInvariant())
            err("enum-lowercase", $"'{name}' enum value '{v}' must be lowercase.", Line(node, d));
    }

    private static void CheckList(string name, YamlNode node, FieldSpec spec, Doc d, Action<string, string, int?> err)
    {
        if (node is not YamlSequenceNode seq)
        {
            err("list", $"'{name}' must be a YAML sequence.", Line(node, d));
            return;
        }

        foreach (var item in seq.Children)
        {
            var v = Scalar(item);
            if (spec.Of == "id" && v is not null && !LooksLikeId(v))
                err("id-format", $"'{name}' entry '{v}' is not a valid id.", Line(item, d));
        }
    }

    private static void CheckKeyOrder(Doc d, TypeSchema t, Schema schema, Action<string, string, int?> err)
    {
        // All ordered pairs within each declared chain (transitive, so an absent
        // intermediate key does not drop a constraint between its neighbours).
        var edges = new HashSet<(string, string)>();

        AddChain(schema.UniversalOrder);
        AddChain(t.FieldOrder);

        var pos = new Dictionary<string, int>();
        for (var i = 0; i < d.FrontKeys.Count; i++)
            if (!pos.ContainsKey(d.FrontKeys[i]))
                pos[d.FrontKeys[i]] = i;

        foreach (var (a, b) in edges)
            if (pos.TryGetValue(a, out var pa) && pos.TryGetValue(b, out var pb) && pa > pb)
                err("key-order", $"'{a}' must appear before '{b}' in the frontmatter.", d.FrontStartLine);
        return;

        void AddChain(List<string> chain)
        {
            for (var i = 0; i < chain.Count; i++)
            for (var j = i + 1; j < chain.Count; j++)
                edges.Add((chain[i], chain[j]));
        }
    }

    private static void CheckId(Doc d, TypeSchema t, Dictionary<string, YamlNode> present,
        Action<string, string, int?> err)
    {
        if (!present.TryGetValue("id", out var idNode)) return;
        var id = Scalar(idNode);
        if (id is null) return;
        var expectPrefix = t.IdPrefix + "-";
        if (!id.StartsWith(expectPrefix, StringComparison.Ordinal))
        {
            err("id-prefix", $"id '{id}' must start with '{expectPrefix}'.", Line(idNode, d));
            return;
        }

        var numPart = id[expectPrefix.Length..];
        var fileNum = FilenameNumber(d.Rel);
        if (t.IdStyle == "numbered")
        {
            if (numPart.Length != t.IdWidth || !numPart.All(char.IsDigit))
                err("id-format", $"id '{id}' must be '{expectPrefix}' followed by {t.IdWidth} digits.",
                    Line(idNode, d));
            else if (fileNum is not null && numPart != fileNum)
                err("id-matches-filename", $"id '{id}' number does not match filename number '{fileNum}'.",
                    Line(idNode, d));
        }
    }

    private static void CheckFilename(Doc d, TypeSchema t, Action<string, string, int?> err)
    {
        var name = Path.GetFileName(d.Rel);
        if (t.FilenamePattern is not null && !System.Text.RegularExpressions.Regex.IsMatch(name, t.FilenamePattern))
            err("filename-pattern", $"filename '{name}' does not match {t.FilenamePattern}.", null);
        var slug = name;
        if (slug.EndsWith(".md")) slug = slug[..^3];
        var dash = slug.IndexOf('-');
        if (t.IdStyle == "numbered" && dash >= 0 && slug[..dash].All(char.IsDigit))
            slug = slug[(dash + 1)..];
        if (slug.Length > t.SlugMax)
            err("slug-length", $"slug '{slug}' is {slug.Length} characters; the limit is {t.SlugMax}.", null);
    }

    private static void CheckH1(Doc d, TypeSchema t, Dictionary<string, YamlNode> present,
        Action<string, string, int?> err)
    {
        if (t.H1Pattern is null) return;
        if (d.H1 is null)
        {
            err("h1", "document has no H1.", 1);
            return;
        }

        var m = System.Text.RegularExpressions.Regex.Match(d.H1, t.H1Pattern);
        if (!m.Success)
        {
            err("h1-pattern", $"H1 '{d.H1}' does not match {t.H1Pattern}.", d.H1Line);
            return;
        }

        if (t.TitleMatchesId && m.Groups.Count > 1 && present.TryGetValue("id", out var idNode))
        {
            var num = m.Groups[1].Value;
            var fileNum = FilenameNumber(d.Rel);
            if (fileNum is not null && num != fileNum)
                err("h1-matches-id", $"H1 number '{num}' does not match filename number '{fileNum}'.", d.H1Line);
        }
    }

    private static void CheckLinks(Doc d, string repoRoot, Action<string, string, int?> err,
        Action<string, string, int?> warn)
    {
        foreach (var link in d.Links)
        {
            var target = link.Target;
            if (string.IsNullOrEmpty(target)) continue;
            if (IsExternal(target) || target.StartsWith('#')) continue;
            if (!ResolveTarget(repoRoot, d.Rel, target))
                err("link-resolves", $"link target '{target}' does not resolve.", link.Line);
        }

        // undefined shortcut/reference labels left as literal '[x]'
        var defined = new HashSet<string>(d.DefinedLabels, StringComparer.OrdinalIgnoreCase);
        foreach (var (inner, line) in d.BareBracketTokens)
        {
            if (defined.Contains(inner)) continue; // a genuine reference that resolved
            if (inner.StartsWith("ADR-", StringComparison.OrdinalIgnoreCase) && inner.Skip(4).All(char.IsDigit))
                err("undefined-label", $"reference '[{inner}]' has no link definition.", line);
            else
                warn("bracket-literal",
                    $"'[{inner}]' looks like a reference but has no definition (or use an inline link).", line);
        }

        // unused definitions
        foreach (var label in d.DefinedLabels.Distinct(StringComparer.OrdinalIgnoreCase))
            if (!d.UsedLabels.Contains(label))
                warn("unused-definition", $"link definition '[{label}]' is never referenced.", null);
    }

    private static void CheckMirrorsSection(Doc d, TypeSchema t, Schema schema, Dictionary<string, YamlNode> present,
        Action<string, string, int?> err)
    {
        foreach (var name in t.FieldOrder)
        {
            var spec = t.Fields[name];
            if (spec.MirrorsSection is null || spec.Ref is null) continue;
            if (!schema.ByFolder.TryGetValue(spec.Ref, out var refType)) continue;

            var inFront = new HashSet<string>(FrontIdList(d, name), StringComparer.OrdinalIgnoreCase);
            var inSection = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var link in d.RelatedSectionLinks)
            {
                var id = IdFromLink(link, refType);
                if (id is not null) inSection.Add(id);
            }

            foreach (var id in inFront.Except(inSection))
                err("related-matches-section",
                    $"'{name}' lists '{id}' but it is not referenced in the '## {spec.MirrorsSection}' section.",
                    d.FrontStartLine);
            foreach (var id in inSection.Except(inFront))
                err("related-matches-section",
                    $"the '## {spec.MirrorsSection}' section references '{id}' but '{name}' does not list it.",
                    d.FrontStartLine);
        }
    }

    private static void CheckWarnings(Doc d, TypeSchema t, Action<string, string, int?> warn)
    {
        foreach (var rule in t.Rules)
        {
            var ruleId = rule.TryGetValue("id", out var rid) ? rid.ToString() : null;
            var severity = rule.TryGetValue("severity", out var sv) ? sv.ToString() : null;
            if (severity != "warning") continue;

            switch (ruleId)
            {
                case "y-statement-present":
                {
                    var max = rule.TryGetValue("max-words", out var mw) && int.TryParse(mw.ToString(), out var m)
                        ? m
                        : 60;
                    if (d.YStatement is null)
                        warn("y-statement", "no Y-statement block-quote follows the H1.", d.H1Line);
                    else
                    {
                        var words = Md.PlainText(d.YStatement)
                            .Split([' ', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries).Length;
                        if (words > max)
                            warn("y-statement", $"Y-statement is {words} words; keep it under {max}.",
                                d.YStatement.Line + 1);
                    }

                    break;
                }
                case "alternatives-have-verdicts":
                {
                    foreach (var (text, line) in AlternativeBullets(d))
                        if (!HasVerdict(text))
                            warn("alternatives-verdict",
                                $"Alternatives Considered bullet has no verdict: \"{Trim(text)}\".", line);
                    break;
                }
            }
        }
    }

    private static IEnumerable<(string text, int line)> AlternativeBullets(Doc d)
    {
        var inSection = false;
        foreach (var block in d.Ast)
        {
            if (block is HeadingBlock h)
            {
                inSection = h.Level switch
                {
                    2 => string.Equals(Md.PlainText(h.Inline), "Alternatives Considered",
                        StringComparison.OrdinalIgnoreCase),
                    < 2 => false,
                    _ => inSection
                };
                continue;
            }

            if (inSection && block is ListBlock list)
                foreach (var item in list)
                    if (item is ListItemBlock li)
                    {
                        var text = string.Join(" ", li.Descendants<LiteralInline>().Select(x => x.Content.ToString()));
                        yield return (text, li.Line + 1);
                    }
        }
    }

    private static bool HasVerdict(string text)
    {
        var t = text.ToLowerInvariant();
        // An explicit verdict word, or a contrastive / negative-outcome cue that shows
        // the option was weighed to a conclusion. A genuinely open bullet ("we could
        // also use X") carries none of these and is what this warning is for.
        string[] markers =
        [
            "reject", "accept", "chosen", "choose", "defer", "declined", "discarded", "adopted",
            "not for this adr", "no real alternative", "not adopted", "not pursued", "not chosen",
            "not relevant", "not worth", "no need", "unnecessary", "ruled out", "set aside",
            "we use", "instead", "however", "but ", "overkill", "heavier", "too ", "revisit"
        ];
        return markers.Any(t.Contains);
    }

    // -- small utilities --

    private static bool RequiredWhenHolds(string? expr, Dictionary<string, YamlNode> present)
    {
        if (string.IsNullOrWhiteSpace(expr)) return false;
        var parts = expr.Split("==", 2);
        if (parts.Length != 2) return false;
        var field = parts[0].Trim();
        var val = parts[1].Trim();
        return present.TryGetValue(field, out var node) && Scalar(node) == val;
    }

    private static bool IsAbsentValue(YamlNode node) =>
        node switch
        {
            YamlScalarNode sc => string.IsNullOrEmpty(sc.Value) || sc.Value is "~" or "null" or "Null" or "NULL",
            YamlSequenceNode seq => seq.Children.Count == 0,
            _ => false
        };

    private static bool IsBareKey(YamlNode node)
        => node is YamlScalarNode { Style: ScalarStyle.Plain } sc && string.IsNullOrEmpty(sc.Value);

    private static bool IsIsoDate(string v)
        => v.Length == 10 && v[4] == '-' && v[7] == '-'
           && v[..4].All(char.IsDigit) && v[5..7].All(char.IsDigit) && v[8..].All(char.IsDigit);

    private static bool LooksLikeId(string v) => v.Contains('-') && v == v.ToLowerInvariant();

    private static string? Scalar(YamlNode node) => (node as YamlScalarNode)?.Value;

    private static int? Line(YamlNode node, Doc d)
        => node.Start.Line > 0 ? (int)node.Start.Line + d.FrontStartLine - 1 : d.FrontStartLine;

    private static string? FrontScalar(Doc d, string key) =>
        d.Front is null
            ? null
            : (from kv in d.Front.Children where ((YamlScalarNode)kv.Key).Value == key select Scalar(kv.Value))
            .FirstOrDefault();

    private static List<string> FrontIdList(Doc d, string key)
    {
        var result = new List<string>();
        if (d.Front is null) return result;
        foreach (var kv in d.Front.Children)
            if (((YamlScalarNode)kv.Key).Value == key)
            {
                if (kv.Value is YamlSequenceNode seq)
                    result.AddRange(seq.Children.Select(Scalar).OfType<string>());
                else if (Scalar(kv.Value) is { Length: > 0 } s) result.Add(s);
            }

        return result;
    }

    private static string? FilenameNumber(string rel)
    {
        var name = Path.GetFileName(rel);
        var i = 0;
        while (i < name.Length && char.IsDigit(name[i])) i++;
        return i > 0 ? name[..i] : null;
    }

    private static string? IdFromLink(LinkRef link, TypeSchema refType)
    {
        // Resolve the link's target filename to the ref type's id, e.g. 0007-…md -> adr-0007.
        var target = link.Target;
        var hash = target.IndexOf('#');
        if (hash >= 0) target = target[..hash];
        var file = target.Split('/').LastOrDefault() ?? "";
        var i = 0;
        while (i < file.Length && char.IsDigit(file[i])) i++;
        return i == refType.IdWidth ? $"{refType.IdPrefix}-{file[..i]}" : null;
    }

    private static bool IsExternal(string t)
        => t.StartsWith("http://") || t.StartsWith("https://") || t.StartsWith("mailto:") || t.StartsWith("tel:");

    private static bool ResolveTarget(string repoRoot, string fromRel, string target)
    {
        var hash = target.IndexOf('#');
        if (hash >= 0) target = target[..hash];
        var q = target.IndexOf('?');
        if (q >= 0) target = target[..q];
        if (target.Length == 0) return true; // pure fragment

        string basePath;
        if (target.StartsWith('/'))
            basePath = Path.Combine(repoRoot, target.TrimStart('/'));
        else
        {
            var fromDir = Path.GetDirectoryName(Path.Combine(repoRoot, fromRel)) ?? repoRoot;
            basePath = Path.GetFullPath(Path.Combine(fromDir, target));
        }

        basePath = basePath.Replace('\\', '/');

        return File.Exists(basePath)
               || File.Exists(basePath + ".md") // ADO resolves links with .md omitted
               || Directory.Exists(basePath);
    }

    private static string Trim(string s) => s.Length > 60 ? s[..57] + "…" : s;
}

// ---------------------------------------------------------------------------
// Markdown helpers
// ---------------------------------------------------------------------------

internal static class Md
{
    public static string PlainText(ContainerInline? container)
    {
        if (container is null) return "";
        var sb = new StringBuilder();
        Walk(container, sb);
        return sb.ToString().Trim();
    }

    public static string PlainText(LeafBlock? block)
        => block?.Inline is null ? "" : PlainText(block.Inline);

    public static string PlainText(QuoteBlock quote)
    {
        var sb = new StringBuilder();
        foreach (var para in quote.Descendants<ParagraphBlock>())
            if (para.Inline is not null)
            {
                Walk(para.Inline, sb);
                sb.Append(' ');
            }

        return sb.ToString().Trim();
    }

    private static void Walk(Inline inline, StringBuilder sb)
    {
        switch (inline)
        {
            case LiteralInline lit: sb.Append(lit.Content.ToString()); break;
            case CodeInline code: sb.Append(code.Content); break;
            case LineBreakInline: sb.Append(' '); break;
        }

        if (inline is ContainerInline c)
            foreach (var child in c)
                Walk(child, sb);
    }
}

// ---------------------------------------------------------------------------
// YAML helpers over the representation model
// ---------------------------------------------------------------------------

internal static class Yaml
{
    public static YamlNode LoadFile(string path)
    {
        var stream = new YamlStream();
        stream.Load(new StringReader(File.ReadAllText(path)));
        return stream.Documents.Count > 0 ? stream.Documents[0].RootNode : new YamlMappingNode();
    }

    public static YamlNode? Get(YamlNode? node, string key)
    {
        if (node is YamlMappingNode map)
            foreach (var kv in map.Children)
                if (kv.Key is YamlScalarNode k && k.Value == key)
                    return kv.Value;
        return null;
    }

    public static IEnumerable<(string, YamlNode)> Map(YamlNode? node)
    {
        if (node is YamlMappingNode map)
            foreach (var kv in map.Children)
                yield return (((YamlScalarNode)kv.Key).Value ?? "", kv.Value);
    }

    public static string? Str(YamlNode? node) => (node as YamlScalarNode)?.Value;

    public static bool Bool(YamlNode? node)
        => (node as YamlScalarNode)?.Value?.ToLowerInvariant() is "true" or "yes";

    public static int Int(YamlNode? node, int fallback)
        => int.TryParse((node as YamlScalarNode)?.Value, out var v) ? v : fallback;

    public static List<string> StrList(YamlNode? node)
    {
        var result = new List<string>();
        if (node is YamlSequenceNode seq)
            foreach (var item in seq.Children)
                if (item is YamlScalarNode s)
                    result.Add(s.Value ?? "");
        return result;
    }
}