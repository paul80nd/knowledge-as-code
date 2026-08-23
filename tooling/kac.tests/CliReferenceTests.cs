// The CLI reference pages restate the command line, which is the one thing in this repository that a page cannot be
// left to state for itself. A hand-written usage drifts from the parser inside two pull requests and nothing notices.
//
// So each page carries a generated block, and these hold it to the parser's own command model. No `kac docs` verb
// stands behind it, because a verb no corpus would ever run is weight every consumer carries. `kac generate` does not
// write it either: plenty of corpora have a `docs/` folder, and rewriting one would be a bad surprise.
//
// Set `KAC_UPDATE_DOCS=1` to rewrite the blocks instead of asserting on them, which is how a command that grew an
// option gets its page back in step. Read the diff afterwards, because the update blesses a regression as happily as
// a fix.

using System.Text.RegularExpressions;

namespace kac.tests;

public partial class CliReferenceTests
{
    // Written before their commands exist, so the parser knows nothing about them. Each says so at its head, and the
    // test below holds it to saying so. An exception that stops being true is an exception that stops being silent.
    private static readonly string[] NoCommandYet = ["new", "update"];

    private static bool Updating => Environment.GetEnvironmentVariable("KAC_UPDATE_DOCS") == "1";

    // The first heading of a page, after which a missing block is inserted.
    [GeneratedRegex(@"^# .*$", RegexOptions.Multiline)]
    private static partial Regex Heading();

    [Fact]
    public void Every_command_the_parser_declares_has_a_page()
    {
        var declared = CliReference.Verbs().Select(v => v.Name).ToHashSet(StringComparer.Ordinal);
        var pages = Directory.EnumerateFiles(CliReference.Cli, "*.md")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => !NoCommandYet.Contains(name, StringComparer.Ordinal))
            .ToHashSet(StringComparer.Ordinal)!;

        Assert.Equal(declared.Order(StringComparer.Ordinal), pages.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void A_page_without_a_command_says_it_is_a_specification()
    {
        foreach (var name in NoCommandYet)
        {
            var page = File.ReadAllText(Path.Combine(CliReference.Cli, name + ".md"));
            Assert.Contains("**Draft, pending implementation.**", page, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Each_page_carries_the_usage_the_parser_would_accept()
    {
        var stale = new List<string>();

        foreach (var verb in CliReference.Verbs())
        {
            var path = Path.Combine(CliReference.Cli, verb.Name + ".md");
            var page = File.ReadAllText(path);
            var wanted = Replaced(page, verb);

            if (page == wanted) continue;

            if (Updating) File.WriteAllText(path, wanted);
            else stale.Add($"docs/cli/{verb.Name}.md");
        }

        Assert.True(stale.Count == 0 || Updating,
            $"the usage block is stale in {string.Join(", ", stale)}. "
            + "Run: KAC_UPDATE_DOCS=1 dotnet test tooling/kac.tests");
    }

    // The page with its block replaced, or with one inserted below the heading where it has none yet.
    private static string Replaced(string page, CliReference.Verb verb)
    {
        var begin = CliReference.BeginMarker(verb.Name);
        var end = CliReference.EndMarker(verb.Name);
        var block = $"{begin}\n\n{CliReference.Render(verb)}\n{end}";

        var from = page.IndexOf(begin, StringComparison.Ordinal);
        var to = page.IndexOf(end, StringComparison.Ordinal);

        if (from >= 0 && to > from)
            return page[..from] + block + page[(to + end.Length)..];

        var heading = Heading().Match(page);
        Assert.True(heading.Success, $"docs/cli/{verb.Name}.md has no heading to put a usage block under.");

        return page[..(heading.Index + heading.Length)] + "\n\n" + block + page[(heading.Index + heading.Length)..];
    }
}
