namespace kac.core;

// How a publishing target addresses what it publishes: the rule joining a base to a record's path, and the
// anchor rule for a part inside it. Only the address a person opens is built. An agent reads the source
// through a client that authenticates to the target. See `docs/design/export.md`.
public sealed class Publishing
{
    public const string AzureDevOps = "azure-devops";
    public const string AzureDevOpsWiki = "azure-devops-wiki";
    public const string GitHub = "github";
    public const string MkDocs = "mkdocs";
    public const string None = "none";

    // Every target a descriptor may name. `new` refuses any other at `--publishing`.
    public static readonly IReadOnlyList<string> Targets =
        [AzureDevOps, AzureDevOpsWiki, GitHub, MkDocs, None];

    public const string PathToken = "{path}";
    public const string AnchorToken = "{anchor}";

    // The wiki identifier after this is the last segment a wiki base may carry.
    private const string WikiSegment = "/_wiki/wikis/";

    // `mkdocs` is named by the descriptor and addressed by nothing, so a corpus on it exports without links.
    private static readonly IReadOnlyList<string> Addressable = [AzureDevOps, AzureDevOpsWiki, GitHub];

    private readonly string _base;

    // With its own leading slash, or empty where the corpus is the repository.
    private readonly string _prefix;

    public required string Target { get; init; }

    // The commit every link resolves against, so a link reads the corpus as it was exported.
    //
    // `azure-devops-wiki` is the exception it cannot help. No `?pagePath=` URL takes a commit, so a
    // person following that target's link reads whatever the wiki holds now. An agent still reads the
    // pinned version, because the ref reaches it through the export rather than through the link.
    public required string Ref { get; init; }

    // Without a leading slash, or null where the corpus is the repository. Carried separately as well as baked
    // into the template, because an agent joins it to a record's path.
    public string? PathPrefix => _prefix is { Length: > 0 } ? _prefix[1..] : null;

    private Publishing(string publishedBase, string? pathPrefix)
    {
        _base = publishedBase.TrimEnd('/');
        _prefix = pathPrefix?.Trim('/') is { Length: > 0 } p ? "/" + p : "";
    }

    // Null where the corpus has no addressable published form: no target, a target nothing builds links for, no
    // base, a base the target cannot join, or no `gitRef`.
    public static Publishing? For(CorpusDescriptor descriptor, string? gitRef)
    {
        if (descriptor.PublishingTarget is not { } target) return null;
        if (!Addressable.Contains(target, StringComparer.Ordinal)) return null;
        if (descriptor.Base is not { Length: > 0 } published) return null;
        if (gitRef is not { Length: > 0 }) return null;
        if (!Joinable(target, published)) return null;

        return new Publishing(published, descriptor.PathPrefix) { Target = target, Ref = gitRef };
    }

    // Only the wiki has anything to check. Azure DevOps assigns a page a numeric id when it is first
    // created and shows that id in the URL, so a base copied out of the address bar carries one and
    // names a page rather than the wiki. Nothing derives the id of a second page from the id of the
    // first, so a base carrying one addresses exactly one record and silently misaddresses the rest.
    private static bool Joinable(string target, string published)
    {
        if (!target.Equals(AzureDevOpsWiki, StringComparison.Ordinal)) return true;

        var at = published.IndexOf(WikiSegment, StringComparison.OrdinalIgnoreCase);
        if (at < 0) return false;

        var identifier = published[(at + WikiSegment.Length)..].TrimEnd('/');
        return identifier.Length > 0 && !identifier.Contains('/');
    }

    // The base a repository's origin implies, or null where nothing here can derive one. SSH and HTTPS remotes
    // are both read. `azure-devops-wiki` is absent, because a remote says nothing about which wiki publishes it.
    public static string? BaseFrom(string target, string? origin)
    {
        if (origin?.Trim() is not { Length: > 0 } url) return null;

        var repo = url.EndsWith(".git", StringComparison.Ordinal) ? url[..^4] : url;

        return target switch
        {
            GitHub => GitHubBase(repo),
            AzureDevOps => AzureDevOpsBase(repo),
            _ => null
        };
    }

