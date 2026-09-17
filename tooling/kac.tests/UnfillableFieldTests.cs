using kac.core;

// In-process unit tests for the one case where `required-field` says nothing: a field pointing only at
// types nothing in the corpus supplies. The fixture corpus cannot cover it, because every corpus there
// adopts the types its own schema declares.

namespace kac.tests;

public class UnfillableFieldTests
{
    [Fact]
    public void A_required_ref_field_is_reported_where_the_corpus_adopted_the_type_it_points_at()
        => Assert.Equal("missing required field 'implemented-by'.",
            RequiredField(Ref("implemented-by", required: true), fillable: ["offerings", "services"]));

    [Fact]
    public void A_required_ref_field_is_skipped_where_the_corpus_declined_every_type_it_points_at()
        => Assert.Null(RequiredField(Ref("implemented-by", required: true), fillable: ["offerings"]));

    // One fillable target is enough. The record can fill the field with an id of that type.
    [Fact]
    public void A_ref_field_naming_two_types_is_reported_where_the_corpus_adopted_either()
        => Assert.Equal("missing required field 'explains'.",
            RequiredField(
                Ref("explains", required: true, refs: ["services", "offerings"]),
                fillable: ["explanations", "offerings"]));

    [Fact]
    public void A_required_when_obligation_is_skipped_where_the_corpus_declined_the_type_it_points_at()
        => Assert.Null(RequiredField(
            Ref("nfrs", required: false, refs: ["nfrs"], requiredWhen: "status == live"),
            fillable: ["offerings"]));

    [Fact]
    public void A_required_when_obligation_is_reported_where_the_corpus_adopted_the_type()
        => Assert.Equal("missing required field 'nfrs' (required when status == live).",
            RequiredField(
                Ref("nfrs", required: false, refs: ["nfrs"], requiredWhen: "status == live"),
                fillable: ["offerings", "nfrs"]));

    // `allow-literal` gives the field a value that resolves against nothing, so declining the type it
    // points at leaves it fillable.
    [Fact]
    public void A_ref_field_taking_a_literal_is_reported_whatever_the_corpus_adopted()
        => Assert.Equal("missing required field 'applies-to'.",
            RequiredField(
                Ref("applies-to", required: true, allowLiteral: ["all"]), fillable: ["offerings"]));

    // The default for every caller that validates one document on its own, and for the three test
    // harnesses that do.
    [Fact]
    public void A_caller_that_names_no_adopted_types_is_told_about_every_missing_field()
        => Assert.Equal("missing required field 'implemented-by'.",
            RequiredField(Ref("implemented-by", required: true), fillable: null));

    private static FieldSpec Ref(
        string name,
        bool required,
        IReadOnlyList<string>? refs = null,
        IReadOnlyList<string>? allowLiteral = null,
        string? requiredWhen = null) =>
        new()
        {
            Name = name,
            Required = required,
            Refs = refs ?? ["services"],
            AllowLiteral = allowLiteral ?? [],
            RequiredWhen = requiredWhen,
            RequiredWhenCondition = requiredWhen is null ? null : new RequiredWhen("status", false, ["live"])
        };

    // The whole document pass runs, so the finding under test is picked out of what it produced. The
    // record carries `status: live` to turn a `required-when` on, and nothing else: whatever else the
    // pass says about a document this bare is about other fields and other checks.
    private static string? RequiredField(FieldSpec spec, string[]? fillable)
    {
        var type = new TypeSchema
        {
            Key = "offerings",
            Folder = "offerings",
            Label = "Offering",
            LabelPlural = "Offerings",
            DeclaredFields = [spec]
        };
        var schema = new Schema { ByFolder = new Dictionary<string, TypeSchema> { [type.Key] = type } };
        var doc = Doc.Parse("offerings/borrowing.md", "---\nid: ofr-borrowing\nstatus: live\n---\n\n# An offering\n",
            schema);
        Assert.NotNull(doc);

        var found = new List<Finding>();
        Validator.CheckDocument(doc, schema, new Tree(new HashSet<string>(), _ => ""), found, Required.Today,
            fillable: fillable is null ? null : fillable.ToHashSet(StringComparer.Ordinal));

        return found.SingleOrDefault(x => x.Check.Value == "required-field" && x.Message.Contains($"'{spec.Name}'"))
            ?.Message;
    }

    // A type the corpus declined is still one an import can supply, and a record may then cite an id of
    // it. This runs the whole pass, because the set of fillable folders is built where a corpus is, and
    // `CheckDocument` is handed the answer.
    [Fact]
    public void A_type_an_import_supplies_is_one_a_record_here_can_cite()
        => Assert.Equal("missing required field 'implements'.",
            RequiredFieldInCorpus(Supplying("policy")));

    [Fact]
    public void A_type_nothing_supplies_is_one_no_record_here_can_cite()
        => Assert.Null(RequiredFieldInCorpus(ImportGraph.None));

    private static ImportGraph Supplying(string type) =>
        new([
            new Import("eng", "example-engineering", "0.1.0", null,
                [new ImportedRecord("eng", "pol-KNOW", type, "policies/pol-KNOW.md", false, [])])
        ], [], []);

    // A corpus adopting `standards` and not `policies`, holding one standard that names no `implements`.
    private static string? RequiredFieldInCorpus(ImportGraph imports)
    {
        var standards = new TypeSchema
        {
            Key = "standards",
            TypeName = "standard",
            Folder = "standards",
            Label = "Standard",
            LabelPlural = "Standards",
            DeclaredFields = [Ref("implements", required: true, refs: ["policies"])]
        };
        var policies = new TypeSchema
        {
            Key = "policies", TypeName = "policy", Folder = "policies", Label = "Policy", LabelPlural = "Policies"
        };
        var schema = new Schema
        {
            ByFolder = new Dictionary<string, TypeSchema>(StringComparer.Ordinal)
                { ["standards"] = standards, ["policies"] = policies }
        };

        const string rel = "standards/prose.md";
        const string text = "---\nid: std-PROSE\n---\n\n# A standard\n";
        var tree = new Tree(new HashSet<string>(StringComparer.Ordinal) { rel }, _ => text, p => p == rel);
        var descriptor = new CorpusDescriptor { Types = ["standards"] };

        return Validator.CheckAll(Corpus.Load(tree, schema, descriptor, imports), Required.Today)
            .SingleOrDefault(x => x.Check.Value == "required-field" && x.Message.Contains("'implements'"))
            ?.Message;
    }
}
