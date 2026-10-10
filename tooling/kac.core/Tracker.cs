namespace kac.core;

// One address with `Publishing` on GitHub, and two on Azure DevOps, where a project holds the backlog.
// `docs/design/export.md` sets out what `Id` drops and why.
public static class Tracker
{
    public static readonly IReadOnlyList<string> Targets = [Publishing.AzureDevOps, Publishing.GitHub];

    public static readonly IReadOnlyList<string> Areas = [Publishing.AzureDevOps];

    // `validate` holds `tracker:` and `framework:` to this list.
    public static readonly IReadOnlyList<string> Stated =
        [Publishing.AzureDevOps, Publishing.GitHub, Publishing.None];

    private static readonly string[] Inside = ["/_git/", "/_wiki/"];

    public static ExportTracker None => new(Publishing.None, null, null, null);

    // A stated key wins over the derived one. The base falls back only where the stated target agrees
    // with the derived one: a corpus naming another platform's client knows an address nobody else does.
    public static ExportTracker Own(CorpusDescriptor descriptor)
    {
        var derived = Derived(descriptor.PublishingTarget, descriptor.Base);
        var target = descriptor.TrackerTarget ?? derived.Target;
        var agreed = target.Equals(derived.Target, StringComparison.Ordinal);

        return For(target, descriptor.TrackerBase ?? (agreed ? derived.Base : null), descriptor.TrackerArea);
    }

    // Never derived: where a corpus publishes says nothing about who maintains `kac`.
    public static ExportTracker Framework(CorpusDescriptor descriptor) =>
        For(descriptor.FrameworkTarget, descriptor.FrameworkBase, descriptor.FrameworkArea);

    // For an export that states no tracker.
    public static ExportTracker From(ExportPublishing publishing) =>
        Derived(publishing.Target, publishing.Base);

    // The target is written through as stated, so a misspelling stays visible in the manifest. `Base` and
    // `Id` are null together wherever the pair addresses no backlog. `Area` is dropped on the same terms,
    // and on GitHub. `validate` reports a dropped area under `descriptor-area`.
    public static ExportTracker For(string? target, string? published, string? area)
    {
        var named = target is { Length: > 0 } ? target : Publishing.None;
        if (!Targets.Contains(named, StringComparer.Ordinal)) return new ExportTracker(named, null, null, null);

        var filed = Areas.Contains(named, StringComparer.Ordinal) ? Written(area) : null;

        return Project(named, published) is { } url
            ? new ExportTracker(named, url, filed, $"{named}:{Identity(url)}")
            : new ExportTracker(named, null, null, null);
    }

    // Whitespace alone is no area, and a stray separator would build an empty segment.
    private static string? Written(string? area) =>
        area?.Trim().Trim('/') is { Length: > 0 } stated ? stated : null;

    // A publishing block says nothing about areas, so no arm derives one.
    private static ExportTracker Derived(string? publishingTarget, string? published) =>
        publishingTarget switch
        {
            Publishing.GitHub => For(Publishing.GitHub, published, null),

            // `az boards` files against the project whichever of them publishes the records, so the
            // wiki target does not travel. It says how a record is read and nothing about where a ticket
            // goes.
            Publishing.AzureDevOps or Publishing.AzureDevOpsWiki => For(Publishing.AzureDevOps, published, null),
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
