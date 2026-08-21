using System.Text.RegularExpressions;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using YamlDotNet.RepresentationModel;

namespace kac.core;

// `{{…}}` is what a template marks as the author's to supply — an id, a value, a filename, a phrase.
// One mark rather than a vocabulary of stand-in words, because the tool has to recognise exactly what
// the templates teach. A corpus reading `NNNN`, `XXXX` and `example` as pretend too would have three
// more ways to write a placeholder than it documents, and `example` is a slug a real document may want.
//
// The mark is only meaningful in a template. A record carrying one is an unfinished copy rather than a
// document with a placeholder in it, so nothing here is asked of a record.
public static partial class Placeholder
{
    private const string Mark = "{{";

    public static bool In(string? value) => value?.Contains(Mark, StringComparison.Ordinal) ?? false;

    // Every placeholder a record still carries, in document order — the frontmatter, the identity line,
    // the prose and the link targets.
    //
    // What is deliberately not read is code: a fenced block and a code span are where a document quotes
    // a templating language it is describing, and `${{ … }}` in a pipeline snippet is the example, not
    // an oversight. Both fall out of reading the parsed inlines rather than the source — neither a
    // fenced block nor a code span carries a LiteralInline. The identity line is the exception, read
    // through its code spans on purpose: it is a structured field that happens to be written in
    // backticks, and the id it repeats is the likeliest thing to be left unfilled.
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

    // Every scalar the mapping holds at any depth, values only — a key carrying the mark is a key the
    // schema does not declare, and unknown-key has already said so in its own words.
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

    // A whole placeholder, for quoting back at whoever left one in. Tolerant of an unclosed mark, which
    // is still a placeholder to a reader and still wants reporting.
    [GeneratedRegex(@"\{\{[^}\n]*\}?\}?")]
    private static partial Regex TokenRegex();
}
