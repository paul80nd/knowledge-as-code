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

    // An empty range would refuse every value a record carries, so a corpus that opened the key and wrote
    // nothing under it reads exactly as one that never opened it.
    [Fact]
    public void A_corpus_that_wrote_an_empty_list_leaves_the_range_unresolved()
        => Assert.Null(Platform([]).Values);

    // A scalar reaches `Yaml.StrList` as nothing, which is the same state and the same reading.
    [Fact]
    public void A_range_written_as_a_scalar_leaves_it_unresolved()
    {
        var descriptor = Descriptor("corpus: probe\nenums:\n  platform: dotnet-api\n");

        Assert.Empty(Assert.Single(descriptor.Enums).Value);
    }

    // A range no record can satisfy, reported where the values were written rather than where they were met.
    [Fact]
    public void A_value_that_is_not_lower_case_is_reported_against_the_descriptor()
    {
        var finding = Assert.Single(Findings(PlatformField, ["Dotnet-Web"]));

        Assert.Equal(".corpus.yaml", finding.File);
        Assert.Contains("carries 'Dotnet-Web'", finding.Message);
    }

    [Fact]
    public void A_range_the_corpus_can_satisfy_is_silent()
        => Assert.Empty(Findings(PlatformField, ["dotnet-web"]));

    // `values:` is read wherever a field is declared, so the check has to look wherever one can be.
    [Fact]
    public void A_key_inside_an_object_entry_is_asked_for_its_range_too()
    {
        var finding = Assert.Single(Undeclared(
            "fields:\n  releases:\n    type: list\n    of: object\n    entry:\n"
            + "      channel:\n        type: enum\n        values: $corpus.channel\n"));

        Assert.Contains("'channel' on a service", finding.Message);
        Assert.Contains("`channel: [a, b]`", finding.Message);
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

    private const string PlatformField =
        "fields:\n  platform:\n    type: enum\n    values: $corpus.platform\n";

    private static List<Finding> Undeclared(string fields) => Findings(fields, null);

    // One service, so the pass has a record of the type and asks the question the record's arrival raises.
    private static List<Finding> Findings(string fields, IReadOnlyList<string>? declared)
    {
        var schema = Schema.Load(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["_universal.yaml"] = "fields:\n  id:\n    required: true\n",
            ["services.yaml"] = "type: service\nfolder: services\npage: services.md\n" + fields
        }, Declared(declared));

        const string record = "---\nid: svc-one\ntype: service\n---\n\n# One\n";
        var tree = new Tree(
            new HashSet<string>(StringComparer.Ordinal) { "services/one.md" }, _ => record, _ => true);

        return
        [
            .. Validator.CheckAll(Corpus.Load(tree, schema, WithEnums(declared)), Required.Today)
                .Where(f => f.Check.Value == "corpus-enum-undeclared")
        ];
    }

    private static CorpusDescriptor WithEnums(IReadOnlyList<string>? platform)
    {
        var descriptor = new CorpusDescriptor();
        if (platform is not null) descriptor.Enums["platform"] = platform;

        return descriptor;
    }

    private static CorpusDescriptor Descriptor(string yaml)
    {
        var dir = Directory.CreateTempSubdirectory("kac-corpus-enums");
        File.WriteAllText(Path.Combine(dir.FullName, ".corpus.yaml"), yaml);
        try { return CorpusDescriptor.Load(dir.FullName); }
        finally { dir.Delete(true); }
    }

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
