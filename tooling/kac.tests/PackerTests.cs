using System.IO.Compression;
using System.Text;
using kac.core;
using Xunit.Sdk;

namespace kac.tests;

public class PackerTests
{
    [Fact]
    public void An_export_with_no_manifest_is_refused()
        => Assert.Contains("Run the export first",
            Assert.Single(Plan([("glossary/terms.jsonl", "{}")]).Problems));

    [Fact]
    public void An_export_manifest_that_is_not_JSON_is_refused()
        => Assert.Contains("Run the export first",
            Assert.Single(Plan([(Exporter.ManifestFile, "not json at all")]).Problems));

    [Fact]
    public void An_export_at_another_format_version_is_refused()
        => Assert.Contains("Rebuild it: kac export",
            Assert.Single(Plan([Manifest(format: Exporter.FormatVersion + 1)]).Problems));

    [Fact]
    public void An_export_naming_no_corpus_is_refused()
        => Assert.Contains("names no corpus",
            Assert.Single(Plan([Manifest(corpus: null)]).Problems));

    [Fact]
    public void An_export_stating_no_content_version_is_refused()
        => Assert.Contains("states no content version",
            Assert.Single(Plan([Manifest(contentVersion: null)]).Problems));

    [Fact]
    public void An_export_declaring_no_shortcode_is_refused()
        => Assert.Contains("cites what it imports by one",
            Assert.Single(Plan([Manifest(shortcode: null)]).Problems));

    [Theory]
    [InlineData("has spaces")]
    [InlineData("slash/es")]
    [InlineData("trailing.")]
    public void A_corpus_name_a_registry_cannot_file_is_refused(string name)
        => Assert.Contains("cannot be a package id",
            Assert.Single(Plan([Manifest(corpus: name)]).Problems));

    [Fact]
    public void A_corpus_name_written_as_nothing_reads_as_absent()
        => Assert.Contains("names no corpus",
            Assert.Single(Plan([Manifest(corpus: "")]).Problems));

    [Theory]
    [InlineData("0.1")]
    [InlineData("v1.0.0")]
    [InlineData("1.0.0.0")]
    [InlineData("latest")]
    public void A_content_version_a_registry_cannot_order_is_refused(string version)
        => Assert.Contains("not a version a registry can order",
            Assert.Single(Plan([Manifest(contentVersion: version)]).Problems));

    [Fact]
    public void A_prerelease_content_version_is_taken()
        => Assert.Equal("1.2.0-rc.1", Plan([Manifest(contentVersion: "1.2.0-rc.1")]).Version);

    // The name a registry stores the package under, which is also how every NuGet client on either
    // registry expects to find it on disk.
    [Fact]
    public void The_file_is_named_for_the_corpus_and_its_content_version()
        => Assert.Equal("example-engineering.0.1.0.nupkg", Plan([Manifest()]).FileName);

    [Fact]
    public void Every_export_file_travels_under_the_payload_directory()
    {
        var plan = Plan([Manifest(), ("glossary/terms.jsonl", "{}"), ("glossary/gls-a.json", "{}")]);

        Assert.Equal(
            ["corpus/glossary/gls-a.json", "corpus/glossary/terms.jsonl", "corpus/manifest.json"],
            plan.Entries.Select(e => e.Path).Where(p => p.StartsWith("corpus/")).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void The_envelope_is_the_three_files_a_registry_reads()
    {
        var plan = Plan([Manifest()]);

        Assert.Equal(
            ["[Content_Types].xml", "_rels/.rels", "example-engineering.nuspec"],
            plan.Entries.Select(e => e.Path).Where(p => !p.StartsWith("corpus/")).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void The_nuspec_carries_the_id_the_version_and_the_shortcode()
    {
        var nuspec = Text(Plan([Manifest()]), "example-engineering.nuspec");

        Assert.Contains("<id>example-engineering</id>", nuspec);
        Assert.Contains("<version>0.1.0</version>", nuspec);
        Assert.Contains("'eng:'", nuspec);
    }

    [Theory]
    [InlineData("NOTICE")]
    [InlineData("NOTICE.")]
    public void A_payload_file_with_no_extension_is_typed_by_name(string file)
        => Assert.Contains($"""PartName="/corpus/{file}" """,
            Text(Plan([Manifest(), (file, "read me")]), "[Content_Types].xml"));

    // OPC compares an extension without regard to case, so two spellings of one extension must not
    // become two declarations of it. A package carrying both is invalid.
    [Fact]
    public void One_extension_spelled_two_ways_is_declared_once()
    {
        var types = Text(Plan([Manifest(), ("a.JSON", "{}"), ("b.json", "{}")]), "[Content_Types].xml");

        Assert.Equal(1, types.Split("""Extension="json" """).Length - 1);
        Assert.DoesNotContain("""Extension="JSON" """, types);
    }

    [Fact]
    public void No_repository_is_named_where_none_was_given()
        => Assert.DoesNotContain("<repository", Text(Plan([Manifest()]), "example-engineering.nuspec"));

    // A registry may act on it: GitHub Packages reads the URL to decide which repository a package
    // belongs to, and a token scoped to a repository refuses a package naming none.
    [Fact]
    public void A_repository_given_is_written_into_the_envelope()
    {
        var plan = Packer.Plan(
            [new BundleFile(Exporter.ManifestFile, Encoding.UTF8.GetBytes(Manifest().Item2))],
            "https://github.com/paul80nd/knowledge-as-code");

        Assert.Contains("""url="https://github.com/paul80nd/knowledge-as-code" """,
            Text(plan, "example-engineering.nuspec"));
    }

    [Fact]
    public void Two_archives_from_one_export_are_identical()
    {
        var export = new[] { Manifest(), ("glossary/terms.jsonl", "{\"term\":\"a\"}") };

        Assert.Equal(Packer.Archive(Plan(export)), Packer.Archive(Plan(export)));
    }

    [Fact]
    public void The_payload_is_the_export_unchanged()
    {
        var line = "{\"term\":\"resilience\",\"level\":\"MUST\"}\n";
        var archive = Packer.Archive(Plan([Manifest(), ("glossary/terms.jsonl", line)]));

        using var zip = new ZipArchive(new MemoryStream(archive), ZipArchiveMode.Read);
        var terms = zip.GetEntry("corpus/glossary/terms.jsonl")
                    ?? throw new XunitException("the package holds no corpus/glossary/terms.jsonl.");
        using var reader = new StreamReader(terms.Open());

        Assert.Equal(line, reader.ReadToEnd());
    }

    private static PackPlan Plan((string Path, string Content)[] export) =>
        Packer.Plan([.. export.Select(f => new BundleFile(f.Path, Encoding.UTF8.GetBytes(f.Content)))]);

    private static string Text(PackPlan plan, string path) =>
        Encoding.UTF8.GetString(plan.Entries.Single(e => e.Path == path).Content);

    // Each argument is nullable, so a case can take one away and leave the rest standing.
    private static (string, string) Manifest(
        int? format = null,
        string? corpus = "example-engineering",
        string? shortcode = "eng",
        string? contentVersion = "0.1.0") =>
        (Exporter.ManifestFile,
            $$"""
              {
                "formatVersion": {{format ?? Exporter.FormatVersion}},
                "corpus": {{Quoted(corpus)}},
                "shortcode": {{Quoted(shortcode)}},
                "contentVersion": {{Quoted(contentVersion)}},
                "types": []
              }
              """);

    private static string Quoted(string? value) => value is null ? "null" : $"\"{value}\"";
}
