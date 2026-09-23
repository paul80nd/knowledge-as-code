using YamlDotNet.RepresentationModel;

namespace kac.core;

// A record graded above something it depends on is either mis-graded or hides a degradation nobody wrote
// down, and the record stating the edge cannot show which: the grade at the far end sits on another
// document. See docs/design/shaping-a-type.md for why this warns rather than fails.
//
// Both fields come from the schema. The grading field is whichever one draws on an enum declaring
// `ordered: true`, and the edge is whichever field points back at the type's own records, as
// `no-dependency-cycles` reads its graph. A rule spelling `criticality` or `depends-on` in its own code
// would walk nothing the day either is renamed, and say so in silence.
public sealed class DependencyCriticality : ICorpusRule
{
    public RuleId RuleId => new("dependency-criticality");

    private static readonly CheckId Reports = new("dependency-criticality");

    public IReadOnlyList<CheckId> Emits => [Reports];

    public void Check(CorpusRuleContext ctx)
    {
        var declared = ctx.Type.FieldOrder.Select(n => ctx.Type.Fields[n]).ToList();

        // Every pairing, because the grading field and the edge are separate declarations and a type may
        // declare more than one of either. Each message states both fields, so two graded fields give
        // two findings.
        foreach (var graded in declared.Where(f => f.Ordered))
        foreach (var edge in declared.Where(f => f.PointsAt(ctx.Type)))
            Walk(ctx, graded, edge);
    }

    private static void Walk(CorpusRuleContext ctx, FieldSpec graded, FieldSpec edge)
    {
        foreach (var doc in ctx.Records)
        {
            // A missing grade, and one outside the range, belong to the field's own checks. Comparing
            // against either would turn one malformed value into a finding on every record citing it.
            if (doc.FrontScalar(graded.Name) is not { } ours || graded.Rank(ours) is not { } here) continue;

            foreach (var item in Entries(doc, edge.Name))
            {
                if (item is not YamlScalarNode { Value: { Length: > 0 } target }) continue;
                if (edge.IsLiteral(target)) continue;

                // `ref-resolves` reports an id nothing declares, and one resolving to another type. Either
                // way there is no grade at the far end.
                if (!ctx.ById.TryGetValue(target, out var to) || to.Type != ctx.Type) continue;
                if (to.FrontScalar(graded.Name) is not { } theirs || graded.Rank(theirs) is not { } there)
                    continue;

                // Positions, so the most significant value has the lowest one. This record outranks the
                // target when its position is lower.
                if (here >= there) continue;

                ctx.Warn(doc, Reports,
                    $"'{edge.Name}' cites '{target}', graded '{theirs}' where this record is '{ours}'. "
                    + "Regrade one of the two, or write down what degrades when it is gone.",
                    Yaml.LineOf(item, doc.FrontStartLine));
            }
        }
    }

    // The field's entries as nodes, so a finding about one entry lands on the line that entry sits on. A
    // scalar is the one-entry case, which is how `Doc.FrontList` reads one.
    private static IEnumerable<YamlNode> Entries(Doc doc, string field) =>
        doc.FrontNode(field) switch
        {
            YamlSequenceNode seq => seq.Children,
            YamlScalarNode one => [one],
            _ => []
        };
}
