using System.Diagnostics;

// ---------------------------------------------------------------------------
// Git — the questions the tool asks of the repository itself
// ---------------------------------------------------------------------------

namespace kac.core;

// What git can tell the tool about the tree it is running over. Everything here answers null where git
// is unavailable or the tree is not a repository, because both are states the tool goes on working in:
// the test harness assembles a corpus on disk and never initialises one, and a corpus unpacked from an
// archive is still a corpus.
public static class Git
{
    // Run a git command in `root` and return its standard output, or null where it could not be run or
    // exited non-zero. One place does the process dance so that a new question is one method rather than
    // a second copy of the deadlock-avoiding read below.
    public static string? Run(string root, string args)
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
            using var p = Process.Start(psi);
            if (p is null) return null;

            // Drain both streams concurrently before waiting: reading one to end while the other's
            // pipe buffer fills would deadlock.
            var stdout = p.StandardOutput.ReadToEndAsync();
            _ = p.StandardError.ReadToEndAsync();
            p.WaitForExit();
            return p.ExitCode == 0 ? stdout.Result : null;
        }
        catch
        {
            return null;
        }
    }

    // The commit the tree stands on, full length. An export names it so that a link it writes resolves
    // against the content the export was built from rather than against whatever the branch holds when
    // the link is followed.
    public static string? Head(string root) => Run(root, "rev-parse HEAD")?.Trim() is { Length: > 0 } sha
        ? sha
        : null;

    // Whether the working tree carries changes the commit above does not. An export built from a dirty
    // tree cannot be reproduced from its own commit, and nothing else in the output would record that,
    // so the manifest carries the answer rather than leaving a consumer to assume the commit is the
    // whole story.
    //
    // Null where git could not answer, which is not the same as clean: a tree nobody can ask about is
    // reported as unknown rather than as either state.
    public static bool? Dirty(string root) =>
        Run(root, "status --porcelain") is { } status ? status.Trim().Length > 0 : null;
}
