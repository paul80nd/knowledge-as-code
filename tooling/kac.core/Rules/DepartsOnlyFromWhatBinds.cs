namespace kac.core;

/// <summary>
/// A part whose level cannot be read, or any of whose bullets states none, is passed over: <c>clause-modal</c> and
/// <c>part-modal</c> report it in the corpus that wrote it. A citation that does not resolve is <c>ref-resolves</c>'s.
/// </summary>
public sealed class DepartsOnlyFromWhatBinds : ICorpusRule
{
    public RuleId RuleId => new("departs-only-from-what-binds");

    private static readonly CheckId NeedsNone = new("departure-needs-none");

    private const string Field = "departs-from";

    /// <summary>A closed deviation is silent, as <c>expiry</c> leaves it.</summary>
    private const string Closed = "closed";

    public IReadOnlyList<CheckId> Emits => [NeedsNone];

    public void Check(CorpusRuleContext ctx)
    {
        foreach (var doc in ctx.Records)
        {
            if (string.Equals(doc.FrontScalar("status"), Closed, StringComparison.Ordinal)) continue;

            foreach (var entry in doc.FrontList(Field))
            {
                var citation = Citation.Read(entry);
                if (citation.Part is not { } part) continue;

                var landed = ctx.Resolver.Resolve(citation);
                if (landed.How is not (Landing.Local or Landing.Imported)) continue;
                if (!landed.Carries(part)) continue;
                if (landed.Type(ctx.Schema)?.Parts is not { Binding.Count: > 0 } spec) continue;

                if (landed.Levels(part) is not { Count: > 0 } levels || levels.Any(l => l is null)) continue;
                if (levels.Any(l => spec.Binding.Contains(l, StringComparer.Ordinal))) continue;

                var stated = string.Join(", ", levels.Distinct(StringComparer.Ordinal));
                var binding = string.Join(" or ", spec.Binding);

                ctx.Warn(doc, NeedsNone,
                    $"`{Field}` names '{citation}', a {spec.Noun} stating {stated} and no {binding}, so departing "
                    + "from it needs no deviation. Drop the entry and give the reason in the change that departs, or "
                    + $"name the {spec.Noun} that binds.", doc.FrontStartLine);
            }
        }
    }
}
