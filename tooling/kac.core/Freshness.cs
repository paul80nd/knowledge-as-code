namespace kac.core;

public enum Standing
{
    Current,
    Behind,
    Capped,
    Unreachable
}

// `Available` is the version to move to, and null where `How` is `Current` or `Unreachable`. `Problem` is set
// exactly where `How` is `Unreachable`.
public sealed record ImportStanding(
    string Shortcode,
    string Range,
    string Locked,
    Standing How,
    string? Available,
    string? Problem);

// What each import's source now publishes, against the locked version. `validate` asks this once per run
// and never reports it as an error: being a version behind is not a broken corpus.
public static class Freshness
{
    // Every declared import that can be compared. An incomplete entry is skipped, because `restore` refuses it
    // by name, and so is one with no lock or a lock its range no longer admits.
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

    // The range is asked first, because a version it admits is one `restore` moves to with no other change.
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
