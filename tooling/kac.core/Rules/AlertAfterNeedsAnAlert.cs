namespace kac.core;

// `alert-after` is the delay an alert is configured with, so a target stating one is claiming an alert
// exists. Whether it does is a fact about the services the target binds, and no NFR states it.
//
// A class rather than an `expr:`, because the walk crosses two types: `applies-to` reaches an offering
// as often as a service, an offering emits nothing itself, and the services behind it are named by its
// own field. No expression reads a second document.
//
// Reported, never failed. A target can state the delay an alert will have before anybody builds it,
// and an error would make a record lie about the plan to stay green.
public sealed class AlertAfterNeedsAnAlert : ICorpusRule
{
    public RuleId RuleId => new("alert-after-needs-an-alert");

    private static readonly CheckId Reports = new("alert-after-needs-an-alert");

    public IReadOnlyList<CheckId> Emits => [Reports];

    // The field the rule is about. Renaming it in `.schema/nfrs.yaml` means renaming it here, and
    // nothing holds the two together.
    private const string AlertAfter = "alert-after";

    // The field on a service saying how a live problem is raised, and the values that raise one. Named
    // here because only this rule knows that `alert` and `ticket` are emitted by the system and that
    // `log` and `none` are not.
    private const string MonitoringOutput = "monitoring-output";

    private static readonly HashSet<string> Emitting =
        new(["alert", "ticket"], StringComparer.OrdinalIgnoreCase);

    // The folder the walk ends in, and the one both hops are held against.
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

            // An id nothing resolves is `ref-resolves`'s to report, and a target reaching no service at
            // all leaves nothing to judge.
            if (services.Count == 0) continue;

            var stated = services.Select(s => s.FrontScalar(MonitoringOutput))
                .OfType<string>()
                .Where(o => o.Length > 0)
                .ToList();

            // A service the field does not oblige to state one says nothing either way, and one silence
            // suppresses the warning rather than earning it.
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

    // A field of ids naming services. `applies-to` on an NFR and `implemented-by` on an offering are both
    // this shape, which is what lets one walk follow the pair. Read from the type rather than by name,
    // where the constants above are named: a rename of either reaches this file through the schema.
    //
    // Answered only where the type declares exactly one. A second would make which graph this walks
    // follow the order the fields were declared in, and pick one of them in silence.
    private static FieldSpec? PointingField(TypeSchema t)
    {
        var found = t.FieldOrder.Select(n => t.Fields[n])
            .Where(f => (f.Type == "id" || f is { Type: "list", Of: "id" })
                        && f.Refs.Contains(ServicesFolder, StringComparer.Ordinal))
            .Take(2)
            .ToList();

        return found.Count == 1 ? found[0] : null;
    }

    // Every service one target names, directly or through the offering that names it. An offering
    // reaching a second offering is not a shape any type declares, so the walk stops at one hop.
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
