using kac.core;

namespace kac.tests;

public sealed class BaselineTests : IDisposable
{
    private static readonly Schema Schema = Schema.Load(Repo.Root);

    private readonly string _repo = Path.Combine(Path.GetTempPath(), "kac-baseline-" + Guid.NewGuid().ToString("N"));

    /// <summary>A corpus one folder down, so every path git reports has to be read relative to it.</summary>
    private string Corpus => Path.Combine(_repo, "corpus");

    public void Dispose()
    {
        try { Files.DeleteTree(_repo); }
        // A file still open leaves the tree under the system temp directory, which the machine clears.
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
        }
    }

    [Fact]
    public void The_records_are_read_as_they_stood_at_the_ref_and_not_as_they_stand_now()
    {
        Write("adrs/0001-git.md", Adr("adr-0001", "We keep records in git."));
        Write("adrs/0002-review.md", Adr("adr-0002", "Every change is reviewed."));
        Assert.SkipUnless(GitCli.Repository(_repo), GitCli.Unavailable);

        Write("adrs/0001-git.md", Adr("adr-0001", "We keep records in a wiki."));
        File.Delete(Path.Combine(Corpus, "adrs/0002-review.md"));

        var (baseline, problem) = Read("HEAD");

        Assert.Null(problem);
        Assert.NotNull(baseline);
        Assert.Equal("HEAD", baseline.Ref);
        Assert.Equal(["adrs/0001-git.md", "adrs/0002-review.md"], baseline.Docs.Select(d => d.Rel).Order());
        Assert.Contains("records in git", baseline.Docs.Single(d => d.Rel == "adrs/0001-git.md").Text);
    }

    [Theory]
    [InlineData("no-such-branch")]
    [InlineData("--output=elsewhere")]
    public void A_ref_git_cannot_resolve_is_a_problem_naming_it(string reference)
    {
        Write("adrs/0001-git.md", Adr("adr-0001", "We keep records in git."));
        Assert.SkipUnless(GitCli.Repository(_repo), GitCli.Unavailable);

        var (baseline, problem) = Read(reference);

        Assert.Null(baseline);
        Assert.NotNull(problem);
        Assert.Contains($"'{reference}'", problem);
        Assert.Contains("fetch-depth: 0", problem);
    }

    [Fact]
    public void A_tree_outside_git_is_a_problem()
    {
        Write("adrs/0001-git.md", Adr("adr-0001", "We keep records in git."));

        var (baseline, problem) = Read("HEAD");

        Assert.Null(baseline);
        Assert.Contains("'HEAD'", problem);
    }

    private (Baseline? Baseline, string? Problem) Read(string reference) =>
        Baseline.Read(Corpus, reference, Schema, CorpusDescriptor.Load(Corpus));

    private void Write(string rel, string text)
    {
        var path = Path.Combine(Corpus, rel);
        Files.OpenFolderFor(path);
        File.WriteAllText(path, text);
        File.WriteAllText(Path.Combine(Corpus, ".corpus.yaml"), "# A corpus for the baseline tests.\n");
    }

    private static string Adr(string id, string decision) => $"""
        ---
        id: {id}
        type: adr
        status: accepted
        ---

        # A decision

        ## Decision

        {decision}
        """;
}
