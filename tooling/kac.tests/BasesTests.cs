// Unit tests for the bases a repository's own remote implies. Which one a clone used is a fact about the
// person rather than about the repository, so both spellings are read.

using kac.core;

namespace kac.tests;

public class BasesTests
{
    [Theory]
    [InlineData("git@github.com:acme/corpus.git")]
    [InlineData("git@github.com:acme/corpus")]
    [InlineData("https://github.com/acme/corpus.git")]
    [InlineData("https://github.com/acme/corpus")]
    [InlineData("https://github.com/acme/corpus/")]
    [InlineData("ssh://git@github.com/acme/corpus.git")]
    public void A_github_remote_gives_both_bases(string origin)
        => Assert.Equal(
            ("https://github.com/acme/corpus/blob", "https://raw.githubusercontent.com/acme/corpus"),
            Publishing.BasesFrom(Publishing.GitHub, origin));

    [Theory]
    [InlineData(Publishing.AzureDevOpsWiki)]
    [InlineData(Publishing.MkDocs)]
    [InlineData(Publishing.None)]
    public void No_other_target_is_derived(string target)
        => Assert.Null(Publishing.BasesFrom(target, "git@github.com:acme/corpus.git"));

    [Theory]
    [InlineData(null)]                                  // no remote yet
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("git@gitlab.com:acme/corpus.git")]      // another host
    [InlineData("https://github.com/acme")]             // no repository
    [InlineData("https://github.com/acme/corpus/wiki")] // deeper than a repository
    public void A_remote_nothing_can_be_read_from_gives_nothing(string? origin)
        => Assert.Null(Publishing.BasesFrom(Publishing.GitHub, origin));
}
