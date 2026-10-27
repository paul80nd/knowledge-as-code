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
            // Under a skipped dir.
            File.WriteAllText(Path.Combine(dir.FullName, ".git", "c.md"), "");
            // Wrong pattern.
            File.WriteAllText(Path.Combine(dir.FullName, "x.txt"), "");

            var md = GitFiles.Walk(dir.FullName, "*.md", ".git").OrderBy(x => x, StringComparer.Ordinal).ToList();
            Assert.Equal(["a.md", "sub/b.md"], md);

            var all = GitFiles.Walk(dir.FullName, "*", ".git");
            // The '*' pattern picks up non-.md.
            Assert.Contains("x.txt", all);
            // ...but the skipped dir is still dropped.
            Assert.DoesNotContain(".git/c.md", all);
        }
        finally
        {
            Files.DeleteTree(dir.FullName);
        }
    }

    /// <summary>
    /// <c>git ls-files --cached</c> lists a file the index holds whether or not the working tree still has it, so a
    /// deletion nobody has staged yet would otherwise reach every caller as a path to read.
    /// </summary>
    [Fact]
    public void Tracked_drops_a_file_the_working_tree_no_longer_has()
    {
        var dir = Directory.CreateTempSubdirectory("kac-tracked-");
        try
        {
            File.WriteAllText(Path.Combine(dir.FullName, "kept.md"), "");
            File.WriteAllText(Path.Combine(dir.FullName, "gone.md"), "");

            // No git on this machine is the state Tracked answers null for, and the callers walk instead.
            Assert.SkipUnless(GitCli.Repository(dir.FullName), GitCli.Unavailable);

            File.Delete(Path.Combine(dir.FullName, "gone.md"));

            var tracked = GitFiles.Tracked(dir.FullName);
            Assert.NotNull(tracked);
            Assert.Contains("kept.md", tracked);
            Assert.DoesNotContain("gone.md", tracked);
        }
        finally
        {
            Files.DeleteTree(dir.FullName);
        }
    }
}
