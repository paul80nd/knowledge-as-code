namespace kac.core;

// How a publishing target addresses what it publishes: the rule joining a base to a record's path, and
// the anchor rule for a part inside it.
//
// The rules live here rather than in `.corpus.yaml` because they are a property of the target and not of
// the corpus. Two corpora published to GitHub build a link the same way and differ only in where their
// repository sits, so the descriptor supplies the base and nothing else.
//
// One address is built, the one a person opens. An agent reads a record's source through a client that
// authenticates to the target, from the base, the path and the ref an export carries. There is no host
// serving a corpus's raw source to an anonymous caller except GitHub's, and only for a public
// repository. See `docs/design/export.md`.
public sealed class Publishing
{
    public const string AzureDevOps = "azure-devops";
    public const string AzureDevOpsWiki = "azure-devops-wiki";
    public const string GitHub = "github";
    public const string MkDocs = "mkdocs";
    public const string None = "none";

    // Every target a descriptor may name. `new` offers this list and holds `--publishing` to it, so a
    // target the tool cannot act on is refused at the flag rather than written into a corpus.
    public static readonly IReadOnlyList<string> Targets =
        [AzureDevOps, AzureDevOpsWiki, GitHub, MkDocs, None];

    // The two placeholders a template leaves for its reader to fill.
    public const string PathToken = "{path}";
    public const string AnchorToken = "{anchor}";

    // What sits between `_wiki/wikis/` and the wiki's identifier in an Azure DevOps wiki URL. The
    // identifier is the last segment a base may carry, so this is where `For` starts counting.
    private const string WikiSegment = "/_wiki/wikis/";

    // The targets that build a link today. `mkdocs` is named by the descriptor and addressed by
    // nothing, so a corpus on it exports without links rather than with links built on a convention no
    // one has settled.
    private static readonly IReadOnlyList<string> Addressable = [AzureDevOps, AzureDevOpsWiki, GitHub];

    private readonly string _base;

    // The corpus root's place inside the published repository, carrying its own leading slash, or empty
    // where the corpus is the repository. Normalised on the way in so the rest of the class joins it the
    // same way whether the descriptor wrote it with slashes or without.
    private readonly string _prefix;

    public required string Target { get; init; }

    // The commit every link resolves against. A link naming a branch answers a later question than the
    // one the export was built to answer: what the corpus said when it was read.
    //
    // `azure-devops-wiki` is the exception it cannot help. No `?pagePath=` URL takes a commit, so a
    // person following that target's link reads whatever the wiki holds now. An agent still reads the
    // pinned version, because the ref reaches it through the export rather than through the link.
    public required string Ref { get; init; }

    // Where the corpus sits inside its repository, without a leading slash, or null where the corpus is
    // the repository. An agent joins it to a record's path to reach the file, which is why it is carried
    // separately as well as baked into the template.
    public string? PathPrefix => _prefix is { Length: > 0 } ? _prefix[1..] : null;

    private Publishing(string publishedBase, string? pathPrefix)
    {
        _base = publishedBase.TrimEnd('/');
        _prefix = pathPrefix?.Trim('/') is { Length: > 0 } p ? "/" + p : "";
    }

    // How this corpus addresses its published form, or null where it has no addressable one: it
    // publishes nowhere, it names a target nothing builds links for, it names one and supplies no base,
    // or the base it supplies is not one that target can join to. Null in every case, because a caller's
    // question is whether it can write a link at all, and four ways of being unable to are one answer.
    //
    // `gitRef` is the commit the export was built from. Null where git could not say, which is a corpus
    // whose records have no stable address, so it takes the same answer as the rest.
    public static Publishing? For(CorpusDescriptor descriptor, string? gitRef)
    {
        if (descriptor.PublishingTarget is not { } target) return null;
        if (!Addressable.Contains(target, StringComparer.Ordinal)) return null;
        if (descriptor.Base is not { Length: > 0 } published) return null;
        if (gitRef is not { Length: > 0 }) return null;
        if (!Joinable(target, published)) return null;

        return new Publishing(published, descriptor.PathPrefix) { Target = target, Ref = gitRef };
    }

