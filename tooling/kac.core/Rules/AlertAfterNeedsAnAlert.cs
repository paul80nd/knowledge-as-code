namespace kac.core;

public sealed class AlertAfterNeedsAnAlert : ICorpusRule
{
    public RuleId RuleId => new("alert-after-needs-an-alert");

    private static readonly CheckId Reports = new("alert-after-needs-an-alert");

    public IReadOnlyList<CheckId> Emits => [Reports];

    // Renaming the field in `.schema/nfrs.yaml` means renaming it here, and nothing holds the two together.
    private const string AlertAfter = "alert-after";

    private const string MonitoringOutput = "monitoring-output";

    private static readonly HashSet<string> Emitting =
        new(["alert", "ticket"], StringComparer.OrdinalIgnoreCase);

    private const string ServicesFolder = "services";

    public void Check(CorpusRuleContext ctx)
    {
        if (PointingField(ctx.Type) is not { } appliesTo) return;

        foreach (var doc in ctx.Records)
        {
            if (doc.FrontScalar(AlertAfter) is not { Length: > 0 }) continue;

            var services = doc.FrontList(appliesTo.Name)
                .Where(t => !appliesTo.IsLiteral(t))
                .SelectMany(t => ServicesBehind(ctx, t))
                .ToList();

            if (services.Count == 0) continue;

            var stated = services.Select(s => s.FrontScalar(MonitoringOutput))
                .OfType<string>()
                .Where(o => o.Length > 0)
                .ToList();

            if (stated.Count != services.Count) continue;
            if (stated.Any(Emitting.Contains)) continue;

            var values = string.Join("' and '",
                stated.Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.Ordinal));

            ctx.Warn(doc, Reports,
                $"'{AlertAfter}' states a delay and nothing this binds raises an alert or a ticket: "
                + $"'{MonitoringOutput}' is '{values}'. Bind a service that raises one, or drop '{AlertAfter}'.",
                doc.FrontStartLine);
        }
    }

    // Answered only where the type declares exactly one such field. With two, which graph the walk follows would turn
    // on declaration order.
    private static FieldSpec? PointingField(TypeSchema t)
    {
        var found = t.FieldOrder.Select(n => t.Fields[n])
            .Where(f => f.DeclaresId && f.Refs.Contains(ServicesFolder, StringComparer.Ordinal))
            .Take(2)
            .ToList();

        return found.Count == 1 ? found[0] : null;
    }

    private static IEnumerable<Doc> ServicesBehind(CorpusRuleContext ctx, string id)
    {
        if (ctx.ById.GetValueOrDefault(id) is not { Type: { } type } doc) yield break;

        if (type.Key == ServicesFolder)
        {
            yield return doc;
            yield break;
        }

        if (PointingField(type) is not { } implementedBy) yield break;

        foreach (var name in doc.FrontList(implementedBy.Name))
            if (!implementedBy.IsLiteral(name)
                && ctx.ById.GetValueOrDefault(name) is { Type.Key: ServicesFolder } service)
                yield return service;
    }
}
