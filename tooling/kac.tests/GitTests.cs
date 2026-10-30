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
            Files.DeleteTree(dir.FullName);
        }
    }

    [Fact]
    public void A_path_above_the_repository_is_left_out_of_what_counts_as_dirty()
    {
        var dir = Directory.CreateTempSubdirectory("kac-git-");
        try
        {
            var repository = Path.Combine(dir.FullName, "corpus");
            Directory.CreateDirectory(repository);
            File.WriteAllText(Path.Combine(repository, "notes.md"), "the body\n");
            Assert.SkipUnless(GitCli.Repository(repository), GitCli.Unavailable);

            Assert.False(Git.Dirty(repository, ["../.schema", "notes.md"]));
        }
        finally
        {
            Files.DeleteTree(dir.FullName);
        }
    }
}
