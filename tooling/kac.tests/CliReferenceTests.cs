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
            .Where(name => name is not "index")
            .Where(name => !CliReference.Unbuilt.Contains(name, StringComparer.Ordinal))
            .ToHashSet(StringComparer.Ordinal)!;

        Assert.Equal(declared.Order(StringComparer.Ordinal), pages.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void A_page_without_a_command_says_it_is_a_specification()
    {
        foreach (var name in CliReference.Unbuilt)
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
            var wanted = Replaced(page, "usage-" + verb.Name, CliReference.Render(verb));

            if (page == wanted) continue;

            if (Updating) File.WriteAllText(path, wanted);
            else stale.Add($"docs/cli/{verb.Name}.md");
        }

        Assert.True(stale.Count == 0 || Updating,
            $"the usage block is stale in {string.Join(", ", stale)}. "
            + "Run: KAC_UPDATE_DOCS=1 dotnet test tooling/kac.tests");
    }

    // The overview indexes every page of the reference, and takes each row's wording from that page's own heading. So
    // a command whose page is added, renamed or retitled drops out of step here rather than quietly out of the list.
    [Fact]
    public void The_overview_indexes_every_page()
    {
        var page = File.ReadAllText(CliReference.Index);
        var wanted = Replaced(page, "command-table", CliReference.CommandTable());

        if (page == wanted) return;

        if (Updating) File.WriteAllText(CliReference.Index, wanted);
        else Assert.Fail("the command table is stale in docs/cli/index.md. "
                         + "Run: KAC_UPDATE_DOCS=1 dotnet test tooling/kac.tests");
    }

    // The page with its block replaced, or with one inserted below the heading where it has none yet.
    private static string Replaced(string page, string name, string body)
    {
        var begin = CliReference.BeginMarker(name);
        var end = CliReference.EndMarker(name);
        var block = $"{begin}\n\n{body}\n{end}";

        var from = page.IndexOf(begin, StringComparison.Ordinal);
        var to = page.IndexOf(end, StringComparison.Ordinal);

        if (from >= 0 && to > from)
            return page[..from] + block + page[(to + end.Length)..];

        var heading = Heading().Match(page);
        Assert.True(heading.Success, $"the page carrying '{name}' has no heading to put a generated block under.");

        return page[..(heading.Index + heading.Length)] + "\n\n" + block + page[(heading.Index + heading.Length)..];
    }
}
