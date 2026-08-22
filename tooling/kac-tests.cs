#:property TargetFramework=net10.0
#:property Nullable=enable

// kac-tests: the golden-file test suite for kac.
//
// `tooling/tests/README.md` is the reference: how a scenario runs, what each mode asserts, what the exit
// codes mean, and what every fixture covers.
//
// The schema a fixture is assembled over is the real one, copied in per run rather than a committed
// copy, so a schema change that alters behaviour surfaces in this suite rather than after it.

using System.Diagnostics;
using System.Text.Json;

var repoRoot = FindRepoRoot(Directory.GetCurrentDirectory());
if (repoRoot is null)
{
    Console.Error.WriteLine("kac-tests: could not locate the repo root (no kac.slnx above the cwd).");
    return 2;
}

var kacProject = Path.Combine(repoRoot, "tooling", "kac", "kac.csproj");

// The corpus in this repository, and what the suite runs `kac` against where a scenario needs a real one
// rather than an assembled fixture. The command has to start inside a corpus to find anything.
var exampleRoot = Path.Combine(repoRoot, "example");

// The schema every fixture is assembled from: the template's, which is the copy every corpus receives.
// Read where it is authored rather than from a corpus's copy of it, so a schema edit surfaces as a
// broken golden in the same run that made it.
var schemaDir = Path.Combine(repoRoot, "template", ".schema");
var manifestFile = Path.Combine(repoRoot, "tooling", "manifest.yaml");
var fixturesDir = Path.Combine(repoRoot, "tooling", "tests", "fixtures");

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

// Every scenario below runs kac as a subprocess, because what it asserts is the CLI's contract
// rather than the library's: the exit code, and what lands on stdout. `dotnet run` would repeat its
// up-to-date check on each of those invocations, which costs an order of magnitude more than the
// suite's real work; building once and calling the built assembly keeps the process boundary
// without paying for it a dozen times over.
var kac = BuildKac(kacProject);
if (kac is null) return 2;

var coveredChecks = new HashSet<string>(StringComparer.Ordinal);
var failures = new List<string>();

