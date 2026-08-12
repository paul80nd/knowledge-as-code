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

// One row of a clause table, held as the parser found it rather than as it should be: an id cell that
// is not a single code span keeps its text and reports no span, a clause cell that opens with no bold
// run reports none. Every judgement about what is wrong belongs to the validator, which has the schema
// to judge against and the words to say what was expected.
public class ClauseRow
{
    public string? IdSpan;    // the Id cell's content when it is exactly one code span
    public string IdText = "";  // the Id cell flattened, for quoting back when it is not
    public string Text = "";    // the Clause cell flattened
    public string? BoldLead;    // the leading bold run's text, when the cell opens with one
    public int Line;
}

// What is being read. A record is a document of the corpus; a template is the file every record of its
// type is copied from. The two are held to different questions — a template has no id of its own, no
// filename to agree with and no values filled in — and this is what the checks branch on, so that
// "which of these applies to a template" is answered once per check, where the check is written.
public enum DocKind
{
    Record,
    Template
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

    // Where the body begins: the first character after the frontmatter block, or 0 where there is none.
    // Held so a check can read the document as it was written — code fences and link targets included —
    // rather than as the AST renders it. A credential is most likely to be in a fenced block, which is
    // exactly what the flattened text drops.
    public int BodyStart;

    public string? H1;
    public int H1Line;

    // The identity line — the paragraph directly beneath the H1, naming the document's type, id and
    // status as code spans: "`Policy: pol-A11Y` `DRAFT`". Held as the raw span contents, in order, so
    // the validator owns every judgement about their shape and can word each mismatch itself. Null
    // when no paragraph beneath the H1 opens with a code span, which is the line being absent
    // altogether; an empty list never occurs.
    public List<string>? IdentitySpans;
    public int IdentityLine;

    // The clause table beneath the section the type's schema names. `ClauseHeaders` is null where that
    // section holds no table at all — the section being absent and the section holding prose are
    // different faults, and only the parser can tell them apart. `ClauseRefs` collects every code span
    // shaped like a citation of one, from anywhere in the document.
    public List<string>? ClauseHeaders;
    public readonly List<ClauseRow> Clauses = [];
    public int ClauseSectionLine;
    public int ClauseTableLine;
    public readonly List<(string Ref, int Line)> ClauseRefs = [];

    public readonly List<string> H2 = [];
    public readonly List<LinkRef> Links = [];
    public readonly List<string> DefinedLabels = [];
    public readonly HashSet<string> UsedLabels = new(StringComparer.OrdinalIgnoreCase);
    public readonly List<(string inner, int line)> BareBracketTokens = [];
    public QuoteBlock? YStatement;

    // The links written under each section a field of this type mirrors, keyed by the section name the
    // field declared — keyed rather than flattened, because two fields on a type may mirror two
    // different sections. A section this document does not carry arrives as an empty list, so the
    // reconciliation reports every id in the field rather than passing in silence.
    public IReadOnlyDictionary<string, List<LinkRef>> MirroredSectionLinks =
        new Dictionary<string, List<LinkRef>>(StringComparer.OrdinalIgnoreCase);

    // The two extensions every record depends on: the frontmatter block, and the pipe tables a clause
    // section is written as. A built pipeline is immutable, so one is shared across every parse rather
    // than assembled per document.
    private static readonly MarkdownPipeline Pipeline =
        new MarkdownPipelineBuilder().UseYamlFrontMatter().UsePipeTables().Build();

    // `requireFrontmatter: false` is for a type page of a collection type. It carries no frontmatter
    // and is not a record, but it holds links and generated blocks that are worth checking, so it is
    // parsed for its prose alone.
    public static Doc? Parse(string rel, string text, Schema schema, bool requireFrontmatter = true)
    {
        var ast = Markdown.Parse(text, Pipeline);

        var fmBlock = ast.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
        if (fmBlock is null && requireFrontmatter) return null; // not migrated — caller counts and skips

        var top = rel.Split('/')[0];
        schema.ByFolder.TryGetValue(top, out var type);

        var doc = new Doc { Rel = rel, Folder = top, Type = type, Text = text, Ast = ast };

        // Frontmatter: strip the --- fences and parse with the representation model so
        // key order and scalar quoting survive. A page parsed without it keeps `Front` null and is
        // only ever asked about its prose, so the rest of the parse still runs.
        if (fmBlock is not null)
        {
            var yamlText = StripFences(text, fmBlock);
            doc.FrontStartLine = fmBlock.Line + 1;
            doc.BodyStart = Math.Min(text.Length, fmBlock.Span.End + 1);
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
        }

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

        // Identity line — the block immediately after the H1, when it is a paragraph opening with a
        // code span. Anchoring on "the next block" rather than "the first paragraph of code spans
        // anywhere" is what lets the validator say the line is missing: a document that puts its
        // Y-statement or its prose first has no identity line, however its later paragraphs read.
        ReadIdentity(ast, doc);

        // Clause table, and the citations of one anywhere in the document.
        if (type?.Clauses is { } clauseSpec) ReadClauses(ast, doc, clauseSpec.Section);
        foreach (var code in ast.Descendants<CodeInline>())
            if (code.Content is { } content && ClauseCitation.IsMatch(content))
                doc.ClauseRefs.Add((content, code.Line + 1));

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

        doc.MirroredSectionLinks = SectionLinks(ast, type?.DeclaredFields.Select(spec => spec.MirrorsSection));

        return doc;
    }

