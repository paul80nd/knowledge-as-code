namespace kac.core;

/// <summary>
/// Each vocabulary is read from the code that dispatches it, never restated. <c>docs/maintaining/checks.md</c> says
/// why.
/// </summary>
public static class SchemaChecks
{
    public static void Check(Schema schema, List<Finding> f)
    {
        // Walked in declared order, so several faults are reported in the order a reader meets them.
        UnreadKeys(".schema/_enums.yaml", schema, f);
        UnreadKeys(".schema/_tiers.yaml", schema, f);
        UnreadKeys(".schema/_checks.yaml", schema, f);
        UnreadKeys(".schema/_universal.yaml", schema, f);
        CheckTiers(schema, f);
        CheckDeclaredChecks(schema, f);
        foreach (var name in schema.UniversalOrder)
            if (schema.Universal.TryGetValue(name, out var spec))
                CheckField(".schema/_universal.yaml", name, spec, null, schema, f);

        foreach (var (key, t) in schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            var at = $".schema/{key}.yaml";

            UnreadKeys(at, schema, f);
            CheckFolder(at, key, t, f);

            if (!IdChecks.IdStyles.Contains(t.IdStyle))
                Dispatch(at, $"type '{key}' declares 'id.style: {t.IdStyle}', which no id check reads. "
                             + $"The styles the tool applies are {List(IdChecks.IdStyles)}.", f);

            if (t.IdWidth.Min > t.IdWidth.Max)
                f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                    $"type '{key}' declares 'id.width' with a 'min:' of {t.IdWidth.Min} above its 'max:' of "
                    + $"{t.IdWidth.Max}. No id can be both, so every record of the type would fail. Put the "
                    + "shorter end in 'min:'."));

