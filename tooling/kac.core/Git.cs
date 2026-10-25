using System.ComponentModel;
using System.Diagnostics;

namespace kac.core;

/// <summary>What one git command came to. <c>Error</c> is git's stderr, which says why a clone failed.</summary>
public sealed record GitRun(int ExitCode, string Output, string Error)
{
    public bool Ok => ExitCode == 0;
}

/// <summary>
/// Most of these return null where git is unavailable or the tree is not a repository, because the tool goes on working
/// in both. <c>Attempt</c> also returns a failed run, for the caller that reports it.
/// </summary>
public static class Git
{
    /// <summary>Everything a git command said, or null where git could not be started.</summary>
    public static GitRun? Attempt(string root, IReadOnlyList<string> args,
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

    /// <summary>Standard output, or null where the command could not be run or exited non-zero.</summary>
    public static string? Run(string root, params string[] args) =>
        Attempt(root, args) is { Ok: true } run ? run.Output : null;

    /// <summary>The full commit id the tree stands on, or null where git could not answer.</summary>
    public static string? Head(string root) => Run(root, "rev-parse", "HEAD")?.Trim() is { Length: > 0 } sha
        ? sha
        : null;

    /// <summary>
    /// The path from the repository's root to <paramref name="root"/>, ending in <c>/</c> and empty at the root, or
    /// null where git could not answer.
    /// </summary>
    public static string? Prefix(string root) => Run(root, "rev-parse", "--show-prefix")?.Trim();

    /// <summary>Null where git could not answer, which is not the same as clean.</summary>
    public static bool? Dirty(string root) =>
        Run(root, "status", "--porcelain") is { } status ? status.Trim().Length > 0 : null;

    public static WorkingTree Status(string root) => Dirty(root) switch
    {
        true => WorkingTree.Dirty,
        false => WorkingTree.Clean,
        null => WorkingTree.Unknown
    };
}

/// <summary><c>Unknown</c> is a folder that is no repository, or a repository git could not answer for.</summary>
public enum WorkingTree
{
    Clean,
    Dirty,
    Unknown
}
