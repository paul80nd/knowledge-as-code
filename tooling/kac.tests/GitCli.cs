using System.ComponentModel;
using System.Diagnostics;

namespace kac.tests;

/// <summary>
/// git, for the tests that need a real repository rather than a folder. Every method answers false where git could not
/// be run or did not succeed, and a test meeting that skips with <see cref="Unavailable"/> as its reason.
/// </summary>
internal static class GitCli
{
    /// <summary>The reason a test skips. It names neither git's absence nor a failed command.</summary>
    internal const string Unavailable = "git could not set up the repository this test reads.";

    /// <summary>The branch every repository is created on, so a test cloning a ref knows which.</summary>
    internal const string Branch = "main";

    /// <summary>A repository with one commit in it, holding whatever the caller has already written there.</summary>
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
            // Drain both streams first: Windows git warns per file on line endings, and an unread pipe blocks it.
            var stdout = p.StandardOutput.ReadToEndAsync();
            var stderr = p.StandardError.ReadToEndAsync();
            p.WaitForExit();
            Task.WaitAll(stdout, stderr);
            return p.ExitCode == 0;
        }
        catch (Win32Exception)
        {
            return false;
        }
    }
}
