namespace kac.core;

// What a `version:` on a consumed corpus may say, and which published versions it admits.
//
// Two forms, and no more. `1.2.0` takes exactly that version, and `^1.2.0` takes the newest version that
// cannot have changed a meaning since: the same major, or below `1.0.0` the same minor, because a pre-1.0
// major promises nothing. A corpus states its intent in one of those two ways and a restore records what
// the intent came to.
//
// A grammar of comparators and unions was left out deliberately. It answers questions nobody consuming a
// corpus has asked, and every one of them would need a rule for what it means against a content version,
// which is a statement about meaning rather than about an API.
//
// **A caret never resolves to a prerelease.** A `-rc.1` is published to be asked for by name, and a
// range meaning "the newest safe version" that quietly took one would put an unfinished vocabulary
// behind a consumer's citations. Naming it exactly is how a corpus opts in.
public static class VersionRange
{
    private const char Caret = '^';

    // Whether this range is one of the two forms, so a typo is refused where it was written rather than
    // silently matching nothing.
    //
    // A caret over a prerelease is one of the typos. `^0.2.0-rc.1` parses, and then admits nothing at all
    // because no caret takes a prerelease, so a range accepted here would be reported later as a corpus
    // holding none of the versions it holds. Naming the prerelease exactly is how a corpus opts in.
    public static bool Legible(string range) =>
        Parsed(Bare(range)) is { } version && (!range.StartsWith(Caret) || version.Prerelease is null);

    // Whether `version` is one this range admits. A version the parser cannot read is admitted by
    // nothing: it came from a registry, and this tool orders versions it can order.
    public static bool Admits(string range, string version)
    {
        var bare = Bare(range);
        if (Parsed(bare) is not { } floor || Parsed(version) is not { } candidate) return false;

        if (!range.StartsWith(Caret)) return version.Equals(bare, StringComparison.Ordinal);

        // A prerelease is taken only where the range named one, which a caret never does.
        if (candidate.Prerelease is not null) return false;

        return Compare(candidate, floor) >= 0 && Compare(candidate, Ceiling(floor)) < 0;
    }

    // The highest version this range admits, or null where none of them does. `available` is what the
    // registry listed, in whatever order it listed them.
    public static string? Best(string range, IEnumerable<string> available)
    {
        string? best = null;

        foreach (var version in available.Where(v => Admits(range, v)))
            if (best is null || Compare(Parsed(version)!.Value, Parsed(best)!.Value) > 0)
                best = version;

        return best;
    }

    // Whether `candidate` orders above `than`. False where either is a version this tool cannot order,
    // which is the answer `Admits` gives the same string for the same reason.
    public static bool Newer(string candidate, string than) =>
        Parsed(candidate) is { } a && Parsed(than) is { } b && Compare(a, b) > 0;

    // The highest release version a feed listed, or null where it listed none this tool can order.
    //
    // A prerelease is left out. Only an exact range reaches one, so naming it as available would offer a
    // version no caret could ever take and no reader could act on without rewriting their range.
    public static string? Newest(IEnumerable<string> available)
    {
        string? best = null;

        foreach (var version in available)
            if (Parsed(version) is { Prerelease: null } && (best is null || Newer(version, best)))
                best = version;

        return best;
    }

    // Where a caret stops. A major above zero promises that nothing below it changed meaning, so the
    // range runs to the next major. Below one there is no such promise, and the minor is what carries it
    // instead.
    private static Version Ceiling(Version floor) =>
        floor.Major > 0
            ? new Version(floor.Major + 1, 0, 0, null)
            : new Version(0, floor.Minor + 1, 0, null);

    // Ordered as semantic versioning orders them, over the part of it these two forms can reach: the
    // three numbers, and a prerelease sorting below the release it leads to.
    //
    // Two prereleases are compared as strings rather than identifier by identifier. Only an exact range
    // reaches one, so the comparison decides nothing a corpus asked for; the ordering exists so that a
    // list holding one can still be sorted.
    private static int Compare(Version a, Version b)
    {
        if (a.Major != b.Major) return a.Major.CompareTo(b.Major);
        if (a.Minor != b.Minor) return a.Minor.CompareTo(b.Minor);
        if (a.Patch != b.Patch) return a.Patch.CompareTo(b.Patch);

        return (a.Prerelease, b.Prerelease) switch
        {
            (null, null) => 0,
            (null, _) => 1,
            (_, null) => -1,
            var (x, y) => string.CompareOrdinal(x, y)
        };
    }

    private static string Bare(string range) =>
        range.StartsWith(Caret) ? range[1..] : range;

    // `major.minor.patch`, and the prerelease behind a hyphen where there is one. Null for anything else,
    // which is the same answer `Packer` gives a corpus whose `content-version` a registry could not
    // order.
    private static Version? Parsed(string version)
    {
        var hyphen = version.IndexOf('-', StringComparison.Ordinal);
        var prerelease = hyphen < 0 ? null : version[(hyphen + 1)..];
        var numbers = (hyphen < 0 ? version : version[..hyphen]).Split('.');

        if (numbers.Length != 3 || prerelease is { Length: 0 }) return null;

        // Digits, and a count of them small enough to hold. `int.TryParse` alone would take a sign and
        // surrounding space, neither of which belongs in a version, and the digit test alone would throw
        // on a number too large from the middle of a comparison.
        if (!numbers.All(n => n.Length > 0 && n.All(char.IsAsciiDigit) && int.TryParse(n, out _)))
            return null;

        return new Version(int.Parse(numbers[0]), int.Parse(numbers[1]), int.Parse(numbers[2]), prerelease);
    }

    private readonly record struct Version(int Major, int Minor, int Patch, string? Prerelease);
}
