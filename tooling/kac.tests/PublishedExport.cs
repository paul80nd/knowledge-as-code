using System.Text.Json.Nodes;
using kac.core;

namespace kac.tests;

/// <summary>
/// A corpus at one revision: its files relative to its own root, and the <c>.schema/*.yaml</c> above it by name.
/// <c>Name</c> is what a refusal reports.
/// </summary>
internal sealed record Revision(string Name, Tree Corpus, IReadOnlyDictionary<string, string> Schema);

/// <summary>
/// What a corpus publishes, built in-process by this build's <c>Exporter</c> whichever revision it reads, so a change to
/// <c>kac</c> alone moves no byte. <c>Moved</c> leaves inherited records out of both builds: <c>.imports/</c> is
/// untracked, so git keeps no base revision of them.
/// </summary>
internal static class PublishedExport
{
    /// <summary>Stands in for the commit a published link pins, which differs between any two revisions.</summary>
    private const string LinkRef = "ref";

    /// <summary>
    /// Each export path whose bytes differ between the two revisions, in ordinal order, with <c>mechanismVersion</c>
    /// left out of the manifest. A path one side writes and the other does not is included.
    /// </summary>
    /// <exception cref="InvalidOperationException">Where the exporter refuses either revision.</exception>
    internal static IReadOnlyList<string> Moved(Revision before, Revision after, DateOnly today)
    {
        var was = Files(before, today);
        var now = Files(after, today);

        return
        [
            .. was.Keys.Union(now.Keys)
                .Where(path => was.GetValueOrDefault(path) != now.GetValueOrDefault(path))
                .Order(StringComparer.Ordinal)
        ];
    }

    /// <summary>
    /// The export's files by path. The run states no timestamp, commit or dirty flag, so two calls over one revision
    /// return the same bytes.
    /// </summary>
    private static Dictionary<string, string> Files(Revision revision, DateOnly today) =>
        Built(revision, today).ToDictionary(file => file.Path,
            file => file.Path == Exporter.ManifestFile ? WithoutMechanismVersion(file.Content) : file.Content,
            StringComparer.Ordinal);

    /// <summary>
    /// The export's files as <c>kac export</c> would write them, with <c>inherited</c> merged in where it is given.
    /// </summary>
    /// <exception cref="InvalidOperationException">Where the exporter refuses the revision.</exception>
    internal static IReadOnlyList<ExportFile> Built(Revision revision, DateOnly today,
        IReadOnlyList<InheritedCorpus>? inherited = null)
    {
        var descriptor = CorpusDescriptor.Parse(
            revision.Corpus.Exists(Corpus.Descriptor) ? revision.Corpus.Read(Corpus.Descriptor) : "");

        var corpus = Corpus.Load(revision.Corpus, Schema.Load(revision.Schema, descriptor.Enums), descriptor);

        var plan = Exporter.Plan(corpus, Publishing.For(descriptor, LinkRef), null,
            new ExportRun("", today, null, null), inherited);

        if (plan.Refused.Count > 0)
            throw new InvalidOperationException(
                $"{revision.Name} cannot be exported, so its export was not compared:\n  "
                + string.Join("\n  ", plan.Refused));

        return plan.Files;
    }

    /// <summary>The manifest without <c>mechanismVersion</c>, which a template take moves in every corpus.</summary>
    private static string WithoutMechanismVersion(string manifest)
    {
        var node = JsonNode.Parse(manifest)?.AsObject()
                   ?? throw new InvalidOperationException($"{Exporter.ManifestFile} is not a JSON object.");

        node.Remove("mechanismVersion");

        return node.ToJsonString();
    }
}
