using System.Text.Json;
using kac.core;

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

    /// <summary>
    /// The prefixed families are patterns in the same <c>anyOf</c> and are not tested here: a pattern admits a name the
    /// type supplies, so there is no list to compare.
    /// </summary>
    [Fact]
    public void The_line_source_enum_offers_what_the_exporter_fills()
    {
        using var json = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(Repo.Root, ".schema", "meta", "type.schema.json")));

        var offered = json.RootElement.GetProperty("$defs").GetProperty("lineSource").GetProperty("anyOf")[0]
            .GetProperty("enum").EnumerateArray().Select(v => v.GetString() ?? "").Order(StringComparer.Ordinal);

        Assert.Equal(PartLineSource.Fixed.Order(StringComparer.Ordinal), offered);
    }

    /// <summary>
    /// One key of the shared <c>field</c> definition, which an entry key is declared with as well: the <c>entry:</c>
    /// block points every key at this same definition, so one list answers for both.
    /// </summary>
    private static IEnumerable<string> EnumOf(string key)
    {
        using var json = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(Repo.Root, ".schema", "meta", "type.schema.json")));

        return json.RootElement.GetProperty("$defs").GetProperty("field").GetProperty("properties")
            .GetProperty(key).GetProperty("enum").EnumerateArray()
            .Select(v => v.GetString() ?? "").Order(StringComparer.Ordinal).ToList();
    }
}
