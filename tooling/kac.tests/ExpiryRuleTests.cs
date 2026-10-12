// The expressions come from the real `.schema/deviations.yaml` rather than a copy, so a rewrite there is
// judged by these cases.

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
    public void A_review_date_gone_by_is_reported_by_expiry_for_an_active_deviation_alone(
        string status, string reviewBy, bool silent)
        => Assert.Equal(silent, Holds("expiry", $"status: {status}\nreview-by: \"{reviewBy}\""));

    [Theory]
    [InlineData("draft", LongPast, false)]
    [InlineData("active", LongPast, true)]
    [InlineData("closed", LongPast, true)]
    [InlineData("draft", "2030-01-01", true)]
    public void A_review_date_gone_by_is_reported_by_draft_expiry_for_a_draft_deviation_alone(
        string status, string reviewBy, bool silent)
        => Assert.Equal(silent, Holds("draft-expiry", $"status: {status}\nreview-by: \"{reviewBy}\""));

    [Theory]
    [InlineData("expiry", "active")]
    [InlineData("draft-expiry", "draft")]
    public void A_deviation_carrying_no_review_date_is_left_to_required_field(string rule, string status)
    {
        Assert.True(Holds(rule, $"status: {status}"));
        Assert.True(Holds(rule, $"status: {status}\nreview-by:"));
    }

    /// <summary>
    /// Whether the rule is satisfied: true where it has nothing to say, as an <c>expr:</c> is written.
    /// </summary>
    private static bool Holds(string rule, string frontmatter)
    {
        var doc = Required.Parsed("deviations/a-slug.md",
            $"---\nid: dev-a-slug\n{frontmatter}\n---\n\n# A title\n\nSome prose.", new Schema());

        return RuleExpr.Eval(Expression(rule), new Facts(doc, Required.Today));
    }

    private static Expr Expression(string id)
    {
        var rule = Schema.Load(Repo.Root).ByFolder["deviations"].Rules
            .Single(r => r.Id == new RuleId(id));

        return rule.Compiled ?? throw new InvalidOperationException(
            $"the deviations type declares `{id}` with no compiled expr: this rule is what these cases test.");
    }
}
