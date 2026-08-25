using kac.core;

// In-process unit tests for the pass that reads the schema itself. Built from in-memory objects,
// because what each case pins is a single declaration held against a single dispatch table, and a
// fixture would have to carry a whole broken type file to say the same thing. The fixture that does
// carry one is tests/fixtures/schema-declarations, and it is there to prove the findings reach the CLI.
//
// Every case is a declaration a schema can carry that nothing else would report. The types are invented,
// because a case naming a real one would move whenever that type did.

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

    // The tiers a type may claim, and the field that admits them, as a sound schema carries them. So a
    // case declaring nothing about tiers reports nothing about them. CheckTiers has its own cases below.
    private static readonly string[] TierNames = ["decided", "normative", "descriptive", "procedural", "observed"];

    // What `_checks.yaml` carries in a sound schema: an entry for every id the rule classes report
    // under. Defaulted here for the same reason the tiers are: a case about a field should not also
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

    // The registry names what a rule class reports under, so an id missing from the file is a check a
    // reader would meet with no entry behind it.
    [Fact]
    public void A_check_a_rule_class_reports_under_and_the_schema_omits_is_reported()
    {
        var short1 = SoundChecks().Skip(1).ToList();

        var finding = Assert.Single(Check(WithTiers(checks: short1)));

        Assert.Equal("schema-dispatch", finding.Check.Value);
        Assert.Equal(".schema/_checks.yaml", finding.File);
        Assert.Contains(CheckCatalogue.EmittedByRules()[0].Value, finding.Message);
    }

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

    // Nothing about a rule with no severity is a defect: it is the state most of the taxonomy's rules
    // are honestly in.
    [Fact]
    public void A_rule_with_no_severity_is_an_intention_and_passes()
        => Assert.Empty(Check(Widgets(
            rules: [new RuleSpec { Id = new RuleId("widgets-are-blue"), Description = "One day." }])));

    // The loader records the fault rather than throwing.
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

    // A scalar `ref:` parses to a one-entry list, so both forms arrive here the same way.
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

    // The section a field mirrors is read from the record, so the reconciliation runs against a heading
    // no record may carry.
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

    // A dispatched rule is precisely the one whose description reaches the table, and CheckRule lets
    // those go early.
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

    // An absent key and a deliberate `folder: null` are the same empty string by the time this reads it.
    [Fact]
    public void A_type_with_no_folder_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(folder: "")));

        Assert.Equal("schema-shape", finding.Check.Value);
        Assert.Contains("no 'folder:'", finding.Message);
    }

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
    // cannot: an `s` appended to `nfr` is not "NFRs".
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

    // The walk runs to a heading no record may carry, and every citation into the type fails against
    // what it did not find.
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

    // The modals belong to the table source.
    [Fact]
    public void A_heading_source_is_not_asked_for_modals()
        => Assert.Empty(Check(Widgets(sections: ["Terms"],
            parts: new PartSpec(PartSpec.Headings, "", [], []) { Section = "Terms" })));

    [Fact]
    public void A_type_declaring_no_parts_is_asked_nothing_about_them()
        => Assert.Empty(Check(Widgets()));

    // A section the export names and the type does not declare is a projection promising words no
    // record carries.
    [Fact]
    public void An_exported_section_the_type_does_not_declare_is_reported()
    {
        var export = new ExportSpec { Version = 1, Sections = [("Provenance", ExportSpec.Full)] };
        var found = Assert.Single(Check(Widgets(sections: ["Scope"], export: export)));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("'export.sections: Provenance'", found.Message);
    }

    // The author is often one letter from one of the three that work.
    [Fact]
    public void An_exported_fidelity_nothing_carries_names_the_ones_that_are_written()
    {
        var export = new ExportSpec { Version = 1, Sections = [("Scope", "gist")] };
        var found = Assert.Single(Check(Widgets(sections: ["Scope"], export: export)));

        Assert.Equal("schema-dispatch", found.Check.Value);
        Assert.Contains("at fidelity 'gist', which nothing carries there", found.Message);
        Assert.Contains("'full', 'reference', 'summary'", found.Message);
    }

    // The two vocabularies are separate, so a fidelity sound against a section is reported against a line.
    [Fact]
    public void A_part_line_reaching_for_a_reduced_fidelity_is_reported()
    {
        var export = new ExportSpec
        {
            Version = 1,
            Parts = ExportSpec.Summary,
            PartsDeclared = true,
            Line = [("id", PartLineSource.PartId)]
        };

        var found = Assert.Single(Check(Widgets(sections: ["Terms"], export: export,
            parts: new PartSpec(PartSpec.Headings, "", [], []) { Section = "Terms" })));

        Assert.Equal("schema-dispatch", found.Check.Value);
        Assert.Contains("'export.parts.fidelity:' at fidelity 'summary'", found.Message);
        Assert.Contains("An export carries 'full'.", found.Message);
    }

    // Fidelity is what a consumer reads the export for, so there is no default to fall back on.
    [Fact]
    public void An_export_entry_declaring_no_fidelity_is_reported()
    {
        var export = new ExportSpec { Version = 1, Sections = [("Scope", "")] };
        var found = Assert.Single(Check(Widgets(sections: ["Scope"], export: export)));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("at no fidelity", found.Message);
    }

    [Fact]
    public void An_exported_field_no_record_carries_is_reported()
    {
        var export = new ExportSpec { Version = 1, Fields = ["colour"] };
        var found = Assert.Single(Check(Widgets(export: export)));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("'export.fields: colour'", found.Message);
    }

    // A field the type inherits is a field its records carry, so the universal layer answers as well as
    // the type's own declarations.
    [Fact]
    public void An_exported_field_the_type_inherits_passes()
        => Assert.Empty(Check(Widgets(export: new ExportSpec { Version = 1, Fields = ["tier"] })));

    // A consumer holds a type's files to its shape version, and without one has nothing to refuse a
    // moved shape by.
    [Fact]
    public void An_export_block_declaring_no_shape_version_is_reported()
    {
        var found = Assert.Single(Check(Widgets(export: new ExportSpec())));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("no 'version:' above 0", found.Message);
    }

    // Parts are taken from wherever the `parts:` block locates them, so a type exporting them without
    // one has named a source that does not exist.
    [Fact]
    public void Exported_parts_on_a_type_that_locates_none_are_reported()
    {
        var export = new ExportSpec { Version = 1, Parts = ExportSpec.Full, PartsDeclared = true };
        var found = Assert.Single(Check(Widgets(export: export)));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("carries no 'parts:' block", found.Message);
    }

    // The line is the whole of what a consumer greps, so a type exporting parts and naming none of its
    // keys writes a file of empty objects.
    [Fact]
    public void Exported_parts_with_no_line_are_reported()
    {
        var found = Assert.Single(Check(Widgets(
            sections: ["Terms"],
            parts: new PartSpec(PartSpec.Headings, "", [], []) { Section = "Terms" },
            export: new ExportSpec { Version = 1, Parts = ExportSpec.Full, PartsDeclared = true })));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("no 'line:' beneath it", found.Message);
    }

    // A source outside the vocabulary fills nothing, so the key would be null on every line while the
    // schema goes on saying the type carries it.
    [Fact]
    public void A_line_source_nothing_fills_is_reported()
    {
        var found = Assert.Single(Check(Line(("colour", "part.colour"))));

        Assert.Equal("schema-dispatch", found.Check.Value);
        Assert.Contains("at source 'part.colour', which nothing fills", found.Message);
        Assert.Contains("'part.text'", found.Message);
    }

    [Fact]
    public void A_line_key_with_no_source_is_reported()
    {
        var found = Assert.Single(Check(Line(("colour", ""))));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("with no source", found.Message);
    }

    // `front.` resolves against the same declarations `export.fields:` does, so a field neither the type
    // nor the universal layer declares is reported rather than exported as a null.
    [Fact]
    public void A_line_source_naming_a_field_no_record_carries_is_reported()
    {
        var found = Assert.Single(Check(Line(("colour", "front.colour"))));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("'front.colour'", found.Message);
    }

    [Fact]
    public void A_line_source_naming_an_inherited_field_passes()
        => Assert.Empty(Check(Line(("tier", "front.tier"))));

    // A record's heading is a part's own text here.
    [Fact]
    public void A_line_source_naming_the_title_as_a_field_is_reported()
        => Assert.Contains("'front.title'", Assert.Single(Check(Line(("title", "front.title")))).Message);

    // A heading-sourced type stands under no headers, so a column source on one names nothing.
    [Fact]
    public void A_line_source_naming_a_column_the_type_does_not_declare_is_reported()
    {
        var found = Assert.Single(Check(Line(("alignment", "column.Alignment"))));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("declares no such column", found.Message);
    }

    [Fact]
    public void A_line_source_naming_a_declared_column_passes()
        => Assert.Empty(Check(Line(
            ("alignment", "column.Alignment"),
            parts: new PartSpec(PartSpec.Table, "", ["MUST"], [])
                { Section = "Terms", Columns = ["Id", "Clause", "Alignment"] })));

    // A table row is its own body, so neither source has anything to read and both would write null on
    // every line.
    [Theory]
    [InlineData("part.lead")]
    [InlineData("part.aside")]
    public void A_line_source_reading_a_body_against_a_table_is_reported(string source)
    {
        var found = Assert.Single(Check(Line(
            ("definition", source),
            parts: new PartSpec(PartSpec.Table, "", ["MUST"], []) { Section = "Terms" })));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("A row is its own body", found.Message);
    }

    // The level is matched against the modals the type declares, so a type declaring none carries a key
    // that is null wherever it appears. A heading-sourced type is the case with nothing else to say it:
    // a table-sourced one declaring no modals is already reported against its `parts:` block.
    [Fact]
    public void A_line_source_taking_a_level_from_a_type_declaring_none_is_reported()
    {
        var found = Assert.Single(Check(Line(("level", "part.level"))));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("no binding or advisory levels", found.Message);
    }

    [Fact]
    public void A_projection_every_key_of_which_resolves_passes()
        => Assert.Empty(Check(Widgets(
            sections: ["Scope", "Terms"],
            parts: new PartSpec(PartSpec.Headings, "", [], []) { Section = "Terms" },
            export: new ExportSpec
            {
                Version = 1,
                Fields = ["tier"],
                Sections = [("Scope", ExportSpec.Full)],
                Parts = ExportSpec.Full,
                PartsDeclared = true,
                Line = [("id", PartLineSource.PartId), ("title", PartLineSource.PartText)]
            })));

    // A type whose parts and export are otherwise sound, carrying the one line entry under test. Every
    // other key resolves, so the single finding a test asserts is the one it declared.
    private static TypeSchema Line((string Key, string Source) entry, PartSpec? parts = null)
        => Widgets(
            sections: ["Terms"],
            parts: parts ?? new PartSpec(PartSpec.Headings, "", [], []) { Section = "Terms" },
            export: new ExportSpec
            {
                Version = 1,
                Parts = ExportSpec.Full,
                PartsDeclared = true,
                Line = [entry]
            });

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
    // them in step.
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
