using kac.core;

namespace kac.tests;

public class InheritedTests
{
    /// <summary>
    /// The order is asserted, because two runs over one tree have to write the same bytes. This producer's own records
    /// come first, then each folder by name.
    /// </summary>
    [Fact]
    public void A_record_a_producer_inherited_is_read_from_the_folder_it_was_filed_in()
    {
        var records = Assert.Single(Assert.Single(Read(Chain).Carried).Types).Records;

        Assert.Equal(["mid", "alpha", "gp"], records.Select(r => r.Producer));
        Assert.Equal(["gls-middle.json", "gls-first.json", "gls-old.json"], records.Select(r => r.Name));
    }

    [Fact]
    public void A_parts_file_named_like_a_record_is_still_not_one()
    {
        var records = Assert.Single(Assert.Single(Read(Flat).Carried).Types).Records;

        Assert.Equal(["gls-middle.json"], records.Select(r => r.Name));
    }

    [Fact]
    public void A_type_reads_its_in_force_status_from_its_producer_s_manifest()
    {
        var files = new Dictionary<string, string>(Flat, StringComparer.Ordinal)
        {
            ["mid/manifest.json"] = Manifest("glossary/terms.json", "accepted")
        };

        Assert.Equal("accepted", Assert.Single(Assert.Single(Read(files).Carried).Types).InForce);
    }

    [Fact]
    public void A_manifest_stating_no_in_force_status_leaves_it_null()
        => Assert.Null(Assert.Single(Assert.Single(Read(Flat).Carried).Types).InForce);

    [Fact]
    public void A_type_reads_its_record_and_part_counts_from_its_producer_s_manifest()
    {
        var files = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["mid/manifest.json"] = Manifest("glossary/terms.jsonl").Replace(
                "\"dir\": \"glossary\",", "\"dir\": \"glossary\", \"records\": 3, \"parts\": 7,",
                StringComparison.Ordinal)
        };

        var type = Assert.Single(Assert.Single(Read(files).Carried).Types);

        Assert.Empty(type.Records);
        Assert.Equal((3, 7), (type.StatedRecords, type.StatedParts));
    }

    /// <summary><c>inForce</c> is left out where it is null.</summary>
    private static string Manifest(string partsFile, string? inForce = null) =>
        $$"""
          {
            "formatVersion": {{Exporter.FormatVersion}},
            "corpus": "probe-mid",
            "shortcode": "mid",
            "types": [{
              "type": "glossary", "shapeVersion": 1, "dir": "glossary",
              "partsFile": "{{partsFile}}"{{(inForce is null ? "" : $", \"inForce\": \"{inForce}\"")}}
            }]
          }
          """;

    /// <summary>
    /// Two producer folders, named so that neither the order they are written in nor a reverse of it is the order the
    /// reader has to return.
    /// </summary>
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

    private static IEnumerable<string> Named(Dictionary<string, string> files, string folder) =>
        files.Keys
            .Where(k => k.StartsWith(folder + "/", StringComparison.Ordinal))
            .Select(k => k[(folder.Length + 1)..]);
}
