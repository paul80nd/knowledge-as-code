using System.Text;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace kac.core;

public static class Md
{
    /// <summary>
    /// Frontmatter, pipe tables and task lists. With task lists on, a <c>[ ]</c> heading a list item parses as a
    /// checkbox and breaks the literal run the bracket scan in <c>Doc</c> reads, while a <c>[x]</c> mid-sentence still
    /// reaches it. Built once and shared, because a built pipeline is immutable.
    /// </summary>
    private static readonly MarkdownPipeline Pipeline =
        new MarkdownPipelineBuilder().UseYamlFrontMatter().UsePipeTables().UseTaskLists().Build();

    /// <summary>
    /// Every read of a document goes through here, so one file has one reading. Markdig's own default leaves the
    /// frontmatter block unparsed. The <c>---</c> closing the block then reads as a setext heading carrying the whole
    /// block as its text, and <c>fragment-resolves</c> accepts an anchor no renderer offers.
    /// </summary>
    internal static MarkdownDocument Parse(string text) => Markdown.Parse(text, Pipeline);

    public static string PlainText(ContainerInline? container)
    {
        if (container is null) return "";
        var sb = new StringBuilder();
        Walk(container, sb, code: true);
        return sb.ToString().Trim();
    }

    /// <summary>For a check about the words of a sentence. A term in backticks is named, not used.</summary>
    public static string PlainTextWithoutCode(ContainerInline? container)
    {
        if (container is null) return "";
        var sb = new StringBuilder();
        Walk(container, sb, code: false);
        return sb.ToString().Trim();
    }

    public static string PlainText(QuoteBlock quote)
    {
        var sb = new StringBuilder();
        foreach (var para in quote.Descendants<ParagraphBlock>())
            if (para.Inline is not null)
            {
                Walk(para.Inline, sb, code: true);
                sb.Append(' ');
            }

        return sb.ToString().Trim();
    }

    /// <summary>
    /// Read on the source, so a horizontal rule, a leftover bullet marker or an em dash counts as nothing written.
    /// </summary>
    public static bool HasContent(ReadOnlySpan<char> text)
    {
        foreach (var c in text)
            if (char.IsLetterOrDigit(c))
                return true;
        return false;
    }

    public static string Snippet(string s, int max = 60) => s.Length > max ? s[..(max - 3)] + "…" : s;

    public static IEnumerable<(int Level, string Text)> Headings(string markdown) =>
        Parse(markdown).Descendants<HeadingBlock>()
            .Select(h => (h.Level, PlainText(h.Inline)));

    /// <summary>One anchor per heading, at every level.</summary>
    public static HashSet<string> Anchors(string markdown) => Anchors(Parse(markdown));

    /// <summary>For a document already parsed, so one file has one reading.</summary>
    public static HashSet<string> Anchors(MarkdownDocument ast)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        foreach (var h in ast.Descendants<HeadingBlock>())
        {
            var slug = Slug(PlainText(h.Inline));
            if (slug.Length > 0) set.Add(slug);
        }

        return set;
    }

    /// <summary>
    /// The anchor a renderer derives from a heading: lower-cased, spaces hyphenated, all but letters, digits, <c>-</c>
    /// and <c>_</c> dropped. Azure DevOps percent-encodes the punctuation GitHub drops, so a heading meant to be linked
    /// to carries none, which is why <c>frameworks.md</c> keeps its version in the opening line.
    /// </summary>
    public static string Slug(string heading)
    {
        var sb = new StringBuilder();
        foreach (var ch in heading.ToLowerInvariant())
            if (char.IsLetterOrDigit(ch) || ch is '-' or '_') sb.Append(ch);
            else if (ch is ' ') sb.Append('-');

        return sb.ToString();
    }

    /// <summary>
    /// The items of the first list beneath a heading. A later list under the same heading is not read.
    /// </summary>
    public static IEnumerable<(string Text, string Plain, IReadOnlyList<string> Bold, int Line)> Bullets(
        MarkdownDocument ast, int start, int end) =>
        FirstList(ast, start, end) is { } list ? Bullets(list) : [];

    /// <summary>
    /// The same bullets as written, marker dropped and wrapped lines joined. Both walk <c>Leads</c>, so the two pair
    /// item for item.
    /// </summary>
    public static IEnumerable<(string Markdown, IReadOnlyList<string> Bold)> BulletSources(
        MarkdownDocument ast, string text, int start, int end)
    {
        if (FirstList(ast, start, end) is not { } list) return [];

        return Leads(list).Zip(Bullets(list), (lead, bullet) =>
        {
            var source = text.Substring(lead.Paragraph.Span.Start, lead.Paragraph.Span.Length);
            var lines = source.Replace("\r\n", "\n").Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0);
            return (string.Join(' ', lines), bullet.Bold);
        });
    }

    /// <summary>Only the outermost nested list is returned, so a list nested three deep is one finding.</summary>
    public static IEnumerable<int> NestedLists(MarkdownDocument ast, int start, int end) =>
        ast.Descendants<ListBlock>()
            .Where(l => l.Span.Start >= start && l.Span.End <= end)
            .Where(l => l.Parent is ListItemBlock { Parent: ListBlock { Parent: not ListItemBlock } })
            .Select(l => l.Line + 1);

    private static ListBlock? FirstList(MarkdownDocument ast, int start, int end) =>
        ast.Descendants<ListBlock>().FirstOrDefault(b => b.Span.Start >= start && b.Span.End <= end);

    /// <summary>An item opening on no paragraph is no bullet.</summary>
    private static IEnumerable<(ListItemBlock Item, ParagraphBlock Paragraph, ContainerInline Inline)> Leads(
        ListBlock list)
    {
        foreach (var item in list.OfType<ListItemBlock>())
            if (item.OfType<ParagraphBlock>().FirstOrDefault() is { Inline: { } inline } paragraph)
                yield return (item, paragraph, inline);
    }

    /// <summary>
    /// <c>Plain</c> is the item without its bold runs and code spans, so a capitalised modal left in it is unbolded.
    /// Direct children only: a nested list is not a bullet of this list.
    /// </summary>
    public static IEnumerable<(string Text, string Plain, IReadOnlyList<string> Bold, int Line)> Bullets(
        ListBlock list)
    {
        foreach (var (item, _, inline) in Leads(list))
        {
            var text = new StringBuilder();
            var plain = new StringBuilder();
            var bold = new List<string>();

            foreach (var child in inline)
            {
                var from = text.Length;
                Walk(child, text, code: true);

                if (child is EmphasisInline { DelimiterCount: 2 } run) bold.Add(PlainText(run));
                else if (child is not CodeInline) plain.Append(text, from, text.Length - from);
            }

            yield return (text.ToString().Trim(), plain.ToString(), bold, item.Line + 1);
        }
    }

    /// <summary><c>code</c> says whether a code span contributes its content. A link's target never does.</summary>
    private static void Walk(Inline inline, StringBuilder sb, bool code)
    {
        switch (inline)
        {
            case LiteralInline lit: sb.Append(lit.Content.ToString()); break;
            case CodeInline span when code: sb.Append(span.Content); break;
            case LineBreakInline: sb.Append(' '); break;
        }

        if (inline is ContainerInline c)
            foreach (var child in c)
                Walk(child, sb, code);
    }
}
