using System.Text;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using YamlDotNet.RepresentationModel;

namespace kac.core;

/// <summary>One part of a record that changed in substance, and the later text's line a finding points at.</summary>
public sealed record Change(string Part, int Line);

/// <summary>
/// Whether a record changed in substance between two revisions, read through the parse one section at a time.
/// <c>docs/use/cli/validate.md</c> states what a change in substance is.
/// </summary>
public static class Substance
{
    /// <summary>The frontmatter fields a record may move without changing in substance.</summary>
    private static readonly HashSet<string> Movable =
        new(["status", "superseded-by", "review-by", "accepts"], StringComparer.Ordinal);

    /// <summary>The most edits one word may take and still read as a typo fixed.</summary>
    private const int TypoEdits = 2;

    private const string Opening = "the title and opening";

    /// <summary>
    /// Empty where <paramref name="after"/> says what <paramref name="before"/> said. The frontmatter comes first, then
    /// the body.
    /// </summary>
    public static IReadOnlyList<Change> Changes(Doc before, Doc after)
    {
        var changes = new List<Change>();
        Frontmatter(before, after, changes);

        if (!Close(Words(before, before.BodyStart, FirstSection(before)), Words(after, after.BodyStart,
                FirstSection(after))))
            changes.Add(new Change(Opening, Math.Max(after.H1Line, 1)));

        var was = Keyed(before);
        var now = Keyed(after);

        foreach (var key in was.Keys.Where(k => !now.ContainsKey(k)))
            changes.Add(new Change($"`## {key.Title}` removed", Math.Max(after.H1Line, 1)));

        foreach (var (key, section) in now)
        {
            if (!was.TryGetValue(key, out var then))
            {
                changes.Add(new Change($"`## {section.Title}` added", section.Line));
                continue;
            }

            if (!Close(Words(before, then.BodyStart, then.BodyEnd), Words(after, section.BodyStart, section.BodyEnd)))
                changes.Add(new Change($"`## {section.Title}`", section.Line));
        }

        return changes;
    }

    /// <summary>One change per field, named in the order the later frontmatter writes it.</summary>
    private static void Frontmatter(Doc before, Doc after, List<Change> changes)
    {
        var line = Math.Max(after.FrontStartLine, 1);
        var was = Fields(before);
        var now = Fields(after);

        foreach (var key in now.Keys.Concat(was.Keys.Where(k => !now.ContainsKey(k))))
        {
            if (Movable.Contains(key)) continue;
            if (was.TryGetValue(key, out var a) && now.TryGetValue(key, out var b) && Same(a, b)) continue;

            changes.Add(new Change($"`{key}`", line));
        }
    }

    private static Dictionary<string, YamlNode> Fields(Doc d)
    {
        var fields = new Dictionary<string, YamlNode>(StringComparer.Ordinal);
        if (d.Front is null) return fields;

        foreach (var (key, value) in d.Front.Children)
            fields.TryAdd((key as YamlScalarNode)?.Value ?? "", value);
        return fields;
    }

    /// <summary>Compared by value, so a date quoted in one revision and bare in the other is the same date.</summary>
    private static bool Same(YamlNode a, YamlNode b) => (a, b) switch
    {
        (YamlScalarNode x, YamlScalarNode y) => x.Value == y.Value,
        (YamlSequenceNode x, YamlSequenceNode y) =>
            x.Children.Count == y.Children.Count && x.Children.Zip(y.Children).All(p => Same(p.First, p.Second)),
        (YamlMappingNode x, YamlMappingNode y) =>
            x.Children.Count == y.Children.Count
            && x.Children.All(kv => y.Children.TryGetValue(kv.Key, out var other) && Same(kv.Value, other)),
        _ => false
    };

    /// <summary>
    /// Each section under its title and how many earlier sections share it, so two sections titled alike pair in order.
    /// </summary>
    private static Dictionary<(string Title, int Nth), Section> Keyed(Doc d)
    {
        var keyed = new Dictionary<(string Title, int Nth), Section>();
        foreach (var section in d.Sections)
            keyed[(section.Title, keyed.Keys.Count(k => k.Title == section.Title))] = section;
        return keyed;
    }

    private static int FirstSection(Doc d) =>
        d.Ast.Descendants<HeadingBlock>().FirstOrDefault(h => h.Level == 2)?.Span.Start ?? d.Text.Length;

    /// <summary>
    /// Equal word for word, or the same count of words with each changed word a typo away from the one it replaced.
    /// </summary>
    private static bool Close(IReadOnlyList<string> before, IReadOnlyList<string> after) =>
        before.Count == after.Count
        && before.Zip(after).All(p => p.First == p.Second || Edits(p.First, p.Second) <= TypoEdits);

    /// <summary>
    /// The words between two offsets, whitespace folded, each link reduced to its text and each autolink to nothing.
    /// The identity line is left out, and <c>identity-status</c> checks it against <c>status</c>.
    /// </summary>
    private static List<string> Words(Doc d, int start, int end)
    {
        var cuts = Cuts(d).Where(c => c.End > start && c.Start < end).OrderBy(c => c.Start);
        var kept = new StringBuilder();
        var at = start;

        foreach (var (from, to) in cuts)
        {
            if (from > at) kept.Append(d.Text, at, from - at);
            at = Math.Max(at, to);
        }

        if (end > at) kept.Append(d.Text, at, end - at);

        // A table's pipes and delimiter row are layout, which an editor realigns around a longer cell.
        return
        [
            .. kept.ToString().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                .Where(word => word.Any(c => c is not ('|' or '-' or ':')))
        ];
    }

    /// <summary>The spans a reader never reads as prose: link markup, link definitions and the identity line.</summary>
    private static IEnumerable<(int Start, int End)> Cuts(Doc d)
    {
        foreach (var span in d.DefinitionSpans) yield return span;

        foreach (var autolink in d.Ast.Descendants<AutolinkInline>())
            yield return (autolink.Span.Start, autolink.Span.End + 1);

        foreach (var link in d.Ast.Descendants<LinkInline>())
        {
            if (link.IsAutoLink || link.FirstChild is not { } first || link.LastChild is not { } last)
            {
                yield return (link.Span.Start, link.Span.End + 1);
                continue;
            }

            yield return (link.Span.Start, first.Span.Start);
            yield return (last.Span.End + 1, link.Span.End + 1);
        }

        var blocks = d.Ast.ToList();
        var h1 = blocks.FindIndex(b => b is HeadingBlock { Level: 1 });
        if (h1 >= 0 && h1 + 1 < blocks.Count
                    && blocks[h1 + 1] is ParagraphBlock { Inline.FirstChild: CodeInline } identity)
            yield return (identity.Span.Start, identity.Span.End + 1);
    }

    /// <summary>Levenshtein distance, with a substitution, an insertion and a deletion each costing one.</summary>
    private static int Edits(string a, string b)
    {
        var row = Enumerable.Range(0, b.Length + 1).ToArray();
        for (var i = 1; i <= a.Length; i++)
        {
            var diagonal = row[0];
            row[0] = i;
            for (var j = 1; j <= b.Length; j++)
            {
                var above = row[j];
                row[j] = Math.Min(Math.Min(row[j] + 1, row[j - 1] + 1), diagonal + (a[i - 1] == b[j - 1] ? 0 : 1));
                diagonal = above;
            }
        }

        return row[b.Length];
    }
}
