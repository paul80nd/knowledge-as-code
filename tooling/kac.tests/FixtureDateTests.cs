// A fixture date near today flips its golden the day it goes by, in a run nobody connects to the
// fixture. This asks every date a today-measured rule reads to sit far from today, and names the ones
// that do not.

using System.Globalization;
using System.Text.RegularExpressions;
using kac.core;

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public partial class FixtureDateTests
{
    /// <summary>
    /// Long enough that whoever meets a failure here moves the date and has years before the next one.
    /// </summary>
    private static readonly TimeSpan Clearance = TimeSpan.FromDays(365 * 2);

    [Fact]
    public void No_fixture_date_a_rule_measures_falls_near_today()
    {
        var today = DateTime.UtcNow.Date;
        var close = new List<string>();
        var read = 0;

        foreach (var (rel, fields) in Records())
        foreach (var (line, field, date) in Dates(File.ReadLines(Path.Combine(Repo.Root, rel)), fields))
        {
            read++;
            var away = (date.ToDateTime(TimeOnly.MinValue) - today).Duration();
            if (away >= Clearance) continue;

            close.Add($"{rel}:{line} {field} {date:yyyy-MM-dd} is {(int)away.TotalDays} day(s) from today.");
        }

        // A run that read nothing is this test gone quiet, which moved folders or renamed fields would cause.
        Assert.True(read > 0,
            "no fixture date was read, so this guard is checking nothing. Either no type declares a rule "
            + "measured against the day of the run, or the fixtures it would cover have moved.");

        Assert.True(close.Count == 0,
            "a fixture date is close enough to today to change what the fixture reports:\n  "
            + string.Join("\n  ", close)
            + "\nMove it well clear of today, on the side the fixture's expectations assume, and regenerate "
            + "that golden.");
    }

    [Fact]
    public void A_dotted_path_is_read_from_an_inline_object_and_from_a_block()
    {
        string[] lines =
        [
            "---", "last-rehearsed: { at: \"2020-01-01\", by: human:alex.doe }", "last-verified:",
            "  at: \"2021-02-02\"", "  by: human:alex.doe", "review-by: \"2022-03-03\"", "---"
        ];

        Assert.Equal(
            [(2, "last-rehearsed.at", new DateOnly(2020, 1, 1)), (4, "last-verified.at", new DateOnly(2021, 2, 2)),
                (6, "review-by", new DateOnly(2022, 3, 3))],
            Dates(lines, new HashSet<string> { "last-rehearsed.at", "last-verified.at", "review-by" }));
    }

    /// <summary>
    /// The quoted ISO dates under the fields given, with their lines. A path such as <c>last-rehearsed.at</c> names a
    /// key of the field, written inline or on an indented line below it. Anything that is not a date is left to the
    /// validator.
    /// </summary>
    private static IEnumerable<(int Line, string Field, DateOnly Date)> Dates(
        IEnumerable<string> lines, IReadOnlySet<string> fields)
    {
        var line = 0;
        string? parent = null;
        foreach (var text in lines)
        {
            line++;
            if (DatedKey().Match(text) is not { Success: true } m) continue;

            var key = m.Groups[2].Value;
            var indented = m.Groups[1].Value.Length > 0;
            if (!indented) parent = key;

            var path = indented ? $"{parent}.{key}" : key;
            if (Day(m.Groups[3].Value) is { } day && fields.Contains(path))
                yield return (line, path, day);

            if (indented) continue;

            foreach (Match inner in InlineDate().Matches(m.Groups[3].Value))
                if (Day(inner.Groups[2].Value) is { } nested && fields.Contains($"{key}.{inner.Groups[1].Value}"))
                    yield return (line, $"{key}.{inner.Groups[1].Value}", nested);
        }
    }

    private static DateOnly? Day(string value) =>
        DateOnly.TryParseExact(value.Trim().Trim('"'), "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var date)
            ? date
            : null;

    private static IEnumerable<(string Rel, IReadOnlySet<string> Fields)> Records()
    {
        var measured = Schema.Load(Repo.Root).ByFolder
            .Select(pair => (Folder: $"/{pair.Key}/", Fields: Measured(pair.Value)))
            .Where(pair => pair.Fields.Count > 0)
            .ToList();

        var tracked = (GitFiles.Tracked(Repo.Root) ?? GitFiles.Walk(Repo.Root, "*.md", ".git"))
            .Select(rel => rel.Replace('\\', '/'))
            .Where(rel => rel.StartsWith("tooling/tests/fixtures/", StringComparison.Ordinal)
                          && rel.EndsWith(".md", StringComparison.Ordinal));

        foreach (var rel in tracked)
        foreach (var (folder, fields) in measured)
            if (rel.Contains(folder, StringComparison.Ordinal))
                yield return (rel, fields);
    }

    /// <summary>
    /// <c>OfType</c> drops the rules declaring no expression, which are intentions rather than checks.
    /// </summary>
    private static IReadOnlySet<string> Measured(TypeSchema type) =>
        type.Rules
            .Select(rule => rule.Expr)
            .OfType<string>()
            .Where(expr => expr.Contains("today()", StringComparison.Ordinal)
                           || expr.Contains("days_since(", StringComparison.Ordinal))
            .SelectMany(expr => FieldNames().Matches(expr).Select(m => m.Groups[1].Value))
            .ToHashSet(StringComparer.Ordinal);

    [GeneratedRegex("""^( *)([a-z0-9-]+):(.*)$""")]
    private static partial Regex DatedKey();

    [GeneratedRegex("""([a-z0-9-]+):\s*"?(\d{4}-\d{2}-\d{2})"?""")]
    private static partial Regex InlineDate();

    [GeneratedRegex("""(?:field|days_since)\('([^']+)'\)""")]
    private static partial Regex FieldNames();
}
