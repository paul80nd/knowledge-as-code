using YamlDotNet.RepresentationModel;

namespace kac.core;

public sealed class NoSelfVerification : IDocumentRule
{
    public RuleId RuleId => new("no-self-verification");

    private static readonly CheckId SelfVerified = new("self-verification");

    public IReadOnlyList<CheckId> Emits => [SelfVerified];

    private const string Generated = "generated";
    private const string Verified = "verified";
    private const string By = "by";

    public void Check(RuleContext ctx)
    {
        var doc = ctx.Doc;
        if (doc.Front is null) return;

        if (Yaml.Get(doc.Front, Generated) is not YamlMappingNode generated) return;
        if (Yaml.Get(generated, By) is not YamlScalarNode { Value: { Length: > 0 } author }) return;

        // A single mapping is the one-entry list, as `Exporter.Trust` reads it, so the two readings of the field agree.
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
