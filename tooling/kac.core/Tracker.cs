namespace kac.core;

// Where work about a corpus is filed. `Publishing` says where the corpus is read, and the two are one
// address on GitHub, where a repository has an issue list of its own, and two on Azure DevOps, where one
// project holds a backlog and many repositories.
//
// A descriptor states a tracker where its own is not the one its publishing block implies, and one that
// states none gets the derivation below. `Id` is the pair normalised, and it is how a caller holding
// several of these tells one backlog from two. `docs/design/export.md` sets out what it drops and why.
public static class Tracker
{
    // The targets that address a tracker: `github` and `azure-devops`. A wiki, a documentation site and
    // nowhere are the rest of what `publishing-target` takes, and none of them has a backlog to file on.
    public static readonly IReadOnlyList<string> Targets = [Publishing.AzureDevOps, Publishing.GitHub];

    // What stands between a project and the repository or wiki inside it. Azure DevOps addresses both
    // under the project, so the project is what comes before either segment.
    private static readonly string[] Inside = ["/_git/", "/_wiki/"];

    // No tracker at all: the state a corpus reaches by publishing nowhere, by publishing somewhere with
    // no backlog, or by stating a block with nothing in it.
    public static ExportTracker None => new(Publishing.None, null, null);

    // Where work about this corpus is filed. A stated key wins over the derived one, so a corpus
    // publishing to Azure Repos and filing on its project's backlog states the base and lets the client
    // follow from where it publishes.
    //
    // The base falls back only where the stated target agrees with the derived one. A corpus naming a
    // client its publishing block does not imply is filing on another platform, and the address there is
    // one only that corpus knows. Pairing the two would state a backlog nobody supplied.
    public static ExportTracker Own(CorpusDescriptor descriptor)
    {
        var derived = Derived(descriptor.PublishingTarget, descriptor.Base);
        var target = descriptor.TrackerTarget ?? derived.Target;
        var agreed = target.Equals(derived.Target, StringComparison.Ordinal);

        return For(target, descriptor.TrackerBase ?? (agreed ? derived.Base : null));
    }

    // Where to report a problem with the framework the corpus took. Stated or absent, and never derived:
    // where a corpus publishes says nothing about who maintains `kac`.
    public static ExportTracker Framework(CorpusDescriptor descriptor) =>
        For(descriptor.FrameworkTarget, descriptor.FrameworkBase);

    // The tracker a publishing block implies, for an export written before a corpus could state one. The
    // same derivation `Own` falls back to, reading the block an export published instead of a descriptor.
    public static ExportTracker From(ExportPublishing publishing) =>
        Derived(publishing.Target, publishing.Base);

    // A target and a base as the pair they address. The target is written through as it was stated, so a
    // value nobody spelled right stays visible in the manifest.
    //
    // `Base` and `Id` are null together, and are null wherever the pair addresses no backlog: a target
    // that files nowhere, a target nobody spelled right, or a base nobody supplied. A base standing
    // beside a target that cannot use it would read as an address, and every caller would have to test
    // the target before believing it.
    public static ExportTracker For(string? target, string? published)
    {
        var named = target is { Length: > 0 } ? target : Publishing.None;
        if (!Targets.Contains(named, StringComparer.Ordinal)) return new ExportTracker(named, null, null);

        return Project(named, published) is { } url
            ? new ExportTracker(named, url, $"{named}:{Identity(url)}")
            : new ExportTracker(named, null, null);
    }

    private static ExportTracker Derived(string? publishingTarget, string? published) =>
        publishingTarget switch
        {
            Publishing.GitHub => For(Publishing.GitHub, published),

            // `az boards` files against the project whichever of them publishes the records, so the
            // wiki target does not travel. It says how a record is read and nothing about where a ticket
            // goes.
            Publishing.AzureDevOps or Publishing.AzureDevOpsWiki => For(Publishing.AzureDevOps, published),
            _ => None
        };

    // The base as a backlog is addressed, or null where nothing stated one. An Azure DevOps base is cut
    // back to the project holding the backlog, whether the corpus stated it or `Own` derived it: the
    // segment below it names a repository or a wiki, and no ticket is filed on either. A GitHub base is
    // already the repository the issues belong to, so it loses only a trailing separator.
    private static string? Project(string target, string? published)
    {
        if (published?.Trim().TrimEnd('/') is not { Length: > 0 } url) return null;
        if (!target.Equals(Publishing.AzureDevOps, StringComparison.Ordinal)) return url;

        var at = Inside
            .Select(segment => url.IndexOf(segment, StringComparison.OrdinalIgnoreCase))
            .Where(found => found > 0)
            .DefaultIfEmpty(-1)
            .Min();

        return at < 0 ? url : url[..at];
    }

    // One base written one way, so two spellings of one address compare equal: no scheme, no `www.`, no
    // `.git` suffix, and lower case throughout. Both hosts route case-insensitively and serve the same
    // repository over either scheme, so every part this drops is one that never told two backlogs apart.
    private static string Identity(string url)
    {
        var at = url.IndexOf("://", StringComparison.Ordinal);
        var host = at < 0 ? url : url[(at + 3)..];

        var bare = host.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ? host[4..] : host;
        if (bare.EndsWith(".git", StringComparison.OrdinalIgnoreCase)) bare = bare[..^4];

        return bare.TrimEnd('/').ToLowerInvariant();
    }
}