            if (t.IdWidth.Min != t.IdWidth.Max && t.IdStyle == "numbered")
                f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                    $"type '{key}' is 'id.style: numbered' and declares 'id.width' as a span of {t.IdWidth}. "
                    + "A number is padded to one width so that ids sort, and a span would make '"
                    + $"{t.IdPrefix}-007' and '{t.IdPrefix}-0007' two ids for one record. Give the type an exact "
                    + "width, or the mnemonic style a span was written for."));

            if (t.FilenameCarriesId && t.IdWidth.Min != t.IdWidth.Max)
                f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                    $"type '{key}' declares 'id.width' as a span of {t.IdWidth} and leaves its filename "
                    + "carrying the id. A span admits several lengths, so a topical filename binds to "
                    + "whichever id its first segment happens to spell. Declare 'filename.carries-id: false', "
                    + "or make the width exact."));

            if (!t.FilenameCarriesId && t.IdStyle == "slug")
                f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                    $"type '{key}' is 'id.style: slug' and declares 'filename.carries-id: false'. A slug id "
                    + "is the filename stem, so there is nothing left for the id to be. Take the key out, or "
                    + "give the type a style whose id is written in frontmatter."));

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

            CheckInForce(at, key, t, schema, f);

            foreach (var name in t.FieldOrder)
                CheckField(at, name, t.Fields[name], t, schema, f);

            foreach (var rule in t.Rules)
                CheckRule(at, key, rule, schema, f);
        }

        CheckVersus(schema, f);
    }

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
                    $"type '{key}' declares a 'versus:' against itself: a disambiguation has two sides."));
                continue;
            }

            // A type this corpus declined renders no pair, so there is nothing to report.
            if (!schema.ByFolder.ContainsKey(other)) continue;

            var pair = string.CompareOrdinal(key, other) < 0 ? $"{key}|{other}" : $"{other}|{key}";
            if (declared.TryGetValue(pair, out var first))
                f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                    $"type '{key}' declares a 'versus: {other}' that '{first}.yaml' already declares. A pair is "
                    + "written once, by the type its heading is titled from."));
            else
                declared[pair] = key;
        }
    }

    /// <summary>
    /// Reported against <c>_tiers.yaml</c> whichever side is short, because its entries are the cheap side to add.
    /// </summary>
    private static void CheckTiers(Schema schema, List<Finding> f)
    {
        if (!schema.Universal.TryGetValue("tier", out var field)) return;

        const string at = ".schema/_tiers.yaml";
        var declared = schema.Tiers.Select(t => t.Name).ToList();
        var admitted = field.Values ?? [];

        foreach (var value in admitted.Where(v => !declared.Contains(v)))
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"the 'tier' field admits '{value}', and no tier here declares it: a record may carry a tier that "
                + "nothing can name on a page."));

        foreach (var tier in declared.Where(t => !admitted.Contains(t)))
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"tier '{tier}' is declared here and the 'tier' field in '_universal.yaml' does not admit it: no "
                + "document can ever carry it."));

        foreach (var tier in schema.Tiers.Where(t => t.Label.Length == 0 || t.Behaviour.Length == 0))
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"tier '{tier.Name}' declares no {(tier.Label.Length == 0 ? "'label:'" : "'behaviour:'")}. Both head "
                + "the tier's section in the generated taxonomy."));
    }

    private static void UnreadKeys(string at, Schema schema, List<Finding> f)
    {
        foreach (var key in schema.UnreadKeys.Where(k => k.File == at))
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-unknown-key"),
                $"{key.Where} declares '{key.Key}', which the loader does not read. Implement it, drop it, "
                + "or write what it was saying as 'notes:', the one key every level admits."));
    }

    private static void CheckParts(string at, string key, TypeSchema t, List<Finding> f)
    {
        if (t.Parts is not { } parts) return;

        if (!PartSpec.Sources.Contains(parts.Source))
            Dispatch(at, $"type '{key}' declares 'parts.source: {parts.Source}', which nothing extracts. The "
                         + $"sources the tool reads are {List(PartSpec.Sources)}.", f);

        if (!t.RequiredSections.Concat(t.OptionalSections).Contains(parts.Section, StringComparer.OrdinalIgnoreCase))
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"type '{key}' declares 'parts.section: {parts.Section}', and its 'sections:' block declares no "
                + "such section. Name a section the type has, or add it. No record can carry a part otherwise."));

        if (parts is { Source: PartSpec.Table, Binding.Count: 0 })
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"type '{key}' sources its parts from a table and declares no 'binding:'. Say which modals "
                + "oblige, or every row is reported as opening with none of them."));
    }

    private static void CheckExport(string at, string key, TypeSchema t, Schema schema, List<Finding> f)
    {
        if (t.Export is not { } export) return;

        var declared = t.RequiredSections.Concat(t.OptionalSections).ToList();

        foreach (var (section, fidelity) in export.Sections)
        {
            if (!declared.Contains(section, StringComparer.OrdinalIgnoreCase))
                f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                    $"type '{key}' declares 'export.sections: {section}', and its 'sections:' block declares no "
                    + "such section. Name a section the type has, or add it. Selecting by key is what keeps the "
                    + "two declarations in step."));

            Fidelity($"export.sections: {section}", fidelity, ExportSpec.Carried);
        }

        // `title` answers as a field without being one: a record's title is its H1.
        foreach (var name in export.Fields.Where(n =>
                     n != Generator.Title && schema.EffectiveField(t, n) is null))
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"type '{key}' declares 'export.fields: {name}', and neither the type nor '_universal.yaml' "
                + $"declares such a field. Name a field a record carries, or '{Generator.Title}', which is a "
                + "record's heading rather than its frontmatter."));

        if (export.Version <= 0)
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"type '{key}' declares an 'export:' block and no 'version:' above 0. A consumer reads these "
                + "files against that number, and moving it by hand is what says a reader written against the "
                + "shape before it would now be wrong."));

        if (!export.PartsDeclared) return;

        if (t.Parts is not { } parts)
        {
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"type '{key}' declares 'export.parts:' and carries no 'parts:' block. Say where a record "
                + "keeps the parts an export is to carry, or drop the entry."));
            return;
        }

        Fidelity("export.parts.fidelity:", export.Parts, ExportSpec.CarriedByParts);

        if (export.Line.Count == 0)
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"type '{key}' declares 'export.parts:' and no 'line:' beneath it. Name the keys one part "
                + "writes, each with the source filling it. Nothing else decides what a consumer reads."));

        foreach (var (name, source) in export.Line)
            Source(name, source);

        return;

        void Source(string name, string source)
        {
            if (source.Length == 0)
            {
                f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                    $"type '{key}' declares 'export.parts.line: {name}' with no source. Say where the key "
                    + "takes its value from."));
                return;
            }

            if (PartLineSource.Argument(source, PartLineSource.FrontPrefix) is { } field)
            {
                var spec = schema.EffectiveField(t, field);
                if (spec is null)
                    f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                        $"type '{key}' declares 'export.parts.line: {name}' at '{source}', and neither the type "
                        + "nor '_universal.yaml' declares such a field. A record's heading is 'part.text' rather "
                        + "than a field."));
                else if (spec.Type is "list" or "object")
                    f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                        $"type '{key}' declares 'export.parts.line: {name}' at '{source}', and that field is not a "
                        + "single value. A line reads one value from a field, so the key would be null on every "
                        + "line. Name a field with one value, or drop the key."));
                return;
            }

            if (PartLineSource.Argument(source, PartLineSource.ColumnPrefix) is { } header)
            {
                if (!parts.Columns.Contains(header, StringComparer.OrdinalIgnoreCase))
                    f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                        $"type '{key}' declares 'export.parts.line: {name}' at '{source}', and the type's "
                        + "'parts.columns:' declares no such column. Name a header a row stands under."));
                return;
            }

            if (PartLineSource.Argument(source, PartLineSource.CitationPrefix) is { } label)
            {
                if (!t.DeclaredFields.Any(d =>
                        string.Equals(d.MirrorsCitations, label, StringComparison.OrdinalIgnoreCase)))
                    f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                        $"type '{key}' declares 'export.parts.line: {name}' at '{source}', and no field of "
                        + "the type declares a 'mirrors-citations:' with that label. A footnote nothing "
                        + "reconciles is prose."));
                return;
            }

            if (PartLineSource.Argument(source, PartLineSource.AsidePrefix) is { } aside)
            {
                if (!parts.Asides.Contains(aside, StringComparer.Ordinal))
                    f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                        $"type '{key}' declares 'export.parts.line: {name}' at '{source}', and the type's "
                        + "'parts.asides:' declares no such label. Nothing would recognise the block, so the "
                        + "key would be null on every line."));
                else if (parts.Source == PartSpec.Table) RefuseBodySource(name, source);
                return;
            }

            if (!PartLineSource.Fixed.Contains(source, StringComparer.Ordinal))
            {
                Dispatch(at, $"type '{key}' declares 'export.parts.line: {name}' at source '{source}', which "
                             + $"nothing fills. A line takes {List(PartLineSource.Fixed)}, a "
                             + $"'{PartLineSource.FrontPrefix}<field>', a "
                             + $"'{PartLineSource.ColumnPrefix}<Header>', a "
                             + $"'{PartLineSource.CitationPrefix}<Label>' or a "
                             + $"'{PartLineSource.AsidePrefix}<Label>'.", f);
                return;
            }

            if (source is (PartLineSource.PartLead or PartLineSource.PartBullets) && parts.Source == PartSpec.Table)
                RefuseBodySource(name, source);

            if (source is (PartLineSource.PartLevel or PartLineSource.PartBullets) && parts.Levels.Count == 0)
                f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                    $"type '{key}' declares 'export.parts.line: {name}' at '{source}', and its 'parts:' block "
                    + "declares no binding or advisory levels. Declare the modals a part may open with, or "
                    + "drop the key."));
        }

        void RefuseBodySource(string name, string source) =>
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"type '{key}' declares 'export.parts.line: {name}' at '{source}', and sources its parts "
                + "from a table. A row is its own body, so the source would write null on every line. "
                + $"Carry the row through '{PartLineSource.PartText}' and its columns."));

        void Fidelity(string entry, string value, IReadOnlyList<string> carried)
        {
            if (value.Length == 0)
                f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                    $"type '{key}' declares '{entry}' at no fidelity. Say how much of it travels. Fidelity is "
                    + "what a consumer reads the export for, and is defaulted nowhere."));
            else if (!carried.Contains(value, StringComparer.Ordinal))
                Dispatch(at, $"type '{key}' declares '{entry}' at fidelity '{value}', which nothing carries "
                             + $"there. An export carries {List(carried)}.", f);
        }
    }

    private static void CheckInForce(string at, string key, TypeSchema t, Schema schema, List<Finding> f)
    {
        if (schema.EffectiveField(t, "status") is not { Type: "enum", Values.Count: > 0 } status) return;
        if (status.InForce is not null) return;

        f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
            $"type '{key}' declares 'status:' and no 'in-force:' beside its values. Name the value that says "
            + "a record of this type is in force, which nothing else states."));
    }

    private static void CheckFolder(string at, string key, TypeSchema t, List<Finding> f)
    {
        // An absent `folder:` and `folder: null` parse to the same empty string, so the value is asked, not the key.
        if (string.IsNullOrEmpty(t.Folder))
        {
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"type '{key}' declares no 'folder:'. Say which folder holds its records."));
            return;
        }

        if (!string.Equals(t.Folder, key, StringComparison.Ordinal))
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"type '{key}' declares 'folder: {t.Folder}', and this file is named for '{key}'. A document's "
                + "type is read from the folder it sits in, and that lookup uses the file's name. Records under "
                + $"'{t.Folder}/' would reach nothing. Make the two names agree."));
    }

    /// <summary>
    /// <c>label:</c> is not required, because it falls back to the type name capitalised. Nothing derives a plural.
    /// </summary>
    private static void CheckProse(string at, string key, TypeSchema t, List<Finding> f)
    {
        Line("label-plural", t.LabelPlural, "what a folder of these is called (\"ADRs\", \"Policies\", \"NFRs\")");
        Line("summary", t.Summary, "what the type is");
        Line("goes-here", t.GoesHere, "what a contributor has in hand when this type is the answer");

        // Not held to the cell bound: `detail:` renders as prose, not into a table.
        if (t.Detail.Length == 0)
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"type '{key}' declares no 'detail:'. Say what the type carries beyond its first sentence, and the "
                + "edge a reader is most likely to walk over."));

        // Only the prior art is required. A type with no ancestor answers the rest with an empty pair.
        if (t.Lineage is null)
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"type '{key}' declares no 'lineage.prior-art:'. Name what the type is nearest to, or say that "
                + "nothing established fits. Claiming an ancestor a type does not have is worse than admitting none."));

        return;

        void Line(string name, string value, string says)
        {
            if (value.Length == 0)
                f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                    $"type '{key}' declares no '{name}:'. Say {says}, in one line the taxonomy and the "
                    + "corpus index can be generated from."));
            else if (value.Length > ChecksTable.DescriptionMax)
                f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                    $"type '{key}' has a {value.Length}-character '{name}:'; the limit is "
                    + $"{ChecksTable.DescriptionMax}. It is rendered as a table cell. The fuller account belongs "
                    + $"on {(t.Page.Length > 0 ? t.Page : $"{key}.md")}."));
        }
    }

    /// <summary>
    /// <c>t</c> is null for a universal field, which belongs to no type and is asked none of a type's own questions.
    /// </summary>
    private static void CheckField(string at, string name, FieldSpec spec, TypeSchema? t, Schema schema,
        List<Finding> f)
    {
        if (spec.Problem is { } problem)
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-unreadable"), problem));

        CheckDeclaredTypes(at, $"field '{name}'", spec, f);

        if (spec.Values is { Count: > 0 } && spec.Type != "enum")
            Dispatch(at, $"field '{name}' is 'type: {spec.Type}' and declares 'values:', which only an "
                         + "enum's range is read from. Declare it 'type: enum', or drop the values.", f);

        if (spec.InForce is { } inForce)
        {
            if (spec.Type != "enum")
                Dispatch(at, $"field '{name}' is 'type: {spec.Type}' and declares 'in-force:', which names one "
                             + "of an enum's values. Declare it 'type: enum', or drop the key.", f);
            else if (spec.Values is { Count: > 0 } range && !range.Contains(inForce, StringComparer.Ordinal))
                f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                    $"field '{name}' declares 'in-force: {inForce}', and its 'values:' does not include it. "
                    + $"Name one of {List(range)}."));
        }

        if (spec.MinItems is not null && spec.Type != "list")
            Dispatch(at, $"field '{name}' is 'type: {spec.Type}' and declares 'min-items:', which only a "
                         + "list's length is read against. Declare it 'type: list', or drop the floor.", f);

        if (spec.MinRecords is not null && spec.Type != "list")
            Dispatch(at, $"field '{name}' is 'type: {spec.Type}' and declares 'min-records:', which is read "
                         + "against the entries of a list. Declare it 'type: list', or drop the floor.", f);

        var holdsObjects = spec.Type == "object" || spec.Of == "object";
        var declared = spec.Type == "object" ? "type: object" : $"of: {spec.Of}";

        if (holdsObjects && spec.Entry is null or { Count: 0 })
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"field '{name}' declares '{declared}' and neither an 'entry:' block nor a 'shape:', so "
                + "nothing says what it holds. Declare the keys, name a shape, or give it a scalar type."));

        if (spec.Of == "object")
            foreach (var key in ScalarEntryKeys(spec))
                f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                    $"field '{name}' declares 'of: object' and '{key}:'. That key reads each entry as one plain "
                    + "value, and every entry here is an object, so it would check nothing. Drop the key, or "
                    + "declare the field 'of: id'."));

        if (spec.Entry is { Count: > 0 } && !holdsObjects)
            Dispatch(at, $"field '{name}' declares "
                         + $"{(spec.Shape.Length > 0 ? $"'shape: {spec.Shape}'" : "an 'entry:' block")} and "
                         + $"'type: {spec.Type}', and an object's keys are read only where the value is "
                         + "an object. Declare 'type: object' or 'of: object', or drop it.", f);

        if (spec.From is { } from && !Doc.DerivedSources.Contains(from))
            Dispatch(at, $"field '{name}' declares 'from: {from}', which no derivation reads. The "
                         + $"sources the tool resolves are {List(Doc.DerivedSources)}.", f);

        if (spec.From is not null && spec.Required)
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"field '{name}' declares both 'from: {spec.From}' and 'required: true'. A derived field "
                + "is not the author's to write, so nothing can satisfy the requirement. Drop 'required'."));

        if (spec.PartRequired && spec.Refs.Count == 0)
            Dispatch(at, $"field '{name}' declares 'part-required: true' and no 'ref:'. A part is looked up "
                         + "in the record the reference names, so nothing reads the key. Declare a 'ref:', "
                         + "or drop the key.", f);

        if (spec.PartRequired)
            foreach (var folder in spec.Refs)
                if (schema.ByFolder.TryGetValue(folder, out var target) && target.Parts is null)
                    f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                        $"field '{name}' declares 'part-required: true' and points at '{folder}', which keeps "
                        + "no parts. Nothing an author writes could reach one, so every record filling the "
                        + "field would fail. Take the key out, or give the type a 'parts:' block."));

        if (t is not null && spec.MirrorsSection is { } section
                          && !t.RequiredSections.Concat(t.OptionalSections)
                              .Contains(section, StringComparer.OrdinalIgnoreCase))
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"field '{name}' declares 'mirrors-section: {section}', and the type's 'sections:' block "
                + "declares no such section. Name a section the type has, or add it."));

        if (spec.MirrorsCitations is { } label && spec.Refs.Count == 0)
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"field '{name}' declares 'mirrors-citations: {label}' and no 'ref:'. The line gathers "
                + "citations of another record, and this field carries none, so the two sides could "
                + "never meet. Declare a 'ref:', or drop the key."));
    }

    /// <summary>The keys whose checks read the field's value through <c>Doc.FrontList</c>.</summary>
    private static IEnumerable<string> ScalarEntryKeys(FieldSpec spec)
    {
        if (spec.Refs.Count > 0) yield return "ref";
        if (spec.MinRecords is not null) yield return "min-records";
        if (spec.MirrorsSection is not null) yield return "mirrors-section";
        if (spec.Reciprocal is not null) yield return "reciprocal";
        if (spec.MirrorsCitations is not null) yield return "mirrors-citations";
    }

    private static void CheckRule(string at, string key, RuleSpec rule, Schema schema, List<Finding> f)
    {
        if (rule.SeverityWritten is { } written && !RuleSpec.Severities.ContainsKey(written))
            Dispatch(at, $"rule '{rule.Id}' on type '{key}' declares 'severity: {written}', which the tool does not "
                         + $"read. A severity is {List(RuleSpec.Severities.Keys)}.", f, rule.Line);

        if (rule.Problem is { } problem)
        {
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-unreadable"), problem));
            return;
        }

        // Asked of every rule before the dispatched ones return, because a rule that runs has its description on the
        // type page.
        if (rule.Description is { Length: > ChecksTable.DescriptionMax } description)
            f.Add(new Finding(at, null, Sev.Error, new CheckId("schema-shape"),
                $"rule '{rule.Id}' on type '{key}' has a {description.Length}-character description; the "
                + $"limit is {ChecksTable.DescriptionMax}. A description says what is checked. The reasoning "
                + "belongs in its 'message:', where the author who trips it reads it."));

        CheckConfig(at, key, rule, schema, f);

        if (rule.Compiled is not null
            || DocumentRules.ByRuleId.ContainsKey(rule.Id)
            || CorpusRules.ByRuleId.ContainsKey(rule.Id)) return;

        if (rule.Severity is { } severity)
            Dispatch(at, $"rule '{rule.Id}' on type '{key}' declares 'severity: "
                         + $"{severity.ToString().ToLowerInvariant()}' and nothing dispatches it. Give it an "
                         + "'expr:', implement it as a rule class, or drop the severity and leave it "
                         + "declared as an intention.", f);
    }

    /// <summary>
    /// A rule's config keys are what its class <c>Reads</c>, and an <c>expr:</c> rule reads none. A rule nothing
    /// dispatches is an intention, and its keys are left alone. <c>Validator</c> judges a corpus rule's values, because
    /// they may name something only the corpus has.
    /// </summary>
    private static void CheckConfig(string at, string key, RuleSpec rule, Schema schema, List<Finding> f)
    {
        IDocumentRule? document = null;
        IReadOnlyList<string>? reads = rule.Compiled is not null ? []
            : DocumentRules.ByRuleId.TryGetValue(rule.Id, out document) ? document.Reads
            : CorpusRules.ByRuleId.TryGetValue(rule.Id, out var corpus) ? corpus.Reads
            : null;
        if (reads is null) return;

        var reader = rule.Compiled is not null ? "an 'expr:' rule reads no config"
            : reads.Count == 0 ? "its rule class reads no config"
            : $"its rule class reads only {List(reads)}";

        foreach (var config in rule.Config.Keys.Where(c => !reads.Contains(c, StringComparer.Ordinal)))
            Dispatch(at, $"rule '{rule.Id}' on type '{key}' declares '{config}:', and {reader}. Drop the key.", f,
                rule.Line);

        foreach (var problem in document?.Misconfigured(rule, schema) ?? [])
            Misconfigured(at, key, rule, problem, f);
    }

    /// <summary>Reported against the rule's entry in the type's <c>.schema/</c> file.</summary>
    public static void Misconfigured(TypeSchema t, RuleSpec rule, string problem, List<Finding> f)
        => Misconfigured($".schema/{t.Key}.yaml", t.Key, rule, problem, f);

    private static void Misconfigured(string at, string key, RuleSpec rule, string problem, List<Finding> f)
        => Dispatch(at, $"rule '{rule.Id}' on type '{key}': {problem} The rule checks no record until this is "
                        + "fixed.", f, rule.Line);

    /// <summary>
    /// One direction only. An entry nothing reports is the coverage gate's to fail, because a core check reports
    /// through a literal no registry holds.
    /// </summary>
    private static void CheckDeclaredChecks(Schema schema, List<Finding> f)
    {
        const string at = ".schema/_checks.yaml";
        var declared = schema.Checks.Select(c => c.Id).ToHashSet();

        foreach (var id in CheckCatalogue.EmittedByRules().Where(id => !declared.Contains(id)).Distinct())
            Dispatch(at, $"a rule class reports under '{id}', which this file does not declare. A check "
                         + "reaches a reader through its entry here, so one without an entry has no severity, "
                         + "no description, and nothing to render.", f);
    }

    /// <summary>
    /// The vocabularies are <c>ValueChecks</c>'s, kept beside the switch dispatching them. Asked of entry keys too, to
    /// whatever depth the field nests, because <c>Entry</c> sends each key back through <c>Check</c>.
    /// </summary>
    private static void CheckDeclaredTypes(string at, string what, FieldSpec spec, List<Finding> f,
        bool insideAnObject = false)
    {
        if (!ValueChecks.FieldTypes.Contains(spec.Type))
            Dispatch(at, $"{what} declares 'type: {spec.Type}', which no value check reads. The types the "
                         + $"tool acts on are {List(ValueChecks.FieldTypes)}.", f);

        // Before the vocabulary below, so a scalar carrying `of:` is not told its entry type is wrong.
        if (spec.Of is { } of)
        {
            if (spec.Type != "list")
                Dispatch(at, $"{what} is 'type: {spec.Type}' and declares 'of: {of}', which is read from "
                             + "the entries of a list. Declare it 'type: list', or drop the key.", f);
            else if (!ValueChecks.EntryTypes.Contains(of))
                Dispatch(at, $"{what} declares 'of: {of}', which no entry check reads. A list's entries "
                             + $"are {List(ValueChecks.EntryTypes)}.", f);
        }

        if (spec.DeclaresId)
        {
            var declared = spec.Type == "id" ? "type: id" : "of: id";

            if (insideAnObject)
                Dispatch(at, $"{what} declares '{declared}', and the reference pass reads a field's own "
                             + "value and never an entry of one. Nothing resolves the id, with a 'ref:' "
                             + "or without. Declare the value a string, or make the id a field of its own.",
                    f);
            else if (spec.Refs.Count == 0)
                Dispatch(at, $"{what} declares '{declared}' and no 'ref:'. An id is resolved against the "
                             + "folders a reference names, so nothing checks what the value points at. "
                             + "Declare a 'ref:', or declare the value a string.", f);
        }

        foreach (var key in spec.Entry ?? [])
            CheckDeclaredTypes(at, $"{what} entry key '{key.Name}'", key, f, true);
    }

    private static void Dispatch(string at, string message, List<Finding> f, int? line = null)
        => f.Add(new Finding(at, line, Sev.Error, new CheckId("schema-dispatch"), message));

    private static string List(IEnumerable<string> values)
        => string.Join(", ", values.Order(StringComparer.Ordinal).Select(v => $"'{v}'"));

    /// <summary>Quoted as declared, because sorting the tiers would misstate them.</summary>
    private static string Ordered(IEnumerable<string> values)
        => string.Join(", ", values.Select(v => $"'{v}'"));
}
