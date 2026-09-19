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

    // Whether a stretch of a document says anything: a letter or a digit somewhere in it. Read on the
    // source as written, so that a horizontal rule, a bullet marker left behind or an em dash standing
    // in for the words counts as nothing written. The rendered blocks would offer all three as
    // content.
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

    // Every heading in a document, in the order it is written, with the level it is written at.
    //
    // Beside `Anchors`, which answers what a link may reach. This answers how the headings nest, which
    // is what a reader takes from a page grouping its subject under `##` and naming each one under
    // `###`.
    public static IEnumerable<(int Level, string Text)> Headings(string markdown) =>
        Parse(markdown).Descendants<HeadingBlock>()
            .Select(h => (h.Level, PlainText(h.Inline)));

    // The anchors a document offers a link: one per heading, at every level.
    public static HashSet<string> Anchors(string markdown) => Anchors(Parse(markdown));

    // Beside the overload above, for a document already parsed. A check holding a `Doc` has its AST,
    // and parsing the text again would make two readings of one file.
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

    // The bullets one heading gathers: the items of the first list beneath it, read off the document's
    // own parse. A bullet wrapped over three lines arrives as one item, and a bullet inside a fenced
    // block is no bullet at all, because the parse decides both and a line scan decides neither.
    //
    // `Plain` is the item with its bold runs and its code spans left out. A modal surviving into it was
    // written in capitals and left unbolded, which under BCP 14 is a keyword and under this corpus's
    // grammar is nothing: bold is what binds. A modal inside backticks is being named rather than used.
    //
    // Direct children throughout: the first list's own items, and each item's own first paragraph. A
    // nested list is one bullet's workings, and asking each of its points for a modal would hold a
    // rule's detail to the shape of a rule.
    public static IEnumerable<(string Text, string Plain, IReadOnlyList<string> Bold, int Line)> Bullets(
        MarkdownDocument ast, int start, int end)
    {
        var list = ast.Descendants<ListBlock>().FirstOrDefault(b => b.Span.Start >= start && b.Span.End <= end);
        if (list is null) yield break;

        foreach (var item in list.OfType<ListItemBlock>())
        {
            if (item.OfType<ParagraphBlock>().FirstOrDefault()?.Inline is not { } inline) continue;

            var text = new StringBuilder();
            var plain = new StringBuilder();
            var bold = new List<string>();

            foreach (var child in inline)
            {
                var from = text.Length;
                Walk(child, text);

                if (child is EmphasisInline { DelimiterCount: 2 } run) bold.Add(PlainText(run));
                else if (child is not CodeInline) plain.Append(text, from, text.Length - from);
            }

            yield return (text.ToString().Trim(), plain.ToString(), bold, item.Line + 1);
        }
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
