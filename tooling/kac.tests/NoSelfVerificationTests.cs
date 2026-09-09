using kac.core;

// Any actor may verify a report, and one may not: the producer that generated it. The fixture turns the
// id green; these are the branches a fixture would only duplicate.

namespace kac.tests;

public class NoSelfVerificationTests
{
    [Fact]
    public void The_producer_verifying_its_own_report_is_reported()
        => Assert.Equal(
            "'kac/0.24.0' generated this report and verifies it here. A run confirming its own output "
            + "tells a reader nothing. Delete the entry, or have somebody else read the verdicts and "
            + "name them.",
            Assert.Single(Run("kac/0.24.0", "kac/0.24.0")).Message);

    [Fact]
    public void A_person_verifying_the_report_is_left_alone()
        => Assert.Empty(Run("kac/0.24.0", "human:alex.doe"));

    // The point of accepting any actor. An agent that read the verdicts did work worth recording, and
    // the only thing this rule asks is whether it also wrote them.
    [Fact]
    public void An_agent_verifying_the_report_is_left_alone()
        => Assert.Empty(Run("kac/0.24.0", "agent:coverage-sweep"));

    // A later version reading the corpus again is a second look rather than self-confirmation, so the
    // comparison is exact and never on the producer's name alone.
    [Fact]
    public void A_later_version_of_the_producer_is_left_alone()
        => Assert.Empty(Run("kac/0.24.0", "kac/0.25.0"));

    // The finding points at the entry, because the fix is to delete one line of a list and keep the rest.
    [Fact]
    public void The_finding_points_at_the_entry()
        => Assert.Equal(11, Assert.Single(Run("kac/0.24.0", "agent:sweep", "kac/0.24.0")).Line);

    [Fact]
    public void Every_offending_entry_is_reported()
        => Assert.Equal(2, Run("kac/0.24.0", "kac/0.24.0", "kac/0.24.0").Count);

    // `required-field` has already said so, in better words than a rule about actors could.
    [Fact]
    public void A_report_carrying_no_generated_producer_is_left_alone()
        => Assert.Empty(Run(null, "human:alex.doe"));

    private static List<Finding> Run(string? producer, params string[] verifiers)
    {
        var generated = producer is null
            ? "generated: { at: 2026-09-08T10:00:00Z }\n"
            : $"generated: {{ at: 2026-09-08T10:00:00Z, by: {producer} }}\n";
        var entries = string.Concat(verifiers.Select(v => $"  - {{ at: 2026-09-08T11:00:00Z, by: {v} }}\n"));

        var text = "---\nid: rpt-coverage\ntype: report\ntier: descriptive\nstatus: active\n"
                   + "owner: human:alex.doe\n" + generated
                   + "sources:\n  - { resource: eng, version: \"0.2.0\" }\n"
                   + "verified:\n" + entries + "---\n\n"
                   + "# Clause coverage\n\n`Report: rpt-coverage` `ACTIVE`\n";

        var schema = new Schema
        {
            ByFolder = new Dictionary<string, TypeSchema>
            {
                ["reports"] = new() { Key = "reports", IdPrefix = "rpt" }
            }
        };

        var doc = Required.Parsed("reports/coverage.md", text, schema);

        var found = new List<Finding>();
        new NoSelfVerification().Check(new RuleContext(
            doc, doc.TypeOf(), new RuleSpec { Id = new RuleId("no-self-verification") },
            new Report(doc.Rel, found)));

        return found;
    }
}
