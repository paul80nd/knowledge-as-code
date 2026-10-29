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

        var host = RepoBase.Read(root);

        foreach (var doc in ctx.Records)
        {
            var section = doc.Sections.FirstOrDefault(s =>
                string.Equals(s.Title, Heading, StringComparison.OrdinalIgnoreCase));
            if (section is null) continue;

            var declared = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            var misfits = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in doc.FrontList(Repos).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var parts = entry.Split('/');
                if (host.Misfit(parts) is not var (kind, form))
                {
                    declared.TryAdd(host.Key(parts), parts);
                    continue;
                }

                // A link to a repository of a misfit's name is not reported as unstated as well.
                misfits.Add(parts[^1]);
                ctx.Warn(doc, Reports,
                    $"'{Repos}: {entry}' does not fit `repos-base`, which {kind}. Write it as '{form}'.",
                    doc.FrontStartLine);
            }

            var linked = new Dictionary<string, (string[] Parts, int Line)>(StringComparer.OrdinalIgnoreCase);
            foreach (var link in doc.Links)
            {
                if (link.Position < section.BodyStart || link.Position >= section.BodyEnd) continue;
                if (host.Named(link.Target, declared) is not { } parts) continue;
                linked.TryAdd(host.Key(parts), (parts, link.Line));
            }

            foreach (var (key, parts) in declared.OrderBy(d => Entry(d.Value), StringComparer.OrdinalIgnoreCase))
            {
                if (linked.ContainsKey(key)) continue;
                ctx.Warn(doc, Reports,
                    $"'{Repos}: {Entry(parts)}' is not linked under '{Heading}'. Link it as {host.Url(parts)}, "
                    + $"or drop it from `{Repos}`.",
                    doc.FrontStartLine);
            }

            foreach (var (key, (parts, line)) in linked.OrderBy(l => l.Value.Line))
            {
                if (declared.ContainsKey(key) || misfits.Contains(parts[^1])) continue;
                ctx.Warn(doc, Reports,
                    $"'{Heading}' links {host.Url(parts)}, which `{Repos}` does not state. Add '{Entry(parts)}' "
                    + $"to `{Repos}`, or link a repository this service is changed in.",
                    line);
            }
        }
    }

    private static string Entry(string[] parts) => string.Join('/', parts);

    /// <summary>
    /// Where a corpus's <c>repos-base</c> points, and how an entry and a link resolve against it.
    /// </summary>
    private abstract class RepoBase
    {
        public static RepoBase Read(string root)
        {
            // Escaped once here, so a base written with a space still suggests a link that works in Markdown.
            root = Uri.TryCreate(root, UriKind.Absolute, out var uri) ? uri.AbsoluteUri : root;
            root = root.TrimEnd('/');
            return (RepoBase?)AzureDevOps.Parse(root) ?? new AnyHost(root);
        }

        /// <summary>The key an entry and a link to its repository share, compared case-insensitively.</summary>
        public abstract string Key(string[] parts);

        /// <summary>The link an entry resolves to, percent-encoded a segment at a time.</summary>
        public abstract string Url(string[] parts);

        /// <summary>The entry a link names, or null where it points outside the base.</summary>
        public abstract string[]? Named(string target, IReadOnlyDictionary<string, string[]> declared);

        /// <summary>
        /// What the base is and the form an entry takes against it, where the entry does not take that form.
        /// </summary>
        public virtual (string Kind, string Form)? Misfit(string[] parts) => null;

        protected static string Encode(IEnumerable<string> parts) =>
            string.Join('/', parts.Select(Uri.EscapeDataString));

        /// <summary>The path segments of a link, cut at its query and its fragment, each percent-decoded.</summary>
        protected static string[] Segments(string path)
        {
            var end = path.IndexOfAny(['#', '?']);
            if (end >= 0) path = path[..end];
            return path.Split('/', StringSplitOptions.RemoveEmptyEntries).Select(Uri.UnescapeDataString).ToArray();
        }
    }

    /// <summary>
    /// A host with no form of its own: an entry resolves to <c>&lt;base&gt;/&lt;entry&gt;</c> as written.
    /// </summary>
    private sealed class AnyHost(string root) : RepoBase
    {
        public override string Key(string[] parts) => Entry(parts);

        public override string Url(string[] parts) => $"{root}/{Encode(parts)}";

        public override string[]? Named(string target, IReadOnlyDictionary<string, string[]> declared)
        {
            var end = target.IndexOfAny(['#', '?']);
            var link = Uri.UnescapeDataString(end < 0 ? target : target[..end]);
            var prefix = Uri.UnescapeDataString(root) + "/";
            if (!link.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return null;

            var segments = link[prefix.Length..].Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0) return null;

            // A link reaching inside a repository runs on past its name, so the entry it names is the longest declared
            // prefix, and a link no entry names is read in the form this record's entries take.
            if (segments.Length > 1 && declared.ContainsKey(Entry(segments[..2]))) return segments[..2];
            if (declared.ContainsKey(segments[0])) return segments[..1];

            var width = segments.Length > 1 && declared.Values.Any(e => e.Length > 1) ? 2 : 1;
            return segments[..width];
        }
    }

    /// <summary>
    /// An Azure DevOps organisation, project or <c>_git</c> address, read from the host and the path. One
    /// organisation answers on <c>dev.azure.com/{org}</c> and on <c>{org}.visualstudio.com</c>, so a link on either
    /// host is read against the same base.
    /// </summary>
    private sealed class AzureDevOps(string root, AzureDevOps.Level level, string organisation, string? project)
        : RepoBase
    {
        public enum Level { Organisation, Project, Git }

        private const string Git = "_git";
        private const string Host = "dev.azure.com";
        private const string LegacyHost = ".visualstudio.com";

        public static AzureDevOps? Parse(string root)
        {
            if (Address(root) is not var (at, path)) return null;

            return path switch
            {
                [] => new AzureDevOps(root, Level.Organisation, at, null),
                [var p] when !IsGit(p) => new AzureDevOps(root, Level.Project, at, p),
                [var p, var g] when IsGit(g) && !IsGit(p) => new AzureDevOps(root, Level.Git, at, p),
                _ => null
            };
        }

        public override string Key(string[] parts) =>
            level == Level.Organisation ? Entry(parts) : $"{project}/{Entry(parts)}";

        public override (string Kind, string Form)? Misfit(string[] parts) => level switch
        {
            Level.Organisation when parts.Length != 2 => ("is an Azure DevOps organisation", "<project>/<repo>"),
            not Level.Organisation when parts.Length != 1 => ("names an Azure DevOps project", "<repo>"),
            _ => null
        };

        public override string Url(string[] parts) => level switch
        {
            Level.Organisation => $"{root}/{Encode(parts[..^1])}/{Git}/{Encode(parts[^1..])}",
            Level.Project => $"{root}/{Git}/{Encode(parts)}",
            _ => $"{root}/{Encode(parts)}"
        };

        public override string[]? Named(string target, IReadOnlyDictionary<string, string[]> declared)
        {
            if (Address(target) is not var (at, path)) return null;
            if (!string.Equals(at, organisation, StringComparison.OrdinalIgnoreCase)) return null;
            if (path is not [var p, var g, var repo, ..] || !IsGit(g)) return null;

            if (level == Level.Organisation) return [p, repo];
            return string.Equals(p, project, StringComparison.OrdinalIgnoreCase) ? [repo] : null;
        }

        /// <summary>The organisation an address is in and the decoded path below it, or null on another host.</summary>
        private static (string Organisation, string[] Path)? Address(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return null;

            var path = Segments(uri.AbsolutePath);
            if (string.Equals(uri.Host, Host, StringComparison.OrdinalIgnoreCase))
                return path is [var org, .. var rest] ? (org, rest) : null;

            return uri.Host.EndsWith(LegacyHost, StringComparison.OrdinalIgnoreCase)
                ? (uri.Host[..^LegacyHost.Length], path)
                : null;
        }

        private static bool IsGit(string segment) => string.Equals(segment, Git, StringComparison.OrdinalIgnoreCase);
    }
}
