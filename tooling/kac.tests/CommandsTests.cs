using System.Text.Json;
using kac.core;

// Each verb writes through `Out`, so what these read back is the exit code and what landed on disk.

namespace kac.tests;

public class CommandsTests
{
    [Fact]
    public void Export_stamps_the_manifest_with_the_commit_day_and_dirtiness_it_is_given()
    {
        var root = Fixture("export");

        var exit = Commands.Export(root, null, new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc),
            "0123456789abcdef0123456789abcdef01234567", dirty: _ => true);

        Assert.Equal(0, exit);
        var manifest = Manifest(root);
        Assert.Equal("0123456789abcdef0123456789abcdef01234567", manifest.Text("commit"));
        Assert.Equal("2026-01-02T03:04:05Z", manifest.Text("generatedAt"));
        Assert.True(manifest.GetProperty("dirty").GetBoolean());
    }

    [Theory]
    [InlineData("corpus/scratch/notes.md", true)]
    [InlineData("corpus/handover.md", true)]
    [InlineData("corpus/policies/notes.txt", true)]
    [InlineData(".schema/notes.txt", true)]
    [InlineData("_plan/notes.md", false)]
    [InlineData("tooling/notes.cs", false)]
    public void Export_counts_the_tree_dirty_only_for_an_untracked_file_where_it_reads(string untracked, bool dirty)
    {
        var corpus = Fixture("export", under: "corpus");
        var repository = Path.GetDirectoryName(corpus) ?? corpus;
        Assert.SkipUnless(GitCli.Repository(repository), GitCli.Unavailable);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(repository, untracked)) ?? repository);
        File.WriteAllText(Path.Combine(repository, untracked), "work in progress\n");

        Commands.Export(corpus, null, new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc), null,
            paths => Git.Dirty(corpus, paths));

        Assert.Equal(dirty, Manifest(corpus).GetProperty("dirty").GetBoolean());
    }

    [Theory]
    [InlineData("corpus/README.md", true)]
    [InlineData("corpus/.corpus.yaml", true)]
    [InlineData("README.md", false)]
    public void Export_counts_the_tree_dirty_only_for_a_tracked_change_where_it_reads(string tracked, bool dirty)
    {
        var corpus = Fixture("export", under: "corpus");
        var repository = Path.GetDirectoryName(corpus) ?? corpus;
        File.WriteAllText(Path.Combine(repository, tracked), "");
        Assert.SkipUnless(GitCli.Repository(repository), GitCli.Unavailable);
        File.WriteAllText(Path.Combine(repository, tracked), "# changed\n");

        Commands.Export(corpus, null, new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc), null,
            paths => Git.Dirty(corpus, paths));

        Assert.Equal(dirty, Manifest(corpus).GetProperty("dirty").GetBoolean());
    }

    [Theory]
    [InlineData("scratch/\n",
        "validate: git ignores scratch/corpus/, so validate reads no record in it. Move the corpus, or stop git ignoring "
        + "the folder.")]
    [InlineData("", null)]
    public void Validate_names_the_corpus_folder_git_ignores(string ignore, string? said)
    {
        var repository = Path.Combine(Path.GetTempPath(), "kac-commands-" + Guid.NewGuid().ToString("n")[..12]);
        var corpus = Path.Combine(repository, "scratch", "corpus");
        Directory.CreateDirectory(corpus);
        File.WriteAllText(Path.Combine(repository, ".gitignore"), ignore);
        File.WriteAllText(Path.Combine(corpus, ".corpus.yaml"), "");
        try
        {
            Assert.SkipUnless(GitCli.Repository(repository), GitCli.Unavailable);

            Assert.Equal(said, Commands.Ignored(corpus, records: 0));
        }
        finally
        {
            Files.DeleteTree(repository);
        }
    }

    [Fact]
    public void Validate_says_nothing_of_an_ignored_folder_whose_records_git_tracks()
    {
        var corpus = Fixture("export", under: "scratch");
        var repository = Path.GetDirectoryName(corpus) ?? corpus;
        File.WriteAllText(Path.Combine(repository, ".gitignore"), "scratch/\n");
        Assert.SkipUnless(GitCli.Repository(repository)
                          && GitCli.Run(repository, "add", "-f", "scratch")
                          && GitCli.Run(repository, "-c", "user.email=test@example.com", "-c", "user.name=Test",
                              "-c", "commit.gpgsign=false", "commit", "-qm", "force-added"),
            GitCli.Unavailable);

        var records = Corpus.Load(corpus).Docs.Count;

        Assert.True(records > 0);
        Assert.Null(Commands.Ignored(corpus, records));
    }

    [Fact]
    public void Export_writes_nothing_for_a_type_the_corpus_has_not_adopted()
    {
        var root = Fixture("export");

        var exit = Commands.Export(root, "adrs", new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc), null, _ => null);

        Assert.Equal(1, exit);
        Assert.False(Directory.Exists(Path.Combine(root, Dist.Export)));
    }

    [Fact]
    public void Report_writes_no_file_for_a_corpus_whose_descriptor_states_no_name()
    {
        var root = Fixture("report");
        File.WriteAllText(Path.Combine(root, ".corpus.yaml"), "content-version: 1.2.3\n");
        var to = Path.Combine(root, "reports", "clause-coverage.md");

        var exit = Commands.Report(root, "coverage", "9.9.9", "2026-06-15T09:00:00Z", to);

        Assert.Equal(1, exit);
        Assert.False(File.Exists(to));
    }

    [Fact]
    public void A_path_written_with_backslashes_is_reported_with_forward_slashes_and_its_folder_in_grey()
        => Assert.Equal("[grey]wrote reports/[/]clause-coverage.md", Commands.Wrote(@"reports\clause-coverage.md"));

    [Fact]
    public void Validate_asks_the_registry_it_is_given_about_each_locked_import()
    {
        var root = Fixture("import-versions");
        var asked = new List<string>();

        Commands.Validate(root, json: false, Required.Today, Recording(asked), annotator: null,
            comment: null);

        Assert.Equal(["feed", "feed", "nowhere"], asked.Select(Path.GetFileName));
    }

    [Fact]
    public void Validate_reads_no_registry_for_a_corpus_consuming_nothing()
    {
        var root = Fixture("clean");
        var asked = new List<string>();

        var exit = Commands.Validate(root, json: false, Required.Today, Recording(asked), annotator: null,
            comment: null);

        Assert.Equal(0, exit);
        Assert.Empty(asked);
    }

    [Fact]
    public void Validate_since_a_ref_git_cannot_resolve_exits_1_on_a_corpus_that_is_otherwise_clean()
    {
        var root = Fixture("clean");

        Assert.Equal(0, Commands.Validate(root, json: false, Required.Today, Recording([]), annotator: null,
            comment: null));
        Assert.Equal(1, Commands.Validate(root, json: false, Required.Today, Recording([]), annotator: null,
            comment: null, since: "origin/main"));
    }

    [Fact]
    public void Validate_with_warnings_exits_0_when_the_pull_request_comment_cannot_be_posted()
    {
        var root = Fixture("warnings");
        var sent = new List<HttpCall>();
        var comment = new PullRequestComment(
            new PullRequest(CiPlatform.GitHub, "https://api.github.com/repos/o/r", "1", "GITHUB_TOKEN"),
            call =>
            {
                sent.Add(call);
                return call.Method == "GET"
                    ? new HttpAnswer(200, "[]", null)
                    : new HttpAnswer(403, null, "Forbidden");
            });

        var exit = Commands.Validate(root, json: false, Required.Today, Recording([]),
            new Annotator(CiPlatform.GitHub, "", ""), comment);

        Assert.Equal(0, exit);
        Assert.Equal(["GET", "POST"], sent.Select(c => c.Method));
    }

    [Fact]
    public void Validate_with_json_posts_no_pull_request_comment()
    {
        var root = Fixture("warnings");
        var sent = new List<HttpCall>();
        var comment = new PullRequestComment(
            new PullRequest(CiPlatform.GitHub, "https://api.github.com/repos/o/r", "1", "GITHUB_TOKEN"),
            call =>
            {
                sent.Add(call);
                return new HttpAnswer(200, "[]", null);
            });

        Commands.Validate(root, json: true, Required.Today, Recording([]), new Annotator(CiPlatform.GitHub, "", ""),
            comment);

        Assert.Empty(sent);
    }

    private static Registry Recording(List<string> asked) =>
        new(_ => new Fetched(null, 0, "not a feed."),
            new FolderFeed(source =>
            {
                asked.Add(source);
                return null;
            }, _ => null));

    private static JsonElement Manifest(string root) =>
        JsonDocument.Parse(File.ReadAllText(Path.Combine(root, Dist.Export, Exporter.ManifestFile))).RootElement;

    /// <summary>
    /// A golden fixture's corpus, assembled the way <c>kac.goldens/Runner.cs</c> assembles it: the real schema above
    /// it, and an empty descriptor to mark it as a corpus where the fixture carries none.
    /// </summary>
    /// <summary>
    /// The corpus root. Where <paramref name="under"/> is given, the corpus sits in that folder and the schema stays
    /// at the root above it.
    /// </summary>
    private static string Fixture(string name, string? under = null)
    {
        var root = Path.Combine(Path.GetTempPath(), "kac-commands-" + Guid.NewGuid().ToString("n")[..12]);
        Copy(Path.Combine(Repo.Root, ".schema"), Path.Combine(root, ".schema"));
        var corpus = under is null ? root : Path.Combine(root, under);
        Copy(Path.Combine(Repo.Root, "tooling", "tests", "fixtures", name, "corpus"), corpus);

        var descriptor = Path.Combine(corpus, ".corpus.yaml");
        if (!File.Exists(descriptor)) File.WriteAllText(descriptor, "");
        return corpus;
    }

    private static void Copy(string from, string to)
    {
        Directory.CreateDirectory(to);
        foreach (var file in Directory.GetFiles(from, "*", SearchOption.AllDirectories))
        {
            var target = Path.Combine(to, Path.GetRelativePath(from, file));
            if (Path.GetDirectoryName(target) is { } dir) Directory.CreateDirectory(dir);
            File.Copy(file, target, overwrite: true);
        }
    }
}
