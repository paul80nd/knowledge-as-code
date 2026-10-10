namespace kac.core;

// Two forms: `1.2.0` takes exactly that version, and `^1.2.0` the newest with the same major, or below
// `1.0.0` the same minor. A caret never resolves to a prerelease. Naming one exactly is how a corpus opts in.
public static class VersionRange
{
    private const char Caret = '^';

    // Whether this range is one of the two forms. A caret over a prerelease is not, because it would admit nothing.
    public static bool Legible(string range) =>
        Parsed(Bare(range)) is { } version && (!range.StartsWith(Caret) || version.Prerelease is null);

    // A version the parser cannot read is admitted by nothing.
    public static bool Admits(string range, string version)
    {
        var bare = Bare(range);
        if (Parsed(bare) is not { } floor || Parsed(version) is not { } candidate) return false;

        if (!range.StartsWith(Caret)) return version.Equals(bare, StringComparison.Ordinal);

        if (candidate.Prerelease is not null) return false;

        return Compare(candidate, floor) >= 0 && Compare(candidate, Ceiling(floor)) < 0;
    }

    // The highest version this range admits, or null where it admits none. `available` may be in any order.
    public static string? Best(string range, IEnumerable<string> available)
    {
        string? best = null;
        Version? highest = null;

        foreach (var version in available.Where(v => Admits(range, v)))
        {
            if (Parsed(version) is not { } parsed) continue;
            if (highest is not null && Compare(parsed, highest.Value) <= 0) continue;

            best = version;
            highest = parsed;
        }

        return best;
    }

    // False where either is a version this tool cannot order.
    public static bool Newer(string candidate, string than) =>
        Parsed(candidate) is { } a && Parsed(than) is { } b && Compare(a, b) > 0;

    // The highest release a feed listed, or null. A prerelease is left out, because no caret can take one.
    public static string? Newest(IEnumerable<string> available)
    {
        string? best = null;

        foreach (var version in available)
            if (Parsed(version) is { Prerelease: null } && (best is null || Newer(version, best)))
                best = version;

        return best;
    }

    // Below `1.0.0` the minor carries the promise a major carries above it.
    private static Version Ceiling(Version floor) =>
        floor.Major > 0
            ? new Version(floor.Major + 1, 0, 0, null)
            : new Version(0, floor.Minor + 1, 0, null);

    // Two prereleases compare as strings, not identifier by identifier. Only an exact range reaches one, so the
    // order decides nothing a corpus asked for.
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

    // `major.minor.patch`, with an optional prerelease after a hyphen. Null for anything else.
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
