using YamlDotNet.RepresentationModel;

namespace kac.core;

public sealed class ReportStale : ICorpusRule
{
    public RuleId RuleId => new("report-stale");

    private static readonly CheckId Stale = new("report-stale");

    public IReadOnlyList<CheckId> Emits => [Stale];

    private const string Field = "sources";
    private const string Resource = "resource";
    private const string Version = "version";

    public void Check(CorpusRuleContext ctx)
    {
        foreach (var doc in ctx.Records)
        foreach (var (resource, version, line) in Sources(doc))
        {
            if (!ctx.Versions.TryGetValue(resource, out var now)) continue;
            if (!VersionRange.Newer(now, version)) continue;

            ctx.Warn(doc, Stale,
                $"this answers for '{resource}' at {version}, and the corpus is at {now}. Check the "
                + "report still holds and raise the version, or run it again.", line);
        }
    }

    private static IEnumerable<(string Resource, string Version, int? Line)> Sources(Doc doc)
    {
        if (doc.Front is null) yield break;
        if (Yaml.Get(doc.Front, Field) is not YamlSequenceNode entries) yield break;

        foreach (var item in entries.Children)
        {
            if (item is not YamlMappingNode map) continue;
            if (Yaml.Get(map, Resource) is not YamlScalarNode { Value: { Length: > 0 } resource }) continue;
            if (Yaml.Get(map, Version) is not YamlScalarNode { Value: { Length: > 0 } version }) continue;

            yield return (resource, version, Yaml.LineOf(item, doc.FrontStartLine));
        }
    }
}
