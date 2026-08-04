// In-process unit tests for the pure Schema helpers, built from in-memory objects (no YAML files
// needed). Schema.Load itself is covered end-to-end by the golden suite against the real schema.

using kac.core;

namespace kac.tests;

public class SchemaTests
{
    private static Schema SampleSchema() => new()
    {
        UniversalOrder = ["id", "status"],
        Universal = new Dictionary<string, FieldSpec>
        {
            ["id"] = new() { Name = "id" },
            ["status"] = new() { Name = "status" } // universal status
        },
        Reserved = ["wiki"]
    };

    private static TypeSchema SampleType() => new()
    {
        FieldOrder = ["status", "date"],
        Fields = new Dictionary<string, FieldSpec> { ["status"] = new() { Name = "status", Required = true } } // per-type override
    };

    [Fact]
    public void EffectiveField_prefers_the_type_override_then_falls_back_to_universal()
    {
        var schema = SampleSchema();
        var t = SampleType();

        Assert.Same(t.Fields["status"], schema.EffectiveField(t, "status")); // type wins
        Assert.Same(schema.Universal["id"], schema.EffectiveField(t, "id")); // falls back to universal
        Assert.Null(schema.EffectiveField(t, "nope"));                       // unknown → null
    }

    [Fact]
    public void KnownKeys_is_universal_then_type_then_reserved_deduplicated()
    {
        var schema = SampleSchema();

        // 'status' appears in both universal and the type's field order — it collapses to one entry.
        Assert.Equal(["id", "status", "date", "wiki"], schema.KnownKeys(SampleType()));
    }
}
