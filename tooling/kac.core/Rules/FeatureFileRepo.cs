namespace kac.core;

// A feature file path is a plain string, so `ref-resolves` never reads it and nothing resolves it until
// `feature-file-orphans` sweeps the code repositories. Its first segment is the one half this corpus can
// answer for on its own: the services are already named in the same record, and each of them lists the
// repositories it lives in.
//
// Reported, never failed. A regression pack can live in a repository no service claims, and refusing that
// would ask the corpus to lie about where the tests are.
public sealed class FeatureFileRepo : ICorpusRule
{
    public RuleId RuleId => new("feature-file-repo");

    private static readonly CheckId Reports = new("feature-file-repo");

    public IReadOnlyList<CheckId> Emits => [Reports];

    // The field of paths, and the field a service lists its repositories under. Named here rather than read
    // from the type: neither has a key saying what its strings mean, and only this rule knows that the
    // first segment of one is a value of the other.
    private const string Paths = "feature-files";
    private const string Repos = "repos";

    public void Check(CorpusRuleContext ctx)
    {
        // The services this record implements, read from whichever field points at them. A rule holding
        // the string `implemented-by` would walk nothing the day the field is renamed, and say so in
        // silence.
        var services = ctx.Type.FieldOrder
            .Select(name => ctx.Type.Fields[name])
            .FirstOrDefault(f =>
                f is { Type: "list", Of: "id" } && f.Refs.Contains("services", StringComparer.Ordinal));
        if (services is null) return;

        foreach (var doc in ctx.Records)
        {
            var repos = Repositories(ctx, doc, services);

            // A record whose services name no repository between them has nothing to compare against.
            // Saying so for every path would report the services' silence once per feature file.
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

    // The repositories of the services a record implements. A dangling id is `ref-resolves`'s to report and
    // contributes nothing here, as does a service that states no repository.
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
