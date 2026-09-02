using System.Text;

namespace kac.core;

// The pair of markers a generated block lives between, and the two readings of a page carrying them.
//
// Composed in one place so that nothing else spells them out: a second spelling is free to drift, and a
// marker nothing matches is a block that stops being written in silence. `generate` splices between the
// pair, `validate` holds a file to still carrying it, and `update` reads the page around it.
public static class Markers
{
    private const string BeginMarker = "<!-- BEGIN GENERATED: ";
    private const string EndMarker = "<!-- END GENERATED: ";
    private const string MarkerClose = " -->";

    public static string Begin(string name) => $"{BeginMarker}{name}{MarkerClose}";

    public static string End(string name) => $"{EndMarker}{name}{MarkerClose}";

    public static string SpliceBlock(string text, string name, string inner)
    {
        var begin = Begin(name);
        var end = End(name);
        var bi = text.IndexOf(begin, StringComparison.Ordinal);
        if (bi < 0) return text;
        var ei = text.IndexOf(end, bi, StringComparison.Ordinal);
        if (ei < 0) return text;

        // A block with nothing to say closes on the next line. Padding it out to the usual blank line
        // either side leaves two of them and reads as content someone deleted. Several blocks are
        // legitimately empty in a corpus that adopted few types: no pair of its types is easily confused,
        // and none of the words it kept collides with anything.
        var body = inner.Length == 0 ? "\n" : $"\n\n{inner}\n\n";
        return text[..(bi + begin.Length)] + body + text[ei..];
    }

    // A page with its generated blocks emptied, leaving the markers and everything a person wrote.
    //
    // `update` compares this, so an overlay page may carry a block derived from the corpus holding it. Two
    // corpora running the same framework hold the same prose and a different table beneath it, and both are
    // correct. The division is exact: `generate --check` answers for the generated half against the local
    // schema, and `update --check` answers for the authored half against the template. Neither has an
    // opinion about the other's half.
    //
    // The markers stay, so deleting a block, rather than regenerating it, is still drift. An unclosed
    // marker leaves the rest of the page compared as written, which is the honest reading of a file whose
    // structure the generator can no longer follow.
    public static string Authored(string text)
    {
        var sb = new StringBuilder();
        var at = 0;

        while (true)
        {
            var bi = text.IndexOf(BeginMarker, at, StringComparison.Ordinal);
            if (bi < 0) break;
            var opened = text.IndexOf(MarkerClose, bi, StringComparison.Ordinal);
            if (opened < 0) break;
            opened += MarkerClose.Length;
            var ei = text.IndexOf(EndMarker, opened, StringComparison.Ordinal);
            if (ei < 0) break;

            sb.Append(text, at, opened - at);
            at = ei;
        }

        return sb.Append(text, at, text.Length - at).ToString();
    }
}
