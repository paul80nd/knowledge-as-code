#:property TargetFramework=net10.0
#:property Nullable=enable

// kac-tests — the golden-file test suite for kac.
//
// Each fixture under .tooling/tests/fixtures/<scenario>/ is a deliberately-broken (or deliberately-clean)
// mini-wiki. The runner assembles a throwaway repo root from the REAL schema plus the fixture's
// corpus, runs `kac validate --json` against it, and diffs the findings against the scenario's
// committed golden (expected.json). Because the golden captures the WHOLE findings set for the
// corpus, an accidental extra finding shows up as a diff — that is the point.
//
//   dotnet run .tooling/kac-tests.cs                 # run every scenario, fail on any mismatch
//   dotnet run .tooling/kac-tests.cs -- clean        # run only scenarios whose name contains "clean"
//   dotnet run .tooling/kac-tests.cs -- --update     # regenerate every golden from current kac output
//
// Testing against the real schema (copied in per run, never a stale committed copy) means the
// suite exercises the production rules: a schema change that alters behaviour surfaces here.
//
// Exit codes: 0 all scenarios matched (or goldens updated); 1 a mismatch, a missing golden, a
// golden referencing a check id kac no longer emits, or a reachable check with no fixture — the
// coverage gate: a new rule cannot ship without a scenario exercising it.

using System.Diagnostics;
using System.Text;
using System.Text.Json;

var repoRoot = FindRepoRoot(Directory.GetCurrentDirectory());
if (repoRoot is null)
{
    Console.Error.WriteLine("kac-tests: could not locate the repo root (no .schema above the cwd).");
    return 2;
}

var kac = Path.Combine(repoRoot, ".tooling", "kac.cs");
var schemaDir = Path.Combine(repoRoot, ".schema");
var manifestFile = Path.Combine(repoRoot, "knowledge-as-code", "manifest.yaml");
var fixturesDir = Path.Combine(repoRoot, ".tooling", "tests", "fixtures");

var rawArgs = args.ToList();
var update = rawArgs.RemoveAll(a => a is "--update" or "-u") > 0;
var filters = rawArgs; // remaining positional args narrow which scenarios run

if (!Directory.Exists(fixturesDir))
{
    Console.Error.WriteLine($"kac-tests: no fixtures directory at {Rel(repoRoot, fixturesDir)}.");
    return 2;
}

var scenarios = Directory.EnumerateDirectories(fixturesDir)
    .Where(d => Directory.Exists(Path.Combine(d, "corpus")))
    .Where(d => filters.Count == 0 || filters.Any(f => Path.GetFileName(d).Contains(f, StringComparison.OrdinalIgnoreCase)))
    .OrderBy(d => d, StringComparer.Ordinal)
    .ToList();

if (scenarios.Count == 0)
{
    Console.Error.WriteLine("kac-tests: no matching scenarios.");
    return 2;
}

var coveredChecks = new HashSet<string>(StringComparer.Ordinal);
var failures = new List<string>();

foreach (var scenario in scenarios)
{
    var name = Path.GetFileName(scenario);
    var corpusDir = Path.Combine(scenario, "corpus");
    // A scenario's `mode` file selects what is asserted; absent means the default, a validate diff.
    //   validate     run `validate --json`, diff findings against expected.json (contributes coverage)
    //   index        run `index --check`, assert the committed generated files are fresh (exit 0)
    //   index-stale  run `index --check` against a hand-broken corpus, assert staleness is caught (exit 1)
    //   mechanism    run `mechanism --check` (corpus/ as local, reference/ as the source); assert
    //                the synced paths in expected-drift.txt are flagged (exit 1), or in step (exit 0)
    var modePath = Path.Combine(scenario, "mode");
    var mode = File.Exists(modePath) ? File.ReadAllText(modePath).Trim() : "validate";

    try
    {
        switch (mode)
        {
            case "validate":
                RunValidateScenario(name, scenario, corpusDir);
                break;
            case "index":
                RunIndexScenario(name, scenario, mustBeStale: false);
                break;
            case "index-stale":
                RunIndexScenario(name, scenario, mustBeStale: true);
                break;
            case "mechanism":
                RunMechanismScenario(name, scenario, corpusDir);
                break;
            default:
                failures.Add(name);
                Console.WriteLine($"ERROR  {name}  — unknown mode '{mode}'");
                break;
        }
    }
    catch (Exception ex)
    {
        failures.Add(name);
        Console.WriteLine($"ERROR  {name}\n       {ex.Message}");
    }
}