    public string? FrontScalar(string key) => Front is null
        ? null
        : (from kv in Front.Children
            where ((YamlScalarNode)kv.Key).Value == key
            select (kv.Value as YamlScalarNode)?.Value).FirstOrDefault();

    // Walk the top-level blocks to the H1, then look at the one after it. A paragraph whose first
    // inline is a code span is taken as an attempted identity line and its code spans are collected —
    // taken as attempted, rather than as correct, so a line with the wrong number of spans is reported
    // as a malformed identity line rather than as no line at all. Only the code spans are kept; any
    // stray prose between them is invisible here and caught by the span count.
    private static void ReadIdentity(MarkdownDocument ast, Doc doc)
    {
        var blocks = ast.ToList();
        var i = blocks.FindIndex(b => b is HeadingBlock { Level: 1 });
        if (i < 0 || i + 1 >= blocks.Count) return;

        if (blocks[i + 1] is not ParagraphBlock { Inline: { FirstChild: CodeInline } inline } p) return;

        doc.IdentitySpans = [.. inline.Descendants<CodeInline>().Select(c => c.Content ?? "")];
        doc.IdentityLine = p.Line + 1;
    }

    // A code span shaped like a citation of a clause — an id, a dot, and a clause id, as `pol-VURM.TIMEBOX`.
    // Deliberately loose on case and width: a mis-cased or over-long citation is one the validator should
    // report as unresolved rather than one the parser should quietly decline to see.
    private static readonly System.Text.RegularExpressions.Regex ClauseCitation =
        new(@"^[a-z]{2,4}-[A-Za-z0-9]+\.[A-Za-z0-9]+$");

    // The clause table: the first table under the H2 the schema names, read down to the next H2. Rows are
    // taken whole and unjudged — the header row supplies `ClauseHeaders`, every other row a `ClauseRow`,
    // however malformed — so that "no table here" is the only thing the parser decides.
    private static void ReadClauses(MarkdownDocument ast, Doc doc, string section)
    {
        var inSection = false;
        foreach (var block in ast)
        {
            if (block is HeadingBlock h)
            {
                if (h.Level > 2) continue; // a sub-heading inside the section does not end it
                inSection = string.Equals(Md.PlainText(h.Inline), section, StringComparison.OrdinalIgnoreCase);
                if (inSection) doc.ClauseSectionLine = h.Line + 1;
                continue;
            }

            if (!inSection || block is not Markdig.Extensions.Tables.Table table) continue;

            doc.ClauseTableLine = table.Line + 1;
            foreach (var row in table.OfType<Markdig.Extensions.Tables.TableRow>())
            {
                var cells = row.OfType<Markdig.Extensions.Tables.TableCell>()
                    .Select(c => (c.FirstOrDefault() as LeafBlock)?.Inline)
                    .ToList();

                if (row.IsHeader)
                {
                    doc.ClauseHeaders = [.. cells.Select(c => Md.PlainText(c))];
                    continue;
                }

                var clause = cells.Count > 1 ? cells[1] : null;
                doc.Clauses.Add(new ClauseRow
                {
                    IdSpan = cells.Count > 0 && cells[0]?.FirstChild is CodeInline { NextSibling: null } code
                        ? code.Content
                        : null,
                    IdText = Md.PlainText(cells.Count > 0 ? cells[0] : null),
                    Text = Md.PlainText(clause),
                    BoldLead = clause?.FirstChild is EmphasisInline { DelimiterCount: 2 } bold
                        ? Md.PlainText(bold)
                        : null,
                    Line = row.Line + 1
                });
            }

            doc.ClauseHeaders ??= [];
            return;
        }
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

    // The links under each of the named H2 sections, gathered in one walk however many are asked for.
    // A section title matches case-insensitively, an H1 ends whichever section is open, and a heading
    // deeper than H2 leaves it open — a sub-heading is inside its section, not after it.
    private static Dictionary<string, List<LinkRef>> SectionLinks(
        MarkdownDocument ast, IEnumerable<string?>? sectionTitles)
    {
        var result = (sectionTitles ?? [])
            .OfType<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToDictionary(title => title, _ => new List<LinkRef>(), StringComparer.OrdinalIgnoreCase);
        if (result.Count == 0) return result;

        List<LinkRef>? section = null;
        foreach (var block in ast)
        {
            if (block is HeadingBlock h)
            {
                section = h.Level switch
                {
                    2 => result.GetValueOrDefault(Md.PlainText(h.Inline)),
                    < 2 => null,
                    _ => section
                };
                continue;
            }

            section?.AddRange(from leaf in Leaves(block)
                where leaf.Inline is not null
                from link in leaf.Inline!.Descendants<LinkInline>()
                where !link.IsImage
                select new LinkRef
                    { Target = link.Url ?? "", Line = link.Line + 1, Label = link.Reference?.Label ?? link.Label });
        }

        return result;
    }

    // Every leaf block one block holds, itself included when it is one. Inlines hang off leaves and
    // nowhere else, which is what makes this the way to reach a link written as prose: descending from
    // the block itself yields the links in a list beneath it and none of the links in a paragraph, and
    // to whoever wrote the section those are the same link.
    private static IEnumerable<LeafBlock> Leaves(Block block) =>
        block is LeafBlock leaf ? [leaf] : ((ContainerBlock)block).Descendants<LeafBlock>();
}
