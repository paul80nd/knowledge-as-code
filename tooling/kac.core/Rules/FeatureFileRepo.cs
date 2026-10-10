namespace kac.core;

public sealed class FeatureFileRepo : ICorpusRule
{
    public RuleId RuleId => new("feature-file-repo");

    private static readonly CheckId Reports = new("feature-file-repo");

    public IReadOnlyList<CheckId> Emits => [Reports];

    private const string Paths = "feature-files";
    private const string Repos = "repos";

    public void Check(CorpusRuleContext ctx)
    {
        // Read from whichever field points at services, so a rename of `implemented-by` reaches here through the
        // schema.
        var services = ctx.Type.FieldOrder
            .Select(name => ctx.Type.Fields[name])
            .FirstOrDefault(f =>
                f is { Type: "list", Of: "id" } && f.Refs.Contains("services", StringComparer.Ordinal));
        if (services is null) return;

        foreach (var doc in ctx.Records)
        {
            var repos = Repositories(ctx, doc, services);

            if (repos.Count == 0) continue;

            foreach (var path in doc.FrontList(Paths))
            {
                var first = path.Split('/')[0];
                if (repos.Contains(first)) continue;

                var known = string.Join(", ", repos.Order(StringComparer.OrdinalIgnoreCase));
                ctx.Warn(doc, Reports,
                    $"'{Paths}: {path}' starts in '{first}', which is no repository of a service this "
                    + $"record implements. Those services are in: {known}.",
                    doc.FrontStartLine);
            }
        }
    }

    private static HashSet<string> Repositories(CorpusRuleContext ctx, Doc doc, FieldSpec services)
    {
        var repos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var id in doc.FrontList(services.Name))
        {
            if (services.IsLiteral(id)) continue;
            if (!ctx.ById.TryGetValue(id, out var service)) continue;
            foreach (var repo in service.FrontList(Repos)) repos.Add(repo);
        }

        return repos;
    }
}
