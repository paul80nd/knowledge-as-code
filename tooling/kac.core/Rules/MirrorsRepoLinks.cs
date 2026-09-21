namespace kac.core;

// A `repos` entry is a bare name and `Where it lives` states the same repository as a link a person
// follows, so the two are one fact written twice. `repos-base` in `.corpus.yaml` is what joins them:
// `<base>/<entry>` is the URL an entry resolves to, and this rule reconciles the resolved set against
// the links under the heading, in both directions.
//
// `mirrors-section:` cannot ask this. It reconciles ids, and a repository name resolves to no record.
//
// Reported, never failed, for the reason `feature-file-repo` is: a service can sit in a repository
// outside the estate's usual host, and an error would make one outlier drop `repos-base` for every
// other record.
public sealed class MirrorsRepoLinks : ICorpusRule
{
    public RuleId RuleId => new("mirrors-repo-links");

    private static readonly CheckId Reports = new("mirrors-repo-links");

    public IReadOnlyList<CheckId> Emits => [Reports];

    // The field of repository names, and the heading their links are written under. Named here rather
    // than read from the type: neither has a key saying that one states the other, and only this rule
    // knows that a name under `repos` is the last segment of a URL under the heading.
    private const string Repos = "repos";
    private const string Heading = "Where it lives";

    public void Check(CorpusRuleContext ctx)
    {
        // A corpus stating no base gives an entry nothing to resolve against, so there is no URL to
        // compare a link with.
        if (ctx.ReposBase is not { Length: > 0 } root) return;

        // The separator is added here, so a base written with a trailing slash resolves to the same URL
        // as one written without rather than warning on every record in the corpus.
        var prefix = root.TrimEnd('/') + "/";

        foreach (var doc in ctx.Records)
        {
            // A record missing the heading is `required-section`'s to report. Comparing against a
            // section that is not there would report every entry the record states, twice over.
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

    // The repository a URL under the base addresses, or null where the URL is not under it. Everything
    // past the name is a path inside the repository, which is what a link stating a file or a folder
    // carries and what the entry never states.
    private static string? Named(string target, string prefix)
    {
        if (!target.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return null;

        var rest = target[prefix.Length..];
        var end = rest.IndexOfAny(['/', '#', '?']);
        var name = end < 0 ? rest : rest[..end];
        return name.Length == 0 ? null : name;
    }
}
