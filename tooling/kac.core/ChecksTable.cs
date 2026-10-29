namespace kac.core;

/// <summary>
/// The "What CI checks" table on a type page, the universal checks on <c>metadata.md</c>, and the gate that checks both
/// against the catalogue.
/// </summary>
public static class ChecksTable
{
    /// <summary>
    /// A description says what is verified, and a rule's <c>message:</c> says what to do about it. <c>SchemaChecks</c>
    /// applies the same bound to a schema's own rules.
    /// </summary>
    public const int DescriptionMax = 120;

    /// <summary>
    /// Hand-curated, not generated from the catalogue: related checks fold into one row, each worded for a skim.
    /// <c>Problems</c> verifies the coverage. <c>When</c> is null for a row that asks nothing of the type's schema,
    /// and otherwise asks the schema whether the check can fire.
    /// </summary>
    private static readonly (string Label, CheckId[] Ids, string Description, Func<TypeSchema, bool>? When)[] DocRows =
    [
        ("frontmatter-parses", [new("frontmatter-parses")], "Frontmatter is present and is a valid YAML mapping.",
            null),
        ("unknown-key", [new("unknown-key")], "Every frontmatter key is a schema field or a reserved ADO key.", null),
        ("derived-key", [new("derived-key")],
            "A field derived from the record's folder is not written in frontmatter.",
            t => t.AnyField(f => f.From is not null)),
        ("key-order", [new("key-order")], "Key order is a topological extension of the schema's field order.", null),
        ("required-field", [new("required-field")], "Required and conditionally-required fields are present.", null),
        ("bare-key", [new("bare-key")],
            "An absent value is a bare key, never `null`, `~`, `\"\"`, `''`, `—` or an unquoted `{{…}}`.", null),
        ("empty-optional-key", [new("empty-optional-key")],
            "An optional field is filled in or left out, rather than written with no value.", null),
        ("date-quoted / date-format", [new("date-quoted"), new("date-format")],
            "Date fields are quoted, and name a day the calendar has: `YYYY-MM-DD`.", null),
        ("timestamp-format", [new("timestamp-format")],
            "Timestamp fields name a moment the calendar has, in UTC: `YYYY-MM-DDThh:mm:ssZ`.",
            t => t.AnyField(f => f.Type == "timestamp") || t.AnyEntryKey(k => k.Type == "timestamp")),
        ("enum", [new("enum"), new("enum-lowercase")], "Enum values are in range and lowercase.", null),
        ("field-pattern", [new("field-pattern")],
            "Values match the pattern their field declares (e.g. `tags`).", null),
        ("min-items", [new("min-items")],
            "A list field carries at least as many entries as its schema asks for.",
            t => t.AnyField(f => f.MinItems is not null)),
        ("list-order", [new("list-order")],
            "List entries read in alphabetical order, numbers compared as numbers, unless the field keeps them as "
            + "written.", null),
        ("entry-shape / entry-key", [new("entry-shape"), new("entry-key")],
            "An object field, and each entry of an object list, carries the keys the field declares and no others.",
            t => t.AnyField(f => f.Of == "object" || f.Type == "object")),
        ("acceptance-check", [Acceptances.Refused],
            "An `accepts` entry names a check that reports warnings, and is not `acceptance-stale`.", null),
        ("acceptance-stale", [Acceptances.Stale],
            "Each `accepts` entry matches a warning its check reports on the same record.", null),
        ("min-records", [new("min-records")],
            "A value in a grouping field is carried by at least as many records as the schema asks for.",
            t => t.AnyField(f => f.MinRecords is not null)),
        ("type-matches-folder", [new("type-matches-folder")],
            "`type` matches the singular type name the record's folder declares.", null),
        ("tier-matches-type", [new("tier-matches-type")], "`tier` matches the tier the type declares.", null),
        ("id", [new("id-prefix"), new("id-format"), new("id-matches-filename")],
            "`id` carries the type's prefix, takes the shape the type declares, and names the same document "
            + "as the filename.", null),
        ("id-unique", [new("id-unique")], "`id` is unique across the whole corpus.", null),
        ("filename / slug-length", [new("filename-pattern"), new("slug-length")],
            "Filename matches the pattern. The slug is within 30 characters.", null),
        ("h1", [new("h1")], "The document has an H1.", null),
        ("identity", [new("identity"), new("identity-type"), new("identity-id"), new("identity-status")],
            "An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter.",
            null),
        ("sections", [new("required-section"), new("empty-section")],
            "Every required section heading is present, and no declared section is left as a bare heading.", null),
        ("placeholder-left", [new("placeholder-left")],
            "No `{{…}}` from the template is left unfilled, outside code.", null),
        // The pipe is escaped because this text lands in a table cell: GFM splits a cell on a bare `|`
        // even inside a code span, so an unescaped one would break the row it is describing.
        ("clauses", [new("clause-table"), new("clause-id-format"), new("clause-modal")],
            "The clause section is a table of `Id \\| Clause` rows, each id a code span and each "
            + "clause opening with its modal.", t => t.Parts?.Source == PartSpec.Table),
        ("clause-order / clause-compound", [new("clause-order"), new("clause-compound")],
            "Clause rows are grouped by binding level, and no row states a second modal.",
            t => t.Parts?.Source == PartSpec.Table),
        ("part-none / part-empty", [new("part-none"), new("part-empty")],
            "The parts section holds at least one heading, and each has something under it.",
            t => t.Parts?.Source == PartSpec.Headings),
        ("part-id-format", [new("part-id-format")],
            "Each part heading opens with its id, as `### PERMS: A job declares the permission it needs`.",
            t => t.Parts is { Source: PartSpec.Headings, IdPattern.Length: > 0 }),
        // Only where the type declares modals: a glossary has heading parts too, and no obligations.
        ("part-modal / part-nested", [new("part-modal"), new("part-nested")],
            "A part heading states its obligations as bullets, each with a declared modal in bold and no "
            + "list inside it.",
            t => t.Parts is { Source: PartSpec.Headings, Binding.Count: > 0 }),
        // Shown wherever the rule is declared, not wherever modals are: a type may declare modals and still accept
        // a bullet outside the parts.
        ("modal-outside-rules", [new("modal-outside-rules")],
            "Only the parts section binds: no bullet outside it names a declared modal.",
            t => t.Rules.Any(r => r.Id == new RuleId("binds-only-under-rules"))),
        // This scopes the documentation only. Both checks run corpus-wide, because any document may carry a citation.
        ("part-id-unique / part-ref", [new("part-id-unique"), new("part-ref")],
            "No two parts of a record share an address, and a `record-id.part` citation reaches the part "
            + "it names.", t => t.Parts is not null),
        // Shown wherever the rule is declared, which `Applies` decides from the registry.
        ("clause-quoted-faithfully", [new("clause-quoted-faithfully")],
            "A span quoted beside a clause citation is still in the clause it cites.", null),
        ("pair-one-way", [new("pair-one-way")],
            "A clause signposted by a clause of the same id in another policy signposts that clause back.", null),
        ("shared-id-unsignposted", [new("shared-id-unsignposted")],
            "A clause id used in two policies is signposted from at least one of them.", null),
        ("link-resolves", [new("link-resolves"), new("fragment-resolves")],
            "Every internal link resolves (all forms, `.md` optional), and a `#fragment` names a heading there.",
            null),
        ("undefined-label", [new("undefined-label")], "Every shortcut reference has a link definition.", null),
        ("label-canonical", [new("label-canonical")],
            "A link label is the id of the record it leads to, written as that record carries it.", null),
        ("related-matches-section", [new("related-matches-section")],
            "A field that mirrors a section reconciles with the ids in that section.",
            t => t.AnyField(f => f.MirrorsSection is not null)),
        ("mirrors-citations", [new("mirrors-citations")],
            "A field that mirrors a label reconciles with the citations the labelled lines gather.",
            t => t.AnyField(f => f.MirrorsCitations is not null)),
        ("ref-resolves", [new("ref-resolves")],
            "An id in a field that references another document names one that exists, of the type the field names.",
            t => t.AnyField(f => f.Refs.Count > 0)),
        ("reciprocal", [new("reciprocal")], "A reciprocal field and its counterpart agree in both directions.",
            t => t.AnyField(f => f.Reciprocal is not null)),
        ("unused-definition", [new("unused-definition")], "A link definition that nothing references.", null),
        ("y-statement", [new("y-statement")],
            "A Y-statement block-quote follows the H1, states all six moves, and is within its word ceiling.",
            null),
        ("alternatives-verdict", [new("alternatives-verdict")], "Each Alternatives Considered bullet states a verdict.",
            null),
        ("decision-cites-descriptive", [new("decision-cites-descriptive")],
            "No link in Decision or Consequences lands on a record of a descriptive type.",
            t => t.Rules.Any(r => r.Id == new RuleId("decisions-cite-no-descriptive-record"))),
        ("alignment-rollup / framework-posture", [new("alignment-rollup"), new("framework-posture")],
            "`aligns-with` carries every binding reference the `Alignment` column cites, and the register "
            + "places each framework.",
            t => t.Rules.Any(r => r.Id == new RuleId("alignment-rollup"))),
        ("alignment-unlinked", [new("alignment-unlinked")],
            "Every framework an `Alignment` cell cites is written as a link the corpus resolves.",
            t => t.Rules.Any(r => r.Id == new RuleId("alignment-rollup"))),
        ("framework-uncited", [new("framework-uncited")],
            "Every framework on the register is cited by at least one clause.",
            t => t.Rules.Any(r => r.Id == new RuleId("alignment-rollup"))),
        ("report-stale", [new("report-stale")],
            "Each corpus a report answers for is at the version the report names.",
            t => t.Rules.Any(r => r.Id == new RuleId("report-stale"))),
        ("self-verification", [new("self-verification")],
            "A report's `verified` list does not name the producer that `generated.by` names.",
            t => t.Rules.Any(r => r.Id == new RuleId("no-self-verification"))),
        ("duration-matches-the-moments", [new("duration-matches-the-moments")],
            "`duration` is the span from `occurred-at` to `restored-at`.",
            t => t.Rules.Any(r => r.Id == new RuleId("duration-matches-the-moments"))),
        ("terms-alphabetical", [new("terms-alphabetical")], "A glossary's entries read in alphabetical order.", null),
        ("definition-circular", [new("definition-circular")],
            "A glossary entry's definition does not repeat the term it defines.",
            t => t.Rules.Any(r => r.Id == new RuleId("definitions-do-not-repeat-the-term"))),
        ("redefinition-unlinked", [new("redefinition-unlinked")],
            "Where two glossaries define one term, at least one of the entries links to the other's anchor.",
            t => t.Rules.Any(r => r.Id == new RuleId("redefinitions-are-linked"))),
        ("entry-requirement", [new("entry-requirement")],
            "No bold BCP 14 keyword states a requirement inside a glossary entry.",
            t => t.Rules.Any(r => r.Id == new RuleId("entries-state-no-requirement"))),
        ("immutable-after-accepted", [new("immutable-after-accepted")],
            "With `--since`, an accepted or rejected ADR has changed by no more than a typo, a link, its status or "
            + "`accepts`.",
            null),
        ("immutable-after-published", [new("immutable-after-published")],
            "With `--since`, a published postmortem has changed by no more than a typo, a link, its status or "
            + "`accepts`.",
            null),
        ("clause-id-kept", [new("clause-id-kept")],
            "With `--since`, a policy active at that revision keeps the id of every clause it had there.", null),
        ("rule-id-kept", [new("rule-id-kept")],
            "With `--since`, a standard active at that revision keeps the id of every rule it had there.", null),
        ("retire-not-delete", [HistoryRules.RetireNotDelete],
            "With `--since`, no record in force at that revision has been deleted.", null),
        ("changelog-order", [new("changelog-order")], "A standard's changelog entries read newest first.",
            t => t.Rules.Any(r => r.Id == new RuleId("changelog-newest-first"))),
        ("diagnosis-dead-end / diagnosis-no-escalation",
            [new("diagnosis-dead-end"), new("diagnosis-no-escalation")],
            "Every diagnosis branch routes to a section or falls through, and one branch reaches `Escalation`.",
            t => t.Rules.Any(r => r.Id == new RuleId("escalation-required"))),
        ("alert-after-needs-an-alert", [new("alert-after-needs-an-alert")],
            "A target stating `alert-after` binds a service that raises an alert or a ticket, "
            + "through any offering between.",
            null),
        ("departure-needs-none", [new("departure-needs-none")],
            "Each clause or rule in `departs-from` states a MUST or a MUST NOT, and so needs a deviation.", null),
        ("dependency-cycle", [new("dependency-cycle")],
            "A cycle in the dependency graph these records form, naming every record the loop runs through.", null),
        ("dependency-criticality", [new("dependency-criticality")],
            "No record is graded above the criticality of anything it depends on.", null),
        ("feature-file-repo", [new("feature-file-repo")],
            "A feature file path begins with a repository one of the implementing services names.", null),
        ("mirrors-repo-links", [new("mirrors-repo-links")],
            "Every `repos` entry is linked under `Where it lives`, and every repository linked there is stated.",
            null)
    ];

