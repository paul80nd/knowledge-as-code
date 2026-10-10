using System.Text.RegularExpressions;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using YamlDotNet.RepresentationModel;

namespace kac.core;

// `{{…}}` is what a template marks as the author's to supply: an id, a value, a filename, a phrase. It is the only
// placeholder the tool recognises. A record carrying one is an unfinished copy.
public static partial class Placeholder
{
    private const string Mark = "{{";

    public static bool In(string? value) => value?.Contains(Mark, StringComparison.Ordinal) ?? false;

    // Every placeholder a record still carries, in document order: the frontmatter, the identity line, the prose and
    // the link targets. Code is not read, because a fenced block or code span may quote a templating language such as
    // `${{ … }}`. The identity line is the exception, read through its code spans on purpose.
    public static IEnumerable<(string Token, int Line)> Occurrences(Doc d)
    {
        if (d.Front is not null)
            foreach (var found in Scalars(d.Front).SelectMany(n => From(n.Value, NodeLine(n, d))))
                yield return found;

        foreach (var found in (d.IdentitySpans ?? []).SelectMany(s => From(s, d.IdentityLine)))
            yield return found;

        foreach (var found in d.Ast.Descendants<LiteralInline>()
                     .SelectMany(lit => From(lit.Content.ToString(), lit.Line + 1)))
            yield return found;

        foreach (var found in d.Links.SelectMany(link => From(link.Target, link.Line)))
            yield return found;
    }

    private static IEnumerable<(string, int)> From(string? text, int line) =>
        text is null ? [] : TokenRegex().Matches(text).Select(m => (m.Value, line));

    // Every scalar the mapping holds at any depth, values only. A key carrying the mark is already reported by
    // `unknown-key`.
    private static IEnumerable<YamlScalarNode> Scalars(YamlNode node) =>
        node switch
        {
            YamlScalarNode scalar => [scalar],
            YamlSequenceNode seq => seq.Children.SelectMany(Scalars),
            YamlMappingNode map => map.Children.Values.SelectMany(Scalars),
            _ => []
        };

    private static int NodeLine(YamlNode node, Doc d)
        => node.Start.Line > 0 ? (int)node.Start.Line + d.FrontStartLine - 1 : d.FrontStartLine;

    // A whole placeholder, for quoting back. Tolerant of an unclosed mark.
    [GeneratedRegex(@"\{\{[^}\n]*\}?\}?")]
    private static partial Regex TokenRegex();
}
