using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using static kac.goldens.Suite;

namespace kac.goldens;

/// <summary>
/// One scenario per fixture, dispatched on the fixture's <c>mode</c> file. <c>tooling/tests/README.md</c> says what
/// each mode asserts.
/// </summary>
internal static class Runner
{
    /// <summary>An exception is reported in the outcome, not thrown.</summary>
    internal static Outcome RunScenario(string name)
    {
        var outcome = new Outcome();
        var scenario = Path.Combine(FixturesDir, name);
        var corpusDir = Path.Combine(scenario, "corpus");
        var modePath = Path.Combine(scenario, "mode");
        var mode = File.Exists(modePath) ? File.ReadAllText(modePath).Trim() : "validate";

        try
        {
            switch (mode)
            {
                case "validate":
                    RunValidateScenario(outcome, name, scenario, RunValidate(Kac, SchemaDir, corpusDir));
                    break;
                case "validate-since":
                    RunValidateScenario(outcome, name, scenario,
                        RunValidateSince(Kac, SchemaDir, Path.Combine(scenario, "base"), corpusDir));
                    break;
                case "generate":
                    RunGenerateScenario(outcome, name, scenario, mustBeStale: false);
                    break;
                case "generate-stale":
                    RunGenerateScenario(outcome, name, scenario, mustBeStale: true);
                    break;
                case "update":
                    RunUpdateScenario(outcome, name, scenario);
                    break;
                case "export":
                    RunExportScenario(outcome, name, scenario, corpusDir);
                    break;
                case "bundle":
                    RunBundleScenario(outcome, name, scenario, corpusDir);
                    break;
                case "pack":
                    RunPackScenario(outcome, name, scenario, corpusDir);
                    break;
                case "new":
                    RunNewScenario(outcome, name, scenario);
                    break;
                case "report":
                    RunReportScenario(outcome, name, corpusDir);
                    break;
                case "restore":
                    RunRestoreScenario(outcome, name, scenario, corpusDir);
                    break;
                default:
                    outcome.Fail();
                    outcome.Say($"ERROR  {name}: unknown mode '{mode}'");
                    break;
            }
        }
        catch (Exception ex)
        {
            outcome.Fail();
            outcome.Say($"ERROR  {name}\n       {ex.Message}");
        }

        return outcome;
    }

    /// <summary>The command that regenerates one scenario's goldens, for a message to print.</summary>
    private static string UpdateCommand(string name) =>
        $"KAC_UPDATE_GOLDENS=1 dotnet test --project tooling/kac.goldens --filter-trait \"Scenario={name}\"";

    /// <summary>
    /// <c>report --out</c> asserted against the same run's stdout rather than a golden, because the frontmatter has
    /// the moment the run happened.
    /// </summary>
    private static void RunReportScenario(Outcome outcome, string name, string corpusDir)
    {
        var temp = AssembleTemp(SchemaDir, corpusDir);
        try
        {
            var target = Path.Combine("reports", "clause-coverage.md");
            var full = Path.Combine(temp, target);

            var (printed, _, printExit) = Run(temp, "dotnet", Kac, "report", "coverage");
            var (wroteOut, _, writeExit) = Run(temp, "dotnet", Kac, "report", "coverage", "--out", target);
            var written = File.Exists(full) ? File.ReadAllText(full) : "";
            var (_, refuseErr, refuseExit) = Run(temp, "dotnet", Kac, "report", "coverage", "--out", target);
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
            if (written.Contains('\r', StringComparison.Ordinal))
                problems.Add("the file ends a line with a carriage return, so it differs from the Linux build");
            if (!written.Contains("| `WRITTEN` | MUST | [`std-COVER`][std-COVER]`.NAMED` |", StringComparison.Ordinal))
                problems.Add("the covered clause did not reach the file");
            if (!written.Contains("\n[std-COVER]: ../standards/coverage.md\n", StringComparison.Ordinal))
                problems.Add("the file defines no link from reports/ to the standard it cites");
            if (!written.Contains("\n### [eng:pol-HELD][eng:pol-HELD]\n", StringComparison.Ordinal))
                problems.Add("the imported policy's heading does not link the page its producer published");
            if (!written.Contains("Nothing imported from `old` states the page", StringComparison.Ordinal))
                problems.Add("the limits do not name the import that states no page");

            if (refuseExit != 1) problems.Add($"a second --out over the same path exited {refuseExit}, not 1");
            if (!refuseErr.Contains("already exists", StringComparison.Ordinal))
                problems.Add($"the refusal did not say the path is taken: {refuseErr.Trim()}");
            if (after != written) problems.Add("the refused run changed the file it refused to write");

            if (problems.Count > 0)
            {
                outcome.Fail();
                outcome.Say($"FAIL   {name}");
                foreach (var problem in problems) outcome.Say($"         {problem}");
                return;
            }

            outcome.Say($"ok     {name}  (wrote {target}, refused the second run)");
        }
        finally
        {
            Directory.Delete(temp, true);
        }
    }

