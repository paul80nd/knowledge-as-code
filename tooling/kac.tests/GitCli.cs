using System.Diagnostics;

namespace kac.tests;

// git, for the tests that need a real repository rather than a folder. Two of them do: the file listing
// differs from a walk only where git is answering, and `new` clones its template.
//
// Every method answers false where git could not be run, which is a machine with no git on it. A test
// meeting that returns rather than failing, because what it is asking about is git's behaviour and there
// is none to ask about.
internal static class GitCli
{
    // A repository with one commit in it, holding whatever the caller has already written there. The
    // branch is named rather than left to `init.defaultBranch`, so a test cloning a ref knows which.
    internal const string Branch = "main";

    internal static bool Repository(string root) =>
        Run(root, "init", "-q", "-b", Branch, ".")
        && Run(root, "add", "-A")
        && Run(root, "-c", "user.email=test@example.com", "-c", "user.name=Test",
            "-c", "commit.gpgsign=false", "commit", "-qm", "fixture");

    internal static bool Run(string root, params string[] args)
    {
        var psi = new ProcessStartInfo("git")
        {
            WorkingDirectory = root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        foreach (var a in args) psi.ArgumentList.Add(a);

        try
        {
            using var p = Process.Start(psi);
            if (p is null) return false;
            p.WaitForExit();
            return p.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
