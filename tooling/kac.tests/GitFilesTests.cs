// The golden suite assembles a non-git tree, so what git itself reports is only asked here.

using kac.core;

namespace kac.tests;

public class GitFilesTests
{
    [Fact]
    public void Walk_filters_by_pattern_and_skips_dirs()
    {
        var dir = Directory.CreateTempSubdirectory("kac-walk-");
        try
        {
            Directory.CreateDirectory(Path.Combine(dir.FullName, "sub"));
            Directory.CreateDirectory(Path.Combine(dir.FullName, ".git"));
            File.WriteAllText(Path.Combine(dir.FullName, "a.md"), "");
            File.WriteAllText(Path.Combine(dir.FullName, "sub", "b.md"), "");
            File.WriteAllText(Path.Combine(dir.FullName, ".git", "c.md"), ""); // under a skipped dir
            File.WriteAllText(Path.Combine(dir.FullName, "x.txt"), "");        // wrong pattern

            var md = GitFiles.Walk(dir.FullName, "*.md", ".git").OrderBy(x => x, StringComparer.Ordinal).ToList();
            Assert.Equal(["a.md", "sub/b.md"], md);

            var all = GitFiles.Walk(dir.FullName, "*", ".git");
            Assert.Contains("x.txt", all);           // the '*' pattern picks up non-.md
            Assert.DoesNotContain(".git/c.md", all); // ...but the skipped dir is still dropped
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    // `git ls-files --cached` lists a file the index holds whether or not the working tree still has it,
    // so a deletion nobody has staged yet would otherwise reach every caller as a path to read.
    [Fact]
    public void Tracked_drops_a_file_the_working_tree_no_longer_has()
    {
        var dir = Directory.CreateTempSubdirectory("kac-tracked-");
        try
        {
            File.WriteAllText(Path.Combine(dir.FullName, "kept.md"), "");
            File.WriteAllText(Path.Combine(dir.FullName, "gone.md"), "");

            // No git on this machine is the state Tracked answers null for, and the callers walk instead.
            if (!GitCli.Repository(dir.FullName)) return;

            File.Delete(Path.Combine(dir.FullName, "gone.md"));

            var tracked = GitFiles.Tracked(dir.FullName);
            Assert.NotNull(tracked);
            Assert.Contains("kept.md", tracked);
            Assert.DoesNotContain("gone.md", tracked);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }
}
