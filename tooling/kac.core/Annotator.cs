namespace kac.core;

public enum CiPlatform
{
    GitHub,
    AzurePipelines
}

/// <summary>
/// Writes a finding as the line a CI platform reads as an annotation, and the findings as the body of the pull request
/// comment. Each prefix is the folder's path from the root of its git repository, ending in <c>/</c>, and is empty at
/// the root: <c>CorpusPrefix</c> places a file in the corpus, and <c>SchemaPrefix</c> places one under <c>.schema/</c>.
/// </summary>
public sealed record Annotator(CiPlatform Platform, string CorpusPrefix, string SchemaPrefix)
{
    /// <summary>
    /// A Markdown table of the errors and warnings, in file order, under a heading with their counts. A run with
    /// neither is one line, which is what an earlier comment is updated to once its findings have gone. Each icon sits
    /// beside the word it repeats, as std-A11Y.COLOUR asks of colour. <paramref name="corpus"/> names it in the
    /// heading, from <see cref="Named"/>.
    /// </summary>
    public string Table(string corpus, IReadOnlyList<Finding> findings)
    {
        var shown = findings.Where(f => f.Severity is Sev.Error or Sev.Warning)
            .OrderBy(f => f.File).ThenBy(f => f.Line ?? 0).ToList();
        if (shown.Count == 0)
            return $"### ✅ `kac validate` for {corpus}: the corpus validated with no errors or warnings\n";

        var errors = shown.Count(f => f.Severity is Sev.Error);
        var warnings = shown.Count - errors;
        var summary = new System.Text.StringBuilder()
            .Append($"### {(errors > 0 ? ErrorIcon : WarningIcon)} `kac validate` for {corpus}: ")
            .Append($"{Counted(errors, "error")}, {Counted(warnings, "warning")}\n\n")
            .Append("| Severity | File | Check | Message |\n")
            .Append("|----------|------|-------|---------|\n");

        foreach (var f in shown)
        {
            var at = Path(f.File) + (f.Line is { } n ? $":{n}" : "");
            var severity = f.Severity is Sev.Error ? $"{ErrorIcon} error" : $"{WarningIcon} warning";
            summary.Append($"| {severity} | `{Cell(at)}` | `{Cell(f.Check.Value)}` | {Cell(f.Message)} |\n");
        }

        return summary.ToString();

        static string Counted(int n, string noun) => $"{n} {noun}{(n == 1 ? "" : "s")}";
    }

    /// <summary>
    /// What the comment calls the corpus: the <c>corpus:</c> its descriptor <paramref name="declared"/>, else its path
    /// in the repository, else "this corpus" at the root.
    /// </summary>
    public string Named(string? declared) =>
        declared is { Length: > 0 } ? declared
        : CorpusPrefix.TrimEnd('/') is { Length: > 0 } path ? path
        : "this corpus";

    private const string ErrorIcon = "❌", WarningIcon = "⚠️";

    /// <summary>A pipe would end the cell and a line break the row.</summary>
    private static string Cell(string text) =>
        text.Replace("|", "\\|").Replace("\r\n", "<br>").Replace("\n", "<br>").Replace("\r", "<br>");

    /// <summary>
    /// The platform running this process, read through <paramref name="variable"/>, or null for a local run. GitHub
    /// Actions sets <c>GITHUB_ACTIONS</c> to <c>true</c>, and Azure Pipelines sets <c>TF_BUILD</c> to <c>True</c>.
    /// </summary>
    public static CiPlatform? Detect(Func<string, string?> variable)
    {
        if (variable("GITHUB_ACTIONS") == "true") return CiPlatform.GitHub;
        if (string.Equals(variable("TF_BUILD"), "true", StringComparison.OrdinalIgnoreCase))
            return CiPlatform.AzurePipelines;
        return null;
    }

    /// <summary>Null for an info finding: Azure Pipelines has no third type, so neither platform is sent one.</summary>
    public string? Line(Finding finding)
    {
        var kind = finding.Severity switch
        {
            Sev.Error => "error",
            Sev.Warning => "warning",
            _ => null
        };
        if (kind is null) return null;

        var file = Path(finding.File);
        var check = finding.Check.Value;

        return Platform switch
        {
            CiPlatform.GitHub =>
                $"::{kind} file={GitHubProperty(file)}"
                + (finding.Line is { } n ? $",line={n}" : "")
                + $",title={GitHubProperty(check)}::{GitHubData(finding.Message)}",
            CiPlatform.AzurePipelines =>
                $"##vso[task.logissue type={kind};sourcepath={AzureProperty(file)}"
                + (finding.Line is { } m ? $";linenumber={m}" : "")
                + $";code={AzureProperty(check)}]{AzureData(finding.Message)}",
            _ => throw new InvalidOperationException($"no annotation is written for {Platform}.")
        };
    }

    private string Path(string file) =>
        (file.StartsWith(".schema/", StringComparison.Ordinal) ? SchemaPrefix : CorpusPrefix) + file;

    /// <summary>
    /// The escapes <c>@actions/core</c> writes. The percent sign goes first, or it would escape the escapes.
    /// </summary>
    private static string GitHubData(string text) =>
        text.Replace("%", "%25").Replace("\r", "%0D").Replace("\n", "%0A");

    private static string GitHubProperty(string text) =>
        GitHubData(text).Replace(":", "%3A").Replace(",", "%2C");

    /// <summary>The escapes <c>azure-pipelines-task-lib</c> writes, percent sign first.</summary>
    private static string AzureData(string text) =>
        text.Replace("%", "%AZP25").Replace("\r", "%0D").Replace("\n", "%0A");

    private static string AzureProperty(string text) =>
        AzureData(text).Replace(";", "%3B").Replace("]", "%5D");
}
