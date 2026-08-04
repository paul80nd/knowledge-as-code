// Unit test for the non-git fallback walk (GitFiles.Walk), driven through a real temp tree.
// GitFiles.Tracked (the git ls-files path) and Corpus.Discover are covered by the golden suite.

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
            Assert.Contains("x.txt", all);         // the '*' pattern picks up non-.md
            Assert.DoesNotContain(".git/c.md", all); // ...but the skipped dir is still dropped
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }
}
