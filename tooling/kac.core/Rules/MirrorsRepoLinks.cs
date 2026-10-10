namespace kac.core;

public sealed class MirrorsRepoLinks : ICorpusRule
{
    public RuleId RuleId => new("mirrors-repo-links");

    private static readonly CheckId Reports = new("mirrors-repo-links");

    public IReadOnlyList<CheckId> Emits => [Reports];

    private const string Repos = "repos";
    private const string Heading = "Where it lives";

    public void Check(CorpusRuleContext ctx)
    {
        if (ctx.ReposBase is not { Length: > 0 } root) return;

        // A base written with a trailing slash resolves to the same URL as one written without.
        var prefix = root.TrimEnd('/') + "/";

        foreach (var doc in ctx.Records)
        {
            var section = doc.Sections.FirstOrDefault(s =>
                string.Equals(s.Title, Heading, StringComparison.OrdinalIgnoreCase));
            if (section is null) continue;

            var declared = new HashSet<string>(doc.FrontList(Repos), StringComparer.OrdinalIgnoreCase);
            var linked = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var link in doc.Links)
            {
                if (link.Position < section.BodyStart || link.Position >= section.BodyEnd) continue;
                if (Named(link.Target, prefix) is not { } name) continue;
                linked.TryAdd(name, link.Line);
            }

            foreach (var name in declared.Order(StringComparer.OrdinalIgnoreCase))
            {
                if (linked.ContainsKey(name)) continue;
                ctx.Warn(doc, Reports,
                    $"'{Repos}: {name}' is not linked under '{Heading}'. Link it as {prefix}{name}, "
                    + $"or drop it from `{Repos}`.",
                    doc.FrontStartLine);
            }

            foreach (var (name, line) in linked.OrderBy(l => l.Value))
            {
                if (declared.Contains(name)) continue;
                ctx.Warn(doc, Reports,
                    $"'{Heading}' links {prefix}{name}, which `{Repos}` does not state. Add '{name}' to "
                    + $"`{Repos}`, or link a repository this service is changed in.",
                    line);
            }
        }
    }

    private static string? Named(string target, string prefix)
    {
        if (!target.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return null;

        var rest = target[prefix.Length..];
        var end = rest.IndexOfAny(['/', '#', '?']);
        var name = end < 0 ? rest : rest[..end];
        return name.Length == 0 ? null : name;
    }
}
