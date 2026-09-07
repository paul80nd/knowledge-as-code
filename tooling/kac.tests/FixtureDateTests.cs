// A rule calling `today()` reads a record against the day the run happens, so a fixture carrying a date near
// today passes for whoever wrote it and fails the day it goes by, in a run nobody connects to the fixture.
// This asks every such date to sit far enough from today that the golden expectations beside it stay true for
// years, and names the ones that do not.
//
// The schema decides which folders it looks in, so a type declaring a rule against `today()` tomorrow is
// covered without anybody remembering this file. `review-by` is the only field a rule compares that way
// today, and a second one would be read from the expressions the same way.

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
    public void No_fixture_review_date_falls_near_today()
    {
        var today = DateTime.UtcNow.Date;
        var close = new List<string>();
        var read = 0;

        foreach (var rel in Records())
        foreach (var (line, date) in ReviewDates(Path.Combine(Repo.Root, rel)))
        {
            read++;
            var away = (date.ToDateTime(TimeOnly.MinValue) - today).Duration();
            if (away >= Clearance) continue;

            close.Add($"{rel}:{line} review-by {date:yyyy-MM-dd} is {(int)away.TotalDays} day(s) from today.");
        }

        // A run that read nothing is this test having gone quiet, which it would do if the folders moved or
        // the field were renamed. Both leave it green, and green is what it would report for years.
        Assert.True(read > 0,
            "no fixture date was read, so this guard is checking nothing. Either no type declares a rule "
            + "against today(), or the fixtures it would cover have moved.");

        Assert.True(close.Count == 0,
            "a fixture's review date is close enough to today to change what the fixture reports:\n  "
            + string.Join("\n  ", close)
            + "\nMove it well clear of today, on the side the fixture's expectations assume, and regenerate "
            + "that golden.");
    }

    // The quoted ISO date a `review-by` carries, with the line it sits on. A template writes the key bare and
    // a corpus may leave it unfilled, so anything that is not a date is left to the validator.
    private static IEnumerable<(int Line, DateOnly Date)> ReviewDates(string path)
    {
        var line = 0;
        foreach (var text in File.ReadLines(path))
        {
            line++;
            if (ReviewBy().Match(text) is { Success: true } m
                && DateOnly.TryParseExact(m.Groups[1].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var date))
                yield return (line, date);
        }
    }

    // Every markdown file inside a fixture corpus, under a folder whose type is judged against today. The
    // fixtures are the only trees whose expectations are committed beside them, so they are the only ones
    // where a date going by rewrites a stored answer.
    private static IEnumerable<string> Records()
    {
        var folders = Schema.Load(Repo.Root).ByFolder
            .Where(pair => pair.Value.Rules.Any(r => r.Expr?.Contains("today()", StringComparison.Ordinal) is true))
            .Select(pair => $"/{pair.Key}/")
            .ToList();

        return (GitFiles.Tracked(Repo.Root) ?? GitFiles.Walk(Repo.Root, "*.md", ".git"))
            .Select(rel => rel.Replace('\\', '/'))
            .Where(rel => rel.StartsWith("tooling/tests/fixtures/", StringComparison.Ordinal)
                          && rel.EndsWith(".md", StringComparison.Ordinal)
                          && folders.Any(folder => rel.Contains(folder, StringComparison.Ordinal)));
    }

    [GeneratedRegex("""^review-by:\s*"?(\d{4}-\d{2}-\d{2})"?\s*$""")]
    private static partial Regex ReviewBy();
}
