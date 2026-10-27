namespace kac.core;

/// <summary>
/// A record whose status was its type's <c>in-force:</c> value keeps the id of every part it had then. Reports each id
/// that has gone once, against the frontmatter.
/// </summary>
public sealed class PartIdKept : IHistoryRule
{
    public RuleId RuleId { get; } = new("part-id-kept");

    public IReadOnlyList<CheckId> Emits { get; } = [new("part-id-kept")];

    public void Check(HistoryRuleContext ctx)
    {
        if (ctx.Before.FrontScalar("status") is not { } status
            || status != ctx.Type.Fields.GetValueOrDefault("status")?.InForce) return;

        var kept = ctx.After.Parts.Select(p => p.Id).OfType<string>().ToHashSet(StringComparer.Ordinal);
        var noun = ctx.Type.Parts?.Noun ?? "part";
        var id = ctx.Before.FrontScalar("id") ?? "";

        foreach (var part in ctx.Before.Parts.Select(p => p.Id).OfType<string>().Distinct(StringComparer.Ordinal)
                     .Where(p => !kept.Contains(p)))
            ctx.Report.Err(Emits[0],
                $"this {ctx.Type.TypeName} was `{status}` at {ctx.Since}, and its {noun} `{part}` is gone. A citation "
                + $"of {id}.{part} may outlive it, so restore the {noun}, and write the change in a new "
                + $"{ctx.Type.TypeName} that supersedes {id}.");
    }
}
