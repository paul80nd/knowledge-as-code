// Unit tests for the pure Generator helpers. The full index / <type>.md generation is covered by the
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
        Assert.Contains("_template.md", page);   // an empty index points at the way to fill it
        Assert.DoesNotContain("| Id |", page);  // …rather than a table with headers and no rows
    }

    [Fact]
    public void IndexPage_heading_drops_the_prefix_when_it_only_repeats_the_label()
    {
        var t = new TypeSchema { Label = "ADR", IdPrefix = "adr", IndexColumns = ["id", "title"] };

        Assert.Contains("# ADR Index\n", Generator.IndexPage(t, []));
    }

    // The three sorts a type can declare, over one set of rows: no `sort:` at all, several columns, and
    // a direction. Each is a value the loader reads and the golden 'index' scenario cannot reach, because
    // the fixture corpora it runs over hold one type whose index sorts by id.
    private static List<Doc> Rows(params (string Id, string Severity)[] rows) =>
        [.. rows.Select(r => Doc.Parse($"runbooks/{r.Id}.md",
            $"---\nid: {r.Id}\nseverity: {r.Severity}\n---\n\n# {r.Id}\n", new Schema())!)];

    [Fact]
    public void IndexPage_sorts_by_id_where_the_type_declares_no_sort()
    {
        var t = new TypeSchema { Label = "Runbook", IdPrefix = "rbk", IndexColumns = ["id"] };

        var page = Generator.IndexPage(t, Rows(("rbk-b", "sev1"), ("rbk-a", "sev3")));

        Assert.True(page.IndexOf("rbk-a", StringComparison.Ordinal)
                    < page.IndexOf("rbk-b", StringComparison.Ordinal));
    }

    [Fact]
    public void IndexPage_sorts_on_each_declared_column_in_turn()
    {
        var t = new TypeSchema
        {
            Label = "Runbook", IdPrefix = "rbk", IndexColumns = ["id", "severity"], IndexSort = ["severity", "id"]
        };

        var page = Generator.IndexPage(t, Rows(("rbk-a", "sev3"), ("rbk-c", "sev1"), ("rbk-b", "sev1")));

        // severity first, then id within it — so the sev1 pair leads, in id order, and sev3 follows.
        Assert.True(page.IndexOf("rbk-b", StringComparison.Ordinal)
                    < page.IndexOf("rbk-c", StringComparison.Ordinal));
        Assert.True(page.IndexOf("rbk-c", StringComparison.Ordinal)
                    < page.IndexOf("rbk-a", StringComparison.Ordinal));
    }

    [Fact]
    public void IndexPage_reverses_the_whole_ordering_where_the_type_asks_for_descending()
    {
        var t = new TypeSchema
        {
            Label = "Runbook", IdPrefix = "rbk", IndexColumns = ["id", "severity"],
            IndexSort = ["severity", "id"], IndexOrder = Generator.Descending
        };

        var page = Generator.IndexPage(t, Rows(("rbk-a", "sev3"), ("rbk-c", "sev1"), ("rbk-b", "sev1")));

        // The direction applies to the sort as a whole, tie-breaker included.
        Assert.True(page.IndexOf("rbk-a", StringComparison.Ordinal)
                    < page.IndexOf("rbk-c", StringComparison.Ordinal));
        Assert.True(page.IndexOf("rbk-c", StringComparison.Ordinal)
                    < page.IndexOf("rbk-b", StringComparison.Ordinal));
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

        // Values are what blows a column's width out, so they belong below in a table of their own,
        // where the page still reads as formatted code.
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
        // rows must not appear. Unconditional rows would tell a policy reader their documents are
        // checked for Y-statements, which is the ADR-shaped table advertised on every page.
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

    // -- the tables of types --

    private static TypeSchema Type(string label, string plural, string tier, string page, string goesHere) => new()
    {
        Label = label, LabelPlural = plural, Tier = tier, Page = page, GoesHere = goesHere,
        Summary = $"What a {label.ToLowerInvariant()} holds."
    };

    private static TypeSchema[] Four() =>
    [
        Type("Runbook", "Runbooks", "procedural", "runbooks.md", "A step-by-step for when something is broken"),
        Type("ADR", "ADRs", "decided", "adrs.md", "A decision that affects more than one repo"),
        Type("Standard", "Standards", "normative", "standards.md", "A rule people must follow when building"),
        Type("Control", "Controls", "normative", "controls.md", "A check that proves a rule is being followed")
    ];

    private static List<int> PositionsOf(string text, params string[] needles) =>
        [.. needles.Select(n => text.IndexOf(n, StringComparison.Ordinal))];

    // The decision table is read down its left column, so that is the column it is ordered by.
    [Fact]
    public void The_decision_table_is_sorted_by_what_the_reader_is_holding()
    {
        var order = PositionsOf(Generator.PlacementTable(Four()),
            "A check that", "A decision that", "A rule people", "A step-by-step");

        Assert.Equal(order.Order(), order);
    }

    // It points at the collection, so it names the collection.
    [Fact]
    public void The_decision_table_names_a_type_in_the_plural()
    {
        var rows = Generator.PlacementTable(Four());

        Assert.Contains("[ADRs](/adrs)", rows);      // the link form ADO renders and LinkChecks resolves
        Assert.DoesNotContain("[ADR](", rows);
    }

    // A lookup table, ordered by the one thing the reader already knows. Tier is a column, not a grouping.
    [Fact]
    public void The_corpus_index_is_sorted_by_type_name()
    {
        var order = PositionsOf(Generator.TypesIndex(Four(), "taxonomy.md"),
            "[ADR]", "[Control]", "[Runbook]", "[Standard]");

        Assert.Equal(order.Order(), order);
    }

    [Fact]
    public void The_corpus_index_carries_the_way_on_to_the_taxonomy_inside_the_block()
    {
        var index = Generator.TypesIndex(Four(), "knowledge-as-code/taxonomy.md");

        Assert.Contains("[taxonomy](knowledge-as-code/taxonomy.md)", index);
        Assert.True(index.IndexOf("| [ADR]", StringComparison.Ordinal)
                    < index.IndexOf("[taxonomy]", StringComparison.Ordinal));
        Assert.All(index.Split('\n').Where(l => !l.StartsWith('|')), l => Assert.True(l.Length <= 120));
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

    // Authored is what `mechanism --check` compares, so what it keeps and what it drops decides whether a
    // shared page may carry a corpus-specific table.
    [Fact]
    public void Authored_drops_what_a_generated_block_holds_and_keeps_the_markers()
    {
        const string local = "prose\n<!-- BEGIN GENERATED: a -->\n\n| one |\n\n<!-- END GENERATED: a -->\nafter";
        const string reference = "prose\n<!-- BEGIN GENERATED: a -->\n\n| another |\n\n<!-- END GENERATED: a -->\nafter";

        Assert.Equal(Generator.Authored(reference), Generator.Authored(local));
        Assert.Contains("<!-- BEGIN GENERATED: a -->", Generator.Authored(local));
        Assert.Contains("<!-- END GENERATED: a -->", Generator.Authored(local));
        Assert.DoesNotContain("one", Generator.Authored(local));
    }

    [Fact]
    public void Authored_still_sees_a_difference_in_the_prose_around_a_block()
    {
        const string local = "prose\n<!-- BEGIN GENERATED: a -->\nX\n<!-- END GENERATED: a -->\n";
        const string reference = "other prose\n<!-- BEGIN GENERATED: a -->\nX\n<!-- END GENERATED: a -->\n";

        Assert.NotEqual(Generator.Authored(reference), Generator.Authored(local));
    }

    [Fact]
    public void Authored_empties_every_block_on_a_page_that_carries_several()
    {
        const string text = "a\n<!-- BEGIN GENERATED: one -->\nX\n<!-- END GENERATED: one -->\n"
                            + "b\n<!-- BEGIN GENERATED: two -->\nY\n<!-- END GENERATED: two -->\nc";

        var authored = Generator.Authored(text);

        Assert.DoesNotContain("X", authored);
        Assert.DoesNotContain("Y", authored);
        Assert.Contains("\nb\n", authored);
    }

    [Fact]
    public void Authored_compares_the_whole_page_where_a_block_is_never_closed()
    {
        // The generator cannot follow the structure, so nothing is treated as generated and the drift
        // stays visible rather than being masked by a marker someone deleted half of.
        const string text = "a\n<!-- BEGIN GENERATED: one -->\nX\n";

        Assert.Equal(text, Generator.Authored(text));
    }

    [Fact]
    public void Authored_leaves_a_page_with_no_blocks_exactly_as_it_is()
    {
        const string text = "just prose\n";
        Assert.Equal(text, Generator.Authored(text));
    }
}
