namespace kac.core;

public enum Standing
{
    Current,
    Behind,
    Capped,
    Unreachable
}

/// <summary>
/// <c>Available</c> is the version to move to, and null where <c>How</c> is <c>Current</c> or <c>Unreachable</c>.
/// <c>Problem</c> is set exactly where <c>How</c> is <c>Unreachable</c>.
/// </summary>
public sealed record ImportStanding(
    string Shortcode,
    string Range,
    string Locked,
    Standing How,
    string? Available,
    string? Problem);

/// <summary>
/// What each import's source now publishes, against the locked version. <c>validate</c> asks this once per run and
/// never reports it as an error: being a version behind is not a broken corpus.
/// </summary>
public static class Freshness
{
    /// <summary>
    /// Every declared import that can be compared. An incomplete entry is skipped, because <c>restore</c> refuses it by
    /// name, and so is one with no lock or a lock its range no longer admits.
    /// </summary>
    public static List<ImportStanding> Read(IReadOnlyList<Consumed> declared, Registry registry)
    {
        var standings = new List<ImportStanding>();

        foreach (var entry in declared)
        {
            if (entry.Shortcode is not { } shortcode) continue;
            if (entry.Version is not { } range) continue;
            if (entry.Resolved is not { } locked) continue;
            if (entry.Source is not { } source) continue;
            if (entry.Corpus is not { } corpus) continue;

            if (!VersionRange.Admits(range, locked)) continue;

            var published = registry.Versions(source, corpus);
            if (published.Value is not { } versions)
            {
                standings.Add(new ImportStanding(
                    shortcode, range, locked, Standing.Unreachable, null, published.Problem));
                continue;
            }

            // An empty listing is not a current lock. A feed answers 404 for an unpublished package and for a
            // private one refusing an anonymous read (see `Registry`), and an empty folder was never packed into.
            // None of the three says the locked version is the newest.
            if (versions.Count == 0)
            {
                standings.Add(new ImportStanding(
                    shortcode, range, locked, Standing.Unreachable, null, Registry.Absent(source)));
                continue;
            }

            standings.Add(Compare(shortcode, range, locked, versions));
        }

        return standings;
    }

    /// <summary>
    /// The range is asked first, because a version it admits is one <c>restore</c> moves to with no other change.
    /// </summary>
    private static ImportStanding Compare(
        string shortcode, string range, string locked, IReadOnlyList<string> versions)
    {
        if (VersionRange.Best(range, versions) is { } admitted && VersionRange.Newer(admitted, locked))
            return new ImportStanding(shortcode, range, locked, Standing.Behind, admitted, null);

        if (VersionRange.Newest(versions) is { } newest && VersionRange.Newer(newest, locked))
            return new ImportStanding(shortcode, range, locked, Standing.Capped, newest, null);

        return new ImportStanding(shortcode, range, locked, Standing.Current, null, null);
    }
}
