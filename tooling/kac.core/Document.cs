using System.Text;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using YamlDotNet.RepresentationModel;

namespace kac.core;

// Parsed document

public class LinkRef
{
    public string Target = ""; // raw url/target after the label resolves
    public int Line;
    public bool IsReference; // reference or shortcut link (has a label/definition)
    public string? Label;

    // Where the link sits in the document's text. A line is what a finding quotes, and this is what
    // answers which part of the record a link stands inside: a part's body is held as a span, and a
    // line number would have to be converted back into one to compare them.
    public int Position;
}

// One part of a record, held as the parser found it rather than as it should be: a table row whose id
// cell is not a single code span keeps its text and reports no id, a cell that opens with no bold run
// reports none. Every judgement about what is wrong belongs to the validator, which has the schema to
// judge against and the words to say what was expected.
//
// Where the id comes from is the difference between the two sources, and the only one this record shows.
// A table row carries the id its author wrote, so `Id` is null wherever the parser could read none. A
// heading is slugged into its own id, so it always has one. The fields below `Id` belong to the table
// source and stand empty for a heading, which carries no cells to fill them.
public class PartRow
{
    public string? Id;         // the part's own id: the Id cell's code span, or a heading's slug
    public string IdText = ""; // the Id cell flattened, for quoting back when it is not a span
    public string Text = "";   // the Clause cell flattened, or the heading as written
    public string? BoldLead;   // the leading bold run's text, when the cell opens with one
    public int Line;

    // Where the part's body begins and ends in the document's text, for a part written as a heading:
    // everything beneath it, up to the next part, the end of the section holding it, or the end of the
    // document. Both zero for a part written as a table row, whose body is the row.
    //
    // Held as a span rather than as flattened prose because a body is several blocks and what a reader
    // wants of it differs: an export carries the words as written, and a rule counting paragraphs would
    // count them here.
    public int BodyStart;
    public int BodyEnd;

    public ReadOnlySpan<char> Body(string text) =>
        BodyEnd > BodyStart ? text.AsSpan()[BodyStart..Math.Min(BodyEnd, text.Length)] : default;
}

// One H2 and what stands under it: the heading as written, the line it sits on, and where its body
// begins and ends in the document's text. The body runs to the next heading at the same level or
// above, so a sub-heading and everything beneath it stand inside the section above them. Found once
// here, because two checks read the same stretch — whether anything is written under the heading, and
// whether what is written matches a pattern a rule supplies.
public record Section(string Title, int Line, int BodyStart, int BodyEnd);

// What is being read. A record is a document of the corpus; a template is the file every record of its
// type is copied from. The two are held to different questions — a template has no id of its own, no
// filename to agree with and no values filled in — and this is what the checks branch on, so that
// "which of these applies to a template" is answered once per check, where the check is written.
public enum DocKind
{
    Record,
    Template
}

