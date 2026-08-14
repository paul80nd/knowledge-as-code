namespace kac.core;

// A dependency graph is written one record at a time, and each edge is reasonable where it is written.
// A loop only exists in the set of them, so nobody adding an edge is in a position to see it — which is
// the whole case for asking the question here rather than in review.
//
// Reported, never failed. Some loops are real: two services that call each other are a fact about the
// estate, and a check that refused them would be asking the corpus to lie about what is deployed. What
// the warning is for is the loop nobody meant, and the message names the records on it so the reader
// can decide which of the two it is.
//
// The graph is whichever field points back at the type that declares the rule, read from the schema
// rather than named here: a rule holding the string `depends-on` would walk nothing at all the day the
// field is renamed, and say so in silence.
public sealed class NoDependencyCycles : ICorpusRule
{
    public RuleId RuleId => new("no-dependency-cycles");

    // A field because every report below names it: one place to change, so what the rule declares it
    // emits and what it actually reports cannot come apart. `_checks.yaml` declares what it means.
    private static readonly CheckId Reports = new("dependency-cycle");

    public IReadOnlyList<CheckId> Emits => [Reports];

    public void Check(CorpusRuleContext ctx)
    {
        foreach (var name in ctx.Type.FieldOrder)
        {
            var field = ctx.Type.Fields[name];
            if (PointsAtOwnType(field, ctx.Type)) Walk(ctx, field);
        }
    }

    // A field whose ids name documents of the type that carries it. Each such field is its own graph and
    // is walked on its own, so a message names one relation rather than a path stitched from two.
    private static bool PointsAtOwnType(FieldSpec f, TypeSchema t)
        => (f.Type == "id" || f is { Type: "list", Of: "id" })
           && f.Refs.Contains(t.Key, StringComparer.Ordinal);

    private static void Walk(CorpusRuleContext ctx, FieldSpec field)
    {
        // Ids as their own documents write them. A second document claiming an id is `id-unique`'s to
        // report, and is left out here rather than given a second node with the same name.
        var records = new Dictionary<string, Doc>(StringComparer.OrdinalIgnoreCase);
        foreach (var d in ctx.Records)
            if (d.FrontScalar("id") is { Length: > 0 } id)
                records.TryAdd(id, d);

        // An edge is kept only where both ends are records of this type: an id nothing carries is
        // `ref-resolves`'s to report, and a literal the field admits is not an id at all. Targets are
        // canonicalised through the document they name, so a message quotes the id as the corpus writes
        // it however the edge spelled it.
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

        // A depth-first walk, entering nodes in id order so the same corpus is always walked the same
        // way. Every cycle leaves at least one edge back into the path being walked, so following those
        // finds each looping group of records; a group reachable by more than one of them is recognised
        // as one cycle below rather than reported twice.
        var state = new Dictionary<string, int>(StringComparer.Ordinal); // 0 unseen, 1 on the path, 2 done
        var path = new List<string>();
        var reported = new HashSet<string>(StringComparer.Ordinal);

        foreach (var id in edges.Keys.Order(StringComparer.Ordinal))
            Visit(id);

        void Visit(string id)
        {
            if (state.GetValueOrDefault(id) != 0) return;

            state[id] = 1;
            path.Add(id);

            foreach (var next in edges[id])
                if (state.GetValueOrDefault(next) == 1) Report(next);
                else Visit(next);

            path.RemoveAt(path.Count - 1);
            state[id] = 2;
        }

        void Report(string target)
        {
            var cycle = path[path.IndexOf(target)..];

            // Rotated to open at the lowest id, so which record carries the warning follows from the
            // cycle rather than from where the walk entered it — and so the same cycle reached twice is
            // written the same way, which is what lets it be recognised as one.
            var lowest = cycle.IndexOf(cycle.Min(StringComparer.Ordinal)!);
            List<string> rotated = [.. cycle[lowest..], .. cycle[..lowest]];

            var route = string.Join(" → ", rotated.Append(rotated[0]));
            if (!reported.Add(route)) return;

            var at = records[rotated[0]];
            ctx.Warn(at, Reports, $"'{field.Name}' forms a cycle: {route}.", at.FrontStartLine);
        }
    }
}
