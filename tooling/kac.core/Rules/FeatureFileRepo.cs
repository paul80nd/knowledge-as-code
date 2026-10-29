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

            var entries = repos.Select(repo => repo.Split('/')).ToList();

            foreach (var path in doc.FrontList(Paths))
            {
                var segments = path.Split('/');
                if (entries.Any(entry => Opens(segments, entry))) continue;

                // A second segment is a repository only after a group some entry names, and a folder anywhere else.
                var grouped = segments.Length > 1 && entries.Any(entry =>
                    entry.Length > 1 && string.Equals(entry[0], segments[0], StringComparison.OrdinalIgnoreCase));
                var opening = string.Join('/', segments[..(grouped ? 2 : 1)]);

                var known = string.Join(", ", repos.Order(StringComparer.OrdinalIgnoreCase));
                ctx.Warn(doc, Reports,
                    $"'{Paths}: {path}' starts in '{opening}', which is no repository of a service this "
                    + $"record implements. Those services are in: {known}.",
                    doc.FrontStartLine);
            }
        }
    }

    /// <summary>
    /// Whether a path opens with every part of a <c>repos</c> entry, so <c>&lt;group&gt;/&lt;repo&gt;</c> takes two
    /// segments and <c>&lt;repo&gt;</c> one.
    /// </summary>
    private static bool Opens(string[] segments, string[] entry) =>
        entry.Length <= segments.Length
        && entry.Zip(segments).All(p => string.Equals(p.First, p.Second, StringComparison.OrdinalIgnoreCase));

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
