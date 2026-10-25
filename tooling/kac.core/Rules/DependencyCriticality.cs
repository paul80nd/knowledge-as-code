using YamlDotNet.RepresentationModel;

namespace kac.core;

/// <summary>
/// Both fields are read from the schema, so a rename of <c>criticality</c> or <c>depends-on</c> cannot leave the rule
/// walking nothing. See docs/framework/shaping-a-type.md for why this warns rather than fails.
/// </summary>
public sealed class DependencyCriticality : ICorpusRule
{
    public RuleId RuleId => new("dependency-criticality");

    private static readonly CheckId Reports = new("dependency-criticality");

    public IReadOnlyList<CheckId> Emits => [Reports];

    public void Check(CorpusRuleContext ctx)
    {
        var declared = ctx.Type.FieldOrder.Select(n => ctx.Type.Fields[n]).ToList();

        foreach (var graded in declared.Where(f => f.Ordered))
        foreach (var edge in declared.Where(f => f.PointsAt(ctx.Type)))
            Walk(ctx, graded, edge);
    }

    private static void Walk(CorpusRuleContext ctx, FieldSpec graded, FieldSpec edge)
    {
        foreach (var doc in ctx.Records)
        {
            if (doc.FrontScalar(graded.Name) is not { } ours || graded.Rank(ours) is not { } here) continue;

            foreach (var item in Entries(doc, edge.Name))
            {
                if (item is not YamlScalarNode { Value: { Length: > 0 } target }) continue;
                if (edge.IsLiteral(target)) continue;

                if (!ctx.ById.TryGetValue(target, out var to) || to.Type != ctx.Type) continue;
                if (to.FrontScalar(graded.Name) is not { } theirs || graded.Rank(theirs) is not { } there)
                    continue;

                // `Rank` is a position, so the most significant value has the lowest one.
                if (here >= there) continue;

                ctx.Warn(doc, Reports,
                    $"'{edge.Name}' cites '{target}', graded '{theirs}' where this record is '{ours}'. "
                    + "Regrade one of the two, or say what degrades when it is gone and accept this warning in "
                    + "`accepts:`.",
                    Yaml.LineOf(item, doc.FrontStartLine));
            }
        }
    }

    private static IEnumerable<YamlNode> Entries(Doc doc, string field) =>
        doc.FrontNode(field) switch
        {
            YamlSequenceNode seq => seq.Children,
            YamlScalarNode one => [one],
            _ => []
        };
}