    // Whether this target can join a record's path to the base it was handed.
    //
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

    // The base a repository's origin implies, or null where nothing here can derive one.
    //
    // A URL nobody can recall is a URL nobody should be made to type, so `new` fills the answer in and
    // asks the person to confirm it.
    //
    // Both SSH and HTTPS remotes are read, because which one a clone used is a fact about the person
    // rather than about the repository.
    //
    // `azure-devops-wiki` is absent deliberately. A repository's remote says nothing about which wiki,
    // if any, publishes it, so that base is typed in.
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

    // What follows the host, however the remote spelled the separator: `:` after an SSH host, `/` after
    // an HTTPS one.
    private static string Rest(string repo, int from) =>
        repo[from..].TrimStart(':', '/').TrimEnd('/');

    // The anchor a link uses to land on a part. GitHub derives it from the heading by discarding the
    // punctuation it meets, which is the form `Md.Slug` writes and the form the corpus's own links
    // already use.
    //
    // Azure DevOps percent-encodes that punctuation instead, so a heading carrying `/`, `:` or `.` has
    // an anchor this returns the wrong spelling of. The rule has not been read off a live wiki, and a
    // second function written on a guess buys a wrong anchor rather than a missing one. What holds the
    // cost down meanwhile is the corpus's own convention: a heading meant to be linked to is written
    // without punctuation. See `Md.cs` and `frameworks.md`.
    public static string Anchor(string heading) => Md.Slug(heading);

    // The link rule as one string a reader substitutes into, so an export states the address once and a
    // line carries only what varies: the record's path, and the anchor of the part inside it.
    //
    // The ref is baked in rather than left as a placeholder. `docs/design/export.md` argues it, and the
    // template carries the decision so no reader has to remember it.
    //
    // **The path prefix is baked in for the same reason the ref is.** It is a property of where the
    // corpus sits and never varies between two records, so a reader substituting into the template has
    // one thing to supply and cannot put it on the wrong side of the commit.
    //
    // **What `{path}` takes is the target's business.** GitHub and Azure Repos address a file and take
    // the record's path whole. An Azure DevOps wiki addresses a page, so it takes the same path with
    // `.md` removed and its separators percent-encoded. `Link` performs that transform, and
    // `docs/corpus-descriptor.md` states it for whoever substitutes by hand.
    public string Template() => Target switch
    {
        GitHub => $"{_base}/blob/{Ref}{_prefix}/{PathToken}#{AnchorToken}",
        AzureDevOps => $"{_base}?path={_prefix}/{PathToken}&version=GC{Ref}#{AnchorToken}",
        AzureDevOpsWiki => $"{_base}?pagePath={Encode(_prefix)}%2F{PathToken}&anchor={AnchorToken}",
        _ => throw new InvalidOperationException($"'{Target}' builds no link.")
    };

    // Where a record is read, resolved. `anchor` names a part inside it, and a link with no anchor drops
    // the anchor along with it.
    //
    // Built by substituting into the template above, so a link this method resolves and a link a
    // consumer assembles for the same record are the same string. Two constructions of one address would
    // be two places for the rule to live.
    public string Link(string relPath, string? anchor = null)
    {
        var template = Template();

        var human = anchor is { Length: > 0 }
            ? template.Replace(AnchorToken, anchor, StringComparison.Ordinal)
            : template.Replace($"{AnchorMark}{AnchorToken}", "", StringComparison.Ordinal);

        return human.Replace(PathToken, PathFor(relPath), StringComparison.Ordinal);
    }

    // What introduces the anchor in this target's template, so dropping the anchor drops what named it.
    // A wiki takes a query parameter because `?pagePath=` has already opened the query string, where the
    // other two take a fragment.
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
