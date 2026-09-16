using kac.core;

// What `.imports/` comes to, read as the folders a restore wrote rather than from disk.
//
// A restore unpacks one producer's export whole, and an export files an inherited record in a folder
// named for the corpus that wrote it. So the folder a consumer reads is the shape an export writes, and
// these hold the reader to it.

namespace kac.tests;

public class InheritedTests
{
    // A chain three corpora deep, as the middle one published it: its own record beside the folder it
    // filed its producer's record in. Reading the flat files alone would drop the grandparent's record
    // and leave its part lines naming a file nobody received.
    [Fact]
    public void A_record_a_producer_inherited_is_read_from_the_folder_it_was_filed_in()
    {
        var types = Assert.Single(Read().Carried).Types;
        var records = Assert.Single(types).Records;

        Assert.Equal(["mid", "gp"], records.Select(r => r.Producer));
        Assert.Equal(["gls-middle.json", "gls-old.json"], records.Select(r => r.Name));
    }

    // The parts file sits in the type's folder and is not a record, and neither is a folder's name. Both
    // are the shapes an export writes beside the records, so a reader meeting either walks past it.
    [Fact]
    public void A_parts_file_beside_the_records_is_not_one_of_them()
        => Assert.DoesNotContain(
            Assert.Single(Assert.Single(Read().Carried).Types).Records,
            r => r.Name.EndsWith("jsonl", StringComparison.Ordinal));

    private static readonly Dictionary<string, string> Files = new(StringComparer.Ordinal)
    {
        ["mid/manifest.json"] =
            $$"""
              {
                "formatVersion": {{Exporter.FormatVersion}},
                "corpus": "probe-mid",
                "shortcode": "mid",
                "types": [{
                  "type": "glossary", "shapeVersion": 1, "dir": "glossary",
                  "partsFile": "glossary/terms.jsonl"
                }]
              }
              """,
        ["mid/glossary/gls-middle.json"] = "{}\n",
        ["mid/glossary/terms.jsonl"] = "{\"id\":\"gls-middle.one\"}\n",
        ["mid/glossary/gp/gls-old.json"] = "{}\n"
    };

    private static (IReadOnlyList<InheritedCorpus> Carried, IReadOnlyList<string> Missing) Read() =>
        Inherited.Read(
            [new Consumed("probe-mid", "mid", "^0.1.0", "0.1.0", "../mid/.dist/package")],
            folder => Named(folder).Where(n => !n.Contains('/', StringComparison.Ordinal)).ToList(),
            folder => Named(folder)
                .Where(n => n.Contains('/', StringComparison.Ordinal))
                .Select(n => n[..n.IndexOf('/', StringComparison.Ordinal)])
                .Distinct(StringComparer.Ordinal)
                .ToList(),
            file => Files.GetValueOrDefault(file));

    // Everything below one folder, named relative to it. The two delegates above split that into the
    // files sitting directly in it and the folders beside them.
    private static IEnumerable<string> Named(string folder) =>
        Files.Keys
            .Where(k => k.StartsWith(folder + "/", StringComparison.Ordinal))
            .Select(k => k[(folder.Length + 1)..]);
}
