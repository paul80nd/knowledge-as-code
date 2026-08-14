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

// One check as the schema declares it. `Summary` is what a reader meets, which `kac checks` prints,
// and `Notes` is the reasoning and the boundary, which only someone reading the schema wants.
//
// `OnTypePage` says whether the check belongs in the "What CI checks" table generated onto a type
// page. That table tells whoever writes a record what their document is held to, so a check reading
// the schema, the template or the page itself has no row there — it is real, and it is not theirs to
// act on. True unless a check says otherwise.
public readonly record struct CheckDef(
    CheckId Id,
    Sev Severity,
    string Summary,
    string Notes = "",
    bool OnTypePage = true);

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

    // The check ids the rule classes report under — the code side of the catalogue, and the whole of it
    // that a registry can name. Every other check reports through a literal written where the check is.
    public static IReadOnlyList<CheckId> EmittedByRules() =>
    [
        .. DocumentRules.All.SelectMany(r => r.Emits),
        .. CorpusRules.All.SelectMany(r => r.Emits)
    ];
}
