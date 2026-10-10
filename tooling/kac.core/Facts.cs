using System.Collections.Concurrent;
using System.Globalization;
using System.Text.RegularExpressions;
using Markdig.Syntax;
using YamlDotNet.RepresentationModel;

namespace kac.core;

// Everything a rule expression may ask. A new fact is one method here and one entry in `RuleExpr`'s function
// table. Built per document and discarded, which is what makes its caches safe.
public sealed class Facts(Doc doc, DateOnly today)
{
    // Null where the document does not carry the key, or carries it bare. A field the type derives answers with
    // what it derived.
    public string? Field(string name) => doc.FrontScalar(name);

    // Asks either shape a field may be written in, so `present('derived-from')` is true of a scalar or a list.
    // A bare key and an empty sequence are both absent.
    public bool Present(string name) => Entries(name) > 0;

    // Zero for an absent field, so a rule that must stay quiet on one guards with `present()` first.
    public int Entries(string name) => doc.FrontList(name).Count;

    // Case-insensitive, matching `required-section`.
    public bool Section(string title) =>
        doc.Sections.Any(s => string.Equals(s.Title, title, StringComparison.OrdinalIgnoreCase));

    public int SectionCount(string title) =>
        doc.Sections.Count(s => string.Equals(s.Title, title, StringComparison.OrdinalIgnoreCase));

    // Empty where the document has no H2.
    public string FirstSection() => doc.Sections.Count > 0 ? doc.Sections[0].Title : "";

    public int Links() => doc.Links.Count;

    // The run's day as an ISO date, so a rule compares it with a date field as a string.
    public string Today() => today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    // An ISO 8601 duration, or empty where nothing can be measured. See Instants.Span.
    public string Span(string from, string to) => Instants.Span(Field(from), Field(to));

    // Zero where nothing can be measured. See Instants.Days.
    public int Days(string from, string to) => Instants.Days(Field(from), Field(to));

    // Zero where nothing can be measured. See Instants.Days.
    public int DaysSince(string from) => Instants.Days(Field(from), Today());

    // Read on the source, so code fences, link targets and markdown syntax are in scope. `docs/design/checks.md`
    // says which rules need that. Frontmatter is excluded.
    public bool Matches(string pattern) => Pattern(pattern).IsMatch(Body);

    // False for an absent field. Whether it ought to be there is `required-field`'s question.
    public bool FieldMatches(string name, string pattern) =>
        doc.FrontScalar(name) is { Length: > 0 } value && Pattern(pattern).IsMatch(value);

    // A pattern asked of one key in every object the field holds. True where the field or the key is absent, so
    // a false means the value is there and wrong.
    public bool EntriesMatch(string field, string key, string pattern)
    {
        var re = Pattern(pattern);

        return Objects(field).All(o => Yaml.Get(o, key) is not YamlScalarNode { Value: { Length: > 0 } v }
                                       || re.IsMatch(v));
    }

    // True where the field is absent, and an object without the key counts for nothing.
    public bool EntriesUnique(string field, string key)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);

        return Objects(field).All(o => Yaml.Get(o, key) is not YamlScalarNode { Value: { Length: > 0 } v }
                                       || seen.Add(v));
    }

    private IEnumerable<YamlMappingNode> Objects(string field)
    {
        if (doc.Front is null) return [];

        return Yaml.Get(doc.Front, field) switch
        {
            YamlMappingNode map => [map],
            YamlSequenceNode seq => seq.Children.OfType<YamlMappingNode>(),
            _ => []
        };
    }

    // False where the document has no such section.
    public bool SectionMatches(string title, string pattern) =>
        SectionText(title) is { } text && Pattern(pattern).IsMatch(text);

    // Words in every heading and paragraph, split on whitespace. Frontmatter and fenced code have no inline
    // content, so neither counts.
    public int Words() => _words ??= CountWords();

    private int? _words;

    private string Body => field ??= doc.Text[doc.BodyStart..];

    // The first of them, where a document repeats a heading.
    private string? SectionText(string title) =>
        doc.Sections.FirstOrDefault(s => string.Equals(s.Title, title, StringComparison.OrdinalIgnoreCase))
            is { } section
            ? doc.Text[section.BodyStart..section.BodyEnd]
            : null;

    private static readonly ConcurrentDictionary<string, Regex> Patterns = new();

    private static Regex Pattern(string pattern) =>
        Patterns.GetOrAdd(pattern, p => new Regex(p, RegexOptions.CultureInvariant));

    private int CountWords()
    {
        var total = 0;
        foreach (var leaf in doc.Ast.Descendants<LeafBlock>())
        {
            if (leaf.Inline is null) continue;
            total += Md.PlainText(leaf.Inline)
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        return total;
    }
}
