using kac.core;
using Reqnroll;
using Xunit;

namespace kac.features;

[Binding]
public sealed class ValidationSteps
{
    private string _fixture = "";
    private ValidationResult? _result;

    private ValidationResult Result =>
        _result ?? throw new InvalidOperationException(
            "validate the corpus first (missing 'When I validate the corpus')");

    [Given("the (.*) fixture corpus")]
    public void GivenTheFixtureCorpus(string name) => _fixture = name;

    [When("I validate the corpus")]
    public void WhenIValidateTheCorpus() => _result = Harness.Validate(_fixture);

    [Then(@"validation reports (\d+) documents? and (\d+) skipped")]
    public void ThenValidationReports(int validated, int skipped)
    {
        Assert.Equal(validated, Result.Validated);
        Assert.Equal(skipped, Result.Skipped);
    }

    [Then("no findings are reported")]
    public void ThenNoFindingsAreReported() => Assert.Empty(Result.Findings);

    [Then("no warnings are reported")]
    public void ThenNoWarningsAreReported()
        => Assert.DoesNotContain(Result.Findings, f => f.Severity == Sev.Warning);

    // A stray or missing finding on the file fails. A table with no `severity` column reads every row as an error.
    [Then(@"the findings for ""(.*)"" are exactly:")]
    public void ThenTheFindingsForFileAreExactly(string file, Table table)
    {
        var sev = table.Header.Contains("severity");
        var actual = Result.Findings.Where(f => f.File == file).Select(Row).Order().ToList();
        var expected = table.Rows
            .Select(r => (sev ? r["severity"] : "error", Line(r["line"]), r["check"], r["message"]))
            .Order().ToList();
        Assert.Equal(expected, actual);
    }

    [Then("the findings are exactly:")]
    public void ThenTheFindingsAreExactly(Table table)
    {
        var sev = table.Header.Contains("severity");
        var actual = Result.Findings.Select(f => (f.File, Row(f))).Order().ToList();
        var expected = table.Rows
            .Select(r => (r["file"], (sev ? r["severity"] : "error", Line(r["line"]), r["check"], r["message"])))
            .Order().ToList();
        Assert.Equal(expected, actual);
    }

    private static (string, int?, string, string) Row(Finding f)
        => (f.Severity.ToString().ToLowerInvariant(), f.Line, f.Check.Value, f.Message);

    private static int? Line(string cell) => string.IsNullOrWhiteSpace(cell) ? null : int.Parse(cell);
}
