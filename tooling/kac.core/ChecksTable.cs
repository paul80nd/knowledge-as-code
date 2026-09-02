namespace kac.core;

// The reader-facing "What CI checks" table on a type page, and the gate holding it to the catalogue.
//
// Apart from the renderers in `Generator` because nothing calls the two together: `generate` splices this
// table into a page, and `kac checks` runs the gate over the schema alone.
public static class ChecksTable
{
    // What a description may run to. A reader scans the checks table to find the row they tripped, not to
    // read a paragraph about every check on the page. A cell running to several sentences has taken on the
    // message's job as well as its own. A description says what is verified. A rule's `message:` says what
    // to do about it, and is where the author who tripped it reads the reasoning. Held here because the
    // table is what makes this a limit, and read by SchemaChecks so a schema's own rules are held to the
    // same bound as the rows written below.
    public const int DescriptionMax = 120;

    // The rows, curated rather than taken from the catalogue as it stands: related checks fold into one
    // row (the three `id-*` checks read as one `id` row) and each is worded for a human skim. Generating
    // them from the catalogue would change what the table means, so a row names the catalogue ids it
    // stands for, `Problems` verifies the coverage, and the wording stays hand-tuned.
    //
    // `When` is the row's applicability: null fires for every type, otherwise the predicate asks the
    // type's own schema whether the check can fire at all, so a policy page does not advertise that its
    // documents are checked for Y-statements. Read from the schema rather than hand-listed per type, so
    // declaring a rule remains the only thing needed to document it.
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
        ("bare-key", [new("bare-key")], "An absent value is a bare key, never `null`, `~`, `\"\"` or `—`.", null),
        ("date-quoted / date-format", [new("date-quoted"), new("date-format")],
            "Date fields are quoted, and name a day the calendar has: `YYYY-MM-DD`.", null),
        ("enum", [new("enum"), new("enum-lowercase")], "Enum values are in range and lowercase.", null),
        ("field-pattern", [new("field-pattern")],
            "Values match the pattern their field declares (e.g. `tags`).", null),
        ("min-items", [new("min-items")],
            "A list field carries at least as many entries as its schema asks for.",
            t => t.AnyField(f => f.MinItems is not null)),
        ("list-order", [new("list-order")],
            "List entries read in alphabetical order, with numbers compared as numbers.", null),
        ("entry-shape / entry-key", [new("entry-shape"), new("entry-key")],
            "Each entry of an object list is a mapping, carrying the keys the field declares and no others.",
            t => t.AnyField(f => f.Of == "object")),
        ("min-records", [new("min-records")],
            "A value in a grouping field is carried by at least as many records as the schema asks for.",
            t => t.AnyField(f => f.MinRecords is not null)),
        ("tier-matches-type", [new("tier-matches-type")], "`tier` matches the tier the type declares.", null),
        // Which of the three shapes an id takes is the type's to decide, so the row says a shape is held
        // to rather than naming the three a reader might be on.
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
            "Clause rows are grouped by binding level, and each carries a single obligation.",
            t => t.Parts?.Source == PartSpec.Table),
        // What the `clauses` rows ask of a table, asked of the source that writes its parts as headings.
        ("part-none / part-empty", [new("part-none"), new("part-empty")],
            "The parts section holds at least one heading, and each has something under it.",
            t => t.Parts?.Source == PartSpec.Headings),
        // Shown on the pages of the types that keep addressable parts, rather than on every page. Both
        // checks run corpus-wide, since a citation is checked where it is written and any document may
        // carry one. This predicate scopes the documentation, and a type whose records have no parts has
        // no reason to describe how one is cited.
        ("part-id-unique / part-ref", [new("part-id-unique"), new("part-ref")],
            "No two parts of a record share an address, and a `record-id.part` citation reaches the part "
            + "it names.", t => t.Parts is not null),
        ("link-resolves", [new("link-resolves"), new("fragment-resolves")],
            "Every internal link resolves (all forms, `.md` optional), and a `#fragment` names a heading there.",
            null),
        ("undefined-label", [new("undefined-label")], "Every shortcut reference has a link definition.", null),
        ("label-canonical", [new("label-canonical")],
            "A shortcut label that names a document is written as that document's id.", null),
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
        ("alignment-rollup / framework-posture", [new("alignment-rollup"), new("framework-posture")],
            "`aligns-with` carries every binding reference the `Alignment` column cites, and the register "
            + "places each framework.",
            t => t.Rules.Any(r => r.Id == new RuleId("alignment-rollup"))),
        ("terms-alphabetical", [new("terms-alphabetical")], "A glossary's entries read in alphabetical order.", null),
        ("dependency-cycle", [new("dependency-cycle")],
            "A cycle in the dependency graph these records form, naming every record the loop runs through.", null)
    ];

    // Which rule class reports under which check id, read from the registries rather than written out.
    // A row whose checks come from a rule class belongs on a type page only where that type declares the
    // rule. Naming the rule id here instead would let a rename stop a page advertising the check,
    // silently and with nothing looking wrong.
    private static readonly IReadOnlyDictionary<CheckId, RuleId> RuleByCheck =
        DocumentRules.All.SelectMany(r => r.Emits.Select(c => (Check: c, r.RuleId)))
            .Concat(CorpusRules.All.SelectMany(r => r.Emits.Select(c => (Check: c, r.RuleId))))
            .ToDictionary(x => x.Check, x => x.RuleId);

    // A row applies where its own predicate allows it and where the type declares whatever rule class
    // reports its checks. A row naming no rule-class check always passes the second half.
    private static bool Applies((string Label, CheckId[] Ids, string Description, Func<TypeSchema, bool>? When) row,
        TypeSchema t) =>
        (row.When is null || row.When(t))
        && row.Ids.All(id => !RuleByCheck.TryGetValue(id, out var rule) || t.HasRule(rule));

    // The curated rows, then a row for each expression rule the type declares. A core check is worded
    // here because several ids fold into one reader-facing row. An expression rule is one id reporting
    // under its own name, and its `description:` in the schema is already that row written out. Copying
    // it here would be the same sentence in two files, drifting apart at the first edit.
    //
    // Beneath both, the rules the type declares and nothing runs. See Intentions.
    public static string Render(Schema schema, TypeSchema t)
    {
        var severity = schema.Checks.ToDictionary(c => c.Id, c => c.Severity);
        List<string> headers = ["Check", "Level", "What it verifies"];
        var rows = DocRows.Where(r => Applies(r, t)).Select(r => new List<string>
        {
            $"`{r.Label}`",
            severity.GetValueOrDefault(r.Ids[0], Sev.Error).ToString().ToLowerInvariant(),
            r.Description
        }).ToList();

        rows.AddRange(t.Rules.Where(r => r.Compiled is not null).Select(r => new List<string>
        {
            $"`{r.Id}`",
            (r.Severity ?? Sev.Warning).ToString().ToLowerInvariant(),
            Gfm.Escape(r.Description ?? r.Message ?? "")
        }));

        return Gfm.RenderTable(headers, rows) + Intentions(t);
    }

    // The rules the type declares that nothing runs: no `expr:`, no implementation. Each is a real
    // decision about what this type should be held to, and each is unenforced, so a page that showed
    // only the table above would say the schema promises nothing more than CI delivers. It promises
    // rather a lot more, and that is worth a reader knowing before they rely on it.
    //
    // Kept apart from the table rather than folded in as rows, because the two answer different
    // questions: one is what a build will say about a document, the other is what has been written down
    // and not built. A rule leaves this block by gaining an `expr:` or a class, at which point it
    // appears above under its own name.
    private static string Intentions(TypeSchema t)
    {
        var intended = t.Rules
            .Where(r => r.Compiled is null
                        && !DocumentRules.ByRuleId.ContainsKey(r.Id)
                        && !CorpusRules.ByRuleId.ContainsKey(r.Id))
            .ToList();
        if (intended.Count == 0) return "";

        List<string> headers = ["Rule", "What it would verify"];
        var rows = intended
            .Select(r => new List<string> { $"`{r.Id}`", Gfm.Escape(r.Description ?? r.Message ?? "") })
            .ToList();
        return "\n\n**Declared, not yet enforced**: carried by the schema, run by nothing.\n\n"
               + Gfm.RenderTable(headers, rows);
    }

    // Reconcile the curated table with the catalogue. Empty means the reader-facing table is a faithful,
    // complete view of what the validator enforces, and any entry is a drift a human must resolve.
    // `kac checks` calls this and fails on a non-empty result, which the test suite asserts.
    public static IReadOnlyList<string> Problems(Schema schema)
    {
        var catalogue = schema.Checks.Select(c => c.Id).ToHashSet();
        var advertised = schema.Checks.Where(c => c.OnTypePage).Select(c => c.Id).ToHashSet();
        var documented = DocRows.SelectMany(r => r.Ids).ToHashSet();

        var problems = new List<string>();

        foreach (var id in documented.Where(id => !catalogue.Contains(id)).Order())
            problems.Add($"the checks table documents '{id}', which the schema does not declare (stale row).");
        foreach (var id in advertised.Where(id => !documented.Contains(id)).Order())
            problems.Add($"check '{id}' has no row in the checks table. declare 'on-type-page: false' where "
                         + "the schema declares the check, or give it a row in the table kac ships.");
        foreach (var id in documented.Where(id => catalogue.Contains(id) && !advertised.Contains(id)).Order())
            problems.Add($"check '{id}' declares 'on-type-page: false' and has a row anyway. drop the flag, "
                         + "or drop the row from the table kac ships.");

        // The rows written above, held to the bound a schema's rules are held to. Nothing else would
        // notice: these are C# literals rendered into a generated table, so a row that grows past it
        // reads as intentional in the diff and as noise on the page.
        foreach (var row in DocRows.Where(r => r.Description.Length > DescriptionMax))
            problems.Add($"the checks table row '{row.Label}' has a {row.Description.Length}-character "
                         + $"description. the limit is {DescriptionMax}.");

        return problems;
    }
}
