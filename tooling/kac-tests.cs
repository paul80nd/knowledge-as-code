#:property TargetFramework=net10.0
#:property Nullable=enable

// The golden-file suite for kac. `tooling/tests/README.md` says how a scenario runs, what each mode asserts and
// what every fixture covers.

using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

var repoRoot = FindRepoRoot(Directory.GetCurrentDirectory());
if (repoRoot is null)
{
    Console.Error.WriteLine("kac-tests: could not locate the repo root (no kac.slnx above the cwd).");
    return 2;
}

var kacProject = Path.Combine(repoRoot, "tooling", "kac", "kac.csproj");

// The corpus the coverage and checks-table gates run `kac checks` in. `kac checks` reads the schema and never the
// corpus's adoption, so any corpus here answers the same.
var libraryRoot = Path.Combine(repoRoot, "examples", "library");

// The real schema rather than a committed copy, so a schema edit that changes behaviour breaks a golden in the
// same run.
var schemaDir = Path.Combine(repoRoot, ".schema");
var fixturesDir = Path.Combine(repoRoot, "tooling", "tests", "fixtures");

var rawArgs = args.ToList();
var update = rawArgs.RemoveAll(a => a is "--update" or "-u") > 0;
var filters = rawArgs;

if (!Directory.Exists(fixturesDir))
{
    Console.Error.WriteLine($"kac-tests: no fixtures directory at {Rel(repoRoot, fixturesDir)}.");
    return 2;
}

var scenarios = Directory.EnumerateDirectories(fixturesDir)
    .Where(d => Directory.Exists(Path.Combine(d, "corpus")) || File.Exists(Path.Combine(d, "mode")))
    .Where(d => filters.Count == 0 || filters.Any(f => Path.GetFileName(d).Contains(f, StringComparison.OrdinalIgnoreCase)))
    .OrderBy(d => d, StringComparer.Ordinal)
    .ToList();

if (scenarios.Count == 0)
{
    Console.Error.WriteLine("kac-tests: no matching scenarios.");
    return 2;
}

// Each scenario runs kac as a subprocess, because it asserts the CLI's exit code and stdout. It calls a build made
// once, because `dotnet run` would repeat its up-to-date check on every invocation.
var kac = BuildKac(kacProject);
if (kac is null) return 2;

var coveredChecks = new HashSet<string>(StringComparer.Ordinal);
var failures = new List<string>();

