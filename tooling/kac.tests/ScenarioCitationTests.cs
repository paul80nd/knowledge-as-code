using System.Text.RegularExpressions;

namespace kac.tests;

/// <summary>
/// A specification page under <c>docs/maintaining/</c> is one with a <c>## Specified by</c> section. In it, a line
/// citing a feature file opens a group, and each bullet under it is a scenario name, written exactly.
/// <c>docs/maintaining/charter.md</c> says what a specification page is.
/// </summary>
[Trait(Kind.Of, Kind.Repository)]
public partial class ScenarioCitationTests
{
    private const string FeatureUrl = "https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/";

    [GeneratedRegex(@"^\[(?<label>[^\]]+)\]:\s*(?<url>\S+)\s*$")]
    private static partial Regex Definition();

    [GeneratedRegex(@"\[(?<label>[^\]]+)\](?!\()")]
    private static partial Regex Reference();

    [GeneratedRegex(@"^\s*Scenario(?: Outline)?:\s*(?<name>.+?)\s*$")]
    private static partial Regex Scenario();

    [Fact]
    public void Every_scenario_a_specification_page_cites_is_in_its_feature_file()
    {
        var missing = Citations()
            .Where(c => !Scenarios(c.Feature).Contains(c.Scenario))
            .Select(c => $"{c.Page} cites '{c.Scenario}', which {c.Feature} does not hold")
            .ToList();

        Assert.True(missing.Count == 0, string.Join("\n", missing));
    }

    [Fact]
    public void Every_scenario_is_cited_by_a_specification_page()
    {
        var cited = Citations().Select(c => (c.Feature, c.Scenario)).ToHashSet();
        var uncited = Directory.EnumerateFiles(FeaturesDir, "*.feature")
            .Select(Path.GetFileName)
            .OfType<string>()
            .SelectMany(f => Scenarios(f).Select(s => (Feature: f, Scenario: s)))
            .Where(fs => !cited.Contains(fs))
            .Select(fs => $"{fs.Feature}: {fs.Scenario}")
            .Order(StringComparer.Ordinal)
            .ToList();

        Assert.True(uncited.Count == 0, "no specification page cites:\n" + string.Join("\n", uncited));
    }

    [Fact]
    public void There_are_specification_pages() => Assert.NotEmpty(Pages());

    private static string FeaturesDir => Path.Combine(Repo.Root, "tooling", "kac.features");

    private static IEnumerable<string> Pages() =>
        Directory.EnumerateFiles(Path.Combine(Repo.Root, "docs", "maintaining"), "*.md", SearchOption.AllDirectories)
            .Where(p => File.ReadLines(p).Any(l => l == "## Specified by"));

    private static HashSet<string> Scenarios(string feature)
    {
        var path = Path.Combine(FeaturesDir, feature);
        if (!File.Exists(path)) return [];
        return
        [
            .. File.ReadLines(path).Select(l => Scenario().Match(l)).Where(m => m.Success)
                .Select(m => m.Groups["name"].Value)
        ];
    }

    /// <summary>
    /// A bullet before any line citing a feature file is reported as a citation of a file named <c>(none)</c>, so it
    /// fails the first fact.
    /// </summary>
    private static List<(string Page, string Feature, string Scenario)> Citations()
    {
        var found = new List<(string, string, string)>();
        foreach (var page in Pages())
        {
            var lines = File.ReadAllLines(page);
            var features = lines.Select(l => Definition().Match(l)).Where(m => m.Success)
                .Where(m => m.Groups["url"].Value.StartsWith(FeatureUrl, StringComparison.Ordinal))
                .ToDictionary(m => m.Groups["label"].Value, m => m.Groups["url"].Value[FeatureUrl.Length..]);

            var name = Path.GetRelativePath(Repo.Root, page).Replace('\\', '/');
            var feature = "(none)";
            var inSection = false;
            foreach (var line in lines)
            {
                if (line.StartsWith("## ", StringComparison.Ordinal)) inSection = line == "## Specified by";
                if (!inSection || line.Length == 0 || line.StartsWith("## ", StringComparison.Ordinal)) continue;

                if (line.StartsWith("* ", StringComparison.Ordinal))
                    found.Add((name, feature, line[2..].Trim()));
                else if (Reference().Matches(line).Select(m => m.Groups["label"].Value)
                             .FirstOrDefault(features.ContainsKey) is { } label)
                    feature = features[label];
            }
        }

        return found;
    }
}
