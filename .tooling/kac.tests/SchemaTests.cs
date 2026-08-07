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

    // Order carries no meaning in the result — the only question asked of it is whether a key is in it —
    // so the comparison is on membership, sorted to be stable.
    [Fact]
    public void KnownKeys_spans_universal_type_and_reserved_deduplicated()
    {
        // 'status' appears in both universal and the type's field order — it collapses to one entry.
        var keys = TypeSchema.DeriveKnownKeys(["id", "status"], ["status", "date"], ["wiki"]);

        Assert.Equal(["date", "id", "status", "wiki"], keys.Order(StringComparer.Ordinal));
    }

    // Pairs span the whole chain rather than neighbouring keys, so a document that omits an intermediate
    // key still has the keys either side of it constrained against one another.
    [Fact]
    public void KeyOrderEdges_are_every_pair_within_a_chain_not_only_neighbours()
    {
        var edges = TypeSchema.DeriveKeyOrderEdges(["id", "status", "owner"], []);

        Assert.Contains(("id", "owner"), edges); // not neighbours, still ordered
        Assert.Contains(("id", "status"), edges);
        Assert.Contains(("status", "owner"), edges);
        Assert.DoesNotContain(("owner", "id"), edges); // the constraint has a direction
        Assert.Equal(3, edges.Count);
    }

    // Each chain constrains only its own keys. Nothing invents an order between a universal key and a
    // type key that no single chain places together, which is what lets the two files be written
    // independently of one another.
    [Fact]
    public void KeyOrderEdges_do_not_cross_between_the_two_chains()
    {
        var edges = TypeSchema.DeriveKeyOrderEdges(["id", "status"], ["status", "date"]);

        Assert.Contains(("id", "status"), edges);
        Assert.Contains(("status", "date"), edges);
        Assert.DoesNotContain(("id", "date"), edges);
        Assert.Equal(2, edges.Count);
    }
}
