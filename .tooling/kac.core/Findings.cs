// ---------------------------------------------------------------------------
// Findings
// ---------------------------------------------------------------------------

namespace kac.core;

public enum Sev
{
    Error,
    Warning
}

public record Finding(string File, int? Line, Sev Severity, string Check, string Message);

// The registry of every check the validator can emit. It exists so the test suite can assert
// coverage (every id here must be triggered by a fixture) and so a human can `kac checks` to see
// the ruleset. Adding a new Err/Warn check to the validator means adding its id here — the
// coverage meta-test fails on any id emitted in a fixture golden that is missing from this list,
// which keeps the two honest. Ordered roughly as a document is checked, then corpus-wide.
public readonly record struct CheckDef(string Id, Sev Severity, string Summary);

public static class CheckCatalogue
{
    public static readonly IReadOnlyList<CheckDef> All =
    [
        new("type", Sev.Error, "The document's folder maps to a type schema."),
        new("frontmatter-parses", Sev.Error, "The frontmatter block is valid YAML and a mapping."),
        new("unknown-key", Sev.Error, "Every frontmatter key is a known field or reserved key."),
        new("key-order", Sev.Error, "Key order is a topological extension of the schema's field orders."),
        new("required-field", Sev.Error, "Every required (and required-when) field is present."),
        new("bare-key", Sev.Error, "An absent value is a bare key, not null / ~ / \"\" / —."),
        new("date-quoted", Sev.Error, "A date field is a quoted string."),
        new("date-format", Sev.Error, "A date field is YYYY-MM-DD in shape."),
        new("enum", Sev.Error, "An enum value is a scalar in the declared range."),
        new("enum-lowercase", Sev.Error, "An enum value is lowercase."),
        new("list", Sev.Error, "A list field is a YAML sequence."),
        new("field-pattern", Sev.Error, "A field value matches the pattern the schema declares for it."),
        new("tier-matches-type", Sev.Error, "tier equals the tier the type declares."),
        new("id-prefix", Sev.Error, "id carries the type's prefix."),
        new("id-format", Sev.Error, "id has the type's prefix and numeric width."),
        new("id-matches-filename", Sev.Error, "id's number matches the filename's number."),
        new("filename-pattern", Sev.Error, "The filename matches the type's filename pattern."),
        new("slug-length", Sev.Error, "The slug is within the type's slug-max."),
        new("h1", Sev.Error, "The document has an H1."),
        new("h1-pattern", Sev.Error, "The H1 matches the type's h1-pattern."),
        new("h1-matches-id", Sev.Error, "The H1 opens with the document's id, written as a code span."),
        new("required-section", Sev.Error, "Every required section heading is present."),
        new("link-resolves", Sev.Error, "Every internal link resolves."),
        new("undefined-label", Sev.Error, "A shortcut reference has a link definition."),
        new("label-canonical", Sev.Error, "An id-shaped shortcut label is written as the canonical id."),
        new("related-matches-section", Sev.Error, "A mirrors-section field reconciles with its section."),
        new("id-unique", Sev.Error, "id is unique across the whole wiki."),
        new("reciprocal", Sev.Error, "A reciprocal field agrees in both directions."),
        new("list-order", Sev.Warning, "A list field's entries are in alphabetical order."),
        new("unused-definition", Sev.Warning, "A link definition that nothing references."),
        new("bracket-literal", Sev.Warning, "A [...] in prose that looks like a broken reference."),
        new("y-statement", Sev.Warning, "A short Y-statement block-quote follows the H1."),
        new("alternatives-verdict", Sev.Warning, "Each Alternatives Considered bullet states an outcome.")
    ];
}
