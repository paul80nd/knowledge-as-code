using kac.core;

// In-process unit tests for `ref-resolves`, which is answered across the whole corpus rather than
// within a document. What a fixture would have to build for each of these is a second mini-corpus with
// a second schema over it, so the graphs are handed to the check directly: one ADR carrying the field
// under test, and a document of each type for it to point at.

namespace kac.tests;

public class RefCheckTests
{
    [Fact]
    public void A_target_of_the_type_the_declaration_names_passes()
        => Assert.Empty(Refs(Field("supersedes", "adrs"), "adr-0002"));

    // The message names both ends as a reader would say them: what the target turned out to be, and
    // what the field was declared to take.
    [Fact]
    public void A_target_of_another_type_is_reported_naming_both()
    {
        var found = Refs(Field("supersedes", "adrs"), "svc-catalogue");

        Assert.Equal("ref-resolves", Assert.Single(found).Check.Value);
        Assert.Equal("'supersedes' points at 'svc-catalogue', which is a Service, not an ADR.",
            Assert.Single(found).Message);
    }

    // A field may name several types, and any of them satisfies it.
    [Fact]
    public void A_declaration_naming_several_types_admits_each_of_them()
        => Assert.Empty(Refs(Field("promoted-to", "adrs", "services"), "svc-catalogue"));

    [Fact]
    public void A_declaration_naming_several_types_offers_them_all_when_none_matches()
        => Assert.Equal("'promoted-to' points at 'dat-borrowers', which is Data, not an ADR or a Service.",
            Assert.Single(Refs(Field("promoted-to", "adrs", "services"), "dat-borrowers")).Message);

    // A type whose singular and plural are one word takes no article, at either end of the message.
    [Fact]
    public void A_type_whose_label_is_a_mass_noun_is_named_without_an_article()
    {
        Assert.Equal("'data-stores' points at 'svc-catalogue', which is a Service, not Data.",
            Assert.Single(Refs(Field("data-stores", "data"), "svc-catalogue")).Message);
        Assert.Equal("'supersedes' points at 'dat-borrowers', which is Data, not an ADR.",
            Assert.Single(Refs(Field("supersedes", "adrs"), "dat-borrowers")).Message);
    }

    // An id nothing carries is the other half of this check, and the type question cannot be asked of a
    // document that is not there.
    [Fact]
    public void A_target_that_does_not_exist_is_reported_as_absent()
        => Assert.Equal("'supersedes' points at 'adr-0099', which does not exist.",
            Assert.Single(Refs(Field("supersedes", "adrs"), "adr-0099")).Message);

    // A `ref:` at a folder no schema covers is `schema-dispatch`'s to report.
    [Fact]
    public void A_ref_at_a_folder_no_schema_covers_still_asks_that_the_target_exists()
    {
        Assert.Empty(Refs(Field("supersedes", "widgets"), "svc-catalogue"));
        Assert.Equal("'supersedes' points at 'adr-0099', which does not exist.",
            Assert.Single(Refs(Field("supersedes", "widgets"), "adr-0099")).Message);
    }

    // A wrong-type target carries no counterpart field to point back with, so the reciprocity check
    // stays out of it and one fault stays one finding.
    [Fact]
    public void A_wrong_type_target_on_a_reciprocal_field_is_reported_once()
        => Assert.Equal("ref-resolves",
            Assert.Single(Refs(Reciprocating("supersedes", "adrs"), "svc-catalogue")).Check.Value);

    // Where the target is of the right type, the same field asks the question this one is named for.
    [Fact]
    public void A_target_of_the_right_type_is_still_held_to_pointing_back()
        => Assert.Equal("reciprocal", Assert.Single(Refs(Reciprocating("supersedes", "adrs"), "adr-0002")).Check.Value);

    private static FieldSpec Field(string name, params string[] refs) =>
        new() { Name = name, Type = "id", Refs = refs };

    private static FieldSpec Reciprocating(string name, params string[] refs) =>
        new() { Name = name, Type = "id", Refs = refs, Reciprocal = "superseded-by" };

    // Three types, so that a field can name one, two or none of them. The labels are the real ones
    // because how a message names a type is read off them. The article follows how the label sounds,
    // as "an ADR" and "a Service" do, and a label whose plural is the same word takes none.
    //
    // The cast is fixed: an ADR carrying the field, and one document of each type to aim it at. An id
    // outside the cast is the target that does not exist.
    private static List<Finding> Refs(FieldSpec field, string target)
    {
        var adrs = new TypeSchema
        {
            Key = "adrs", Folder = "adrs", Label = "ADR", LabelPlural = "ADRs", FieldOrder = [field.Name],
            Fields = new Dictionary<string, FieldSpec> { [field.Name] = field }
        };
        var services = new TypeSchema
            { Key = "services", Folder = "services", Label = "Service", LabelPlural = "Services" };
        var data = new TypeSchema { Key = "data", Folder = "data", Label = "Data", LabelPlural = "Data" };

        var schema = new Schema
        {
            ByFolder = new Dictionary<string, TypeSchema>
                { ["adrs"] = adrs, ["services"] = services, ["data"] = data }
        };

        string[] cast = ["adrs/adr-0002.md", "services/svc-catalogue.md", "data/dat-borrowers.md"];

        var docs = cast
            .Select(rel => Parse(schema, rel, Path.GetFileNameWithoutExtension(rel), null))
            .Prepend(Parse(schema, "adrs/adr-0001.md", "adr-0001", $"{field.Name}: {target}"))
            .OfType<Doc>()
            .ToList();
        Assert.Equal(cast.Length + 1, docs.Count); // a document that did not parse would quietly shrink the corpus

        var found = new List<Finding>();
        Validator.CheckCorpus(schema, docs, found);
        return found;
    }

    private static Doc? Parse(Schema schema, string rel, string id, string? extra) =>
        Doc.Parse(rel, $"---\nid: {id}\n{(extra is null ? "" : extra + "\n")}---\n\n# {id}\n", schema);
}
