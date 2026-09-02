// Unit tests for the page renderers. The full index and `<type>.md` generation is covered by the golden
// `generate` scenario; these pin in-process what one renderer makes of a schema built for the question.

using kac.core;

namespace kac.tests;

public class GeneratorTests
{
    [Fact]
    public void IndexPage_says_it_is_empty_rather_than_rendering_a_headless_table()
    {
        var t = new TypeSchema { Label = "Control", IdPrefix = "ctl", IndexColumns = ["id", "title"] };

        var page = Generator.IndexPage(t, []);

        Assert.Contains("# Control Index (CTL)", page);
        Assert.Contains("Nothing here yet", page);
        Assert.Contains("_template.md", page); // an empty index points at the way to fill it
        Assert.DoesNotContain("| Id |", page); // …rather than a table with headers and no rows
    }

    [Fact]
    public void IndexPage_heading_drops_the_prefix_when_it_only_repeats_the_label()
    {
        var t = new TypeSchema { Label = "ADR", IdPrefix = "adr", IndexColumns = ["id", "title"] };

        Assert.Contains("# ADR Index\n", Generator.IndexPage(t, []));
    }

    // The three sorts a type can declare, over one set of rows: no `sort:` at all, several columns, and
    // a direction. Each is a value the loader reads and the golden 'generate' scenario cannot reach, because
    // the fixture corpora it runs over hold one type whose index sorts by id.
    private static List<Doc> Rows(params (string Id, string Severity)[] rows) =>
    [
        .. rows.Select(r => Required.Parsed($"runbooks/{r.Id}.md",
            $"---\nid: {r.Id}\nseverity: {r.Severity}\n---\n\n# {r.Id}\n", new Schema()))
    ];

    // A link naming the file alone would land beside the index rather than on the record.
    [Fact]
    public void IndexPage_links_a_record_through_the_category_folder_holding_it()
    {
        var t = new TypeSchema { Label = "Standard", IdPrefix = "std", IndexColumns = ["title"] };
        var doc = Required.Parsed("standards/common/build-gates.md",
            "---\nid: std-GATES\n---\n\n# A failing check blocks the merge\n", new Schema());

        var page = Generator.IndexPage(t, [doc]);

        Assert.Contains("(common/build-gates.md)", page);
    }

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

        // severity first, then id within it. So the sev1 pair leads, in id order, and sev3 follows.
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

