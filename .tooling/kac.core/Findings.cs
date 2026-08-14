// ---------------------------------------------------------------------------
// Findings
// ---------------------------------------------------------------------------

namespace kac.core;

public enum Sev
{
    Error,
    Warning
}

public record Finding(string File, int? Line, Sev Severity, CheckId Check, string Message);

// One check as the schema declares it. `Summary` is what a reader meets — in `kac checks` and in the
// generated tables; `Notes` is the reasoning and the boundary, which only someone reading the schema
// wants; `Group` is the concern it belongs to, and so the table it renders into.
public readonly record struct CheckDef(CheckId Id, Sev Severity, string Summary, string Group = "", string Notes = "");

public static class CheckCatalogue
{
    // The catalogue as it stands for a given corpus: every check `_checks.yaml` declares, which each
    // corpus takes with the schema, plus one entry per expression rule its own type files declare. A
    // rule with an `expr:` reports under its own id, so it is a check like any other — it appears in
    // `kac checks`, and the coverage gate holds it to the same requirement of a fixture exercising it.
    //
    // The declaration is the schema's and what runs is the code's, which is the division every other
    // part of the schema is under.
    //
    // Ordered so the shared checks keep the sequence `_checks.yaml` declares them in — a document read
    // top to bottom — and the schema's own rules follow, grouped by the type that declares them.
    public static IReadOnlyList<CheckDef> For(Schema schema) =>
    [
        .. schema.Checks,
        .. schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal)
            .SelectMany(kv => kv.Value.Rules)
            .Where(r => r.Compiled is not null)
            .Select(r => new CheckDef(new CheckId(r.Id.Value), r.Severity ?? Sev.Warning,
                r.Description ?? r.Message ?? r.Id.Value))
    ];
}
