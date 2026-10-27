// Indented JSON otherwise ends each line with `Environment.NewLine`, so an export built on Windows differs byte for
// byte from the Linux build. On Linux and macOS the two agree, so only the source can show a missing setting there.

using System.Text.RegularExpressions;

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public partial class JsonNewLineTests
{
    [Fact]
    public void Every_indented_JSON_setting_in_the_tool_ends_its_lines_with_a_line_feed()
    {
        var found = new List<string>();

        foreach (var file in Sources())
        {
            var text = File.ReadAllText(file);
            var indented = Indented().Count(text);
            var lineFeeds = LineFeed().Count(text);
            if (indented > lineFeeds)
                found.Add($"{Path.GetRelativePath(Repo.Root, file)}: {indented} WriteIndented, {lineFeeds} NewLine");
        }

        Assert.True(found.Count == 0,
            "Each WriteIndented needs a NewLine = \"\\n\" beside it, so the bytes match on every platform:\n  "
            + string.Join("\n  ", found));
    }

    private static IEnumerable<string> Sources() =>
        new[] { "kac", "kac.core" }
            .SelectMany(project => Directory.EnumerateFiles(Path.Combine(Repo.Root, "tooling", project), "*.cs",
                SearchOption.AllDirectories))
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                        && !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"));

    [GeneratedRegex(@"\bWriteIndented\s*=")]
    private static partial Regex Indented();

    [GeneratedRegex(@"\bNewLine\s*=\s*""\\n""")]
    private static partial Regex LineFeed();
}
