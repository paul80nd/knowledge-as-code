using kac.core;
using YamlDotNet.RepresentationModel;

// The types are invented, because a case naming a real one would move whenever that type did.

namespace kac.tests;

public class SchemaCheckTests
{
    /// <summary>
    /// The defaults are what a sound type carries, so that a case reports the one fault it declares and nothing else.
    /// </summary>
    private static TypeSchema Widgets(string idStyle = "slug", string folder = "widgets",
        (string Name, FieldSpec Spec)[]? fields = null, RuleSpec[]? rules = null,
        string[]? sections = null, string tier = "descriptive", string summary = "A widget.",
        string goesHere = "A widget", string labelPlural = "Widgets", string detail = "It is a widget.",
        (string Other, string Text)[]? versus = null, bool lineage = true, PartSpec? parts = null,
        ExportSpec? export = null, WidthSpec? width = null, bool carriesId = true) => new()
    {
        TypeName = "widget",
        Key = folder,
        Versus = versus ?? [],
        Lineage = lineage ? new LineageSpec("None.", "", "") : null,
        Folder = folder,
        IdStyle = idStyle,
        IdWidth = width ?? new WidthSpec(4, 4),
        FilenameCarriesId = carriesId,
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

    private static readonly string[] TierNames = ["decided", "normative", "descriptive", "procedural"];

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

    [Fact]
    public void A_check_a_rule_class_reports_under_and_the_schema_omits_is_reported()
    {
        var short1 = SoundChecks().Skip(1).ToList();

        var finding = Assert.Single(Check(WithTiers(checks: short1)));

        Assert.Equal("schema-dispatch", finding.Check.Value);
        Assert.Equal(".schema/_checks.yaml", finding.File);
        Assert.Contains(CheckCatalogue.EmittedByRules()[0].Value, finding.Message);
    }

    [Fact]
    public void A_folder_that_is_not_the_files_own_name_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(folder: "gadgets")));

        Assert.Equal("schema-shape", finding.Check.Value);
        Assert.Equal(".schema/widgets.yaml", finding.File);
        Assert.Contains("gadgets", finding.Message);
    }

    [Fact]
    public void A_rule_claiming_a_severity_that_nothing_dispatches_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(
            rules: [new RuleSpec { Id = new RuleId("widgets-are-blue"), Severity = Sev.Warning }])));

        Assert.Equal("schema-dispatch", finding.Check.Value);
        Assert.Equal(".schema/widgets.yaml", finding.File);
        Assert.Contains("widgets-are-blue", finding.Message);
    }

    [Fact]
    public void A_rule_with_no_severity_is_an_intention_and_passes()
        => Assert.Empty(Check(Widgets(
            rules: [new RuleSpec { Id = new RuleId("widgets-are-blue"), Description = "One day." }])));

    [Fact]
    public void A_config_key_on_an_expression_rule_is_reported_at_the_rules_line()
    {
        var finding = Assert.Single(Check(Widgets(rules:
        [
            new RuleSpec
            {
                Id = new RuleId("widgets-are-blue"), Severity = Sev.Warning, Expr = "present('colour')",
                Compiled = RuleExpr.Compile("present('colour')"), Message = "say what colour it is.",
                Config = Declaring("max-words", new YamlScalarNode("5")), Line = 12
            }
        ])));

        Assert.Equal("schema-dispatch", finding.Check.Value);
        Assert.Equal(".schema/widgets.yaml", finding.File);
        Assert.Equal(12, finding.Line);
        Assert.Contains("'max-words:'", finding.Message);
    }

    [Fact]
    public void A_config_key_the_rule_class_does_not_read_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(rules:
        [
            YStatement(Declaring("postures", new YamlSequenceNode(new YamlScalarNode("Obliged"))))
        ])));

        Assert.Equal("schema-dispatch", finding.Check.Value);
        Assert.Contains("'postures:'", finding.Message);
        Assert.Contains("'max-words'", finding.Message);
    }

    [Theory]
    [InlineData("five")]
    [InlineData("0")]
    [InlineData("-3")]
    [InlineData("2.5")]
    public void A_word_limit_that_is_not_a_whole_number_above_zero_is_reported(string declared)
    {
        var finding = Assert.Single(Check(Widgets(rules:
        [
            YStatement(Declaring("max-words", new YamlScalarNode(declared)))
        ])));

        Assert.Equal("schema-dispatch", finding.Check.Value);
        Assert.Equal(7, finding.Line);
        Assert.Contains($"'{declared}'", finding.Message);
    }

    [Fact]
    public void A_word_limit_written_as_a_list_is_reported_as_no_single_value()
        => Assert.Contains("is not a single value", Assert.Single(Check(Widgets(rules:
        [
            YStatement(Declaring("max-words", new YamlSequenceNode(new YamlScalarNode("60"))))
        ]))).Message);

    [Fact]
    public void A_word_limit_that_is_a_whole_number_above_zero_passes()
        => Assert.Empty(Check(Widgets(rules: [YStatement(Declaring("max-words", new YamlScalarNode("60")))])));

    private static Dictionary<string, YamlNode> Declaring(string key, YamlNode value) => new() { [key] = value };

    private static RuleSpec YStatement(Dictionary<string, YamlNode> config) => new()
    {
        Id = new RuleId("y-statement-present"), Severity = Sev.Warning, Config = config, Line = 7
    };

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
    public void A_ref_naming_a_type_this_corpus_declined_is_left_alone()
        => Assert.Empty(Check(Widgets(fields:
        [
            ("stored-in", new FieldSpec { Name = "stored-in", Type = "list", Of = "id", Refs = ["data"] })
        ])));

    [Fact]
    public void A_list_ref_naming_an_adopted_type_and_a_declined_one_is_left_alone()
        => Assert.Empty(Check(Widgets(fields:
        [
            ("departs-from",
                new FieldSpec { Name = "departs-from", Type = "list", Of = "id", Refs = ["widgets", "gadgets"] })
        ])));

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
            ("status", new FieldSpec
                { Name = "status", Type = "enum", Values = ["draft", "active"], InForce = "active" })
        ])));

    [Fact]
    public void An_in_force_value_the_range_does_not_include_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(fields:
        [
            ("status", new FieldSpec
                { Name = "status", Type = "enum", Values = ["draft", "active"], InForce = "live" })
        ])));

        Assert.Equal("schema-shape", finding.Check.Value);
        Assert.Contains("'in-force: live'", finding.Message);
        Assert.Contains("'active'", finding.Message);
    }

    [Fact]
    public void An_in_force_value_on_anything_but_an_enum_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(fields:
        [
            ("status", new FieldSpec { Name = "status", Type = "string", InForce = "active" })
        ])));

        Assert.Equal("schema-dispatch", finding.Check.Value);
        Assert.Contains("'status'", finding.Message);
    }

    [Fact]
    public void A_status_with_a_range_and_no_in_force_value_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(fields:
        [
            ("status", new FieldSpec { Name = "status", Type = "enum", Values = ["draft", "active"] })
        ])));

        Assert.Equal("schema-shape", finding.Check.Value);
        Assert.Contains("'in-force:'", finding.Message);
    }

    [Fact]
    public void A_status_stating_no_range_is_asked_for_no_in_force_value()
        => Assert.Empty(Check(Widgets(fields:
        [
            ("status", new FieldSpec { Name = "status", Type = "enum", Values = [] })
        ])));

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

    [Fact]
    public void A_mirrors_citations_on_a_field_with_no_ref_is_reported()
    {
        var findings = Check(Widgets(fields:
        [
            ("implements", new FieldSpec
                { Name = "implements", Type = "list", Of = "id", MirrorsCitations = "Covers" })
        ]));

        // The entries have no `ref:` to resolve against either, and each key answers for itself.
        Assert.Equal(2, findings.Count);
        var finding = Assert.Single(findings, x => x.Message.Contains("mirrors-citations: Covers"));
        Assert.Equal("schema-shape", finding.Check.Value);
        Assert.Contains("Covers", finding.Message);
    }

    [Fact]
    public void A_rule_description_longer_than_the_table_allows_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(rules:
        [
            new RuleSpec
                { Id = new RuleId("very-wordy"), Description = new string('x', ChecksTable.DescriptionMax + 1) }
        ])));

        Assert.Equal("schema-shape", finding.Check.Value);
        Assert.Contains($"the limit is {ChecksTable.DescriptionMax}", finding.Message);
    }

    [Fact]
    public void The_bound_holds_for_a_rule_that_is_dispatched_too()
    {
        var implemented = DocumentRules.All.First().RuleId;
        var finding = Assert.Single(Check(Widgets(rules:
        [
            new RuleSpec { Id = implemented, Description = new string('x', ChecksTable.DescriptionMax + 1) }
        ])));

        Assert.Equal("schema-shape", finding.Check.Value);
    }

    [Fact]
    public void A_description_at_the_bound_passes()
        => Assert.Empty(Check(Widgets(rules:
        [
            new RuleSpec { Id = new RuleId("just-fits"), Description = new string('x', ChecksTable.DescriptionMax) }
        ])));

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

    [Fact]
    public void A_span_of_widths_the_wrong_way_round_is_reported()
    {
        var finding = Assert.Single(Check(
            Widgets(idStyle: "mnemonic", width: new WidthSpec(7, 2), carriesId: false)));

        Assert.Equal("schema-shape", finding.Check.Value);
        Assert.Contains("above its 'max:' of 2", finding.Message);
    }

    [Fact]
    public void A_span_of_widths_on_a_numbered_type_is_reported()
    {
        var finding = Assert.Single(Check(
            Widgets(idStyle: "numbered", width: new WidthSpec(2, 7), carriesId: false)));

        Assert.Equal("schema-shape", finding.Check.Value);
        Assert.Contains("padded to one width", finding.Message);
    }

    [Fact]
    public void A_span_of_widths_beside_a_filename_that_carries_the_id_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(idStyle: "mnemonic", width: new WidthSpec(2, 7))));

        Assert.Equal("schema-shape", finding.Check.Value);
        Assert.Contains("span of 2 to 7", finding.Message);
    }

    [Fact]
    public void A_span_of_widths_passes_where_the_filename_carries_no_id()
        => Assert.Empty(Check(Widgets(idStyle: "mnemonic", width: new WidthSpec(2, 7), carriesId: false)));

    [Fact]
    public void A_slug_type_that_withholds_its_id_from_the_filename_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(carriesId: false)));

        Assert.Equal("schema-shape", finding.Check.Value);
        Assert.Contains("nothing left for the id to be", finding.Message);
    }

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
    public void Every_declared_tier_passes(string tier)
        => Assert.Empty(Check(Widgets(tier: tier)));

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
        var tiers = schema.Tiers.Select(t => t.Name == "procedural" ? t with { Label = "" } : t).ToList();

        var finding = Assert.Single(Check(new Schema
        {
            Tiers = tiers, UniversalOrder = schema.UniversalOrder, Universal = schema.Universal,
            Checks = schema.Checks
        }));

        Assert.Contains("declares no 'label:'", finding.Message);
    }

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

    [Fact]
    public void A_summary_too_long_for_the_cell_it_becomes_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(summary: new string('x', ChecksTable.DescriptionMax + 1))));

        Assert.Equal("schema-shape", finding.Check.Value);
        Assert.Contains($"the limit is {ChecksTable.DescriptionMax}", finding.Message);
    }

    [Fact]
    public void A_summary_at_the_bound_passes()
        => Assert.Empty(Check(Widgets(summary: new string('x', ChecksTable.DescriptionMax))));

    [Fact]
    public void A_type_with_no_paragraph_beneath_its_one_liner_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(detail: "")));

        Assert.Equal("schema-shape", finding.Check.Value);
        Assert.Contains("no 'detail:'", finding.Message);
    }

    [Fact]
    public void A_detail_longer_than_a_table_cell_passes()
        => Assert.Empty(Check(Widgets(detail: new string('x', ChecksTable.DescriptionMax * 3))));

    [Fact]
    public void A_type_that_names_no_prior_art_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(lineage: false)));

        Assert.Equal("schema-shape", finding.Check.Value);
        Assert.Contains("no 'lineage.prior-art:'", finding.Message);
    }

    [Fact]
    public void A_prior_art_of_none_with_nothing_beside_it_passes()
        => Assert.Empty(Check(Widgets()));

    [Fact]
    public void A_part_source_nothing_extracts_names_the_ones_that_are_read()
    {
        var parts = new PartSpec("footnotes", "", [], []) { Section = "Terms" };
        var found = Assert.Single(Check(Widgets(sections: ["Terms"], parts: parts)));

        Assert.Equal("schema-dispatch", found.Check.Value);
        Assert.Contains("'parts.source: footnotes', which nothing extracts", found.Message);
        Assert.Contains("'headings', 'table'", found.Message);
    }

    [Fact]
    public void A_part_section_the_type_does_not_declare_is_reported()
    {
        var parts = new PartSpec(PartSpec.Headings, "", [], []) { Section = "Glossary" };
        var found = Assert.Single(Check(Widgets(sections: ["Terms"], parts: parts)));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("'parts.section: Glossary'", found.Message);
    }

    [Fact]
    public void A_table_source_declaring_no_binding_modals_is_reported()
    {
        var parts = new PartSpec(PartSpec.Table, "^[A-Z]+$", [], ["SHOULD"]) { Section = "Clauses" };
        var found = Assert.Single(Check(Widgets(sections: ["Clauses"], parts: parts)));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("declares no 'binding:'", found.Message);
    }

    [Fact]
    public void A_heading_source_is_not_asked_for_modals()
        => Assert.Empty(Check(Widgets(sections: ["Terms"],
            parts: new PartSpec(PartSpec.Headings, "", [], []) { Section = "Terms" })));

    [Fact]
    public void A_type_declaring_no_parts_is_asked_nothing_about_them()
        => Assert.Empty(Check(Widgets()));

    [Fact]
    public void An_exported_section_the_type_does_not_declare_is_reported()
    {
        var export = new ExportSpec { Version = 1, Sections = [("Provenance", ExportSpec.Full)] };
        var found = Assert.Single(Check(Widgets(sections: ["Scope"], export: export)));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("'export.sections: Provenance'", found.Message);
    }

    [Fact]
    public void An_exported_fidelity_nothing_carries_names_the_ones_that_are_written()
    {
        var export = new ExportSpec { Version = 1, Sections = [("Scope", "gist")] };
        var found = Assert.Single(Check(Widgets(sections: ["Scope"], export: export)));

        Assert.Equal("schema-dispatch", found.Check.Value);
        Assert.Contains("at fidelity 'gist', which nothing carries there", found.Message);
        Assert.Contains("'full', 'reference', 'summary'", found.Message);
    }

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

    [Fact]
    public void An_exported_field_the_type_inherits_passes()
        => Assert.Empty(Check(Widgets(export: new ExportSpec { Version = 1, Fields = ["tier"] })));

    [Fact]
    public void An_export_block_declaring_no_shape_version_is_reported()
    {
        var found = Assert.Single(Check(Widgets(export: new ExportSpec())));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("no 'version:' above 0", found.Message);
    }

    [Fact]
    public void Exported_parts_on_a_type_that_locates_none_are_reported()
    {
        var export = new ExportSpec { Version = 1, Parts = ExportSpec.Full, PartsDeclared = true };
        var found = Assert.Single(Check(Widgets(export: export)));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("carries no 'parts:' block", found.Message);
    }

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

    [Fact]
    public void A_line_source_naming_a_list_field_is_reported()
    {
        var found = Assert.Single(Check(Line(("shelves", "front.shelves"),
            fields: [("shelves", new FieldSpec { Name = "shelves", Type = "list", Of = "string" })])));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("at 'front.shelves', and that field is not a single value", found.Message);
    }

    [Fact]
    public void A_line_source_naming_a_list_of_objects_is_reported()
    {
        var found = Assert.Single(Check(Line(("signoffs", "front.signoffs"), fields:
        [
            ("signoffs", new FieldSpec
            {
                Name = "signoffs", Type = "list", Of = "object",
                Entry = [new FieldSpec { Name = "on", Type = "date" }]
            })
        ])));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("at 'front.signoffs', and that field is not a single value", found.Message);
    }

    [Fact]
    public void A_line_source_naming_an_object_field_is_reported()
    {
        var found = Assert.Single(Check(Line(("signoff", "front.signoff"), fields:
        [
            ("signoff", new FieldSpec
            {
                Name = "signoff", Type = "object", Entry = [new FieldSpec { Name = "on", Type = "date" }]
            })
        ])));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("at 'front.signoff', and that field is not a single value", found.Message);
    }

    [Fact]
    public void A_line_source_naming_a_universal_list_field_is_reported()
    {
        var schema = WithTiers(Line(("tags", "front.tags")));
        var universal = new Dictionary<string, FieldSpec>(schema.Universal)
        {
            ["tags"] = new() { Name = "tags", Type = "list", Of = "string" }
        };
        var found = Assert.Single(Check(new Schema
        {
            Checks = schema.Checks, Tiers = schema.Tiers, UniversalOrder = ["tier", "tags"], Universal = universal,
            ByFolder = schema.ByFolder
        }));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("at 'front.tags', and that field is not a single value", found.Message);
    }

    [Fact]
    public void A_line_source_naming_a_scalar_field_passes()
        => Assert.Empty(Check(Line(("shelf", "front.shelf"),
            fields: [("shelf", new FieldSpec { Name = "shelf", Type = "string" })])));

    [Fact]
    public void A_line_source_naming_the_title_as_a_field_is_reported()
        => Assert.Contains("'front.title'", Assert.Single(Check(Line(("title", "front.title")))).Message);

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

    /// <summary>
    /// Asked of the lead here and of an aside below, because each reaches the refusal by its own arm: the lead through
    /// the fixed vocabulary, an aside through the prefixed one.
    /// </summary>
    [Fact]
    public void A_line_source_reading_a_lead_against_a_table_is_reported()
    {
        var found = Assert.Single(Check(Line(
            ("definition", "part.lead"),
            parts: new PartSpec(PartSpec.Table, "", ["MUST"], []) { Section = "Terms" })));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("A row is its own body", found.Message);
    }

    [Fact]
    public void A_line_source_reading_an_aside_against_a_table_is_reported()
    {
        var found = Assert.Single(Check(Line(
            ("not", "part.aside.Not"),
            parts: new PartSpec(PartSpec.Table, "", ["MUST"], []) { Section = "Terms", Asides = ["Not"] })));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("A row is its own body", found.Message);
    }

    [Fact]
    public void A_line_source_naming_an_undeclared_aside_is_reported()
    {
        var found = Assert.Single(Check(Line(("avoid", "part.aside.Avoid"))));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("declares no such label", found.Message);
    }

    [Fact]
    public void A_line_source_naming_a_declared_aside_passes()
        => Assert.Empty(Check(Line(
            ("not", "part.aside.Not"),
            parts: new PartSpec(PartSpec.Headings, "", [], []) { Section = "Terms", Asides = ["Not"] })));

    [Fact]
    public void A_line_source_taking_a_level_from_a_type_declaring_none_is_reported()
    {
        var found = Assert.Single(Check(Line(("level", "part.level"))));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("no binding or advisory levels", found.Message);
    }

    [Fact]
    public void A_line_source_taking_bullets_from_a_type_declaring_no_levels_is_reported()
    {
        var found = Assert.Single(Check(Line(("obligations", "part.bullets"))));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("no binding or advisory levels", found.Message);
    }

    [Fact]
    public void A_line_source_reading_bullets_against_a_table_is_reported()
    {
        var found = Assert.Single(Check(Line(
            ("obligations", "part.bullets"),
            parts: new PartSpec(PartSpec.Table, "", ["MUST"], []) { Section = "Terms" })));

        Assert.Equal("schema-shape", found.Check.Value);
        Assert.Contains("A row is its own body", found.Message);
    }

    [Fact]
    public void A_line_source_taking_bullets_from_a_type_declaring_levels_passes()
        => Assert.Empty(Check(Line(
            ("obligations", "part.bullets"),
            parts: new PartSpec(PartSpec.Headings, "", ["MUST"], ["SHOULD"]) { Section = "Terms" })));

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

    /// <summary>
    /// A type whose parts and export are otherwise sound, carrying the one line entry under test. Every other key
    /// resolves, so the single finding a test asserts is the one it declared.
    /// </summary>
    private static TypeSchema Line((string Key, string Source) entry, PartSpec? parts = null,
        (string Name, FieldSpec Spec)[]? fields = null)
        => Widgets(
            fields: fields,
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
    public void A_versus_against_a_type_this_corpus_declined_is_left_alone()
        => Assert.Empty(Check(TwoTypes(("widgets", [("gizmos", "Not a type here.")]))));

    [Fact]
    public void A_versus_against_itself_is_reported()
    {
        var finding = Assert.Single(Check(TwoTypes(("widgets", [("widgets", "Widget vs Widget.")]))));

        Assert.Equal("schema-shape", finding.Check.Value);
        Assert.Contains("against itself", finding.Message);
    }

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

    [Fact]
    public void A_field_deriving_from_a_source_nothing_resolves_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(
            fields: [("shelf", new FieldSpec { Name = "shelf", From = "the-filename" })])));

        Assert.Equal("schema-dispatch", finding.Check.Value);
        Assert.Equal(".schema/widgets.yaml", finding.File);
        Assert.Contains("from: the-filename", finding.Message);
        Assert.Contains("sub-path", finding.Message);
    }

    [Fact]
    public void A_field_deriving_from_a_source_the_tool_resolves_is_left_alone()
        => Assert.Empty(Check(Widgets(
            fields: [("shelf", new FieldSpec { Name = "shelf", From = "sub-path" })])));

    [Fact]
    public void A_derived_field_that_is_also_required_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(
            fields: [("shelf", new FieldSpec { Name = "shelf", From = "sub-path", Required = true })])));

        Assert.Equal("schema-shape", finding.Check.Value);
        Assert.Equal(".schema/widgets.yaml", finding.File);
        Assert.Contains("'shelf'", finding.Message);
    }

    [Fact]
    public void A_field_whose_type_no_check_reads_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(
            fields: [("stamped", new FieldSpec { Name = "stamped", Type = "tiemstamp" })])));

        Assert.Equal("schema-dispatch", finding.Check.Value);
        Assert.Equal(".schema/widgets.yaml", finding.File);
        Assert.Contains("type: tiemstamp", finding.Message);
        Assert.Contains("'timestamp'", finding.Message);
    }

    [Fact]
    public void A_list_whose_entries_no_check_reads_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(
            fields: [("due", new FieldSpec { Name = "due", Type = "list", Of = "date" })])));

        Assert.Equal("schema-dispatch", finding.Check.Value);
        Assert.Contains("of: date", finding.Message);
    }

    [Fact]
    public void An_entry_key_whose_type_no_check_reads_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(fields:
        [
            ("verified", new FieldSpec
            {
                Name = "verified", Type = "list", Of = "object",
                Entry = [new FieldSpec { Name = "at", Type = "moment" }]
            })
        ])));

        Assert.Equal("schema-dispatch", finding.Check.Value);
        Assert.Contains("entry key 'at'", finding.Message);
        Assert.Contains("type: moment", finding.Message);
    }

    [Fact]
    public void An_entry_key_nested_two_levels_in_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(fields:
        [
            ("approvals", new FieldSpec
            {
                Name = "approvals", Type = "list", Of = "object",
                Entry =
                [
                    new FieldSpec
                    {
                        Name = "signoffs", Type = "list", Of = "object",
                        Entry = [new FieldSpec { Name = "at", Type = "moment" }]
                    }
                ]
            })
        ])));

        Assert.Equal("schema-dispatch", finding.Check.Value);
        Assert.Contains("entry key 'signoffs' entry key 'at'", finding.Message);
    }

    [Fact]
    public void An_of_on_a_field_that_is_not_a_list_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(
            fields: [("shelf", new FieldSpec { Name = "shelf", Type = "string", Of = "bool" })])));

        Assert.Equal("schema-dispatch", finding.Check.Value);
        Assert.Contains("is 'type: string' and declares 'of: bool'", finding.Message);
        Assert.DoesNotContain("A list's entries are", finding.Message);
    }

    [Theory]
    [InlineData("date")]
    [InlineData("enum")]
    [InlineData("list")]
    [InlineData("string")]
    [InlineData("timestamp")]
    public void A_field_whose_type_the_tool_dispatches_is_left_alone(string type)
        => Assert.Empty(Check(Widgets(
            fields: [("value", new FieldSpec { Name = "value", Type = type })])));

    [Theory]
    [InlineData("id", null)]
    [InlineData("list", "id")]
    public void A_field_declaring_an_id_and_the_folders_it_points_into_is_left_alone(string type, string? of)
        => Assert.Empty(Check(Widgets(
            fields: [("value", new FieldSpec { Name = "value", Type = type, Of = of, Refs = ["widgets"] })])));

    [Theory]
    [InlineData("id", null, "type: id")]
    [InlineData("list", "id", "of: id")]
    public void A_field_declaring_an_id_and_no_reference_is_reported(string type, string? of, string declared)
    {
        var finding = Assert.Single(Check(Widgets(
            fields: [("sponsor", new FieldSpec { Name = "sponsor", Type = type, Of = of })])));

        Assert.Equal("schema-dispatch", finding.Check.Value);
        Assert.Contains($"field 'sponsor' declares '{declared}' and no 'ref:'", finding.Message);
    }

    /// <summary>A sound list of objects that declares <c>key</c> as well.</summary>
    private static FieldSpec Signoffs(string key) => new()
    {
        Name = "signoffs", Type = "list", Of = "object", Entry = [new FieldSpec { Name = "on", Type = "date" }],
        Refs = key == "ref" ? ["widgets"] : [],
        MinRecords = key == "min-records" ? 2 : null,
        MirrorsSection = key == "mirrors-section" ? "Sign-offs" : null,
        Reciprocal = key == "reciprocal" ? "signed" : null,
        MirrorsCitations = key == "mirrors-citations" ? "Signed off" : null
    };

    /// <summary><c>mirrors-citations:</c> with no <c>ref:</c> is a second finding, which this case leaves aside.</summary>
    [Theory]
    [InlineData("ref")]
    [InlineData("min-records")]
    [InlineData("mirrors-section")]
    [InlineData("reciprocal")]
    [InlineData("mirrors-citations")]
    public void A_key_read_from_scalar_entries_on_a_list_of_objects_is_reported(string key)
    {
        var finding = Assert.Single(Check(Widgets(sections: ["Sign-offs"], fields: [("signoffs", Signoffs(key))])),
            x => x.Message.Contains("declares 'of: object'"));

        Assert.Equal("schema-shape", finding.Check.Value);
        Assert.Equal(".schema/widgets.yaml", finding.File);
        Assert.Contains($"field 'signoffs' declares 'of: object' and '{key}:'", finding.Message);
    }

    [Fact]
    public void A_key_read_from_scalar_entries_on_a_universal_list_of_objects_is_reported()
    {
        var schema = WithTiers();
        var universal = new Dictionary<string, FieldSpec>(schema.Universal) { ["signoffs"] = Signoffs("ref") };
        var finding = Assert.Single(Check(new Schema
        {
            Checks = schema.Checks, Tiers = schema.Tiers, UniversalOrder = ["tier", "signoffs"],
            Universal = universal, ByFolder = schema.ByFolder
        }));

        Assert.Equal("schema-shape", finding.Check.Value);
        Assert.Equal(".schema/_universal.yaml", finding.File);
        Assert.Contains("field 'signoffs' declares 'of: object' and 'ref:'", finding.Message);
    }

    [Fact]
    public void A_ref_on_a_list_of_ids_passes()
        => Assert.Empty(Check(Widgets(fields:
        [
            ("depends-on", new FieldSpec { Name = "depends-on", Type = "list", Of = "id", Refs = ["widgets"] })
        ])));

    [Theory]
    [InlineData(null)]
    [InlineData("widgets")]
    public void An_entry_key_declaring_an_id_is_reported(string? re)
    {
        var finding = Assert.Single(Check(Widgets(fields:
        [
            ("signoffs", new FieldSpec
            {
                Name = "signoffs", Type = "list", Of = "object",
                Entry = [new FieldSpec { Name = "by", Type = "id", Refs = re is null ? [] : [re] }]
            })
        ])));

        Assert.Equal("schema-dispatch", finding.Check.Value);
        Assert.Contains("entry key 'by' declares 'type: id', and the reference pass reads", finding.Message);
        Assert.DoesNotContain("Declare a 'ref:'", finding.Message);
    }

    [Fact]
    public void A_field_requiring_a_part_and_naming_no_reference_is_reported()
    {
        var findings = Check(Widgets(
            fields: [("implements", new FieldSpec { Name = "implements", Type = "id", PartRequired = true })]));

        // The id has no `ref:` to resolve against either, and each key answers for itself.
        Assert.Equal(2, findings.Count);
        var finding = Assert.Single(findings, x => x.Message.Contains("part-required: true"));
        Assert.Equal("schema-dispatch", finding.Check.Value);
    }

    [Fact]
    public void A_field_requiring_a_part_of_a_type_that_keeps_none_is_reported()
    {
        var finding = Assert.Single(Check(Widgets(fields:
        [
            ("implements",
                new FieldSpec { Name = "implements", Type = "id", Refs = ["widgets"], PartRequired = true })
        ])));

        Assert.Equal("schema-shape", finding.Check.Value);
        Assert.Contains("keeps no parts", finding.Message);
    }

    [Fact]
    public void A_field_requiring_a_part_of_a_type_that_keeps_them_passes()
        => Assert.Empty(Check(Widgets(
            fields:
            [
                ("implements",
                    new FieldSpec { Name = "implements", Type = "id", Refs = ["widgets"], PartRequired = true })
            ],
            sections: ["Terms"],
            parts: new PartSpec(PartSpec.Headings, "", [], []) { Section = "Terms" })));

    [Fact]
    public void A_field_requiring_a_part_of_a_type_this_corpus_declined_is_left_alone()
        => Assert.Empty(Check(Widgets(fields:
        [
            ("implements",
                new FieldSpec { Name = "implements", Type = "id", Refs = ["gadgets"], PartRequired = true })
        ])));
}
