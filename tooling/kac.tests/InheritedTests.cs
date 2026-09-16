using kac.core;

// What `.imports/` comes to, read as the folders a restore wrote rather than from disk.
//
// A restore unpacks one producer's export whole, and an export files an inherited record in a folder
// named for the corpus that wrote it. So the folders a consumer reads are the shape an export writes,
// and these hold the reader to it.

namespace kac.tests;

public class InheritedTests
{
    // A chain three corpora deep, as the middle one published it: its own record beside the folders it
    // filed two producers' records in. Reading the flat files alone would drop both, and leave their part
    // lines naming files nobody received.
    //
    // The order is asserted as well, because two runs over one tree have to write the same bytes. This
    // producer's own records come first, then each folder by name, which is neither the order the fixture
    // lists them in nor a reverse of it.
    [Fact]
    public void A_record_a_producer_inherited_is_read_from_the_folder_it_was_filed_in()
    {
        var records = Assert.Single(Assert.Single(Read(Chain).Carried).Types).Records;

        Assert.Equal(["mid", "alpha", "gp"], records.Select(r => r.Producer));
        Assert.Equal(["gls-middle.json", "gls-first.json", "gls-old.json"], records.Select(r => r.Name));
    }

    // A type may name its parts file anything, so the name is read from the manifest rather than assumed
    // from an extension. A producer calling it `terms.json` still gets its records and not its parts.
    [Fact]
    public void A_parts_file_named_like_a_record_is_still_not_one()
    {
        var records = Assert.Single(Assert.Single(Read(Flat).Carried).Types).Records;

        Assert.Equal(["gls-middle.json"], records.Select(r => r.Name));
    }

    private static string Manifest(string partsFile) =>
        $$"""
          {
            "formatVersion": {{Exporter.FormatVersion}},
            "corpus": "probe-mid",
            "shortcode": "mid",
            "types": [{
              "type": "glossary", "shapeVersion": 1, "dir": "glossary",
              "partsFile": "{{partsFile}}"
            }]
          }
          """;

    // Two producer folders, named so that neither the order they are written in nor a reverse of it is
    // the order the reader has to return.
    private static readonly Dictionary<string, string> Chain = new(StringComparer.Ordinal)
    {
        ["mid/manifest.json"] = Manifest("glossary/terms.jsonl"),
        ["mid/glossary/gls-middle.json"] = "{}\n",
        ["mid/glossary/terms.jsonl"] = "{\"id\":\"gls-middle.one\"}\n",
        ["mid/glossary/gp/gls-old.json"] = "{}\n",
        ["mid/glossary/alpha/gls-first.json"] = "{}\n"
    };

    private static readonly Dictionary<string, string> Flat = new(StringComparer.Ordinal)
    {
        ["mid/manifest.json"] = Manifest("glossary/terms.json"),
        ["mid/glossary/gls-middle.json"] = "{}\n",
        ["mid/glossary/terms.json"] = "{\"id\":\"gls-middle.one\"}\n"
    };

    private static (IReadOnlyList<InheritedCorpus> Carried, IReadOnlyList<string> Missing) Read(
        Dictionary<string, string> files) =>
        Inherited.Read(
            [new Consumed("probe-mid", "mid", "^0.1.0", "0.1.0", "../mid/.dist/package")],
            folder => Named(files, folder).Where(n => !n.Contains('/', StringComparison.Ordinal)).ToList(),
            folder => Named(files, folder)
                .Where(n => n.Contains('/', StringComparison.Ordinal))
                .Select(n => n[..n.IndexOf('/', StringComparison.Ordinal)])
                .Distinct(StringComparer.Ordinal)
                .ToList(),
            file => files.GetValueOrDefault(file));

    // Everything below one folder, named relative to it. The two delegates above split that into the
    // files sitting directly in it and the folders beside them.
    private static IEnumerable<string> Named(Dictionary<string, string> files, string folder) =>
        files.Keys
            .Where(k => k.StartsWith(folder + "/", StringComparison.Ordinal))
            .Select(k => k[(folder.Length + 1)..]);
}
