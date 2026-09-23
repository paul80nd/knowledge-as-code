using YamlDotNet.RepresentationModel;

namespace kac.core;

// Nothing is more dependable than what it calls. A record graded above something it depends on is either
// mis-graded or hides a degradation nobody wrote down, and the record stating the edge cannot show which:
// the grade at the far end sits on another document. Reported, never failed: the edge is often a
// degradation the estate accepts, and the warning asks for the sentence that says so.
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
        // Every pairing, because the grading field and the edge are separate declarations and a type may
        // declare more than one of either. Each message states both fields, so two graded fields give
        // two findings.
        foreach (var graded in Declared(ctx).Where(f => f.Ordered))
        foreach (var edge in Declared(ctx).Where(f => PointsAtOwnType(f, ctx.Type)))
            Walk(ctx, graded, edge);
    }

    private static IEnumerable<FieldSpec> Declared(CorpusRuleContext ctx) =>
        ctx.Type.FieldOrder.Select(n => ctx.Type.Fields[n]);

    // A field whose ids point at documents of its own type, read as `no-dependency-cycles` reads it.
    private static bool PointsAtOwnType(FieldSpec f, TypeSchema t)
        => f.DeclaresId && f.Refs.Contains(t.Key, StringComparer.Ordinal);

    private static void Walk(CorpusRuleContext ctx, FieldSpec graded, FieldSpec edge)
    {
        foreach (var doc in ctx.Records)
        {
            // A missing grade, and one outside the range, belong to the field's own checks. Comparing
            // against either would turn one malformed value into a finding on every record citing it.
            if (doc.FrontScalar(graded.Name) is not { } ours || graded.Rank(ours) is not { } here) continue;

            foreach (var target in doc.FrontList(edge.Name))
            {
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
                    LineOf(doc, edge.Name, target));
            }
        }
    }

    // The line the entry sits on, so a finding about one entry lands on it. `FrontList` reads the values;
    // this looks one up again only to place the finding. A field written as anything but a sequence falls
    // back to the frontmatter.
    private static int? LineOf(Doc doc, string field, string value) =>
        doc.FrontNode(field) is YamlSequenceNode seq
        && seq.Children.OfType<YamlScalarNode>()
            .FirstOrDefault(c => string.Equals(c.Value, value, StringComparison.Ordinal)) is { } at
            ? Yaml.LineOf(at, doc.FrontStartLine)
            : doc.FrontStartLine;
}
