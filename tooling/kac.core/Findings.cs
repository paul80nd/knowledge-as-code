namespace kac.core;

/// <summary>
/// <c>Error</c> fails the build and the other two do not. <c>Info</c> is for something true that is nobody's fault,
/// such as a corpus capped at a version deliberately.
/// </summary>
public enum Sev
{
    Error,
    Warning,
    Info
}

public record Finding(string File, int? Line, Sev Severity, CheckId Check, string Message);

/// <summary>
/// Passes with no one file to read, such as <c>id-unique</c> and <c>min-records</c>, build their <c>Finding</c>s
/// directly.
/// </summary>
public readonly record struct Report(string File, List<Finding> Findings)
{
    public void Err(CheckId check, string message, int? line = null) =>
        Findings.Add(new Finding(File, line, Sev.Error, check, message));

    public void Warn(CheckId check, string message, int? line = null) =>
        Findings.Add(new Finding(File, line, Sev.Warning, check, message));
}

/// <summary>
/// <c>OnTypePage</c> says whether the check belongs in the table generated onto a type page, which lists what a
/// record's author can act on. True unless a check says otherwise.
/// </summary>
public readonly record struct CheckDef(
    CheckId Id,
    Sev Severity,
    string Summary,
    string Notes = "",
    bool OnTypePage = true);

public static class CheckCatalogue
{
    /// <summary>
    /// Every check <c>_checks.yaml</c> declares, in its order, then one per expression rule grouped by type. An
    /// expression rule reports under its own id, so the coverage gate holds it to a fixture like any check.
    /// </summary>
    public static IReadOnlyList<CheckDef> For(Schema schema) =>
    [
        .. schema.Checks,
        .. schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal)
            .SelectMany(kv => kv.Value.Rules)
            .Where(r => r.Compiled is not null)
            .Select(r => new CheckDef(new CheckId(r.Id.Value), r.Severity ?? Sev.Warning,
                r.Description ?? r.Message ?? r.Id.Value))
    ];

    /// <summary>Every other check reports through a literal written where the check is.</summary>
    public static IReadOnlyList<CheckId> EmittedByRules() =>
    [
        .. DocumentRules.All.SelectMany(r => r.Emits),
        .. CorpusRules.All.SelectMany(r => r.Emits)
    ];

    /// <summary>
    /// Every config key a rule class reads. The loader asks a rule's entry for these alone, so any other key is unread.
    /// </summary>
    public static IReadOnlyList<string> ReadByRules() =>
    [
        .. DocumentRules.All.SelectMany(r => r.Reads)
            .Concat(CorpusRules.All.SelectMany(r => r.Reads))
            .Distinct(StringComparer.Ordinal)
    ];
}
