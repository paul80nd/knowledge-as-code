using Markdig.Syntax;

namespace kac.core;

public sealed class TermsAreAlphabetical : IDocumentRule
{
    public RuleId RuleId => new("terms-are-alphabetical");

    private static readonly CheckId Reports = new("terms-alphabetical");

    public IReadOnlyList<CheckId> Emits => [Reports];

    public void Check(RuleContext ctx)
    {
        string? previous = null;

        foreach (var (term, line) in Entries(ctx.Doc))
        {
            if (previous is not null && string.Compare(term, previous, StringComparison.OrdinalIgnoreCase) < 0)
                ctx.Report.Warn(Reports,
                    $"'{Md.Snippet(term)}' is out of order: it belongs before '{Md.Snippet(previous)}'.", line);

            previous = term;
        }
    }

    // Every H3, not only those under `Terms`: an entry filed outside the list is reported for its position.
    private static IEnumerable<(string Term, int Line)> Entries(Doc d)
    {
        foreach (var block in d.Ast)
            if (block is HeadingBlock { Level: 3 } h)
                yield return (Md.PlainText(h.Inline), h.Line + 1);
    }
}
