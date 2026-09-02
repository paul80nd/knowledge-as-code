// The walk `LoadNearest` takes, over a real folder tree. Where a schema sits relative to the corpus it
// judges is a fact about the filesystem, so each case builds one.

using kac.core;

namespace kac.tests;

public class SchemaRootTests
{
    [Fact]
    public void A_corpus_is_judged_by_the_schema_above_it()
    {
        var root = Temp();
        WriteSchema(root, "adr");
        var corpus = Path.Combine(root, "examples", "library");
        Directory.CreateDirectory(corpus);

        Assert.Equal(["adr"], Schema.LoadNearest(corpus).IdPrefixes);
    }

    [Fact]
    public void A_corpus_holding_its_own_schema_is_judged_by_that_one()
    {
        var root = Temp();
        WriteSchema(root, "above");
        var corpus = Path.Combine(root, "standalone");
        WriteSchema(corpus, "own");

        Assert.Equal(["own"], Schema.LoadNearest(corpus).IdPrefixes);
    }

    // The fallback the walk takes where it lands on nothing. `kac` declines such a corpus before it
    // reaches the loader, so what this pins is the failure a caller of `kac.core` gets: the missing
    // folder, named.
    [Fact]
    public void A_root_with_no_schema_anywhere_above_it_fails_on_the_folder()
    {
        var root = Temp();

        var thrown = Assert.Throws<DirectoryNotFoundException>(() => Schema.LoadNearest(root));

        Assert.Contains(".schema", thrown.Message, StringComparison.Ordinal);
    }

    // A schema declaring one type, which is enough to tell two of them apart by what they hold.
    private static void WriteSchema(string root, string prefix)
    {
        var dir = Path.Combine(root, ".schema");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "_enums.yaml"), "enums: {}");
        File.WriteAllText(Path.Combine(dir, "_tiers.yaml"), "tiers: {}");
        File.WriteAllText(Path.Combine(dir, "_checks.yaml"), "checks: {}");
        File.WriteAllText(Path.Combine(dir, "_universal.yaml"), "fields: {}");
        File.WriteAllText(Path.Combine(dir, "adrs.yaml"),
            $"""
             label: Decision
             id:
               prefix: {prefix}
             """);
    }

    private static string Temp()
    {
        var root = Path.Combine(Path.GetTempPath(), "kac-schema-root-" + Guid.NewGuid().ToString("n")[..12]);
        Directory.CreateDirectory(root);
        return root;
    }
}
