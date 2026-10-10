namespace kac.tests;

internal static class RecordPath
{
    /// <summary>
    /// The folder holding the corpora this repository publishes. <c>template/</c> states no <c>content-version</c>, and
    /// a fixture corpus under <c>tooling/</c> states one as part of the fixture, so neither is read here.
    /// </summary>
    internal const string Published = "examples";

    /// <summary>
    /// The type folder is looked for anywhere above the file, because a corpus may file records in subfolders.
    /// <c>_index.md</c> and <c>_template.md</c> are not records.
    /// </summary>
    internal static bool IsRecord(string rel, IReadOnlySet<string> folders)
    {
        var parts = rel.Split('/');

        return rel.EndsWith(".md", StringComparison.Ordinal)
               && !parts[^1].StartsWith('_')
               && parts[..^1].Any(folders.Contains);
    }

    /// <summary>
    /// The corpus whose export includes <c>rel</c>, or null where none does. <c>corpora</c> names each corpus folder
    /// under <c>examples/</c>, and <c>exported</c> each type folder declaring an <c>export:</c> block.
    /// </summary>
    internal static string? CorpusOf(string rel, IReadOnlySet<string> corpora, IReadOnlySet<string> exported)
    {
        var parts = rel.Split('/');

        if (parts.Length < 4 || parts[0] != Published || !corpora.Contains(parts[1])) return null;

        return IsRecord(string.Join('/', parts[2..]), exported) ? parts[1] : null;
    }
}
