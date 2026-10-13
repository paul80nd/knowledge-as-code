using kac.core;
using Reqnroll;
using Xunit;

namespace kac.features;

/// <summary>
/// <c>checks</c> writes nothing and prints its catalogue, so a scenario asserts the catalogue the command prints from,
/// read through the same two calls, and the exit code the table comparison sets.
/// </summary>
[Binding]
public sealed class ChecksSteps(Workspace ws)
{
    private const string ChecksFile = ".schema/_checks.yaml";

    [Given(@"the corpus's schema declares a type ""(.*)"" whose rule ""(.*)"" checks ""(.*)""")]
    public void GivenTheCorpusSchemaDeclaresATypeWhoseRuleChecks(string type, string rule, string expr)
        => ws.Write($".schema/{type}.yaml",
            $"type: {type}\nfolder: {type}\nrules:\n  - id: {rule}\n    description: A rule of the corpus's own.\n"
            + $"    severity: warning\n    expr: \"{expr}\"\n    message: Fix the record.\n");

    [Given(@"the corpus's schema declares a check ""(.*)"" for the type page")]
    public void GivenTheCorpusSchemaDeclaresACheckForTheTypePage(string id)
        => ws.Write(ChecksFile, ws.Read(ChecksFile).TrimEnd('\n')
                                + $"\n\n  {id}:\n    severity: warning\n"
                                + "    description: A check of the corpus's own.\n");

    [Given(@"the corpus's schema keeps the check ""(.*)"" off the type page")]
    public void GivenTheCorpusSchemaKeepsTheCheckOffTheTypePage(string id)
    {
        var text = ws.Read(ChecksFile);
        var heading = $"\n  {id}:\n";
        Assert.Contains(heading, text);
        ws.Write(ChecksFile, text.Replace(heading, heading + "    on-type-page: false\n"));
    }

    /// <summary>Removes the entry's key and every line indented beneath it.</summary>
    [Given(@"the corpus's schema declares no check ""(.*)""")]
    public void GivenTheCorpusSchemaDeclaresNoCheck(string id)
    {
        var lines = ws.Read(ChecksFile).Split('\n');
        var start = Array.IndexOf(lines, $"  {id}:");
        Assert.True(start >= 0, $"{ChecksFile} declares no '{id}'");

        var end = start + 1;
        while (end < lines.Length
               && (lines[end].StartsWith("    ", StringComparison.Ordinal) || lines[end].Length == 0))
            end++;
        ws.Write(ChecksFile, string.Join('\n', lines[..start].Concat(lines[end..])));
    }

    [When("I list the checks")]
    [When("I list the checks as text")]
    public void WhenIListTheChecks() => ws.Run(root => Commands.Checks(root, json: false));

    [When("I list the checks as JSON")]
    public void WhenIListTheChecksAsJson() => ws.Run(root => Commands.Checks(root, json: true));

    [Then("the catalogue lists:")]
    public void ThenTheCatalogueLists(Table table)
    {
        var catalogue = Catalogue();
        foreach (var row in table.Rows)
            Assert.Contains(catalogue, c => c.Id.Value == row["check"]
                                            && c.Severity.ToString().ToLowerInvariant() == row["severity"]
                                            && c.Summary == row["description"]);
    }

    [Then(@"the catalogue lists ""(.*)""")]
    public void ThenTheCatalogueListsId(string id) => Assert.Contains(Catalogue(), c => c.Id.Value == id);

    [Then(@"the catalogue does not list ""(.*)""")]
    public void ThenTheCatalogueDoesNotList(string id) => Assert.DoesNotContain(Catalogue(), c => c.Id.Value == id);

    /// <summary>The two calls <c>Commands.Checks</c> makes before it prints.</summary>
    private IReadOnlyList<CheckDef> Catalogue() => CheckCatalogue.For(Schema.LoadNearest(ws.Root));
}
