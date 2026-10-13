// `staleness-loud` is the runbook twin of `staleness`. Its 92 days are named here, for the reason
// `docs/reference/checks.md` gives, and the expression is read from the real `.schema/runbooks.yaml`.

using kac.core;

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public class StalenessLoudRuleTests
{
    /// <summary>92 days before <c>Required.Today</c>, which is 2026-06-15.</summary>
    private const string LastDayInside = "2026-03-15";
    /// <summary>93 days before <c>Required.Today</c>, which is 2026-06-15.</summary>
    private const string FirstDayPast = "2026-03-14";

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

    [Fact]
    public void A_runbook_stating_no_rehearsal_date_is_left_to_required_field()
    {
        Assert.True(Holds("quarterly", null));
        Assert.True(Holds("quarterly", ""));
    }

    [Fact]
    public void A_rehearsal_date_in_the_future_is_not_reported()
        => Assert.True(Holds("quarterly", "\"2029-06-01\""));

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
