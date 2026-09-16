using YamlDotNet.RepresentationModel;

namespace kac.core;

// A report is verified by anybody who read its verdicts, and by anything that read them except whoever
// wrote them.
//
// Accepting any actor is what lets an agent's check count for something. It also opens the one hole the
// old person-only rule was closing, which is an author signing off its own writing. So the comparison is
// against `generated.by` alone, and every other actor is admitted.
//
// In C# rather than in an `expr:`, because the message worth reading names the entry at fault. The
// grammar compares a list's entries against a pattern and never against another field, so an expression
// could report only that something in the list is wrong.
public sealed class NoSelfVerification : IDocumentRule
{
    public RuleId RuleId => new("no-self-verification");

    private static readonly CheckId SelfVerified = new("self-verification");

    public IReadOnlyList<CheckId> Emits => [SelfVerified];

    // The field naming who wrote the content, the field holding the history, and the key each carries
    // the actor under. Named here rather than read from the type: the type declares two fields of actors,
    // and only this rule knows that one of the two may not be the other.
    private const string Generated = "generated";
    private const string Verified = "verified";
    private const string By = "by";

    public void Check(RuleContext ctx)
    {
        var doc = ctx.Doc;
        if (doc.Front is null) return;

        // A report short of either field is one `required-field` has already reported, in better words.
        if (Yaml.Get(doc.Front, Generated) is not YamlMappingNode generated) return;
        if (Yaml.Get(generated, By) is not YamlScalarNode { Value: { Length: > 0 } author }) return;

        // A list written as one mapping is the one-entry case, which is how `Exporter.Trust` reads it.
        // `list` refuses that shape and `validate` reports it, so a record writing it is already being
        // told; reading it the same way here keeps the two accounts of one field together.
        IEnumerable<YamlNode> entries = Yaml.Get(doc.Front, Verified) switch
        {
            YamlSequenceNode seq => seq.Children,
            YamlMappingNode map => [map],
            _ => []
        };

        foreach (var item in entries)
        {
            if (item is not YamlMappingNode entry) continue;
            if (Yaml.Get(entry, By) is not YamlScalarNode { Value: { } actor }) continue;
            if (!string.Equals(actor, author, StringComparison.Ordinal)) continue;

            ctx.Report.Err(SelfVerified,
                $"'{author}' wrote this report and verifies it here. Nobody confirms their own writing. "
                + "Delete the entry, or have somebody else read the verdicts and name them.",
                Yaml.LineOf(item, doc.FrontStartLine));
        }
    }
}
