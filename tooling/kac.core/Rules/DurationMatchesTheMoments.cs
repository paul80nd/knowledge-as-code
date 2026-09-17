using YamlDotNet.RepresentationModel;

namespace kac.core;

// `duration` restates a span two other fields already fix, so the two accounts can disagree and the
// record then states a length its own moments refuse.
//
// In C# rather than in an `expr:`, because the message worth reading carries the span the moments give.
// An expression compares two strings and reports that they differ, which leaves the author to subtract
// one timestamp from another by hand to find a value the tool already holds.
//
// Guarded on the order as well as on presence: a `restored-at` before `occurred-at` is one fault, and
// `restored-not-before-detected` is the rule that names it.
public sealed class DurationMatchesTheMoments : IDocumentRule
{
    public RuleId RuleId => new("duration-matches-the-moments");

    private static readonly CheckId Mismatched = new("duration-matches-the-moments");

    public IReadOnlyList<CheckId> Emits => [Mismatched];

    private const string Duration = "duration";
    private const string Occurred = "occurred-at";
    private const string Restored = "restored-at";

    public void Check(RuleContext ctx)
    {
        var doc = ctx.Doc;
        if (doc.Front is null) return;

        // A record short of any of the three is one `required-field` has already reported, and one
        // carrying a timestamp that is not a moment is `timestamp-format`'s.
        if (doc.FrontScalar(Duration) is not { Length: > 0 } stated) return;
        if (Facts.Between(doc.FrontScalar(Occurred), doc.FrontScalar(Restored)) is not { Length: > 0 } span) return;
        if (string.Equals(stated, span, StringComparison.Ordinal)) return;

        ctx.Report.Err(Mismatched,
            $"'{Duration}' is '{stated}', and '{Occurred}' to '{Restored}' is '{span}'. Write '{span}', "
            + "or correct whichever of the two moments is wrong.",
            Yaml.LineOf(Yaml.Get(doc.Front, Duration) ?? doc.Front, doc.FrontStartLine));
    }
}