    /// <summary>Read from the registries, so renaming a rule cannot stop a page advertising its check.</summary>
    private static readonly IReadOnlyDictionary<CheckId, RuleId> RuleByCheck =
        DocumentRules.All.SelectMany(r => r.Emits.Select(c => (Check: c, r.RuleId)))
            .Concat(CorpusRules.All.SelectMany(r => r.Emits.Select(c => (Check: c, r.RuleId))))
            .Concat(HistoryRules.All.SelectMany(r => r.Emits.Select(c => (Check: c, r.RuleId))))
            .ToDictionary(x => x.Check, x => x.RuleId);

    private static readonly List<string> Headers = ["Check", "Level", "What it verifies"];

    private static bool Applies((string Label, CheckId[] Ids, string Description, Func<TypeSchema, bool>? When) row,
        TypeSchema t) =>
        (row.When is null || row.When(t))
        && row.Ids.All(id => !RuleByCheck.TryGetValue(id, out var rule) || t.HasRule(rule));

    /// <summary>
    /// A row every type shows: it asks nothing of the type's schema, and none of its checks belongs to a rule a type
    /// has to declare.
    /// </summary>
    private static bool Universal(
        (string Label, CheckId[] Ids, string Description, Func<TypeSchema, bool>? When) row) =>
        row.When is null && row.Ids.All(id => !RuleByCheck.ContainsKey(id));

