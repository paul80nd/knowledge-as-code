// Unit tests for the pure Generator helpers. The full INDEX/<type>.md generation is covered by the
// golden 'index' scenario; these pin the table/catalogue consistency and the splice in-process.

using kac.core;

namespace kac.tests;

public class GeneratorTests
{
    [Fact]
    public void ChecksTableProblems_is_empty_for_the_shipped_catalogue()
    {
        // The reader-facing table must stay a faithful, complete view of the catalogue. `kac checks`
        // enforces this out-of-process for CI; this is the same invariant, unit-testable in-process.
        Assert.Empty(Generator.ChecksTableProblems());
    }

    [Fact]
    public void IndexPage_says_it_is_empty_rather_than_rendering_a_headless_table()
    {
        var t = new TypeSchema { Label = "Control", IdPrefix = "ctl", IndexColumns = ["id", "title"] };

        var page = Generator.IndexPage(t, []);

        Assert.Contains("# Control Index (CTL)", page);
        Assert.Contains("Nothing here yet", page);
        Assert.Contains("template.md", page);   // an empty index points at the way to fill it
        Assert.DoesNotContain("| Id |", page);  // …rather than a table with headers and no rows
    }

    [Fact]
    public void IndexPage_heading_drops_the_prefix_when_it_only_repeats_the_label()
    {
        var t = new TypeSchema { Label = "ADR", IdPrefix = "adr", IndexColumns = ["id", "title"] };

        Assert.Contains("# ADR Index\n", Generator.IndexPage(t, []));
    }

    [Fact]
    public void SchemaTable_prefers_description_and_falls_back_to_notes()
    {
        var t = new TypeSchema
        {
            FieldOrder = ["both", "notes-only", "neither"],
            Fields = new Dictionary<string, FieldSpec>
            {
                ["both"] = new() { Name = "both", Description = "SHORT", Notes = "LONG" },
                ["notes-only"] = new() { Name = "notes-only", Notes = "FALLBACK" },
                ["neither"] = new() { Name = "neither" }
            }
        };

        var table = Generator.SchemaTable(t, new Schema());

        Assert.Contains("SHORT", table);
        Assert.DoesNotContain("LONG", table); // description wins outright — the two are not concatenated
        Assert.Contains("FALLBACK", table);   // notes still render where no description exists
    }

    [Fact]
    public void SchemaTable_leads_with_marked_universal_fields_and_shows_the_type_refinement()
    {
        var s = new Schema
        {
            UniversalOrder = ["id", "status"],
            Universal = new Dictionary<string, FieldSpec>
            {
                ["id"] = new() { Name = "id", Required = true, Description = "UNIVERSAL-ID" },
                ["status"] = new() { Name = "status", Type = "enum", Description = "VARIES BY TYPE" }
            }
        };
        var t = new TypeSchema
        {
            FieldOrder = ["status", "own-field"],
            Fields = new Dictionary<string, FieldSpec>
            {
                // The type narrows the universal status; the table must show the narrowing.
                ["status"] = new() { Name = "status", Type = "enum", Values = ["draft"], Description = "REFINED" },
                ["own-field"] = new() { Name = "own-field", Description = "TYPE-ONLY" }
            }
        };

        var table = Generator.SchemaTable(t, s);

        Assert.Contains("`id` †", table);         // universal fields are marked…
        Assert.Contains("`status` †", table);     // …including one the type redeclares
        Assert.Contains("`own-field`", table);
        Assert.DoesNotContain("`own-field` †", table);
        Assert.Contains("REFINED", table);        // EffectiveField wins over the universal declaration
        Assert.DoesNotContain("VARIES BY TYPE", table);
        Assert.True(table.IndexOf("`id`", StringComparison.Ordinal)
                    < table.IndexOf("`own-field`", StringComparison.Ordinal)); // universal first
        Assert.Contains("† Carried by every document", table);
    }

