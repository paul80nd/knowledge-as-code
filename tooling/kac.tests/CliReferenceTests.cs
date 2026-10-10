using System.Text.RegularExpressions;

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public partial class CliReferenceTests
{
    private static bool Updating => Environment.GetEnvironmentVariable("KAC_UPDATE_DOCS") == "1";

    [GeneratedRegex(@"^```mermaid$.*?^```$", RegexOptions.Multiline | RegexOptions.Singleline)]
    private static partial Regex Mermaid();

    // A node whose label is a command, as `kac validate`.
    [GeneratedRegex(@"\bkac (?<verb>[a-z]+)\b")]
    private static partial Regex Invocation();

    // One node declaration, as `id[label];` or `id(label);`. An edge line carries no bracket.
    [GeneratedRegex(@"^\s*\w+(?<open>[\[(])(?<label>[^\])]+)[\])];$", RegexOptions.Multiline)]
    private static partial Regex Node();

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
            var wanted = GeneratedPage.Replaced(page, "usage-" + verb.Name, CliReference.Render(verb));

            if (page == wanted) continue;

            if (Updating) File.WriteAllText(path, wanted);
            else stale.Add($"docs/cli/{verb.Name}.md");
        }

        Assert.True(stale.Count == 0 || Updating,
            $"the usage block is stale in {string.Join(", ", stale)}. "
            + "Run: KAC_UPDATE_DOCS=1 dotnet test tooling/kac.tests");
    }

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

    // The verbs the flow chart leaves out, because neither is a step in any sequence. A verb added to the
    // parser fails the test below until somebody decides which it is.
    private static readonly string[] NotSteps = ["checks", "report"];

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

    [Fact]
    public void A_command_is_drawn_as_a_box_and_everything_else_is_rounded()
    {
        var chart = Mermaid().Match(File.ReadAllText(CliReference.Index)).Value;

        foreach (Match node in Node().Matches(chart))
        {
            var command = node.Groups["label"].Value.StartsWith("kac ", StringComparison.Ordinal);
            Assert.Equal(command ? "[" : "(", node.Groups["open"].Value);
        }
    }

    [Fact]
    public void The_overview_indexes_every_page()
    {
        var page = File.ReadAllText(CliReference.Index);
        var wanted = GeneratedPage.Replaced(page, "command-table", CliReference.CommandTable());

        if (page == wanted) return;

        if (Updating) File.WriteAllText(CliReference.Index, wanted);
        else
            Assert.Fail("the command table is stale in docs/cli/index.md. "
                        + "Run: KAC_UPDATE_DOCS=1 dotnet test tooling/kac.tests");
    }
}
