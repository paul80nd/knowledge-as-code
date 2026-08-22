using System.Text.RegularExpressions;

// `publish-tool.yml` publishes whenever `kac.csproj` names a version nuget.org does not already hold, and it opens a
// GitHub release whose body is this changelog's section for that version. Nothing else in the pipeline asks for that
// section, so a version moved without one publishes cleanly and leaves an empty release behind it.
//
// These read the repository rather than a value built in the test, as `DocumentationTests` does and for the same
// reason: the fault they exist to catch is a page going quietly out of step with the code beside it.
//
// The reach is the section for the version being published, and no further. Whether that section says anything worth
// reading is a judgement, and the release it becomes is where a reader would notice.

namespace kac.tests;

public partial class ChangelogTests
{
    private static readonly string Kac = Path.Combine(Repo.Root, "tooling", "kac");
    private static readonly string Project = File.ReadAllText(Path.Combine(Kac, "kac.csproj"));
    private static readonly string Changelog = File.ReadAllText(Path.Combine(Kac, "CHANGELOG.md"));

    // The element the pack reads, not the `Version="2.0.*"` attribute a package reference carries.
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

    // A heading with nothing under it is what this pass is really for: a section added to satisfy the test above and
    // left empty, which publishes a release saying nothing.
    [Fact]
    public void That_section_says_what_the_version_carries()
    {
        var version = Version();

        Assert.False(
            string.IsNullOrEmpty(Body(version)),
            $"'{Opening(version)}…' in CHANGELOG.md is empty, and its text is the body of the release.");
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

    // Each heading with the text below it, up to the next heading.
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
