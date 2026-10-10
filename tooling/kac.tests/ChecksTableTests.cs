using kac.core;

namespace kac.tests;

public class ChecksTableTests
{
    [Fact]
    public void Problems_names_a_row_the_catalogue_does_not_carry()
    {
        var problems = ChecksTable.Problems(new Schema());

        Assert.Contains(problems, p => p.Contains("'frontmatter-parses'") && p.Contains("stale row"));
    }

    [Fact]
    public void Problems_names_a_check_no_row_documents()
    {
        var schema = new Schema { Checks = [new CheckDef(new CheckId("invented-check"), Sev.Error, "Something new.")] };

        var problems = ChecksTable.Problems(schema);

        Assert.Contains(problems, p => p.Contains("'invented-check'") && p.Contains("no row in the checks table"));
    }

    [Fact]
    public void Problems_names_a_waived_check_that_has_a_row_anyway()
    {
        var schema = new Schema
        {
            Checks = [new CheckDef(new CheckId("frontmatter-parses"), Sev.Error, "Parsed.", OnTypePage: false)]
        };

        var problems = ChecksTable.Problems(schema);

        Assert.Contains(problems, p => p.Contains("'frontmatter-parses'") && p.Contains("has a row anyway"));
    }

    [Fact]
    public void Problems_asks_for_nothing_a_corpus_does_not_hold()
    {
        var schema = new Schema { Checks = [new CheckDef(new CheckId("invented-check"), Sev.Error, "New.")] };

        Assert.All(ChecksTable.Problems(schema), p =>
        {
            Assert.DoesNotContain(".cs", p, StringComparison.Ordinal);
            Assert.DoesNotContain("tooling/", p, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void Render_omits_rows_for_checks_the_type_cannot_trip()
    {
        var table = ChecksTable.Render(new Schema(), new TypeSchema());

        Assert.DoesNotContain("y-statement", table);
        Assert.DoesNotContain("alternatives-verdict", table);
        Assert.DoesNotContain("related-matches-section", table);
        Assert.DoesNotContain("reciprocal", table);
        // Unconditional rows still render.
        Assert.Contains("frontmatter-parses", table);
    }

    [Fact]
    public void Render_includes_rows_the_type_opts_into_through_its_schema()
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
                new RuleSpec { Id = new RuleId("y-statement-present") },
                new RuleSpec { Id = new RuleId("alternatives-have-verdicts") }
            ]
        };

        var table = ChecksTable.Render(new Schema(), t);

        Assert.Contains("y-statement", table);
        Assert.Contains("alternatives-verdict", table);
        Assert.Contains("related-matches-section", table);
        Assert.Contains("reciprocal", table);
    }
}
