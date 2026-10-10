using System.ComponentModel;
using System.Diagnostics;

namespace kac.core;

// What one git command came to. `Error` is git's stderr, which says why a clone failed.
public sealed record GitRun(int ExitCode, string Output, string Error)
{
    public bool Ok => ExitCode == 0;
}

// Most of these return null where git is unavailable or the tree is not a repository, because the tool goes
// on working in both. `Attempt` also returns a failed run, for the caller that reports it.
public static class Git
{
    // Everything a git command said, or null where git could not be started.
    public static GitRun? Attempt(string root, string args,
        IReadOnlyDictionary<string, string>? environment = null)
    {
        try
        {
            var psi = new ProcessStartInfo("git", args)
            {
                WorkingDirectory = root,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            if (environment is not null)
                foreach (var (key, value) in environment)
                    psi.Environment[key] = value;

            using var p = Process.Start(psi);
            if (p is null) return null;

            // Drain both streams concurrently before waiting: reading one to end while the other's
            // pipe buffer fills would deadlock.
            var stdout = p.StandardOutput.ReadToEndAsync();
            var stderr = p.StandardError.ReadToEndAsync();
            p.WaitForExit();
            return new GitRun(p.ExitCode, stdout.Result, stderr.Result);
        }
        // What a machine with no git on it throws, which is the absence this answers null for. A read
        // of what git said that fails is a different thing, and stopping is better than reporting it as
        // a git nobody has installed.
        catch (Win32Exception)
        {
            return null;
        }
    }

    // Standard output, or null where the command could not be run or exited non-zero.
    public static string? Run(string root, string args) =>
        Attempt(root, args) is { Ok: true } run ? run.Output : null;

    // The full commit id the tree stands on.
    public static string? Head(string root) => Run(root, "rev-parse HEAD")?.Trim() is { Length: > 0 } sha
        ? sha
        : null;

    // Null where git could not answer, which is not the same as clean.
    public static bool? Dirty(string root) =>
        Run(root, "status --porcelain") is { } status ? status.Trim().Length > 0 : null;
}
