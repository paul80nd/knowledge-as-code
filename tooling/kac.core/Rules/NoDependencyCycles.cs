namespace kac.core;

// The graph is whichever field points back at the type declaring the rule, read from the schema, so a rename of
// `depends-on` cannot leave the rule walking nothing.
public sealed class NoDependencyCycles : ICorpusRule
{
    public RuleId RuleId => new("no-dependency-cycles");

    private static readonly CheckId Reports = new("dependency-cycle");

    public IReadOnlyList<CheckId> Emits => [Reports];

    public void Check(CorpusRuleContext ctx)
    {
        foreach (var name in ctx.Type.FieldOrder)
        {
            var field = ctx.Type.Fields[name];
            if (field.PointsAt(ctx.Type)) Walk(ctx, field);
        }
    }

    // `Unseen` is the default, so a record the walk has no entry for yet reads as unseen.
    private enum Step
    {
        Unseen,
        OnPath,
        Done
    }

    private static void Walk(CorpusRuleContext ctx, FieldSpec field)
    {
        var records = new Dictionary<string, Doc>(StringComparer.OrdinalIgnoreCase);
        foreach (var d in ctx.Records)
            if (d.FrontScalar("id") is { Length: > 0 } id)
                records.TryAdd(id, d);

        var edges = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var (id, doc) in records)
            edges[id] =
            [
                .. doc.FrontList(field.Name)
                    .Where(t => !field.IsLiteral(t))
                    .Select(t => records.GetValueOrDefault(t)?.FrontScalar("id"))
                    .OfType<string>()
                    .Order(StringComparer.Ordinal)
            ];

        // Entered in id order, so the same corpus is always walked the same way.
        var state = new Dictionary<string, Step>(StringComparer.Ordinal);
        var path = new List<string>();
        var reported = new HashSet<string>(StringComparer.Ordinal);

        foreach (var id in edges.Keys.Order(StringComparer.Ordinal))
            Visit(id);
        return;

        void Visit(string id)
        {
            if (state.GetValueOrDefault(id) != Step.Unseen) return;

            state[id] = Step.OnPath;
            path.Add(id);

            foreach (var next in edges[id])
                if (state.GetValueOrDefault(next) == Step.OnPath) Report(next);
                else Visit(next);

            path.RemoveAt(path.Count - 1);
            state[id] = Step.Done;
        }

        void Report(string target)
        {
            var cycle = path[path.IndexOf(target)..];

            // Rotated to open at the lowest id, so a cycle reached twice is written the same way and reported once.
            var lowest = cycle.IndexOf(cycle.Order(StringComparer.Ordinal).First());
            List<string> rotated = [.. cycle[lowest..], .. cycle[..lowest]];

            var route = string.Join(" → ", rotated.Append(rotated[0]));
            if (!reported.Add(route)) return;

            var at = records[rotated[0]];
            ctx.Warn(at, Reports, $"'{field.Name}' forms a cycle: {route}.", at.FrontStartLine);
        }
    }
}
