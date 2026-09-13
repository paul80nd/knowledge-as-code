using System.Text;

namespace kac.core;

// A control quotes the clause it verifies, and the clause is reworded somewhere else.
//
// The quotation is a reader's evidence that the control and the rule are about the same obligation.
// Edit the clause and the quotation still reads as one: the marks are there and the sentence scans, so
// nothing on the page shows that the words moved. This reads the cited clause and asks whether the
// quoted span is still in it.
//
// Only a clause this corpus stores can be checked. An imported record arrives as ids and fields, and
// its clause text stays in the corpus that published it, so a scoped citation is skipped in silence.
// So is a citation no local id matches, which `part-ref` reports instead.
//
// One quotation is read per citation. A reference link is collected once however many times a document
// cites it, so a second quotation of the same clause in the same record is not compared.
//
// A corpus rule rather than a document rule, because the words quoted are in a second document.
public sealed class ClauseQuotedFaithfully : ICorpusRule
{
    public RuleId RuleId => new("clause-quoted-faithfully");

    private static readonly CheckId Misquoted = new("clause-quoted-faithfully");

    public IReadOnlyList<CheckId> Emits => [Misquoted];

    public void Check(CorpusRuleContext ctx)
    {
        var lines = new Dictionary<string, string[]>(StringComparer.Ordinal);

        foreach (var doc in ctx.Records)
        foreach (var (text, at) in doc.PartRefs)
        {
            var citation = Citation.Read(text);
            if (citation.Scope is not null || citation.Part is not { } id) continue;
            if (!ctx.ById.TryGetValue(citation.Record, out var cited)) continue;
            if (Part(cited, id) is not { } clause) continue;

            var quotations = Quoted(Sentence(LinesOf(lines, doc), at)).ToList();
            if (quotations.Count == 0) continue;

            var readings = Readings(cited, clause, LinesOf(lines, cited)).Select(Flat).ToList();

            foreach (var quoted in quotations)
            {
                if (readings.Any(r => r.Contains(Flat(quoted), StringComparison.Ordinal))) continue;

                ctx.Err(doc, Misquoted,
                    $"'{citation}' does not say \"{Md.Snippet(quoted)}\". Re-read the clause in "
                    + $"{cited.Rel} and quote it word for word, or cite the clause that says this.", at);
            }
        }
    }

    // The part the citation addresses, or null where the record has no such part. Compared ordinally,
    // as `part-ref` compares it.
    private static PartRow? Part(Doc cited, string id) =>
        cited.Parts.FirstOrDefault(p => string.Equals(p.Id, id, StringComparison.Ordinal));

    // Everywhere the quotation could have come from: what the part renders to, the line it is written
    // on, and for a part written as a heading the lines beneath it.
    //
    // Three readings because two types write a part two ways. A policy writes one as a table row, where
    // `Text` is the clause flattened and the raw line is the same clause with its bold run and its code
    // spans intact. A standard writes one as a heading, where `Text` is the heading alone and the
    // obligation is a bullet in the body under it.
    private static IEnumerable<string> Readings(Doc cited, PartRow part, string[] lines)
    {
        yield return part.Text;

        var i = part.Line - 1;
        if (i >= 0 && i < lines.Length) yield return lines[i];

        var body = part.Body(cited.Text);
        if (!body.IsEmpty) yield return body.ToString();
    }

    // The citation's line together with the lines it wraps onto. Prose wraps at 120 characters, so a
    // quotation routinely runs past the line its citation sits on. A blank line, or one opening a block
    // of its own, ends the wrap.
    private static string Sentence(string[] lines, int at)
    {
        if (at < 1 || at > lines.Length) return "";

        var joined = new StringBuilder(lines[at - 1]);

        for (var i = at; i < lines.Length; i++)
        {
            var next = lines[i].TrimStart();
            if (next.Length == 0 || Opens(next)) break;
            joined.Append(' ').Append(next);
        }

        return joined.ToString();
    }

    // Whether a line begins a block of its own. A list marker takes a space after it, which is what
    // tells a new bullet from a wrapped line opening `**MUST**`.
    private static bool Opens(string line) =>
        line[0] is '#' or '>' or '|'
        || line.StartsWith("```", StringComparison.Ordinal)
        || (line[0] is '*' or '-' or '+' && (line.Length == 1 || line[1] == ' '));

    // Every double-quoted span on the line, in the order it is written. An unclosed mark ends the
    // reading, since pairing on past it would take the words after the quotation as a second one.
    private static IEnumerable<string> Quoted(string sentence)
    {
        var from = 0;

        while (sentence.IndexOf('"', from) is var open && open >= 0)
        {
            var close = sentence.IndexOf('"', open + 1);
            if (close < 0) yield break;

            if (close > open + 1) yield return sentence[(open + 1)..close];
            from = close + 1;
        }
    }

    // Whitespace runs collapsed to one space, so a span wrapped across two lines compares equal to the
    // same words wrapped elsewhere. Case and punctuation are kept: a quotation is word for word.
    private static string Flat(string text)
    {
        var flat = new StringBuilder(text.Length);
        var space = false;

        foreach (var c in text)
        {
            if (char.IsWhiteSpace(c))
            {
                space = flat.Length > 0;
                continue;
            }

            if (space) flat.Append(' ');
            space = false;
            flat.Append(c);
        }

        return flat.ToString();
    }

    // A document's text as lines, cached for one run. Every citation into a standard reads the same
    // lines, and a dozen controls citing three standards would otherwise split each of them a dozen
    // times.
    private static string[] LinesOf(Dictionary<string, string[]> cache, Doc doc)
    {
        if (cache.TryGetValue(doc.Rel, out var known)) return known;
        return cache[doc.Rel] = doc.Text.ReplaceLineEndings("\n").Split('\n');
    }
}