    /// <summary>The rows every type shows, printed once on <c>knowledge-as-code/metadata.md</c>.</summary>
    public static string RenderUniversal(Schema schema) =>
        Gfm.RenderTable(Headers, [.. DocRows.Where(Universal).Select(r => CuratedRow(schema, r))]);

    /// <summary>
    /// A link to the universal rows, then the curated rows this type can trip, then one per expression rule the type
    /// declares, worded by its schema <c>description:</c>. <paramref name="up"/> climbs from the type page to the
    /// corpus root.
    /// </summary>
    public static string Render(Schema schema, TypeSchema t, string up)
    {
        var rows = DocRows.Where(r => !Universal(r) && Applies(r, t)).Select(r => CuratedRow(schema, r)).ToList();

        rows.AddRange(t.Rules.Where(r => r.Compiled is not null).Select(r => new List<string>
        {
            $"`{r.Id}`",
            (r.Severity ?? Sev.Warning).ToString().ToLowerInvariant(),
            Gfm.Escape(r.Description ?? r.Message ?? "")
        }));

        var universal = $"[universal checks]({up}knowledge-as-code/metadata.md#universal-checks)";
        return rows.Count == 0
            ? Generator.Wrap($"CI runs the {universal} on this type's records, and nothing more.") + Intentions(t)
            : Generator.Wrap($"CI runs the {universal} on every record, and the checks below on this type's records.")
              + "\n\n" + Gfm.RenderTable(Headers, rows) + Intentions(t);
    }

