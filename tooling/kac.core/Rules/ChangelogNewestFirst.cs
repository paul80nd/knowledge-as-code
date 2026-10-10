using System.Globalization;
using Markdig.Syntax;

namespace kac.core;

public sealed class ChangelogNewestFirst : IDocumentRule
{
    public RuleId RuleId => new("changelog-newest-first");

    private static readonly CheckId Reports = new("changelog-order");

    public IReadOnlyList<CheckId> Emits => [Reports];

    public void Check(RuleContext ctx)
    {
        var entries = Entries(ctx.Doc).ToList();

        for (var i = 1; i < entries.Count; i++)
        {
            var (date, line) = entries[i];
            if (string.CompareOrdinal(date, entries[i - 1].Date) <= 0) continue;

            // Scanned from the top, not taken from the neighbour, so the message names the line the entry moves to.
            var above = entries.First(e => string.CompareOrdinal(date, e.Date) > 0).Date;
            ctx.Report.Warn(Reports,
                $"changelog entry '{date}' is out of order: it belongs above '{above}'.", line);
        }
    }

    // The section's own items only. A nested list is one entry's workings and states no date.
    private static IEnumerable<(string Date, int Line)> Entries(Doc d)
    {
        var inSection = false;

        foreach (var block in d.Ast)
        {
            if (block is HeadingBlock h)
            {
                inSection = h.Level switch
                {
                    2 => string.Equals(Md.PlainText(h.Inline), "Changelog", StringComparison.OrdinalIgnoreCase),
                    < 2 => false,
                    _ => inSection
                };
                continue;
            }

            if (!inSection || block is not ListBlock list) continue;

            foreach (var item in list.OfType<ListItemBlock>())
            {
                if (item.OfType<ParagraphBlock>().FirstOrDefault()?.Inline is not { } inline) continue;
                if (OpeningDate(Md.PlainText(inline)) is { } date) yield return (date, item.Line + 1);
            }
        }
    }

    public static string? OpeningDate(string text)
    {
        var colon = text.IndexOf(':', StringComparison.Ordinal);
        if (colon < 0) return null;

        var date = text[..colon].Trim();
        return DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
            ? date
            : null;
    }
}