void RunValidateScenario(string name, string scenario, string corpusDir)
{
    var expectedPath = Path.Combine(scenario, "expected.json");
    var (actualJson, actualExit) = RunValidate(kac, schemaDir, corpusDir);
    var actual = ParseFindings(actualJson);
    foreach (var f in actual.Findings) coveredChecks.Add(f.Check);

    if (update)
    {
        File.WriteAllText(expectedPath, actualJson.TrimEnd('\n') + "\n");
        Console.WriteLine($"UPDATE {name}  ({actual.Findings.Count} finding(s))");
        return;
    }

    if (!File.Exists(expectedPath))
    {
        failures.Add(name);
        Console.WriteLine($"MISSING {name}  — no golden. Run: dotnet run .tooling/kac-tests.cs -- --update {name}");
        return;
    }

    var diff = Diff(ParseFindings(File.ReadAllText(expectedPath)), actual);

    // Exit-code contract: validate exits 1 exactly when it produced an error-severity finding, 0
    // otherwise (warnings never fail). The feature tests assert the findings but bypass the CLI, so
    // this is the one place the process exit code of `validate` is checked.
    var expectedExit = actual.Findings.Any(f => f.Severity == "error") ? 1 : 0;
    if (actualExit != expectedExit)
        diff.Add($"exit code {actualExit}, expected {expectedExit} ({(expectedExit == 1 ? "errors present" : "no errors")})");

    if (diff.Count == 0)
    {
        Console.WriteLine($"ok     {name}  ({actual.Findings.Count} finding(s))");
    }
    else
    {
        failures.Add(name);
        Console.WriteLine($"FAIL   {name}");
        foreach (var line in diff) Console.WriteLine($"         {line}");
    }
}

// index (mustBeStale=false): the committed corpus already holds fresh generated files; `--update`
// regenerates them, a normal run asserts `index --check` finds them fresh (exit 0). The golden is
// the committed INDEX.md / <type>.md itself — reviewable in git, kept fresh by `--update`.
// index-stale (mustBeStale=true): the corpus is deliberately stale; the run asserts `--check` flags
// it (exit 1) and, if expected-stale.txt is present, names those files. `--update` leaves it alone.
void RunIndexScenario(string name, string scenario, bool mustBeStale)
{
    var corpusDir = Path.Combine(scenario, "corpus");

    if (update && !mustBeStale)
    {
        RegenerateIndex(kac, schemaDir, corpusDir);
        var (rexit, _) = RunIndex(kac, schemaDir, corpusDir, check: true);
        Console.WriteLine(rexit == 0
            ? $"UPDATE {name}  (index regenerated, fresh)"
            : $"UPDATE {name}  — WARNING: still stale after regen");
        return;
    }

    if (update)
    {
        Console.WriteLine($"UPDATE {name}  (stale fixture — nothing to regenerate)");
        return;
    }

    var (exit, output) = RunIndex(kac, schemaDir, corpusDir, check: true);

    if (mustBeStale)
    {
        if (exit == 0)
        {
            failures.Add(name);
            Console.WriteLine($"FAIL   {name}  — expected index --check to detect staleness, but it exited 0");
            return;
        }

        var expectedStale = Path.Combine(scenario, "expected-stale.txt");
        var missing = File.Exists(expectedStale)
            ? File.ReadAllLines(expectedStale).Select(l => l.Trim()).Where(l => l.Length > 0)
                .Where(l => !output.Contains(l)).ToList()
            : [];
        if (missing.Count > 0)
        {
            failures.Add(name);
            Console.WriteLine($"FAIL   {name}  — stale detected but these files were not named: {string.Join(", ", missing)}");
        }
        else
        {
            Console.WriteLine($"ok     {name}  (staleness detected)");
        }

        return;
    }

    if (exit == 0)
    {
        Console.WriteLine($"ok     {name}  (generated output fresh)");
    }
    else
    {
        failures.Add(name);
        Console.WriteLine($"FAIL   {name}  — committed generated files do not match the generator. Run: dotnet run .tooling/kac-tests.cs -- --update {name}");
        foreach (var l in output.Split('\n').Where(l => l.Trim().Length > 0)) Console.WriteLine($"         {l}");
    }
}