public partial class Doc
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

    // The record's addressable parts, read from wherever the type's `parts:` block says they live — the
    // rows of a table, or the headings a section is written as. `PartTableHeaders` belongs to the table
    // source alone and is null where the named section holds no table: the section being absent and the
    // section holding prose are different faults, and only the parser can tell them apart. `PartRefs`
    // collects every code span shaped like a citation of a part, from anywhere in the document.
    public List<string>? PartTableHeaders;
    public readonly List<PartRow> Parts = [];
    public int PartSectionLine;
    public int PartTableLine;
    public readonly List<(string Ref, int Line)> PartRefs = [];

    // Citations of a part written with a colon, which is not the separator the corpus uses. Collected
    // apart from `PartRefs` so the validator can name the separator and quote the form to write. A
    // citation reaching no part and one spelled wrongly need different words back.
    public readonly List<(string Ref, int Line)> ColonCitations = [];

    public readonly List<Section> Sections = [];
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

    // The extensions every record depends on: the frontmatter block, the pipe tables a clause section
    // is written as, and task lists. A `[ ]` at the head of a list item is a checkbox, and the same
    // brackets mid-sentence are prose, so what they mean depends on where the block puts them. The
    // bracket scan below reads inlines and cannot ask. Markdig can: with task lists on, the checkbox
    // parses as an inline of its own and breaks the run of literals the scan reads, while a `[x]`
    // typed mid-sentence still reaches it. A built pipeline is immutable, so it is built once here
    // and shared across every parse.
    private static readonly MarkdownPipeline Pipeline =
        new MarkdownPipelineBuilder().UseYamlFrontMatter().UsePipeTables().UseTaskLists().Build();

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

        // H1 and H2 in one walk, over the headings that divide the document: an H2's body ends at the
        // next of them, so where a section stops is a question about its siblings.
        var divisions = ast.Descendants<HeadingBlock>().Where(h => h.Level <= 2).ToList();
        for (var i = 0; i < divisions.Count; i++)
        {
            var h = divisions[i];
            var txt = Md.PlainText(h.Inline);
            if (h.Level == 1)
            {
                if (doc.H1 is not null) continue;
                doc.H1 = txt;
                doc.H1Line = h.Line + 1;
                continue;
            }

            var from = Math.Min(h.Span.End + 1, text.Length);
            var to = i + 1 < divisions.Count ? divisions[i + 1].Span.Start : text.Length;
            doc.Sections.Add(new Section(txt, h.Line + 1, from, Math.Max(from, to)));
        }

        // Identity line — the block immediately after the H1, when it is a paragraph opening with a
        // code span. Anchoring on "the next block" rather than "the first paragraph of code spans
        // anywhere" is what lets the validator say the line is missing: a document that puts its
        // Y-statement or its prose first has no identity line, however its later paragraphs read.
        ReadIdentity(ast, doc);

        // The record's own parts, from wherever its type keeps them, and the citations of a part anywhere
        // in the document. A citation is collected whatever this document's type — a part is cited from
        // the record that answers it, which is rarely one of the same type.
        if (type?.Parts is { } parts) ReadParts(ast, doc, parts);
        foreach (var code in ast.Descendants<CodeInline>())
        {
            if (code.Content is not { } content) continue;
            if (!NamesARecord(content, schema)) continue;
            if (PartCitationRegex().IsMatch(content)) doc.PartRefs.Add((content, code.Line + 1));
            else if (ColonCitationRegex().IsMatch(content)) doc.ColonCitations.Add((content, code.Line + 1));
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
                Label = link.Reference?.Label ?? link.Label,
                Position = link.Span.Start
            });
            if (link.Reference is not null) doc.UsedLabels.Add(link.Reference.Label ?? "");
        }

        foreach (var def in ast.Descendants<LinkReferenceDefinition>())
            if (def.Label is not null)
                doc.DefinedLabels.Add(def.Label);

        // Bare [bracket] tokens left in prose (an undefined shortcut ref like [ADR-0099] renders as
        // literal text). Markdig emits a failed link opener '[' as its own literal inline and leaves
        // the ']' in the following sibling, so the brackets never survive in one literal: the scan
        // rejoins the run of consecutive literal siblings first. A real link or code span is a
        // non-literal inline and breaks the run, which is what keeps code and resolved links out.
        // Driven from each leaf block's root inline, because `Descendants<ContainerInline>` yields
        // only nested containers and never the root that holds the run.
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

    // What a field holds, read as a list however it is written. A scalar is the one-entry case rather
    // than a separate shape, so a check over the values of a field asks the same question of
    // `depends-on: [svc-a, svc-b]` and of `successor: tol-vault` — whether either shape is the one the
    // schema declared is the field's own check, and is answered before this is asked.
    public List<string> FrontList(string key)
    {
        var values = new List<string>();
        if (Front is null) return values;

        foreach (var kv in Front.Children)
        {
            if (((YamlScalarNode)kv.Key).Value != key) continue;
            if (kv.Value is YamlSequenceNode seq)
                values.AddRange(seq.Children.OfType<YamlScalarNode>().Select(s => s.Value).OfType<string>());
            else if ((kv.Value as YamlScalarNode)?.Value is { Length: > 0 } scalar) values.Add(scalar);
        }

        return values;
    }

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

        doc.IdentitySpans = [.. inline.Descendants<CodeInline>().Select(c => c.Content)];
        doc.IdentityLine = p.Line + 1;
    }

    // A code span shaped like a citation of a part — an id, a dot, and the part's id, as
    // `pol-VURM.TIMEBOX` or `gls-knowledge-as-code.corpus`. Both halves admit a hyphen, because a record
    // named by a slug carries one and so does the term a heading is slugged from.
    //
    // Deliberately loose on case and width: a mis-cased or over-long citation is one the validator should
    // report as unresolved rather than one the parser should quietly decline to see.
    [System.Text.RegularExpressions.GeneratedRegex(@"^[a-z]{2,4}-[A-Za-z0-9][A-Za-z0-9-]*\.[A-Za-z0-9][A-Za-z0-9_-]*$")]
    private static partial System.Text.RegularExpressions.Regex PartCitationRegex();

    // Whether the half before the dot opens with a prefix some type declares. The shape alone cannot
    // tell a citation from a filename written as a code span: `vurm-vulnerability-remediation.md` has
    // every feature of one, and reading it as a citation would report a policy nobody wrote and a part
    // called `md`. Asking after the prefix settles it, because a record's id opens with its type's.
    //
    // The prefixes come from the schema, so adopting a type admits citations into it and declining one
    // shuts them off, with no list here to keep in step.
    private static bool NamesARecord(string content, Schema schema)
        => content.IndexOf('-') is var dash and > 0 && schema.IdPrefixes.Contains(content[..dash]);

    // The same citation with a colon where the dot belongs, as `std-A11Y:WCAG`. An id has to sit on the
    // left for this to match, which is what keeps a scoped reference — `eng:pol-VURM`, a shortcode and
    // then an id — out of it: a shortcode carries no type prefix and no hyphen before the colon.
    [System.Text.RegularExpressions.GeneratedRegex("^[a-z]{2,4}-[A-Za-z0-9]+:[A-Za-z0-9]+$")]
    private static partial System.Text.RegularExpressions.Regex ColonCitationRegex();

    // The record's parts, from the source its type declares. One walk either way, over the blocks
    // standing under the H2 the schema names, and nothing here judges what it finds: the validator has
    // the schema to judge against and the words to say what was expected.
    private static void ReadParts(MarkdownDocument ast, Doc doc, PartSpec spec)
    {
        var inSection = false;

        // The heading part whose body is still running. A body ends where the next thing that can hold
        // one begins, so the row is filled in on meeting that rather than on being added.
        PartRow? open = null;

        foreach (var block in ast)
        {
            if (block is HeadingBlock h)
            {
                // A heading below the section level does not end the section, and where parts are
                // written as headings it is one of them. Anything deeper than a part stands inside the
                // part above it, so it closes nothing.
                if (h.Level > 2)
                {
                    if (h.Level != spec.Level) continue;
                    if (!inSection || spec.Source != PartSpec.Headings) continue;

                    Close(h.Span.Start);
                    open = AddHeading(doc, h);
                    continue;
                }

                Close(h.Span.Start);
                inSection = string.Equals(Md.PlainText(h.Inline), spec.Section, StringComparison.OrdinalIgnoreCase);
                if (inSection) doc.PartSectionLine = h.Line + 1;
                continue;
            }

            if (!inSection || spec.Source != PartSpec.Table) continue;
            if (block is not Markdig.Extensions.Tables.Table table) continue;

            ReadTable(doc, table);
            return;
        }

        Close(doc.Text.Length);
        return;

        void Close(int at)
        {
            if (open is null) return;
            open.BodyEnd = Math.Max(open.BodyStart, at);
            open = null;
        }
    }

    // A heading is a part nobody wrote an id for, so the id comes from the heading: the same anchor a
    // link to it would use, which lets a citation and a link name one entry. An empty slug — a heading
    // of punctuation alone — leaves the id null, and that reads downstream as a part offering no
    // address, exactly as an unreadable id cell does.
    private static PartRow AddHeading(Doc doc, HeadingBlock h)
    {
        var text = Md.PlainText(h.Inline);
        var slug = Md.Slug(text);
        var row = new PartRow
        {
            Id = slug.Length > 0 ? slug : null,
            IdText = text,
            Text = text,
            Line = h.Line + 1,
            BodyStart = Math.Min(h.Span.End + 1, doc.Text.Length)
        };

        doc.Parts.Add(row);
        return row;
    }

    // The first table under the named section, read whole. The header row supplies `PartTableHeaders`
    // and every other row a `PartRow`, however malformed, so that "no table here" is the only thing the
    // parser decides.
    private static void ReadTable(Doc doc, Markdig.Extensions.Tables.Table table)
    {
        doc.PartTableLine = table.Line + 1;
        foreach (var row in table.OfType<Markdig.Extensions.Tables.TableRow>())
        {
            var cells = row.OfType<Markdig.Extensions.Tables.TableCell>()
                .Select(c => (c.FirstOrDefault() as LeafBlock)?.Inline)
                .ToList();

            if (row.IsHeader)
            {
                doc.PartTableHeaders = [.. cells.Select(Md.PlainText)];
                continue;
            }

            var clause = cells.Count > 1 ? cells[1] : null;
            doc.Parts.Add(new PartRow
            {
                Id = cells.Count > 0 && cells[0]?.FirstChild is CodeInline { NextSibling: null } code
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

        doc.PartTableHeaders ??= [];
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
            // literal '[x]' is a candidate undefined reference. Skip one followed by
            // '(' or '[', which the AST handled elsewhere, or by ':', which opens a
            // link definition rather than a reference to one.
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
