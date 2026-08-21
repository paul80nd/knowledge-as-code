using kac.core;

// In-process unit tests for the pass that reads the schema itself. Built from in-memory objects,
// because what each case pins is a single declaration held against a single dispatch table, and a
// fixture would have to carry a whole broken type file to say the same thing. The fixture that does
// carry one — tests/fixtures/schema-declarations — is there to prove the findings reach the CLI.
//
// Every case is a declaration a schema can carry that nothing else would report, and each was found in
// a real schema rather than imagined for a test.

namespace kac.tests;

public class SchemaCheckTests
{
    // The defaults are what a sound type carries, so that a case reports the one fault it declares and
    // nothing else. Each is overridable, because each has its own cases further down.
    private static TypeSchema Widgets(string idStyle = "slug", string folder = "widgets",
        (string Name, FieldSpec Spec)[]? fields = null, RuleSpec[]? rules = null,
        string[]? sections = null, string tier = "descriptive", string summary = "A widget.",
        string goesHere = "A widget", string labelPlural = "Widgets", string detail = "It is a widget.",
        (string Other, string Text)[]? versus = null, bool lineage = true, PartSpec? parts = null,
        ExportSpec? export = null) => new()
    {
        TypeName = "widget",
        Key = folder,
        Versus = versus ?? [],
        Lineage = lineage ? new LineageSpec("None.", "", "") : null,
        Folder = folder,
        IdStyle = idStyle,
        Tier = tier,
        Summary = summary,
        GoesHere = goesHere,
        LabelPlural = labelPlural,
        Detail = detail,
        FieldOrder = [.. (fields ?? []).Select(x => x.Name)],
        Fields = (fields ?? []).ToDictionary(x => x.Name, x => x.Spec),
        Rules = rules ?? [],
        OptionalSections = sections ?? [],
        Parts = parts,
        Export = export
    };

    // The tiers a type may claim, and the field that admits them, as a sound schema carries them — so a
    // case declaring nothing about tiers reports nothing about them. CheckTiers has its own cases below.
    private static readonly string[] TierNames = ["decided", "normative", "descriptive", "procedural", "observed"];

    // What `_checks.yaml` carries in a sound schema: an entry for every id the rule classes report
    // under. Defaulted here for the same reason the tiers are — a case about a field should not also
    // report that the catalogue is incomplete.
    private static IReadOnlyList<CheckDef> SoundChecks() =>
        [.. CheckCatalogue.EmittedByRules().Select(id => new CheckDef(id, Sev.Warning, "It is checked."))];