    private static string? GitHubBase(string repo)
    {
        var at = repo.IndexOf("github.com", StringComparison.OrdinalIgnoreCase);
        if (at < 0) return null;

        var path = Rest(repo, at + "github.com".Length);
        if (path.Count(c => c == '/') != 1 || path.Split('/').Any(p => p.Length == 0)) return null;

        return $"https://github.com/{path}";
    }

    // Azure DevOps spells its two remotes differently enough that neither reduces to the other. An SSH
    // remote opens with the protocol version and names the repository last; an HTTPS one carries `_git`
    // in the position the SSH form leaves out, and may prefix the host with a user name.
    private static string? AzureDevOpsBase(string repo)
    {
        var at = repo.IndexOf("dev.azure.com", StringComparison.OrdinalIgnoreCase);
        if (at < 0) return null;

        var parts = Rest(repo, at + "dev.azure.com".Length).Split('/');
        if (parts.Length != 4 || parts.Any(p => p.Length == 0)) return null;

        return parts switch
        {
            ["v3", var org, var project, var name] => $"https://dev.azure.com/{org}/{project}/_git/{name}",
            [var org, var project, "_git", var name] => $"https://dev.azure.com/{org}/{project}/_git/{name}",
            _ => null
        };
    }

    private static string Rest(string repo, int from) =>
        repo[from..].TrimStart(':', '/').TrimEnd('/');

    // The anchor a link uses to land on a part. GitHub derives it from the heading by discarding the
    // punctuation it meets, which is the form `Md.Slug` writes and the form the corpus's own links
    // already use.
    //
    // Azure DevOps percent-encodes that punctuation, so a heading with `/`, `:` or `.` gets the wrong anchor
    // there. Its rule is unverified, so the corpus writes a linked heading without punctuation. See `Md.cs`.
    public static string Anchor(string heading) => Md.Slug(heading);

    // The link rule as one string with `{path}` and `{anchor}` left to fill. The ref and the path prefix are
    // baked in, as `docs/design/export.md` argues. An Azure DevOps wiki takes the path with `.md` removed and its
    // separators encoded, which `Link` does and `docs/corpus-descriptor.md` states.
    public string Template() => Target switch
    {
        GitHub => $"{_base}/blob/{Ref}{_prefix}/{PathToken}#{AnchorToken}",
        AzureDevOps => $"{_base}?path={_prefix}/{PathToken}&version=GC{Ref}#{AnchorToken}",
        AzureDevOpsWiki => $"{_base}?pagePath={Encode(_prefix)}%2F{PathToken}&anchor={AnchorToken}",
        _ => throw new InvalidOperationException($"'{Target}' builds no link.")
    };

    // Built by substituting into `Template`, so this and a consumer's own link for a record are the same string.
    public string Link(string relPath, string? anchor = null)
    {
        var template = Template();

        var human = anchor is { Length: > 0 }
            ? template.Replace(AnchorToken, anchor, StringComparison.Ordinal)
            : template.Replace($"{AnchorMark}{AnchorToken}", "", StringComparison.Ordinal);

        return human.Replace(PathToken, PathFor(relPath), StringComparison.Ordinal);
    }

    // A wiki takes a query parameter because `?pagePath=` has already opened the query string.
    private string AnchorMark =>
        Target.Equals(AzureDevOpsWiki, StringComparison.Ordinal) ? "&anchor=" : "#";

    // A record's path as this target spells it. A wiki page is the file without its extension, and every
    // separator inside a `pagePath` is encoded, the leading one included.
    private string PathFor(string relPath) =>
        Target.Equals(AzureDevOpsWiki, StringComparison.Ordinal)
            ? Encode(relPath.EndsWith(".md", StringComparison.Ordinal) ? relPath[..^3] : relPath)
            : relPath;

    private static string Encode(string path) =>
        path.Replace("/", "%2F", StringComparison.Ordinal);
}
