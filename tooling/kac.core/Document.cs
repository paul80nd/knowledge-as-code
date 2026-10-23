using System.Text;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace kac.core;

public class LinkRef
{
    /// <summary>The url, taken from the definition where a label stands in for it.</summary>
    public string Target = "";
    public int Line;
    /// <summary>A reference or shortcut link, which names a definition rather than a url.</summary>
    public bool IsReference;
    /// <summary>The definition's label, which is what resolved the target.</summary>
    public string? Label;

    /// <summary>
    /// Spelled as the use site spells it, which may differ from <c>Label</c> in case. Null where the reader is shown
    /// something else: an inline link, or a full reference displaying its own words.
    /// </summary>
    public string? DisplayedLabel;

    /// <summary>Offset in the document's text, so a rule can compare it with a part's body span.</summary>
    public int Position;
}

/// <summary>
/// One part as the parser found it, not as it should be. The validator makes every judgement. A table row's <c>Id</c>
/// is null where the parser read none, and a heading's only where its slug is empty. <c>BoldLead</c> is always null for
/// a heading.
/// </summary>
public class PartRow
{
    /// <summary>The part's own id: the Id cell's code span, or a heading's slug.</summary>
    public string? Id;
    /// <summary>The Id cell flattened, for quoting back when it is not a span.</summary>
    public string IdText = "";
    /// <summary>The Clause cell flattened, or the heading as written.</summary>
    public string Text = "";
    /// <summary>The leading bold run's text, when the cell opens with one.</summary>
    public string? BoldLead;
    public int Line;

    /// <summary>
    /// Null for a heading. The first two cells are read into <c>Id</c> and <c>Text</c>. Further columns reach a reader
    /// through <c>parts.columns:</c> and an export line's <c>column.&lt;Header&gt;</c>.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Cells;

    /// <summary>
    /// The link labels each cell writes, in order. Flattening drops where a label ended, so <c>[ISO
    /// 27001:2022].A.8.24</c> arrives in <c>Cells</c> as <c>ISO 27001:2022.A.8.24</c>.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>>? CellLinks;

    /// <summary>
    /// The body of a part written as a heading: up to the next part, the end of its section or the end of the document.
    /// Both zero for a table row, whose body is the row.
    /// </summary>
    public int BodyStart;
    public int BodyEnd;

    public ReadOnlySpan<char> Body(string text) =>
        BodyEnd > BodyStart ? text.AsSpan()[BodyStart..Math.Min(BodyEnd, text.Length)] : default;

    /// <summary>False for a part written as a table row.</summary>
    public bool Contains(Markdig.Syntax.SourceSpan span) =>
        BodyEnd > BodyStart && span.Start >= BodyStart && span.End <= BodyEnd;
}

/// <summary>
/// One labelled footnote and its citations: <c>_**Covers:** [pol-SCRT].EMBED_</c>, the last block of its section.
/// <c>Italic</c> is false where a space inside the underscores stopped markdown reading them. A line failing either
/// flag is kept, so it is reported and its citations still count.
/// </summary>
public record CitationFootnote(int Line, bool Italic, bool ClosesSection, IReadOnlyList<string> Citations);

/// <summary>One H2. Its body runs to the next H1 or H2, so a sub-heading stands inside the section above it.</summary>
public record Section(string Title, int Line, int BodyStart, int BodyEnd);

