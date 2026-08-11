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
    // The defaults are what a sound type carries, so that a case reports the one fault it declares and
    // nothing else. Each is overridable, because each has its own cases further down.
    private static TypeSchema Widgets(string idStyle = "slug", string folder = "widgets",
        string shape = TypeSchema.CollectionShape,
        (string Name, FieldSpec Spec)[]? fields = null, RuleSpec[]? rules = null,
        string[]? sections = null, string tier = "descriptive", string summary = "A widget.",
        string goesHere = "A widget", string labelPlural = "Widgets", string detail = "It is a widget.",
        (string Other, string Text)[]? versus = null, bool lineage = true) => new()
    {
        TypeName = "widget",
        Key = folder,
        Versus = versus ?? [],
        Lineage = lineage ? new LineageSpec("None.", "", "") : null,
        Folder = folder,
        Shape = shape,
        IdStyle = idStyle,
        Tier = tier,
        Summary = summary,
        GoesHere = goesHere,
        LabelPlural = labelPlural,
        Detail = detail,
        FieldOrder = [.. (fields ?? []).Select(x => x.Name)],
        Fields = (fields ?? []).ToDictionary(x => x.Name, x => x.Spec),
        Rules = rules ?? [],
        OptionalSections = sections ?? []
    };

    // The tiers a type may claim, and the field that admits them, as a sound schema carries them — so a
    // case declaring nothing about tiers reports nothing about them. CheckTiers has its own cases below.
    private static readonly string[] TierNames = ["decided", "normative", "descriptive", "procedural", "observed"];

    private static Schema WithTiers(TypeSchema? widgets = null, IEnumerable<string>? tiers = null,
        IEnumerable<string>? admitted = null) => new()
    {
        Tiers = [.. (tiers ?? TierNames).Select(t => new TierSpec(t, t, "how it behaves", ""))],
        UniversalOrder = ["tier"],
        Universal = new Dictionary<string, FieldSpec>
        {
            ["tier"] = new() { Name = "tier", Type = "enum", Values = [.. admitted ?? TierNames] }
        },
        ByFolder = widgets is null
            ? new Dictionary<string, TypeSchema>()
            : new Dictionary<string, TypeSchema> { ["widgets"] = widgets }
    };

    private static List<Finding> Check(TypeSchema widgets) => Check(WithTiers(widgets));

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

    // A description is rendered into the type page's checks table, which is read by scanning. The bound
    // is held here rather than left to review because the table is generated: an over-long cell reads as
    // deliberate in the diff, and there is nowhere else it would be noticed.
    [Fact]
    public void A_rule_description_longer_than_the_table_allows_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(rules:
        [
            new RuleSpec { Id = "very-wordy", Description = new string('x', Generator.DescriptionMax + 1) }
        ])));

        Assert.Equal("schema-shape", finding.Check);
        Assert.Contains($"the limit is {Generator.DescriptionMax}", finding.Message);
    }

    // Asked of a rule that runs, not only of one that does not: a dispatched rule is precisely the one
    // whose description reaches the table, and CheckRule lets those go early.
    [Fact]
    public void The_bound_holds_for_a_rule_that_is_dispatched_too()
    {
        var implemented = DocumentRules.All.SelectMany(r => r.Emits).First().Id;
        var finding = Assert.Single(Check(Widgets(rules:
        [
            new RuleSpec { Id = implemented, Description = new string('x', Generator.DescriptionMax + 1) }
        ])));

        Assert.Equal("schema-shape", finding.Check);
    }

    [Fact]
    public void A_description_at_the_bound_passes()
        => Assert.Empty(Check(Widgets(rules:
        [
            new RuleSpec { Id = "just-fits", Description = new string('x', Generator.DescriptionMax) }
        ])));

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

    // -- what a type says about itself --

    [Theory]
    [InlineData("decided")]
    [InlineData("normative")]
    [InlineData("descriptive")]
    [InlineData("procedural")]
    [InlineData("observed")]
    public void Every_declared_tier_passes(string tier)
        => Assert.Empty(Check(Widgets(tier: tier)));

    // A type claiming a tier nothing declares has no heading to sit under and no behaviour to inherit.
    [Fact]
    public void A_tier_no_file_declares_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(tier: "experimental")));

        Assert.Equal("schema-shape", finding.Check);
        Assert.Contains("experimental", finding.Message);
    }

    // -- the two files that between them define a tier --

    [Fact]
    public void A_tier_the_field_admits_and_no_file_names_is_reported()
    {
        var finding = Assert.Single(Check(WithTiers(admitted: [.. TierNames, "experimental"])));

        Assert.Equal(".schema/_tiers.yaml", finding.File);
        Assert.Contains("admits 'experimental'", finding.Message);
    }

    [Fact]
    public void A_tier_declared_that_no_document_may_carry_is_reported()
    {
        var finding = Assert.Single(Check(WithTiers(tiers: [.. TierNames, "experimental"])));

        Assert.Equal(".schema/_tiers.yaml", finding.File);
        Assert.Contains("does not admit it", finding.Message);
    }

    [Fact]
    public void A_tier_with_nothing_to_head_its_section_is_reported()
    {
        var schema = WithTiers();
        var tiers = schema.Tiers.Select(t => t.Name == "observed" ? t with { Label = "" } : t).ToList();

        var finding = Assert.Single(Check(new Schema
        {
            Tiers = tiers, UniversalOrder = schema.UniversalOrder, Universal = schema.Universal
        }));

        Assert.Contains("declares no 'label:'", finding.Message);
    }

    // Required where `label:` is not, because a singular can be derived from the type name and a plural
    // cannot — an `s` appended to `nfr` is not "NFRs".
    [Fact]
    public void A_type_that_does_not_name_its_collection_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(labelPlural: "")));

        Assert.Equal("schema-shape", finding.Check);
        Assert.Contains("no 'label-plural:'", finding.Message);
    }

    [Fact]
    public void A_type_that_does_not_say_what_it_is_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(summary: "")));

        Assert.Equal("schema-shape", finding.Check);
        Assert.Contains("no 'summary:'", finding.Message);
    }

    [Fact]
    public void A_type_that_does_not_say_what_goes_in_it_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(goesHere: "")));

        Assert.Equal("schema-shape", finding.Check);
        Assert.Contains("no 'goes-here:'", finding.Message);
    }

    // Held to the bound a rule's description is held to, and for the same reason: both are table cells.
    [Fact]
    public void A_summary_too_long_for_the_cell_it_becomes_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(summary: new string('x', Generator.DescriptionMax + 1))));

        Assert.Equal("schema-shape", finding.Check);
        Assert.Contains($"the limit is {Generator.DescriptionMax}", finding.Message);
    }

    [Fact]
    public void A_summary_at_the_bound_passes()
        => Assert.Empty(Check(Widgets(summary: new string('x', Generator.DescriptionMax))));

    [Fact]
    public void A_type_with_no_paragraph_beneath_its_one_liner_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(detail: "")));

        Assert.Equal("schema-shape", finding.Check);
        Assert.Contains("no 'detail:'", finding.Message);
    }

    // The paragraph is prose, not a cell, so the bound the other three are held to does not apply to it.
    [Fact]
    public void A_detail_longer_than_a_table_cell_passes()
        => Assert.Empty(Check(Widgets(detail: new string('x', Generator.DescriptionMax * 3))));

    [Fact]
    public void A_type_that_names_no_prior_art_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(lineage: false)));

        Assert.Equal("schema-shape", finding.Check);
        Assert.Contains("no 'lineage.prior-art:'", finding.Message);
    }

    // "None" is one of the answers, and the two columns beside it are questions a type with no ancestor
    // cannot answer — so an empty pair is a settled state rather than an unfinished one.
    [Fact]
    public void A_prior_art_of_none_with_nothing_beside_it_passes()
        => Assert.Empty(Check(Widgets()));

    // -- the disambiguations, which are the one thing a type says about another --

    private static Schema TwoTypes(params (string Key, (string Other, string Text)[] Versus)[] types)
    {
        var schema = WithTiers();
        return new Schema
        {
            Tiers = schema.Tiers, UniversalOrder = schema.UniversalOrder, Universal = schema.Universal,
            ByFolder = types.ToDictionary(t => t.Key, t => Widgets(folder: t.Key, versus: t.Versus))
        };
    }

    [Fact]
    public void A_versus_against_a_type_no_schema_covers_is_reported()
    {
        var finding = Assert.Single(Check(TwoTypes(("widgets", [("gizmos", "Not a type here.")]))));

        Assert.Equal("schema-dispatch", finding.Check);
        Assert.Contains("versus: gizmos", finding.Message);
    }

    [Fact]
    public void A_versus_against_itself_is_reported()
    {
        var finding = Assert.Single(Check(TwoTypes(("widgets", [("widgets", "Widget vs Widget.")]))));

        Assert.Equal("schema-shape", finding.Check);
        Assert.Contains("against itself", finding.Message);
    }

    // A pair both sides declare renders twice, with two accounts of one distinction and nothing keeping
    // them in step. Reported against the second file to declare it, naming the first.
    [Fact]
    public void A_pair_declared_from_both_sides_is_reported()
    {
        var finding = Assert.Single(Check(TwoTypes(
            ("adrs", [("widgets", "One account.")]),
            ("widgets", [("adrs", "And another.")]))));

        Assert.Equal(".schema/widgets.yaml", finding.File);
        Assert.Contains("'adrs.yaml' already declares", finding.Message);
    }

    [Fact]
    public void A_pair_declared_from_one_side_passes()
        => Assert.Empty(Check(TwoTypes(
            ("adrs", [("widgets", "One account, written once.")]),
            ("widgets", []))));
}
