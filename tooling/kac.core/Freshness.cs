namespace kac.core;

// How one import stands against what its source holds now.
//
// `Current` covers both ways of being level: the lock is the newest release published, or the newest one
// the range admits and the range is exact. Neither is worth a line.
public enum Standing
{
    Current,
    Behind,
    Capped,
    Unreachable
}

// One declared import, and what asking its source came to.
//
// `Available` is the version the reader would move to, and is null only where nothing could be asked.
// `Problem` is the refusal in the source's own words, and is set exactly where `How` is `Unreachable`.
public sealed record ImportStanding(
    string Shortcode, string Range, string Locked, Standing How, string? Available, string? Problem);

// What each import's source now publishes, against the version this corpus locked.
//
// **A restore never asks this.** `Restore.Version` takes the locked version whenever the range still
// admits it, so two restores of an unchanged descriptor write the same bytes. That is what makes a build
// reproducible and it is also how a corpus sits on a version for a year without noticing. Asking is
// therefore `validate`'s, once per run, and the answer is never an error: being a version behind is not a
// broken corpus, and failing on somebody else's release would turn every downstream red the day the
// governance layer ships.
//
// The registry is passed in for the reason `Restore.Plan` takes one. What a corpus's imports come to
// stays decidable from a set of strings, so the three standings are unit tests rather than a network.
public static class Freshness
{
    // Every declared import that can be compared, and nothing about the ones that cannot.
    //
    // An entry short of a shortcode, a range or a source is skipped in silence: `restore` refuses it by
    // name, in sentences saying which key is missing. An entry with no `resolved:` is skipped too, having
    // no locked version to hold anything against.
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

            var published = registry.Versions(source, corpus);
            if (published.Value is not { } versions)
            {
                standings.Add(new ImportStanding(
                    shortcode, range, locked, Standing.Unreachable, null, published.Problem));
                continue;
            }

            standings.Add(Compare(shortcode, range, locked, versions));
        }

        return standings;
    }

    // Which of the three a locked version is in, given everything the source listed.
    //
    // The range is asked first, because a version the corpus already said it would take is the one it can
    // move to by running `restore` and nothing else. Only where the range holds the newest release back is
    // the cap worth naming, and then the reader's choice is to widen the range or to leave it alone.
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
