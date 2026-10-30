using System.Text.RegularExpressions;

namespace kac.tests;

/// <summary>
/// Every golden-suite fixture states what it proves in a <c>README.md</c>, which a regenerated golden cannot take away,
/// and has a row in the scenario table of <c>tooling/tests/README.md</c>, which states both rules.
/// </summary>
[Trait(Kind.Of, Kind.Repository)]
public partial class FixtureReadmeTests
{
    private static readonly string FixturesDir = Path.Combine(Repo.Root, "tooling", "tests", "fixtures");
    private static readonly string SuiteReadme = Path.Combine(Repo.Root, "tooling", "tests", "README.md");

    [Fact]
    public void Every_fixture_says_what_it_proves()
    {
        var fixtures = Directory.EnumerateDirectories(FixturesDir).ToList();

        // A run that finds no fixture checks nothing.
        Assert.NotEmpty(fixtures);

        var withoutReadme = fixtures
            .Where(dir => !File.Exists(Path.Combine(dir, "README.md")))
            .Select(Path.GetFileName)
            .Order(StringComparer.Ordinal)
            .ToList();

        Assert.True(withoutReadme.Count == 0,
            "a fixture has no README.md saying what it proves:\n  "
            + string.Join("\n  ", withoutReadme)
            + "\nWrite one beside its corpus. tooling/tests/README.md says what it states.");
    }

    [Fact]
    public void The_scenario_table_has_one_row_per_fixture()
    {
        var fixtures = Directory.EnumerateDirectories(FixturesDir)
            .Select(dir => Path.GetFileName(dir))
            .ToHashSet(StringComparer.Ordinal);
        var rows = ScenarioRows(File.ReadAllLines(SuiteReadme)).ToHashSet(StringComparer.Ordinal);

        // A table the parser cannot find checks nothing.
        Assert.NotEmpty(rows);

        var faults = fixtures.Except(rows)
            .Select(name => $"fixture '{name}' has no row in the scenario table.")
            .Concat(rows.Except(fixtures).Select(name => $"row '{name}' names no fixture directory."))
            .Order(StringComparer.Ordinal)
            .ToList();

        Assert.True(faults.Count == 0,
            "tooling/tests/README.md is out of step with tooling/tests/fixtures/:\n  "
            + string.Join("\n  ", faults)
            + "\nAdd or remove the row under '## The scenarios'. Its prose is yours to write.");
    }

    /// <summary>
    /// The scenario named in the first cell of each row under <c>## The scenarios</c>, up to the next heading.
    /// </summary>
    private static IEnumerable<string> ScenarioRows(IEnumerable<string> lines) =>
        lines.SkipWhile(line => line != "## The scenarios")
            .Skip(1)
            .TakeWhile(line => !line.StartsWith('#'))
            .Select(line => FirstCell().Match(line))
            .Where(match => match.Success)
            .Select(match => match.Groups[1].Value);

    [GeneratedRegex(@"^\|\s*`([^`]+)`\s*\|")]
    private static partial Regex FirstCell();
}
