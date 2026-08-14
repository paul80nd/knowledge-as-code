#:property TargetFramework=net10.0
#:property Nullable=enable

// kac-tests — the golden-file test suite for kac.
//
// Each fixture under .tooling/tests/fixtures/<scenario>/ is a deliberately-broken (or deliberately-clean)
// mini-corpus. The runner assembles a throwaway repo root from the real schema plus the fixture's
// corpus, runs `kac validate --json` against it, and diffs the findings against the scenario's
// committed golden (expected.json). The golden captures the whole findings set for the corpus, so an
// accidental extra finding shows up as a diff — which is the point.
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
using System.Text.Json;

var repoRoot = FindRepoRoot(Directory.GetCurrentDirectory());
if (repoRoot is null)
{
    Console.Error.WriteLine("kac-tests: could not locate the repo root (no .schema above the cwd).");
    return 2;
}

var kacSource = Path.Combine(repoRoot, ".tooling", "kac.cs");
var schemaDir = Path.Combine(repoRoot, ".schema");
var manifestFile = Path.Combine(repoRoot, ".tooling", "manifest.yaml");
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

// Every scenario below runs kac as a subprocess, because what it asserts — the exit code and what
// lands on stdout — is the CLI's contract rather than the library's. `dotnet run` would repeat its
// up-to-date check on each of those invocations, which costs an order of magnitude more than the
// suite's real work; building once and calling the built assembly keeps the process boundary
// without paying for it a dozen times over.
var kac = BuildKac(kacSource);
if (kac is null) return 2;

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
    //   sync         run `mechanism --sync` the same way, then assert the tree it left: the lines in
    //                expected-sync.txt appear in the output, every path in expected-files.txt is present
    //                afterwards (or absent, where the line is prefixed `!`), and every
    //                `<path> :: <text>` in expected-content.txt holds (or does not, prefixed `!`)
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
            case "sync":
                RunSyncScenario(name, scenario, corpusDir);
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
// the committed _index.md / <type>.md itself — reviewable in git, kept fresh by `--update`.
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
// lists what must appear in the failure output — the shared paths named, and any other line the
// scenario pins; absent/empty means "expect in step" (exit 0). Accepted divergences, forked
// differences and files the corpus's descriptor declines are exercised by the fixture but must not fail
// the run — the golden is the exit code plus the named lines, not free-form output.
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
        var (stdout, stderr, exit) = Run(localTemp, "dotnet", kac, "mechanism", "--check", "--against", refTemp);
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

// sync: the same two trees, run through `mechanism --sync`. It asserts the one thing the check cannot —
// the tree afterwards. expected-files.txt carries most of that weight. Each line names a path that must
// exist in the local corpus once the sync has run; a line prefixed `!` names one that must not. That is
// how a fixture pins that a declined type was left upstream rather than quietly stood up.
void RunSyncScenario(string name, string scenario, string corpusDir)
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
        Console.WriteLine($"UPDATE {name}  (sync scenario — nothing to regenerate)");
        return;
    }

    var expected = ReadLines(Path.Combine(scenario, "expected-sync.txt"));
    var expectedFiles = ReadLines(Path.Combine(scenario, "expected-files.txt"));
    var localTemp = AssembleMechanismTemp(schemaDir, manifestFile, corpusDir, ReadKeptTypes(scenario));
    var refTemp = AssembleMechanismTemp(schemaDir, manifestFile, referenceDir);
    try
    {
        var (stdout, stderr, exit) = Run(localTemp, "dotnet", kac, "mechanism", "--sync", "--against", refTemp);
        var output = stderr + stdout;

        var problems = new List<string>();
        if (exit != 0) problems.Add($"expected a clean sync (exit 0), got exit {exit}");
        problems.AddRange(expected.Where(l => !output.Contains(l)).Select(l => $"not reported: {l}"));

        foreach (var line in expectedFiles)
        {
            var absent = line.StartsWith('!');
            var rel = absent ? line[1..].Trim() : line;
            var exists = File.Exists(Path.Combine(localTemp, rel.Replace('/', Path.DirectorySeparatorChar)));
            if (exists == absent) problems.Add(absent ? $"should not exist: {rel}" : $"missing afterwards: {rel}");
        }

        // `<path> :: <text>`, or `!<text>` for text that must be gone. These lines pin copy-then-
        // regenerate: a shared page comes down whole, generated block and all, and is only right once
        // rebuilt against the types the receiving corpus holds.
        foreach (var line in ReadLines(Path.Combine(scenario, "expected-content.txt")))
        {
            var parts = line.Split("::", 2);
            if (parts.Length != 2) { problems.Add($"malformed expected-content line: {line}"); continue; }

            var rel = parts[0].Trim();
            var wanted = parts[1].Trim();
            var absent = wanted.StartsWith('!');
            if (absent) wanted = wanted[1..].Trim();

            var full = Path.Combine(localTemp, rel.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(full)) { problems.Add($"missing afterwards: {rel}"); continue; }
            if (File.ReadAllText(full).Contains(wanted) == absent)
                problems.Add(absent ? $"{rel} still says '{wanted}'" : $"{rel} does not say '{wanted}'");
        }

        if (problems.Count == 0)
        {
            Console.WriteLine($"ok     {name}  (synced)");
        }
        else
        {
            failures.Add(name);
            Console.WriteLine($"FAIL   {name}");
            foreach (var p in problems) Console.WriteLine($"         {p}");
            foreach (var l in output.Split('\n').Where(l => l.Trim().Length > 0)) Console.WriteLine($"       | {l}");
        }
    }
    finally
    {
        TryDelete(localTemp);
        TryDelete(refTemp);
    }
}

