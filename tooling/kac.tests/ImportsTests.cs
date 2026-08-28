using kac.core;

// In-process unit tests for reading what `kac restore` unpacked, and for the citation form that reaches
// it.
//
// No fixture stands behind these. `.imports/` holds another corpus's export and nothing commits one, so
// the folders are stood up here as a dictionary of paths.

namespace kac.tests;

public class ImportsTests
{
    [Fact]
    public void A_record_and_its_parts_are_read_from_the_export()
    {
        var record = Assert.Single(Assert.Single(Loaded().Imports).Records);

        Assert.Equal("pol-SCRT", record.Id);
        Assert.Equal("policies", record.Type);
        Assert.Equal("policies/scrt-secrets-are-never-embedded.md", record.Path);
        Assert.Equal(["STORE", "ROTATE"], record.Parts);
        Assert.True(record.KeepsParts);
    }

    [Fact]
    public void The_corpus_and_version_come_from_the_manifest_rather_than_the_declaration()
    {
        var import = Assert.Single(Loaded().Imports);

        Assert.Equal("example-engineering", import.Corpus);
        Assert.Equal("0.1.0", import.Version);
    }

    // A consumer building a link from its own publishing would address its own repository.
    [Fact]
    public void The_link_template_is_the_producers_own()
        => Assert.Equal("https://example.com/eng/{path}#{anchor}", Assert.Single(Loaded().Imports).Link);

    // The parts file sits in the same folder as the records and is not one of them.
    [Fact]
    public void The_parts_file_is_not_read_as_a_record()
        => Assert.Single(Assert.Single(Loaded().Imports).Records);

    // A type keeping no parts is told apart from a record carrying none, because the two earn different
    // sentences from `part-ref`.
    [Fact]
    public void A_type_declaring_no_parts_file_keeps_no_parts()
    {
        var import = Assert.Single(Loaded(Files(partsFile: null)).Imports);

        Assert.False(Assert.Single(import.Records).KeepsParts);
    }

    [Fact]
    public void A_declaration_whose_folder_holds_no_manifest_is_named_as_not_restored()
    {
        var graph = Loaded(new Dictionary<string, string>());

        Assert.Empty(graph.Imports);
        Assert.Equal(["eng"], graph.NotRestored);
    }

    [Theory]
    [InlineData("eng:pol-VURM.TIMEBOX", "eng", "pol-VURM", "TIMEBOX")]
    [InlineData("eng:pol-VURM", "eng", "pol-VURM", null)]
    [InlineData("pol-VURM.TIMEBOX", null, "pol-VURM", "TIMEBOX")]
    [InlineData("pol-VURM", null, "pol-VURM", null)]
    public void A_citation_is_read_as_scope_record_and_part(
        string text, string? scope, string record, string? part)
    {
        var citation = Citation.Read(text);

        Assert.Equal(scope, citation.Scope);
        Assert.Equal(record, citation.Record);
        Assert.Equal(part, citation.Part);
        Assert.Equal(text, citation.ToString());
    }

    // A shortcode is lower-case letters and digits alone, so a colon anywhere else belongs to the text.
    // `std-A11Y:WCAG` is a citation written with the wrong separator, and reading a scope off it would
    // hide the fault the corpus reports as `part-separator`.
    [Theory]
    [InlineData("std-A11Y:WCAG")]
    [InlineData("Policy: pol-SCRT")]
    [InlineData(":pol-SCRT")]
    public void A_colon_that_follows_no_shortcode_leaves_the_text_unscoped(string text)
        => Assert.Null(Citation.Read(text).Scope);

    private static ImportGraph Loaded(Dictionary<string, string>? files = null)
    {
        var held = files ?? Files();

        return Imports.Load(
            [new Consumed("example-engineering", "eng", "^0.1.0", "0.1.0", "../engineering")],
            folder => held.Keys
                .Where(p => p.LastIndexOf('/') == folder.Length && p.StartsWith(folder + "/", StringComparison.Ordinal))
                .Select(p => p[(folder.Length + 1)..]).ToList() is { Count: > 0 } names
                ? names
                : null,
            path => held.GetValueOrDefault(path));
    }

    // One import's folder, as `restore` writes it: the manifest, one record, and the parts file the
    // manifest names. `partsFile: null` is the same corpus with a type that keeps no parts.
    private static Dictionary<string, string> Files(string? partsFile = "policies/clauses.jsonl")
    {
        var files = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["eng/manifest.json"] = $$"""
                {
                  "formatVersion": 3,
                  "corpus": "example-engineering",
                  "shortcode": "eng",
                  "contentVersion": "0.1.0",
                  "publishing": { "humanTemplate": "https://example.com/eng/{path}#{anchor}" },
                  "types": [
                    {
                      "type": "policies",
                      "dir": "policies"
                      {{(partsFile is null ? "" : $", \"partsFile\": \"{partsFile}\"")}}
                    }
                  ]
                }
                """,
            ["eng/policies/pol-SCRT.json"] = """
                {
                  "type": "policies",
                  "path": "policies/scrt-secrets-are-never-embedded.md",
                  "fields": { "id": "pol-SCRT" }
                }
                """
        };

        if (partsFile is not null)
            files[$"eng/{partsFile}"] =
                """{"record":"pol-SCRT","part":"STORE"}"""
                + "\n"
                + """{"record":"pol-SCRT","part":"ROTATE"}"""
                + "\n";

        return files;
    }
}