    private static Schema WithTiers(TypeSchema? widgets = null, IEnumerable<string>? tiers = null,
        IEnumerable<string>? admitted = null, IReadOnlyList<CheckDef>? checks = null) => new()
    {
        Checks = checks ?? SoundChecks(),
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

    // -- the checks the schema declares --

    // The direction a load can decide: the registry names what a rule class reports under, so an id
    // missing from the file is a check a reader would meet with no entry behind it.
    [Fact]
    public void A_check_a_rule_class_reports_under_and_the_schema_omits_is_reported()
    {
        var short1 = SoundChecks().Skip(1).ToList();

        var finding = Assert.Single(Check(WithTiers(checks: short1)));

        Assert.Equal("schema-dispatch", finding.Check.Value);
        Assert.Equal(".schema/_checks.yaml", finding.File);
        Assert.Contains(CheckCatalogue.EmittedByRules()[0].Value, finding.Message);
    }

    // -- rules --

    // The arrangement that reads as enforced from every angle and is not.
    [Fact]
    public void A_rule_claiming_a_severity_that_nothing_dispatches_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(
            rules: [new RuleSpec { Id = new RuleId("widgets-are-blue"), Severity = Sev.Warning }])));

        Assert.Equal("schema-dispatch", finding.Check.Value);
        Assert.Equal(".schema/widgets.yaml", finding.File);
        Assert.Contains("widgets-are-blue", finding.Message);
    }

    // A rule with no severity is an intention, and the type page renders it as one. Nothing about it is
    // a defect — it is the state most of the taxonomy's rules are honestly in.
    [Fact]
    public void A_rule_with_no_severity_is_an_intention_and_passes()
        => Assert.Empty(Check(Widgets(
            rules: [new RuleSpec { Id = new RuleId("widgets-are-blue"), Description = "One day." }])));

    // What the loader could not read at all, reported where it was written rather than thrown.
    [Fact]
    public void A_rule_the_loader_could_not_read_is_reported_against_its_file()
    {
        var finding = Assert.Single(Check(Widgets(rules:
        [
            new RuleSpec
                { Id = new RuleId("widgets-are-blue"), Problem = "rule 'widgets-are-blue': unknown fact 'colour'." }
        ])));

        Assert.Equal("schema-unreadable", finding.Check.Value);
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

        Assert.Equal("schema-dispatch", finding.Check.Value);
        Assert.Contains("ref: data", finding.Message);
    }

    // Each entry of a list ref is its own promise and each is checked. A scalar `ref:` parses to a
    // one-entry list, so both forms arrive here the same way.
    [Fact]
    public void Every_entry_of_a_list_ref_is_checked_and_a_covered_one_passes()
    {
        var finding = Assert.Single(Check(Widgets(fields:
        [
            ("promoted-to",
                new FieldSpec { Name = "promoted-to", Type = "list", Of = "id", Refs = ["widgets", "gadgets"] })
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

        Assert.Equal("schema-dispatch", finding.Check.Value);
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

        Assert.Equal("schema-shape", finding.Check.Value);
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
            new RuleSpec { Id = new RuleId("very-wordy"), Description = new string('x', Generator.DescriptionMax + 1) }
        ])));

        Assert.Equal("schema-shape", finding.Check.Value);
        Assert.Contains($"the limit is {Generator.DescriptionMax}", finding.Message);
    }

    // Asked of a rule that runs, not only of one that does not: a dispatched rule is precisely the one
    // whose description reaches the table, and CheckRule lets those go early.
    [Fact]
    public void The_bound_holds_for_a_rule_that_is_dispatched_too()
    {
        var implemented = DocumentRules.All.First().RuleId;
        var finding = Assert.Single(Check(Widgets(rules:
        [
            new RuleSpec { Id = implemented, Description = new string('x', Generator.DescriptionMax + 1) }
        ])));

        Assert.Equal("schema-shape", finding.Check.Value);
    }

    [Fact]
    public void A_description_at_the_bound_passes()
        => Assert.Empty(Check(Widgets(rules:
        [
            new RuleSpec { Id = new RuleId("just-fits"), Description = new string('x', Generator.DescriptionMax) }
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
            Checks = SoundChecks(),
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

        Assert.Equal("schema-dispatch", finding.Check.Value);
        Assert.Contains("roman-numeral", finding.Message);
    }

    [Theory]
    [InlineData("numbered")]
    [InlineData("mnemonic")]
    [InlineData("slug")]
    public void Every_style_the_id_checks_apply_passes(string style)
        => Assert.Empty(Check(Widgets(idStyle: style)));

    // A type with no folder has nowhere to put a record, and an absent key and a deliberate
    // `folder: null` are the same empty string by the time this reads it.
    [Fact]
    public void A_type_with_no_folder_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(folder: "")));

        Assert.Equal("schema-shape", finding.Check.Value);
        Assert.Contains("no 'folder:'", finding.Message);
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

        Assert.Equal("schema-shape", finding.Check.Value);
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
            Tiers = tiers, UniversalOrder = schema.UniversalOrder, Universal = schema.Universal,
            Checks = schema.Checks
        }));

        Assert.Contains("declares no 'label:'", finding.Message);
    }

    // Required where `label:` is not, because a singular can be derived from the type name and a plural
    // cannot — an `s` appended to `nfr` is not "NFRs".
    [Fact]
    public void A_type_that_does_not_name_its_collection_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(labelPlural: "")));

        Assert.Equal("schema-shape", finding.Check.Value);
        Assert.Contains("no 'label-plural:'", finding.Message);
    }

    [Fact]
    public void A_type_that_does_not_say_what_it_is_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(summary: "")));

        Assert.Equal("schema-shape", finding.Check.Value);
        Assert.Contains("no 'summary:'", finding.Message);
    }

    [Fact]
    public void A_type_that_does_not_say_what_goes_in_it_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(goesHere: "")));

        Assert.Equal("schema-shape", finding.Check.Value);
        Assert.Contains("no 'goes-here:'", finding.Message);
    }

    // Held to the bound a rule's description is held to, and for the same reason: both are table cells.
    [Fact]
    public void A_summary_too_long_for_the_cell_it_becomes_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(summary: new string('x', Generator.DescriptionMax + 1))));

        Assert.Equal("schema-shape", finding.Check.Value);
        Assert.Contains($"the limit is {Generator.DescriptionMax}", finding.Message);
    }

    [Fact]
    public void A_summary_at_the_bound_passes()
        => Assert.Empty(Check(Widgets(summary: new string('x', Generator.DescriptionMax))));

    [Fact]
    public void A_type_with_no_paragraph_beneath_its_one_liner_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(detail: "")));

        Assert.Equal("schema-shape", finding.Check.Value);
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

        Assert.Equal("schema-shape", finding.Check.Value);
        Assert.Contains("no 'lineage.prior-art:'", finding.Message);
    }

    // An empty pair beside "None" is a settled state rather than an unfinished one.
    [Fact]
    public void A_prior_art_of_none_with_nothing_beside_it_passes()
        => Assert.Empty(Check(Widgets()));

    // -- where a type keeps its parts --

    // A type declaring `parts:` is the only reason a citation into its records resolves, so every way of
    // getting the block wrong ends with the type silently offering none.
    [Fact]
    public void A_part_source_nothing_extracts_names_the_ones_that_are_read()
    {
        var parts = new PartSpec("footnotes", "", [], []) { Section = "Terms" };
        var found = Assert.Single(Check(Widgets(sections: ["Terms"], parts: parts)));

        Assert.Equal("schema-dispatch", found.Check.Value);
        Assert.Contains("'parts.source: footnotes', which nothing extracts", found.Message);
        Assert.Contains("'headings', 'table'", found.Message);
    }

    // The same fault as a `mirrors-section:` at a section the type has not got: the walk runs to a
    // heading no record may carry, and every citation into the type fails against what it did not find.
    [Fact]
    public void A_part_section_the_type_does_not_declare_is_reported()
    {
        var parts = new PartSpec(PartSpec.Headings, "", [], []) { Section = "Glossary" };
        var found = Assert.Single(Check(Widgets(sections: ["Terms"], parts: parts)));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("'parts.section: Glossary'", found.Message);
    }

    // A table with no binding modals reports every row as opening with none of them, and lists the
    // modals to write as nothing at all.
    [Fact]
    public void A_table_source_declaring_no_binding_modals_is_reported()
    {
        var parts = new PartSpec(PartSpec.Table, "^[A-Z]+$", [], ["SHOULD"]) { Section = "Clauses" };
        var found = Assert.Single(Check(Widgets(sections: ["Clauses"], parts: parts)));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("declares no 'binding:'", found.Message);
    }

    // The modals belong to the table source, so a type sourcing headings is not asked for them.
    [Fact]
    public void A_heading_source_is_not_asked_for_modals()
        => Assert.Empty(Check(Widgets(sections: ["Terms"],
            parts: new PartSpec(PartSpec.Headings, "", [], []) { Section = "Terms" })));

    [Fact]
    public void A_type_declaring_no_parts_is_asked_nothing_about_them()
        => Assert.Empty(Check(Widgets()));

    // -- what a type contributes to an export --

    // Selection is by key, and this is where the key is resolved. A section named here and absent from
    // the type's own `sections:` block is a projection promising words no record carries.
    [Fact]
    public void An_exported_section_the_type_does_not_declare_is_reported()
    {
        var export = new ExportSpec { Sections = [("Provenance", ExportSpec.Full)] };
        var found = Assert.Single(Check(Widgets(sections: ["Scope"], export: export)));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("'export.sections: Provenance'", found.Message);
    }

    // Neither `summary` nor `reference` is carried yet. A type reaching for one is told so, rather than
    // exporting silence under it.
    [Fact]
    public void An_exported_fidelity_nothing_carries_names_the_ones_that_are_written()
    {
        var export = new ExportSpec { Sections = [("Scope", ExportSpec.Summary)] };
        var found = Assert.Single(Check(Widgets(sections: ["Scope"], export: export)));

        Assert.Equal("schema-dispatch", found.Check.Value);
        Assert.Contains("at fidelity 'summary', which nothing carries", found.Message);
        Assert.Contains("'full'", found.Message);
    }

    // Fidelity is what a consumer reads the export for, so an entry naming none is a declaration with
    // its meaning missing rather than one to fall back on a default.
    [Fact]
    public void An_export_entry_declaring_no_fidelity_is_reported()
    {
        var export = new ExportSpec { Sections = [("Scope", "")] };
        var found = Assert.Single(Check(Widgets(sections: ["Scope"], export: export)));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("at no fidelity", found.Message);
    }

    [Fact]
    public void An_exported_field_no_record_carries_is_reported()
    {
        var export = new ExportSpec { Fields = ["colour"] };
        var found = Assert.Single(Check(Widgets(export: export)));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("'export.fields: colour'", found.Message);
    }

    // A field the type inherits is a field its records carry, so the universal layer answers as well as
    // the type's own declarations.
    [Fact]
    public void An_exported_field_the_type_inherits_passes()
        => Assert.Empty(Check(Widgets(export: new ExportSpec { Fields = ["tier"] })));

    // Parts are taken from wherever the `parts:` block locates them, so a type exporting them without
    // one has named a source that does not exist.
    [Fact]
    public void Exported_parts_on_a_type_that_locates_none_are_reported()
    {
        var found = Assert.Single(Check(Widgets(export: new ExportSpec { Parts = ExportSpec.Full })));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("carries no 'parts:' block", found.Message);
    }

    [Fact]
    public void A_projection_every_key_of_which_resolves_passes()
        => Assert.Empty(Check(Widgets(
            sections: ["Scope", "Terms"],
            parts: new PartSpec(PartSpec.Headings, "", [], []) { Section = "Terms" },
            export: new ExportSpec
            {
                Fields = ["tier"],
                Sections = [("Scope", ExportSpec.Full)],
                Parts = ExportSpec.Full
            })));

    [Fact]
    public void A_type_declaring_no_export_is_asked_nothing_about_one()
        => Assert.Empty(Check(Widgets()));

    private static Schema TwoTypes(params (string Key, (string Other, string Text)[] Versus)[] types)
    {
        var schema = WithTiers();
        return new Schema
        {
            Tiers = schema.Tiers, UniversalOrder = schema.UniversalOrder, Universal = schema.Universal,
            Checks = schema.Checks,
            ByFolder = types.ToDictionary(t => t.Key, t => Widgets(folder: t.Key, versus: t.Versus))
        };
    }

    [Fact]
    public void A_versus_against_a_type_no_schema_covers_is_reported()
    {
        var finding = Assert.Single(Check(TwoTypes(("widgets", [("gizmos", "Not a type here.")]))));

        Assert.Equal("schema-dispatch", finding.Check.Value);
        Assert.Contains("versus: gizmos", finding.Message);
    }

    [Fact]
    public void A_versus_against_itself_is_reported()
    {
        var finding = Assert.Single(Check(TwoTypes(("widgets", [("widgets", "Widget vs Widget.")]))));

        Assert.Equal("schema-shape", finding.Check.Value);
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
