namespace kac.core;

// What the schema says the tool will do, held against what the tool can actually do.
//
// Every other check reads a document. This one reads the files that decide how documents are read, and
// it exists because those files are copied into corpora whose authors cannot ask what a key meant. A
// declaration nothing dispatches is not inert to a reader: `rules:` is documented as behaviour the
// validator applies, so a rule id no code answers to reads as a commitment, and a `ref:` at a type the
// corpus never adopted reads as a link that is being checked.
//
// The question asked of each value is not "is this key spelled right" but "is there code that acts on
// this value". `style: mnemonic` is a real style and would pass a spelling test; what makes it sound is
// the branch in IdChecks. So each vocabulary here is read from the code that dispatches it rather than
// restated.
//
// Findings land against `.schema/<file>.yaml`, because that is the file a corpus owner edits and the
// one the mechanism sync will carry. A rule or field the loader could not read at all carries its own
// account of why, recorded where it was parsed, and is reported here alongside the rest so that a
// schema defect is a finding like any other rather than a stack trace on the way to one.
public static class SchemaChecks
{
    public static void Check(Schema schema, List<Finding> f)
    {
        // Walked in declared order, here and below, so that a schema with several faults reports them
        // in the order someone reading the file would meet them.
        UnreadKeys(".schema/_enums.yaml", schema, f);
        UnreadKeys(".schema/_tiers.yaml", schema, f);
        UnreadKeys(".schema/_checks.yaml", schema, f);
        UnreadKeys(".schema/_universal.yaml", schema, f);
        CheckTiers(schema, f);
        CheckDeclaredChecks(schema, f);
        foreach (var name in schema.UniversalOrder)
            if (schema.Universal.TryGetValue(name, out var spec))
                CheckField(".schema/_universal.yaml", name, spec, schema, null, f);

        foreach (var (key, t) in schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            var at = $".schema/{key}.yaml";

            UnreadKeys(at, schema, f);
            CheckFolder(at, key, t, f);

            if (!IdChecks.IdStyles.Contains(t.IdStyle))
                Dispatch(at, $"type '{key}' declares 'id.style: {t.IdStyle}', which no id check reads. "
                             + $"The styles the tool applies are {List(IdChecks.IdStyles)}.", f);

            if (t.IndexOrder.Length > 0 && !Generator.IndexOrders.Contains(t.IndexOrder))
                Dispatch(at, $"type '{key}' declares 'index.order: {t.IndexOrder}', which the generator does "
                             + $"not read. An index is written {List(Generator.IndexOrders)}.", f);

            if (schema.Tiers.All(tier => tier.Name != t.Tier))
                f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                    $"type '{key}' declares 'tier: {t.Tier}', and '_tiers.yaml' declares no such tier. Tier decides "
                    + "how a document behaves and is written into the frontmatter of every record of the type; the "
                    + $"tiers are {Ordered(schema.Tiers.Select(tier => tier.Name))}."));

            CheckParts(at, key, t, f);
            CheckExport(at, key, t, schema, f);
            CheckProse(at, key, t, f);

            foreach (var name in t.FieldOrder)
                CheckField(at, name, t.Fields[name], schema, t, f);

