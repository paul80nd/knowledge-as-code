using System.Text.RegularExpressions;
using kac.core;
using YamlDotNet.RepresentationModel;

// Two versions of one record, and whether the prose moved without anybody reading it. Apart from
// `VerificationTests`, which reads the repository, so the comparison is provable on texts a test writes.

namespace kac.tests;

internal static partial class Verification
{
    // Whether the body differs and nobody answered for it. False where either side is not a record, which
    // is a file the validator reports on its own terms.
    internal static bool Unverified(string was, string now)
    {
        if (Parse(was) is not { } before || Parse(now) is not { } after) return false;
        if (Body(before) == Body(after)) return false;

        var verified = Verifications(before).ToList();
        var reverified = Verifications(after).Except(verified).Any();

        // A record stating `generated` names who wrote the content as it stands, so whoever rewrote the
        // prose answers by writing themselves there. A verification answers as well, because a person
        // confirming text they did not write moves that list and not this line.
        if (Generation(before) is { } generated) return generated == Generation(after) && !reverified;

        // A record nobody had verified leaves nothing behind when its prose moves. A `fix` is written
        // `draft` for exactly that, and the schema asks for the list on every other status.
        return verified.Count > 0 && !reverified;
    }

    // No type is resolved, and none is needed: the body and the `verified` list are read from the
    // document itself, and neither reading asks the schema anything.
    private static Doc? Parse(string text) => Doc.Parse("record.md", text, new Schema());

    // The prose, with every run of whitespace read as one space. Rider rewraps markdown in this
    // repository from `.editorconfig`, in files a session never opened, and a reflow moves no word. A
    // byte-exact comparison would ask somebody to read text again that says what it said before.
    private static string Body(Doc doc) => Whitespace().Replace(doc.Text[doc.BodyStart..], " ").Trim();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    // Each verification as the pair that identifies it, so an entry edited in place reads as a new one
    // and an entry the author only reordered does not.
    private static IEnumerable<string> Verifications(Doc doc) =>
        doc.FrontNode("verified") is YamlSequenceNode entries
            ? entries.Children.OfType<YamlMappingNode>()
                .Select(entry => $"{Scalar(entry, "at")}|{Scalar(entry, "by")}")
            : [];

    // Who wrote the content and when, as the pair that identifies one generation. Null where the document
    // states no `generated`, which is every type but `reports`.
    private static string? Generation(Doc doc) =>
        doc.FrontNode("generated") is YamlMappingNode entry
            ? $"{Scalar(entry, "at")}|{Scalar(entry, "by")}"
            : null;

    private static string? Scalar(YamlMappingNode entry, string key) =>
        (Yaml.Get(entry, key) as YamlScalarNode)?.Value;
}
