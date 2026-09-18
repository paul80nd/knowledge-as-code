using System.Text.Json;
using kac.core;

// The editor's copy of the two field vocabularies, held to the tool's own.
//
// `meta/type.schema.json` is read by an editor through the `$schema` line each type file opens with, and by nothing in
// CI. So it is the one place a value the tool dropped goes on being offered, and the offer is what an author writes
// their schema against. `ValueChecks` holds the lists it is compared with, beside the code dispatching them.

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public class MetaSchemaTests
{
    [Fact]
    public void The_type_enum_offers_what_the_tool_dispatches()
    {
        Assert.Equal(ValueChecks.FieldTypes.Order(StringComparer.Ordinal), EnumOf("type"));
    }

    [Fact]
    public void The_of_enum_offers_what_an_entry_is_read_as()
    {
        Assert.Equal(ValueChecks.EntryTypes.Order(StringComparer.Ordinal), EnumOf("of"));
    }

    // A line source naming one thing, as `export.parts.line:` offers them. The prefixed families are patterns in
    // the same `anyOf` and are not tested here: a pattern admits a name the type supplies, so there is no list to
    // compare. Dropping a source from the tool and leaving it in the enum is what this catches.
    [Fact]
    public void The_line_source_enum_offers_what_the_exporter_fills()
    {
        using var json = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(Repo.Root, ".schema", "meta", "type.schema.json")));

        var offered = json.RootElement.GetProperty("$defs").GetProperty("lineSource").GetProperty("anyOf")[0]
            .GetProperty("enum").EnumerateArray().Select(v => v.GetString() ?? "").Order(StringComparer.Ordinal);

        Assert.Equal(PartLineSource.Fixed.Order(StringComparer.Ordinal), offered);
    }

    // One key of the shared `field` definition, which an entry key is declared with as well: the `entry:` block
    // points every key at this same definition, so one list answers for both.
    private static IEnumerable<string> EnumOf(string key)
    {
        using var json = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(Repo.Root, ".schema", "meta", "type.schema.json")));

        return json.RootElement.GetProperty("$defs").GetProperty("field").GetProperty("properties")
            .GetProperty(key).GetProperty("enum").EnumerateArray()
            .Select(v => v.GetString() ?? "").Order(StringComparer.Ordinal).ToList();
    }
}
