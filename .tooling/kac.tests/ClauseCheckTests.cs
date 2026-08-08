using kac.core;

// In-process unit tests for the clause table. Every branch here is driven by a type's `clauses:`
// declaration rather than by anything specific to policies, so these build a small declaration of their
// own — which is also the only way to show that the modals, the levels and the id pattern are read from
// the schema rather than assumed.

namespace kac.tests;

public class ClauseCheckTests
{
    private static ClauseSpec Spec() =>
        new("^[A-Z]{3,8}$", ["MUST", "MUST NOT"], ["SHOULD"]) { Section = "Clauses", Columns = ["Id", "Clause"] };

    private const string Header = "## Clauses\n\n| Id | Clause |\n|----|--------|\n";

    // -- the table itself, and why a broken one stops the pass --

    [Fact]
    public void A_section_with_no_table_says_what_to_write()
    {
        var found = Run("## Clauses\n\nProse where a table should be.\n");
        Assert.Equal("clause-table", Assert.Single(found).Check);
        Assert.Contains("headed 'Id | Clause'", Assert.Single(found).Message);
    }

    [Fact]
    public void A_mis_headed_table_names_both_headings_and_stops()
    {
        var found = Run("## Clauses\n\n| Ref | Rule |\n|-----|------|\n| `LOGS` | not a modal in sight |\n");
        Assert.Equal("clause-table", Assert.Single(found).Check);
    }

    // Reported and stopped rather than left to the row checks, which would find nothing and say nothing.
    [Fact]
    public void An_empty_table_binds_nobody()
    {
        var found = Run(Header);
        Assert.Equal("clause-table", Assert.Single(found).Check);
        Assert.Contains("binds nothing binds nobody", Assert.Single(found).Message);
    }

    // A missing section is `required-section`'s to report; saying it twice would make one fault two.
    [Fact]
    public void A_document_without_the_section_is_left_to_required_section()
        => Assert.Empty(Run("## Context\n\nNothing to do with clauses.\n"));

    [Fact]
    public void A_type_declaring_no_clauses_is_not_asked_about_them()
        => Assert.Empty(Run(Header + "| `LOGS` | **MUST** be retained. |\n", declareClauses: false));

    // -- the modal, which is the binding level --

    [Fact]
    public void A_row_that_opens_with_no_modal_is_not_an_obligation()
    {
        var found = Run(Header + "| `LOGS` | Audit logs are retained for a year. |\n");
        Assert.Equal("clause-modal", Assert.Single(found).Check);
        Assert.Contains("MUST, MUST NOT, SHOULD", Assert.Single(found).Message);
    }

    // Bold carries the level visually, so a binding modal written plain reads as advice.
    [Fact]
    public void A_binding_modal_written_plain_is_reported()
    {
        var found = Run(Header + "| `LOGS` | MUST be retained for a year. |\n");
        Assert.Equal("clause-modal", Assert.Single(found).Check);
        Assert.Contains("write it bold", Assert.Single(found).Message);
    }

    [Fact]
    public void An_advisory_modal_written_bold_is_reported()
    {
        var found = Run(Header + "| `LOGS` | **SHOULD** be retained for a year. |\n");
        Assert.Contains("write it plain", Assert.Single(found).Message);
    }

    // Longest first, or "MUST NOT" would be read as the "MUST" that prefixes it and then reported as a
    // compound clause carrying a second modal.
    [Fact]
    public void A_longer_modal_is_recognised_before_the_one_that_prefixes_it()
        => Assert.Empty(Run(Header + "| `LOGS` | **MUST NOT** leave the tenancy. |\n"));

    [Fact]
    public void A_second_modal_in_one_row_is_two_obligations_sharing_an_id()
    {
        var found = Run(Header + "| `LOGS` | **MUST** be retained and SHOULD be indexed. |\n");
        Assert.Equal("clause-compound", Assert.Single(found).Check);
        Assert.Contains("carries a second 'SHOULD'", Assert.Single(found).Message);
    }

    // -- the id, which is what makes a clause citable --

    [Fact]
    public void An_id_that_is_not_a_code_span_is_a_word_rather_than_a_handle()
    {
        var found = Run(Header + "| LOGS | **MUST** be retained. |\n");
        Assert.Equal("clause-id-format", Assert.Single(found).Check);
        Assert.Contains("write it as `LOGS`", Assert.Single(found).Message);
    }

    // The pattern is the type's, so this is what shows it is read rather than assumed.
    [Fact]
    public void An_id_outside_the_types_pattern_names_the_pattern()
    {
        var found = Run(Header + "| `lo` | **MUST** be retained. |\n");
        Assert.Contains("does not match ^[A-Z]{3,8}$", Assert.Single(found).Message);
    }

    // Ordinal, because `LOGS` and `logs` differing only in case is not two clauses a reader could tell
    // apart either — though here the pattern catches the lower-cased one first.
    [Fact]
    public void An_id_used_twice_makes_a_citation_of_it_ambiguous()
    {
        var found = Run(Header
                        + "| `LOGS` | **MUST** be retained. |\n"
                        + "| `LOGS` | **MUST** be indexed. |\n");
        Assert.Equal("clause-id-unique", Assert.Single(found).Check);
    }

    // -- the ordering, reported once --

    [Fact]
    public void A_table_out_of_order_is_reported_against_the_first_row_that_breaks_it()
    {
        var found = Run(Header
                        + "| `AAA` | **MUST** be first. |\n"
                        + "| `BBB` | SHOULD be last. |\n"
                        + "| `CCC` | **MUST** be first too. |\n"
                        + "| `DDD` | **MUST** and again. |\n");

        var order = found.Where(f => f.Check == "clause-order").ToList();
        Assert.Single(order);
        Assert.Contains("'CCC' is a 'MUST' but follows a 'SHOULD'", order[0].Message);
    }

    [Fact]
    public void A_table_grouped_binding_before_advisory_is_silent()
        => Assert.Empty(Run(Header
                            + "| `AAA` | **MUST** be retained. |\n"
                            + "| `BBB` | **MUST NOT** leave the tenancy. |\n"
                            + "| `CCC` | SHOULD be indexed. |\n"));

    // -- driving the check --

    private static List<Finding> Run(string body, bool declareClauses = true)
    {
        var type = new TypeSchema { Folder = "policies", Clauses = declareClauses ? Spec() : null };
        var schema = new Schema { ByFolder = new Dictionary<string, TypeSchema> { ["policies"] = type } };
        var doc = Doc.Parse("policies/scrt-security.md", $"---\nid: pol-SCRT\n---\n\n# A policy\n\n{body}", schema);
        Assert.NotNull(doc);

        var found = new List<Finding>();
        ClauseChecks.Check(doc, type,
            (check, msg, line) => found.Add(new Finding(doc.Rel, line, Sev.Error, check, msg)),
            (check, msg, line) => found.Add(new Finding(doc.Rel, line, Sev.Warning, check, msg)));
        return found;
    }
}
