using kac.core;

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

    [Fact]
    public void A_record_its_producer_inherited_is_scoped_to_the_corpus_that_wrote_it()
    {
        var records = Assert.Single(Loaded(Files(grandparent: true)).Imports).Records;
        var old = Assert.Single(records, r => r.Id == "pol-OLD");

        Assert.Equal("gp", old.Scope);
        Assert.Equal(["KEEP"], old.Parts);
    }

    [Fact]
    public void The_parts_file_is_not_read_as_a_record()
        => Assert.Single(Assert.Single(Loaded().Imports).Records);

    [Fact]
    public void The_part_line_keys_are_read_from_what_the_producer_published()
    {
        var record = Assert.Single(
            Assert.Single(Loaded(Files(recordKey: "policy", partKey: "clause")).Imports).Records);

        Assert.Equal(["STORE", "ROTATE"], record.Parts);
    }

    [Fact]
    public void An_export_naming_no_part_line_keys_falls_back_to_record_and_part()
    {
        var record = Assert.Single(Assert.Single(Loaded(Files(publishKeys: false)).Imports).Records);

        Assert.Equal(["STORE", "ROTATE"], record.Parts);
    }

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

    [Fact]
    public void An_entry_naming_no_shortcode_is_reported_under_the_corpus_it_named()
    {
        var graph = Imports.Load(
            [new Consumed("example-engineering", null, "^0.1.0", null, "../engineering")],
            names: _ => null, folders: _ => null, read: _ => null);

        Assert.Empty(graph.NotRestored);
        Assert.Equal(["'example-engineering'"], graph.Undeclared);
    }

    [Fact]
    public void An_entry_naming_neither_a_corpus_nor_a_shortcode_is_named_by_its_position()
        => Assert.Equal(["entry 1"], Imports
            .Load([new Consumed(null, null, null, null, null)],
                names: _ => null, folders: _ => null, read: _ => null).Undeclared);

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

    /// <summary>
    /// A shortcode is lower-case letters and digits alone. Reading a scope off <c>std-A11Y:WCAG</c> would hide the
    /// wrong separator the corpus reports as <c>part-separator</c>.
    /// </summary>
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

    private static IEnumerable<string> Under(Dictionary<string, string> held, string folder) =>
        held.Keys
            .Where(k => k.StartsWith(folder + "/", StringComparison.Ordinal))
            .Select(k => k[(folder.Length + 1)..]);

    /// <summary>
    /// One import's folder, as <c>restore</c> writes it: the manifest, one record, and the parts file the manifest
    /// names. <c>partsFile: null</c> is the same corpus with a type that keeps no parts.
    /// </summary>
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
