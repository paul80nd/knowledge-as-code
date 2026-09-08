using kac.core;

// In-process unit tests for a field whose range the corpus states: `values: $corpus.<name>` in the schema,
// and `enums:` in `.corpus.yaml`.
//
// The `corpus-enums` and `corpus-enums-undeclared` fixtures cover both faults through the CLI. What lives
// here is the binding itself, where a schema and a descriptor are two strings and a case is one of each.

namespace kac.tests;

public class CorpusEnumTests
{
    [Fact]
    public void A_field_drawing_on_the_corpus_carries_the_values_the_corpus_wrote()
    {
        var field = Platform(["dotnet-tool", "static"]);

        Assert.Equal("platform", field.CorpusEnum);
        Assert.Equal(["dotnet-tool", "static"], field.Values);
    }

    // Null rather than empty, because the value checks read a null range as no range at all and would
    // otherwise refuse every value a record carries. What the corpus owes is reported once, elsewhere.
    [Fact]
    public void A_corpus_that_wrote_nothing_leaves_the_range_unresolved()
    {
        var field = Platform(null);

        Assert.Equal("platform", field.CorpusEnum);
        Assert.Null(field.Values);
    }

    // The two prefixes are read in one switch, so a schema mixing them is worth pinning.
    [Fact]
    public void A_field_drawing_on_the_schema_is_untouched_by_what_the_corpus_wrote()
    {
        var schema = Schema.Load(Files("$enums.criticality"), Declared(["dotnet-tool"]));

        Assert.Null(schema.ByFolder["services"].Fields["platform"].CorpusEnum);
        Assert.Equal(["critical", "supporting"], schema.ByFolder["services"].Fields["platform"].Values);
    }

    [Fact]
    public void A_name_the_corpus_answers_nothing_to_is_no_fault_in_the_schema()
        => Assert.Empty(Schema.Load(Files("$corpus.platform"), Declared(null)).UnreadKeys);

    private static FieldSpec Platform(IReadOnlyList<string>? declared) =>
        Schema.Load(Files("$corpus.platform"), Declared(declared)).ByFolder["services"].Fields["platform"];

    private static Dictionary<string, IReadOnlyList<string>> Declared(IReadOnlyList<string>? platform) =>
        platform is null
            ? new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
            : new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal) { ["platform"] = platform };

    // One type and one field, so that what the loader made of `values:` is the only thing under test.
    private static Dictionary<string, string> Files(string values) =>
        new(StringComparer.Ordinal)
        {
            ["_enums.yaml"] = "enums:\n  criticality:\n    values: [critical, supporting]\n",
            ["services.yaml"] =
                "type: service\nfolder: services\npage: services.md\n"
                + "fields:\n  platform:\n    required: true\n    type: enum\n"
                + $"    values: {values}\n"
        };
}
