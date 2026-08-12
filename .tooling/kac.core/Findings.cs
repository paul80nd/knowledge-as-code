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

// The registry of every check the validator can emit. It exists so the test suite can assert coverage
// — every id here must be triggered by a fixture — and so a human can `kac checks` to see the ruleset.
// A core check has to be added here by hand; a rule declares what it emits and is folded in below.
// Either way the coverage meta-test fails on an id a golden carries and this list does not, which
// keeps the two honest. Ordered roughly as a document is checked, then corpus-wide.
public readonly record struct CheckDef(string Id, Sev Severity, string Summary);

public static class CheckCatalogue
{
    public static readonly IReadOnlyList<CheckDef> All =
    [
        // The schema itself, before any document is read against it.
        new("schema-unknown-key", Sev.Error, "A schema key the loader never reads."),
        new("schema-unreadable", Sev.Error, "A schema declaration the loader cannot read as written."),
        new("schema-dispatch", Sev.Error, "A schema declaration naming a value nothing acts on."),
        new("schema-shape", Sev.Error, "A type's declarations are well-formed and agree with one another."),

        new("type", Sev.Error, "The document's folder maps to a type schema."),
        new("frontmatter-parses", Sev.Error, "The frontmatter block is valid YAML and a mapping."),
        new("unknown-key", Sev.Error, "Every frontmatter key is a known field or reserved key."),
        new("key-order", Sev.Error, "Key order is a topological extension of the schema's field orders."),
        new("required-field", Sev.Error, "Every required (and required-when) field is present."),
        new("bare-key", Sev.Error, "An absent value is a bare key, not null / ~ / \"\" / —."),
        new("date-quoted", Sev.Error, "A date field is a quoted string."),
        new("date-format", Sev.Error, "A date field is a YYYY-MM-DD day on the calendar."),
        new("enum", Sev.Error, "An enum value is a scalar in the declared range."),
        new("enum-lowercase", Sev.Error, "An enum value is lowercase."),
        new("list", Sev.Error, "A list field is a YAML sequence."),
        new("min-items", Sev.Error, "A list field carries at least the entries the schema asks for."),
        new("field-pattern", Sev.Error, "A field value matches the pattern the schema declares for it."),
        new("tier-matches-type", Sev.Error, "tier equals the tier the type declares."),
        new("id-prefix", Sev.Error, "id carries the type's prefix."),
        new("id-format", Sev.Error, "id has the shape the type's id style declares."),
        new("id-matches-filename", Sev.Error, "id's discriminator matches the filename's."),
        new("filename-pattern", Sev.Error, "The filename matches the type's filename pattern."),
        new("slug-length", Sev.Error, "The slug is within the type's slug-max."),
        new("h1", Sev.Error, "The document has an H1."),
        new("identity", Sev.Error, "An identity line of two code spans follows the H1."),
        new("identity-type", Sev.Error, "The identity line names the document's own type."),
        new("identity-id", Sev.Error, "The identity line's id matches the frontmatter id."),
        new("identity-status", Sev.Error, "The identity line's status is the frontmatter status, upper-case."),
        new("required-section", Sev.Error, "Every required section heading is present."),
        new("empty-section", Sev.Error, "A section the type declares has something written under it."),
        new("placeholder-left", Sev.Error, "A record carries no `{{…}}` left unfilled from its template."),
        new("clause-table", Sev.Error, "The clause section holds a table of Id and Clause rows."),
        new("clause-id-format", Sev.Error, "A clause id is a code span matching the type's pattern."),
        new("clause-id-unique", Sev.Error, "A clause id is unique within its document."),
        new("clause-modal", Sev.Error, "A clause opens with a modal, bold where it binds."),
        new("link-resolves", Sev.Error, "Every internal link resolves."),
        new("fragment-resolves", Sev.Error, "A link's `#fragment` names a heading in the document it points at."),
        new("undefined-label", Sev.Error, "A shortcut reference has a link definition."),
        new("label-canonical", Sev.Error, "An id-shaped shortcut label is written as the canonical id."),
        new("related-matches-section", Sev.Error, "A mirrors-section field reconciles with its section."),
        new("id-unique", Sev.Error, "id is unique across the whole wiki."),
        new("type-setup", Sev.Error, "A type the schema declares is stood up as both a page and a folder, or neither."),
        new("framework-names-types", Sev.Error,
            "The framework's own documentation names a type rather than linking to one."),
        new("template-fields", Sev.Error, "A type's template carries the fields a copy of it needs, usably."),
        new("generated-block", Sev.Error, "A page carrying a generated block still has both of its markers."),
        new("page-frontmatter", Sev.Error, "A type's page carries no frontmatter — it describes records, and is not one."),
        new("clause-ref", Sev.Error, "A pol-XXXX.CLAUSE citation names a clause that exists."),
        new("ref-resolves", Sev.Error, "An id in a field with a ref names a document that exists."),
        new("reciprocal", Sev.Error, "A reciprocal field agrees in both directions."),
        new("list-order", Sev.Warning, "A list field's entries are in alphabetical order."),
        new("min-records", Sev.Warning, "A value in a grouping field is carried by as many records as its floor."),
        new("clause-order", Sev.Warning, "Clause rows are grouped by binding level."),
        new("clause-compound", Sev.Warning, "A clause carries one obligation, not two."),
        new("unused-definition", Sev.Warning, "A link definition that nothing references."),
        new("bracket-literal", Sev.Warning, "A [...] in prose that looks like a broken reference."),

        // The rules that need C# declare what they report, so implementing one and registering it is
        // the same edit. Restating them here is how an id and its catalogue entry drift apart.
        .. DocumentRules.All.SelectMany(r => r.Emits)
    ];

    // The catalogue as it stands for a given corpus: the checks above, which every corpus gets, plus one
    // entry per expression rule its schema declares. A rule with an `expr:` reports under its own id, so
    // it is a check like any other — it appears in `kac checks`, and the coverage gate holds it to the
    // same requirement of a fixture that exercises it.
    //
    // Ordered so the core checks keep the sequence a document is read in and the schema's own rules
    // follow, grouped by the type that declares them.
    public static IReadOnlyList<CheckDef> For(Schema schema) =>
    [
        .. All,
        .. schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal)
            .SelectMany(kv => kv.Value.Rules)
            .Where(r => r.Compiled is not null)
            .Select(r => new CheckDef(r.Id, r.Severity ?? Sev.Warning, r.Description ?? r.Message ?? r.Id))
    ];
}
