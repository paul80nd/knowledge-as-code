namespace kac.core;

/// <summary>
/// How a publishing target addresses what it publishes: the rule joining a base to a record's path, and the anchor rule
/// for a part inside it. Only the address a person opens is built. An agent reads the source through a client that
/// authenticates to the target. See <c>docs/reference/export.md</c>.
/// </summary>
public sealed class Publishing
{
    public const string AzureDevOps = "azure-devops";
    public const string AzureDevOpsWiki = "azure-devops-wiki";
    public const string GitHub = "github";
    public const string MkDocs = "mkdocs";
    public const string None = "none";

    /// <summary>Every target a descriptor may name. <c>new</c> refuses any other at <c>--publishing</c>.</summary>
    public static readonly IReadOnlyList<string> Targets =
        [AzureDevOps, AzureDevOpsWiki, GitHub, MkDocs, None];

    public const string PathToken = "{path}";
    public const string AnchorToken = "{anchor}";

    /// <summary>The wiki identifier after this is the last segment a wiki base may carry.</summary>
    private const string WikiSegment = "/_wiki/wikis/";

    /// <summary>
    /// <c>mkdocs</c> is named by the descriptor and addressed by nothing, so a corpus on it exports without links.
    /// </summary>
    private static readonly IReadOnlyList<string> Addressable = [AzureDevOps, AzureDevOpsWiki, GitHub];

    private readonly string _base;

    /// <summary>With its own leading slash, or empty where the corpus is the repository.</summary>
    private readonly string _prefix;

    public required string Target { get; init; }

    /// <summary>
    /// The commit every link resolves against, so a link reads the corpus as exported. No <c>?pagePath=</c> URL takes
    /// one, so an <c>azure-devops-wiki</c> link shows the wiki as it is now; an agent still reads the pinned version.
    /// </summary>
    public required string Ref { get; init; }

    /// <summary>
    /// Without a leading slash, or null where the corpus is the repository. Carried separately as well as baked into
    /// the template, because an agent joins it to a record's path.
    /// </summary>
    public string? PathPrefix => _prefix is { Length: > 0 } ? _prefix[1..] : null;

    private Publishing(string publishedBase, string? pathPrefix)
    {
        _base = publishedBase.TrimEnd('/');
        _prefix = pathPrefix?.Trim('/') is { Length: > 0 } p ? "/" + p : "";
    }

    /// <summary>
    /// Null where the corpus has no addressable published form: no target, a target nothing builds links for, no base,
    /// a base the target cannot join, or no <c>gitRef</c>.
    /// </summary>
    public static Publishing? For(CorpusDescriptor descriptor, string? gitRef)
    {
        if (descriptor.PublishingTarget is not { } target) return null;
        if (!Addressable.Contains(target, StringComparer.Ordinal)) return null;
        if (descriptor.Base is not { Length: > 0 } published) return null;
        if (gitRef is not { Length: > 0 }) return null;
        if (!Joinable(target, published)) return null;

        return new Publishing(published, descriptor.PathPrefix) { Target = target, Ref = gitRef };
    }

    /// <summary>
    /// Only the wiki has anything to check. A base copied from the address bar carries the numeric id Azure DevOps gave
    /// one page, and nothing derives another page's id from it, so it addresses one record and misaddresses the rest.
    /// </summary>
    private static bool Joinable(string target, string published)
    {
        if (!target.Equals(AzureDevOpsWiki, StringComparison.Ordinal)) return true;

        var at = published.IndexOf(WikiSegment, StringComparison.OrdinalIgnoreCase);
        if (at < 0) return false;

        var identifier = published[(at + WikiSegment.Length)..].TrimEnd('/');
        return identifier.Length > 0 && !identifier.Contains('/');
    }

    /// <summary>
    /// The base a repository's origin implies, or null where nothing here can derive one. SSH and HTTPS remotes are
    /// both read. <c>azure-devops-wiki</c> is absent, because a remote says nothing about which wiki publishes it.
    /// </summary>
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

    /// <summary>
    /// Azure DevOps spells its two remotes differently enough that neither reduces to the other. An SSH remote opens
    /// with the protocol version and names the repository last; an HTTPS one carries <c>_git</c> in the position the
    /// SSH form leaves out, and may prefix the host with a user name.
    /// </summary>
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

    /// <summary>
    /// The anchor a link uses to land on a part, as GitHub derives it and <c>Md.Slug</c> writes it. Azure DevOps
    /// percent-encodes punctuation, so a heading with <c>/</c>, <c>:</c> or <c>.</c> gets the wrong anchor there. See
    /// <c>Md.cs</c>.
    /// </summary>
    public static string Anchor(string heading) => Md.Slug(heading);

    /// <summary>
    /// The link rule as one string with <c>{path}</c> and <c>{anchor}</c> left to fill. The ref and the path prefix are
    /// baked in, as <c>docs/reference/export.md</c> argues. An Azure DevOps wiki takes the path with <c>.md</c> removed
    /// and its separators encoded, which <c>Link</c> does and <c>docs/use/corpus-descriptor.md</c> states.
    /// </summary>
    public string Template() => Target switch
    {
        GitHub => $"{_base}/blob/{Ref}{_prefix}/{PathToken}#{AnchorToken}",
        AzureDevOps => $"{_base}?path={_prefix}/{PathToken}&version=GC{Ref}#{AnchorToken}",
        AzureDevOpsWiki => $"{_base}?pagePath={Encode(_prefix)}%2F{PathToken}&anchor={AnchorToken}",
        _ => throw new InvalidOperationException($"'{Target}' builds no link.")
    };

    /// <summary>
    /// Built by substituting into <c>Template</c>, so this and a consumer's own link for a record are the same string.
    /// </summary>
    public string Link(string relPath, string? anchor = null)
    {
        var template = Template();

        var human = anchor is { Length: > 0 }
            ? template.Replace(AnchorToken, anchor, StringComparison.Ordinal)
            : template.Replace($"{AnchorMark}{AnchorToken}", "", StringComparison.Ordinal);

        return human.Replace(PathToken, PathFor(relPath), StringComparison.Ordinal);
    }

    /// <summary>A wiki takes a query parameter because <c>?pagePath=</c> has already opened the query string.</summary>
    private string AnchorMark =>
        Target.Equals(AzureDevOpsWiki, StringComparison.Ordinal) ? "&anchor=" : "#";

    /// <summary>
    /// A record's path as this target spells it. A wiki page is the file without its extension, and every separator
    /// inside a <c>pagePath</c> is encoded, the leading one included.
    /// </summary>
    private string PathFor(string relPath) =>
        Target.Equals(AzureDevOpsWiki, StringComparison.Ordinal)
            ? Encode(relPath.EndsWith(".md", StringComparison.Ordinal) ? relPath[..^3] : relPath)
            : relPath;

    private static string Encode(string path) =>
        path.Replace("/", "%2F", StringComparison.Ordinal);
}
