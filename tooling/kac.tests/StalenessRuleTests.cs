// `staleness` is the one rule measuring a record's age against the day the run happens. Its two windows are
// named here rather than in a fixture, for the reason `docs/design/checks.md` gives, so moving 92 or 366 fails
// here. These cases also pin what the rule stays silent about: `on-change` and `per-release` state an event,
// and no window measures one. The expression comes from the real `.schema/processes.yaml` rather than a copy,
// so a rewrite there is judged by these cases instead of leaving them passing against wording nothing uses.

using kac.core;

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public class StalenessRuleTests
{
    // Counted back from `Required.Today`, which is 2026-06-15.
    private const string QuarterlyLastDayInside = "2026-03-15";   // 92 days
    private const string QuarterlyFirstDayPast = "2026-03-14";    // 93 days
    private const string AnnualLastDayInside = "2025-06-14";      // 366 days
    private const string AnnualFirstDayPast = "2025-06-13";       // 367 days

    [Theory]
    [InlineData("quarterly", QuarterlyLastDayInside, true)]
    [InlineData("quarterly", QuarterlyFirstDayPast, false)]
    [InlineData("annual", AnnualLastDayInside, true)]
    [InlineData("annual", AnnualFirstDayPast, false)]
    public void A_cycle_is_reported_the_day_after_its_window_closes(
        string frequency, string lastRehearsed, bool silent)
        => Assert.Equal(silent, Holds(frequency, $"\"{lastRehearsed}\""));

    [Theory]
    [InlineData("on-change")]
    [InlineData("per-release")]
    [InlineData("quarterly")]
    [InlineData("annual")]
    public void A_process_nobody_has_walked_is_reported_whatever_it_states(string frequency)
        => Assert.False(Holds(frequency, "\"never\""));

    // An event has no window, so the age of the date says nothing about whether the process is current. A
    // date old enough to fail both cycles is what shows the rule reads the frequency and not just the gap.
    [Theory]
    [InlineData("on-change")]
    [InlineData("per-release")]
    public void An_event_frequency_is_reported_on_never_alone(string frequency)
        => Assert.True(Holds(frequency, "\"2020-01-01\""));

    // `last-rehearsed` is required, so an absent one is `required-field`'s to report in better words. The
    // guard is what keeps this rule from reporting the same fault a second time and blaming the date for it.
    [Fact]
    public void A_process_stating_no_rehearsal_date_is_left_to_required_field()
    {
        Assert.True(Holds("quarterly", null));
        Assert.True(Holds("quarterly", ""));
    }

    // A date the run has not reached measures nothing, so `days_since` answers zero and the rule passes.
    // No check reports a rehearsal date in the future.
    [Fact]
    public void A_rehearsal_date_in_the_future_is_not_reported()
        => Assert.True(Holds("quarterly", "\"2029-06-01\""));

    // Whether the rule is satisfied, which is the sense an `expr:` is written in: true where it has nothing
    // to say. Judged against a named day, so the reading never depends on when this ran.
    private static bool Holds(string frequency, string? lastRehearsed)
    {
        var doc = Required.Parsed("processes/a-slug.md",
            $"---\nid: prc-a-slug\nrehearsal-frequency: {frequency}\n"
            + $"last-rehearsed:{(lastRehearsed is null ? "" : $" {lastRehearsed}")}\n"
            + "---\n\n# A title\n\nSome prose.", new Schema());

        return RuleExpr.Eval(Expression(), new Facts(doc, Required.Today));
    }

    private static Expr Expression()
    {
        var rule = Schema.Load(Repo.Root).ByFolder["processes"].Rules
            .Single(r => r.Id == new RuleId("staleness"));

        return rule.Compiled ?? throw new InvalidOperationException(
            "the processes type declares `staleness` with no compiled expr: this rule is what these cases test.");
    }
}
