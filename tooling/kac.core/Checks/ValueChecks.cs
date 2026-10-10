using System.Globalization;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace kac.core;

public static class ValueChecks
{
    // `SchemaChecks` reads it to reject a `type:` nothing dispatches. `id` has no arm below: the reference pass in
    // `Validator` reads it once a `ref:` has selected the field.
    public static readonly IReadOnlyList<string> FieldTypes =
        ["date", "enum", "id", "list", "object", "string", "timestamp"];

    public static readonly IReadOnlyList<string> EntryTypes = ["id", "object", "string"];

    public static void Check(string name, YamlNode node, FieldSpec spec, DocKind kind, int frontStart,
        Report report)
    {
        // The order of these returns is the design. A field declaring `object` holds a mapping on purpose, so it is
        // read ahead of the placeholder mapping below, and a template's placeholder is read as absent before any
        // field's own check sees it.
        if (spec.Type == "object" && node is YamlMappingNode obj)
        {
            Entry(name, obj, spec, kind, frontStart, report);
            return;
        }

        // A placeholder opening a value is a flow mapping to YAML, so `review-by: {{date}}` arrives as a mapping
        // with nothing readable in it. Reported here with the fix, rather than as a malformed empty date.
        if (node is YamlMappingNode)
        {
            var key = name[(name.LastIndexOf('.') + 1)..];
            report.Err(new CheckId(kind == DocKind.Template ? "template-fields" : "bare-key"),
                $"'{name}' is read as a YAML mapping rather than a value. A placeholder that opens "
                + "one has to be quoted: " + key + ": \"{{…}}\".", Yaml.LineOf(node, frontStart));
            return;
        }

        if (kind == DocKind.Template && HasPlaceholder(node)) return;

        if (IsAbsent(node))
        {
            if (!IsBareKey(node))
                report.Err(new CheckId("bare-key"),
                    $"'{name}' is absent but not a bare key. Use '{name}:' with no value (not null, ~, \"\", or —).",
                    Yaml.LineOf(node, frontStart));

            // A field declaring `required-when:` is exempt, because whether it is optional depends on another field.
            else if (!spec.Required && spec.RequiredWhen is null)
                report.Warn(new CheckId("empty-optional-key"),
                    $"'{name}' is optional and carries no value. Remove the key, or fill it in.",
                    Yaml.LineOf(node, frontStart));
            return;
        }

        // A list is not short-circuited: there the literal is one entry, and `Sequence` exempts that entry.
        if (spec.Type != "list" && spec.IsLiteral(Yaml.Raw(node))) return;

        switch (spec.Type)
        {
            case "date": Date(name, node, frontStart, report); break;
            case "timestamp": Timestamp(name, node, frontStart, report); break;
            case "enum": Enumerated(name, node, spec, frontStart, report); break;
            case "list": Sequence(name, node, spec, kind, frontStart, report); break;

            case "object": Entry(name, node, spec, kind, frontStart, report); break;
        }

        if (spec.Type != "list" && node is YamlScalarNode)
            Pattern(name, "value", node, spec, frontStart, report);
    }

    // Pass the field's `spec` wherever there is one. Without it, every mapping reads as absent.
    public static bool IsAbsent(YamlNode node, FieldSpec? spec = null) =>
        node switch
        {
            YamlScalarNode sc => string.IsNullOrEmpty(sc.Value) || sc.Value is "~" or "null" or "Null" or "NULL",
            YamlSequenceNode seq => seq.Children.Count == 0,
            // A mapping is a value only where the field declares `object`. Anywhere else it is an unquoted placeholder.
            YamlMappingNode map => spec?.Type != "object" || map.Children.Count == 0,
            _ => false
        };

    private static bool IsBareKey(YamlNode node)
        => node is YamlScalarNode { Style: ScalarStyle.Plain } sc && string.IsNullOrEmpty(sc.Value);

