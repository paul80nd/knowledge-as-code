// `staleness-loud` is the runbook twin of `staleness`, and its 92 days are named here rather than in a fixture,
// for the reason `docs/design/checks.md` gives, so moving the number fails here. What separates the two rules is
// that every `rehearsal-frequency` takes the same floor, and these cases pin that: a process stating an event is
// measured by no window, and a runbook stating one is measured by this. The expression comes from the real
// `.schema/runbooks.yaml` rather than a copy, so a rewrite there is judged by these cases.

using kac.core;

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public class StalenessLoudRuleTests
{
    // Counted back from `Required.Today`, which is 2026-06-15.
    private const string LastDayInside = "2026-03-15";   // 92 days
    private const string FirstDayPast = "2026-03-14";    // 93 days

    [Theory]
    [InlineData("on-change")]
    [InlineData("per-release")]
    [InlineData("quarterly")]
    [InlineData("annual")]
    public void The_floor_closes_on_the_ninety_third_day_whatever_the_frequency_states(string frequency)
    {
        Assert.True(Holds(frequency, $"\"{LastDayInside}\""));
        Assert.False(Holds(frequency, $"\"{FirstDayPast}\""));
    }

    [Theory]
    [InlineData("on-change")]
    [InlineData("per-release")]
    [InlineData("quarterly")]
    [InlineData("annual")]
    public void A_runbook_nobody_has_walked_is_reported_whatever_it_states(string frequency)
        => Assert.False(Holds(frequency, "\"never\""));

    // `last-rehearsed` is required, so an absent one is `required-field`'s to report in better words. The
    // guard is what keeps this rule from reporting the same fault a second time and blaming the date for it.
    [Fact]
    public void A_runbook_stating_no_rehearsal_date_is_left_to_required_field()
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
        var doc = Required.Parsed("runbooks/a-slug.md",
            $"---\nid: rbk-a-slug\nrehearsal-frequency: {frequency}\n"
            + $"last-rehearsed:{(lastRehearsed is null ? "" : $" {lastRehearsed}")}\n"
            + "---\n\n# A title\n\nSome prose.", new Schema());

        return RuleExpr.Eval(Expression(), new Facts(doc, Required.Today));
    }

    private static Expr Expression()
    {
        var rule = Schema.Load(Repo.Root).ByFolder["runbooks"].Rules
            .Single(r => r.Id == new RuleId("staleness-loud"));

        return rule.Compiled ?? throw new InvalidOperationException(
            "the runbooks type declares `staleness-loud` with no compiled expr: this rule is what these cases test.");
    }
}
