using System.Text.RegularExpressions;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace kac.core;

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
