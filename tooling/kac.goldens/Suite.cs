using System.Collections.Concurrent;
using System.Diagnostics;

namespace kac.goldens;

/// <summary>
/// What every scenario and gate shares: the repository's paths, the one build of <c>kac</c>, and each scenario's
/// outcome. A scenario runs once, whichever case asks for it first. <c>tooling/tests/README.md</c> says how a scenario
/// runs, what each mode asserts and what every fixture covers.
/// </summary>
internal static class Suite
{
    internal static readonly string RepoRoot = FindRepoRoot();

    /// <summary>
    /// The real schema rather than a committed copy, so a schema edit that changes behaviour breaks a golden in the
    /// same run.
    /// </summary>
    internal static readonly string SchemaDir = Path.Combine(RepoRoot, ".schema");

    internal static readonly string FixturesDir = Path.Combine(RepoRoot, "tooling", "tests", "fixtures");

    /// <summary>
    /// The corpus the coverage and checks-table gates run <c>kac checks</c> in. <c>kac checks</c> reads the schema and
    /// never the corpus's adoption, so any corpus here answers the same.
    /// </summary>
    internal static readonly string LibraryRoot = Path.Combine(RepoRoot, "examples", "library");

    /// <summary>Every fixture directory holding a corpus or a mode, by name, in ordinal order.</summary>
    internal static readonly IReadOnlyList<string> Scenarios =
    [
        .. Directory.EnumerateDirectories(FixturesDir)
            .Where(d => Directory.Exists(Path.Combine(d, "corpus")) || File.Exists(Path.Combine(d, "mode")))
            .Select(Path.GetFileName)
            .OfType<string>()
            .Order(StringComparer.Ordinal)
    ];

    /// <summary>
    /// Whether a run writes the goldens from what <c>kac</c> printed instead of comparing against them. Read like
    /// <c>KAC_UPDATE_DOCS</c> in <c>kac.tests</c>.
    /// </summary>
    internal static bool Updating => Environment.GetEnvironmentVariable("KAC_UPDATE_GOLDENS") == "1";

    /// <summary>
    /// The check ids some <c>validate</c> scenario's findings reported, gathered for the coverage gate.
    /// </summary>
    internal static readonly ConcurrentDictionary<string, byte> CoveredChecks = new(StringComparer.Ordinal);

    private static readonly Lazy<string> Built =
        new(() => BuildKac(Path.Combine(RepoRoot, "tooling", "kac", "kac.csproj")));

    private static readonly ConcurrentDictionary<string, Lazy<Outcome>> Outcomes = new(StringComparer.Ordinal);

    /// <summary>
    /// The built <c>kac</c> assembly. Each scenario runs it as a subprocess, because it asserts the CLI's exit code and
    /// stdout. A failed build throws here, once, and every case asking for it reports the same failure.
    /// </summary>
    internal static string Kac => Built.Value;

    /// <summary>
    /// The scenario's outcome, running it on the first request and returning that run to every later one.
    /// </summary>
    internal static Outcome OutcomeOf(string scenario) =>
        Outcomes.GetOrAdd(scenario, name => new Lazy<Outcome>(() => Runner.RunScenario(name))).Value;

    /// <summary>Runs every scenario not yet run, in parallel, and returns once all of them have finished.</summary>
    internal static void RunAll() => Parallel.ForEach(Scenarios, scenario => OutcomeOf(scenario));

    /// <summary>
    /// Found by the solution file, not a <c>.corpus.yaml</c>: the suite needs the engine, the fixtures and the corpus
    /// at once.
    /// </summary>
    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "kac.slnx")))
                return dir.FullName;
            dir = dir.Parent;
        }

        throw new InvalidOperationException($"kac.goldens: no kac.slnx above {AppContext.BaseDirectory}.");
    }

    /// <summary>
    /// Builds the tool project and returns the assembly to invoke. The path is read back from the same build with
    /// <c>--getProperty</c> rather than constructed, so it cannot disagree with the project's configuration or
    /// framework.
    /// </summary>
    /// <exception cref="InvalidOperationException">The build failed, or reported no assembly that exists.</exception>
    private static string BuildKac(string kacProject)
    {
        var (stdout, stderr, exit) =
            Run(Directory.GetCurrentDirectory(), "dotnet", "build", kacProject, "-t:Build", "--getProperty:TargetPath");
        var assembly = stdout.Trim();

        if (exit != 0 || assembly.Length == 0 || !File.Exists(assembly))
            throw new InvalidOperationException(
                $"kac.goldens: building {kacProject} gave no assembly (exit {exit}).\n{Indent(stderr + stdout)}");

        Invoked.Assembly = assembly;
        return assembly;
    }

    internal static (string stdout, string stderr, int exit) Run(string workingDir, string file, params string[] argv)
    {
        var psi = new ProcessStartInfo(file)
        {
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        foreach (var a in argv) psi.ArgumentList.Add(a);

        // A flag in the verb's place is `--help` or `--version`, which run no verb.
        if (argv is [var target, var verb, ..] && target == Invoked.Assembly && !verb.StartsWith('-'))
            Invoked.Verbs.TryAdd(verb, 0);

        // Colour off, so what a scenario captures is the same on every machine. Spectre writes escapes to
        // a redirected stream where the environment declares itself a runner that renders them, and
        // GITHUB_ACTIONS is one, so a golden matching on text would pass here and fail in CI.
        psi.Environment["NO_COLOR"] = "1";

        using var p = Process.Start(psi) ?? throw new InvalidOperationException($"could not start {file}.");
        var outTask = p.StandardOutput.ReadToEndAsync();
        var errTask = p.StandardError.ReadToEndAsync();
        p.WaitForExit();
        return (outTask.Result, errTask.Result, p.ExitCode);
    }

    internal static string Indent(string s) => string.Join('\n', s.Split('\n').Select(l => "       " + l));
}

/// <summary>
/// The verbs of every <c>kac</c> process this run started. <c>Suite.Run</c> counts a process as <c>kac</c> where its
/// first argument is <see cref="Assembly"/>, so nothing is counted before the build sets it.
/// </summary>
internal static class Invoked
{
    internal static string? Assembly { get; set; }
    internal static ConcurrentDictionary<string, byte> Verbs { get; } = new(StringComparer.Ordinal);
}

/// <summary>
/// What one scenario printed, and whether it failed. Each scenario writes to its own, so two running at once never
/// interleave.
/// </summary>
internal sealed class Outcome
{
    private readonly System.Text.StringBuilder log = new();

    internal bool Failed { get; private set; }

    internal string Log => log.ToString();

    internal void Say(string line) => log.AppendLine(line);

    internal void Fail() => Failed = true;
}
