using System.Text;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using YamlDotNet.RepresentationModel;

namespace kac.core;

// ---------------------------------------------------------------------------
// Parsed document
// ---------------------------------------------------------------------------

public class LinkRef
{
    public string Target = ""; // raw url/target after the label resolves
    public int Line;
    public bool IsReference; // reference or shortcut link (has a label/definition)
    public string? Label;
}

public class Doc
{
    public required string Rel;
    public required string Folder;
    public required TypeSchema? Type;
    public required string Text;
    public required MarkdownDocument Ast;

    public YamlMappingNode? Front; // frontmatter mapping (representation model)
    public readonly List<string> FrontKeys = [];
    public int FrontStartLine; // 1-based line where the frontmatter block begins

    public string? H1;
    public int H1Line;
    public readonly List<string> H2 = [];
    public readonly List<LinkRef> Links = [];
    public readonly List<string> DefinedLabels = [];
    public readonly HashSet<string> UsedLabels = new(StringComparer.OrdinalIgnoreCase);
    public readonly List<(string inner, int line)> BareBracketTokens = [];
    public List<LinkRef> RelatedSectionLinks = [];
    public QuoteBlock? YStatement;

    public static Doc? Parse(string rel, string text, Schema schema)
    {
        var pipeline = new MarkdownPipelineBuilder().UseYamlFrontMatter().UsePipeTables().Build();
        var ast = Markdown.Parse(text, pipeline);

        var fmBlock = ast.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
        if (fmBlock is null) return null; // not migrated — caller counts and skips

        var top = rel.Split('/')[0];
        schema.ByFolder.TryGetValue(top, out var type);

        var doc = new Doc { Rel = rel, Folder = top, Type = type, Text = text, Ast = ast };

        // Frontmatter: strip the --- fences and parse with the representation model so
        // key order and scalar quoting survive.
        var yamlText = StripFences(text, fmBlock);
        doc.FrontStartLine = fmBlock.Line + 1;
        try
        {
            var stream = new YamlStream();
            stream.Load(new StringReader(yamlText));
            if (stream.Documents.Count > 0 && stream.Documents[0].RootNode is YamlMappingNode map)
            {
                doc.Front = map;
                foreach (var kv in map.Children)
                    doc.FrontKeys.Add(((YamlScalarNode)kv.Key).Value ?? "");
            }
        }
        catch
        {
            doc.Front = null;
        } // signalled as a parse error downstream

        // Headings.
        foreach (var h in ast.Descendants<HeadingBlock>())
        {
            var txt = Md.PlainText(h.Inline);
            switch (h.Level)
            {
                case 1 when doc.H1 is null:
                    doc.H1 = txt;
                    doc.H1Line = h.Line + 1;
                    break;
                case 2:
                    doc.H2.Add(txt);
                    break;
            }
        }

        // Links (inline + resolved reference/shortcut). Iterating LinkInline naturally
        // excludes code — code spans and fenced/indented blocks carry no LinkInline.
        foreach (var link in ast.Descendants<LinkInline>())
        {
            if (link.IsImage) continue;
            doc.Links.Add(new LinkRef
            {
                Target = link.Url ?? "",
                Line = link.Line + 1,
                IsReference = link.Reference is not null,
                Label = link.Reference?.Label ?? link.Label
            });
            if (link.Reference is not null) doc.UsedLabels.Add(link.Reference.Label ?? "");
        }

        // Link reference definitions.
        foreach (var def in ast.Descendants<LinkReferenceDefinition>())
            if (def.Label is not null)
                doc.DefinedLabels.Add(def.Label);

        // Bare [bracket] tokens left in prose (an undefined shortcut ref like [ADR-0099] renders as
        // literal text). Markdig emits a failed link opener '[' as its own literal inline and leaves
        // the ']' in the following sibling, so the brackets never survive in one literal — the run
        // of consecutive literal siblings must be rejoined before scanning. A real link or code span
        // is a non-literal inline and so naturally breaks the run, which is what keeps code and
        // resolved links excluded. Driven from each leaf block's root inline, because Descendants
        // <ContainerInline> yields only nested containers, never the root that holds the run.
        foreach (var leaf in ast.Descendants<LeafBlock>())
            if (leaf.Inline is not null)
                ScanContainer(leaf.Inline, doc.BareBracketTokens);

        // Y-statement — first block-quote after the H1.
        doc.YStatement = ast.Descendants<QuoteBlock>().FirstOrDefault(q => q.Line > (doc.H1Line - 1));

        // Related section links.
        doc.RelatedSectionLinks = SectionLinks(ast, "Related");

        return doc;
    }