        Assert.True(page.IndexOf("rbk-a", StringComparison.Ordinal)
                    < page.IndexOf("rbk-c", StringComparison.Ordinal));
        Assert.True(page.IndexOf("rbk-c", StringComparison.Ordinal)
                    < page.IndexOf("rbk-b", StringComparison.Ordinal));
    }

    // A type deriving a field from where its records sit, which is what an index groups on. The schema
    // is built rather than loaded so each test below can file records wherever it wants to.
    private static Schema Located() => new()
    {
        ByFolder = new Dictionary<string, TypeSchema>
        {
            ["policies"] = new()
            {
                TypeName = "policy",
                Label = "Policy",
                Folder = "policies",
                IdPrefix = "pol",
                IndexColumns = ["id", "title", "category"],
                Fields = new Dictionary<string, FieldSpec>
                {
                    ["category"] = new() { Name = "category", From = "sub-path" }
                }
            }
        }
    };

    private static (TypeSchema Type, List<Doc> Docs) Filed(params string[] rels)
    {
        var schema = Located();
        return (schema.ByFolder["policies"],
        [
            .. rels.Select((rel, i) => Required.Parsed($"policies/{rel}",
                $"---\nid: pol-{i:0000}\n---\n\n# Record {i}\n", schema))
        ]);
    }

    // The grouping, and the reason a reader gets headings at all: a type with folders reads as one
    // undifferentiated list without them.
    [Fact]
    public void IndexPage_heads_a_table_per_folder_where_the_type_derives_one()
    {
        var (t, docs) = Filed("security/one.md", "delivery/two.md");

        var page = Generator.IndexPage(t, docs);

        Assert.Contains("## Delivery\n", page);
        Assert.Contains("## Security\n", page);
        Assert.True(page.IndexOf("## Delivery", StringComparison.Ordinal)
                    < page.IndexOf("## Security", StringComparison.Ordinal));
    }

    // Only the first level heads a table. A heading per level would give Node one of its own, and a
    // reader scanning for it has to already know that Node sits under Platform.
    [Fact]
    public void IndexPage_groups_a_record_nested_deeper_under_its_first_folder()
    {
        var (t, docs) = Filed("platform/node/one.md", "platform/two.md");

        var page = Generator.IndexPage(t, docs);

        Assert.Contains("## Platform\n", page);
        Assert.DoesNotContain("## Node", page);
    }

    // The derived column earns its place only where it says more than the heading does.
    [Fact]
    public void IndexPage_drops_the_derived_column_where_it_repeats_the_heading()
    {
        var (t, docs) = Filed("security/one.md");

        var page = Generator.IndexPage(t, docs);

        Assert.DoesNotContain("Category", page);
    }

    [Fact]
    public void IndexPage_keeps_the_derived_column_where_a_record_sits_below_the_heading()
    {
        var (t, docs) = Filed("platform/node/one.md");

        var page = Generator.IndexPage(t, docs);

        Assert.Contains("Category", page);
        Assert.Contains("platform/node", page);
    }

    // A corpus that has never used a folder reads as it always did, bar the column that had nothing in
    // it to read.
    [Fact]
    public void IndexPage_heads_nothing_where_every_record_is_filed_flat()
    {
        var (t, docs) = Filed("one.md", "two.md");

        var page = Generator.IndexPage(t, docs);

        Assert.DoesNotContain("## ", page);
        Assert.DoesNotContain("Category", page);
    }

    // Folders alone group nothing. A type that derives no field from where its records sit has no
    // grouping key, so its index reads as one table however deep the records are filed.
    [Fact]
    public void IndexPage_heads_nothing_where_the_type_derives_no_field_from_the_path()
    {
        var t = new TypeSchema { Label = "Standard", IdPrefix = "std", IndexColumns = ["title"] };
        var doc = Required.Parsed("standards/common/build-gates.md",
            "---\nid: std-GATES\n---\n\n# A failing check blocks the merge\n", new Schema());

        var page = Generator.IndexPage(t, [doc]);

        Assert.DoesNotContain("## ", page);
    }

    // Records filed flat beside foldered ones lead, in a table with no heading, because there is no
    // folder to name one after.
    [Fact]
    public void IndexPage_leads_with_the_records_no_folder_holds()
    {
        var (t, docs) = Filed("security/one.md", "loose.md");

        var page = Generator.IndexPage(t, docs);

        Assert.True(page.IndexOf("pol-0001", StringComparison.Ordinal)
                    < page.IndexOf("## Security", StringComparison.Ordinal));
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

        var table = Generator.SchemaTable(t, new Schema(), "");

        Assert.Contains("SHORT", table);
        Assert.DoesNotContain("LONG", table); // description wins outright: the two are not concatenated
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

        var table = Generator.SchemaTable(t, s, "");

        Assert.Contains("`id` *†", table);    // universal fields are marked, required ones twice…
        Assert.Contains("`status` †", table); // …including one the type redeclares
        Assert.Contains("`own-field`", table);
        Assert.DoesNotContain("`own-field` †", table);
        Assert.Contains("\\* Field is required", table);
        Assert.Contains("REFINED", table); // EffectiveField wins over the universal declaration
        Assert.DoesNotContain("VARIES BY TYPE", table);
        Assert.True(table.IndexOf("`id`", StringComparison.Ordinal)
                    < table.IndexOf("`own-field`", StringComparison.Ordinal)); // universal first
        Assert.Contains("† Carried by every document", table);
    }

    [Fact]
    public void SchemaTable_carries_enum_values_in_the_value_column()
    {
        var t = new TypeSchema
        {
            FieldOrder = ["status", "owner"],
            Fields = new Dictionary<string, FieldSpec>
            {
                ["status"] = new()
                {
                    Name = "status", Type = "enum", Values = ["draft", "active"], Description = "SHORT PROSE"
                },
                ["owner"] = new() { Name = "owner", Type = "string", Description = "PLAIN PROSE" }
            }
        };

        var table = Generator.SchemaTable(t, new Schema(), "");
        var row = table.Split('\n').Single(l => l.StartsWith("| `status`", StringComparison.Ordinal));

        // The set is what an author came to the page for, so it is what the column carries. The word
        // `enum` is not something anyone can write into frontmatter.
        var header = table.Split('\n')[0];
        Assert.Contains("Value", header);
        Assert.DoesNotContain("Type", header);
        Assert.Contains("`draft` `active`", row);
        Assert.DoesNotContain("enum", row);
        Assert.Contains("SHORT PROSE", row);
        Assert.DoesNotContain("**Enum values**", table);

        // A field that is not an enum still names its type.
        Assert.Contains("string", table.Split('\n').Single(l => l.StartsWith("| `owner`", StringComparison.Ordinal)));
    }

    [Fact]
    public void SchemaTable_narrows_tier_to_the_one_value_the_type_carries()
    {
        var s = new Schema
        {
            UniversalOrder = ["tier"],
            Universal = new Dictionary<string, FieldSpec>
            {
                ["tier"] = new()
                {
                    Name = "tier", Type = "enum", Required = true,
                    Values = ["decided", "normative", "descriptive"]
                }
            }
        };
        var t = new TypeSchema { Tier = "normative" };

        var row = Generator.SchemaTable(t, s, "").Split('\n')
            .Single(l => l.StartsWith("| `tier`", StringComparison.Ordinal));

        // Every document in the folder carries this one, and CI fails any that does not: the other
        // values are not choices the author has.
        Assert.Contains("`normative`", row);
        Assert.DoesNotContain("`decided`", row);
        Assert.DoesNotContain("`descriptive`", row);
    }

    [Fact]
    public void SchemaTable_closes_a_note_with_the_required_when_condition()
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

        var table = Generator.SchemaTable(t, new Schema(), "");
        var row = table.Split('\n').Single(l => l.StartsWith("| `retention`", StringComparison.Ordinal));

        // The condition is quoted exactly, and closes the note it qualifies rather than sitting in a
        // second table the reader has to look away to.
        Assert.Contains("SHORT PROSE Required when `classification in [personal, special-category]`.", row);
        Assert.DoesNotContain("**Conditionally required**", table);
    }

    [Fact]
    public void SchemaTable_omits_the_legend_when_no_universal_field_applies()
    {
        var t = new TypeSchema
        {
            FieldOrder = ["own-field"],
            Fields = new Dictionary<string, FieldSpec> { ["own-field"] = new() { Name = "own-field" } }
        };

        Assert.DoesNotContain("†", Generator.SchemaTable(t, new Schema(), ""));
    }

    private static TypeSchema Type(string label, string plural, string tier, string page, string goesHere,
        params (string Other, string Text)[] versus) => new()
    {
        Key = page[..^3], Versus = versus,
        Label = label, LabelPlural = plural, Tier = tier, Page = page, GoesHere = goesHere,
        Summary = $"What a {label.ToLowerInvariant()} holds.",
        Detail = $"And the edge a reader walks over when they mistake a {label.ToLowerInvariant()} for something else."
    };

    private static readonly List<TierSpec> Tiers =
    [
        new("decided", "Decided", "immutable once accepted", ""),
        new("normative", "Normative", "living, owned, reviewed", "The rules, and what verifies them."),
        new("descriptive", "Descriptive", "living, must mirror reality", ""),
        new("procedural", "Procedural", "living, must be rehearsed", ""),
        new("observed", "Observed", "perishable, unreviewed until promoted", "")
    ];

    private static TypeSchema[] Four() =>
    [
        Type("Runbook", "Runbooks", "procedural", "runbooks.md", "A step-by-step for when something is broken"),
        Type("ADR", "ADRs", "decided", "adrs.md", "A decision that affects more than one repo"),
        Type("Standard", "Standards", "normative", "standards.md", "A rule people must follow when building"),
        Type("Control", "Controls", "normative", "controls.md", "A check that proves a rule is being followed")
    ];

    private static List<int> PositionsOf(string text, params string[] needles) =>
        [.. needles.Select(n => text.IndexOf(n, StringComparison.Ordinal))];

    // The decision table is read down its left column.
    [Fact]
    public void The_decision_table_is_sorted_by_what_the_reader_is_holding()
    {
        var order = PositionsOf(Generator.PlacementTable(Four(), "../"),
            "A check that", "A decision that", "A rule people", "A step-by-step");

        Assert.Equal(order.Order(), order);
    }

    // The link points at the collection.
    [Fact]
    public void The_decision_table_names_a_type_in_the_plural()
    {
        var rows = Generator.PlacementTable(Four(), "../");

        Assert.Contains("[ADRs](../adrs.md)", rows); // relative to the block's own file, and naming the file
        Assert.DoesNotContain("[ADR](", rows);
    }

    // Tier is a column, not a grouping.
    [Fact]
    public void The_corpus_index_is_sorted_by_type_name()
    {
        var order = PositionsOf(Generator.TypesIndex(Four(), "taxonomy.md", ""),
            "[ADR]", "[Control]", "[Runbook]", "[Standard]");

        Assert.Equal(order.Order(), order);
    }

    [Fact]
    public void The_corpus_index_carries_the_way_on_to_the_taxonomy_inside_the_block()
    {
        var index = Generator.TypesIndex(Four(), "knowledge-as-code/taxonomy.md", "");

        Assert.Contains("[taxonomy](knowledge-as-code/taxonomy.md)", index);
        Assert.True(index.IndexOf("| [ADR]", StringComparison.Ordinal)
                    < index.IndexOf("[taxonomy]", StringComparison.Ordinal));
        Assert.All(index.Split('\n').Where(l => !l.StartsWith('|')), l => Assert.True(l.Length <= 120));
    }

    [Fact]
    public void The_catalogue_runs_in_the_order_the_tiers_are_declared()
    {
        var order = PositionsOf(Generator.TypeCatalogue(Tiers, Four(), "../"),
            "### Decided", "### Normative", "### Procedural");

        Assert.Equal(order.Order(), order);
    }

    // A heading with nothing under it reads as a gap in the corpus rather than as a choice it made.
    [Fact]
    public void A_tier_no_stood_up_type_sits_in_gets_no_heading()
    {
        var catalogue = Generator.TypeCatalogue(Tiers, Four(), "../");

        Assert.Contains("### Decided", catalogue);
        Assert.DoesNotContain("### Descriptive", catalogue);
        Assert.DoesNotContain("### Observed", catalogue);
    }

    [Fact]
    public void A_tier_note_is_rendered_before_the_types_beneath_it()
    {
        var catalogue = Generator.TypeCatalogue(Tiers, Four(), "../");
        var order = PositionsOf(catalogue, "### Normative", "The rules, and what verifies them.", "[Controls]");

        Assert.Equal(order.Order(), order);
    }

    [Fact]
    public void A_catalogue_entry_names_the_collection_and_carries_both_halves_of_the_prose()
    {
        var catalogue = Generator.TypeCatalogue(Tiers, Four(), "../");

        Assert.Contains("**[ADRs](../adrs.md).** What a adr holds. And the edge", catalogue);
    }

    // A table cell cannot be broken and is exempt, which is why the wrap belongs to the prose renderers
    // rather than to RenderTable.
    [Fact]
    public void Catalogue_prose_wraps_at_the_corpus_margin()
        => Assert.All(Generator.TypeCatalogue(Tiers, Four(), "../").Split('\n'), l => Assert.True(l.Length <= 120));

    // A link broken after its label's first word still renders, but it reads badly and a formatter meeting
    // it puts it back. The generator then writes it out broken again on the next run.
    [Fact]
    public void A_link_label_is_never_broken_across_lines()
    {
        var t = new TypeSchema
        {
            Key = "widgets", Label = "Widget", LabelPlural = "Widgets", Tier = "decided", Page = "widgets.md",
            Summary = "Short.",
            Detail = "Padding to push the link past the margin so the wrap has to choose a break near it, and then "
                     + "some more padding after that, before [RFC 2119](https://www.rfc-editor.org/rfc/rfc2119) lands."
        };

        var catalogue = Generator.TypeCatalogue(Tiers, [t], "../");

        Assert.Contains("[RFC 2119](https://www.rfc-editor.org/rfc/rfc2119)", catalogue);
    }

    [Fact]
    public void A_word_longer_than_the_margin_is_left_whole_on_its_own_line()
    {
        var long_ = new string('x', 200);
        var t = new TypeSchema
        {
            Label = "Widget", LabelPlural = "Widgets", Tier = "decided", Page = "widgets.md",
            Summary = "Short.", Detail = long_
        };

        Assert.Contains(long_, Generator.TypeCatalogue(Tiers, [t], "../"));
    }

    private static TypeSchema[] Pair(params (string Other, string Text)[] versus) =>
    [
        Type("ADR", "ADRs", "decided", "adrs.md", "A decision", versus),
        Type("Standard", "Standards", "normative", "standards.md", "A rule")
    ];

    [Fact]
    public void A_disambiguation_is_headed_from_the_side_that_declares_it()
    {
        var text = Generator.Disambiguations(Pair(("standards", "The ADR is the decision, frozen.")));

        Assert.Contains("**ADR vs Standard.** The ADR is the decision, frozen.", text);
    }

    // A corpus with no standards is not helped by being told how one differs from an ADR, and the
    // heading would name a page it cannot open.
    [Fact]
    public void A_pair_whose_other_half_is_not_stood_up_is_left_out()
    {
        var adrsOnly = Pair(("standards", "The ADR is the decision, frozen."))[..1];

        Assert.Empty(Generator.Disambiguations(adrsOnly));
    }

    [Fact]
    public void Disambiguations_are_sorted_by_heading()
    {
        var text = Generator.Disambiguations(Pair(
            ("standards", "Second by heading."), ("zzz", "Never rendered — no such type.")));

        Assert.Contains("ADR vs Standard", text);
        Assert.DoesNotContain("Never rendered", text);
    }

    [Fact]
    public void The_metadata_strip_links_each_type_at_the_heading_its_field_table_sits_under()
    {
        var strip = Generator.MetadataStrip(Four(), "../");

        Assert.Contains("[ADR](../adrs.md#metadata)", strip);
        Assert.Contains(" · ", strip);
    }

    [Fact]
    public void The_metadata_strip_wraps_at_the_corpus_margin()
        => Assert.All(Generator.MetadataStrip(Four(), "../").Split('\n'), l => Assert.True(l.Length <= 120));

    private static TypeSchema Ancestor(string label, string key, LineageSpec? lineage) => new()
    {
        Key = key, Label = label, LabelPlural = label + "s", Tier = "decided", Page = $"{key}.md",
        Summary = "A thing.", Detail = "A longer thing.", GoesHere = "A thing", Lineage = lineage
    };

    [Fact]
    public void A_lineage_row_carries_the_prior_art_and_both_sides_of_it()
    {
        var table = Generator.LineageTable(
        [
            Ancestor("ADR", "adrs", new LineageSpec("[Nygard](https://x)", "The three sections", "Ours spans repos"))
        ], "../");

        Assert.Contains("| [ADR](../adrs.md)", table);
        Assert.Contains("[Nygard](https://x)", table);
        Assert.Contains("The three sections", table);
        Assert.Contains("Ours spans repos", table);
    }

    // An empty cell would read as unfinished.
    [Fact]
    public void A_type_with_no_ancestor_says_so_rather_than_leaving_the_cells_blank()
    {
        var table = Generator.LineageTable([Ancestor("Discovery", "discoveries", new LineageSpec("None.", "", ""))],
            "../");

        Assert.Contains("| None.", table);
        Assert.Contains("| — ", table);
    }

    [Fact]
    public void A_type_that_declares_no_lineage_is_left_out_of_the_table()
        => Assert.DoesNotContain("Widget", Generator.LineageTable([Ancestor("Widget", "widgets", null)], "../"));

    private static TypeSchema Colliding(string label, string key, string collision) => new()
    {
        Key = key, Label = label, LabelPlural = label + "s", Tier = "normative", Page = $"{key}.md",
        Summary = "A thing.", Detail = "A longer thing.", GoesHere = "A thing", Collision = collision
    };

    [Fact]
    public void Only_a_type_that_collides_with_something_gets_a_heading()
    {
        var text = Generator.Collisions(
            [Colliding("Control", "controls", "Elsewhere it is the safeguard."), Colliding("ADR", "adrs", "")]);

        Assert.Contains("### Control", text);
        Assert.DoesNotContain("### ADR", text);
    }

    // A blank line in the folded scalar comes back as a newline, and each paragraph is wrapped on its own.
    [Fact]
    public void A_collision_of_several_paragraphs_renders_as_several()
    {
        var text = Generator.Collisions([Colliding("Control", "controls", "First para.\nSecond para.")]);

        Assert.Contains("First para.\n\nSecond para.", text);
    }

    [Fact]
    public void Collisions_are_sorted_by_type_name()
    {
        var order = PositionsOf(Generator.Collisions(
            [
                Colliding("Standard", "standards", "An external body publishes one."),
                Colliding("Control", "controls", "Elsewhere it is the safeguard.")
            ]),
            "### Control", "### Standard");

        Assert.Equal(order.Order(), order);
    }

    [Fact]
    public void Collision_prose_wraps_at_the_corpus_margin()
    {
        const string para = "A paragraph long enough that the generator has to break it somewhere sensible, which is "
                            + "the whole point of wrapping prose a person did not lay out by hand.";

        Assert.All(Generator.Collisions([Colliding("Control", "controls", para)]).Split('\n'),
            l => Assert.True(l.Length <= 120));
    }

    private static TypeSchema Linked(string label, string key, params (string Field, string[] Refs, string? Back)[] fs)
    {
        var fields = fs.ToDictionary(f => f.Field,
            f => new FieldSpec { Name = f.Field, Type = "list", Of = "id", Refs = f.Refs, Reciprocal = f.Back });

        return new TypeSchema
        {
            Key = key, Label = label, LabelPlural = label + "s", Tier = "normative", Page = $"{key}.md",
            Summary = "A thing.", Detail = "A longer thing.", GoesHere = "A thing",
            FieldOrder = [.. fs.Select(f => f.Field)], Fields = fields
        };
    }

    private static TypeSchema[] Graph() =>
    [
        Linked("Standard", "standards", ("implements", ["policies"], null), ("verified-by", ["controls"], "verifies")),
        Linked("Control", "controls", ("verifies", ["standards"], "verified-by")),
        Linked("Policy", "policies")
    ];

    [Fact]
    public void The_relation_table_carries_both_halves_of_a_reciprocal_pair()
    {
        var table = Generator.RelationTable(Graph());

        Assert.Contains("| Standard | `verified-by`", table);
        Assert.Contains("| Control  | `verifies`", table);
        Assert.Contains("`verifies`    |", table); // …each naming the other in the last column
    }

    // A one-directional edge has nobody obliged to answer it.
    [Fact]
    public void A_one_directional_edge_names_no_counterpart()
        => Assert.Matches(@"\| Standard \| `implements`\s+\| Policy\s+\|\s+\|", Generator.RelationTable(Graph()));

    // The field would resolve against no document, so the row would promise an edge that cannot exist.
    [Fact]
    public void An_edge_at_a_type_the_corpus_has_not_stood_up_is_dropped()
    {
        var table = Generator.RelationTable(Graph()[..2]); // policies not stood up

        Assert.DoesNotContain("implements", table);
        Assert.Contains("verifies", table);
    }

    // The subset of Mermaid an Azure DevOps wiki renders: `graph`, never `flowchart`; no subgraphs; and
    // no arrow longer than `-->`. Exceeding it fails silently in the wiki, so it is pinned here.
    [Fact]
    public void The_diagram_stays_inside_what_an_ADO_wiki_renders()
    {
        var diagram = Generator.RelationDiagram(Graph());

        Assert.StartsWith("```mermaid\ngraph LR;\n", diagram);
        Assert.DoesNotContain("flowchart", diagram);
        Assert.DoesNotContain("subgraph", diagram);
        Assert.DoesNotContain("--->", diagram);

        // No pipes: a markdown formatter scanning for tables reformats what it finds between them, and a
        // fenced block is no protection.
        Assert.DoesNotContain("|", diagram);
    }

    // The table carries two rows for the pair, because an author has two fields.
    [Fact]
    public void A_reciprocal_pair_is_drawn_once()
    {
        var diagram = Generator.RelationDiagram(Graph());

        Assert.Contains("t_controls -- verifies --> t_standards;", diagram);
        Assert.DoesNotContain("verified-by", diagram);
    }

    [Fact]
    public void A_type_with_no_edges_is_still_drawn()
        => Assert.Contains("t_policies[Policy];", Generator.RelationDiagram(Graph()));
}
