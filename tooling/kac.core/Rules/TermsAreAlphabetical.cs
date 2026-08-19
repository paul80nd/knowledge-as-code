using Markdig.Syntax;

namespace kac.core;

// A glossary is read end to end and grepped, and both depend on the entries being in one order. The
// order goes wrong an entry at a time: somebody adds a word at the foot of the file rather than in its
// place. So the message names the entry that moved and the one it belongs before.
//
// That naming is why the rule is in C# rather than an `expr:`. The grammar holds no collections, and a
// rule saying only that the file is unsorted leaves the author to find the word themselves.
//
// Compared case-insensitively, because casing is the entry's own — `ADR` and `Borrower` sit in the
// order a reader scans, not the order their code points fall in.
public sealed class TermsAreAlphabetical : IDocumentRule
{
    public RuleId RuleId => new("terms-are-alphabetical");

    // A field because every report below names it: one place to change, so what the rule declares it
    // emits and what it actually reports cannot come apart. `_checks.yaml` declares what it means.
    private static readonly CheckId Reports = new("terms-alphabetical");

    public IReadOnlyList<CheckId> Emits => [Reports];

    public void Check(RuleContext ctx)
    {
        string? previous = null;

        foreach (var (term, line) in Entries(ctx.Doc))
        {
            if (previous is not null && string.Compare(term, previous, StringComparison.OrdinalIgnoreCase) < 0)
                ctx.Report.Warn(Reports,
                    $"'{Md.Snippet(term)}' is out of order — it belongs before '{Md.Snippet(previous)}'.", line);

            previous = term;
        }
    }

    // Every H3 in the document, which is one entry each. Not narrowed to the `Terms` section: an H3
    // outside it is an entry somebody filed outside the list, and reporting its position says so where
    // skipping it would leave the entry unmentioned.
    private static IEnumerable<(string Term, int Line)> Entries(Doc d)
    {
        foreach (var block in d.Ast)
            if (block is HeadingBlock { Level: 3 } h)
                yield return (Md.PlainText(h.Inline), h.Line + 1);
    }
}
