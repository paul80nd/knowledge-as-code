// Unit tests for the checks table a type page carries, and for the gate reconciling it with the
// catalogue. Both are built from a schema written for the question rather than from the shipped one.

using kac.core;

namespace kac.tests;

public class ChecksTableTests
{
    // The reader-facing table must stay a faithful, complete view of the catalogue, and the catalogue is
    // `_checks.yaml`. `kac checks` reconciles the two against the real schema and exits non-zero on any
    // drift, which the golden suite asserts. That is the one place the shipped file is read.
    [Fact]
    public void Problems_names_a_row_the_catalogue_does_not_carry()
    {
        // A catalogue carrying none of the curated rows: each is a row naming a check that does not
        // exist, which has to be reported rather than quietly rendered onto every type page.
        var problems = ChecksTable.Problems(new Schema());

        Assert.Contains(problems, p => p.Contains("'frontmatter-parses'") && p.Contains("stale row"));
    }

    [Fact]
    public void Problems_names_a_check_no_row_documents()
    {
        // The other direction, and the one a new check trips: declared in the schema, rendered by
        // nothing, and not waived.
        var schema = new Schema { Checks = [new CheckDef(new CheckId("invented-check"), Sev.Error, "Something new.")] };

        var problems = ChecksTable.Problems(schema);

        Assert.Contains(problems, p => p.Contains("'invented-check'") && p.Contains("no row in the checks table"));
    }

    // The page would advertise a check the schema says a record author cannot act on.
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
    public void Render_omits_rows_for_checks_the_type_cannot_trip()
    {
        // A type declaring no rules and no reciprocal/mirrors-section field: the schema-conditional
        // rows must not appear. Unconditional rows would tell a policy reader their documents are
        // checked for Y-statements, which is the ADR-shaped table advertised on every page.
        var table = ChecksTable.Render(new Schema(), new TypeSchema());

        Assert.DoesNotContain("y-statement", table);
        Assert.DoesNotContain("alternatives-verdict", table);
        Assert.DoesNotContain("related-matches-section", table);
        Assert.DoesNotContain("reciprocal", table);
        Assert.Contains("frontmatter-parses", table); // unconditional rows still render
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
