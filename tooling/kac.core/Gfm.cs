using System.Text;

namespace kac.core;

/// <summary>
/// <c>Generator</c>, <c>ChecksTable</c> and the CLI reference under <c>docs/use/cli/</c> all render tables through
/// this, so none drifts. Written out because Markdig writes no pipe table: <c>Markdown.Normalize</c> returns one as the
/// run of its cell text.
/// </summary>
public static class Gfm
{
    /// <summary>No trailing newline, because callers add their own.</summary>
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

    /// <summary>
    /// Dashes fill the cell, padding spaces included: |------| rather than | ---- |. Both are valid GFM and the row
    /// width is identical, but this is the form markdown formatters normalise to, and a generated file that an editor
    /// reformats on save is a permanently dirty working tree.
    /// </summary>
    private static string Sep(int[] w)
    {
        var sb = new StringBuilder("|");
        foreach (var width in w)
            sb.Append(new string('-', Math.Max(3, width + 2))).Append('|');
        return sb.ToString();
    }

    public static string Escape(string cell)
        => cell.Replace("\r", " ").Replace("\n", " ").Replace("|", "\\|");
}
