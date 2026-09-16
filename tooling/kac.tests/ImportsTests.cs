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

        Assert.Equal("eng", record.Scope);
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

    // A citation into a grandparent names the corpus that wrote the record, not the one this corpus
    // fetched it through. Its parts are found under the scoped id the export stamped on the line.
    [Fact]
    public void A_record_its_producer_inherited_is_scoped_to_the_corpus_that_wrote_it()
    {
        var records = Assert.Single(Loaded(Files(grandparent: true)).Imports).Records;
        var old = Assert.Single(records, r => r.Id == "pol-OLD");

        Assert.Equal("gp", old.Scope);
        Assert.Equal("policies/old.md", old.Path);
        Assert.Equal(["KEEP"], old.Parts);
    }

    // The parts file sits in the same folder as the records and is not one of them.
    [Fact]
    public void The_parts_file_is_not_read_as_a_record()
        => Assert.Single(Assert.Single(Loaded().Imports).Records);

    // A type names its own part-line keys, so a producer calling them something else exports a package
    // whose parts a consumer assuming the spelling would read as empty.
    [Fact]
    public void The_part_line_keys_are_read_from_what_the_producer_published()
    {
        var record = Assert.Single(
            Assert.Single(Loaded(Files(recordKey: "policy", partKey: "clause")).Imports).Records);

        Assert.Equal(["STORE", "ROTATE"], record.Parts);
    }

    // An export written before the manifest named them carries neither, and the two names every type
    // has used are what it falls back to.
    [Fact]
    public void An_export_naming_no_part_line_keys_falls_back_to_record_and_part()
    {
        var record = Assert.Single(Assert.Single(Loaded(Files(publishKeys: false)).Imports).Records);

        Assert.Equal(["STORE", "ROTATE"], record.Parts);
    }

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
        Assert.Empty(graph.Undeclared);
    }

    // Nothing can ever answer to it, so passing over it would leave `import-restored` reporting clean on
    // a declaration no restore could satisfy.
    [Fact]
    public void An_entry_naming_no_shortcode_is_reported_under_the_corpus_it_named()
    {
        var graph = Imports.Load(
            [new Consumed("example-engineering", null, "^0.1.0", null, "../engineering")],
            _ => null, _ => null, _ => null);

        Assert.Empty(graph.NotRestored);
        Assert.Equal(["'example-engineering'"], graph.Undeclared);
    }

    // An entry short of both is named by where it sits, which is all there is to name it by.
    [Fact]
    public void An_entry_naming_neither_a_corpus_nor_a_shortcode_is_named_by_its_position()
        => Assert.Equal(["entry 1"], Imports
            .Load([new Consumed(null, null, null, null, null)], _ => null, _ => null, _ => null).Undeclared);

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
            folder => Under(held, folder)
                .Where(n => !n.Contains('/', StringComparison.Ordinal)).ToList() is { Count: > 0 } names
                ? names
                : null,
            folder => Under(held, folder)
                .Where(n => n.Contains('/', StringComparison.Ordinal))
                .Select(n => n[..n.IndexOf('/', StringComparison.Ordinal)])
                .Distinct(StringComparer.Ordinal).ToList() is { Count: > 0 } found
                ? found
                : null,
            path => held.GetValueOrDefault(path));
    }

    // Everything below one folder, named relative to it. The two delegates above split that into the
    // files sitting directly in it and the folders beside them.
    private static IEnumerable<string> Under(Dictionary<string, string> held, string folder) =>
        held.Keys
            .Where(k => k.StartsWith(folder + "/", StringComparison.Ordinal))
            .Select(k => k[(folder.Length + 1)..]);

    // One import's folder, as `restore` writes it: the manifest, one record, and the parts file the
    // manifest names. `partsFile: null` is the same corpus with a type that keeps no parts.
    private static Dictionary<string, string> Files(
        string? partsFile = "policies/clauses.jsonl",
        string recordKey = "record",
        string partKey = "part",
        bool publishKeys = true,
        bool grandparent = false)
    {
        var keys = partsFile is null || !publishKeys
            ? ""
            : $", \"recordKey\": \"{recordKey}\", \"partKey\": \"{partKey}\"";

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
                                            {{(partsFile is null ? "" : $", \"partsFile\": \"{partsFile}\"")}}{{keys}}
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

        // A record eng inherited, filed under the corpus that wrote it. Its id is bare, as gp wrote it,
        // and the line naming it carries the scope eng stamped.
        if (grandparent)
            files["eng/policies/gp/pol-OLD.json"] = """
                                                   {
                                                     "type": "policies",
                                                     "path": "policies/old.md",
                                                     "fields": { "id": "pol-OLD" }
                                                   }
                                                   """;

        if (partsFile is not null)
            files[$"eng/{partsFile}"] =
                $$"""{"{{recordKey}}":"pol-SCRT","{{partKey}}":"STORE"}"""
                + "\n"
                + $$"""{"{{recordKey}}":"pol-SCRT","{{partKey}}":"ROTATE"}"""
                + "\n"
                + (grandparent
                    ? $$"""{"{{recordKey}}":"gp:pol-OLD","{{partKey}}":"KEEP"}""" + "\n"
                    : "");

        return files;
    }
}
