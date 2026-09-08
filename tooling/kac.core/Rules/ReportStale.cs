using YamlDotNet.RepresentationModel;

namespace kac.core;

// A report answers for a version of the corpus, and the corpus moves on without it.
//
// Every other record is about the estate, so a corpus that changed leaves it as true as it was. A
// report is about the corpus, so the same change can make it wrong, and nothing in the record itself
// shows that: the numbers still read as numbers and the verdicts still read as verdicts.
//
// So `sources` names the version each corpus was at when the report was true of it, and this holds
// that against the version in front of the reader now. A warning rather than an error, because the
// report may well still hold: what a reader does about it is confirm it and raise the version, or run
// it again, and only they can tell which.
//
// A corpus rule rather than a document rule, because the answer is not in the record. It is the
// descriptor's `content-version` and the version each consumed corpus resolved to, which
// `CorpusRuleContext.Versions` carries.
public sealed class ReportStale : ICorpusRule
{
    public RuleId RuleId => new("report-stale");

    private static readonly CheckId Stale = new("report-stale");

    public IReadOnlyList<CheckId> Emits => [Stale];

    // The field naming what the record answers for, and the two keys of one entry. Named here rather
    // than read from the type, because no declaration says which field holds a provenance source: the
    // type declares a list of objects, and only this rule knows what the pair means.
    private const string Field = "sources";
    private const string Resource = "resource";
    private const string Version = "version";

    public void Check(CorpusRuleContext ctx)
    {
        foreach (var doc in ctx.Records)
        foreach (var (resource, version, line) in Sources(doc))
        {
            // A resource this corpus does not read is a comparison that cannot be made rather than one
            // that failed. A report carried in from elsewhere names corpora this one never consumed.
            if (!ctx.Versions.TryGetValue(resource, out var now)) continue;
            if (!VersionRange.Newer(now, version)) continue;

            ctx.Warn(doc, Stale,
                $"this answers for '{resource}' at {version}, and the corpus is at {now}. Check the "
                + "report still holds and raise the version, or run it again.", line);
        }
    }

    // Every source entry as the pair this rule compares, with the line it sits on. An entry short of
    // either key is skipped: what shape the field takes is the schema's to hold, and `entry-key` has
    // already said so against the entry itself.
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