foreach (var scenario in scenarios)
{
    var name = Path.GetFileName(scenario);
    var corpusDir = Path.Combine(scenario, "corpus");
    // A scenario's `mode` file selects what is asserted; absent means the default, a validate diff.
    // `tooling/tests/README.md` says what each mode asserts and which files it reads.
    var modePath = Path.Combine(scenario, "mode");
    var mode = File.Exists(modePath) ? File.ReadAllText(modePath).Trim() : "validate";

    try
    {
        switch (mode)
        {
            case "validate":
                RunValidateScenario(name, scenario, corpusDir);
                break;
            case "generate":
                RunGenerateScenario(name, scenario, mustBeStale: false);
                break;
            case "generate-stale":
                RunGenerateScenario(name, scenario, mustBeStale: true);
                break;
            case "mechanism":
                RunMechanismScenario(name, scenario, corpusDir);
                break;
            case "sync":
                RunSyncScenario(name, scenario, corpusDir);
                break;
            case "export":
                RunExportScenario(name, scenario, corpusDir);
                break;
            case "bundle":
                RunBundleScenario(name, scenario, corpusDir);
                break;
            default:
                failures.Add(name);
                Console.WriteLine($"ERROR  {name}: unknown mode '{mode}'");
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
        Console.WriteLine($"MISSING {name}: no golden. Run: dotnet run tooling/kac-tests.cs -- --update {name}");
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

// generate and generate-stale, told apart by `mustBeStale`. Fresh: `--update` regenerates the committed
// files and a normal run asserts they are fresh. Stale: the fixture is broken by hand, `--update` leaves
// it alone, and the run asserts the staleness is caught. `tooling/tests/README.md` says the rest.
void RunGenerateScenario(string name, string scenario, bool mustBeStale)
{
    var corpusDir = Path.Combine(scenario, "corpus");

    if (update && !mustBeStale)
    {
        Regenerate(kac, schemaDir, corpusDir);
        var (rexit, _) = RunGenerate(kac, schemaDir, corpusDir, check: true);
        Console.WriteLine(rexit == 0
            ? $"UPDATE {name}  (regenerated, fresh)"
            : $"UPDATE {name}  (WARNING: still stale after regen)");
        return;
    }

    if (update)
    {
        Console.WriteLine($"UPDATE {name}  (stale fixture, nothing to regenerate)");
        return;
    }

    var (exit, output) = RunGenerate(kac, schemaDir, corpusDir, check: true);

    if (mustBeStale)
    {
        if (exit == 0)
        {
            failures.Add(name);
            Console.WriteLine($"FAIL   {name}: expected generate --check to detect staleness, but it exited 0");
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
            Console.WriteLine($"FAIL   {name}: stale detected but these files were not named: {string.Join(", ", missing)}");
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
        Console.WriteLine($"FAIL   {name}: committed generated files do not match the generator. Run: dotnet run tooling/kac-tests.cs -- --update {name}");
        foreach (var l in output.Split('\n').Where(l => l.Trim().Length > 0)) Console.WriteLine($"         {l}");
    }
}

// mechanism, over the fixture's `corpus/` and `reference/`. `tooling/tests/README.md` says what
// `expected-drift.txt` pins and what an absent one means.
void RunMechanismScenario(string name, string scenario, string corpusDir)
{
    var referenceDir = Path.Combine(scenario, "reference");
    if (!Directory.Exists(referenceDir))
    {
        failures.Add(name);
        Console.WriteLine($"ERROR  {name}: no reference/ tree in the fixture");
        return;
    }

    if (update)
    {
        Console.WriteLine($"UPDATE {name}  (mechanism scenario, nothing to regenerate)");
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
                Console.WriteLine($"FAIL   {name}: expected the synced layer in step (exit 0), got exit {exit}");
                foreach (var l in output.Split('\n').Where(l => l.Trim().Length > 0)) Console.WriteLine($"         {l}");
            }

            return;
        }

        if (exit == 0)
        {
            failures.Add(name);
            Console.WriteLine($"FAIL   {name}: expected drift (exit 1) but the check passed");
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
            Console.WriteLine($"FAIL   {name}: drift detected but these paths were not named: {string.Join(", ", missing)}");
        }
    }
    finally
    {
        TryDelete(localTemp);
        TryDelete(refTemp);
    }
}

// sync, over the same two trees. It asserts the one thing a check cannot: the tree afterwards.
// `tooling/tests/README.md` says what each expectation file carries.
void RunSyncScenario(string name, string scenario, string corpusDir)
{
    var referenceDir = Path.Combine(scenario, "reference");
    if (!Directory.Exists(referenceDir))
    {
        failures.Add(name);
        Console.WriteLine($"ERROR  {name}: no reference/ tree in the fixture");
        return;
    }

    if (update)
    {
        Console.WriteLine($"UPDATE {name}  (sync scenario, nothing to regenerate)");
        return;
    }

    var expected = ReadLines(Path.Combine(scenario, "expected-sync.txt"));
    var localTemp = AssembleMechanismTemp(schemaDir, manifestFile, corpusDir, ReadKeptTypes(scenario));
    var refTemp = AssembleMechanismTemp(schemaDir, manifestFile, referenceDir);
    try
    {
        var (stdout, stderr, exit) = Run(localTemp, "dotnet", kac, "mechanism", "--sync", "--against", refTemp);
        var output = stderr + stdout;

        var problems = new List<string>();
        if (exit != 0) problems.Add($"expected a clean sync (exit 0), got exit {exit}");
        problems.AddRange(expected.Where(l => !output.Contains(l)).Select(l => $"not reported: {l}"));

        // The tree the sync left. `expected-content.txt` pins copy-then-regenerate here: a shared page
        // comes down whole, generated block and all, and is only right once rebuilt against the types
        // the receiving corpus holds.
        problems.AddRange(CheckTree(localTemp, scenario));

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

// export, over the fixture corpus. The golden is the whole export, committed under `expected-dist/` and
// diffed file for file, because this is where a change to what a consumer reads becomes visible.
// `tooling/tests/README.md` says what sits beside it and why two fields are normalised away.
void RunExportScenario(string name, string scenario, string corpusDir)
{
    var typePath = Path.Combine(scenario, "export-type");
    var type = File.Exists(typePath) ? File.ReadAllText(typePath).Trim() : null;
    var golden = Path.Combine(scenario, "expected-dist");
    var temp = AssembleTemp(schemaDir, corpusDir);
    try
    {
        string[] argv = type is null ? [kac, "export"] : [kac, "export", "--type", type];
        var (stdout, stderr, exit) = Run(temp, "dotnet", argv);
        var output = stderr + stdout;
        var dist = Path.Combine(temp, ".dist", "export");

        if (update)
        {
            if (exit != 0)
            {
                Console.WriteLine($"UPDATE {name}: WARNING: export failed (exit {exit}), golden left alone");
                return;
            }

            var written = WriteGoldenExport(dist, golden);
            Console.WriteLine($"UPDATE {name}  ({written} export file(s), read the diff: it is the published shape)");
            return;
        }

        var problems = new List<string>();
        if (exit != 0) problems.Add($"expected a clean export (exit 0), got exit {exit}");
        problems.AddRange(ReadLines(Path.Combine(scenario, "expected-export.txt"))
            .Where(l => !output.Contains(l)).Select(l => $"not reported: {l}"));

        problems.AddRange(CheckGoldenExport(dist, golden));

        // A file no record backs, left in the output before the second run. An export nobody reviews
        // would otherwise carry a deleted record's entry indefinitely.
        var orphan = Path.Combine(dist, "orphan.json");
        Directory.CreateDirectory(Path.GetDirectoryName(orphan)!);
        File.WriteAllText(orphan, "{}\n");

        var (_, _, second) = Run(temp, "dotnet", argv);
        if (second != 0) problems.Add($"expected a clean second export (exit 0), got exit {second}");
        if (File.Exists(orphan)) problems.Add("a file no record backs survived a second export");

        problems.AddRange(CheckTree(temp, scenario));

        if (problems.Count == 0)
        {
            Console.WriteLine($"ok     {name}  (exported)");
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
        TryDelete(temp);
    }
}

// bundle, over the plugin tree and the export beneath it. `tooling/tests/README.md` says what it asserts
// and why no committed copy of the plugin sits beside the export's.
void RunBundleScenario(string name, string scenario, string corpusDir)
{
    var pluginDir = Path.Combine(scenario, "plugin");
    if (!Directory.Exists(pluginDir))
    {
        failures.Add(name);
        Console.WriteLine($"ERROR  {name}: no plugin/ tree in the fixture");
        return;
    }

    if (update)
    {
        Console.WriteLine($"UPDATE {name}  (bundle scenario, nothing to regenerate)");
        return;
    }

    var typePath = Path.Combine(scenario, "export-type");
    var type = File.Exists(typePath) ? File.ReadAllText(typePath).Trim() : null;
    var temp = AssembleTemp(schemaDir, corpusDir);
    try
    {
        CopyTree(pluginDir, Path.Combine(temp, ".plugin"));

        string[] exportArgv = type is null ? [kac, "export"] : [kac, "export", "--type", type];
        var (_, exportErr, exportExit) = Run(temp, "dotnet", exportArgv);

        var problems = new List<string>();
        if (exportExit != 0) problems.Add($"the export the bundle needs failed (exit {exportExit}): {exportErr}");

        var (stdout, stderr, exit) = Run(temp, "dotnet", kac, "bundle");
        var output = stderr + stdout;

        if (exit != 0) problems.Add($"expected a clean bundle (exit 0), got exit {exit}");
        problems.AddRange(ReadLines(Path.Combine(scenario, "expected-bundle.txt"))
            .Where(l => !output.Contains(l)).Select(l => $"not reported: {l}"));

        // The copy is the seam between the two commands, so a difference between the two copies is a
        // defect rather than something to interpret. `corpus-root` names where the copy landed, which is
        // the fixture plugin's `metadata.corpusRoot` and is the one thing this cannot read for itself.
        var corpusRoot = File.ReadAllText(Path.Combine(scenario, "corpus-root")).Trim();
        problems.AddRange(SameTree(
            Path.Combine(temp, ".dist", "export"),
            Path.Combine(temp, ".dist", "plugin", corpusRoot)));

        problems.AddRange(CheckTree(temp, scenario));

        // A file no component backs, left in the plugin before the second run. The same failure mode the
        // export has, and the same answer: the directory is replaced whole rather than written over.
        var orphan = Path.Combine(temp, ".dist", "plugin", "skills", "orphan", "SKILL.md");
        Directory.CreateDirectory(Path.GetDirectoryName(orphan)!);
        File.WriteAllText(orphan, "orphan\n");

        var (_, _, second) = Run(temp, "dotnet", kac, "bundle");
        if (second != 0) problems.Add($"expected a clean second bundle (exit 0), got exit {second}");
        if (File.Exists(orphan)) problems.Add("a file no component backs survived a second bundle");

        if (problems.Count == 0)
        {
            Console.WriteLine($"ok     {name}  (bundled)");
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
        TryDelete(temp);
    }
}

// Whether two trees hold the same files with the same bytes. Bytes rather than text, because what is
// being asserted is that a copy was a copy: a comparison that normalised line endings would pass over
// exactly the edit it exists to catch.
static List<string> SameTree(string expected, string actual)
{
    if (!Directory.Exists(actual)) return [$"the bundle wrote no copy of the export at {actual}"];

    var problems = new List<string>();
    var want = TreeBytes(expected);
    var got = TreeBytes(actual);

    foreach (var rel in want.Keys.Union(got.Keys, StringComparer.Ordinal).OrderBy(p => p, StringComparer.Ordinal))
    {
        if (!got.TryGetValue(rel, out var b)) problems.Add($"the copied export is missing {rel}");
        else if (!want.TryGetValue(rel, out var a)) problems.Add($"the copied export holds {rel}, which the export does not");
        else if (!a.SequenceEqual(b)) problems.Add($"the copied export differs from the export at {rel}: bundle edited it");
    }

    return problems;
}

static SortedDictionary<string, byte[]> TreeBytes(string root)
{
    var files = new SortedDictionary<string, byte[]>(StringComparer.Ordinal);
    if (!Directory.Exists(root)) return files;

    foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        files[Path.GetRelativePath(root, file).Replace('\\', '/')] = File.ReadAllBytes(file);

    return files;
}

// The tree a scenario expects: every path in expected-files.txt present (or absent, prefixed `!`), and
// every `<path> :: <text>` in expected-content.txt holding (or not, prefixed `!`). Shared by `sync`,
// `export` and `bundle`, which assert the same kind of thing about three different trees.
static List<string> CheckTree(string root, string scenario)
{
    var problems = new List<string>();

    foreach (var line in ReadLines(Path.Combine(scenario, "expected-files.txt")))
    {
        var absent = line.StartsWith('!');
        var rel = absent ? line[1..].Trim() : line;
        var exists = File.Exists(Path.Combine(root, rel.Replace('/', Path.DirectorySeparatorChar)));
        if (exists == absent) problems.Add(absent ? $"should not exist: {rel}" : $"missing afterwards: {rel}");
    }

    foreach (var line in ReadLines(Path.Combine(scenario, "expected-content.txt")))
    {
        var parts = line.Split("::", 2);
        if (parts.Length != 2) { problems.Add($"malformed expected-content line: {line}"); continue; }

        var rel = parts[0].Trim();
        var wanted = parts[1].Trim();
        var absent = wanted.StartsWith('!');
        if (absent) wanted = wanted[1..].Trim();

        var full = Path.Combine(root, rel.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(full)) { problems.Add($"missing afterwards: {rel}"); continue; }
        if (File.ReadAllText(full).Contains(wanted) == absent)
            problems.Add(absent ? $"{rel} still says '{wanted}'" : $"{rel} does not say '{wanted}'");
    }

    return problems;
}

// The export as written, held against the copy committed beside the fixture. Every file in either tree
// is named, so a file the export stopped writing fails as loudly as one it started writing. Content is
// compared whole, so a key that moved fails too.
//
// A difference here is a change to a published contract, and each message says so. The reflex on a red
// golden is to regenerate it, and this is the one golden where that reflex is expensive.
static List<string> CheckGoldenExport(string dist, string golden)
{
    if (!Directory.Exists(golden))
        return [$"no committed export at {Path.GetFileName(golden)}/. "
                + "Run: dotnet run tooling/kac-tests.cs -- --update export"];

    var actual = ExportTree(dist);
    var expected = ExportTree(golden);
    var paths = expected.Keys.Union(actual.Keys, StringComparer.Ordinal).OrderBy(p => p, StringComparer.Ordinal);

    var problems = new List<string>();
    foreach (var rel in paths)
    {
        if (!actual.TryGetValue(rel, out var got))
            problems.Add($"the export no longer writes {rel}: what a consumer reads has changed");
        else if (!expected.TryGetValue(rel, out var want))
            problems.Add(
                $"the export writes {rel}, which the committed copy does not hold: a new file for a consumer");
        else if (!string.Equals(want, got, StringComparison.Ordinal))
            problems.Add($"{rel} differs from the committed export: {FirstDifference(want, got)}");
    }

    return problems;
}

// Replace the committed export whole, as the exporter replaces `.dist/export/` whole. Anything the
// export stopped writing has to leave the golden with it, or the next run reads it as a file the export
// lost.
static int WriteGoldenExport(string dist, string golden)
{
    if (Directory.Exists(golden)) Directory.Delete(golden, recursive: true);

    var files = ExportTree(dist);
    foreach (var (rel, content) in files)
    {
        var full = Path.Combine(golden, rel.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllText(full, content);
    }

    return files.Count;
}

// One export tree read into memory, keyed by the path a consumer would address, with the manifest
// normalised. Both sides go through it, so the golden on disk is already normalised. The comparison is
// then a string equality, and no rule is applied to one side and remembered for the other.
static SortedDictionary<string, string> ExportTree(string root)
{
    var files = new SortedDictionary<string, string>(StringComparer.Ordinal);
    if (!Directory.Exists(root)) return files;

    foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
    {
        var rel = Path.GetRelativePath(root, file).Replace('\\', '/');
        files[rel] = Normalise(rel, File.ReadAllText(file));
    }

    return files;
}

// The two fields of the manifest that differ between two runs over one tree, replaced by their own
// names. `generatedAt` moves every run and `commit` moves with the branch, so a golden carrying either
// would fail for a reason that says nothing about the export's shape.
//
// The value goes whatever it is, a string and a null alike, so the rule fires in every corpus rather
// than only in one that has a commit to name. `expected-content.txt` pins what a corpus with no
// repository writes there, against the file the run emitted.
static string Normalise(string rel, string content)
{
    if (rel != "manifest.json") return content;

    string[] volatileFields = ["generatedAt", "commit"];

    return string.Join('\n', content.Split('\n').Select(line =>
    {
        foreach (var key in volatileFields)
        {
            var marker = $"\"{key}\": ";
            var at = line.IndexOf(marker, StringComparison.Ordinal);
            if (at < 0) continue;
            return line[..(at + marker.Length)] + $"\"<{key}>\"" + (line.EndsWith(',') ? "," : "");
        }

        return line;
    }));
}

// Where two versions of one file part company, as the first line that differs. A whole-file diff of an
// indented JSON document is unreadable in a test log. One line is what a person needs to
// decide whether they meant it.
static string FirstDifference(string expected, string actual)
{
    var want = expected.Split('\n');
    var got = actual.Split('\n');

    for (var i = 0; i < Math.Max(want.Length, got.Length); i++)
    {
        var a = i < want.Length ? want[i] : "(end of file)";
        var b = i < got.Length ? got[i] : "(end of file)";
        if (a == b) continue;
        return $"line {i + 1}: committed '{Clip(a)}' and exported '{Clip(b)}'";
    }

    return "the files differ in their line endings alone";

    static string Clip(string line) =>
        line.Trim() is { Length: > 120 } long_ ? long_[..120] + "…" : line.Trim();
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
// check with no fixture, and a golden naming a check the catalogue does not hold, which is a rename
// that left a stale golden. Coverage is a property of the whole suite, so it is only computed on a
// full run. A filtered run would undercount and read as a regression.
if (filters.Count == 0)
{
    // Checks that no discovered document can reach, so no fixture can exercise them. `type` fires
    // only for a document whose folder maps to no schema, but discovery excludes non-type folders.
    // A validated document therefore always has a type. Accounted for here so the coverage gate can
    // demand every *reachable* check has a fixture.
    var unreachable = new HashSet<string>(["type"], StringComparer.Ordinal);

    var catalogue = CheckCatalogue(kac, exampleRoot);
    var stale = coveredChecks.Where(c => !catalogue.Contains(c)).OrderBy(c => c).ToList();
    var uncovered = catalogue.Where(c => !coveredChecks.Contains(c) && !unreachable.Contains(c)).OrderBy(c => c).ToList();
    var unreachableSeen = catalogue.Where(unreachable.Contains).OrderBy(c => c).ToList();

    Console.WriteLine($"coverage: {coveredChecks.Count}/{catalogue.Count} checks exercised by a fixture.");
    if (unreachableSeen.Count > 0)
        Console.WriteLine($"  unreachable (no fixture possible): {string.Join(", ", unreachableSeen)}");
    if (uncovered.Count > 0)
    {
        Console.WriteLine($"  NOT COVERED, every reachable check needs a fixture: {string.Join(", ", uncovered)}");
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
    var (_, checksErr, checksExit) = Run(exampleRoot, "dotnet", kac, "checks");
    if (checksExit != 0)
    {
        Console.WriteLine($"  CHECKS TABLE out of step with the catalogue:\n{Indent(checksErr)}");
        failures.Add("(checks table vs catalogue)");
    }

    // -- what answers without a corpus --
    // `--version` and `--help` are answered by the parser, so an installed `kac` says what it is from
    // wherever it was typed; every verb needs a corpus and exits 2 without one. Asserted from a temp
    // directory with no `.schema` above it. The fault this catches is a corpus lookup running before
    // the parse, and that passes every other test here, all of which run inside a corpus.
    var nowhere = Directory.CreateTempSubdirectory("kac-tests-nowhere-").FullName;
    try
    {
        foreach (var flag in new[] { "--version", "--help" })
        {
            var (flagOut, _, flagExit) = Run(nowhere, "dotnet", kac, flag);
            if (flagExit == 0 && flagOut.Trim().Length > 0) continue;
            Console.WriteLine($"  {flag} outside a corpus: exit {flagExit}, printed {flagOut.Trim().Length} char(s)");
            failures.Add($"({flag} outside a corpus)");
        }

        var (_, verbErr, verbExit) = Run(nowhere, "dotnet", kac, "validate");
        if (verbExit != 2)
        {
            Console.WriteLine($"  validate outside a corpus: exit {verbExit}, expected 2\n{Indent(verbErr)}");
            failures.Add("(a verb outside a corpus)");
        }
    }
    finally
    {
        TryDelete(nowhere);
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

// The repository, found by the solution at its root rather than by the corpus beside it. `kac` itself walks
// up for a `.schema/` because what it wants is a corpus; this suite wants the tree that holds the engine,
// the fixtures and the corpus at once, and only one folder answers to that. The solution file is the right
// marker for it: it names every project in the tree, and a corpus that installed the tool holds no such thing.
static string? FindRepoRoot(string start)
{
    var dir = new DirectoryInfo(start);
    while (dir is not null)
    {
        if (File.Exists(Path.Combine(dir.FullName, "kac.slnx")))
            return dir.FullName;
        dir = dir.Parent;
    }

    return null;
}

// Assemble a throwaway repo root: the real schema plus the fixture corpus. The temp dir is not a
// git repo, so kac's discovery falls back to a filesystem walk. That walk is deterministic, and
// generated and finding paths stay corpus-relative (no temp path leaks into a golden). Caller deletes it.
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

    // The schema lives at .schema/, so nothing else creates tooling/ for us.
    Directory.CreateDirectory(Path.Combine(temp, "tooling"));
    File.Copy(manifestFile, Path.Combine(temp, "tooling", "manifest.yaml"));
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

// Run `kac generate` (optionally --check) against an assembled corpus; return exit code and combined
// output. With check:true, exit 0 means the generated files match the generator, non-zero means
// stale (the stale files are named in the output).
static (int exit, string output) RunGenerate(string kac, string schemaDir, string corpusDir, bool check)
{
    var temp = AssembleTemp(schemaDir, corpusDir);
    try
    {
        string[] argv = check
            ? [kac, "generate", "--check"]
            : [kac, "generate"];
        var (stdout, stderr, exit) = Run(temp, "dotnet", argv);
        return (exit, stderr + stdout);
    }
    finally
    {
        TryDelete(temp);
    }
}

// Regenerate a scenario's committed generated files: run `kac generate` (writing) in a temp assembled
// from the corpus, then copy everything the corpus owns (all but `.schema/`) back over it.
// `generate` leaves source docs untouched, so only _index.md and the spliced <type>.md change.
static void Regenerate(string kac, string schemaDir, string corpusDir)
{
    var temp = AssembleTemp(schemaDir, corpusDir);
    try
    {
        var (stdout, stderr, exit) = Run(temp, "dotnet", kac, "generate");
        if (exit != 0) throw new Exception($"kac generate failed (exit {exit}).\n{Indent(stderr)}");

        // Only what the corpus itself owns comes back. `.schema/` is the real one, copied in to assemble
        // the run, and writing it back would commit a stale duplicate of the schema into the fixture.
        // Everything else in the temp tree came from the fixture, including a `knowledge-as-code/`
        // page a fixture stands up to assert what is generated into it.
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

// Build the tool project and return the assembly to invoke, or null having said why. Two calls
// because they do different things: the first builds, and `--getProperty` evaluates the project
// without building. The path is read back rather than constructed, so a configuration or a target
// framework settled in the project cannot silently disagree with a path spelled out here.
static string? BuildKac(string kacProject)
{
    var (buildOut, buildErr, buildExit) = Run(Directory.GetCurrentDirectory(), "dotnet", "build", kacProject);
    if (buildExit != 0)
    {
        Console.Error.WriteLine($"kac-tests: building {kacProject} failed (exit {buildExit}).");
        Console.Error.WriteLine(Indent(buildErr + buildOut));
        return null;
    }

    var (pathOut, pathErr, pathExit) =
        Run(Directory.GetCurrentDirectory(), "dotnet", "build", kacProject, "--getProperty:TargetPath");
    var assembly = pathOut.Trim();
    if (pathExit == 0 && assembly.Length > 0 && File.Exists(assembly)) return assembly;

    Console.Error.WriteLine($"kac-tests: could not locate the built assembly for {kacProject} (exit {pathExit}).");
    Console.Error.WriteLine(Indent(pathErr + pathOut));
    return null;
}

static IReadOnlySet<string> CheckCatalogue(string kac, string corpusRoot)
{
    var (stdout, stderr, exit) = Run(corpusRoot, "dotnet", kac, "checks", "--json");
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

    // Colour off, so what a scenario captures is the same on every machine. Spectre writes escapes to
    // a redirected stream where the environment declares itself a runner that renders them, and
    // GITHUB_ACTIONS is one, so a golden matching on text would pass here and fail in CI.
    psi.Environment["NO_COLOR"] = "1";

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
    catch { /* best effort: it is under the system temp dir */ }
}

static string Rel(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');
static string Indent(string s) => string.Join('\n', s.Split('\n').Select(l => "       " + l));

internal record Report(List<F> Findings);

internal record F(string File, int? Line, string Severity, string Check, string Message);
