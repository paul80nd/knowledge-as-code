namespace kac.tests;

/// <summary>
/// Every golden-suite fixture states what it proves in a <c>README.md</c>, which a regenerated golden cannot take away.
/// <c>tooling/tests/README.md</c> states the rule.
/// </summary>
[Trait(Kind.Of, Kind.Repository)]
public class FixtureReadmeTests
{
    [Fact]
    public void Every_fixture_says_what_it_proves()
    {
        var fixtures = Directory.EnumerateDirectories(Path.Combine(Repo.Root, "tooling", "tests", "fixtures"))
            .ToList();

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
}
