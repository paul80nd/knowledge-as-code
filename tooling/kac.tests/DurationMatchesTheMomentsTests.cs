using kac.core;

// `duration` restates a span `occurred-at` and `restored-at` already fix, and this is what holds the two
// accounts together. The fixture turns the id green; these are the branches a fixture would only
// duplicate, and the guards that keep one fault to one report.

namespace kac.tests;

public class DurationMatchesTheMomentsTests
{
    // The message is the reason this rule is in C#: it carries the value to write, so the fix is a paste.
    [Fact]
    public void A_duration_the_moments_refuse_is_reported()
        => Assert.Equal(
            "'duration' is 'PT30M', and 'occurred-at' to 'restored-at' is 'PT40M'. Write 'PT40M', "
            + "or correct whichever of the two moments is wrong.",
            Assert.Single(Run("2026-06-12T09:00:00Z", "2026-06-12T09:40:00Z", "PT30M")).Message);

    [Fact]
    public void A_duration_the_moments_give_is_left_alone()
        => Assert.Empty(Run("2026-06-12T09:00:00Z", "2026-06-12T09:40:00Z", "PT40M"));

    [Fact]
    public void An_incident_restored_the_moment_it_began_is_left_alone()
        => Assert.Empty(Run("2026-06-12T09:00:00Z", "2026-06-12T09:00:00Z", "PT0S"));

    // Hours rather than days, so one span has one spelling and the comparison is on the text.
    [Fact]
    public void A_span_over_a_day_is_counted_in_hours()
        => Assert.Empty(Run("2026-06-12T09:00:00Z", "2026-06-13T11:00:00Z", "PT26H"));

    [Fact]
    public void The_same_span_spelled_in_days_is_reported()
        => Assert.Single(Run("2026-06-12T09:00:00Z", "2026-06-13T11:00:00Z", "P1DT2H"));

    // The finding points at the value to change rather than at the frontmatter as a whole.
    [Fact]
    public void The_finding_points_at_the_duration()
        => Assert.Equal(8, Assert.Single(Run("2026-06-12T09:00:00Z", "2026-06-12T09:40:00Z", "PT30M")).Line);

    // `restored-not-before-occurred` names this fault, so reporting it here would report it twice.
    [Fact]
    public void Moments_in_the_wrong_order_are_left_to_the_ordering_rule()
        => Assert.Empty(Run("2026-06-12T09:40:00Z", "2026-06-12T09:00:00Z", "PT40M"));

    // Each of these is another check's to report, in better words than a rule about a span could.
    [Fact]
    public void A_record_carrying_no_restored_moment_is_left_alone()
        => Assert.Empty(Run("2026-06-12T09:00:00Z", null, "PT40M"));

    [Fact]
    public void A_record_carrying_no_duration_is_left_alone()
        => Assert.Empty(Run("2026-06-12T09:00:00Z", "2026-06-12T09:40:00Z", null));

    [Fact]
    public void A_moment_that_is_not_a_moment_is_left_alone()
        => Assert.Empty(Run("2026-06-12", "2026-06-12T09:40:00Z", "PT40M"));

    private static List<Finding> Run(string? occurred, string? restored, string? duration)
    {
        var text = "---\nid: pmt-0001\ntype: postmortem\ntier: decided\nstatus: published\n"
                   + Line("occurred-at", occurred)
                   + Line("detected-at", "2026-06-12T09:05:00Z")
                   + Line("restored-at", restored)
                   + Line("duration", duration)
                   + "severity: sev2\nowner: human:alex.doe\n---\n\n"
                   + "# An incident\n\n`Postmortem: pmt-0001` `PUBLISHED`\n";

        var schema = new Schema
        {
            ByFolder = new Dictionary<string, TypeSchema>
            {
                ["postmortems"] = new() { Key = "postmortems", IdPrefix = "pmt" }
            }
        };

        var doc = Required.Parsed("postmortems/0001-an-incident.md", text, schema);

        var found = new List<Finding>();
        new DurationMatchesTheMoments().Check(new RuleContext(
            doc, doc.TypeOf(), new RuleSpec { Id = new RuleId("duration-matches-the-moments") },
            new Report(doc.Rel, found)));

        return found;
    }

    // A key left out entirely, which is the shape a record short of the field actually has.
    private static string Line(string key, string? value) => value is null ? "" : $"{key}: {value}\n";
}
