using System.Text;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace kac.core;

public static class Md
{
    // The extensions every document depends on: the frontmatter block, the pipe tables a clause section
    // is written as, and task lists. A `[ ]` at the head of a list item is a checkbox, and the same
    // brackets mid-sentence are prose, so what they mean depends on where the block puts them. The
    // bracket scan in `Doc` reads inlines and cannot ask. Markdig can: with task lists on, the checkbox
    // parses as an inline of its own and breaks the run of literals the scan reads, while a `[x]` typed
    // mid-sentence still reaches it. A built pipeline is immutable, so it is built once here and shared
    // across every parse.
    private static readonly MarkdownPipeline Pipeline =
        new MarkdownPipelineBuilder().UseYamlFrontMatter().UsePipeTables().UseTaskLists().Build();

    // Every read of a document goes through here, so one file has one reading. Markdig's own default
    // leaves the frontmatter block unparsed. The `---` closing the block then reads as a setext heading
    // carrying the whole block as its text, and `fragment-resolves` accepts an anchor no renderer offers.
    internal static MarkdownDocument Parse(string text) => Markdown.Parse(text, Pipeline);

    public static string PlainText(ContainerInline? container)
    {
        if (container is null) return "";
        var sb = new StringBuilder();
        Walk(container, sb, code: true);
        return sb.ToString().Trim();
    }

    // For a check about the words of a sentence. A term in backticks is named, not used.
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

    // Read on the source, so a horizontal rule, a leftover bullet marker or an em dash counts as nothing written.
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

    // One anchor per heading, at every level.
    public static HashSet<string> Anchors(string markdown) => Anchors(Parse(markdown));

    // For a document already parsed, so one file has one reading.
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

    // The anchor a renderer derives from a heading: lower-cased, spaces hyphenated, everything that is
    // not a letter, digit, hyphen or underscore dropped.
    //
    // Renderers agree on that much and on nothing beyond it. Azure DevOps percent-encodes the
    // punctuation it meets into the anchor where GitHub discards it, so a heading carrying `/`, `:` or
    // `.` has two different anchors and one link cannot name both. This is the discarding form, which
    // is why a heading meant to be linked to is written without punctuation in the first place. See
    // `frameworks.md`, where the framework's version lives in the opening line rather than the heading.
    public static string Slug(string heading)
    {
        var sb = new StringBuilder();
        foreach (var ch in heading.ToLowerInvariant())
            if (char.IsLetterOrDigit(ch) || ch is '-' or '_') sb.Append(ch);
            else if (ch is ' ') sb.Append('-');

        return sb.ToString();
    }

    // The items of the first list beneath a heading. A later list under the same heading is not read.
    public static IEnumerable<(string Text, string Plain, IReadOnlyList<string> Bold, int Line)> Bullets(
        MarkdownDocument ast, int start, int end) =>
        FirstList(ast, start, end) is { } list ? Bullets(list) : [];

    // The same bullets as written, marker dropped and wrapped lines joined. Both walk `Leads`, so the two pair
    // item for item.
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

    // Only the outermost nested list is returned, so a list nested three deep is one finding.
    public static IEnumerable<int> NestedLists(MarkdownDocument ast, int start, int end) =>
        ast.Descendants<ListBlock>()
            .Where(l => l.Span.Start >= start && l.Span.End <= end)
            .Where(l => l.Parent is ListItemBlock { Parent: ListBlock { Parent: not ListItemBlock } })
            .Select(l => l.Line + 1);

    private static ListBlock? FirstList(MarkdownDocument ast, int start, int end) =>
        ast.Descendants<ListBlock>().FirstOrDefault(b => b.Span.Start >= start && b.Span.End <= end);

    // An item opening on no paragraph is no bullet.
    private static IEnumerable<(ListItemBlock Item, ParagraphBlock Paragraph, ContainerInline Inline)> Leads(
        ListBlock list)
    {
        foreach (var item in list.OfType<ListItemBlock>())
            if (item.OfType<ParagraphBlock>().FirstOrDefault() is { Inline: { } inline } paragraph)
                yield return (item, paragraph, inline);
    }

    // `Plain` is the item without its bold runs and code spans, so a capitalised modal left in it is unbolded.
    // Direct children only: a nested list is not a bullet of this list.
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

    // `code` says whether a code span contributes its content. A link's target never does.
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
