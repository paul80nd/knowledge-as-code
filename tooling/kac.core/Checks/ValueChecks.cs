using System.Globalization;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace kac.core;

// What one frontmatter value is held to, given the field the schema declares for it.
//
// Everything here reads a `FieldSpec` and a `YamlNode` and nothing else about the document, so what a
// value is judged against is decidable from the two of them and a line to report on. That is what lets
// a date's calendar arithmetic, an enum's casing, a list's floor and a per-entry pattern be tested from
// a declaration and a string rather than from a corpus on disk.
//
// `Check` is the whole of the sequence, in the order it runs. The order carries the design: a template's
// unfilled marks are read as absent before any field's own check sees them, so a placeholder is never
// reported as a malformed date. Read it top to bottom and the exemptions explain themselves.
public static class ValueChecks
{
    // One value, and every question the schema's declaration asks of it.
    //
    // The caller has already established that the key is known, so what arrives here is a value the type
    // expects, carrying something the type may not. An unknown key is `unknown-key`'s, and has no
    // `FieldSpec` to judge against.
    public static void Check(string name, YamlNode node, FieldSpec spec, DocKind kind, int frontStart,
        Report report)
    {
        // A placeholder opening a value is a flow mapping to YAML, not text. `{` is an indicator in that
        // one position, so `review-by: {{date}}` parses as a mapping and arrives here with nothing
        // readable in it. Reported with the fix rather than left to the value checks, which would say
        // the date is malformed and quote an empty string back at whoever wrote it.
        if (kind == DocKind.Template && node is YamlMappingNode)
        {
            report.Err(new CheckId("template-fields"),
                $"'{name}' is read as a YAML mapping rather than a value. A placeholder that opens "
                + "one has to be quoted: " + name + ": \"{{…}}\".", Yaml.LineOf(node, frontStart));
            return;
        }

        // A placeholder is not a value: `{{slug}}` is the instruction to supply one. It is read as absent
        // in a template. The field's own checks would otherwise report the mark as a malformed date, an
        // unknown enum value or an id of the wrong shape. That is three ways of saying the file has not
        // been filled in.
        if (kind == DocKind.Template && HasPlaceholder(node)) return;

        if (IsAbsent(node))
        {
            if (!IsBareKey(node))
                report.Err(new CheckId("bare-key"),
                    $"'{name}' is absent but not a bare key. Use '{name}:' with no value (not null, ~, \"\", or —).",
                    Yaml.LineOf(node, frontStart));
            return;
        }

        // A word the field admits beside its declared type: `last-rehearsed: "never"`. It is taken as
        // written and nothing further is asked of it, which is the whole of what `allow-literal` means.
        // A list is not short-circuited here: there the literal is one entry among ids, and `Sequence`
        // exempts that entry rather than the field.
        if (spec.Type != "list" && spec.IsLiteral(Yaml.Raw(node))) return;

        switch (spec.Type)
        {
            case "date": Date(name, node, frontStart, report); break;
            case "enum": Enumerated(name, node, spec, frontStart, report); break;
            case "list": Sequence(name, node, spec, kind, frontStart, report); break;
        }

        // A declared `pattern:` applies to a scalar field's value; for a list it applies to each entry,
        // so `Sequence` handles that half where it already walks the sequence.
        if (spec.Type != "list" && node is YamlScalarNode)
            Pattern(name, "value", node, spec, frontStart, report);
    }

    // Whether a value says "nothing supplied". A bare key and the several ways of writing an explicit
    // nothing are one state to every check downstream, and `bare-key` above is the one place the
    // difference between them is the finding.
    //
    // Public because the required-field pass asks the same question before this class is reached: a
    // field that is declared required and carries an explicit nothing is missing, and a second reading
    // of "absent" would be free to disagree with this one.
    public static bool IsAbsent(YamlNode node) =>
        node switch
        {
            YamlScalarNode sc => string.IsNullOrEmpty(sc.Value) || sc.Value is "~" or "null" or "Null" or "NULL",
            YamlSequenceNode seq => seq.Children.Count == 0,
            _ => false
        };

    private static bool IsBareKey(YamlNode node)
        => node is YamlScalarNode { Style: ScalarStyle.Plain } sc && string.IsNullOrEmpty(sc.Value);

    // Whether a value carries the placeholder mark anywhere: the scalar itself, or any entry of a
    // sequence. Asked of the whole field rather than of each entry, because a list in a template is a
    // demonstration of the field's shape and `[ svc-{{a}}, svc-real ]` is not a state worth modelling.
    private static bool HasPlaceholder(YamlNode node) =>
        node switch
        {
            YamlScalarNode sc => Placeholder.In(sc.Value),
            YamlSequenceNode seq => seq.Children.Any(HasPlaceholder),
            _ => false
        };

