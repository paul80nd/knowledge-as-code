using Markdig.Syntax;

namespace kac.core;

// A glossary is read end to end and grepped, and both depend on the entries being in one order. The
// order goes wrong an entry at a time — somebody adds a word at the foot of the file rather than in
// its place — so the message names the entry that is out of position and the one it should follow.
// That is why the rule is in C# rather than an `expr:`: the grammar holds no collections, and a rule
// that could only say "the terms are out of order" would leave the author to find which.
//
// Compared case-insensitively, because casing is the entry's own — `ADR` and `Borrower` sit in the
// order a reader scans, not the order their code points fall in.
public sealed class TermsAreAlphabetical : IDocumentRule
{
    public string RuleId => "terms-are-alphabetical";

    public IReadOnlyList<CheckDef> Emits =>
    [
        new("terms-alphabetical", Sev.Warning, "A glossary's entries are in alphabetical order.")
    ];

    public void Check(RuleContext ctx)
    {
        string? previous = null;

        foreach (var (term, line) in Entries(ctx.Doc))
        {
            if (previous is not null && string.Compare(term, previous, StringComparison.OrdinalIgnoreCase) < 0)
                ctx.Warn("terms-alphabetical",
                    $"'{Md.Snippet(term)}' is out of order — it belongs before '{Md.Snippet(previous)}'.", line);

            previous = term;
        }
    }

    // Every H3, which is what an entry is. Read across the whole document rather than within one
    // section: a glossary holds its terms under `Terms`, and an H3 anywhere else in one would be an
    // entry filed outside the list, which this reports as being out of order rather than ignoring.
    private static IEnumerable<(string Term, int Line)> Entries(Doc d)
    {
        foreach (var block in d.Ast)
            if (block is HeadingBlock { Level: 3 } h)
                yield return (Md.PlainText(h.Inline), h.Line + 1);
    }
}
