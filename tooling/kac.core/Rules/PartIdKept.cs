namespace kac.core;

/// <summary>
/// A record whose status was its type's <c>in-force:</c> value keeps the id of every part it had then. Each id that has
/// gone is one finding, reported at the heading of the parts section.
/// </summary>
public sealed class PartIdKept : IHistoryRule
{
    public RuleId RuleId { get; } = new("part-id-kept");

    public IReadOnlyList<CheckId> Emits { get; } = [new("part-id-kept")];

    public void Check(HistoryRuleContext ctx)
    {
        if (ctx.Before.FrontScalar("status") is not { } status || status != ctx.InForce) return;

        // A part whose id no longer parses is one the id-format checks report, and reads here as an id removed.
        if (ctx.After.Parts.Any(p => p.Id is null)) return;

        var kept = ctx.After.Parts.Select(p => p.Id).OfType<string>().ToHashSet(StringComparer.Ordinal);
        var (type, noun) = (ctx.Type.TypeName, ctx.Type.Parts?.Noun ?? "part");
        var id = ctx.Before.FrontScalar("id") ?? "";
        var replace = ctx.Type.Fields.ContainsKey("supersedes")
            ? $"write the change in a new {type} that names {id} in `supersedes`"
            : $"retire {id} and write the change in a new {type}";
        int? line = ctx.After.PartSectionLine > 0 ? ctx.After.PartSectionLine : null;

        foreach (var part in ctx.Before.Parts.Select(p => p.Id).OfType<string>().Distinct(StringComparer.Ordinal)
                     .Where(p => !kept.Contains(p)))
            ctx.Report.Err(Emits[0],
                $"this {type} was `{status}` at {ctx.Since}, and its {noun} `{part}` is gone. A citation of "
                + $"{id}.{part} may outlive it, so restore the {noun}, or {replace}.", line);
    }
}
