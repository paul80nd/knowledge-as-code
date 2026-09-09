using System.Text.Json;
using kac.core;

// How far a record has been taken on trust, derived from `verified` and shipped as `trust`. The Open
// Knowledge Format names the three tiers, and this is where each one is pinned.

namespace kac.tests;

public class TrustTierTests
{
    [Fact]
    public void A_person_in_the_list_makes_the_record_human_reviewed()
        => Assert.Equal("human-reviewed", Trust("  - { at: 2026-09-08T11:00:00Z, by: human:alex.doe }\n"));

    [Fact]
    public void Agents_alone_make_the_record_machine_confirmed()
        => Assert.Equal("machine-confirmed", Trust("  - { at: 2026-09-08T11:00:00Z, by: sweep/1.0.0 }\n"));

    // One person is enough, wherever in the list they sit. The tiers rank, so the highest actor decides.
    [Fact]
    public void One_person_among_agents_still_makes_it_human_reviewed()
        => Assert.Equal("human-reviewed", Trust(
            "  - { at: 2026-09-08T11:00:00Z, by: sweep/1.0.0 }\n"
            + "  - { at: 2026-09-08T12:00:00Z, by: human:alex.doe }\n"));

    [Fact]
    public void An_empty_list_makes_the_record_unverified()
        => Assert.Equal("unverified", Trust("", "verified: []\n"));

    [Fact]
    public void A_record_carrying_no_such_field_is_unverified()
        => Assert.Equal("unverified", Trust("", ""));

    // `list` refuses a bare mapping and `validate` reports it, so the tier reads it the way the field
    // beside it does rather than answering `unverified` about a record naming a person.
    [Fact]
    public void A_list_written_as_one_mapping_is_the_one_entry_case()
        => Assert.Equal("human-reviewed",
            Trust("", "verified: { at: 2026-09-08T11:00:00Z, by: human:alex.doe }\n"));

    // A type whose export never declares `verified` answers nothing rather than `unverified`. Reading a
    // tier off a type that has no such field would tell a consumer something the corpus never said.
    [Fact]
    public void A_type_that_does_not_export_the_field_carries_no_tier()
    {
        var record = Record(Page("verified:\n  - { at: 2026-09-08T11:00:00Z, by: human:alex.doe }\n"),
            ["id", "title", "status"]);

        Assert.Equal(JsonValueKind.Null, record.GetProperty("trust").ValueKind);
    }

    private static string? Trust(string entries, string? field = null)
    {
        var front = field ?? $"verified:\n{entries}";
        return Record(Page(front), ["id", "title", "status", "verified"]).GetProperty("trust").GetString();
    }

    private static string Page(string verified) =>
        "---\nid: rpt-coverage\ntype: report\ntier: descriptive\nstatus: active\n"
        + "owner: human:alex.doe\n" + verified + "---\n\n"
        + "# Clause coverage\n\n`Report: rpt-coverage` `ACTIVE`\n";

    private static JsonElement Record(string text, string[] fields)
    {
        var type = new TypeSchema
        {
            Key = "reports",
            TypeName = "report",
            Folder = "reports",
            Page = "reports.md",
            IdPrefix = "rpt",
            Export = new ExportSpec { Version = 2, Fields = fields }
        };

        var schema = new Schema { ByFolder = new Dictionary<string, TypeSchema> { ["reports"] = type } };
        var doc = Required.Parsed("reports/coverage.md", text, schema);

        var tree = new Tree(new HashSet<string>([doc.Rel], StringComparer.Ordinal), _ => text);
        var corpus = new LoadedCorpus
        {
            Schema = schema,
            Descriptor = new CorpusDescriptor { Name = "test-corpus", ContentVersion = "2.1.0" },
            Tree = tree,
            Adopted = [type],
            Docs = [doc],
            Templates = [],
            SkippedNoFrontmatter = 0
        };

        var plan = Exporter.Plan(corpus, null, null,
            new ExportRun("2026-08-17T00:00:00Z", new DateOnly(2026, 8, 17), "abc123", false));

        return JsonDocument
            .Parse(plan.Files.Single(f => f.Path == "reports/rpt-coverage.json").Content)
            .RootElement;
    }
}
