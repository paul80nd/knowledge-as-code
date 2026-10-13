// The two windows are named here, for the reason `docs/reference/checks.md` gives, so moving 92 or 366 fails
// here. The expression is read from the real `.schema/processes.yaml`, so a rewrite there is judged by
// these cases.

using kac.core;

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public class StalenessRuleTests
{
    /// <summary>92 days before <c>Required.Today</c>, which is 2026-06-15.</summary>
    private const string QuarterlyLastDayInside = "2026-03-15";
    /// <summary>93 days before <c>Required.Today</c>, which is 2026-06-15.</summary>
    private const string QuarterlyFirstDayPast = "2026-03-14";
    /// <summary>366 days before <c>Required.Today</c>, which is 2026-06-15.</summary>
    private const string AnnualLastDayInside = "2025-06-14";
    /// <summary>367 days before <c>Required.Today</c>, which is 2026-06-15.</summary>
    private const string AnnualFirstDayPast = "2025-06-13";

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

    /// <summary>A date old enough to fail both cycles shows the rule reads the frequency and not the gap.</summary>
    [Theory]
    [InlineData("on-change")]
    [InlineData("per-release")]
    public void An_event_frequency_is_reported_on_never_alone(string frequency)
        => Assert.True(Holds(frequency, "\"2020-01-01\""));

    [Fact]
    public void A_process_stating_no_rehearsal_date_is_left_to_required_field()
    {
        Assert.True(Holds("quarterly", null));
        Assert.True(Holds("quarterly", ""));
    }

    [Fact]
    public void A_rehearsal_date_in_the_future_is_not_reported()
        => Assert.True(Holds("quarterly", "\"2029-06-01\""));

    /// <summary>
    /// Whether the rule is satisfied: true where it has nothing to say, as an <c>expr:</c> is written.
    /// </summary>
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
