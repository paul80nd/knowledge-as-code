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

        var table = Generator.SchemaTable(t);

        Assert.Contains("SHORT", table);
        Assert.DoesNotContain("LONG", table); // description wins outright — the two are not concatenated
        Assert.Contains("FALLBACK", table);   // notes still render where no description exists
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
            }
        };
        t.Rules.Add(new Dictionary<string, object> { ["id"] = "y-statement-present" });
        t.Rules.Add(new Dictionary<string, object> { ["id"] = "alternatives-have-verdicts" });

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
