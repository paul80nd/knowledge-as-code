// Unit tests for the base a repository's own remote implies. Which spelling a clone used is a fact about
// the person rather than about the repository, so both are read.

using kac.core;

namespace kac.tests;

public class BaseFromTests
{
    private const string RepoBase = "https://dev.azure.com/acme/Standards/_git/corpus";

    [Theory]
    [InlineData("git@github.com:acme/corpus.git")]
    [InlineData("git@github.com:acme/corpus")]
    [InlineData("https://github.com/acme/corpus.git")]
    [InlineData("https://github.com/acme/corpus")]
    [InlineData("https://github.com/acme/corpus/")]
    [InlineData("ssh://git@github.com/acme/corpus.git")]
    public void A_github_remote_gives_the_repository_url(string origin)
        => Assert.Equal("https://github.com/acme/corpus", Publishing.BaseFrom(Publishing.GitHub, origin));

    // Azure DevOps spells its two remotes differently enough that neither reduces to the other: an SSH
    // remote opens with the protocol version, and an HTTPS one carries `_git` and may prefix the host
    // with a user name.
    [Theory]
    [InlineData("https://acme@dev.azure.com/acme/Standards/_git/corpus")]
    [InlineData("https://dev.azure.com/acme/Standards/_git/corpus")]
    [InlineData("https://dev.azure.com/acme/Standards/_git/corpus/")]
    [InlineData("git@ssh.dev.azure.com:v3/acme/Standards/corpus")]
    [InlineData("ssh://git@ssh.dev.azure.com/v3/acme/Standards/corpus")]
    public void An_azure_devops_remote_gives_the_repository_url(string origin)
        => Assert.Equal(RepoBase, Publishing.BaseFrom(Publishing.AzureDevOps, origin));

    // A repository's remote says nothing about which wiki, if any, publishes it, and the remaining
    // targets are served from somewhere the remote cannot name either.
    [Theory]
    [InlineData(Publishing.AzureDevOpsWiki)]
    [InlineData(Publishing.MkDocs)]
    [InlineData(Publishing.None)]
    public void No_other_target_is_derived(string target)
        => Assert.Null(Publishing.BaseFrom(target, "git@github.com:acme/corpus.git"));

    [Theory]
    [InlineData(Publishing.GitHub, null)] // no remote yet
    [InlineData(Publishing.GitHub, "")]
    [InlineData(Publishing.GitHub, "   ")]
    [InlineData(Publishing.GitHub, "git@gitlab.com:acme/corpus.git")]      // another host
    [InlineData(Publishing.GitHub, "https://github.com/acme")]             // no repository
    [InlineData(Publishing.GitHub, "https://github.com/acme/corpus/wiki")] // deeper than a repository
    [InlineData(Publishing.AzureDevOps, "https://dev.azure.com/acme/corpus")]
    [InlineData(Publishing.AzureDevOps, "git@ssh.dev.azure.com:acme/Standards/corpus")]
    [InlineData(Publishing.AzureDevOps, "https://github.com/acme/corpus.git")]
    public void A_remote_nothing_can_be_read_from_gives_nothing(string target, string? origin)
        => Assert.Null(Publishing.BaseFrom(target, origin));
}
