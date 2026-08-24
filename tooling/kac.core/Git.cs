using System.Diagnostics;

namespace kac.core;

// What one git command came to. `Error` is what git wrote to stderr, which is the only account of why a
// clone failed that says whether the URL, the ref or the credential was wrong.
public sealed record GitRun(int ExitCode, string Output, string Error)
{
    public bool Ok => ExitCode == 0;
}

// What git can tell the tool about the tree it is running over. Most of it answers null where git is
// unavailable or the tree is not a repository, because both are states the tool goes on working in: the
// test harness assembles a corpus on disk and never initialises one, and a corpus unpacked from an
// archive is still a corpus. `Attempt` is the exception, for the caller that has to report the failure.
public static class Git
{
    // Run a git command in `root` and answer with everything it said, or null where git could not be
    // started at all. One place does the process dance, so a new question costs one method and the
    // deadlock-avoiding read below is written once.
    //
    // `environment` adds variables to the child. `new` sets `GIT_TERMINAL_PROMPT=0` where nobody is
    // watching, because a clone waiting on a password nobody can type is a pipeline that never returns.
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
        catch
        {
            return null;
        }
    }

    // Standard output, or null where the command could not be run or exited non-zero. What every caller
    // asking git a question wants, as against the one caller reporting why a command failed.
    public static string? Run(string root, string args) =>
        Attempt(root, args) is { Ok: true } run ? run.Output : null;

    // The commit the tree stands on, full length. An export names it so that a link it writes resolves
    // against the content the export was built from rather than against whatever the branch holds when
    // the link is followed.
    public static string? Head(string root) => Run(root, "rev-parse HEAD")?.Trim() is { Length: > 0 } sha
        ? sha
        : null;

    // Whether the working tree carries changes the commit above does not. An export built from a dirty
    // tree cannot be reproduced from its own commit, and nothing else in the output would record that.
    // So the manifest carries the answer, and a consumer never has to assume the commit is the whole
    // story.
    //
    // Null where git could not answer, which is not the same as clean. A tree nobody can ask about is
    // reported as unknown.
    public static bool? Dirty(string root) =>
        Run(root, "status --porcelain") is { } status ? status.Trim().Length > 0 : null;
}