// mechanism: assemble a local corpus (corpus/) and a reference source (reference/), each over the
// real schema + manifest, then run `mechanism --check --against <reference>`. expected-drift.txt
// lists the synced paths that must be named in the failure output; absent/empty means "expect in
// step" (exit 0). Accepted divergences and forked differences are exercised by the fixture but must
// not fail the run — the golden is the exit code plus the named synced paths, not free-form output.
void RunMechanismScenario(string name, string scenario, string corpusDir)
{
    var referenceDir = Path.Combine(scenario, "reference");
    if (!Directory.Exists(referenceDir))
    {
        failures.Add(name);
        Console.WriteLine($"ERROR  {name}  — no reference/ tree in the fixture");
        return;
    }

    if (update)
    {
        Console.WriteLine($"UPDATE {name}  (mechanism scenario — nothing to regenerate)");
        return;
    }

    var expectedFile = Path.Combine(scenario, "expected-drift.txt");
    var expected = File.Exists(expectedFile)
        ? File.ReadAllLines(expectedFile).Select(l => l.Trim()).Where(l => l.Length > 0).ToList()
        : [];

    var localTemp = AssembleMechanismTemp(schemaDir, manifestFile, corpusDir);
    var refTemp = AssembleMechanismTemp(schemaDir, manifestFile, referenceDir);
    try
    {
        var (stdout, stderr, exit) = Run(localTemp, "dotnet", "run", kac, "--", "mechanism", "--check", "--against", refTemp);
        var output = stderr + stdout;

        if (expected.Count == 0)
        {
            if (exit == 0)
            {
                Console.WriteLine($"ok     {name}  (synced layer in step)");
            }
            else
            {
                failures.Add(name);
                Console.WriteLine($"FAIL   {name}  — expected the synced layer in step (exit 0), got exit {exit}");
                foreach (var l in output.Split('\n').Where(l => l.Trim().Length > 0)) Console.WriteLine($"         {l}");
            }

            return;
        }

        if (exit == 0)
        {
            failures.Add(name);
            Console.WriteLine($"FAIL   {name}  — expected drift (exit 1) but the check passed");
            return;
        }

        var missing = expected.Where(l => !output.Contains(l)).ToList();
        if (missing.Count == 0)
        {
            Console.WriteLine($"ok     {name}  (drift detected)");
        }
        else
        {
            failures.Add(name);
            Console.WriteLine($"FAIL   {name}  — drift detected but these paths were not named: {string.Join(", ", missing)}");
        }
    }
    finally
    {
        TryDelete(localTemp);
        TryDelete(refTemp);
    }
}

Console.WriteLine();

// -- coverage meta-test --
// Every check kac can emit should be exercised by some fixture. A golden that references a check
// kac no longer emits is a hard error (a rename left a stale golden); a check with no fixture yet
// is reported but does not fail the build while the corpus is still being filled out. Coverage is
// a property of the WHOLE suite, so it is only computed on a full run — a filtered run would
// undercount and read as a regression.
if (filters.Count == 0)
{
    // Checks that no discovered document can reach, so no fixture can exercise them. `type` fires
    // only for a document whose folder maps to no schema, but discovery excludes non-type folders —
    // a validated document therefore always has a type. Accounted for here so the coverage gate can
    // demand every *reachable* check has a fixture.
    var unreachable = new HashSet<string>(["type"], StringComparer.Ordinal);

    var catalogue = CheckCatalogue(kac);
    var stale = coveredChecks.Where(c => !catalogue.Contains(c)).OrderBy(c => c).ToList();
    var uncovered = catalogue.Where(c => !coveredChecks.Contains(c) && !unreachable.Contains(c)).OrderBy(c => c).ToList();
    var unreachableSeen = catalogue.Where(unreachable.Contains).OrderBy(c => c).ToList();

    Console.WriteLine($"coverage: {coveredChecks.Count}/{catalogue.Count} checks exercised by a fixture.");
    if (unreachableSeen.Count > 0)
        Console.WriteLine($"  unreachable (no fixture possible): {string.Join(", ", unreachableSeen)}");
    if (uncovered.Count > 0)
    {
        Console.WriteLine($"  NOT COVERED — every reachable check needs a fixture: {string.Join(", ", uncovered)}");
        failures.Add("(coverage: reachable checks with no fixture)");
    }

    if (stale.Count > 0)
    {
        Console.WriteLine($"  STALE (in a golden but kac no longer emits): {string.Join(", ", stale)}");
        failures.Add("(coverage: stale check ids)");
    }

    // The reader-facing "What CI checks" table must stay a faithful view of the catalogue. `kac
    // checks` self-verifies its curated rows against the catalogue and exits non-zero on any drift
    // (a new check with no row, a row naming a check that no longer exists, a stale waiver).
    var (_, checksErr, checksExit) = Run(Directory.GetCurrentDirectory(), "dotnet", "run", kac, "--", "checks");
    if (checksExit != 0)
    {
        Console.WriteLine($"  CHECKS TABLE out of step with the catalogue:\n{Indent(checksErr)}");
        failures.Add("(checks table vs catalogue)");
    }

    Console.WriteLine();
}
if (update)
{
    Console.WriteLine($"updated {scenarios.Count} golden(s).");
    return 0;
}

