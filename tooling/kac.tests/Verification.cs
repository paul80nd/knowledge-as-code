using kac.core;
using YamlDotNet.RepresentationModel;

// Two versions of one record, and whether the prose moved without anybody reading it. Apart from
// `VerificationTests`, which reads the repository, so the comparison is provable on texts a test writes.

namespace kac.tests;

internal static class Verification
{
    // Whether the body differs and the `verified` list gained no entry. False where either side is not a
    // record, which is a file the validator reports on its own terms.
    internal static bool Unverified(string was, string now)
    {
        if (Parse(was) is not { } before || Parse(now) is not { } after) return false;

        var verified = Verifications(before).ToList();

        // A record nobody had verified leaves nothing behind when its prose moves. A `fix` is written
        // `draft` for exactly that, and the schema asks for the list on every other status.
        if (verified.Count == 0) return false;

        return Body(before) != Body(after) && !Verifications(after).Except(verified).Any();
    }

    // No type is resolved, and none is needed: the body and the `verified` list are read from the
    // document itself, and neither reading asks the schema anything.
    private static Doc? Parse(string text) => Doc.Parse("record.md", text, new Schema());

    private static string Body(Doc doc) => doc.Text[doc.BodyStart..];

    // Each verification as the pair that identifies it, so an entry edited in place reads as a new one
    // and an entry the author only reordered does not.
    private static IEnumerable<string> Verifications(Doc doc) =>
        doc.FrontNode("verified") is YamlSequenceNode entries
            ? entries.Children.OfType<YamlMappingNode>()
                .Select(entry => $"{Scalar(entry, "at")}|{Scalar(entry, "by")}")
            : [];

    private static string? Scalar(YamlMappingNode entry, string key) =>
        (Yaml.Get(entry, key) as YamlScalarNode)?.Value;
}
