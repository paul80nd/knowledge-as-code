using YamlDotNet.RepresentationModel;

namespace kac.core;

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

        if (doc.FrontScalar(Duration) is not { Length: > 0 } stated) return;
        // A `restored-at` before `occurred-at` gives no span, and is `restored-not-before-occurred`'s to report.
        if (Instants.Span(doc.FrontScalar(Occurred), doc.FrontScalar(Restored)) is not { Length: > 0 } span) return;
        if (string.Equals(stated, span, StringComparison.Ordinal)) return;

        ctx.Report.Err(Mismatched,
            $"'{Duration}' is '{stated}', and '{Occurred}' to '{Restored}' is '{span}'. Write '{span}', "
            + "or correct whichever of the two moments is wrong.",
            Yaml.LineOf(Yaml.Get(doc.Front, Duration) ?? doc.Front, doc.FrontStartLine));
    }
}
