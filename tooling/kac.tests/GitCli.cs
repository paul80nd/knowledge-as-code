using System.ComponentModel;
using System.Diagnostics;

namespace kac.tests;

/// <summary>
/// git, for the tests that need a real repository rather than a folder. Every method answers false where git could not
/// be run or did not succeed, and a test meeting that returns rather than failing.
/// </summary>
internal static class GitCli
{
    /// <summary>
    /// A repository with one commit in it, holding whatever the caller has already written there. The branch is named
    /// rather than left to <c>init.defaultBranch</c>, so a test cloning a ref knows which.
    /// </summary>
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
        catch (Win32Exception)
        {
            return false;
        }
    }
}
