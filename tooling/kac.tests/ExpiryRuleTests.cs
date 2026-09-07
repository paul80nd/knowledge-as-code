// `expiry` is the one rule written against `today()`, and what it stays silent about matters as much as what
// it reports. A fixture proves the record it fires on. These prove the three it does not, which would each
// need a record of its own in the corpus and would then read as a fault somebody meant to fix.
//
// The expression comes from the real `.schema/deviations.yaml` rather than a copy, so a rewrite there is
// judged by these cases instead of leaving them passing against wording nothing uses.

using kac.core;

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public class ExpiryRuleTests
{
    private const string LongPast = "2020-01-01";

    [Theory]
    [InlineData("active", LongPast, false)]
    [InlineData("draft", LongPast, true)]
    [InlineData("closed", LongPast, true)]
    [InlineData("active", "2030-01-01", true)]
    public void A_review_date_gone_by_is_reported_for_an_active_deviation_alone(
        string status, string reviewBy, bool silent)
        => Assert.Equal(silent, Holds($"status: {status}\nreview-by: \"{reviewBy}\""));

    // `review-by` is required, so an absent one is `required-field`'s to report in better words. The guard is
    // what keeps this rule from reporting the same fault a second time and blaming the date for it.
    [Fact]
    public void A_deviation_carrying_no_review_date_is_left_to_required_field()
    {
        Assert.True(Holds("status: active"));
        Assert.True(Holds("status: active\nreview-by:"));
    }

    // Whether the rule is satisfied, which is the sense an `expr:` is written in: true where it has nothing
    // to say. Judged against a named day, so the reading never depends on when this ran.
    private static bool Holds(string frontmatter)
    {
        var doc = Required.Parsed("deviations/a-slug.md",
            $"---\nid: dev-a-slug\n{frontmatter}\n---\n\n# A title\n\nSome prose.", new Schema());

        return RuleExpr.Eval(Expression(), new Facts(doc, Required.Today));
    }

    private static Expr Expression()
    {
        var rule = Schema.Load(Repo.Root).ByFolder["deviations"].Rules
            .Single(r => r.Id == new RuleId("expiry"));

        return rule.Compiled ?? throw new InvalidOperationException(
            "the deviations type declares `expiry` with no compiled expr: this rule is what these cases test.");
    }
}