    /// <summary>
    /// The report without its stamp. <c>generated:</c> opens a block, so the key and the <c>at:</c> under it both go.
    /// </summary>
    private static string Stamped(string report)
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

    private static void RunValidateScenario(Outcome outcome, string name, string scenario,
        (string Json, int Exit) run)
    {
        var expectedPath = Path.Combine(scenario, "expected.json");
        var (actualJson, actualExit) = run;
        var actual = ParseFindings(actualJson);
        foreach (var f in actual.Findings) CoveredChecks.TryAdd(f.Check, 0);

        if (Updating)
        {
            File.WriteAllText(expectedPath, actualJson.TrimEnd('\n') + "\n");
            outcome.Say($"UPDATE {name}  ({actual.Findings.Count} finding(s))");
            return;
        }

        if (!File.Exists(expectedPath))
        {
            outcome.Fail();
            outcome.Say($"MISSING {name}: no golden. Run: {UpdateCommand(name)}");
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
            outcome.Say($"ok     {name}  ({actual.Findings.Count} finding(s))");
        }
        else
        {
            outcome.Fail();
            outcome.Say($"FAIL   {name}");
            foreach (var line in diff) outcome.Say($"         {line}");
        }
    }

    private static void RunGenerateScenario(Outcome outcome, string name, string scenario, bool mustBeStale)
    {
        var corpusDir = Path.Combine(scenario, "corpus");

        if (Updating && !mustBeStale)
        {
            Regenerate(Kac, SchemaDir, corpusDir);
            var (rexit, _) = RunGenerate(Kac, SchemaDir, corpusDir, check: true);
            outcome.Say(rexit == 0
                ? $"UPDATE {name}  (regenerated, fresh)"
                : $"UPDATE {name}  (WARNING: still stale after regen)");
            return;
        }

        if (Updating)
        {
            outcome.Say($"UPDATE {name}  (stale fixture, nothing to regenerate)");
            return;
        }

        var (exit, output) = RunGenerate(Kac, SchemaDir, corpusDir, check: true);

        if (mustBeStale)
        {
            if (exit == 0)
            {
                outcome.Fail();
                outcome.Say($"FAIL   {name}: expected generate --check to detect staleness, but it exited 0");
                return;
            }

            var expectedStale = Path.Combine(scenario, "expected-stale.txt");
            var missing = File.Exists(expectedStale)
                ? File.ReadAllLines(expectedStale).Select(l => l.Trim()).Where(l => l.Length > 0)
                    .Where(l => !output.Contains(l)).ToList()
                : [];
            if (missing.Count > 0)
            {
                outcome.Fail();
                outcome.Say($"FAIL   {name}: stale detected but these files were not named: {string.Join(", ", missing)}");
            }
            else
            {
                outcome.Say($"ok     {name}  (staleness detected)");
            }

            return;
        }

        if (exit == 0)
        {
            outcome.Say($"ok     {name}  (generated output fresh)");
        }
        else
        {
            outcome.Fail();
            outcome.Say(
                $"FAIL   {name}: committed generated files do not match the generator. Run: {UpdateCommand(name)}");
            foreach (var l in output.Split('\n').Where(l => l.Trim().Length > 0)) outcome.Say($"         {l}");
        }
    }

