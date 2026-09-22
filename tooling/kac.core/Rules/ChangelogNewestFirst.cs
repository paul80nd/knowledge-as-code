using System.Globalization;
using Markdig.Syntax;

namespace kac.core;

// A changelog is read from the top, so the newest entry sits there. The order goes wrong one entry at a
// time: somebody appends the change they just made to the foot of the section. The message states the date
// that moved and the first entry it should sit above, which is the line the author edits, and naming that
// line is what the expression grammar cannot do.
//
// Dates compare as ISO strings, which is exact for the one form an entry may open on. A bullet opening on
// anything else has no date to compare and is skipped.
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

            // The topmost entry this one is newer than. Scanning from the top rather than reporting the
            // neighbour is what makes the message name the line the entry moves to.
            var above = entries.First(e => string.CompareOrdinal(date, e.Date) > 0).Date;
            ctx.Report.Warn(Reports,
                $"changelog entry '{date}' is out of order: it belongs above '{above}'.", line);
        }
    }

    // The date opening each bullet directly under `## Changelog`. A nested list is one entry's workings
    // and states no date of its own, so only the section's own items are read.
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

    // An entry reads `- <date>: what changed.`, so the date is the run before the first colon. Anything
    // else is not an entry this rule can compare, and returns null.
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
