using System.Text.RegularExpressions;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace kac.core;

// A glossary says what a word means. An obligation written into an entry is addressed by nothing: no
// control cites it, and a reader looking for the rule opens the standards. ISO/IEC Directives, Part 2,
// clause 16.5.6 states the rule. The message names the entry, which is why this is a class, where
// `not-normative` reports the same fault over a whole explanation.
//
// Bold counts and plain capitals do not. Bold is what binds under BCP 14, and a glossary is the one
// place in the corpus that defines a keyword: `Standard` names `MUST` in the course of saying what a
// standard is.
//
// The whole entry is read, because an obligation under `Not:` is as misplaced as one above it.
//
// Reported as a warning, per bold run, because the rule infers a requirement from a keyword.
public sealed partial class EntriesStateNoRequirement : IDocumentRule
{
    public RuleId RuleId => new("entries-state-no-requirement");

    private static readonly CheckId Reports = new("entry-requirement");

    public IReadOnlyList<CheckId> Emits => [Reports];

    public void Check(RuleContext ctx)
    {
        foreach (var (part, modal) in Requirements(ctx.Doc))
            ctx.Report.Warn(Reports,
                $"'{Md.Snippet(part.Text)}' writes a bold '{modal}', and a definition states no requirement. "
                + "Move the obligation into a standard and link to it from here.",
                part.Line);
    }

    // Every bold run inside an entry that names a BCP 14 keyword, with the entry it sits in. The parts
    // are the type's own reading, so a keyword written in `Scope` sits inside no entry and is passed
    // over.
    private static IEnumerable<(PartRow Part, string Modal)> Requirements(Doc d)
    {
        foreach (var bold in d.Ast.Descendants<EmphasisInline>())
        {
            if (bold.DelimiterCount != 2) continue;
            if (ModalRegex().Match(Md.PlainText(bold)) is not { Success: true } m) continue;

            if (d.Parts.FirstOrDefault(p => p.Contains(bold.Span)) is { } part) yield return (part, m.Value);
        }
    }

    // The two-word keywords first, so a `MUST NOT` is never reported as a `MUST`.
    [GeneratedRegex(@"\b(MUST NOT|SHOULD NOT|MUST|SHOULD|MAY)\b", RegexOptions.CultureInvariant)]
    private static partial Regex ModalRegex();
}