    // Shape then calendar, under one id: both answers leave the author with the same thing to do, and the
    // message is what tells them which they wrote. `2026/06/12` is not written as a date at all, where
    // `2026-13-40` is written as one and names a day that has never existed.
    private static void Date(string name, YamlNode node, int frontStart, Report report)
    {
        var sc = node as YamlScalarNode;
        var v = sc?.Value ?? "";
        var quoted = sc?.Style is ScalarStyle.DoubleQuoted or ScalarStyle.SingleQuoted;
        if (!quoted)
            report.Err(new CheckId("date-quoted"), $"'{name}' date must be quoted, e.g. \"{v}\".",
                Yaml.LineOf(node, frontStart));

        if (!IsIsoShape(v))
            report.Err(new CheckId("date-format"), $"'{name}' must be a YYYY-MM-DD date, got '{v}'.",
                Yaml.LineOf(node, frontStart));
        else if (!DateOnly.TryParseExact(v, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            report.Err(new CheckId("date-format"),
                $"'{name}' is not a date on the calendar, got '{v}'.", Yaml.LineOf(node, frontStart));
    }

    // Written as a date, which is a question about the characters. Whether those characters name a day is
    // `DateOnly`'s to answer, and asking it here as well would be two tests of the same string with one
    // able to disagree with the other.
    private static bool IsIsoShape(string v)
        => v.Length == 10 && v[4] == '-' && v[7] == '-'
           && v[..4].All(char.IsDigit) && v[5..7].All(char.IsDigit) && v[8..].All(char.IsDigit);

    // Named for what it judges rather than for the keyword, which the language has taken.
    private static void Enumerated(string name, YamlNode node, FieldSpec spec, int frontStart, Report report)
    {
        var v = Yaml.Raw(node);
        if (v is null)
        {
            report.Err(new CheckId("enum"), $"'{name}' must be a scalar.", Yaml.LineOf(node, frontStart));
            return;
        }

        if (spec.Values is not null && !spec.Values.Contains(v))
            report.Err(new CheckId("enum"),
                $"'{name}' value '{v}' is not one of: {string.Join(", ", spec.Values)}.",
                Yaml.LineOf(node, frontStart));
        if (v != v.ToLowerInvariant())
            report.Err(new CheckId("enum-lowercase"), $"'{name}' enum value '{v}' must be lowercase.",
                Yaml.LineOf(node, frontStart));
    }

    private static void Sequence(string name, YamlNode node, FieldSpec spec, DocKind kind, int frontStart,
        Report report)
    {
        if (node is not YamlSequenceNode seq)
        {
            report.Err(new CheckId("list"), $"'{name}' must be a YAML sequence.", Yaml.LineOf(node, frontStart));
            return;
        }

        // A floor the schema sets on a field whose value is its breadth. Reported before the entries are
        // read, because an author told both that the list is short and that one of its entries is
        // malformed will fix the second and re-run to find the first.
        if (spec.MinItems is { } min && seq.Children.Count < min)
            report.Err(new CheckId("min-items"),
                $"'{name}' has {seq.Children.Count} {(seq.Children.Count == 1 ? "entry" : "entries")}: "
                + $"the schema asks for at least {min}.", Yaml.LineOf(node, frontStart));

        foreach (var item in seq.Children)
        {
            if (spec.Of == "object")
            {
                Entry(name, item, spec, kind, frontStart, report);
                continue;
            }

            var v = Yaml.Raw(item);
            if (spec.IsLiteral(v)) continue; // a word the field admits beside its ids: `applies-to: [all]`
            if (spec.Of == "id" && v is not null && !LooksLikeId(v))
                report.Err(new CheckId("id-format"), $"'{name}' entry '{v}' is not a valid id.",
                    Yaml.LineOf(item, frontStart));
            Pattern(name, "entry", item, spec, frontStart, report);
        }

        // Every list field in the taxonomy is a set: no field's sequence carries meaning. So
        // alphabetical is the order that scan-reads, and the one order two authors will agree on. Only
        // the first pair out of order is reported. The rest are noise once the author re-sorts the field.
        for (var i = 1; i < seq.Children.Count; i++)
        {
            if (SortKey(seq.Children[i - 1], spec) is not { } prev
                || SortKey(seq.Children[i], spec) is not { } cur) continue;
            if (Natural.Compare(prev, cur) <= 0) continue;
            report.Warn(new CheckId("list-order"),
                $"'{name}' is not in alphabetical order: '{cur}' should come before '{prev}'.",
                Yaml.LineOf(seq.Children[i], frontStart));
            break;
        }
    }

    // One entry of a list whose entries are objects, held to the shape the field declares for them.
    //
    // Three faults, and each is the entry's own rather than the field's: the entry is not a mapping at
    // all, it carries a key the shape does not declare, or it omits one the shape requires. Every key it
    // does declare goes back through `Check`, so a nested list is held to its own order and a nested
    // scalar to its own pattern without a second reading of either.
    private static void Entry(string name, YamlNode item, FieldSpec spec, DocKind kind, int frontStart,
        Report report)
    {
        if (item is not YamlMappingNode map)
        {
            report.Err(new CheckId("entry-shape"),
                $"'{name}' entry must be a mapping, with the keys {Keys(spec)}.",
                Yaml.LineOf(item, frontStart));
            return;
        }

        var present = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (key, value) in Yaml.Map(map))
        {
            present.Add(key);
            if (spec.EntryKey(key) is not { } inner)
            {
                report.Err(new CheckId("entry-key"),
                    $"'{name}' entry carries '{key}', and the schema declares {Keys(spec)}.",
                    Yaml.LineOf(value, frontStart));
                continue;
            }

            Check($"{name}.{key}", value, inner, kind, frontStart, report);
        }

        // Named against the entry rather than the field, using whatever the entry does carry to say
        // which of several it is. An entry missing the very key that names it has nothing to be quoted
        // by, and the line is what locates it instead.
        foreach (var missing in spec.Entry!.Where(k => k.Required && !present.Contains(k.Name)))
            report.Err(new CheckId("entry-key"),
                $"'{name}' entry {Names(map, spec)} is missing '{missing.Name}'.",
                Yaml.LineOf(map, frontStart));
    }

    // The entry keys a field declares, in declared order, for a message that has to list them.
    private static string Keys(FieldSpec spec) =>
        string.Join(", ", (spec.Entry ?? []).Select(k => $"'{k.Name}'"));

    // How a message points at one entry among several: by what its naming key holds, and by nothing
    // where that key is the one absent.
    private static string Names(YamlMappingNode map, FieldSpec spec) =>
        spec.Entry is [{ } first, ..] && Yaml.Get(map, first.Name) is { } named
                                      && Yaml.Raw(named) is { Length: > 0 } value
            ? $"'{value}'"
            : "here";

    // What an entry sorts on. A scalar sorts on itself; an object sorts on the first key its shape
    // declares, which is the one naming it. An entry with nothing readable there is skipped rather than
    // sorted as an empty string, because `entry-key` above has already reported it and a second finding
    // about its position would be about the first fault.
    private static string? SortKey(YamlNode item, FieldSpec spec) =>
        spec.Of == "object"
            ? spec.Entry is [{ } first, ..] && Yaml.Get(item, first.Name) is { } named ? Yaml.Raw(named) : null
            : Yaml.Raw(item);

    // Whether an entry is shaped like an id at all: a lower-case type prefix, a hyphen, then a
    // discriminator. The prefix names a type and is lower-case in every style. The discriminator is
    // upper-case where the type numbers its documents by mnemonic (`pol-VURM`), and lower-case where it
    // numbers or slugs them (`adr-0007`, `svc-search`). So the case of the two halves is asked
    // separately, and a mixed-case discriminator is what falls out.
    //
    // A shape and not an identity. Whether the entry names a document the corpus holds is the reference
    // pass's question, and that pass reads the corpus; everything here is decidable from a declaration
    // and a string, which is what lets these checks be tested without one.
    private static bool LooksLikeId(string v)
    {
        // A producer's shortcode may open it, as `eng:pol-VURM`. What follows is held to the same shape
        // whichever corpus wrote it. See Citation.cs.
        var (_, id) = Citation.Split(v);

        var dash = id.IndexOf('-');
        if (dash <= 0 || dash == id.Length - 1) return false;

        var rest = id[(dash + 1)..];
        return id[..dash].All(char.IsLower)
               && (rest == rest.ToLowerInvariant() || rest == rest.ToUpperInvariant());
    }

    // A field's declared `pattern:` is the schema's own regex, applied to whatever scalar carries the
    // value. `noun` distinguishes a scalar field's "value" from a list's "entry" in the message.
    private static void Pattern(string name, string noun, YamlNode node, FieldSpec spec, int frontStart,
        Report report)
    {
        if (spec.PatternRegex is null) return;
        var v = Yaml.Raw(node);
        if (v is null) return;
        if (!spec.PatternRegex.IsMatch(v))
            report.Err(new CheckId("field-pattern"),
                $"'{name}' {noun} '{v}' does not match {spec.Pattern}.", Yaml.LineOf(node, frontStart));
    }
}