    /// <summary>
    /// Asserts the paths written and the descriptor, and no file bodies, which would be a second copy of the template.
    /// <c>new</c> runs <c>validate</c> before it returns, so exit 0 says the corpus it created validates.
    /// </summary>
    private static void RunNewScenario(Outcome outcome, string name, string scenario)
    {
        var temp = Path.Combine(Path.GetTempPath(), "kac-new-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temp);
        try
        {
            var (stdout, stderr, exit) = Run(temp, "dotnet", Kac, "new", "--yes",
                "--from", RepoRoot, "--name", "acme-corpus", "--ci", "github");

            var descriptorPath = Path.Combine(temp, ".corpus.yaml");
            var listing = Listing(temp);
            var tree = string.Join('\n', listing) + "\n";
            var descriptor = File.Exists(descriptorPath)
                ? Stable(File.ReadAllText(descriptorPath), RepoRoot)
                : "";

            var treePath = Path.Combine(scenario, "expected-tree.txt");
            var yamlPath = Path.Combine(scenario, "expected-descriptor.yaml");

            if (Updating)
            {
                File.WriteAllText(treePath, tree);
                File.WriteAllText(yamlPath, descriptor);
                outcome.Say($"UPDATE {name}  ({listing.Count} file(s), exit {exit})");
                return;
            }

            var problems = new List<string>();
            if (exit != 0) problems.Add($"expected a corpus that validates (exit 0), got exit {exit}");
            problems.AddRange(Differences("the file list", treePath, tree));
            problems.AddRange(Differences(".corpus.yaml", yamlPath, descriptor));

            if (problems.Count == 0)
            {
                outcome.Say($"ok     {name}  (created {listing.Count} file(s))");
            }
            else
            {
                outcome.Fail();
                outcome.Say($"FAIL   {name}");
                foreach (var p in problems) outcome.Say($"         {p}");
                foreach (var l in (stderr + stdout).Split('\n').Where(l => l.Trim().Length > 0).TakeLast(5))
                    outcome.Say($"       | {l}");
            }
        }
        finally
        {
            TryDelete(temp);
        }
    }

    /// <summary>
    /// The consumer is a folder beside the fixture's <c>producer/</c>, so its <c>source:</c> is a relative path out of
    /// the corpus.
    /// </summary>
    private static void RunRestoreScenario(Outcome outcome, string name, string scenario, string corpusDir)
    {
        var temp = Path.Combine(Path.GetTempPath(), "kac-restore-" + Guid.NewGuid().ToString("N"));
        var consumer = Path.Combine(temp, "consumer");
        try
        {
            CopyTree(SchemaDir, Path.Combine(consumer, ".schema"));
            CopyTree(corpusDir, consumer);
            CopyTree(Path.Combine(scenario, "producer"), Path.Combine(temp, "producer"));

            var (stdout, stderr, exit) = Run(consumer, "dotnet", Kac, "restore");

            var imports = Path.Combine(consumer, ".imports");
            var listing = Directory.Exists(imports) ? Listing(imports).Select(rel => ".imports/" + rel).ToList() : [];
            var tree = string.Join('\n', listing) + "\n";
            var descriptor = File.ReadAllText(Path.Combine(consumer, ".corpus.yaml")).Replace("\r\n", "\n");

            var treePath = Path.Combine(scenario, "expected-tree.txt");
            var yamlPath = Path.Combine(scenario, "expected-descriptor.yaml");

            if (Updating)
            {
                File.WriteAllText(treePath, tree);
                File.WriteAllText(yamlPath, descriptor);
                outcome.Say($"UPDATE {name}  ({listing.Count} file(s), exit {exit})");
                return;
            }

            var problems = new List<string>();
            if (exit != 0) problems.Add($"expected the restore to succeed (exit 0), got exit {exit}");
            problems.AddRange(Differences("the files under .imports/", treePath, tree));
            problems.AddRange(Differences(".corpus.yaml", yamlPath, descriptor));

            if (problems.Count == 0)
            {
                outcome.Say($"ok     {name}  (restored {listing.Count} file(s))");
            }
            else
            {
                outcome.Fail();
                outcome.Say($"FAIL   {name}");
                foreach (var p in problems) outcome.Say($"         {p}");
                foreach (var l in (stderr + stdout).Split('\n').Where(l => l.Trim().Length > 0))
                    outcome.Say($"       | {l}");
            }
        }
        finally
        {
            TryDelete(temp);
        }
    }

    private static List<string> Listing(string root) =>
    [
        .. Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(root, f).Replace('\\', '/'))
            .Where(rel => rel != ".git" && !rel.StartsWith(".git/", StringComparison.Ordinal))
            .OrderBy(rel => rel, StringComparer.Ordinal)
    ];

    /// <summary>The descriptor without this machine's repository path and today's date.</summary>
    private static string Stable(string descriptor, string repoRoot) =>
        string.Join('\n', descriptor
            .Replace(repoRoot, "<repository>", StringComparison.Ordinal)
            .Replace("\r\n", "\n")
            .Split('\n')
            .Select(l => l.StartsWith("  taken-on:", StringComparison.Ordinal) ? "  taken-on: <today>" : l));

