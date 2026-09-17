using kac.core;

// In-process unit tests for the one case where `required-field` says nothing: a field pointing only at
// types the corpus declined. The fixture corpus cannot cover it, because every corpus there adopts the
// types its own schema declares.

namespace kac.tests;

public class UnfillableFieldTests
{
    [Fact]
    public void A_required_ref_field_is_reported_where_the_corpus_adopted_the_type_it_points_at()
        => Assert.Equal("missing required field 'implemented-by'.",
            RequiredField(Ref("implemented-by", required: true), adopted: ["offerings", "services"]));

    [Fact]
    public void A_required_ref_field_is_skipped_where_the_corpus_declined_every_type_it_points_at()
        => Assert.Null(RequiredField(Ref("implemented-by", required: true), adopted: ["offerings"]));

    // One adopted target is enough. The record can fill the field with an id of that type.
    [Fact]
    public void A_ref_field_naming_two_types_is_reported_where_the_corpus_adopted_either()
        => Assert.Equal("missing required field 'explains'.",
            RequiredField(
                Ref("explains", required: true, refs: ["services", "offerings"]),
                adopted: ["explanations", "offerings"]));

    [Fact]
    public void A_required_when_obligation_is_skipped_where_the_corpus_declined_the_type_it_points_at()
        => Assert.Null(RequiredField(
            Ref("nfrs", required: false, refs: ["nfrs"], requiredWhen: "status == live"),
            adopted: ["offerings"]));

    [Fact]
    public void A_required_when_obligation_is_reported_where_the_corpus_adopted_the_type()
        => Assert.Equal("missing required field 'nfrs' (required when status == live).",
            RequiredField(
                Ref("nfrs", required: false, refs: ["nfrs"], requiredWhen: "status == live"),
                adopted: ["offerings", "nfrs"]));

    // `allow-literal` gives the field a value that resolves against nothing, so declining the type it
    // points at leaves it fillable.
    [Fact]
    public void A_ref_field_taking_a_literal_is_reported_whatever_the_corpus_adopted()
        => Assert.Equal("missing required field 'applies-to'.",
            RequiredField(
                Ref("applies-to", required: true, allowLiteral: ["all"]), adopted: ["offerings"]));

    // The default for every caller that validates one document on its own, and for the three test
    // harnesses that do.
    [Fact]
    public void A_caller_that_names_no_adopted_types_is_told_about_every_missing_field()
        => Assert.Equal("missing required field 'implemented-by'.",
            RequiredField(Ref("implemented-by", required: true), adopted: null));

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
    private static string? RequiredField(FieldSpec spec, string[]? adopted)
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
            adopted: adopted is null ? null : adopted.ToHashSet(StringComparer.Ordinal));

        return found.SingleOrDefault(x => x.Check.Value == "required-field" && x.Message.Contains($"'{spec.Name}'"))
            ?.Message;
    }
}
