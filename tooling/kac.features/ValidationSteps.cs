using kac.core;
using Reqnroll;
using Xunit;

namespace kac.features;

[Binding]
public sealed class ValidationSteps(Workspace ws)
{
    private ValidationResult? _result;

    private ValidationResult Result =>
        _result ?? throw new InvalidOperationException(
            "validate the corpus first (missing 'When I validate the corpus')");

    [When("I validate the corpus")]
    public void WhenIValidateTheCorpus() => _result = Harness.Validate(ws.Root);

    [When(@"I validate the corpus in ""(.*)""")]
    public void WhenIValidateTheCorpusIn(string folder) => _result = Harness.Validate(ws.PathOf(folder));

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

    /// <summary>
    /// A stray or missing finding on the file fails. A table with no <c>severity</c> column reads every row as an
    /// error.
    /// </summary>
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

    /// <summary>A source answers from the folder it names, and a scenario reaching for the network fails.</summary>
    [When("I validate the corpus against its sources")]
    public void WhenIValidateTheCorpusAgainstItsSources()
        => _result = Harness.Validate(ws.Root, new Registry(
            url => throw new InvalidOperationException($"a validate spec reached for the network: {url}"),
            Registry.OnDisk(ws.Root)));

    [Then(@"no findings are reported for ""(.*)""")]
    public void ThenNoFindingsAreReportedFor(string file)
        => Assert.DoesNotContain(Result.Findings, f => f.File == file);

    [Then(@"no finding is reported under ""(.*)""")]
    public void ThenNoFindingIsReportedUnder(string folder)
        => Assert.DoesNotContain(Result.Findings, f => f.File.StartsWith(folder + "/", StringComparison.Ordinal));

    private static (string, int?, string, string) Row(Finding f)
        => (f.Severity.ToString().ToLowerInvariant(), f.Line, f.Check.Value, f.Message);

    private static int? Line(string cell) => string.IsNullOrWhiteSpace(cell) ? null : int.Parse(cell);
}