    private static List<string> CuratedRow(Schema schema,
        (string Label, CheckId[] Ids, string Description, Func<TypeSchema, bool>? When) row) =>
    [
        $"`{row.Label}`",
        schema.Checks.Where(c => c.Id == row.Ids[0]).Select(c => c.Severity).DefaultIfEmpty(Sev.Error).First()
            .ToString().ToLowerInvariant(),
        row.Description
    ];

    /// <summary>
    /// The rules the type declares that nothing runs: no <c>expr:</c>, no class. Kept out of the table, which says only
    /// what a build reports.
    /// </summary>
    private static string Intentions(TypeSchema t)
    {
        var intended = t.Rules
            .Where(r => r.Compiled is null
                        && !DocumentRules.ByRuleId.ContainsKey(r.Id)
                        && !CorpusRules.ByRuleId.ContainsKey(r.Id)
                        && !HistoryRules.ByRuleId.ContainsKey(r.Id))
            .ToList();
        if (intended.Count == 0) return "";

        List<string> headers = ["Rule", "What it would verify"];
        var rows = intended
            .Select(r => new List<string> { $"`{r.Id}`", Gfm.Escape(r.Description ?? r.Message ?? "") })
            .ToList();
        return "\n\n**Declared, not yet enforced**: carried by the schema, run by nothing.\n\n"
               + Gfm.RenderTable(headers, rows);
    }

    /// <summary>Empty where the table covers the catalogue exactly. <c>kac checks</c> fails on anything else.</summary>
    public static IReadOnlyList<string> Problems(Schema schema)
    {
        var catalogue = schema.Checks.Select(c => c.Id).ToHashSet();
        var advertised = schema.Checks.Where(c => c.OnTypePage).Select(c => c.Id).ToHashSet();
        var documented = DocRows.SelectMany(r => r.Ids).ToHashSet();

        var problems = new List<string>();

        foreach (var id in documented.Where(id => !catalogue.Contains(id)).Order())
            problems.Add($"the checks table documents '{id}', which the schema does not declare (stale row).");
        foreach (var id in advertised.Where(id => !documented.Contains(id)).Order())
            problems.Add($"check '{id}' has no row in the checks table. Declare 'on-type-page: false' where "
                         + "the schema declares the check, or give it a row in the table kac ships.");
        foreach (var id in documented.Where(id => catalogue.Contains(id) && !advertised.Contains(id)).Order())
            problems.Add($"check '{id}' declares 'on-type-page: false' and has a row anyway. Drop the flag, "
                         + "or drop the row from the table kac ships.");

        // Nothing else checks these C# rows against the bound a schema's rules meet.
        foreach (var row in DocRows.Where(r => r.Description.Length > DescriptionMax))
            problems.Add($"the checks table row '{row.Label}' has a {row.Description.Length}-character "
                         + $"description. The limit is {DescriptionMax}.");

        return problems;
    }
}
