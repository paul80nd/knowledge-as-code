using System.Text.Json.Nodes;
using kac.core;

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public class PluginSkillStalenessTests
{
    private static readonly string Plugin = Path.Combine(Repo.Root, "template", ".plugin");

    /// <summary>
    /// Every lookup skill writes this heading about its own noun, so the words after it are the type's.
    /// </summary>
    private const string Opens = "## Say when ";
    private const string Closes = " is unsettled";

    private const string Taken = "generatedAt";

    /// <summary>
    /// Which type each skill reads, taken from the plugin manifest. <c>requires</c> is what the bundler trims a
    /// component against, so a second list here would be free to disagree with the one that ships.
    /// </summary>
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

            // A skill naming no type reads no record and reports no staleness.
            if (requires.Count == 0) continue;

            // One skill writes one staleness section, so a second type would put its values somewhere this test
            // does not read.
            if (requires.Count != 1)
                throw new InvalidOperationException(
                    $"'{path}' requires {requires.Count} types, and one skill holds one staleness section.");

            data.Add(path["skills/".Length..], requires[0].Split('@')[0]);
        }

        return data;
    }

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

    /// <summary>
    /// A type declaring parts writes the line's own keys, and one without them exports the record's frontmatter under
    /// the schema's names.
    /// </summary>
    private static IEnumerable<string> Carried(TypeSchema type)
    {
        var line = type.DeclaredExport.Line.Select(f => f.Key).ToHashSet(StringComparer.Ordinal);
        var fields = type.DeclaredExport.Fields.ToHashSet(StringComparer.Ordinal);

        if (line.Contains("status") || fields.Contains("status")) yield return "status";
        if (line.Contains("reviewBy")) yield return "reviewBy";
        else if (fields.Contains("review-by")) yield return "review-by";
    }

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
