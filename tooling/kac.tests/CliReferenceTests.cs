// A hand-written usage drifts from the parser inside two pull requests and nothing notices, so each page
// carries a generated block and these hold it to the parser's own command model.
//
// `KAC_UPDATE_DOCS=1` rewrites the blocks instead of asserting on them. Read the diff afterwards: the
// update blesses a regression as happily as a fix.

using System.Text.RegularExpressions;

namespace kac.tests;

public partial class CliReferenceTests
{
    private static bool Updating => Environment.GetEnvironmentVariable("KAC_UPDATE_DOCS") == "1";

    // The first heading of a page, after which a missing block is inserted.
    [GeneratedRegex(@"^# .*$", RegexOptions.Multiline)]
    private static partial Regex Heading();

    // The fenced block holding the flow chart, opened by the plain fence GitHub renders natively.
    [GeneratedRegex(@"^```mermaid$.*?^```$", RegexOptions.Multiline | RegexOptions.Singleline)]
    private static partial Regex Mermaid();

    // A node whose label is a command, as `kac validate`. The chart also holds nodes that are not
    // commands, and those are what the label rather than the pattern tells apart.
    [GeneratedRegex(@"\bkac (?<verb>[a-z]+)\b")]
    private static partial Regex Invocation();

    [Fact]
    public void Every_command_the_parser_declares_has_a_page()
    {
        var declared = CliReference.Verbs().Select(v => v.Name).ToHashSet(StringComparer.Ordinal);
        var pages = Directory.EnumerateFiles(CliReference.Cli, "*.md")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => name is not "index")
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal(declared.Order(StringComparer.Ordinal), pages.Order(StringComparer.Ordinal));
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

    // The three headings a command page may carry, in the order it carries them. The set is fixed so that a reader can
    // ask the same question of any command and find the answer in the same place. `Known limits` is left out where a
    // command has none, and nothing else may be added or reordered: deeper structure goes underneath one of these
    // rather than beside it, which is what keeps a page with several worked examples from growing a fourth section.
    //
    // Why a command works the way it does is not on these pages. It is under docs/design/, which each one links, so
    // what a reader meets here is what they need to type the command and read what it printed.
    private static readonly string[] Sections = ["What it does", "Examples", "Known limits"];

    [Fact]
    public void Every_command_page_carries_the_same_sections()
    {
        foreach (var page in CliReference.Pages())
        {
            var carried = File.ReadAllLines(Path.Combine(CliReference.Cli, page + ".md"))
                .Where(l => l.StartsWith("## ", StringComparison.Ordinal))
                .Select(l => l[3..].Trim())
                .ToList();

            Assert.Equal(Sections.Where(s => carried.Contains(s, StringComparer.Ordinal)), carried);
            Assert.Equal(Sections[..2], carried.Take(2));
        }
    }

    // The verbs the flow chart deliberately leaves out, each because it is not a step in any sequence.
    // `checks` reads the schema and prints what could ever fire, which is a question about the schema.
    //
    // A list rather than an absence, so a verb added to the parser fails the test below until somebody
    // decides which it is. That is the bargain `on-type-page: false` already strikes for a check the type
    // pages do not advertise.
    private static readonly string[] NotSteps = ["checks"];

    // The chart is drawn by hand, and nothing in MkDocs would notice a verb missing from it. A new
    // command that nobody placed in the sequence is exactly what a reader comes to this page to find.
    [Fact]
    public void The_flow_chart_places_every_command_that_is_a_step()
    {
        var chart = Mermaid().Match(File.ReadAllText(CliReference.Index));
        Assert.True(chart.Success, "docs/cli/index.md carries no ```mermaid block for the flow chart.");

        var drawn = Invocation().Matches(chart.Value)
            .Select(m => m.Groups["verb"].Value)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal(
            CliReference.Verbs().Select(v => v.Name).Except(NotSteps, StringComparer.Ordinal)
                .Order(StringComparer.Ordinal),
            drawn.Order(StringComparer.Ordinal));
    }

    // Held to the subset an Azure DevOps wiki renders, which every diagram this repository writes answers
    // to. `docs/design/generation.md` argues it: a diagram exceeding that subset renders nothing at all,
    // with no error to say why. So the generator writes `graph` and this is held to the same.
    [Fact]
    public void The_flow_chart_stays_inside_the_subset_the_narrowest_renderer_reads()
    {
        var chart = Mermaid().Match(File.ReadAllText(CliReference.Index)).Value;

        Assert.StartsWith("graph ", chart[(chart.IndexOf('\n') + 1)..], StringComparison.Ordinal);
        Assert.DoesNotContain("subgraph", chart, StringComparison.Ordinal);
        Assert.DoesNotContain("-.-", chart, StringComparison.Ordinal);
        Assert.DoesNotContain("==>", chart, StringComparison.Ordinal);
    }

    // Each row's wording comes from the page it indexes, so a command whose page is added, renamed or
    // retitled drops out of step here rather than quietly out of the list.
    [Fact]
    public void The_overview_indexes_every_page()
    {
        var page = File.ReadAllText(CliReference.Index);
        var wanted = Replaced(page, "command-table", CliReference.CommandTable());

        if (page == wanted) return;

        if (Updating) File.WriteAllText(CliReference.Index, wanted);
        else
            Assert.Fail("the command table is stale in docs/cli/index.md. "
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
