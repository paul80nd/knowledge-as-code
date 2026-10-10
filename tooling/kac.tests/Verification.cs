using System.Text.RegularExpressions;
using kac.core;
using YamlDotNet.RepresentationModel;

namespace kac.tests;

internal static partial class Verification
{
    /// <summary>
    /// Whether the body differs and nobody answered for it. False where either side is not a record, which is a file
    /// the validator reports on its own terms.
    /// </summary>
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

    /// <summary>
    /// No type is resolved, and none is needed: the body and the <c>verified</c> list are read from the document
    /// itself, and neither reading asks the schema anything.
    /// </summary>
    private static Doc? Parse(string text) => Doc.Parse("record.md", text, new Schema());

    /// <summary>
    /// The prose, with every run of whitespace read as one space. Rider rewraps markdown in this repository from
    /// <c>.editorconfig</c>, in files a session never opened, and a reflow moves no word. A byte-exact comparison would
    /// ask somebody to read text again that says what it said before.
    /// </summary>
    private static string Body(Doc doc) => Whitespace().Replace(doc.Text[doc.BodyStart..], " ").Trim();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    /// <summary>
    /// Each verification as the pair that identifies it, so an entry edited in place reads as a new one and an entry
    /// the author only reordered does not.
    /// </summary>
    private static IEnumerable<string> Verifications(Doc doc) =>
        doc.FrontNode("verified") is YamlSequenceNode entries
            ? entries.Children.OfType<YamlMappingNode>()
                .Select(entry => $"{Scalar(entry, "at")}|{Scalar(entry, "by")}")
            : [];

    /// <summary>
    /// Who wrote the content and when, as the pair that identifies one generation. Null where the document states no
    /// <c>generated</c>, which is every type but <c>reports</c>.
    /// </summary>
    private static string? Generation(Doc doc) =>
        doc.FrontNode("generated") is YamlMappingNode entry
            ? $"{Scalar(entry, "at")}|{Scalar(entry, "by")}"
            : null;

    private static string? Scalar(YamlMappingNode entry, string key) =>
        (Yaml.Get(entry, key) as YamlScalarNode)?.Value;
}
