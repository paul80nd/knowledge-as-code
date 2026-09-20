// A rule measuring a record against the day the run happens reads a fixture carrying a date near today as
// one thing for whoever wrote it and another the day it goes by, in a run nobody connects to the fixture.
// This asks every such date to sit far enough from today that the golden expectations beside it stay true
// for years, and names the ones that do not.
//
// The schema decides which folders it looks in and which fields it reads there. A rule calling `today()`
// compares a field against it, and one calling `days_since()` names its field outright, so both field names
// come out of the expression. A type declaring either tomorrow is covered without anybody remembering this
// file.

using System.Globalization;
using System.Text.RegularExpressions;
using kac.core;

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public partial class FixtureDateTests
{
    // Long enough that whoever meets a failure here moves the date and has years before the next one. Any
    // shorter and the test itself becomes the time bomb.
    private static readonly TimeSpan Clearance = TimeSpan.FromDays(365 * 2);

    [Fact]
    public void No_fixture_date_a_rule_measures_falls_near_today()
    {
        var today = DateTime.UtcNow.Date;
        var close = new List<string>();
        var read = 0;

        foreach (var (rel, fields) in Records())
        foreach (var (line, field, date) in Dates(Path.Combine(Repo.Root, rel), fields))
        {
            read++;
            var away = (date.ToDateTime(TimeOnly.MinValue) - today).Duration();
            if (away >= Clearance) continue;

            close.Add($"{rel}:{line} {field} {date:yyyy-MM-dd} is {(int)away.TotalDays} day(s) from today.");
        }

        // A run that read nothing is this test having gone quiet, which it would do if the folders moved or
        // the fields were renamed. Both leave it green, and green is what it would report for years.
        Assert.True(read > 0,
            "no fixture date was read, so this guard is checking nothing. Either no type declares a rule "
            + "measured against the day of the run, or the fixtures it would cover have moved.");

        Assert.True(close.Count == 0,
            "a fixture date is close enough to today to change what the fixture reports:\n  "
            + string.Join("\n  ", close)
            + "\nMove it well clear of today, on the side the fixture's expectations assume, and regenerate "
            + "that golden.");
    }

    // The quoted ISO dates a record states under the fields given, with the lines they sit on. A template
    // writes a key bare and a corpus may leave it unfilled, so anything that is not a date is left to the
    // validator.
    private static IEnumerable<(int Line, string Field, DateOnly Date)> Dates(
        string path, IReadOnlySet<string> fields)
    {
        var line = 0;
        foreach (var text in File.ReadLines(path))
        {
            line++;
            if (DatedField().Match(text) is { Success: true } m
                && fields.Contains(m.Groups[1].Value)
                && DateOnly.TryParseExact(m.Groups[2].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var date))
                yield return (line, m.Groups[1].Value, date);
        }
    }

    // Every markdown file inside a fixture corpus, under a folder whose type is judged against today, paired
    // with the fields that judgement reads. The fixtures are the only trees whose expectations are committed
    // beside them, so they are the only ones where a date going by rewrites a stored answer.
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

    // The fields a type's rules read against the day of the run. `OfType` drops the rules declaring no
    // expression, which are intentions rather than checks. Every field an expression names is taken, because
    // one naming no date matches no line and costs a scan.
    private static IReadOnlySet<string> Measured(TypeSchema type) =>
        type.Rules
            .Select(rule => rule.Expr)
            .OfType<string>()
            .Where(expr => expr.Contains("today()", StringComparison.Ordinal)
                           || expr.Contains("days_since(", StringComparison.Ordinal))
            .SelectMany(expr => FieldNames().Matches(expr).Select(m => m.Groups[1].Value))
            .ToHashSet(StringComparer.Ordinal);

    [GeneratedRegex("""^([a-z0-9-]+):\s*"?(\d{4}-\d{2}-\d{2})"?\s*$""")]
    private static partial Regex DatedField();

    [GeneratedRegex("""(?:field|days_since)\('([^']+)'\)""")]
    private static partial Regex FieldNames();
}