    public string? FrontScalar(string key) => Front is null
        ? null
        : (from kv in Front.Children
            where ((YamlScalarNode)kv.Key).Value == key
            select (kv.Value as YamlScalarNode)?.Value).FirstOrDefault();

    public string TitleText()
    {
        if (Type?.H1Pattern is null || H1 is null) return H1 ?? "";
        var m = System.Text.RegularExpressions.Regex.Match(H1, Type.H1Pattern);
        return m is { Success: true, Groups.Count: > 2 } ? m.Groups[2].Value : H1;
    }

    private static string StripFences(string text, YamlFrontMatterBlock block)
    {
        // block.Span covers the whole frontmatter incl. the --- fences; drop the first
        // and last fence lines and keep the YAML body.
        var slice = text.Substring(block.Span.Start, block.Span.Length);
        var lines = slice.Replace("\r\n", "\n").Split('\n').ToList();
        if (lines.Count > 0 && lines[0].TrimEnd() == "---") lines.RemoveAt(0);
        if (lines.Count > 0 && lines[^1].TrimEnd() == "---") lines.RemoveAt(lines.Count - 1);
        return string.Join("\n", lines);
    }

    // Rejoin the run of consecutive literal children of a container (breaking on any non-literal
    // inline) and scan each run for bare brackets, then recurse into nested containers (emphasis, a
    // link label). Only direct children are joined at each level, so a literal is scanned once.
    private static void ScanContainer(ContainerInline container, List<(string, int)> outp)
    {
        var run = new StringBuilder();
        var segments = new List<(int offset, int line)>();

        for (var child = container.FirstChild; child is not null; child = child.NextSibling)
            if (child is LiteralInline lit)
            {
                segments.Add((run.Length, lit.Line + 1));
                run.Append(lit.Content.ToString());
            }
            else
            {
                Flush();
                if (child is ContainerInline nested) ScanContainer(nested, outp);
            }

        Flush();
        return;

        void Flush()
        {
            if (run.Length > 0) ScanBrackets(run.ToString(), segments, outp);
            run.Clear();
            segments.Clear();
        }
    }

    private static void ScanBrackets(string s, List<(int offset, int line)> segments, List<(string, int)> outp)
    {
        for (var i = 0; i < s.Length; i++)
        {
            if (s[i] != '[') continue;
            var close = s.IndexOf(']', i + 1);
            if (close < 0) break;
            var inner = s.Substring(i + 1, close - i - 1);
            // A real inline/collapsed/full link would not survive as a literal, so a
            // literal '[x]' is a candidate undefined reference. Skip if immediately
            // followed by '(' or '[' (still part of a link the AST handled elsewhere).
            var next = close + 1 < s.Length ? s[close + 1] : ' ';
            // Map the line from the closing bracket: Markdig gives a failed link-opener '[' its own
            // literal with no source line (0), while the ']' always sits in a real-text literal that
            // carries the correct line.
            if (inner.Length > 0 && inner.IndexOf(']') < 0 && next != '(' && next != '[' && next != ':')
                outp.Add((inner, LineAt(segments, close)));
            i = close;
        }
    }

    // The source line of the literal segment that character `index` of a rejoined run falls in.
    private static int LineAt(List<(int offset, int line)> segments, int index)
    {
        var line = segments.Count > 0 ? segments[0].line : 1;
        foreach (var (offset, l) in segments)
        {
            if (offset > index) break;
            line = l;
        }

        return line;
    }

    private static List<LinkRef> SectionLinks(MarkdownDocument ast, string sectionTitle)
    {
        var result = new List<LinkRef>();
        var inSection = false;
        foreach (var block in ast)
        {
            if (block is HeadingBlock h)
            {
                inSection = h.Level switch
                {
                    2 => string.Equals(Md.PlainText(h.Inline), sectionTitle, StringComparison.OrdinalIgnoreCase),
                    < 2 => false,
                    _ => inSection
                };
            }

            if (inSection && block is not HeadingBlock)
                result.AddRange(from link in block.Descendants<LinkInline>()
                    where !link.IsImage
                    select new LinkRef
                        { Target = link.Url ?? "", Line = link.Line + 1, Label = link.Reference?.Label ?? link.Label });
        }

        return result;
    }
}
