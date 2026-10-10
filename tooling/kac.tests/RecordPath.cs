namespace kac.tests;

internal static class RecordPath
{
    // The folder holding the corpora this repository publishes. `template/` states no `content-version`,
    // and a fixture corpus under `tooling/` states one as part of the fixture, so neither is read here.
    internal const string Published = "examples";

    // The type folder is looked for anywhere above the file, because a corpus may file records in
    // subfolders. `_index.md` and `_template.md` are not records.
    internal static bool IsRecord(string rel, IReadOnlySet<string> folders)
    {
        var parts = rel.Split('/');

        return rel.EndsWith(".md", StringComparison.Ordinal)
               && !parts[^1].StartsWith('_')
               && parts[..^1].Any(folders.Contains);
    }

    // The corpus whose export includes `rel`, or null where none does. `corpora` names each corpus folder
    // under `examples/`, and `exported` each type folder declaring an `export:` block.
    internal static string? CorpusOf(string rel, IReadOnlySet<string> corpora, IReadOnlySet<string> exported)
    {
        var parts = rel.Split('/');

        if (parts.Length < 4 || parts[0] != Published || !corpora.Contains(parts[1])) return null;

        return IsRecord(string.Join('/', parts[2..]), exported) ? parts[1] : null;
    }
}
