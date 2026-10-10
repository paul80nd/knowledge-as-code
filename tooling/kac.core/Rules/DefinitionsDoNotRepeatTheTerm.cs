using System.Text.RegularExpressions;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace kac.core;

public sealed partial class DefinitionsDoNotRepeatTheTerm : IDocumentRule
{
    public RuleId RuleId => new("definitions-do-not-repeat-the-term");

    private static readonly CheckId Reports = new("definition-circular");

    public IReadOnlyList<CheckId> Emits => [Reports];

    public void Check(RuleContext ctx)
    {
        var asides = ctx.Type.Parts?.Asides ?? [];

        foreach (var part in ctx.Doc.Parts)
        {
            if (Definition(ctx.Doc, part, asides) is not { Length: > 0 } definition) continue;

            var sentence = FirstSentence(definition);
            if (!Repeats(part.Text, sentence)) continue;

            ctx.Report.Warn(Reports,
                $"the definition of '{Md.Snippet(part.Text)}' repeats the term: \"{Md.Snippet(sentence)}\". "
                + "Define it in words a reader already has, and link the entry those words belong to.",
                part.Line);
        }
    }

    public static string FirstSentence(string text) =>
        SentenceRegex().Match(text) is { Success: true } m ? m.Groups[1].Value : text;

    // Whether the term appears in the sentence as a whole word, without regard to casing. A boundary
    // is asked for only at an end the term spells with a word character, or a term such as `C#` could
    // never match: `\b` after `#` sits between two non-word characters and fails.
    private static bool Repeats(string term, string sentence)
    {
        if (term.Length == 0) return false;

        return Regex.IsMatch(sentence, $"{Boundary(term[0])}{Regex.Escape(term)}{Boundary(term[^1])}",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        static string Boundary(char c) => char.IsLetterOrDigit(c) || c == '_' ? @"\b" : "";
    }

    private static string? Definition(Doc doc, PartRow part, IReadOnlyList<string> asides)
    {
        foreach (var para in doc.Ast.Descendants<ParagraphBlock>())
        {
            if (!part.Contains(para.Span)) continue;
            if (para.Inline is not { } inline || IsAside(inline, asides)) continue;

            return Md.PlainTextWithoutCode(inline);
        }

        return null;
    }

    private static bool IsAside(ContainerInline inline, IReadOnlyList<string> asides) =>
        inline.FirstChild is EmphasisInline { DelimiterCount: 2 } bold
        && asides.Any(a => string.Equals(Md.PlainText(bold), $"{a}:", StringComparison.Ordinal));

    [GeneratedRegex(@"^(.*?[.!?])(?:\s|$)", RegexOptions.Singleline)]
    private static partial Regex SentenceRegex();
}
