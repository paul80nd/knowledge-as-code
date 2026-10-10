namespace kac.core;

// `Error` fails the build and the other two do not. `Info` is for something true that is nobody's
// fault, such as a corpus capped at a version deliberately.
public enum Sev
{
    Error,
    Warning,
    Info
}

public record Finding(string File, int? Line, Sev Severity, CheckId Check, string Message);

// Passes with no one file to read, such as `id-unique` and `min-records`, build their `Finding`s
// directly.
public readonly record struct Report(string File, List<Finding> Findings)
{
    public void Err(CheckId check, string message, int? line = null) =>
        Findings.Add(new Finding(File, line, Sev.Error, check, message));

    public void Warn(CheckId check, string message, int? line = null) =>
        Findings.Add(new Finding(File, line, Sev.Warning, check, message));
}

// `OnTypePage` says whether the check belongs in the table generated onto a type page, which lists what
// a record's author can act on. True unless a check says otherwise.
public readonly record struct CheckDef(
    CheckId Id,
    Sev Severity,
    string Summary,
    string Notes = "",
    bool OnTypePage = true);

public static class CheckCatalogue
{
    // Every check `_checks.yaml` declares, in its order, then one per expression rule grouped by type. An
    // expression rule reports under its own id, so the coverage gate holds it to a fixture like any check.
    public static IReadOnlyList<CheckDef> For(Schema schema) =>
    [
        .. schema.Checks,
        .. schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal)
            .SelectMany(kv => kv.Value.Rules)
            .Where(r => r.Compiled is not null)
            .Select(r => new CheckDef(new CheckId(r.Id.Value), r.Severity ?? Sev.Warning,
                r.Description ?? r.Message ?? r.Id.Value))
    ];

    // Every other check reports through a literal written where the check is.
    public static IReadOnlyList<CheckId> EmittedByRules() =>
    [
        .. DocumentRules.All.SelectMany(r => r.Emits),
        .. CorpusRules.All.SelectMany(r => r.Emits)
    ];
}
