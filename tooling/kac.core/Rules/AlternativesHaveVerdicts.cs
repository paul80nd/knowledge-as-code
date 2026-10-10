using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace kac.core;

public sealed class AlternativesHaveVerdicts : IDocumentRule
{
    public RuleId RuleId => new("alternatives-have-verdicts");

    private static readonly CheckId Reports = new("alternatives-verdict");

    public IReadOnlyList<CheckId> Emits => [Reports];

    public void Check(RuleContext ctx)
    {
        foreach (var (text, line) in Bullets(ctx.Doc))
            if (!HasVerdict(text))
                ctx.Report.Warn(Reports,
                    $"Alternatives Considered bullet has no verdict: \"{Md.Snippet(text)}\".", line);
    }

    private static IEnumerable<(string text, int line)> Bullets(Doc d)
    {
        var inSection = false;
        foreach (var block in d.Ast)
        {
            if (block is HeadingBlock h)
            {
                inSection = h.Level switch
                {
                    2 => string.Equals(Md.PlainText(h.Inline), "Alternatives Considered",
                        StringComparison.OrdinalIgnoreCase),
                    < 2 => false,
                    _ => inSection
                };
                continue;
            }

            if (inSection && block is ListBlock list)
                foreach (var item in list)
                    if (item is ListItemBlock li)
                    {
                        var text = string.Join(" ", li.Descendants<LiteralInline>().Select(x => x.Content.ToString()));
                        yield return (text, li.Line + 1);
                    }
        }
    }

    public static bool HasVerdict(string text)
    {
        var t = text.ToLowerInvariant();
        string[] markers =
        [
            "reject", "accept", "chosen", "choose", "defer", "declined", "discarded", "adopted",
            "not for this adr", "no real alternative", "not adopted", "not pursued", "not chosen",
            "not relevant", "not worth", "no need", "unnecessary", "ruled out", "set aside",
            "we use", "instead", "however", "but ", "overkill", "heavier", "too ", "revisit"
        ];
        return markers.Any(t.Contains);
    }
}
