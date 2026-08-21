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
        Reserved = ["title"]
    };

    private static TypeSchema SampleType() => new()
    {
        FieldOrder = ["status", "date"],
        Fields = new Dictionary<string, FieldSpec>
            { ["status"] = new() { Name = "status", Required = true } } // per-type override
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

    // The comparison is on membership, sorted to be stable.
    [Fact]
    public void KnownKeys_spans_universal_type_and_reserved_deduplicated()
    {
        // 'status' appears in both universal and the type's field order — it collapses to one entry.
        var keys = TypeSchema.DeriveKnownKeys(["id", "status"], ["status", "date"], ["title"]);

        Assert.Equal(["date", "id", "status", "title"], keys.Order(StringComparer.Ordinal));
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

    // -- required-when --
    //
    // The vocabulary is closed on purpose, and a form outside it stops the load. A form that parsed to
    // nothing would be a condition that never holds, leaving the field it guards quietly unrequired
    // while the schema goes on claiming it is — which is the failure these cases exist to prevent.

    [Theory]
    [InlineData("status == accepted", "accepted", true)]
    [InlineData("status == accepted", "draft", false)]
    [InlineData("mechanism != not-enforced", "ci", true)]
    [InlineData("mechanism != not-enforced", "not-enforced", false)]
    [InlineData("classification in [personal, special-category]", "personal", true)]
    [InlineData("classification in [personal, special-category]", "special-category", true)]
    [InlineData("classification in [personal, special-category]", "internal", false)]
    public void RequiredWhen_reads_every_form_the_schema_uses(string condition, string actual, bool expected)
        => Assert.Equal(expected, Schema.ParseRequiredWhen("f", condition)!.Holds(actual));

    // Including for `!=`, where "not equal to anything" is tempting but wrong: the field the condition
    // names is missing, `required-field` is already saying so, and a second finding would report one
    // omission as two.
    [Theory]
    [InlineData("status == accepted")]
    [InlineData("mechanism != not-enforced")]
    [InlineData("classification in [personal, special-category]")]
    public void RequiredWhen_does_not_hold_when_the_field_it_names_is_absent(string condition)
        => Assert.False(Schema.ParseRequiredWhen("f", condition)!.Holds(null));

    [Fact]
    public void RequiredWhen_is_absent_when_nothing_is_declared()
        => Assert.Null(Schema.ParseRequiredWhen("f", null));

    [Theory]
    [InlineData("status")]
    [InlineData("status ~ accepted")]
    [InlineData("status in personal, special-category")]
    public void A_required_when_the_schema_cannot_read_stops_the_load(string condition)
        => Assert.Throws<RuleExprException>(() => Schema.ParseRequiredWhen("f", condition));
}