static List<string> ReadLines(string path) =>
    File.Exists(path)
        ? [.. File.ReadAllLines(path).Select(l => l.Trim()).Where(l => l.Length > 0 && !l.StartsWith('#'))]
        : [];

// corpus-schema.txt names the type schema files the local corpus holds *before* the sync, which is the
// half-adopted state a consumer runs one from. Absent means the corpus holds them all.
static HashSet<string>? ReadKeptTypes(string scenario)
{
    var path = Path.Combine(scenario, "corpus-schema.txt");
    return File.Exists(path) ? new HashSet<string>(ReadLines(path), StringComparer.Ordinal) : null;
}

Console.WriteLine();

// -- coverage meta-test --
// Every reachable check must be exercised by some fixture, and both directions fail the build: a
// check with no fixture, and a golden naming a check the catalogue does not hold — which is a rename
// that left a stale golden. Coverage is a property of the whole suite, so it is only computed on a
// full run; a filtered run would undercount and read as a regression.
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
    var (_, checksErr, checksExit) = Run(Directory.GetCurrentDirectory(), "dotnet", kac, "checks");
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
// The subtree (a corpus/ or reference/) is laid over the top, and may add its own .corpus.yaml.
// `keptTypes`, where a fixture supplies one, names the per-type schema files this side holds. The real
// `.schema/` cannot express a corpus holding fewer of them than upstream, and that is the state a sync
// resolves. Underscore-prefixed files belong to no type, so every corpus holds them whatever it adopted.
static string AssembleMechanismTemp(string schemaDir, string manifestFile, string subtree,
    HashSet<string>? keptTypes = null)
{
    var temp = Path.Combine(Path.GetTempPath(), "kac-tests-" + Guid.NewGuid().ToString("N"));
    CopyTree(schemaDir, Path.Combine(temp, ".schema"));
    if (keptTypes is not null)
        foreach (var file in Directory.EnumerateFiles(Path.Combine(temp, ".schema"), "*.yaml"))
            if (!Path.GetFileName(file).StartsWith('_') && !keptTypes.Contains(Path.GetFileNameWithoutExtension(file)))
                File.Delete(file);

    // The schema lives at .schema/, so nothing else creates .tooling/ for us.
    Directory.CreateDirectory(Path.Combine(temp, ".tooling"));
    File.Copy(manifestFile, Path.Combine(temp, ".tooling", "manifest.yaml"));
    CopyTree(subtree, temp);
    return temp;
}

// Run `kac validate --json` against an assembled corpus and return the JSON.
static (string json, int exit) RunValidate(string kac, string schemaDir, string corpusDir)
{
    var temp = AssembleTemp(schemaDir, corpusDir);
    try
    {
        var (stdout, stderr, exit) = Run(temp, "dotnet", kac, "validate", "--json");
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
            ? [kac, "index", "--check"]
            : [kac, "index"];
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
// index leaves source docs untouched, so only _index.md and the spliced <type>.md change.
static void RegenerateIndex(string kac, string schemaDir, string corpusDir)
{
    var temp = AssembleTemp(schemaDir, corpusDir);
    try
    {
        var (stdout, stderr, exit) = Run(temp, "dotnet", kac, "index");
        if (exit != 0) throw new Exception($"kac index failed (exit {exit}).\n{Indent(stderr)}");

        // Only what the corpus itself owns comes back. `.schema/` is the real one, copied in to assemble
        // the run, and writing it back would commit a stale duplicate of the schema into the fixture.
        // Everything else in the temp tree came from the fixture — including a `knowledge-as-code/` page
        // a fixture stands up to assert what is generated into it.
        foreach (var dir in Directory.EnumerateDirectories(temp))
            if (Path.GetFileName(dir) is not ".schema")
                CopyTree(dir, Path.Combine(corpusDir, Path.GetFileName(dir)));
        foreach (var file in Directory.EnumerateFiles(temp))
            File.Copy(file, Path.Combine(corpusDir, Path.GetFileName(file)), overwrite: true);
    }
    finally
    {
        TryDelete(temp);
    }
}

// Compile .tooling/kac.cs and return the assembly to invoke, or null having said why. Two calls
// because they do different things: the first builds, and `--getProperty` evaluates the project
// without building. The path cannot simply be constructed — MSBuild puts a file-based app's output
// in a content-addressed directory outside the repo, which is why it is read back rather than
// assumed.
static string? BuildKac(string kacSource)
{
    var (buildOut, buildErr, buildExit) = Run(Directory.GetCurrentDirectory(), "dotnet", "build", kacSource);
    if (buildExit != 0)
    {
        Console.Error.WriteLine($"kac-tests: building {kacSource} failed (exit {buildExit}).");
        Console.Error.WriteLine(Indent(buildErr + buildOut));
        return null;
    }

    var (pathOut, pathErr, pathExit) =
        Run(Directory.GetCurrentDirectory(), "dotnet", "build", kacSource, "--getProperty:TargetPath");
    var assembly = pathOut.Trim();
    if (pathExit == 0 && assembly.Length > 0 && File.Exists(assembly)) return assembly;

    Console.Error.WriteLine($"kac-tests: could not locate the built assembly for {kacSource} (exit {pathExit}).");
    Console.Error.WriteLine(Indent(pathErr + pathOut));
    return null;
}

static IReadOnlySet<string> CheckCatalogue(string kac)
{
    var (stdout, stderr, exit) = Run(Directory.GetCurrentDirectory(), "dotnet", kac, "checks", "--json");
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

// Take the JSON from the first '{', so anything the runtime prints ahead of the report does not
// have to be anticipated to be tolerated.
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
