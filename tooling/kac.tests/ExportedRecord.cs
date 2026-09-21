// One changed path, and which corpus's export it belongs to. Apart from `ContentVersionTests`, which
// reads the repository, so the decision is provable on paths a test writes.

namespace kac.tests;

internal static class ExportedRecord
{
    // The folder under this repository's root that holds the corpora it publishes. `publish-corpus.yml`
    // pushes each of them to a registry. `template/` states no `content-version`, and a fixture corpus
    // under `tooling/` states one as part of the fixture, so neither tree is read here.
    internal const string Published = "examples";

    // The corpus whose export includes `rel`, or null where nothing does. `corpora` is the folder name of
    // each corpus under `examples/`, and `exported` is the folder name of each type declaring an
    // `export:` block.
    //
    // The type folder is looked for anywhere between the corpus root and the file, because a corpus may
    // file its records in subfolders and a type reads them all. `_index.md` is generated and `_template.md`
    // is a shape rather than a record, and an export ships neither.
    internal static string? CorpusOf(string rel, IReadOnlySet<string> corpora, IReadOnlySet<string> exported)
    {
        var parts = rel.Split('/');

        if (parts.Length < 4 || parts[0] != Published || !corpora.Contains(parts[1])) return null;
        if (!rel.EndsWith(".md", StringComparison.Ordinal) || parts[^1].StartsWith('_')) return null;

        return parts[2..^1].Any(exported.Contains) ? parts[1] : null;
    }
}
