// What a path says about the file at the end of it: whether it is a record at all, and which corpus
// publishes it. Apart from the guards reading the repository, so each decision is provable on a path a
// test writes.

namespace kac.tests;

internal static class RecordPath
{
    // The folder under this repository's root holding the corpora it publishes. `publish-corpus.yml`
    // pushes each of them to a registry. `template/` states no `content-version`, and a fixture corpus
    // under `tooling/` states one as part of the fixture, so neither tree is read through `CorpusOf`.
    internal const string Published = "examples";

    // Whether `rel` is a record of one of `folders`. The type folder is looked for anywhere above the
    // file, because a corpus may file its records in subfolders and a type reads them all. `_index.md` is
    // generated and `_template.md` is a shape rather than a record, so neither counts.
    internal static bool IsRecord(string rel, IReadOnlySet<string> folders)
    {
        var parts = rel.Split('/');

        return rel.EndsWith(".md", StringComparison.Ordinal)
               && !parts[^1].StartsWith('_')
               && parts[..^1].Any(folders.Contains);
    }

    // The corpus whose export includes `rel`, or null where none does. `corpora` is the folder name of
    // each corpus under `examples/`, and `exported` the folder name of each type declaring an `export:`
    // block. The corpus is stripped off before the record test, so a corpus sharing a type's name cannot
    // pass for a type folder.
    internal static string? CorpusOf(string rel, IReadOnlySet<string> corpora, IReadOnlySet<string> exported)
    {
        var parts = rel.Split('/');

        if (parts.Length < 4 || parts[0] != Published || !corpora.Contains(parts[1])) return null;

        return IsRecord(string.Join('/', parts[2..]), exported) ? parts[1] : null;
    }
}