if (failures.Count > 0)
{
    Console.WriteLine($"{failures.Count} failure(s): {string.Join(", ", failures)}");
    return 1;
}

Console.WriteLine($"all {scenarios.Count} scenario(s) passed.");
return 0;

// ---------------------------------------------------------------------------

static string? FindRepoRoot(string start)
{
    var dir = new DirectoryInfo(start);
    while (dir is not null)
    {
        if (Directory.Exists(Path.Combine(dir.FullName, ".schema")))
            return dir.FullName;
        dir = dir.Parent;
    }

    return null;
}

// Assemble a throwaway repo root: the real schema plus the fixture corpus. The temp dir is not a
// git repo, so kac's discovery falls back to a filesystem walk — deterministic, and generated /
// finding paths stay corpus-relative (no temp path leaks into a golden). Caller deletes it.
static string AssembleTemp(string schemaDir, string corpusDir)
{
    var temp = Path.Combine(Path.GetTempPath(), "kac-tests-" + Guid.NewGuid().ToString("N"));
    CopyTree(schemaDir, Path.Combine(temp, ".schema"));
    CopyTree(corpusDir, temp);
    return temp;
}

// Like AssembleTemp, but the mechanism check also reads the manifest, so copy the real one in too.
// The subtree (a corpus/ or reference/) is laid over the top, and may add its own .mechanism.lock.
static string AssembleMechanismTemp(string schemaDir, string manifestFile, string subtree)
{
    var temp = Path.Combine(Path.GetTempPath(), "kac-tests-" + Guid.NewGuid().ToString("N"));
    CopyTree(schemaDir, Path.Combine(temp, ".schema"));
    // The schema lives at .schema/, so nothing else creates knowledge-as-code/ for us.
    Directory.CreateDirectory(Path.Combine(temp, "knowledge-as-code"));
    File.Copy(manifestFile, Path.Combine(temp, "knowledge-as-code", "manifest.yaml"));
    CopyTree(subtree, temp);
    return temp;
}

// Run `kac validate --json` against an assembled corpus and return the JSON.
static (string json, int exit) RunValidate(string kac, string schemaDir, string corpusDir)
{
    var temp = AssembleTemp(schemaDir, corpusDir);
    try
    {
        var (stdout, stderr, exit) = Run(temp, "dotnet", "run", kac, "--", "validate", "--json");
        var json = FromFirstBrace(stdout);
        if (json is null)
            throw new Exception($"kac produced no JSON (exit {exit}).\n{Indent(stderr)}\n{Indent(stdout)}");
        return (json, exit);
    }
    finally
    {
        TryDelete(temp);
    }
}

// Run `kac index` (optionally --check) against an assembled corpus; return exit code and combined
// output. With check:true, exit 0 means the generated files match the generator, non-zero means
// stale (the stale files are named in the output).
static (int exit, string output) RunIndex(string kac, string schemaDir, string corpusDir, bool check)
{
    var temp = AssembleTemp(schemaDir, corpusDir);
    try
    {
        string[] argv = check
            ? ["run", kac, "--", "index", "--check"]
            : ["run", kac, "--", "index"];
        var (stdout, stderr, exit) = Run(temp, "dotnet", argv);
        return (exit, stderr + stdout);
    }
    finally
    {
        TryDelete(temp);
    }
}

