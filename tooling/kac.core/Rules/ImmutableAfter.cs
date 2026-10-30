namespace kac.core;

/// <summary>
/// A record that reached one of <paramref name="statuses"/> changes after it by no more than <c>Substance</c> allows.
/// One instance serves each type that declares such a rule.
/// </summary>
/// <param name="noun">The record as a message names it: "ADR".</param>
/// <param name="remedy">The way out, given the record's id, as one or more full sentences.</param>
public sealed class ImmutableAfter(RuleId ruleId, IReadOnlyList<string> statuses, string noun,
    Func<string, string> remedy) : IHistoryRule
{
    public RuleId RuleId => ruleId;

    public IReadOnlyList<CheckId> Emits => [new(ruleId.Value)];

    public void Check(HistoryRuleContext ctx)
    {
        if (ctx.Before.FrontScalar("status") is not { } status || !statuses.Contains(status)) return;

        var changes = Substance.Changes(ctx.Before, ctx.After, ctx.Renamed);
        if (changes.Count == 0) return;

        var id = ctx.After.FrontScalar("id") ?? ctx.Before.FrontScalar("id") ?? "";
        ctx.Report.Err(Emits[0],
            $"this {noun} was {status} at {ctx.Since}, and has changed by more than a typo since: "
            + $"{string.Join(", ", changes.Select(c => c.Part))}. {remedy(id)}",
            changes[0].Line);
    }
}
