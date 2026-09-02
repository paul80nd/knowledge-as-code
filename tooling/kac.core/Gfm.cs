using System.Text;

namespace kac.core;

// GitHub Flavoured Markdown, written the one way this repository writes it.
//
// `Generator` and `ChecksTable` fill the blocks a corpus holds, and `kac.tests` fills the CLI reference
// under `docs/cli/`. A second renderer for any of them would be free to drift from this one, and the
// drift would read as a page somebody had edited by hand.
//
// Written out rather than taken from Markdig, which `Md` already depends on to read markdown. Markdig
// renders an AST to HTML or back to normalised markdown, and neither pass writes a pipe table:
// `Markdown.Normalize` returns one as the run of its cell text.
public static class Gfm
{
    // Deterministic GFM table: columns padded to a fixed width, single-space padding, LF joins and no
    // trailing newline, because callers add their own. A cell that changes length inside its column moves
    // one row and leaves the rest alone.
    public static string RenderTable(List<string> headers, List<List<string>> rows)
    {
        var n = headers.Count;
        var w = new int[n];
        for (var i = 0; i < n; i++) w[i] = headers[i].Length;
        foreach (var row in rows)
            for (var i = 0; i < n; i++)
                w[i] = Math.Max(w[i], row[i].Length);

        List<string> lines =
        [
            Row(headers, w), Sep(w),
            .. rows.Select(r => Row(r, w))
        ];
        return string.Join("\n", lines);
    }

    private static string Row(List<string> cells, int[] w)
    {
        var sb = new StringBuilder("|");
        for (var i = 0; i < cells.Count; i++)
            sb.Append(' ').Append(cells[i].PadRight(w[i])).Append(" |");
        return sb.ToString();
    }

    // Dashes fill the cell, padding spaces included: |------| rather than | ---- |. Both are valid
    // GFM and the row width is identical, but this is the form markdown formatters normalise to, and
    // a generated file that an editor reformats on save is a permanently dirty working tree.
    private static string Sep(int[] w)
    {
        var sb = new StringBuilder("|");
        foreach (var width in w)
            sb.Append(new string('-', Math.Max(3, width + 2))).Append('|');
        return sb.ToString();
    }

    // A newline or a pipe inside a cell breaks the row it sits in.
    public static string Escape(string cell)
        => cell.Replace("\r", " ").Replace("\n", " ").Replace("|", "\\|");
}
