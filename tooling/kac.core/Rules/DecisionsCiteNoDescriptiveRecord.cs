using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace kac.core;

public sealed class DecisionsCiteNoDescriptiveRecord : ICorpusRule
{
    public RuleId RuleId => new("decisions-cite-no-descriptive-record");

    private static readonly CheckId Cites = new("decision-cites-descriptive");

    public IReadOnlyList<CheckId> Emits => [Cites];

    /// <summary>Renaming a section in <c>.schema/adrs.yaml</c> means renaming it here.</summary>
    private static readonly string[] Asserting = ["Decision", "Consequences"];

    /// <summary>Renaming the tier in <c>.schema/_tiers.yaml</c> means renaming it here.</summary>
    private const string Descriptive = "descriptive";

    public void Check(CorpusRuleContext ctx)
    {
        var byRel = ctx.Docs.ToDictionary(d => d.Rel, StringComparer.Ordinal);

        foreach (var adr in ctx.Records.OrderBy(d => d.Rel, StringComparer.Ordinal))
        foreach (var section in adr.Sections.Where(s => Asserting.Contains(s.Title, StringComparer.OrdinalIgnoreCase)))
        foreach (var link in adr.Ast.Descendants<LinkInline>())
        {
            if (link.IsImage || link.Span.Start < section.BodyStart || link.Span.Start >= section.BodyEnd) continue;
            if (LinkChecks.Resolve(ctx.Tree, adr.Rel, link.Url ?? "") is not { } rel) continue;
            if (byRel.GetValueOrDefault(rel) is not { Type: { Tier: Descriptive } type } target) continue;

            ctx.Warn(adr, Cites,
                $"'{section.Title}' rests on '{target.Rel}', a {type.TypeName} record. A {type.TypeName} changes as "
                + "the estate does, and an ADR is not rewritten once accepted. State the constraint itself here, and "
                + $"cite the {type.TypeName} from Context or References.",
                link.Line + 1);
        }
    }
}
