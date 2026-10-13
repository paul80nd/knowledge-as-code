using System.Text.RegularExpressions;
using kac.core;
using Reqnroll;
using Xunit;

namespace kac.features;

[Binding]
public sealed partial class GenerationSteps(Workspace ws)
{
    [Given(@"the generated blocks in ""(.*)"" are emptied")]
    public void GivenTheGeneratedBlocksAreEmptied(string path)
    {
        var text = ws.Read(path);
        var emptied = Block().Replace(text, "$1\n$2");
        Assert.NotEqual(text, emptied);
        ws.Write(path, emptied);
    }

    [Given(@"the descriptor adopts ""(.*)"" as well")]
    public void GivenTheDescriptorAdoptsAsWell(string type)
    {
        var descriptor = ws.Read(".corpus.yaml");
        Assert.Contains("types:\n", descriptor);
        ws.Write(".corpus.yaml", descriptor.Replace("types:\n", $"types:\n  - {type}\n"));
    }

    /// <summary>Deletes both markers and everything between them.</summary>
    [Given(@"the ""(.*)"" block in ""(.*)"" is deleted")]
    public void GivenTheBlockIsDeleted(string block, string path)
    {
        var text = ws.Read(path);
        var (begin, end) = Span(text, block, path);
        ws.Write(path, text[..begin] + text[end..]);
    }

    [Given("a first run has rewritten it")]
    public void GivenAFirstRunHasRewrittenIt()
    {
        ws.Run(root => Commands.Generate(root, check: false));
        Assert.Equal(0, ws.Exit);
        Assert.NotEmpty(ws.Changed());
    }

    [When("I generate")]
    public void WhenIGenerate() => ws.Run(root => Commands.Generate(root, check: false));

    [When("I check the generated files")]
    public void WhenICheckTheGeneratedFiles() => ws.Run(root => Commands.Generate(root, check: true));

    [Then("the generated files are fresh")]
    public void ThenTheGeneratedFilesAreFresh() => Assert.Equal(0, Commands.Generate(ws.Root, check: true));

    [Then(@"""(.*)"" begins with ""(.*)""")]
    public void ThenBeginsWith(string path, string text)
        => Assert.StartsWith(text, ws.Read(path), StringComparison.Ordinal);

    /// <summary>Each text is looked for after the one before it.</summary>
    [Then(@"""(.*)"" lists, in order:")]
    public void ThenListsInOrder(string path, Table table)
    {
        var content = ws.Read(path);
        var at = 0;
        foreach (var text in table.Rows.Select(r => r["text"]))
        {
            var found = content.IndexOf(text, at, StringComparison.Ordinal);
            Assert.True(found >= 0, $"{path} has no '{text}' after offset {at}");
            at = found + text.Length;
        }
    }

    /// <summary>One row per table, in the order the file has them, each row the header cells joined by a comma.</summary>
    [Then(@"the tables in ""(.*)"" are headed:")]
    public void ThenTheTablesAreHeaded(string path, Table table)
    {
        var lines = ws.Read(path).Split('\n');
        var headers = lines.Zip(lines.Skip(1))
            .Where(p => p.First.StartsWith('|') && p.Second.StartsWith("|-", StringComparison.Ordinal))
            .Select(p => string.Join(", ", p.First.Trim('|').Split('|').Select(c => c.Trim())))
            .ToList();
        Assert.Equal([.. table.Rows.Select(r => r["columns"])], headers);
    }

    /// <summary>
    /// Every line inside the fence is a node drawn as <c>[...]</c> or an arrow ending in <c>--&gt;</c>, under
    /// <c>graph</c>. <c>docs/reference/generation.md</c> states the subset.
    /// </summary>
    [Then(@"the types graph in ""(.*)"" keeps to the Mermaid subset")]
    public void ThenTheTypesGraphKeepsToTheMermaidSubset(string path)
    {
        var text = ws.Read(path);
        var (begin, end) = Span(text, "types-graph", path);
        var lines = text[begin..end].Split('\n')
            .SkipWhile(l => l != "```mermaid").Skip(1).TakeWhile(l => l != "```").ToList();

        Assert.NotEmpty(lines);
        Assert.Matches(@"^graph \w+;$", lines[0]);
        var body = lines.Skip(1).ToList();
        Assert.Contains(body, l => Edge().IsMatch(l));
        Assert.All(body, l => Assert.True(Node().IsMatch(l) || Edge().IsMatch(l), $"outside the subset: {l}"));
    }

    /// <summary>Where a block starts at its BEGIN marker, and where it ends after its END marker.</summary>
    private static (int Begin, int End) Span(string text, string block, string path)
    {
        var begin = text.IndexOf(Markers.Begin(block), StringComparison.Ordinal);
        var end = text.IndexOf(Markers.End(block), StringComparison.Ordinal);
        Assert.True(begin >= 0 && end > begin, $"{path} has no '{block}' block");
        return (begin, end + Markers.End(block).Length);
    }

    [GeneratedRegex(@"^\s*\w+\[[^\[\]]+\];$")]
    private static partial Regex Node();

    [GeneratedRegex(@"^\s*\w+ (-- [\w-]+ )?--> \w+;$")]
    private static partial Regex Edge();

    [GeneratedRegex(@"(<!-- BEGIN GENERATED: [^>]+ -->)[\s\S]*?(<!-- END GENERATED: [^>]+ -->)")]
    private static partial Regex Block();
}
