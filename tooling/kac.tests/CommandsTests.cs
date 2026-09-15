using System.Text.Json;
using kac.core;

// The facts a verb reads from outside the corpus arrive as parameters, and these prove they are used.
// `Program.cs` reads the clock, git and the network and passes them down, so a verb can be run here over
// a fixture corpus with a day, a commit and a registry chosen by the test, and none read from the machine.
//
// The fixture is copied under the temp directory with the real schema beside it, which is the tree
// `kac-tests.cs` assembles for the golden suite. Each verb still writes through `Out`, so what these read
// back is the exit code and what landed on disk.

namespace kac.tests;

public class CommandsTests
{
    // The temp tree is no git repository, so a commit and a dirtiness the manifest names can only have
    // come from the call.
    [Fact]
    public void Export_stamps_the_manifest_with_the_commit_day_and_dirtiness_it_is_given()
    {
        var root = Fixture("export");

        var exit = Commands.Export(root, null, new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc),
            "0123456789abcdef0123456789abcdef01234567", dirty: true);

        Assert.Equal(0, exit);
        var manifest = Manifest(root);
        Assert.Equal("0123456789abcdef0123456789abcdef01234567", manifest.Text("commit"));
        Assert.Equal("2026-01-02T03:04:05Z", manifest.Text("generatedAt"));
        Assert.True(manifest.GetProperty("dirty").GetBoolean());
    }

    // Three imports, each locked, so each is asked about once at the source its entry names. An import
    // with no lock is skipped before the registry is reached, which `Freshness.Read` says.
    // A refusal is the plan's, and the verb writes nothing for it.
    [Fact]
    public void Export_writes_nothing_for_a_type_the_corpus_has_not_adopted()
    {
        var root = Fixture("export");

        var exit = Commands.Export(root, "adrs", new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc), null, null);

        Assert.Equal(1, exit);
        Assert.False(Directory.Exists(Path.Combine(root, Dist.Export)));
    }

    [Fact]
    public void Validate_asks_the_registry_it_is_given_about_each_locked_import()
    {
        var root = Fixture("import-versions");
        var asked = new List<string>();

        Commands.Validate(root, json: false, Required.Today, Recording(asked));

        Assert.Equal(["feed", "feed", "nowhere"], asked.Select(Path.GetFileName));
    }

    // A corpus consuming nothing is every corpus standing on its own, and the check costs it no read.
    [Fact]
    public void Validate_reads_no_registry_for_a_corpus_consuming_nothing()
    {
        var root = Fixture("clean");
        var asked = new List<string>();

        var exit = Commands.Validate(root, json: false, Required.Today, Recording(asked));

        Assert.Equal(0, exit);
        Assert.Empty(asked);
    }

    // A registry whose every source is a folder that records the ask and holds nothing.
    private static Registry Recording(List<string> asked) =>
        new(_ => new Fetched(null, 0, "not a feed."),
            new FolderFeed(source =>
            {
                asked.Add(source);
                return null;
            }, _ => null));

    private static JsonElement Manifest(string root) =>
        JsonDocument.Parse(File.ReadAllText(Path.Combine(root, Dist.Export, Exporter.ManifestFile))).RootElement;

    // A golden fixture's corpus, assembled the way `kac-tests.cs` assembles it: the real schema above it,
    // and an empty descriptor to mark it as a corpus where the fixture carries none.
    private static string Fixture(string name)
    {
        var root = Path.Combine(Path.GetTempPath(), "kac-commands-" + Guid.NewGuid().ToString("n")[..12]);
        Copy(Path.Combine(Repo.Root, ".schema"), Path.Combine(root, ".schema"));
        Copy(Path.Combine(Repo.Root, "tooling", "tests", "fixtures", name, "corpus"), root);

        var descriptor = Path.Combine(root, ".corpus.yaml");
        if (!File.Exists(descriptor)) File.WriteAllText(descriptor, "");
        return root;
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
