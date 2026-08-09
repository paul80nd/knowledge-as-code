using kac.core;

// In-process unit tests for the pass that reads the schema itself. Built from in-memory objects,
// because what each case pins is a single declaration held against a single dispatch table, and a
// fixture would have to carry a whole broken type file to say the same thing. The fixture that does
// carry one — tests/fixtures/schema-declarations — is there to prove the findings reach the CLI.
//
// Every case here is a declaration that was accepted in silence before, and each was found in a real
// schema rather than imagined for a test.

namespace kac.tests;

public class SchemaCheckTests
{
    private static TypeSchema Widgets(string idStyle = "slug", string folder = "widgets",
        string shape = TypeSchema.CollectionShape,
        (string Name, FieldSpec Spec)[]? fields = null, RuleSpec[]? rules = null,
        string[]? sections = null) => new()
    {
        TypeName = "widget",
        Folder = folder,
        Shape = shape,
        IdStyle = idStyle,
        FieldOrder = [.. (fields ?? []).Select(x => x.Name)],
        Fields = (fields ?? []).ToDictionary(x => x.Name, x => x.Spec),
        Rules = rules ?? [],
        OptionalSections = sections ?? []
    };

    private static List<Finding> Check(TypeSchema widgets) =>
        Check(new Schema { ByFolder = new Dictionary<string, TypeSchema> { ["widgets"] = widgets } });

    private static List<Finding> Check(Schema schema)
    {
        var findings = new List<Finding>();
        SchemaChecks.Check(schema, findings);
        return findings;
    }

    // -- rules --

    // The arrangement that reads as enforced from every angle and is not.
    [Fact]
    public void A_rule_claiming_a_severity_that_nothing_dispatches_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(
            rules: [new RuleSpec { Id = "widgets-are-blue", Severity = Sev.Warning }])));

        Assert.Equal("schema-dispatch", finding.Check);
        Assert.Equal(".schema/widgets.yaml", finding.File);
        Assert.Contains("widgets-are-blue", finding.Message);
    }

    // A rule with no severity is an intention, and the type page renders it as one. Nothing about it is
    // a defect — it is the state most of the taxonomy's rules are honestly in.
    [Fact]
    public void A_rule_with_no_severity_is_an_intention_and_passes()
        => Assert.Empty(Check(Widgets(
            rules: [new RuleSpec { Id = "widgets-are-blue", Description = "One day." }])));

    // What the loader could not read at all, reported where it was written rather than thrown.
    [Fact]
    public void A_rule_the_loader_could_not_read_is_reported_against_its_file()
    {
        var finding = Assert.Single(Check(Widgets(rules:
        [
            new RuleSpec { Id = "widgets-are-blue", Problem = "rule 'widgets-are-blue': unknown fact 'colour'." }
        ])));

        Assert.Equal("schema-unreadable", finding.Check);
        Assert.Equal(".schema/widgets.yaml", finding.File);
        Assert.Contains("unknown fact", finding.Message);
    }

    // -- fields --

    [Fact]
    public void A_ref_naming_a_folder_no_schema_covers_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(fields:
        [
            ("stored-in", new FieldSpec { Name = "stored-in", Type = "list", Of = "id", Refs = ["data"] })
        ])));

        Assert.Equal("schema-dispatch", finding.Check);
        Assert.Contains("ref: data", finding.Message);
    }

    // Each entry of a list ref is its own promise and each is checked. A scalar `ref:` parses to a
    // one-entry list, so both forms arrive here the same way.
    [Fact]
    public void Every_entry_of_a_list_ref_is_checked_and_a_covered_one_passes()
    {
        var finding = Assert.Single(Check(Widgets(fields:
        [
            ("promoted-to", new FieldSpec { Name = "promoted-to", Type = "list", Of = "id", Refs = ["widgets", "gadgets"] })
        ])));

        Assert.Contains("ref: gadgets", finding.Message);
    }

    // Only an enum's range is applied, so a vocabulary declared anywhere else enforces nothing.
    [Fact]
    public void A_values_list_on_anything_but_an_enum_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(fields:
        [
            ("tags", new FieldSpec { Name = "tags", Type = "list", Of = "string", Values = ["public", "internal"] })
        ])));

        Assert.Equal("schema-dispatch", finding.Check);
        Assert.Contains("'tags'", finding.Message);
    }

    [Fact]
    public void A_values_list_on_an_enum_is_what_the_key_is_for()
        => Assert.Empty(Check(Widgets(fields:
        [
            ("status", new FieldSpec { Name = "status", Type = "enum", Values = ["draft", "active"] })
        ])));

    // The section a field mirrors is read from the record, so a name the type's own `sections:` block
    // does not offer is a reconciliation against a heading no record may carry.
    [Fact]
    public void A_mirrors_section_the_type_does_not_declare_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(fields:
        [
            ("related", new FieldSpec
                { Name = "related", Type = "list", Of = "id", Refs = ["widgets"], MirrorsSection = "See also" })
        ], sections: ["Summary"])));

        Assert.Equal("schema-shape", finding.Check);
        Assert.Contains("See also", finding.Message);
    }

    // Any section the type declares reconciles, not one fixed name.
    [Fact]
    public void A_mirrors_section_the_type_declares_passes()
        => Assert.Empty(Check(Widgets(fields:
        [
            ("depends-on", new FieldSpec
                { Name = "depends-on", Type = "list", Of = "id", Refs = ["widgets"], MirrorsSection = "Dependencies" })
        ], sections: ["Dependencies"])));

    [Fact]
    public void A_universal_field_is_reported_against_the_file_that_declares_it()
    {
        var schema = new Schema
        {
            UniversalOrder = ["tags"],
            Universal = new Dictionary<string, FieldSpec>
            {
                ["tags"] = new() { Name = "tags", Type = "list", Values = ["a", "b"] }
            }
        };

        Assert.Equal(".schema/_universal.yaml", Assert.Single(Check(schema)).File);
    }

    // -- type shape --

    [Fact]
    public void An_id_style_with_no_branch_behind_it_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(idStyle: "roman-numeral")));

        Assert.Equal("schema-dispatch", finding.Check);
        Assert.Contains("roman-numeral", finding.Message);
    }

    [Theory]
    [InlineData("numbered")]
    [InlineData("mnemonic")]
    [InlineData("slug")]
    [InlineData("literal")]
    public void Every_style_the_id_checks_apply_passes(string style)
        => Assert.Empty(Check(Widgets(idStyle: style)));

    // A collection with no folder has nowhere to put a record; a single-document type with one has
    // somewhere it must not. Both were indistinguishable from a type that simply lost the key.
    [Fact]
    public void A_collection_with_no_folder_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(folder: "")));

        Assert.Equal("schema-shape", finding.Check);
        Assert.Contains("no 'folder:'", finding.Message);
    }

    [Fact]
    public void A_single_document_type_declaring_a_folder_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(idStyle: "literal", shape: TypeSchema.SingleDocumentShape)));

        Assert.Equal("schema-shape", finding.Check);
        Assert.Contains("folder: widgets", finding.Message);
    }

    // The shape decides which of the two folder rules applies, so a shape nothing reads is reported on
    // its own and the folder is left alone — there is no telling which rule it should have met.
    [Fact]
    public void A_shape_the_tool_does_not_act_on_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(shape: "chapter")));

        Assert.Equal("schema-dispatch", finding.Check);
        Assert.Contains("chapter", finding.Message);
    }
}
