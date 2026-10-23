using kac.core;

namespace kac.tests;

public class GitTests
{
    [Fact]
    public void A_path_with_a_space_reaches_git_as_one_argument()
    {
        var dir = Directory.CreateTempSubdirectory("kac-git-");
        try
        {
            File.WriteAllText(Path.Combine(dir.FullName, "a record.md"), "the body\n");

            // No git on this machine is the state Run answers null for.
            Assert.SkipUnless(GitCli.Repository(dir.FullName), GitCli.Unavailable);

            Assert.Equal("the body\n", Git.Run(dir.FullName, "show", "HEAD:a record.md"));
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }
}
