// ---------------------------------------------------------------------------
// The schema's own declarations
// ---------------------------------------------------------------------------

namespace kac.core;

// What the schema says the tool will do, held against what the tool can actually do.
//
// Every other check reads a document. This one reads the files that decide how documents are read, and
// it exists because those files are copied into corpora whose authors cannot ask what a key meant. A
// declaration nothing dispatches is not inert to a reader: `rules:` is documented as behaviour the
// validator applies, so a rule id no code answers to reads as a commitment, and a `ref:` at a type the
// corpus never adopted reads as a link that is being checked. Both were, until this pass.
//
// The question asked of each value is not "is this key spelled right" but "is there code that acts on
// this value". `style: literal` is a real style and would pass a spelling test; what makes it sound is
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
        UnreadKeys(".schema/_universal.yaml", schema, f);
        foreach (var name in schema.UniversalOrder)
            if (schema.Universal.TryGetValue(name, out var spec))
                CheckField(".schema/_universal.yaml", name, spec, schema, null, f);

        foreach (var (key, t) in schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            var at = $".schema/{key}.yaml";

            UnreadKeys(at, schema, f);
            CheckShape(at, key, t, f);

            if (!IdChecks.IdStyles.Contains(t.IdStyle))
                Dispatch(at, $"type '{key}' declares 'id.style: {t.IdStyle}', which no id check reads. "
                             + $"The styles the tool applies are {List(IdChecks.IdStyles)}.", f);

            if (t.IndexOrder.Length > 0 && !Generator.IndexOrders.Contains(t.IndexOrder))
                Dispatch(at, $"type '{key}' declares 'index.order: {t.IndexOrder}', which the generator does "
                             + $"not read. An index is written {List(Generator.IndexOrders)}.", f);

            foreach (var name in t.FieldOrder)
                CheckField(at, name, t.Fields[name], schema, t, f);

            foreach (var rule in t.Rules)
                CheckRule(at, key, rule, f);
        }
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
            f.Add(new Finding(at, null, Sev.Error, "schema-unknown-key",
                $"{key.Where} declares '{key.Key}', which the loader does not read — implement it, drop it, "
                + "or write what it was saying as 'notes:', the one key every level admits."));
    }

    // A type is a folder of records or a single document, and the two disagree about `folder:` rather
    // than merely differing: a collection with no folder has nowhere to put a record, and a
    // single-document type with one has somewhere it must not. Both are silent otherwise — an absent
    // `folder:` and a deliberate `folder: null` parse to the same empty string, which is the reason
    // `shape:` is declared rather than inferred from the folder in the first place.
    private static void CheckShape(string at, string key, TypeSchema t, List<Finding> f)
    {
        if (t.Shape is not (TypeSchema.CollectionShape or TypeSchema.SingleDocumentShape))
        {
            Dispatch(at, $"type '{key}' declares 'shape: {t.Shape}', which nothing acts on. A type is "
                         + $"'{TypeSchema.CollectionShape}' or '{TypeSchema.SingleDocumentShape}'.", f);
            return;
        }

        switch (t.IsSingleDocument)
        {
            case false when string.IsNullOrEmpty(t.Folder):
                f.Add(new Finding(at, null, Sev.Error, "schema-shape",
                    $"type '{key}' is a collection and declares no 'folder:' — say which folder holds its "
                    + "records, or declare it 'shape: single-document'."));
                break;
            case true when !string.IsNullOrEmpty(t.Folder):
                f.Add(new Finding(at, null, Sev.Error, "schema-shape",
                    $"type '{key}' is single-document and declares 'folder: {t.Folder}' — its page is the "
                    + "document, so there is no folder of records to name."));
                break;
        }
    }

    // `t` is the type declaring the field, and is null for a universal one — a field declared for every
    // type belongs to none of them, so the questions that read the type's own declarations are not asked
    // of it.
    private static void CheckField(string at, string name, FieldSpec spec, Schema schema, TypeSchema? t,
        List<Finding> f)
    {
        if (spec.Problem is { } problem)
            f.Add(new Finding(at, null, Sev.Error, "schema-unreadable", problem));

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

        // Any section reconciles, so this is not a vocabulary the tool holds — which is why nothing
        // else would catch a section the type never offers. The reconciliation would run against a
        // heading no record may carry and report every id in the field as missing from it.
        if (t is not null && spec.MirrorsSection is { } section
                          && !t.RequiredSections.Concat(t.OptionalSections)
                              .Contains(section, StringComparer.OrdinalIgnoreCase))
            f.Add(new Finding(at, null, Sev.Error, "schema-shape",
                $"field '{name}' declares 'mirrors-section: {section}', and the type's 'sections:' block "
                + "declares no such section — name a section the type has, or add it."));
    }

    // A rule says what it is by what it carries. An `expr:` is a rule that is finished; a matching
    // `IDocumentRule` is a rule finished in C#; and a rule carrying neither is an intention the schema
    // records, which the type page renders as declared-but-not-enforced and nothing pretends to run.
    //
    // What no rule may do is claim a `severity:` with nothing behind it. Severity is the statement that
    // something acts on this — it is what puts a rule in the catalogue and in `kac checks` — so a rule
    // naming a level nothing fires at is the one arrangement that reads as enforced from every angle
    // and is not.
    private static void CheckRule(string at, string key, RuleSpec rule, List<Finding> f)
    {
        if (rule.Problem is { } problem)
        {
            f.Add(new Finding(at, null, Sev.Error, "schema-unreadable", problem));
            return;
        }

        if (rule.Compiled is not null || DocumentRules.ByRuleId.ContainsKey(rule.Id)) return;

        if (rule.Severity is { } severity)
            Dispatch(at, $"rule '{rule.Id}' on type '{key}' declares 'severity: "
                         + $"{severity.ToString().ToLowerInvariant()}' and nothing dispatches it — give it an "
                         + "'expr:', implement it as a DocumentRule, or drop the severity and leave it "
                         + "declared as an intention.", f);
    }

    private static void Dispatch(string at, string message, List<Finding> f)
        => f.Add(new Finding(at, null, Sev.Error, "schema-dispatch", message));

    private static string List(IEnumerable<string> values)
        => string.Join(", ", values.Order(StringComparer.Ordinal).Select(v => $"'{v}'"));
}
