using System.Text;

namespace kac.core;

/// <summary>
/// The generated-block markers, spelled here alone. A marker nothing matches stops a block being written, silently.
/// </summary>
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

        // An empty block closes on the next line. Padding it leaves two blank lines, which read as deleted
        // content.
        var body = inner.Length == 0 ? "\n" : $"\n\n{inner}\n\n";
        return text[..(bi + begin.Length)] + body + text[ei..];
    }

    /// <summary>
    /// A page with its generated blocks emptied, leaving the markers and everything a person wrote. <c>update</c>
    /// compares this. The markers stay, so a deleted block is still drift, and an unclosed marker leaves the rest of
    /// the page compared as written.
    /// </summary>
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