    [Fact]
    public void SchemaTable_lists_enum_values_beneath_the_table_not_inside_the_cell()
    {
        var t = new TypeSchema
        {
            FieldOrder = ["status"],
            Fields = new Dictionary<string, FieldSpec>
            {
                ["status"] = new()
                {
                    Name = "status", Type = "enum", Values = ["draft", "active"], Description = "SHORT PROSE"
                }
            }
        };

        var table = Generator.SchemaTable(t, new Schema());
        var main = table.Split("**Enum values**", StringSplitOptions.None)[0];
        var row = main.Split('\n').Single(l => l.StartsWith("| `status`", StringComparison.Ordinal));

        // The values are the thing that used to blow the column width out — they belong below, in a
        // table of their own so the page still reads as formatted code.
        Assert.DoesNotContain("`draft`", row);
        Assert.Contains("SHORT PROSE", row);
        Assert.Contains("**Enum values**", table);
        Assert.Contains("| `status` | `draft` · `active` |", table);
    }

    [Fact]
    public void SchemaTable_lists_required_when_conditions_beneath_the_table()
    {
        var t = new TypeSchema
        {
            FieldOrder = ["retention"],
            Fields = new Dictionary<string, FieldSpec>
            {
                ["retention"] = new()
                {
                    Name = "retention", Description = "SHORT PROSE",
                    RequiredWhen = "classification in [personal, special-category]"
                }
            }
        };

        var table = Generator.SchemaTable(t, new Schema());
        var main = table.Split("**Conditionally required**", StringSplitOptions.None)[0];

        // The condition has to be quoted exactly, so it is the half that cannot be trimmed to fit.
        Assert.DoesNotContain("classification in", main);
        Assert.Contains("SHORT PROSE", main);
        Assert.Contains("| `retention` | `classification in [personal, special-category]` |", table);
    }

    [Fact]
    public void SchemaTable_omits_the_legend_when_no_universal_field_applies()
    {
        var t = new TypeSchema
        {
            FieldOrder = ["own-field"],
            Fields = new Dictionary<string, FieldSpec> { ["own-field"] = new() { Name = "own-field" } }
        };

        Assert.DoesNotContain("†", Generator.SchemaTable(t, new Schema()));
    }

    [Fact]
    public void ChecksTable_omits_rows_for_checks_the_type_cannot_trip()
    {
        // A type declaring no rules and no reciprocal/mirrors-section field: the schema-conditional
        // rows must not appear. Before this was conditional, every page carried the ADR-shaped table
        // and told (say) a policy reader their documents are checked for Y-statements.
        var table = Generator.ChecksTable(new TypeSchema());

        Assert.DoesNotContain("y-statement", table);
        Assert.DoesNotContain("alternatives-verdict", table);
        Assert.DoesNotContain("related-matches-section", table);
        Assert.DoesNotContain("reciprocal", table);
        Assert.Contains("frontmatter-parses", table); // unconditional rows still render
    }

    [Fact]
    public void ChecksTable_includes_rows_the_type_opts_into_through_its_schema()
    {
        var t = new TypeSchema
        {
            Fields = new Dictionary<string, FieldSpec>
            {
                ["supersedes"] = new() { Name = "supersedes", Reciprocal = "superseded-by" },
                ["related"] = new() { Name = "related", MirrorsSection = "Related" }
            },
            Rules =
            [
                new RuleSpec { Id = "y-statement-present" },
                new RuleSpec { Id = "alternatives-have-verdicts" }
            ]
        };

        var table = Generator.ChecksTable(t);

        Assert.Contains("y-statement", table);
        Assert.Contains("alternatives-verdict", table);
        Assert.Contains("related-matches-section", table);
        Assert.Contains("reciprocal", table);
    }

    [Fact]
    public void SpliceBlock_replaces_only_between_the_named_markers()
    {
        const string text = "before\n<!-- BEGIN GENERATED: x -->\nOLD\n<!-- END GENERATED: x -->\nafter";

        var result = Generator.SpliceBlock(text, "x", "NEW");

        Assert.Contains("<!-- BEGIN GENERATED: x -->\n\nNEW\n\n<!-- END GENERATED: x -->", result);
        Assert.DoesNotContain("OLD", result);
        Assert.StartsWith("before\n", result);
        Assert.EndsWith("\nafter", result);
    }

    [Fact]
    public void SpliceBlock_leaves_text_untouched_when_the_marker_is_absent()
    {
        const string text = "no markers here";
        Assert.Equal(text, Generator.SpliceBlock(text, "missing", "NEW"));
    }
}
