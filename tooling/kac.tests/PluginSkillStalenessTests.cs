using System.Text.Json.Nodes;
using kac.core;

// The staleness section of each lookup skill, held to the fields its type actually exports.
//
// A skill is the whole of what a consumer is told about the export, so a skill silent about the review date leaves a
// reader quoting a policy that passed it. Which fields there are differs by type: `processes` exports no review date
// at all, and a skill naming one would send a reader to a key that is not there. The spelling differs too.
// A type declaring parts writes `reviewBy` on the line, and a type without them exports `review-by` under the schema's
// own name. Deriving the expectation from the schema is what keeps a rule stated once from binding the types it does
// not reach.

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public class PluginSkillStalenessTests
{
    private static readonly string Plugin = Path.Combine(Repo.Root, "template", ".plugin");

    // The heading opening the section. Every lookup skill writes it about its own noun, so the words between are
    // the type's rather than fixed.
    private const string Opens = "## Say when ";
    private const string Closes = " is unsettled";

    // Quoted beside a bad `status` or a passed review date, because an export reads the same however old it is.
    private const string Taken = "generatedAt";

    // Which type each skill reads, taken from the plugin manifest. `requires` is what the bundler trims a component
    // against, so a second list here would be free to disagree with the one that ships.
    public static TheoryData<string, string> Each()
    {
        var manifest = JsonRead.Parse(Files.ReadLf(Path.Combine(Plugin, ".claude-plugin", "plugin.json")));
        var components = manifest?["metadata"]?["components"] as JsonArray
                         ?? throw new InvalidOperationException("the plugin manifest declares no metadata.components.");

        var data = new TheoryData<string, string>();
        foreach (var component in components)
        {
            var path = JsonRead.Str(component?["path"]) ?? "";
            if (!path.StartsWith("skills/", StringComparison.Ordinal)) continue;

            var requires = (component?["requires"] as JsonArray)?
                .Select(r => JsonRead.Str(r)).OfType<string>().ToList() ?? [];

            // A skill naming no type reads no record and reports no staleness. `corpus-retrieval` fetches the
            // published source, which is the current file rather than the copy this section is about.
            if (requires.Count == 0) continue;

            // One skill writes one staleness section, so a second type would put its values somewhere this test
            // does not read. `PluginSkillFieldTests` refuses the same shape for the same reason.
            if (requires.Count != 1)
                throw new InvalidOperationException(
                    $"'{path}' requires {requires.Count} types, and one skill holds one staleness section.");

            data.Add(path["skills/".Length..], requires[0].Split('@')[0]);
        }

        return data;
    }

    // Every staleness field the export writes is named in the section, and `generatedAt` beside them. A field the
    // type does not export is not asserted, because there is nothing on the line for a reader to test.
    [Theory]
    [MemberData(nameof(Each))]
    public void Its_staleness_section_names_every_field_its_type_exports(string skill, string type)
    {
        var schema = Schema.Load(Repo.Root);
        var section = Section(Path.Combine(Plugin, "skills", skill, "SKILL.md"));

        var expected = Carried(schema.ByFolder[type]).Append(Taken).Order(StringComparer.Ordinal);
        var missing = expected.Where(key => !section.Contains(key, StringComparison.Ordinal)).ToList();

        Assert.True(missing.Count == 0,
            $"{skill} reads {type} and its staleness section never names {string.Join(", ", missing)}.");
    }

    // Every value of `status` other than the settled one is a state the reader has to be told about, and each type
    // declares its own. A skill silent about one reports a planned control, or a superseded standard, as though the
    // record were in force.
    [Theory]
    [MemberData(nameof(Each))]
    public void Its_staleness_section_names_every_unsettled_status_its_type_declares(string skill, string type)
    {
        var schema = Schema.Load(Repo.Root);
        var section = Section(Path.Combine(Plugin, "skills", skill, "SKILL.md"));

        var missing = Unsettled(schema.ByFolder[type])
            .Where(value => !section.Contains($"status: {value}", StringComparison.Ordinal)).ToList();

        Assert.True(missing.Count == 0,
            $"{skill} reads {type} and its staleness section never names status {string.Join(", ", missing)}.");
    }

    // The states a record of this type can be in other than in force. The settled one is `in-force:` in the type's
    // own schema file, and it is not `active` across the set: `adrs` settles at `accepted`, `services` at `live`.
    // A type leaving `status` to `_universal.yaml` states no range, which `SchemaChecks.CheckInForce` passes over
    // and a lookup skill cannot be written from, so the throw is what stops one being written for it.
    private static IEnumerable<string> Unsettled(TypeSchema type)
    {
        var status = type.Fields.GetValueOrDefault("status");
        var settled = status?.InForce
                      ?? throw new InvalidOperationException(
                          $"'{type.Folder}' has a lookup skill, and '.schema/{type.Folder}.yaml' declares no "
                          + "'status:' range with an 'in-force:' value beside it. Declare both, and say in that "
                          + "skill's staleness section what every other value means.");

        return (status.Values ?? [])
            .Where(v => !string.Equals(v, settled, StringComparison.Ordinal))
            .Order(StringComparer.Ordinal);
    }

    // The staleness keys one type puts on the line a skill reads. A type declaring parts writes the line's own keys,
    // and one without them exports the record's frontmatter under the schema's names.
    private static IEnumerable<string> Carried(TypeSchema type)
    {
        var line = type.DeclaredExport.Line.Select(f => f.Key).ToHashSet(StringComparer.Ordinal);
        var fields = type.DeclaredExport.Fields.ToHashSet(StringComparer.Ordinal);

        if (line.Contains("status") || fields.Contains("status")) yield return "status";
        if (line.Contains("reviewBy")) yield return "reviewBy";
        else if (fields.Contains("review-by")) yield return "review-by";
    }

    // The text under the staleness heading, up to the next heading of the same level.
    private static string Section(string path)
    {
        var lines = Files.ReadLf(path).Split('\n');

        var from = Array.FindIndex(lines, l =>
            l.StartsWith(Opens, StringComparison.Ordinal) && l.EndsWith(Closes, StringComparison.Ordinal));
        Assert.True(from >= 0, $"{path} holds no '{Opens}… {Closes}' section.");

        var to = Array.FindIndex(lines, from + 1, l => l.StartsWith("## ", StringComparison.Ordinal));
        return string.Join('\n', lines[(from + 1)..(to < 0 ? lines.Length : to)]);
    }
}
