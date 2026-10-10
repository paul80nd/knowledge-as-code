using System.Text;

namespace kac.core;

/// <summary>
/// One quotation is read per citation: a reference link is collected once, so a second quotation of the same clause in
/// one record is not compared.
/// </summary>
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

    private static PartRow? Part(Doc cited, string id) =>
        cited.Parts.FirstOrDefault(p => string.Equals(p.Id, id, StringComparison.Ordinal));

    /// <summary>
    /// Three readings, because a policy writes a part as a table row and a standard writes one as a heading with the
    /// obligation in the body beneath it.
    /// </summary>
    private static IEnumerable<string> Readings(Doc cited, PartRow part, string[] lines)
    {
        yield return part.Text;

        var i = part.Line - 1;
        if (i >= 0 && i < lines.Length) yield return lines[i];

        var body = part.Body(cited.Text);
        if (!body.IsEmpty) yield return body.ToString();
    }

    /// <summary>
    /// Prose wraps at 120 characters, so a quotation runs past its citation's line. A blank line or a new block ends
    /// the wrap.
    /// </summary>
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

    /// <summary>
    /// A list marker takes a space after it, which tells a new bullet from a wrapped line opening <c>**MUST**</c>.
    /// </summary>
    private static bool Opens(string line) =>
        line[0] is '#' or '>' or '|'
        || line.StartsWith("```", StringComparison.Ordinal)
        || (line[0] is '*' or '-' or '+' && (line.Length == 1 || line[1] == ' '));

    /// <summary>
    /// An unclosed mark ends the reading, so the words after it are never taken as a second quotation.
    /// </summary>
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

    /// <summary>Case and punctuation are kept: a quotation is word for word.</summary>
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

    private static string[] LinesOf(Dictionary<string, string[]> cache, Doc doc)
    {
        if (cache.TryGetValue(doc.Rel, out var known)) return known;
        return cache[doc.Rel] = doc.Text.ReplaceLineEndings("\n").Split('\n');
    }
}
