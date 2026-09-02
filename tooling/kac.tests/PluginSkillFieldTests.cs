using System.Text.Json.Nodes;
using kac.core;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Syntax;

// The field table in each lookup skill, held to the line the exporter writes.
//
// A skill is the whole of what a consumer is told about the export. It reads a vendored copy of the data, with no
// access to this repository and no schema beside the file, so a table calling `obligations` a list is somebody
// looping over a string. `tooling/tests/round-trip.sh` asserts what the export carries and never what a skill says
// about it.

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public class PluginSkillFieldTests
{
    // The vocabulary a Type cell is written in. Four words, because the exporter writes four shapes.
    private const string Text = "string";
    private const string OrNull = "string or null";
    private const string List = "list of strings, or null";
    private const string OrMissing = "string, or absent";

    private static readonly string Plugin = Path.Combine(Repo.Root, "template", ".plugin");

    private static readonly MarkdownPipeline Pipeline =
        new MarkdownPipelineBuilder().UseYamlFrontMatter().UsePipeTables().Build();

    // Which type each skill reads, taken from the plugin manifest rather than from a list here. `requires` is what
    // the bundler trims a component against, so the manifest is already the answer and a second copy would drift.
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

            if (requires.Count != 1)
                throw new InvalidOperationException(
                    $"'{path}' requires {requires.Count} types, and one skill holds one field table.");

            data.Add(path["skills/".Length..], requires[0].Split('@')[0]);
        }

        return data;
    }

    // Every key the line carries has a row, every row names a key the line carries, and each says the shape the
    // exporter wrote. A reader has nothing else to go on: a field described as a list is read with a loop.
    [Theory]
    [MemberData(nameof(Each))]
    public void Its_field_table_states_the_type_the_exporter_writes(string skill, string type)
    {
        var schema = Schema.Load(Repo.Root);
        var file = Path.Combine(Plugin, "skills", skill, "SKILL.md");

        Assert.Equal(Sorted(Expected(schema, schema.ByFolder[type])), Sorted(Stated(file)));
    }

    // One entry per field, as `<field>: <type>`, so a mismatch reads as a diff of two lists rather than as two
    // dictionaries the runner prints whole.
    private static string Sorted(IEnumerable<KeyValuePair<string, string>> fields) =>
        string.Join('\n', fields
            .OrderBy(f => f.Key, StringComparer.Ordinal)
            .Select(f => $"{f.Key}: {f.Value}"));

    // What the exporter writes for one type, key by key.
    private static Dictionary<string, string> Expected(Schema schema, TypeSchema type)
    {
        var expected = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // Stamped onto an inherited line by `Exporter.Stamped`, so it is the one key that can be missing.
            [Exporter.ShortcodeKey] = OrMissing
        };

        foreach (var (key, source) in type.DeclaredExport.Line)
            expected[key] = Written(schema, type, source);

        return expected;
    }

    // The shape one declared source arrives in. A source with no answer here throws rather than defaulting, so a
    // source added to `PartLineSource` forces a decision about what the three skills have to say about it.
    private static string Written(Schema schema, TypeSchema type, string source)
    {
        // A frontmatter field the schema requires unconditionally is on every record, so the line always holds it.
        // A `required-when:` field is required of some records only, and `Exporter.Absent` writes null for the rest.
        if (PartLineSource.Argument(source, PartLineSource.FrontPrefix) is { } field)
            return schema.EffectiveField(type, field) is { Required: true, RequiredWhen: null } ? Text : OrNull;

        if (PartLineSource.Argument(source, PartLineSource.ColumnPrefix) is not null) return OrNull;

        return source switch
        {
            PartLineSource.PartId or PartLineSource.PartKey or PartLineSource.PartText
                or PartLineSource.PartAnchor or PartLineSource.RecordId or PartLineSource.RecordType
                or PartLineSource.RecordPath => Text,
            PartLineSource.PartLead or PartLineSource.PartAside or PartLineSource.PartLevel => OrNull,
            PartLineSource.PartSeeAlso => List,
            _ => throw new InvalidOperationException(
                $"'{source}' is a line source this test holds no type for. Add it here and to the three skills.")
        };
    }

    // The Field and Type columns of the skill's field table. A row may name several fields sharing one type, so
    // `status`, `reviewBy` arrives as two entries and a wrong type reads against the field that holds it.
    private static Dictionary<string, string> Stated(string file)
    {
        var table = Markdown.Parse(Files.ReadLf(file), Pipeline).Descendants<Table>()
                        .FirstOrDefault(t => Cells(t.OfType<TableRow>().First()) is ["Field", "Type", ..])
                    ?? throw new InvalidOperationException($"{file} holds no table opening 'Field | Type'.");

        var stated = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var row in table.OfType<TableRow>().Where(r => !r.IsHeader))
        {
            var cells = Cells(row);
            foreach (var name in cells[0].Split(',').Select(n => n.Trim()).Where(n => n.Length > 0))
                stated[name] = cells[1];
        }

        return stated;
    }

    private static string[] Cells(TableRow row) =>
        [.. row.OfType<TableCell>().Select(c => Md.PlainText((c.FirstOrDefault() as LeafBlock)?.Inline))];
}