/// <summary>
/// A template has no id, no filename to agree with and no values filled in, so checks branch on the kind.
/// </summary>
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

    /// <summary>The frontmatter mapping, read with the representation model.</summary>
    public YamlMappingNode? Front;
    public readonly List<string> FrontKeys = [];
    /// <summary>1-based line where the frontmatter block begins.</summary>
    public int FrontStartLine;

    /// <summary>
    /// The first character after the frontmatter, or 0 where there is none. A check reads the source from here, because
    /// a credential is likeliest in a fenced block, which the flattened text drops.
    /// </summary>
    public int BodyStart;

    public string? H1;
    public int H1Line;

    /// <summary>
    /// The raw code spans of the paragraph directly beneath the H1, in order: "<c>Policy: pol-A11Y</c> <c>DRAFT</c>".
    /// Null where that paragraph does not open with a code span. Never an empty list.
    /// </summary>
    public List<string>? IdentitySpans;
    public int IdentityLine;

    /// <summary>
    /// <c>PartTableHeaders</c> is null where the part section holds no table, which tells a missing section from one
    /// holding prose. <c>PartRefs</c> collects every citation of a part, in every form, from anywhere in the document.
    /// </summary>
    public List<string>? PartTableHeaders;
    public readonly List<PartRow> Parts = [];
    public int PartSectionLine;
    public int PartTableLine;
    public readonly List<(string Ref, int Line)> PartRefs = [];

    /// <summary>Citations written with a colon, kept apart so the validator can name the right separator.</summary>
    public readonly List<(string Ref, int Line)> ColonCitations = [];

    public readonly List<Section> Sections = [];
    public readonly List<LinkRef> Links = [];
    public readonly List<string> DefinedLabels = [];

    /// <summary>
    /// Keyed by label, because a reference holds only the label. A label defined twice keeps the first target, which is
    /// the one a renderer resolves.
    /// </summary>
    public readonly Dictionary<string, string> DefinedTargets = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Definitions render as nothing, so an export leaves these spans out of a section's prose.</summary>
    public readonly List<(int Start, int End)> DefinitionSpans = [];
    public readonly HashSet<string> UsedLabels = new(StringComparer.OrdinalIgnoreCase);
    public readonly List<(string inner, int line)> BareBracketTokens = [];
    public QuoteBlock? YStatement;

    /// <summary>
    /// Keyed by the mirrored section, because a type may mirror two. A section the document lacks arrives as an empty
    /// list, so every id in the field is reported.
    /// </summary>
    public IReadOnlyDictionary<string, List<LinkRef>> MirroredSectionLinks =
        new Dictionary<string, List<LinkRef>>(StringComparer.OrdinalIgnoreCase);

    /// <summary>Keyed by label, for the same reason. A label no line carries arrives as an empty list.</summary>
    public IReadOnlyDictionary<string, List<CitationFootnote>> CitationFootnotes =
        new Dictionary<string, List<CitationFootnote>>(StringComparer.OrdinalIgnoreCase);

    /// <summary><c>requireFrontmatter: false</c> reads a type page or a framework document for its prose.</summary>
    public static Doc? Parse(string rel, string text, Schema schema, bool requireFrontmatter = true)
    {
        var ast = Md.Parse(text);

        var fmBlock = ast.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
        // Not migrated, so the caller counts and skips.
        if (fmBlock is null && requireFrontmatter) return null;

        var top = rel.Split('/')[0];
        schema.ByFolder.TryGetValue(top, out var type);

        var doc = new Doc { Rel = rel, Folder = top, Type = type, Text = text, Ast = ast };

        // Parsed with the representation model, so key order and scalar quoting survive.
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

                    // A key that is itself a sequence or a mapping is legal YAML and names no field, so
                    // it is read as the empty key. `unknown-key` then reports it against the document
                    // that wrote it, where a cast would have stopped the parse.
                    foreach (var kv in map.Children)
                        doc.FrontKeys.Add((kv.Key as YamlScalarNode)?.Value ?? "");
                }
            }
            // Only `YamlException`: anything else is a defect here, not the author's. `frontmatter-parses` reports it.
            catch (YamlException)
            {
                doc.Front = null;
            }
        }

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

        // Only the block directly after the H1, so a document opening with its Y-statement has no identity line.
        ReadIdentity(ast, doc);

        // Citations are collected whatever the document's type, because a part is rarely cited from its own type.
        if (type?.Parts is { } parts) ReadParts(ast, doc, parts);
        foreach (var code in ast.Descendants<CodeInline>())
        {
            if (code.Content is not { } content) continue;
            if (!NamesARecord(content, schema)) continue;
            if (Cites(content)) doc.PartRefs.Add((content, code.Line + 1));
            else if (ColonCitationRegex().IsMatch(content)) doc.ColonCitations.Add((content, code.Line + 1));
        }

        // Iterating LinkInline leaves code out: neither a code span nor a code block carries one.
        var citations = new HashSet<string>(StringComparer.Ordinal);
        foreach (var link in ast.Descendants<LinkInline>())
        {
            if (link.IsImage) continue;

            // Collected beside the code spans, so every form of citation answers to one pass. See `part-ref` in
            // .schema/_checks.yaml.
            if (LinkCitation(link, schema) is { } citation)
            {
                // Once per citation, keyed on the whole citation, because several parts of one record share a
                // definition.
                if (citations.Add(citation))
                {
                    if (Cites(citation)) doc.PartRefs.Add((citation, link.Line + 1));
                    else if (ColonCitationRegex().IsMatch(citation))
                        doc.ColonCitations.Add((citation, link.Line + 1));
                }
            }

            doc.Links.Add(new LinkRef
            {
                Target = link.Url ?? "",
                Line = link.Line + 1,
                IsReference = link.Reference is not null,
                Label = link.Reference?.Label ?? link.Label,
                DisplayedLabel = Displayed(link),
                Position = link.Span.Start
            });
            if (link.Reference is not null) doc.UsedLabels.Add(link.Reference.Label ?? "");
        }

        foreach (var def in ast.Descendants<LinkReferenceDefinition>())
        {
            if (def.Label is not null)
            {
                doc.DefinedLabels.Add(def.Label);
                doc.DefinedTargets.TryAdd(def.Label, def.Url ?? "");
            }

            doc.DefinitionSpans.Add((def.Span.Start, def.Span.End + 1));
        }

        // Bare [bracket] tokens left in prose. Markdig splits a failed link's `[` and `]` across literal siblings, so
        // the scan rejoins each run of literals, which a link or a code span breaks. Driven from each leaf's root
        // inline, because `Descendants<ContainerInline>` never yields the root.
        foreach (var leaf in ast.Descendants<LeafBlock>())
            if (leaf.Inline is not null)
                ScanContainer(leaf.Inline, doc.BareBracketTokens);

        doc.YStatement = ast.Descendants<QuoteBlock>().FirstOrDefault(q => q.Line > (doc.H1Line - 1));

        doc.MirroredSectionLinks = SectionLinks(ast, type?.DeclaredFields.Select(spec => spec.MirrorsSection));
        doc.CitationFootnotes =
            Footnotes(ast, schema, type?.Parts, type?.DeclaredFields.Select(spec => spec.MirrorsCitations));

        return doc;
    }

    /// <summary>
    /// Named so the schema's word, the arm resolving it and the index grouping on it cannot drift apart.
    /// </summary>
    public const string SubPath = "sub-path";

    /// <summary>
    /// <c>SchemaChecks</c> refuses a <c>from:</c> not in this list, so an unresolvable source is reported at the
    /// schema.
    /// </summary>
    public static readonly IReadOnlySet<string> DerivedSources =
        new HashSet<string>([SubPath], StringComparer.Ordinal);

    /// <summary>
    /// Null where the type derives nothing for this key. One arm per source in <c>DerivedSources</c>.
    /// </summary>
    public string? Derived(string key) =>
        Type?.Fields.GetValueOrDefault(key)?.From switch
        {
            SubPath => SubPathValue(),
            _ => null
        };

    /// <summary>
    /// Empty for a record sitting directly in its type folder, which every caller reads as unset. See
    /// <c>docs/use/discovery.md</c>.
    /// </summary>
    private string SubPathValue()
    {
        if (Folder.Length == 0 || Rel.Length <= Folder.Length) return "";

        var under = Rel[(Folder.Length + 1)..];
        var cut = under.LastIndexOf('/');
        return cut < 0 ? "" : under[..cut];
    }

    /// <summary>
    /// Derived first, because a derived key is never the author's: <c>derived-key</c> reports one written, and this
    /// keeps the two from disagreeing. <c>FrontNode</c> reads the same way, so <c>field()</c> and <c>present()</c>
    /// agree on a derived key.
    /// </summary>
    public string? FrontScalar(string key) => Derived(key) ?? (Front is null
        ? null
        : (from kv in Front.Children
            where ((YamlScalarNode)kv.Key).Value == key
            select (kv.Value as YamlScalarNode)?.Value).FirstOrDefault());

    /// <summary>
    /// A scalar is the one-entry case, so a check asks the same of <c>depends-on: [a, b]</c> and <c>successor: x</c>.
    /// Reads scalar entries only and drops a mapping. <c>schema-shape</c> refuses every key that would send an
    /// <c>of: object</c> field here.
    /// </summary>
    public List<string> FrontList(string key)
    {
        // An empty derivation is no entries, which makes `present()` false for a record filed flat.
        if (Derived(key) is { } derived)
            return derived.Length == 0 ? [] : [derived];

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

    /// <summary>For display. A mapping entry reads as its <c>naming</c> key, which the schema chooses.</summary>
    public List<string> FrontEntries(string key, string? naming)
    {
        var values = new List<string>();
        if (Front is null) return values;

        foreach (var kv in Front.Children)
        {
            if (((YamlScalarNode)kv.Key).Value != key) continue;
            if (kv.Value is not YamlSequenceNode seq) continue;

            foreach (var item in seq.Children)
                switch (item)
                {
                    case YamlScalarNode { Value: { Length: > 0 } v }: values.Add(v); break;
                    case YamlMappingNode map when naming is not null
                                                  && Yaml.Get(map, naming) is YamlScalarNode
                                                  {
                                                      Value: { Length: > 0 } named
                                                  }:
                        values.Add(named);
                        break;
                }
        }

        return values;
    }

    /// <summary>
    /// The node as written, for an export, which needs mappings intact. A derived field comes back as a scalar, and an
    /// empty derivation as null, so a flat record exports the field as unset.
    /// </summary>
    public YamlNode? FrontNode(string key)
    {
        if (Derived(key) is { } derived)
            return derived.Length == 0 ? null : new YamlScalarNode(derived);

        return Front is null
            ? null
            : (from kv in Front.Children
                where ((YamlScalarNode)kv.Key).Value == key
                select kv.Value).FirstOrDefault();
    }

    /// <summary>
    /// A paragraph after the H1 that opens with a code span is an attempted identity line, so a wrong span count is
    /// reported as malformed rather than missing. The span count also catches stray prose between spans.
    /// </summary>
    private static void ReadIdentity(MarkdownDocument ast, Doc doc)
    {
        var blocks = ast.ToList();
        var i = blocks.FindIndex(b => b is HeadingBlock { Level: 1 });
        if (i < 0 || i + 1 >= blocks.Count) return;

        if (blocks[i + 1] is not ParagraphBlock { Inline: { FirstChild: CodeInline } inline } p) return;

        doc.IdentitySpans = [.. inline.Descendants<CodeInline>().Select(c => c.Content)];
        doc.IdentityLine = p.Line + 1;
    }

    /// <summary>
    /// <c>pol-VURM.TIMEBOX</c> or <c>gls-knowledge-as-code.corpus</c>, optionally scoped as
    /// <c>eng:pol-VURM.TIMEBOX</c>. Loose on case and width after the prefix, so the validator reports a mis-cased id
    /// the parser still sees.
    /// </summary>
    [System.Text.RegularExpressions.GeneratedRegex(
        @"^(?:[a-z0-9]+:)?[a-z]{2,4}-[A-Za-z0-9][A-Za-z0-9-]*\.[A-Za-z0-9][A-Za-z0-9_-]*$")]
    private static partial System.Text.RegularExpressions.Regex PartCitationRegex();

    /// <summary>
    /// An imported record cited whole, as <c>eng:pol-VURM</c>. A local id in a code span is a mention, not a citation,
    /// so only a scoped one is collected.
    /// </summary>
    [System.Text.RegularExpressions.GeneratedRegex(@"^[a-z0-9]+:[a-z]{2,4}-[A-Za-z0-9][A-Za-z0-9-]*$")]
    private static partial System.Text.RegularExpressions.Regex ScopedRecordRegex();

    /// <summary>
    /// The <c>.TIMEBOX</c> after <c>[pol-VURM].TIMEBOX</c>, read off the following text so a closing emphasis delimiter
    /// is not taken into the id. Nothing may stand between separator and id, so <c>See [pol-VURM]. The</c> cites no
    /// part. A colon is collected too, for <c>ColonCitationRegex</c> to word.
    /// </summary>
    private static string PartSuffix(LinkInline link)
    {
        if (link.NextSibling is not LiteralInline next) return "";

        var after = next.Content.ToString();
        if (after.Length < 2) return "";
        if (after[0] is not ('.' or ':') || !char.IsAsciiLetterOrDigit(after[1])) return "";

        var end = 1;
        while (end < after.Length && (char.IsAsciiLetterOrDigit(after[end]) || after[end] is '_' or '-')) end++;
        return after[..end];
    }

    /// <summary>
    /// Null where the link cites nothing. A label already carrying a part id ends the citation, so no second one is
    /// read onto it.
    /// </summary>
    private static string? LinkCitation(LinkInline link, Schema schema)
    {
        var cited = link.Reference?.Label ?? (link.FirstChild as LiteralInline)?.Content.ToString();
        if (cited is null || !NamesARecord(cited, schema)) return null;

        return PartCitationRegex().IsMatch(cited) ? cited : cited + PartSuffix(link);
    }

    /// <summary>
    /// Asks the schema's prefixes, so <c>vurm-vulnerability-remediation.md</c> in a code span is not a citation.
    /// </summary>
    private static bool NamesARecord(string content, Schema schema)
    {
        // The scope comes off first: a shortcode has no hyphen, so `eng:pol` would match no type.
        var (_, id) = Citation.Split(content);
        return id.IndexOf('-') is var dash and > 0 && schema.IdPrefixes.Contains(id[..dash]);
    }

    private static bool Cites(string text) =>
        PartCitationRegex().IsMatch(text) || ScopedRecordRegex().IsMatch(text);

    /// <summary><c>std-A11Y:WCAG</c>. An id must sit left of the colon, which keeps <c>eng:pol-VURM</c> out.</summary>
    [System.Text.RegularExpressions.GeneratedRegex("^[a-z]{2,4}-[A-Za-z0-9]+:[A-Za-z0-9]+$")]
    private static partial System.Text.RegularExpressions.Regex ColonCitationRegex();

    private static void ReadParts(MarkdownDocument ast, Doc doc, PartSpec spec)
    {
        var inSection = false;

        // Filled in on reaching the next thing that can hold a body, not on being added.
        PartRow? open = null;

        foreach (var block in ast)
        {
            if (block is HeadingBlock h)
            {
                // Below the section level: a heading at the part level is a part, and anything deeper stays inside one.
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

    /// <summary>
    /// The id is the heading's slug, the anchor a link would use, so a citation and a link name one entry. A heading of
    /// punctuation alone leaves the id null.
    /// </summary>
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

    /// <summary>
    /// Every row gives a <c>PartRow</c>, however malformed, so "no table here" is the only thing the parser decides.
    /// </summary>
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
                Line = row.Line + 1,
                Cells = Cells(doc.PartTableHeaders, cells),
                CellLinks = Links(doc.PartTableHeaders, cells)
            });
        }

        doc.PartTableHeaders ??= [];
        return;

        // Keys only the cells there are. A moved header is `clause-table`'s to report.
        static Dictionary<string, string> Cells(IReadOnlyList<string>? headers, List<ContainerInline?> row)
        {
            var byHeader = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (headers is null) return byHeader;

            for (var i = 0; i < Math.Min(headers.Count, row.Count); i++)
                byHeader.TryAdd(headers[i], Md.PlainText(row[i]));
            return byHeader;
        }

        // An inline link has no label, so its display text stands in.
        static Dictionary<string, IReadOnlyList<string>> Links(IReadOnlyList<string>? headers,
            List<ContainerInline?> row)
        {
            var byHeader = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
            if (headers is null) return byHeader;

            for (var i = 0; i < Math.Min(headers.Count, row.Count); i++)
                byHeader.TryAdd(headers[i], row[i] is { } inline
                    ? [.. inline.Descendants<LinkInline>().Select(l => l.Label ?? Md.PlainText(l))]
                    : []);
            return byHeader;
        }
    }

    private static string StripFences(string text, YamlFrontMatterBlock block)
    {
        var slice = text.Substring(block.Span.Start, block.Span.Length);
        var lines = slice.Replace("\r\n", "\n").Split('\n').ToList();
        if (lines.Count > 0 && lines[0].TrimEnd() == "---") lines.RemoveAt(0);
        if (lines.Count > 0 && lines[^1].TrimEnd() == "---") lines.RemoveAt(lines.Count - 1);
        return string.Join("\n", lines);
    }

    /// <summary>Only direct children are joined at each level, so a literal is scanned once.</summary>
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
            // Skip one followed by `(` or `[`, which the AST handled, or by `:`, which opens a definition.
            var next = close + 1 < s.Length ? s[close + 1] : ' ';
            // Map the line from the closing bracket: Markdig gives a failed link-opener '[' its own
            // literal whose source line is 0, while the ']' always sits in a real-text literal that
            // carries the correct line.
            if (inner.Length > 0 && inner.IndexOf(']') < 0 && next != '(' && next != '[' && next != ':')
                outp.Add((inner, LineAt(segments, close)));
            i = close;
        }
    }

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

    /// <summary>
    /// The form is a whole line in italic opening on a bold label: <c>**Not:**</c> alone is prose. Nested paragraphs
    /// are read too, so a footnote inside a list item is reported where it sits, not dropped. A heading closes the
    /// footnote's section only at or above the part level. Where <paramref name="parts"/> is null, any heading closes
    /// it.
    /// </summary>
    private static Dictionary<string, List<CitationFootnote>> Footnotes(
        MarkdownDocument ast, Schema schema, PartSpec? parts, IEnumerable<string?>? labels)
    {
        // Matched on the label without its colon, so `mirrors-citations: "Covers:"` matches. Two fields spelling
        // one label two ways share the list.
        var result = new Dictionary<string, List<CitationFootnote>>(StringComparer.OrdinalIgnoreCase);
        var matching = new Dictionary<string, List<CitationFootnote>>(StringComparer.OrdinalIgnoreCase);
        foreach (var label in (labels ?? []).OfType<string>())
        {
            if (!matching.TryGetValue(Label(label), out var footnotes))
                matching[Label(label)] = footnotes = [];
            result[label] = footnotes;
        }

        if (result.Count == 0) return result;

        // The link definitions render as nothing, and markdig gathers them into one block at the end of
        // every document whatever the source. Left in, a footnote closing the last section would read as
        // standing in the middle of one.
        var blocks = ast.Where(b => b is not LinkReferenceDefinitionGroup).ToList();

        foreach (var para in ast.Descendants<ParagraphBlock>())
        {
            if (para.Inline?.FirstChild is not { } first) continue;

            // Two shapes reach this line: the form, as one italic opening on the bold label, and a mark with a space
            // against it, as an unpaired delimiter run beside the label. `Italic` tells them apart. Each reads only
            // the stretch its label opens: to the closing mark, or to the line break.
            bool italic;
            IEnumerable<MarkdownObject> scope;
            EmphasisInline bold;
            if (first is EmphasisInline { DelimiterCount: 1 } em
                && em.FirstChild is EmphasisInline { DelimiterCount: 2 } inItalic)
                (italic, scope, bold) = (true, em.Descendants(), inItalic);
            else if (first is LiteralInline stray && Unpaired(stray.Content.ToString())
                                                  && stray.NextSibling is EmphasisInline { DelimiterCount: 2 } beside)
                (italic, scope, bold) = (false, OnTheLine(first), beside);
            else continue;

            if (!matching.TryGetValue(Label(Md.PlainText(bold)), out var footnotes)) continue;

            var at = blocks.IndexOf(para);
            var closes = at >= 0
                         && (at + 1 == blocks.Count
                             || blocks[at + 1] is HeadingBlock next && (parts is null || next.Level <= parts.Level));

            // Both notations, in line order: a local record is cited by link, an imported one by code span.
            var cited = scope
                .Select(inline => inline switch
                {
                    LinkInline { IsImage: false } link => LinkCitation(link, schema),
                    CodeInline { Content: { } content } when NamesARecord(content, schema) => content,
                    _ => null
                })
                .OfType<string>();

            footnotes.Add(new CitationFootnote(para.Line + 1, italic, closes, [.. cited]));
        }

        return result;
    }

    /// <summary>Either mark, since an author may reach for the wrong pair.</summary>
    private static bool Unpaired(string text)
    {
        var run = text.Trim();
        return run.Length > 0 && (run.All(c => c == '_') || run.All(c => c == '*'));
    }

    /// <summary>A soft line break ends it, because the prose after one is not the footnote's.</summary>
    private static IEnumerable<MarkdownObject> OnTheLine(Inline? from)
    {
        for (var node = from; node is not null and not LineBreakInline; node = node.NextSibling)
        {
            yield return node;
            if (node is ContainerInline container)
                foreach (var inner in container.Descendants())
                    yield return inner;
        }
    }

    /// <summary>
    /// The label this link shows a reader, or null where it shows something else. <c>IsShortcut</c> does not separate a
    /// collapsed reference from a full one, so the flattened text does. A label carrying emphasis or a code span
    /// answers null, and is judged at its definition alone.
    /// </summary>
    private static string? Displayed(LinkInline link) =>
        link.Label is { } label && Md.PlainText(link) == label ? label : null;

    /// <summary>Without the colon the form puts after it, so schema and document compare equal.</summary>
    private static string Label(string text) => text.Trim().TrimEnd(':').TrimEnd();

    /// <summary>The links under each H2 a field mirrors. <c>Md.Blocks</c> says where a section ends.</summary>
    private static Dictionary<string, List<LinkRef>> SectionLinks(
        MarkdownDocument ast, IEnumerable<string?>? sectionTitles)
    {
        var result = (sectionTitles ?? [])
            .OfType<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToDictionary(title => title, _ => new List<LinkRef>(), StringComparer.OrdinalIgnoreCase);
        if (result.Count == 0) return result;

        foreach (var (block, title) in Md.Blocks(ast))
        {
            if (title is null || !result.TryGetValue(title, out var section)) continue;

            section.AddRange(from inline in Inlines(Leaves(block))
                from link in inline.Descendants<LinkInline>()
                where !link.IsImage
                select new LinkRef
                {
                    Target = link.Url ?? "", Line = link.Line + 1,
                    Label = link.Reference?.Label ?? link.Label, DisplayedLabel = Displayed(link)
                });
        }

        return result;
    }

    /// <summary>
    /// The inline content of each leaf carrying any. A <c>where</c> in a query cannot tell the <c>from</c> after it
    /// that it settled the value, so the leaves without content are dropped here instead.
    /// </summary>
    private static IEnumerable<ContainerInline> Inlines(IEnumerable<LeafBlock> leaves)
    {
        foreach (var leaf in leaves)
            if (leaf.Inline is { } inline)
                yield return inline;
    }

    /// <summary>
    /// Itself included when it is a leaf. Inlines hang off leaves, so this reaches a link written as prose.
    /// </summary>
    private static IEnumerable<LeafBlock> Leaves(Block block) =>
        block is LeafBlock leaf ? [leaf] : ((ContainerBlock)block).Descendants<LeafBlock>();
}