foreach (var scenario in scenarios)
{
    var name = Path.GetFileName(scenario);
    var corpusDir = Path.Combine(scenario, "corpus");
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
            case "update":
                RunUpdateScenario(name, scenario);
                break;
            case "export":
                RunExportScenario(name, scenario, corpusDir);
                break;
            case "bundle":
                RunBundleScenario(name, scenario, corpusDir);
                break;
            case "pack":
                RunPackScenario(name, scenario, corpusDir);
                break;
            case "new":
                RunNewScenario(name, scenario);
                break;
            case "report":
                RunReportScenario(name, scenario, corpusDir);
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

// `report --out` asserted against the same run's stdout rather than a golden, because the frontmatter has the
// moment the run happened.
void RunReportScenario(string name, string scenario, string corpusDir)
{
    var temp = AssembleTemp(schemaDir, corpusDir);
    try
    {
        var target = Path.Combine("reports", "clause-coverage.md");
        var full = Path.Combine(temp, target);

        var (printed, _, printExit) = Run(temp, "dotnet", kac, "report", "coverage");
        var (wroteOut, _, writeExit) = Run(temp, "dotnet", kac, "report", "coverage", "--out", target);
        var written = File.Exists(full) ? File.ReadAllText(full) : "";
        var (_, refuseErr, refuseExit) = Run(temp, "dotnet", kac, "report", "coverage", "--out", target);
        var after = File.Exists(full) ? File.ReadAllText(full) : "";

        var problems = new List<string>();
        if (printExit != 0) problems.Add($"printing to stdout exited {printExit}");
        if (writeExit != 0) problems.Add($"--out exited {writeExit}, and said: {wroteOut.Trim()}");
        if (!File.Exists(full)) problems.Add($"--out wrote no {target}");
        if (!wroteOut.Contains(target.Replace('\\', '/'), StringComparison.Ordinal))
            problems.Add($"--out did not name what it wrote: {wroteOut.Trim()}");

        // The console ends on a blank line and a record does not, so both are trimmed and the file's own ending is
        // asserted below.
        if (Stamped(written) != Stamped(printed))
            problems.Add("the file --out wrote is not what the same report printed");
        if (!written.EndsWith("\n", StringComparison.Ordinal) || written.EndsWith("\n\n", StringComparison.Ordinal))
            problems.Add("the file does not end on one newline, as every other record here does");
        if (!written.Contains("| `WRITTEN` | MUST | `std-COVER` |", StringComparison.Ordinal))
            problems.Add("the covered clause did not reach the file");

        if (refuseExit != 1) problems.Add($"a second --out over the same path exited {refuseExit}, not 1");
        if (!refuseErr.Contains("already exists", StringComparison.Ordinal))
            problems.Add($"the refusal did not say the path is taken: {refuseErr.Trim()}");
        if (after != written) problems.Add("the refused run changed the file it refused to write");

        if (problems.Count > 0)
        {
            failures.Add(name);
            Console.WriteLine($"FAIL   {name}");
            foreach (var problem in problems) Console.WriteLine($"         {problem}");
            return;
        }

        Console.WriteLine($"ok     {name}  (wrote {target}, refused the second run)");
    }
    finally
    {
        Directory.Delete(temp, true);
    }
}

// The report without its stamp. `generated:` opens a block, so the key and the `at:` under it both go.
static string Stamped(string report)
{
    var kept = new List<string>();
    var generated = false;

    foreach (var line in report.Replace("\r\n", "\n").TrimEnd('\n').Split('\n'))
    {
        if (line.StartsWith("generated:", StringComparison.Ordinal))
        {
            generated = true;
            continue;
        }

        if (generated && line.StartsWith("  at:", StringComparison.Ordinal)) continue;
        if (!line.StartsWith("  ", StringComparison.Ordinal)) generated = false;

        kept.Add(line);
    }

    return string.Join('\n', kept);
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

    // The one place `validate`'s exit code is checked: 1 exactly when it found an error. The feature specs bypass
    // the CLI.
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

// Asserts the paths written and the descriptor, and no file bodies, which would be a second copy of the template.
// `new` runs `validate` before it returns, so exit 0 says the corpus it created validates.
void RunNewScenario(string name, string scenario)
{
    var temp = Path.Combine(Path.GetTempPath(), "kac-new-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(temp);
    try
    {
        var (stdout, stderr, exit) = Run(temp, "dotnet", kac, "new", "--yes",
            "--from", repoRoot, "--name", "acme-corpus", "--ci", "github");

        var descriptorPath = Path.Combine(temp, ".corpus.yaml");
        var listing = Listing(temp);
        var tree = string.Join('\n', listing) + "\n";
        var descriptor = File.Exists(descriptorPath)
            ? Stable(File.ReadAllText(descriptorPath), repoRoot)
            : "";

        var treePath = Path.Combine(scenario, "expected-tree.txt");
        var yamlPath = Path.Combine(scenario, "expected-descriptor.yaml");

        if (update)
        {
            File.WriteAllText(treePath, tree);
            File.WriteAllText(yamlPath, descriptor);
            Console.WriteLine($"UPDATE {name}  ({listing.Count} file(s), exit {exit})");
            return;
        }

        var problems = new List<string>();
        if (exit != 0) problems.Add($"expected a corpus that validates (exit 0), got exit {exit}");
        problems.AddRange(Differences("the file list", treePath, tree));
        problems.AddRange(Differences(".corpus.yaml", yamlPath, descriptor));

        if (problems.Count == 0)
        {
            Console.WriteLine($"ok     {name}  (created {listing.Count} file(s))");
        }
        else
        {
            failures.Add(name);
            Console.WriteLine($"FAIL   {name}");
            foreach (var p in problems) Console.WriteLine($"         {p}");
            foreach (var l in (stderr + stdout).Split('\n').Where(l => l.Trim().Length > 0).TakeLast(5))
                Console.WriteLine($"       | {l}");
        }
    }
    finally
    {
        TryDelete(temp);
    }
}

static List<string> Listing(string root) =>
[
    .. Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
        .Select(f => Path.GetRelativePath(root, f).Replace('\\', '/'))
        .Where(rel => rel != ".git" && !rel.StartsWith(".git/", StringComparison.Ordinal))
        .OrderBy(rel => rel, StringComparer.Ordinal)
];

// The descriptor without this machine's repository path and today's date.
static string Stable(string descriptor, string repoRoot) =>
    string.Join('\n', descriptor
        .Replace(repoRoot, "<repository>", StringComparison.Ordinal)
        .Replace("\r\n", "\n")
        .Split('\n')
        .Select(l => l.StartsWith("  taken-on:", StringComparison.Ordinal) ? "  taken-on: <today>" : l));

static List<string> Differences(string what, string expectedPath, string actual)
{
    if (!File.Exists(expectedPath))
        return [$"no expectation for {what} at {Path.GetFileName(expectedPath)}. run with --update, then read the diff"];

    var expected = File.ReadAllText(expectedPath).Replace("\r\n", "\n");
    if (expected == actual.Replace("\r\n", "\n")) return [];

    var wanted = expected.Split('\n');
    var got = actual.Replace("\r\n", "\n").Split('\n');

    return
    [
        $"{what} differs from {Path.GetFileName(expectedPath)}:",
        .. wanted.Except(got, StringComparer.Ordinal).Take(5).Select(l => $"  expected, not found: {l}"),
        .. got.Except(wanted, StringComparer.Ordinal).Take(5).Select(l => $"  found, not expected: {l}")
    ];
}

// `new` and `update` proved as a round trip: the promise pinned is that one leaves the other nothing to say.
void RunUpdateScenario(string name, string scenario)
{
    var temp = Path.Combine(Path.GetTempPath(), "kac-update-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(temp);
    try
    {
        Run(temp, "dotnet", kac, "new", "--yes", "--from", repoRoot, "--name", "acme-corpus", "--ci", "github");
        Commit(temp, "the corpus as created");

        // An overlay file edited and a seed deleted, which answer to different rules.
        File.AppendAllText(Path.Combine(temp, "knowledge-as-code.md"), "\nAn edit this corpus made.\n");
        File.Delete(Path.Combine(temp, ".editorconfig"));

        // A seeded record filed under a category folder is still in step. Offering it again would leave two copies of
        // one id.
        var filed = Path.Combine(temp, "policies", "governance");
        Directory.CreateDirectory(filed);
        File.Move(Path.Combine(temp, "policies", "devi-deviations.md"),
            Path.Combine(filed, "devi-deviations.md"));

        Commit(temp, "drift");

        var (checkOut, checkErr, checkExit) = Run(temp, "dotnet", kac, "update", "--check", "--from", repoRoot);
        var reported = checkOut + checkErr;

        var expectedPath = Path.Combine(scenario, "expected-check.txt");
        if (update)
        {
            File.WriteAllText(expectedPath, Reportable(reported, repoRoot));
            Console.WriteLine($"UPDATE {name}  (check exit {checkExit})");
            return;
        }

        var problems = new List<string>();
        if (checkExit != 1)
            problems.Add($"expected --check to fail on a corpus behind its framework (exit 1), got exit {checkExit}");
        problems.AddRange(Differences("what --check reported", expectedPath, Reportable(reported, repoRoot)));

        var (applyOut, applyErr, applyExit) = Run(temp, "dotnet", kac, "update", "--from", repoRoot);
        if (applyExit != 0)
            problems.Add($"expected the update to succeed (exit 0), got exit {applyExit}\n{applyErr}{applyOut}");

        // Nothing is committed, so the second check runs over the dirty tree as it stands.
        var (_, _, againExit) = Run(temp, "dotnet", kac, "update", "--check", "--from", repoRoot);
        if (againExit != 0)
            problems.Add($"expected the corpus to be in step after an update (exit 0), got exit {againExit}");

        if (problems.Count == 0)
        {
            Console.WriteLine($"ok     {name}  (behind, updated, in step)");
        }
        else
        {
            failures.Add(name);
            Console.WriteLine($"FAIL   {name}");
            foreach (var p in problems) Console.WriteLine($"         {p}");
        }
    }
    finally
    {
        TryDelete(temp);
    }
}

// Under an identity of this run's own, because the machine running the suite may have none configured.
void Commit(string dir, string message)
{
    Run(dir, "git", "add", "-A");
    Run(dir, "git", "-c", "user.email=kac@example.invalid", "-c", "user.name=kac", "commit", "-q", "-m", message);
}

// The output without the repository path and the tallies that move whenever the template gains a file.
static string Reportable(string output, string root) =>
    string.Join('\n', output.Replace(root, "<template>").Split('\n')
        .Select(l => l.TrimEnd())
        .Where(l => l.Length > 0)
        .Where(l => !l.Contains("file(s) compared", StringComparison.Ordinal))
        .Where(l => !l.Contains("withheld", StringComparison.Ordinal))) + "\n";

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

        problems.AddRange(CheckGoldenExport(name, dist, golden));

        // A file no record backs, left in the output for the second run to remove.
        var orphan = Path.Combine(dist, "orphan.json");
        Directory.CreateDirectory(FolderOf(orphan));
        File.WriteAllText(orphan, "{}\n");

        var (_, _, second) = Run(temp, "dotnet", argv);
        if (second != 0) problems.Add($"expected a clean second export (exit 0), got exit {second}");
        if (File.Exists(orphan)) problems.Add("a file no record backs survived a second export");

        // The second run against the same golden, so a value that varies between runs fails here.
        problems.AddRange(CheckGoldenExport(name, dist, golden).Select(p => $"second run: {p}"));

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

void RunPackScenario(string name, string scenario, string corpusDir)
{
    var entriesPath = Path.Combine(scenario, "expected-entries.txt");
    var nuspecPath = Path.Combine(scenario, "expected-nuspec.xml");
    var temp = AssembleTemp(schemaDir, corpusDir);
    try
    {
        var (_, exportErr, exportExit) = Run(temp, "dotnet", kac, "export");

        var problems = new List<string>();
        if (exportExit != 0) problems.Add($"the export the pack needs failed (exit {exportExit}): {exportErr}");

        var (stdout, stderr, exit) = Run(temp, "dotnet", kac, "pack");
        var output = stderr + stdout;
        var dir = Path.Combine(temp, ".dist", "package");

        // Without an archive the goldens below compare against an absence, and `--update` would bless it.
        var built = Directory.Exists(dir) ? Directory.GetFiles(dir, "*.nupkg") : [];
        if (exit != 0 || built.Length != 1)
        {
            failures.Add(name);
            Console.WriteLine($"FAIL   {name}");
            Console.WriteLine($"         expected exit 0 and one .nupkg, got exit {exit} and {built.Length}");
            foreach (var p in problems) Console.WriteLine($"         {p}");
            foreach (var l in output.Split('\n').Where(l => l.Trim().Length > 0)) Console.WriteLine($"       | {l}");
            return;
        }

        var archive = File.ReadAllBytes(built[0]);
        var entries = ZipEntries(archive);

        // Found by extension rather than built from the file name. A package id may carry a dot, so
        // splitting the file name on one would name a part the archive does not hold and report the
        // envelope as missing.
        var nuspec = entries.Keys.FirstOrDefault(k => k.EndsWith(".nuspec", StringComparison.Ordinal));
        if (nuspec is null)
        {
            failures.Add(name);
            Console.WriteLine($"FAIL   {name}");
            Console.WriteLine("         the package holds no .nuspec, so a registry cannot name or version it");
            return;
        }

        if (update)
        {
            File.WriteAllText(entriesPath, string.Join("\n", entries.Keys) + "\n");
            File.WriteAllText(nuspecPath, Encoding.UTF8.GetString(entries[nuspec]));
            Console.WriteLine(
                $"UPDATE {name}  ({entries.Count} entries, read the diff: it is what a consumer imports)");
            return;
        }

        var wanted = ReadLines(entriesPath).ToList();
        problems.AddRange(wanted.Where(e => !entries.ContainsKey(e))
            .Select(e => $"the package no longer holds {e}: what a consumer imports has changed"));
        problems.AddRange(entries.Keys.Where(e => !wanted.Contains(e))
            .Select(e => $"the package holds {e}, which the committed list does not: a new entry for a consumer"));

        // The envelope is the one part of the archive the corpus did not author, so nothing else would show it change.
        var got = Encoding.UTF8.GetString(entries[nuspec]);
        var want = File.Exists(nuspecPath) ? File.ReadAllText(nuspecPath) : null;
        if (want is null) problems.Add($"no committed envelope at {Path.GetFileName(nuspecPath)}");
        else if (!string.Equals(Unix(want), Unix(got), StringComparison.Ordinal))
            problems.Add($"{nuspec} differs from the committed envelope");

        problems.AddRange(SamePayload(Path.Combine(temp, ".dist", "export"), entries));

        // `pack` replaces its directory whole, so a version built and then bumped must not stay beside the new one.
        var orphan = Path.Combine(dir, "orphan.0.0.1.nupkg");
        File.WriteAllBytes(orphan, [0]);

        var (_, _, second) = Run(temp, "dotnet", kac, "pack");
        if (second != 0) problems.Add($"expected a clean second pack (exit 0), got exit {second}");
        if (File.Exists(orphan)) problems.Add("a package no run backs survived a second pack");

        // Two packs of one export are the same bytes, because a registry keeps a published version forever.
        if (!archive.SequenceEqual(File.ReadAllBytes(built[0])))
            problems.Add("two packs of one export produced different archives");

        problems.AddRange(ReadLines(Path.Combine(scenario, "expected-pack.txt"))
            .Where(l => !output.Contains(l)).Select(l => $"not reported: {l}"));

        problems.AddRange(CheckTree(temp, scenario));

        if (problems.Count == 0)
        {
            Console.WriteLine($"ok     {name}  (packed)");
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

// A committed document and a built one compared on their words rather than on the line endings a
// checkout gave them. The envelope is written with `\n` whatever platform built it, and git is free to
// hand the golden back with `\r\n`.
static string Unix(string content) => content.Replace("\r\n", "\n");

static SortedDictionary<string, byte[]> ZipEntries(byte[] archive)
{
    var entries = new SortedDictionary<string, byte[]>(StringComparer.Ordinal);
    using var zip = new ZipArchive(new MemoryStream(archive), ZipArchiveMode.Read);

    foreach (var entry in zip.Entries)
    {
        using var stream = entry.Open();
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        entries[entry.FullName] = buffer.ToArray();
    }

    return entries;
}

static List<string> SamePayload(string exportDir, SortedDictionary<string, byte[]> entries)
{
    const string payload = "corpus/";
    var problems = new List<string>();

    var carried = entries.Where(e => e.Key.StartsWith(payload, StringComparison.Ordinal))
        .ToDictionary(e => e.Key[payload.Length..], e => e.Value, StringComparer.Ordinal);

    foreach (var file in Directory.EnumerateFiles(exportDir, "*", SearchOption.AllDirectories))
    {
        var rel = Path.GetRelativePath(exportDir, file).Replace('\\', '/');
        if (!carried.TryGetValue(rel, out var got)) problems.Add($"the package carries no {rel}");
        else if (!File.ReadAllBytes(file).SequenceEqual(got))
            problems.Add($"the package's copy of {rel} differs from the export: pack edited it");
    }

    foreach (var rel in carried.Keys.Where(r =>
                 !File.Exists(Path.Combine(exportDir, r.Replace('/', Path.DirectorySeparatorChar)))))
        problems.Add($"the package carries {rel}, which the export does not");

    return problems;
}

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

        // `corpus-root` is the fixture plugin's `metadata.corpusRoot`, the one thing this cannot read for itself.
        var corpusRoot = File.ReadAllText(Path.Combine(scenario, "corpus-root")).Trim();
        problems.AddRange(SameTree(
            Path.Combine(temp, ".dist", "export"),
            Path.Combine(temp, ".dist", "plugin", corpusRoot)));

        problems.AddRange(CheckTree(temp, scenario));

        // A file no component backs, left in the plugin for the second run to remove.
        var orphan = Path.Combine(temp, ".dist", "plugin", "skills", "orphan", "SKILL.md");
        Directory.CreateDirectory(FolderOf(orphan));
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

// Bytes rather than text: a comparison normalising line endings would pass over the edit it exists to catch.
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

static List<string> CheckGoldenExport(string name, string dist, string golden)
{
    if (!Directory.Exists(golden))
        return [$"no committed export at {Path.GetFileName(golden)}/. "
                + $"Run: dotnet run tooling/kac-tests.cs -- --update {name}"];

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

// Replaces the committed export whole, or a file the export stopped writing reads as one it lost.
static int WriteGoldenExport(string dist, string golden)
{
    if (Directory.Exists(golden)) Directory.Delete(golden, recursive: true);

    var files = ExportTree(dist);
    foreach (var (rel, content) in files)
    {
        var full = Path.Combine(golden, rel.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(FolderOf(full));
        File.WriteAllText(full, content);
    }

    return files.Count;
}

// Both sides go through this, so the golden on disk is already normalised and the comparison is string equality.
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

// The value goes whatever it is, a null included, so the rule fires in a corpus with no commit too.
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

Console.WriteLine();

// Coverage is a property of the whole suite. A filtered run would undercount and read as a regression.
if (filters.Count == 0)
{
    // `type` fires only for a document in no type folder, and discovery never yields one, so no fixture reaches it.
    var unreachable = new HashSet<string>(["type"], StringComparer.Ordinal);

    var catalogue = CheckCatalogue(kac, libraryRoot);
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

    // `kac checks` verifies its curated rows against the catalogue and exits non-zero on drift.
    var (_, checksErr, checksExit) = Run(libraryRoot, "dotnet", kac, "checks");
    if (checksExit != 0)
    {
        Console.WriteLine($"  CHECKS TABLE out of step with the catalogue:\n{Indent(checksErr)}");
        failures.Add("(checks table vs catalogue)");
    }

    // From a folder with no corpus above it. A corpus lookup running before the parse passes every other test here.
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

// Found by the solution file, not a `.corpus.yaml`: the suite needs the engine, the fixtures and the corpus at once.
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

// The real schema plus the fixture corpus, outside git, so discovery walks the filesystem and no temp path reaches
// a golden. The caller deletes it.
static string AssembleTemp(string schemaDir, string corpusDir)
{
    var temp = Path.Combine(Path.GetTempPath(), "kac-tests-" + Guid.NewGuid().ToString("N"));
    CopyTree(schemaDir, Path.Combine(temp, ".schema"));
    MarkCorpus(temp);
    CopyTree(corpusDir, temp);
    return temp;
}

// A descriptor holding only a comment, which is what an absent one comes to. A fixture with something to declare
// copies its own over it.
static void MarkCorpus(string temp)
{
    Directory.CreateDirectory(temp);
    File.WriteAllText(Path.Combine(temp, ".corpus.yaml"),
        "# Written by the golden suite so that kac reads this tree as a corpus.\n");
}

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

static void Regenerate(string kac, string schemaDir, string corpusDir)
{
    var temp = AssembleTemp(schemaDir, corpusDir);
    try
    {
        var (stdout, stderr, exit) = Run(temp, "dotnet", kac, "generate");
        if (exit != 0) throw new Exception($"kac generate failed (exit {exit}).\n{Indent(stderr)}");

        // Only what the fixture owns comes back. `.schema/` and `.corpus.yaml` were put there to assemble the run.
        foreach (var dir in Directory.EnumerateDirectories(temp))
            if (Path.GetFileName(dir) is not ".schema")
                CopyTree(dir, Path.Combine(corpusDir, Path.GetFileName(dir)));
        foreach (var file in Directory.EnumerateFiles(temp))
            if (Path.GetFileName(file) is not ".corpus.yaml")
                File.Copy(file, Path.Combine(corpusDir, Path.GetFileName(file)), overwrite: true);
    }
    finally
    {
        TryDelete(temp);
    }
}

// Build the tool project and return the assembly to invoke, or null having said why. The path is read back with
// `--getProperty` rather than constructed, so it cannot disagree with the project's configuration or framework.
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
        set.Add(Text(c, "check"));
    return set;
}

static string FolderOf(string path) =>
    Path.GetDirectoryName(path) ?? throw new Exception($"'{path}' names no folder above it.");

static string Text(JsonElement element, string property) =>
    element.TryGetProperty(property, out var found) && found.GetString() is { } text
        ? text
        : throw new Exception($"Expected a '{property}' holding text, and found {element}.");

static Report ParseFindings(string json)
{
    using var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;
    var findings = new List<F>();
    foreach (var e in root.GetProperty("findings").EnumerateArray())
        findings.Add(new F(
            Text(e, "file"),
            e.GetProperty("line").ValueKind == JsonValueKind.Null ? null : e.GetProperty("line").GetInt32(),
            Text(e, "severity"),
            Text(e, "check"),
            Text(e, "message")));
    return new Report(findings);
}

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
    // A file still open leaves the tree under the system temp directory, which the machine clears.
    catch (Exception e) when (e is IOException or UnauthorizedAccessException)
    {
    }
}

static string Rel(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');
static string Indent(string s) => string.Join('\n', s.Split('\n').Select(l => "       " + l));

internal record Report(List<F> Findings);

internal record F(string File, int? Line, string Severity, string Check, string Message);
