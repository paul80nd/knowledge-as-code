using System.IO.Compression;
using System.Text;
using kac.core;

// In-process unit tests for the package.
//
// The golden fixture packs one export and pins what a consumer receives. What it cannot hold is the
// refusals: each needs an export manifest missing or malformed in one particular way, and an export is
// written by the exporter rather than by hand. So the fixture proves the shape and these prove the
// rules, which is the division `BundlerTests` already draws.

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

    // The same question `Bundler.Plan` asks, and for the same reason. A package is a contract, and one
    // built from an envelope this build has never read would publish a shape nobody agreed to.
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

    // Refused where the export leaves it null, unlike everything else the manifest carries. A consumer
    // files an import under its shortcode and resolves `eng:pol-VURM` against it, so a package without
    // one is a package nothing downstream can address.
    [Fact]
    public void An_export_declaring_no_shortcode_is_refused()
        => Assert.Contains("cites what it imports by one",
            Assert.Single(Plan([Manifest(shortcode: null)]).Problems));

    // A corpus may call itself anything until the day it publishes. These are the spellings a registry
    // will not file, and the refusal happens here rather than in the schema for that reason.
    [Theory]
    [InlineData("has spaces")]
    [InlineData("slash/es")]
    [InlineData("trailing.")]
    public void A_corpus_name_a_registry_cannot_file_is_refused(string name)
        => Assert.Contains("cannot be a package id",
            Assert.Single(Plan([Manifest(corpus: name)]).Problems));

    // An empty string reads as an absent value throughout `JsonRead`, so a corpus that wrote `corpus:`
    // and left it blank is told it named none rather than told the blank is an illegal id.
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

    // The whole payload sits under one directory, so the envelope's files are never among the corpus's
    // and a consumer strips a known prefix rather than filtering by name.
    [Fact]
    public void Every_export_file_travels_under_the_payload_directory()
    {
        var plan = Plan([Manifest(), ("glossary/terms.jsonl", "{}"), ("glossary/gls-a.json", "{}")]);

        Assert.Equal(
            ["corpus/glossary/gls-a.json", "corpus/glossary/terms.jsonl", "corpus/manifest.json"],
            plan.Entries.Select(e => e.Path).Where(p => p.StartsWith("corpus/")).Order(StringComparer.Ordinal));
    }

    // What makes the archive a package rather than a zip. Without the relationship nothing points at
    // the part describing the package, and without the content types a strict reader cannot say what
    // any part is.
    [Fact]
    public void The_envelope_is_the_three_files_a_registry_reads()
    {
        var plan = Plan([Manifest()]);

        Assert.Equal(
            ["[Content_Types].xml", "_rels/.rels", "example-engineering.nuspec"],
            plan.Entries.Select(e => e.Path).Where(p => !p.StartsWith("corpus/")).Order(StringComparer.Ordinal));
    }

    // The envelope names the package and nothing else about the corpus. Everything a consumer acts on
    // is in the copied manifest, so a second statement of it here would be a second thing to keep in
    // step. The description is the exception, because a registry requires one.
    [Fact]
    public void The_nuspec_carries_the_id_the_version_and_the_shortcode()
    {
        var nuspec = Text(Plan([Manifest()]), "example-engineering.nuspec");

        Assert.Contains("<id>example-engineering</id>", nuspec);
        Assert.Contains("<version>0.1.0</version>", nuspec);
        Assert.Contains("'eng:'", nuspec);
    }

    // A file with no extension has nothing for a content-type default to key on, so it takes an
    // override. Nothing the exporter writes today is one, and the rule is here rather than in the
    // exporter because a type declares its own file names.
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

    // Left out where nobody named one, because the export states where a record is published and that
    // is a different address from where its source lives.
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

    // Two packs of one export are the same bytes. A registry keeps a published version forever, so
    // "is what I am about to push the thing I proved" has to be answerable by comparing files.
    [Fact]
    public void Two_archives_from_one_export_are_identical()
    {
        var export = new[] { Manifest(), ("glossary/terms.jsonl", "{\"term\":\"a\"}") };

        Assert.Equal(Packer.Archive(Plan(export)), Packer.Archive(Plan(export)));
    }

    // The payload is the export, byte for byte. A package that edited what it carried would publish
    // something the golden fixture never saw.
    [Fact]
    public void The_payload_is_the_export_unchanged()
    {
        var line = "{\"term\":\"resilience\",\"level\":\"MUST\"}\n";
        var archive = Packer.Archive(Plan([Manifest(), ("glossary/terms.jsonl", line)]));

        using var zip = new ZipArchive(new MemoryStream(archive), ZipArchiveMode.Read);
        using var reader = new StreamReader(zip.GetEntry("corpus/glossary/terms.jsonl")!.Open());

        Assert.Equal(line, reader.ReadToEnd());
    }

    private static PackPlan Plan((string Path, string Content)[] export) =>
        Packer.Plan([.. export.Select(f => new BundleFile(f.Path, Encoding.UTF8.GetBytes(f.Content)))]);

    private static string Text(PackPlan plan, string path) =>
        Encoding.UTF8.GetString(plan.Entries.Single(e => e.Path == path).Content);

    // An export manifest as `kac export` writes one, carrying the four keys a pack reads from it. Each
    // argument is nullable so a case can take one away and leave the rest standing.
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