    private static List<string> Differences(string what, string expectedPath, string actual)
    {
        if (!File.Exists(expectedPath))
            return
            [
                $"no expectation for {what} at {Path.GetFileName(expectedPath)}. "
                + "run with KAC_UPDATE_GOLDENS=1, then read the diff"
            ];

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

    /// <summary>
    /// <c>new</c> and <c>update</c> proved as a round trip: the promise pinned is that one leaves the other nothing to
    /// say.
    /// </summary>
    private static void RunUpdateScenario(Outcome outcome, string name, string scenario)
    {
        var temp = Path.Combine(Path.GetTempPath(), "kac-update-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temp);
        try
        {
            Run(temp, "dotnet", Kac, "new", "--yes", "--from", RepoRoot,
                "--name", "acme-corpus", "--ci", "github");
            Commit(temp, "the corpus as created");

            // An overlay file edited and a seed deleted, which answer to different rules.
            File.AppendAllText(Path.Combine(temp, "knowledge-as-code.md"), "\nAn edit this corpus made.\n");
            File.Delete(Path.Combine(temp, ".editorconfig"));

            // A seeded record filed under a category folder is still in step. Offering it again would leave two
            // copies of one id.
            var filed = Path.Combine(temp, "policies", "governance");
            Directory.CreateDirectory(filed);
            File.Move(Path.Combine(temp, "policies", "devi-deviations.md"),
                Path.Combine(filed, "devi-deviations.md"));

            Commit(temp, "drift");

            var (checkOut, checkErr, checkExit) =
                Run(temp, "dotnet", Kac, "update", "--check", "--from", RepoRoot);
            var reported = checkOut + checkErr;

            var expectedPath = Path.Combine(scenario, "expected-check.txt");
            if (Updating)
            {
                File.WriteAllText(expectedPath, Reportable(reported, RepoRoot));
                outcome.Say($"UPDATE {name}  (check exit {checkExit})");
                return;
            }

            var problems = new List<string>();
            if (checkExit != 1)
                problems.Add(
                    $"expected --check to fail on a corpus behind its framework (exit 1), got exit {checkExit}");
            problems.AddRange(Differences("what --check reported", expectedPath, Reportable(reported, RepoRoot)));

            var (applyOut, applyErr, applyExit) = Run(temp, "dotnet", Kac, "update", "--from", RepoRoot);
            if (applyExit != 0)
                problems.Add($"expected the update to succeed (exit 0), got exit {applyExit}\n{applyErr}{applyOut}");

            // Nothing is committed, so the second check runs over the dirty tree as it stands.
            var (_, _, againExit) = Run(temp, "dotnet", Kac, "update", "--check", "--from", RepoRoot);
            if (againExit != 0)
                problems.Add($"expected the corpus to be in step after an update (exit 0), got exit {againExit}");

            if (problems.Count == 0)
            {
                outcome.Say($"ok     {name}  (behind, updated, in step)");
            }
            else
            {
                outcome.Fail();
                outcome.Say($"FAIL   {name}");
                foreach (var p in problems) outcome.Say($"         {p}");
            }
        }
        finally
        {
            TryDelete(temp);
        }
    }

    /// <summary>
    /// Under an identity of this run's own, because the machine running the suite may have none configured.
    /// </summary>
    private static void Commit(string dir, string message)
    {
        Run(dir, "git", "add", "-A");
        Run(dir, "git", "-c", "user.email=kac@example.invalid", "-c", "user.name=kac",
            "commit", "-q", "-m", message);
    }

    /// <summary>
    /// The output without the repository path and the tallies that move whenever the template gains a file.
    /// </summary>
    private static string Reportable(string output, string root) =>
        string.Join('\n', output.Replace(root, "<template>").Split('\n')
            .Select(l => l.TrimEnd())
            .Where(l => l.Length > 0)
            .Where(l => !l.Contains("file(s) compared", StringComparison.Ordinal))
            .Where(l => !l.Contains("withheld", StringComparison.Ordinal))) + "\n";

    private static void RunExportScenario(Outcome outcome, string name, string scenario, string corpusDir)
    {
        var typePath = Path.Combine(scenario, "export-type");
        var type = File.Exists(typePath) ? File.ReadAllText(typePath).Trim() : null;
        var golden = Path.Combine(scenario, "expected-dist");
        var temp = AssembleTemp(SchemaDir, corpusDir);
        try
        {
            string[] argv = type is null ? [Kac, "export"] : [Kac, "export", "--type", type];
            var (stdout, stderr, exit) = Run(temp, "dotnet", argv);
            var output = stderr + stdout;
            var dist = Path.Combine(temp, ".dist", "export");

            if (Updating)
            {
                if (exit != 0)
                {
                    outcome.Say($"UPDATE {name}: WARNING: export failed (exit {exit}), golden left alone");
                    return;
                }

                var written = WriteGoldenExport(dist, golden);
                outcome.Say($"UPDATE {name}  ({written} export file(s), read the diff: it is the published shape)");
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
                outcome.Say($"ok     {name}  (exported)");
            }
            else
            {
                outcome.Fail();
                outcome.Say($"FAIL   {name}");
                foreach (var p in problems) outcome.Say($"         {p}");
                foreach (var l in output.Split('\n').Where(l => l.Trim().Length > 0)) outcome.Say($"       | {l}");
            }
        }
        finally
        {
            TryDelete(temp);
        }
    }

    private static void RunPackScenario(Outcome outcome, string name, string scenario, string corpusDir)
    {
        var entriesPath = Path.Combine(scenario, "expected-entries.txt");
        var nuspecPath = Path.Combine(scenario, "expected-nuspec.xml");
        var temp = AssembleTemp(SchemaDir, corpusDir);
        try
        {
            var (_, exportErr, exportExit) = Run(temp, "dotnet", Kac, "export");

            var problems = new List<string>();
            if (exportExit != 0) problems.Add($"the export the pack needs failed (exit {exportExit}): {exportErr}");

            var (stdout, stderr, exit) = Run(temp, "dotnet", Kac, "pack");
            var output = stderr + stdout;
            var dir = Path.Combine(temp, ".dist", "package");

            // Without an archive the goldens below compare against an absence, and an update would bless it.
            var built = Directory.Exists(dir) ? Directory.GetFiles(dir, "*.nupkg") : [];
            if (exit != 0 || built.Length != 1)
            {
                outcome.Fail();
                outcome.Say($"FAIL   {name}");
                outcome.Say($"         expected exit 0 and one .nupkg, got exit {exit} and {built.Length}");
                foreach (var p in problems) outcome.Say($"         {p}");
                foreach (var l in output.Split('\n').Where(l => l.Trim().Length > 0)) outcome.Say($"       | {l}");
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
                outcome.Fail();
                outcome.Say($"FAIL   {name}");
                outcome.Say("         the package holds no .nuspec, so a registry cannot name or version it");
                return;
            }

            if (Updating)
            {
                File.WriteAllText(entriesPath, string.Join("\n", entries.Keys) + "\n");
                File.WriteAllText(nuspecPath, Encoding.UTF8.GetString(entries[nuspec]));
                outcome.Say(
                    $"UPDATE {name}  ({entries.Count} entries, read the diff: it is what a consumer imports)");
                return;
            }

            var wanted = ReadLines(entriesPath).ToList();
            problems.AddRange(wanted.Where(e => !entries.ContainsKey(e))
                .Select(e => $"the package no longer holds {e}: what a consumer imports has changed"));
            problems.AddRange(entries.Keys.Where(e => !wanted.Contains(e))
                .Select(e => $"the package holds {e}, which the committed list does not: a new entry for a consumer"));

            // The envelope is the one part of the archive the corpus did not author, so nothing else would show it
            // change.
            var got = Encoding.UTF8.GetString(entries[nuspec]);
            var want = File.Exists(nuspecPath) ? File.ReadAllText(nuspecPath) : null;
            if (want is null) problems.Add($"no committed envelope at {Path.GetFileName(nuspecPath)}");
            else if (!string.Equals(Unix(want), Unix(got), StringComparison.Ordinal))
                problems.Add($"{nuspec} differs from the committed envelope");

            problems.AddRange(SamePayload(Path.Combine(temp, ".dist", "export"), entries));

            // `pack` replaces its directory whole, so a version built and then bumped must not stay beside the new one.
            var orphan = Path.Combine(dir, "orphan.0.0.1.nupkg");
            File.WriteAllBytes(orphan, [0]);

            var (_, _, second) = Run(temp, "dotnet", Kac, "pack");
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
                outcome.Say($"ok     {name}  (packed)");
            }
            else
            {
                outcome.Fail();
                outcome.Say($"FAIL   {name}");
                foreach (var p in problems) outcome.Say($"         {p}");
                foreach (var l in output.Split('\n').Where(l => l.Trim().Length > 0)) outcome.Say($"       | {l}");
            }
        }
        finally
        {
            TryDelete(temp);
        }
    }

    /// <summary>
    /// A committed document and a built one compared on their words rather than on the line endings a checkout gave
    /// them. The envelope is written with <c>\n</c> whatever platform built it, and git is free to hand the golden back
    /// with <c>\r\n</c>.
    /// </summary>
    private static string Unix(string content) => content.Replace("\r\n", "\n");

    private static SortedDictionary<string, byte[]> ZipEntries(byte[] archive)
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

    private static List<string> SamePayload(string exportDir, SortedDictionary<string, byte[]> entries)
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

    private static void RunBundleScenario(Outcome outcome, string name, string scenario, string corpusDir)
    {
        var pluginDir = Path.Combine(scenario, "plugin");
        if (!Directory.Exists(pluginDir))
        {
            outcome.Fail();
            outcome.Say($"ERROR  {name}: no plugin/ tree in the fixture");
            return;
        }

        if (Updating)
        {
            outcome.Say($"UPDATE {name}  (bundle scenario, nothing to regenerate)");
            return;
        }

        var typePath = Path.Combine(scenario, "export-type");
        var type = File.Exists(typePath) ? File.ReadAllText(typePath).Trim() : null;
        var temp = AssembleTemp(SchemaDir, corpusDir);
        try
        {
            CopyTree(pluginDir, Path.Combine(temp, ".plugin"));

            string[] exportArgv = type is null ? [Kac, "export"] : [Kac, "export", "--type", type];
            var (_, exportErr, exportExit) = Run(temp, "dotnet", exportArgv);

            var problems = new List<string>();
            if (exportExit != 0) problems.Add($"the export the bundle needs failed (exit {exportExit}): {exportErr}");

            var (stdout, stderr, exit) = Run(temp, "dotnet", Kac, "bundle");
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

            var (_, _, second) = Run(temp, "dotnet", Kac, "bundle");
            if (second != 0) problems.Add($"expected a clean second bundle (exit 0), got exit {second}");
            if (File.Exists(orphan)) problems.Add("a file no component backs survived a second bundle");

            if (problems.Count == 0)
            {
                outcome.Say($"ok     {name}  (bundled)");
            }
            else
            {
                outcome.Fail();
                outcome.Say($"FAIL   {name}");
                foreach (var p in problems) outcome.Say($"         {p}");
                foreach (var l in output.Split('\n').Where(l => l.Trim().Length > 0)) outcome.Say($"       | {l}");
            }
        }
        finally
        {
            TryDelete(temp);
        }
    }

    /// <summary>
    /// Bytes rather than text: a comparison normalising line endings would pass over the edit it exists to catch.
    /// </summary>
    private static List<string> SameTree(string expected, string actual)
    {
        if (!Directory.Exists(actual)) return [$"the bundle wrote no copy of the export at {actual}"];

        var problems = new List<string>();
        var want = TreeBytes(expected);
        var got = TreeBytes(actual);

        foreach (var rel in want.Keys.Union(got.Keys, StringComparer.Ordinal).OrderBy(p => p, StringComparer.Ordinal))
        {
            if (!got.TryGetValue(rel, out var b)) problems.Add($"the copied export is missing {rel}");
            else if (!want.TryGetValue(rel, out var a))
                problems.Add($"the copied export holds {rel}, which the export does not");
            else if (!a.SequenceEqual(b))
                problems.Add($"the copied export differs from the export at {rel}: bundle edited it");
        }

        return problems;
    }

    private static SortedDictionary<string, byte[]> TreeBytes(string root)
    {
        var files = new SortedDictionary<string, byte[]>(StringComparer.Ordinal);
        if (!Directory.Exists(root)) return files;

        foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            files[Path.GetRelativePath(root, file).Replace('\\', '/')] = File.ReadAllBytes(file);

        return files;
    }

    private static List<string> CheckTree(string root, string scenario)
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

    private static List<string> CheckGoldenExport(string name, string dist, string golden)
    {
        if (!Directory.Exists(golden))
            return [$"no committed export at {Path.GetFileName(golden)}/. Run: {UpdateCommand(name)}"];

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

    /// <summary>
    /// Replaces the committed export whole, or a file the export stopped writing reads as one it lost.
    /// </summary>
    private static int WriteGoldenExport(string dist, string golden)
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

    /// <summary>
    /// Both sides go through this, so the golden on disk is already normalised and the comparison is string equality.
    /// </summary>
    private static SortedDictionary<string, string> ExportTree(string root)
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

    /// <summary>
    /// The value goes whatever it is, a null included, so the rule fires in a corpus with no commit too.
    /// </summary>
    private static string Normalise(string rel, string content)
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

    private static string FirstDifference(string expected, string actual)
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

    private static List<string> ReadLines(string path) =>
        File.Exists(path)
            ? [.. File.ReadAllLines(path).Select(l => l.Trim()).Where(l => l.Length > 0 && !l.StartsWith('#'))]
            : [];

    /// <summary>
    /// The real schema plus the fixture corpus, outside git, so discovery walks the filesystem and no temp path reaches
    /// a golden. The caller deletes it.
    /// </summary>
    private static string AssembleTemp(string schemaDir, string corpusDir)
    {
        var temp = Path.Combine(Path.GetTempPath(), "kac-tests-" + Guid.NewGuid().ToString("N"));
        CopyTree(schemaDir, Path.Combine(temp, ".schema"));
        MarkCorpus(temp);
        CopyTree(corpusDir, temp);
        return temp;
    }

    /// <summary>
    /// A descriptor holding only a comment, which is what an absent one comes to. A fixture with something to declare
    /// copies its own over it.
    /// </summary>
    private static void MarkCorpus(string temp)
    {
        Directory.CreateDirectory(temp);
        File.WriteAllText(Path.Combine(temp, ".corpus.yaml"),
            "# Written by the golden suite so that kac reads this tree as a corpus.\n");
    }

    private static (string json, int exit) RunValidate(string kac, string schemaDir, string corpusDir)
    {
        var temp = AssembleTemp(schemaDir, corpusDir);
        try
        {
            var (stdout, stderr, exit) = Run(temp, "dotnet", kac, "validate", "--json");
            var json = FromFirstBrace(stdout)
                ?? throw new InvalidOperationException(
                    $"kac produced no JSON (exit {exit}).\n{Indent(stderr)}\n{Indent(stdout)}");
            return (json, exit);
        }
        finally
        {
            TryDelete(temp);
        }
    }

    /// <summary>
    /// <c>validate --since HEAD</c>, where <c>HEAD</c> commits <paramref name="baseDir"/> and the working tree holds
    /// <paramref name="corpusDir"/> in its place. The repository is the temp root, so paths stay corpus-relative.
    /// </summary>
    private static (string json, int exit) RunValidateSince(string kac, string schemaDir, string baseDir,
        string corpusDir)
    {
        var temp = AssembleTemp(schemaDir, baseDir);
        try
        {
            Run(temp, "git", "init", "-q");
            Commit(temp, "base");

            foreach (var dir in Directory.EnumerateDirectories(temp))
                if (Path.GetFileName(dir) is not (".git" or ".schema"))
                    Directory.Delete(dir, recursive: true);
            foreach (var file in Directory.EnumerateFiles(temp))
                if (Path.GetFileName(file) is not ".corpus.yaml")
                    File.Delete(file);
            CopyTree(corpusDir, temp);

            var (stdout, stderr, exit) = Run(temp, "dotnet", kac, "validate", "--json", "--since", "HEAD");
            var json = FromFirstBrace(stdout)
                ?? throw new InvalidOperationException(
                    $"kac produced no JSON (exit {exit}).\n{Indent(stderr)}\n{Indent(stdout)}");
            return (json, exit);
        }
        finally
        {
            TryDelete(temp);
        }
    }

    /// <summary>
    /// What <c>kac validate</c> printed for a scenario's corpus under <paramref name="environment"/>. The temp is
    /// outside git, so each path in an annotation stays relative to the corpus.
    /// </summary>
    internal static string ValidateUnder(string scenario, IReadOnlyDictionary<string, string> environment)
    {
        var temp = AssembleTemp(SchemaDir, Path.Combine(FixturesDir, scenario, "corpus"));
        try
        {
            return RunUnder(environment, temp, "dotnet", Kac, "validate").stdout;
        }
        finally
        {
            TryDelete(temp);
        }
    }

    private static (int exit, string output) RunGenerate(string kac, string schemaDir, string corpusDir, bool check)
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

    private static void Regenerate(string kac, string schemaDir, string corpusDir)
    {
        var temp = AssembleTemp(schemaDir, corpusDir);
        try
        {
            var (_, stderr, exit) = Run(temp, "dotnet", kac, "generate");
            if (exit != 0) throw new InvalidOperationException($"kac generate failed (exit {exit}).\n{Indent(stderr)}");

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

    internal static IReadOnlySet<string> CheckCatalogue(string kac, string corpusRoot)
    {
        var (stdout, stderr, exit) = Run(corpusRoot, "dotnet", kac, "checks", "--json");
        var json = FromFirstBrace(stdout)
            ?? throw new InvalidOperationException($"kac checks produced no JSON (exit {exit}).\n{stderr}");
        using var doc = JsonDocument.Parse(json);
        var set = new HashSet<string>(StringComparer.Ordinal);
        foreach (var c in doc.RootElement.GetProperty("checks").EnumerateArray())
            set.Add(Text(c, "check"));
        return set;
    }

    /// <summary>
    /// From the parser's own XML documentation, the source <c>kac.tests/CliReference.cs</c> reads, in the order it
    /// declares them.
    /// </summary>
    internal static List<string> DeclaredVerbs(string kac, string corpusRoot)
    {
        var (stdout, stderr, exit) = Run(corpusRoot, "dotnet", kac, "cli", "xmldoc");
        if (exit != 0) throw new InvalidOperationException($"kac cli xmldoc exited {exit}.\n{stderr}");

        var root = XDocument.Parse(stdout).Root
            ?? throw new InvalidOperationException("kac cli xmldoc printed no root element.");
        return [.. root.Elements("Command").Select(c => c.Attribute("Name")?.Value
            ?? throw new InvalidOperationException($"kac cli xmldoc printed a command with no name: {c}"))];
    }

    private static string FolderOf(string path) =>
        Path.GetDirectoryName(path) ?? throw new InvalidOperationException($"'{path}' names no folder above it.");

    private static string Text(JsonElement element, string property) =>
        element.TryGetProperty(property, out var found) && found.GetString() is { } text
            ? text
            : throw new InvalidOperationException($"Expected a '{property}' holding text, and found {element}.");

    private static Report ParseFindings(string json)
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

    private static List<string> Diff(Report expected, Report actual)
    {
        static string Key(F f) => $"{f.File}:{f.Line?.ToString() ?? "-"}  {f.Severity}  [{f.Check}] {f.Message}";
        var exp = expected.Findings.Select(Key).ToList();
        var act = actual.Findings.Select(Key).ToList();
        var expSet = new HashSet<string>(exp);
        var actSet = new HashSet<string>(act);
        var lines = new List<string>();
        foreach (var k in exp.Where(k => !actSet.Contains(k))) lines.Add($"- expected but not produced: {k}");
        foreach (var k in act.Where(k => !expSet.Contains(k))) lines.Add($"+ produced but not expected: {k}");
        return lines;
    }

    private static string? FromFirstBrace(string s)
    {
        var i = s.IndexOf('{');
        return i < 0 ? null : s[i..];
    }

    private static void CopyTree(string src, string dst)
    {
        Directory.CreateDirectory(dst);
        foreach (var dir in Directory.EnumerateDirectories(src, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(Path.Combine(dst, Path.GetRelativePath(src, dir)));
        foreach (var file in Directory.EnumerateFiles(src, "*", SearchOption.AllDirectories))
            File.Copy(file, Path.Combine(dst, Path.GetRelativePath(src, file)), overwrite: true);
    }

    /// <summary>
    /// A refused delete clears each read-only flag bar a link's and tries again, as <c>Files.DeleteTree</c> in
    /// <c>kac.core</c> does, so a <c>.git</c> folder deletes on Windows. This project does not reference
    /// <c>kac.core</c>, so the loop is its own.
    /// </summary>
    internal static void TryDelete(string dir)
    {
        if (!Directory.Exists(dir)) return;

        try
        {
            Directory.Delete(dir, recursive: true);
            return;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
        }

        try
        {
            var root = new DirectoryInfo(dir);
            var everything = new EnumerationOptions { RecurseSubdirectories = true, AttributesToSkip = 0 };
            foreach (var entry in root.EnumerateFileSystemInfos("*", everything).Prepend(root))
                if (entry.LinkTarget is null && entry.Attributes.HasFlag(FileAttributes.ReadOnly))
                    entry.Attributes &= ~FileAttributes.ReadOnly;

            Directory.Delete(dir, recursive: true);
        }
        // A file still open leaves the tree under the system temp directory, which the machine clears.
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
        }
    }

    private sealed record Report(List<F> Findings);

    private sealed record F(string File, int? Line, string Severity, string Check, string Message);
}
