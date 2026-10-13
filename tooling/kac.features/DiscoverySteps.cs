using System.Diagnostics;
using Reqnroll;

namespace kac.features;

/// <summary>
/// The corpus arrangements discovery reads differently: a repository or none, a schema above the corpus, and a file in
/// a folder discovery skips.
/// </summary>
[Binding]
public sealed class DiscoverySteps(Workspace ws)
{
    /// <summary>Initialised and never committed, so every file reaches the listing as untracked.</summary>
    [Given("the corpus is a git repository")]
    public void GivenTheCorpusIsAGitRepository()
    {
        var psi = new ProcessStartInfo("git")
        {
            WorkingDirectory = ws.Root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        foreach (var a in (string[])["init", "-q", "-b", "main", "."]) psi.ArgumentList.Add(a);

        using var p = Process.Start(psi) ?? throw new InvalidOperationException("git did not start");
        p.StandardOutput.ReadToEnd();
        p.StandardError.ReadToEnd();
        p.WaitForExit();
        if (p.ExitCode != 0) throw new InvalidOperationException("git init failed");
    }

    /// <summary>Moves everything but <c>.schema/</c> down into <paramref name="folder"/>.</summary>
    [Given(@"the corpus sits in ""(.*)"" below its schema")]
    public void GivenTheCorpusSitsInBelowItsSchema(string folder)
    {
        var target = ws.PathOf(folder);
        Directory.CreateDirectory(target);
        foreach (var entry in Directory.EnumerateFileSystemEntries(ws.Root))
        {
            var name = Path.GetFileName(entry);
            if (name == ".schema" || entry == target) continue;
            var to = Path.Combine(target, name);
            if (Directory.Exists(entry)) Directory.Move(entry, to);
            else File.Move(entry, to);
        }
    }

    [Given(@"a file ""(.*)"" with no frontmatter")]
    public void GivenAFileWithNoFrontmatter(string path) => ws.Write(path, "# Notes\n\nNo frontmatter here.\n");

    [Given(@"a file ""(.*)""")]
    public void GivenAFile(string path) => ws.Write(path, "not markdown\n");
}