            foreach (var rule in t.Rules)
                CheckRule(at, key, rule, f);
        }

        CheckVersus(schema, f);
    }

    // The disambiguations, which are the one thing a type says about another type rather than about
    // itself. Three ways that goes wrong, and all three read as working until someone opens the page:
    // a pair against a type no schema covers renders a heading naming nothing; a pair against itself
    // renders "ADR vs ADR"; and a pair both sides declare renders twice, with two accounts of the same
    // distinction that nothing keeps in step.
    //
    // Which side declares a pair is a convention rather than a rule the tool could derive — it is the
    // type the heading is titled from — so the tool holds the two sides against each other and leaves the
    // choice to whoever writes it.
    private static void CheckVersus(Schema schema, List<Finding> f)
    {
        var declared = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var (key, t) in schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        foreach (var (other, _) in t.Versus)
        {
            var at = $".schema/{key}.yaml";

            if (other == key)
            {
                f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                    $"type '{key}' declares a 'versus:' against itself — a disambiguation has two sides."));
                continue;
            }

            if (!schema.ByFolder.ContainsKey(other))
            {
                Dispatch(at, $"type '{key}' declares 'versus: {other}', and no schema covers that folder — either "
                             + "the type was never adopted here, or the name is wrong.", f);
                continue;
            }

            var pair = string.CompareOrdinal(key, other) < 0 ? $"{key}|{other}" : $"{other}|{key}";
            if (declared.TryGetValue(pair, out var first))
                f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                    $"type '{key}' declares a 'versus: {other}' that '{first}.yaml' already declares — a pair is "
                    + "written once, by the type its heading is titled from."));
            else
                declared[pair] = key;
        }
    }

    // The two files that between them define a tier, held against each other. `_universal.yaml` gives the
    // `tier` field its range, and every record is validated against it. `_tiers.yaml` says what each of
    // those values is called and means, which a generated page renders. Neither is derivable from the
    // other, and a tier declared in one and not the other is silent in both directions — a record
    // admitted with a tier no page can name, or a heading no document will ever sit under.
    //
    // Reported against `_tiers.yaml` whichever side is short, because that is the file whose entries are
    // cheap to add: widening the field's range is a change to what every corpus may carry.
    private static void CheckTiers(Schema schema, List<Finding> f)
    {
        if (!schema.Universal.TryGetValue("tier", out var field)) return;

        const string at = ".schema/_tiers.yaml";
        var declared = schema.Tiers.Select(t => t.Name).ToList();
        var admitted = field.Values ?? [];

        foreach (var value in admitted.Where(v => !declared.Contains(v)))
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"the 'tier' field admits '{value}', and no tier here declares it — a record may carry a tier that "
                + "nothing can name on a page."));

        foreach (var tier in declared.Where(t => !admitted.Contains(t)))
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"tier '{tier}' is declared here and the 'tier' field in '_universal.yaml' does not admit it — no "
                + "document can ever carry it."));

        foreach (var tier in schema.Tiers.Where(t => t.Label.Length == 0 || t.Behaviour.Length == 0))
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"tier '{tier.Name}' declares no {(tier.Label.Length == 0 ? "'label:'" : "'behaviour:'")} — both head "
                + "the tier's section in the generated taxonomy."));
    }

    // A key the loader never asked for. Every other check here reads a declaration and asks whether code
    // acts on its value; this one asks whether the declaration is read at all, which is the question a
    // key nothing dispatches answers with silence. The vocabulary is not listed anywhere — it is the set
    // of keys the loader requested, recorded as it read the file — so a key gains its meaning and its
    // admission in the same edit.
    //
    // `notes:` is admitted at every level and is the way to say something these files should say and the
    // tool has no use for.
    private static void UnreadKeys(string at, Schema schema, List<Finding> f)
    {
        foreach (var key in schema.UnreadKeys.Where(k => k.File == at))
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-unknown-key"),
                $"{key.Where} declares '{key.Key}', which the loader does not read — implement it, drop it, "
                + "or write what it was saying as 'notes:', the one key every level admits."));
    }

    // Where a type says its parts live, held against what an extractor can actually read.
    //
    // Only a `parts:` block makes a `pol-VURM.TIMEBOX` citation resolvable, so every way of getting it
    // wrong ends the same way: the type offers no parts, every citation into it fails, and the schema
    // reads as though addressing were set up. A source nothing extracts is the first way. A section the
    // type never declares is the second, and it is the `mirrors-section:` fault in another place — the
    // walk would run to a heading no record may carry and find nothing under it.
    //
    // The modals are the table source's alone. Without them every row is reported as opening with no
    // modal, and the message lists the modals to write as an empty list.
    private static void CheckParts(string at, string key, TypeSchema t, List<Finding> f)
    {
        if (t.Parts is not { } parts) return;

        if (!PartSpec.Sources.Contains(parts.Source))
            Dispatch(at, $"type '{key}' declares 'parts.source: {parts.Source}', which nothing extracts. The "
                         + $"sources the tool reads are {List(PartSpec.Sources)}.", f);

        if (!t.RequiredSections.Concat(t.OptionalSections).Contains(parts.Section, StringComparer.OrdinalIgnoreCase))
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"type '{key}' declares 'parts.section: {parts.Section}', and its 'sections:' block declares no "
                + "such section — name a section the type has, or add it. No record can carry a part otherwise."));

        if (parts is { Source: PartSpec.Table, Binding.Count: 0 })
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"type '{key}' sources its parts from a table and declares no 'binding:' — say which modals "
                + "oblige, or every row is reported as opening with none of them."));
    }

    // What a type says it contributes to a published export, held against what the type itself declares
    // and against what an export can carry.
    //
    // The corpus that reads an export took a copy of these files and cannot ask what was meant. So every
    // way of getting this wrong ends in silence: a section, a field or a set of parts named here and not
    // there contributes nothing, while the schema goes on saying it does. Selection is by key for that
    // reason, and this is where each key is resolved.
    //
    // Fidelity is asked of every entry, because it is what a consumer reads the export for — words
    // arriving whole, cut to a line, or replaced by a breadcrumb. A section becomes an entry as soon as
    // its heading is written, so a heading with nothing beside it is asked. `parts:` is the entry itself,
    // so writing it with nothing beside it declares no parts at all.
    private static void CheckExport(string at, string key, TypeSchema t, Schema schema, List<Finding> f)
    {
        if (t.Export is not { } export) return;

        var declared = t.RequiredSections.Concat(t.OptionalSections).ToList();

        foreach (var (section, fidelity) in export.Sections)
        {
            if (!declared.Contains(section, StringComparer.OrdinalIgnoreCase))
                f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                    $"type '{key}' declares 'export.sections: {section}', and its 'sections:' block declares no "
                    + "such section — name a section the type has, or add it. Selecting by key is what keeps the "
                    + "two declarations in step."));

            Fidelity($"export.sections: {section}", fidelity);
        }

        // `title` answers as a field without being one: a record's title is its H1, which every record
        // has and an index column already names the same way.
        foreach (var name in export.Fields.Where(n =>
                     n != Generator.Title && schema.EffectiveField(t, n) is null))
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"type '{key}' declares 'export.fields: {name}', and neither the type nor '_universal.yaml' "
                + $"declares such a field — name a field a record carries, or '{Generator.Title}', which is a "
                + "record's heading rather than its frontmatter."));

        if (export.Parts.Length == 0) return;

        if (t.Parts is null)
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"type '{key}' declares 'export.parts:' and carries no 'parts:' block — say where a record "
                + "keeps the parts an export is to carry, or drop the entry."));

        Fidelity("export.parts:", export.Parts);
        return;

        void Fidelity(string entry, string value)
        {
            if (value.Length == 0)
                f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                    $"type '{key}' declares '{entry}' at no fidelity — say how much of it travels. Fidelity is "
                    + "what a consumer reads the export for, and is defaulted nowhere."));
            else if (!ExportSpec.Carried.Contains(value, StringComparer.Ordinal))
                Dispatch(at, $"type '{key}' declares '{entry}' at fidelity '{value}', which nothing carries. "
                             + $"An export carries {List(ExportSpec.Carried)}.", f);
        }
    }

    // A type is a folder of records, and `folder:` names it. The check asks after the value rather than
    // the key, because an absent `folder:` and a deliberate `folder: null` parse to the same empty
    // string. A type with neither has nowhere to put a record.
    private static void CheckFolder(string at, string key, TypeSchema t, List<Finding> f)
    {
        if (string.IsNullOrEmpty(t.Folder))
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"type '{key}' declares no 'folder:' — say which folder holds its records."));
    }

    // What a type says about itself, which every generated list of types is written from. Each is
    // required, because a type that declares none of them still appears in those lists — as a row with an
    // empty cell, which reads as an oversight in the page rather than in the schema it came from.
    //
    // `label-plural:` is here and `label:` is not, because only one of them can be derived. A singular
    // falls back to the type name capitalised; nothing turns `nfr` into "NFRs".
    //
    // The bound is the one a rule's description is held to, for the same reason: these are table cells a
    // reader scans, and the sentence that will not fit in one is the sentence that belongs on the type's
    // own page.
    private static void CheckProse(string at, string key, TypeSchema t, List<Finding> f)
    {
        Line("label-plural", t.LabelPlural, "what a folder of these is called — \"ADRs\", \"Policies\", \"NFRs\"");
        Line("summary", t.Summary, "what the type is");
        Line("goes-here", t.GoesHere, "what a contributor has in hand when this type is the answer");

        // Not held to the cell bound: `detail:` is the paragraph the other three are too short to be, and
        // it is rendered as prose rather than into a table.
        if (t.Detail.Length == 0)
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"type '{key}' declares no 'detail:' — say what the type carries beyond its first sentence, and the "
                + "edge a reader is most likely to walk over."));

        // Only the prior art is required, and "none" is one of its answers. What the framework took from an
        // ancestor and where it parts company are questions a type with no ancestor cannot answer, so an
        // empty pair is a real state rather than an unfinished one.
        if (t.Lineage is null)
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"type '{key}' declares no 'lineage.prior-art:' — name what the type is nearest to, or say that "
                + "nothing established fits. Claiming an ancestor a type does not have is worse than admitting none."));

        return;

        void Line(string name, string value, string says)
        {
            if (value.Length == 0)
                f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                    $"type '{key}' declares no '{name}:' — say {says}, in one line the taxonomy and the "
                    + "corpus index can be generated from."));
            else if (value.Length > Generator.DescriptionMax)
                f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                    $"type '{key}' has a {value.Length}-character '{name}:'; the limit is "
                    + $"{Generator.DescriptionMax}. It is rendered as a table cell — the fuller account belongs "
                    + $"on {(t.Page.Length > 0 ? t.Page : $"{key}.md")}."));
        }
    }

    // `t` is the type declaring the field, and is null for a universal one — a field declared for every
    // type belongs to none of them, so the questions that read the type's own declarations are not asked
    // of it.
    private static void CheckField(string at, string name, FieldSpec spec, Schema schema, TypeSchema? t,
        List<Finding> f)
    {
        if (spec.Problem is { } problem)
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-unreadable"), problem));

        foreach (var folder in spec.Refs.Where(folder => !schema.ByFolder.ContainsKey(folder)))
            Dispatch(at, $"field '{name}' declares 'ref: {folder}', and no schema covers that folder — "
                         + "either the type was never adopted here, or the name is wrong.", f);

        // Only an enum's range is applied. A `values:` list anywhere else states a vocabulary that
        // nothing holds a document to, which is the shape of promise this pass exists to stop.
        if (spec.Values is { Count: > 0 } && spec.Type != "enum")
            Dispatch(at, $"field '{name}' is 'type: {spec.Type}' and declares 'values:', which only an "
                         + "enum's range is read from — declare it 'type: enum', or drop the values.", f);

        // A floor is a question about a sequence's length, so it has no reading on a scalar. `allow-literal`
        // needs no such guard: it is a word admitted in place of a value, and every field type has one.
        if (spec.MinItems is not null && spec.Type != "list")
            Dispatch(at, $"field '{name}' is 'type: {spec.Type}' and declares 'min-items:', which only a "
                         + "list's length is read against — declare it 'type: list', or drop the floor.", f);

        // `min-records` is read from a list's entries and nowhere else. A scalar could in principle be
        // counted the same way and is not, which makes it exactly the kind of declaration this pass exists
        // to report: one a reader would take as enforced.
        if (spec.MinRecords is not null && spec.Type != "list")
            Dispatch(at, $"field '{name}' is 'type: {spec.Type}' and declares 'min-records:', which is read "
                         + "against the entries of a list — declare it 'type: list', or drop the floor.", f);

        // Any section reconciles, so this is not a vocabulary the tool holds — which is why nothing
        // else would catch a section the type never offers. The reconciliation would run against a
        // heading no record may carry and report every id in the field as missing from it.
        if (t is not null && spec.MirrorsSection is { } section
                          && !t.RequiredSections.Concat(t.OptionalSections)
                              .Contains(section, StringComparer.OrdinalIgnoreCase))
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"field '{name}' declares 'mirrors-section: {section}', and the type's 'sections:' block "
                + "declares no such section — name a section the type has, or add it."));
    }

    // A rule says what it is by what it carries. An `expr:` is a rule that is finished; an id one of the
    // rule registries answers to is a rule finished in C#; and a rule carrying neither is an intention
    // the schema records, which the type page renders as declared-but-not-enforced and nothing pretends
    // to run.
    //
    // What no rule may do is claim a `severity:` with nothing behind it. Severity is the statement that
    // something acts on this — it is what puts a rule in the catalogue and in `kac checks` — so a rule
    // naming a level nothing fires at is the one arrangement that reads as enforced from every angle
    // and is not.
    private static void CheckRule(string at, string key, RuleSpec rule, List<Finding> f)
    {
        if (rule.Problem is { } problem)
        {
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-unreadable"), problem));
            return;
        }

        // Asked of every rule, before the dispatched ones are let go: a rule that runs is a rule whose
        // description reaches the type page, and that is the row a reader has to scan past.
        if (rule.Description is { Length: > Generator.DescriptionMax } description)
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"rule '{rule.Id}' on type '{key}' has a {description.Length}-character description; the "
                + $"limit is {Generator.DescriptionMax}. A description says what is checked — the reasoning "
                + "belongs in its 'message:', where the author who trips it reads it."));

        if (rule.Compiled is not null
            || DocumentRules.ByRuleId.ContainsKey(rule.Id)
            || CorpusRules.ByRuleId.ContainsKey(rule.Id)) return;

        if (rule.Severity is { } severity)
            Dispatch(at, $"rule '{rule.Id}' on type '{key}' declares 'severity: "
                         + $"{severity.ToString().ToLowerInvariant()}' and nothing dispatches it — give it an "
                         + "'expr:', implement it as a rule class, or drop the severity and leave it "
                         + "declared as an intention.", f);
    }

    // `_checks.yaml` against the code that reports, in the one direction a load can answer.
    //
    // A rule class names the check ids it reports under, so an id it emits that nothing here declares is
    // a finding a reader would meet with no entry behind it: no severity, no description, and a blank
    // cell wherever the tables render it. That is decidable from the registry, so it is decided here.
    //
    // The other direction — an entry nothing ever reports — is not. A core check reports through a
    // literal string written where the check is, and no registry holds those; the coverage gate answers
    // it instead, by failing on a declared check no fixture can reach.
    //
    private static void CheckDeclaredChecks(Schema schema, List<Finding> f)
    {
        const string at = ".schema/_checks.yaml";
        var declared = schema.Checks.Select(c => c.Id).ToHashSet();

        foreach (var id in CheckCatalogue.EmittedByRules().Where(id => !declared.Contains(id)).Distinct())
            Dispatch(at, $"a rule class reports under '{id}', which this file does not declare. A check "
                         + "reaches a reader through its entry here, so one without an entry has no severity, "
                         + "no description, and nothing to render.", f);
    }

    private static void Dispatch(string at, string message, List<Finding> f)
        => f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-dispatch"), message));

    private static string List(IEnumerable<string> values)
        => string.Join(", ", values.Order(StringComparer.Ordinal).Select(v => $"'{v}'"));

    // A vocabulary whose order is part of what it declares, quoted as declared. Sorting the tiers would
    // misstate the one thing the reader has to take from them.
    private static string Ordered(IEnumerable<string> values)
        => string.Join(", ", values.Select(v => $"'{v}'"));
}