// Regenerate a scenario's committed generated files: run `kac index` (writing) in a temp assembled
// from the corpus, then copy everything the corpus owns (all but knowledge-as-code/) back over it.
// index leaves source docs untouched, so only INDEX.md and the spliced <type>.md change.
static void RegenerateIndex(string kac, string schemaDir, string corpusDir)
{
    var temp = AssembleTemp(schemaDir, corpusDir);
    try
    {
        var (stdout, stderr, exit) = Run(temp, "dotnet", "run", kac, "--", "index");
        if (exit != 0) throw new Exception($"kac index failed (exit {exit}).\n{Indent(stderr)}");

        // Only what the corpus itself owns comes back. `.schema/` and `knowledge-as-code/` were
        // copied in from the real repo to assemble the run; writing them back would commit a stale
        // duplicate of the schema into the fixture.
        foreach (var dir in Directory.EnumerateDirectories(temp))
            if (Path.GetFileName(dir) is not ("knowledge-as-code" or ".schema"))
                CopyTree(dir, Path.Combine(corpusDir, Path.GetFileName(dir)));
        foreach (var file in Directory.EnumerateFiles(temp))
            File.Copy(file, Path.Combine(corpusDir, Path.GetFileName(file)), overwrite: true);
    }
    finally
    {
        TryDelete(temp);
    }
}

static IReadOnlySet<string> CheckCatalogue(string kac)
{
    var (stdout, stderr, exit) = Run(Directory.GetCurrentDirectory(), "dotnet", "run", kac, "--", "checks", "--json");
    var json = FromFirstBrace(stdout) ?? throw new Exception($"kac checks produced no JSON (exit {exit}).\n{stderr}");
    using var doc = JsonDocument.Parse(json);
    var set = new HashSet<string>(StringComparer.Ordinal);
    foreach (var c in doc.RootElement.GetProperty("checks").EnumerateArray())
        set.Add(c.GetProperty("check").GetString()!);
    return set;
}

static Report ParseFindings(string json)
{
    using var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;
    var findings = new List<F>();
    foreach (var e in root.GetProperty("findings").EnumerateArray())
        findings.Add(new F(
            e.GetProperty("file").GetString()!,
            e.GetProperty("line").ValueKind == JsonValueKind.Null ? null : e.GetProperty("line").GetInt32(),
            e.GetProperty("severity").GetString()!,
            e.GetProperty("check").GetString()!,
            e.GetProperty("message").GetString()!));
    return new Report(findings);
}

// A structural diff keyed on (file, line, severity, check, message): what the golden expected but
// kac did not produce, and what kac produced that the golden did not expect.
static List<string> Diff(Report expected, Report actual)
{
    string Key(F f) => $"{f.File}:{f.Line?.ToString() ?? "-"}  {f.Severity}  [{f.Check}] {f.Message}";
    var exp = expected.Findings.Select(Key).ToList();
    var act = actual.Findings.Select(Key).ToList();
    var expSet = new HashSet<string>(exp);
    var actSet = new HashSet<string>(act);
    var lines = new List<string>();
    foreach (var k in exp.Where(k => !actSet.Contains(k))) lines.Add($"- expected but not produced: {k}");
    foreach (var k in act.Where(k => !expSet.Contains(k))) lines.Add($"+ produced but not expected: {k}");
    return lines;
}

static (string stdout, string stderr, int exit) Run(string workingDir, string file, params string[] argv)
{
    var psi = new ProcessStartInfo(file)
    {
        WorkingDirectory = workingDir,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
    };
    foreach (var a in argv) psi.ArgumentList.Add(a);

    using var p = Process.Start(psi) ?? throw new Exception($"could not start {file}.");
    var outTask = p.StandardOutput.ReadToEndAsync();
    var errTask = p.StandardError.ReadToEndAsync();
    p.WaitForExit();
    return (outTask.Result, errTask.Result, p.ExitCode);
}

// `dotnet run` can prepend build output; take the JSON from the first '{'.
static string? FromFirstBrace(string s)
{
    var i = s.IndexOf('{');
    return i < 0 ? null : s[i..];
}

static void CopyTree(string src, string dst)
{
    Directory.CreateDirectory(dst);
    foreach (var dir in Directory.EnumerateDirectories(src, "*", SearchOption.AllDirectories))
        Directory.CreateDirectory(Path.Combine(dst, Path.GetRelativePath(src, dir)));
    foreach (var file in Directory.EnumerateFiles(src, "*", SearchOption.AllDirectories))
        File.Copy(file, Path.Combine(dst, Path.GetRelativePath(src, file)), overwrite: true);
}

static void TryDelete(string dir)
{
    try { if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true); }
    catch { /* best effort — it is under the system temp dir */ }
}

static string Rel(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');
static string Indent(string s) => string.Join('\n', s.Split('\n').Select(l => "       " + l));

internal record Report(List<F> Findings);

internal record F(string File, int? Line, string Severity, string Check, string Message);
