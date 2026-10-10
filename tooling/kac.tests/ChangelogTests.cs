using System.Text.RegularExpressions;

// `publish-tool.yml` publishes whenever `kac.csproj` names a version nuget.org does not hold, and opens a
// release whose body is this changelog's section for that version. Nothing else asks for that section, so
// a version moved without one publishes cleanly and leaves an empty release behind it.

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public partial class ChangelogTests
{
    private static readonly string Kac = Path.Combine(Repo.Root, "tooling", "kac");
    private static readonly string Project = File.ReadAllText(Path.Combine(Kac, "kac.csproj"));
    private static readonly string Changelog = File.ReadAllText(Path.Combine(Kac, "CHANGELOG.md"));

    [GeneratedRegex(@"<Version>([^<]+)</Version>")]
    private static partial Regex ProjectVersion();

    [GeneratedRegex(@"^## .*$", RegexOptions.Multiline)]
    private static partial Regex SectionHeading();

    [Fact]
    public void The_changelog_holds_a_section_for_the_version_the_project_names()
    {
        var version = Version();

        Assert.True(
            Sections().Any(s => s.Heading.StartsWith(Opening(version), StringComparison.Ordinal)),
            $"kac.csproj publishes {version}, and CHANGELOG.md has no '{Opening(version)}<date>' section.");
    }

    [Fact]
    public void That_section_says_what_the_version_carries()
    {
        var version = Version();

        Assert.False(
            string.IsNullOrEmpty(Body(version)),
            $"'{Opening(version)}…' in CHANGELOG.md is empty, and its text is the body of the release.");
    }

    [Fact]
    public void The_site_says_the_tool_is_below_one_while_it_is()
    {
        var index = File.ReadAllText(Path.Combine(Repo.Root, "docs", "index.md"));
        var below = Version().StartsWith("0.", StringComparison.Ordinal);

        Assert.True(below == index.Contains("sits below `1.0.0`", StringComparison.Ordinal),
            below
                ? "kac.csproj publishes a 0.x, and docs/index.md no longer says the version sits below 1.0.0."
                : $"kac.csproj publishes {Version()}, and docs/index.md still says the version sits below 1.0.0. "
                  + "Rewrite the Maturity section for a released tool.");
    }

    private static string Version()
    {
        var match = ProjectVersion().Match(Project);

        return match.Success
            ? match.Groups[1].Value.Trim()
            : throw new InvalidOperationException(
                "no '<Version>' in kac.csproj: this pass reads the changelog against it.");
    }

    // A released section opens `## 0.1.1 - 2026-08-20`. `## Unreleased` carries no date and is deliberately not one.
    private static string Opening(string version) => $"## {version} - ";

    private static IEnumerable<(string Heading, string Body)> Sections()
    {
        var headings = SectionHeading().Matches(Changelog);

        return headings.Select((heading, i) =>
        {
            var end = i + 1 < headings.Count ? headings[i + 1].Index : Changelog.Length;

            return (heading.Value.Trim(), Changelog[(heading.Index + heading.Length)..end].Trim());
        });
    }

    private static string? Body(string version) =>
        Sections()
            .Where(s => s.Heading.StartsWith(Opening(version), StringComparison.Ordinal))
            .Select(s => s.Body)
            .FirstOrDefault();
}
