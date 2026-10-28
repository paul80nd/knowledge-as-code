namespace kac.core;

/// <summary>
/// One address with <c>Publishing</c> on GitHub, and two on Azure DevOps, where a project holds the backlog.
/// <c>docs/reference/export.md</c> sets out what <c>Id</c> drops.
/// </summary>
public static class Tracker
{
    public static readonly IReadOnlyList<string> Targets = [Publishing.AzureDevOps, Publishing.GitHub];

    public static readonly IReadOnlyList<string> Areas = [Publishing.AzureDevOps];

    /// <summary><c>validate</c> holds <c>tracker:</c> and <c>framework:</c> to this list.</summary>
    public static readonly IReadOnlyList<string> Stated =
        [Publishing.AzureDevOps, Publishing.GitHub, Publishing.None];

    private static readonly string[] Inside = ["/_git/", "/_wiki/"];

    public static ExportTracker None => new(Publishing.None, null, null, null);

    /// <summary>The skills that file on the corpus's tracker, and print the issue where it has no backlog.</summary>
    public static readonly IReadOnlyList<string> Filing = ["raise-finding", "request-deviation"];

    /// <summary>
    /// Opens what <c>new</c> and <c>bundle</c> print where <paramref name="tracker"/> addresses no backlog, and is null
    /// where it addresses one. Each caller closes the line with its own step.
    /// </summary>
    public static string? Unaddressed(ExportTracker tracker, IReadOnlyList<string> skills) =>
        tracker.Id is null && skills.Count > 0
            ? $"this corpus has no tracker to file on, so {string.Join(" and ", skills)} will print each issue "
              + "for you to file."
            : null;

    /// <summary>
    /// A stated key wins over the derived one. The base falls back only where the stated target agrees with the derived
    /// one: a corpus naming another platform's client knows an address nobody else does.
    /// </summary>
    public static ExportTracker Own(CorpusDescriptor descriptor)
    {
        var derived = Derived(descriptor.PublishingTarget, descriptor.Base);
        var target = descriptor.TrackerTarget ?? derived.Target;
        var agreed = target.Equals(derived.Target, StringComparison.Ordinal);

        return For(target, descriptor.TrackerBase ?? (agreed ? derived.Base : null), descriptor.TrackerArea);
    }

    /// <summary>Never derived: where a corpus publishes says nothing about who maintains <c>kac</c>.</summary>
    public static ExportTracker Framework(CorpusDescriptor descriptor) =>
        For(descriptor.FrameworkTarget, descriptor.FrameworkBase, descriptor.FrameworkArea);

    /// <summary>For an export that states no tracker.</summary>
    public static ExportTracker From(ExportPublishing publishing) =>
        Derived(publishing.Target, publishing.Base);

    /// <summary>
    /// The target is written through as stated, so a misspelling stays visible in the manifest. <c>Base</c> and
    /// <c>Id</c> are null together wherever the pair addresses no backlog. <c>Area</c> is dropped on the same terms,
    /// and on GitHub. <c>validate</c> reports a dropped area under <c>descriptor-area</c>.
    /// </summary>
    public static ExportTracker For(string? target, string? published, string? area)
    {
        var named = target is { Length: > 0 } ? target : Publishing.None;
        if (!Targets.Contains(named, StringComparer.Ordinal)) return new ExportTracker(named, null, null, null);

        var filed = Areas.Contains(named, StringComparer.Ordinal) ? Written(area) : null;

        return Project(named, published) is { } url
            ? new ExportTracker(named, url, filed, $"{named}:{Identity(url)}")
            : new ExportTracker(named, null, null, null);
    }

    /// <summary>Whitespace alone is no area, and a stray separator would build an empty segment.</summary>
    private static string? Written(string? area) =>
        area?.Trim().Trim('/') is { Length: > 0 } stated ? stated : null;

    /// <summary>A publishing block says nothing about areas, so no arm derives one.</summary>
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

    /// <summary>
    /// The base as a backlog is addressed, or null where nothing stated one. An Azure DevOps base is cut back to the
    /// project, because the segment below names a repository or a wiki and no ticket is filed on either. A GitHub base
    /// loses only a trailing separator.
    /// </summary>
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

    /// <summary>
    /// One base written one way, so two spellings of one address compare equal: no scheme, no <c>www.</c>, no
    /// <c>.git</c> suffix, and lower case throughout. Both hosts route case-insensitively and serve the same repository
    /// over either scheme, so every part this drops is one that never told two backlogs apart.
    /// </summary>
    private static string Identity(string url)
    {
        var at = url.IndexOf("://", StringComparison.Ordinal);
        var host = at < 0 ? url : url[(at + 3)..];

        var bare = host.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ? host[4..] : host;
        if (bare.EndsWith(".git", StringComparison.OrdinalIgnoreCase)) bare = bare[..^4];

        return bare.TrimEnd('/').ToLowerInvariant();
    }
}
