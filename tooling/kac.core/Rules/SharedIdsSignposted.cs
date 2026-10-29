namespace kac.core;

/// <summary>
/// Two records of one type whose parts share an id are a pair, and each part signposts the other. A signpost naming a
/// part that does not exist is <c>part-ref</c>'s. A scoped signpost reaches another corpus, which cannot write the
/// other end, so it makes no pair.
/// </summary>
public sealed class SharedIdsSignposted : ICorpusRule
{
    public RuleId RuleId => new("shared-ids-signposted");

    private static readonly CheckId OneWay = new("pair-one-way");
    private static readonly CheckId Unsignposted = new("shared-id-unsignposted");

    public IReadOnlyList<CheckId> Emits => [OneWay, Unsignposted];

    /// <summary>A retired record pairs with nothing, so its successor may keep its ids.</summary>
    private const string Retired = "retired";

    /// <summary>Only a table row has its signposts read, so on a heading every shared id would warn.</summary>
    public IEnumerable<string> Misconfigured(CorpusRuleContext ctx)
    {
        if (ctx.Type.Parts?.Source == PartSpec.Table) yield break;

        yield return "it reads each signpost from a row of a parts table, and this type writes its parts as "
                     + "headings. Declare it on a type whose parts are a table.";
    }

    public void Check(CorpusRuleContext ctx)
    {
        var noun = ctx.Type.Parts?.Noun ?? "part";

        var parts = ctx.Records
            .Where(doc => !string.Equals(doc.FrontScalar("status"), Retired, StringComparison.Ordinal))
            .SelectMany(doc => doc.FrontScalar("id") is { } id
                ? doc.Parts.Where(p => p.Id is not null).Select(p => new Held(doc, id, p))
                : [])
            .ToList();

        foreach (var group in parts.GroupBy(p => p.Row.Id, StringComparer.Ordinal))
        {
            var held = group.ToList();
            foreach (var a in held)
            foreach (var b in held)
            {
                if (string.Equals(a.Record, b.Record, StringComparison.OrdinalIgnoreCase)) continue;

                var (forward, back) = (a.Signposts(b), b.Signposts(a));
                if (back) continue;

                var shares = $"{noun} '{b.Row.Id}' shares its id with {a.Record}.{b.Row.Id}";
                if (forward)
                    ctx.Err(b.Doc, OneWay,
                        $"{shares}, which signposts it, and does not signpost that {noun} back. Close this {noun} "
                        + $"with ([{a.Record}].{b.Row.Id} and what the reader gains there).", b.Row.Line);

                // Once a pair, against the record whose id sorts first.
                else if (string.Compare(b.Record, a.Record, StringComparison.Ordinal) < 0)
                    ctx.Warn(b.Doc, Unsignposted,
                        $"{shares}, and neither {noun} signposts the other. Where they state one obligation, close "
                        + "each with a signpost to the other. Where they do not, give one of them another id.",
                        b.Row.Line);
            }
        }
    }

    private sealed record Held(Doc Doc, string Record, PartRow Row)
    {
        public bool Signposts(Held other) =>
            Row.Signposts.Any(s => s.Scope is null
                                   && string.Equals(s.Record, other.Record, StringComparison.OrdinalIgnoreCase)
                                   && string.Equals(s.Part, other.Row.Id, StringComparison.Ordinal));
    }
}
