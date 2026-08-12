using System.Text;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

// ---------------------------------------------------------------------------
// Markdown helpers
// ---------------------------------------------------------------------------

namespace kac.core;

public static class Md
{
    public static string PlainText(ContainerInline? container)
    {
        if (container is null) return "";
        var sb = new StringBuilder();
        Walk(container, sb);
        return sb.ToString().Trim();
    }

    public static string PlainText(QuoteBlock quote)
    {
        var sb = new StringBuilder();
        foreach (var para in quote.Descendants<ParagraphBlock>())
            if (para.Inline is not null)
            {
                Walk(para.Inline, sb);
                sb.Append(' ');
            }

        return sb.ToString().Trim();
    }

    // Whether a stretch of a document says anything, which is a letter or a digit. Read on the source
    // as written rather than on the rendered blocks: a horizontal rule, a bullet marker left behind or
    // an em dash standing in for the words is a stretch nobody has written yet, and the AST would offer
    // all three as content.
    public static bool HasContent(ReadOnlySpan<char> text)
    {
        foreach (var c in text)
            if (char.IsLetterOrDigit(c))
                return true;
        return false;
    }

    // A fragment of a document quoted back in a finding, cut to what a terminal line can hold beside
    // the rest of the message.
    public static string Snippet(string s, int max = 60) => s.Length > max ? s[..(max - 3)] + "…" : s;

    // The anchors a document offers a link — one per heading, at every level.
    public static HashSet<string> Anchors(string markdown)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        foreach (var h in Markdown.Parse(markdown).Descendants<HeadingBlock>())
        {
            var slug = Slug(PlainText(h.Inline));
            if (slug.Length > 0) set.Add(slug);
        }

        return set;
    }

    // The anchor a renderer derives from a heading: lower-cased, spaces hyphenated, everything that is
    // not a letter, digit, hyphen or underscore dropped.
    //
    // Renderers agree on that much and on nothing beyond it. Azure DevOps percent-encodes the
    // punctuation it meets into the anchor where GitHub discards it, so a heading carrying `/`, `:` or
    // `.` has two different anchors and one link cannot name both. This is the discarding form, which
    // is why a heading meant to be linked to is written without punctuation in the first place — see
    // `frameworks.md`, where the framework's version lives in the opening line rather than the heading.
    public static string Slug(string heading)
    {
        var sb = new StringBuilder();
        foreach (var ch in heading.ToLowerInvariant())
            if (char.IsLetterOrDigit(ch) || ch is '-' or '_') sb.Append(ch);
            else if (ch is ' ') sb.Append('-');

        return sb.ToString();
    }

    private static void Walk(Inline inline, StringBuilder sb)
    {
        switch (inline)
        {
            case LiteralInline lit: sb.Append(lit.Content.ToString()); break;
            case CodeInline code: sb.Append(code.Content); break;
            case LineBreakInline: sb.Append(' '); break;
        }

        if (inline is ContainerInline c)
            foreach (var child in c)
                Walk(child, sb);
    }
}
