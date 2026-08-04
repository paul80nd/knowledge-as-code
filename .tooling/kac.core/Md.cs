using System.Text;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

// ---------------------------------------------------------------------------
// Markdown helpers
// ---------------------------------------------------------------------------

public static class Md
{
    public static string PlainText(ContainerInline? container)
    {
        if (container is null) return "";
        var sb = new StringBuilder();
        Walk(container, sb);
        return sb.ToString().Trim();
    }

    public static string PlainText(LeafBlock? block)
        => block?.Inline is null ? "" : PlainText(block.Inline);

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