    // Asked of the whole field, not each entry: a list in a template demonstrates the field's shape.
    private static bool HasPlaceholder(YamlNode node) =>
        node switch
        {
            YamlScalarNode sc => Placeholder.In(sc.Value),
            YamlSequenceNode seq => seq.Children.Any(HasPlaceholder),
            _ => false
        };

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

    private static bool IsIsoShape(string v)
        => v.Length == 10 && v[4] == '-' && v[7] == '-'
           && v[..4].All(char.IsDigit) && v[5..7].All(char.IsDigit) && v[8..].All(char.IsDigit);

    // Style is not read. A date is held to being quoted because an unquoted one is reread as a datetime
    // and shifted into the reader's zone; a `Z` instant names the same moment either way. `_checks.yaml`
    // carries that argument in full.
    private static void Timestamp(string name, YamlNode node, int frontStart, Report report)
    {
        var v = Yaml.Raw(node) ?? "";
        if (!IsInstantShape(v))
            report.Err(new CheckId("timestamp-format"),
                $"'{name}' must be a YYYY-MM-DDThh:mm:ssZ moment in UTC, got '{v}'.",
                Yaml.LineOf(node, frontStart));
        else if (!DateTimeOffset.TryParseExact(v, "yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture,
                     DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out _))
            report.Err(new CheckId("timestamp-format"),
                $"'{name}' is not a moment on the calendar, got '{v}'.", Yaml.LineOf(node, frontStart));
    }

    private static bool IsInstantShape(string v)
        => v.Length == 20 && IsIsoShape(v[..10]) && v[10] == 'T' && v[13] == ':' && v[16] == ':' && v[19] == 'Z'
           && v[11..13].All(char.IsDigit) && v[14..16].All(char.IsDigit) && v[17..19].All(char.IsDigit);

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

        // Before the entries, so an author fixing a malformed entry is not surprised by the floor on the next run.
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
            if (spec.IsLiteral(v)) continue;
            if (spec.Of == "id" && v is not null && !LooksLikeId(v))
                report.Err(new CheckId("id-format"), $"'{name}' entry '{v}' is not a valid id.",
                    Yaml.LineOf(item, frontStart));
            Pattern(name, "entry", item, spec, frontStart, report);
        }

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

        // `of: object` with no `entry:` block requires nothing here. `schema-shape` reports that schema, and a record
        // must not take the run down before it does.
        foreach (var missing in (spec.Entry ?? []).Where(k => k.Required && !present.Contains(k.Name)))
            report.Err(new CheckId("entry-key"),
                $"'{name}' entry {Names(map, spec)} is missing '{missing.Name}'.",
                Yaml.LineOf(map, frontStart));
    }

    private static string Keys(FieldSpec spec) =>
        string.Join(", ", (spec.Entry ?? []).Select(k => $"'{k.Name}'"));

    private static string Names(YamlMappingNode map, FieldSpec spec) =>
        spec.Entry is [{ } first, ..] && Yaml.Get(map, first.Name) is { } named
                                      && Yaml.Raw(named) is { Length: > 0 } value
            ? $"'{value}'"
            : "here";

    // An entry with nothing readable to sort on is skipped, because `entry-key` has already reported it.
    private static string? SortKey(YamlNode item, FieldSpec spec) =>
        spec.Of == "object"
            ? spec.Entry is [{ } first, ..] && Yaml.Get(item, first.Name) is { } named ? Yaml.Raw(named) : null
            : Yaml.Raw(item);

    // A shape, not an identity. Whether the entry names a document is the reference pass's question.
    private static bool LooksLikeId(string v)
    {
        // A shortcode may open the entry and a part may close it. Only the record between them has a shape to read. See
        // Citation.cs.
        var id = Citation.Read(v).Record;

        var dash = id.IndexOf('-');
        if (dash <= 0 || dash == id.Length - 1) return false;

        var rest = id[(dash + 1)..];
        return id[..dash].All(char.IsLower)
               && (rest == rest.ToLowerInvariant() || rest == rest.ToUpperInvariant());
    }

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
