using kac.core;

// A corpus moves its `content-version` in the pull request that changes what the corpus knows.
// `validate` cannot see that: the stamp and the records are each correct on their own, and only the two
// revisions together show one moving without the other. So this reads the diff, as `VerificationTests`
// does.
//
// The reach is a record of a type declaring an `export:` block, in a corpus under `examples/`. An export
// ships records, so a framework page, a README and a `.schema/` file are all outside it. Whether the move
// is the right size is a judgement, and `std-VERS` is where a reviewer reads what each part means.

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public class ContentVersionTests
{
    private const string Descriptor = ".corpus.yaml";

    private const string Stamp = "content-version";

    [Fact]
    public void A_corpus_whose_records_changed_has_moved_its_content_version()
    {
        var before = Diff.Against();
        if (before is null) return;

        var exported = Schema.Load(Repo.Root).ByFolder
            .Where(pair => pair.Value.Export is not null)
            .Select(pair => pair.Key)
            .ToHashSet(StringComparer.Ordinal);

        // Read off the schema and off the tree, so a type that starts exporting and a corpus added under
        // `examples/` are both covered with nothing to remember. Reading none of either is this guard
        // having gone quiet, and green is what it would report from then on.
        Assert.True(exported.Count > 0,
            "no type declares an 'export:' block, so this guard is checking nothing. Either the key was "
            + "renamed, or the types declaring it have gone.");

        var corpora = Corpora();

        Assert.True(corpora.Count > 0,
            $"no folder under '{ExportedRecord.Published}/' holds a {Descriptor}, so this guard is "
            + "checking nothing. Either the corpora moved, or the descriptor was renamed.");

        var changed = new SortedDictionary<string, SortedSet<string>>(StringComparer.Ordinal);

        // Every status, because adding a record, rewriting one, renaming one and deleting one each change
        // what the export publishes. Only a rename gives two paths, and they sit in different corpora
        // where a record moved between them.
        var touched = Diff.Changed(before, "AMRD")
            .SelectMany(file => new[] { file.Was, file.Now })
            .Concat(Diff.Untracked())
            .Distinct(StringComparer.Ordinal);

        foreach (var path in touched)
        {
            if (ExportedRecord.CorpusOf(path, corpora, exported) is not { } corpus) continue;

            if (!changed.TryGetValue(corpus, out var records))
                changed[corpus] = records = new SortedSet<string>(StringComparer.Ordinal);

            records.Add(path);
        }

        var unmoved = changed
            .Where(pair => Unmoved(pair.Key, before))
            .Select(pair => $"{ExportedRecord.Published}/{pair.Key} at {Version(Now(pair.Key))}\n      "
                            + string.Join("\n      ", pair.Value))
            .ToList();

        Assert.True(unmoved.Count == 0,
            $"these corpora changed a record and left {Stamp} where it was:\n  "
            + string.Join("\n  ", unmoved)
            + $"\nMove {Stamp} in each {Descriptor}. std-VERS says which part of it a change moves.");
    }

    // Each corpus under `examples/`, by folder name.
    private static IReadOnlySet<string> Corpora() =>
        Directory
            .EnumerateDirectories(Path.Combine(Repo.Root, ExportedRecord.Published))
            .Where(dir => File.Exists(Path.Combine(dir, Descriptor)))
            .Select(dir => new DirectoryInfo(dir).Name)
            .ToHashSet(StringComparer.Ordinal);

    // Whether this corpus states the same stamp now as it did at `before`. A corpus this branch created
    // has no descriptor at `before`, so there is nothing to compare and its first stamp stands.
    private static bool Unmoved(string corpus, string before)
    {
        var path = $"{ExportedRecord.Published}/{corpus}/{Descriptor}";

        return Git.Run(Repo.Root, $"show {before}:{path}") is { } then
               && Version(then) == Version(Now(corpus));
    }

    private static string Now(string corpus) =>
        Files.ReadLf(Path.Combine(Repo.Root, ExportedRecord.Published, corpus, Descriptor));

    // Null where the descriptor states no stamp. Every corpus under `examples/` states one, and `pack`
    // refuses a corpus that does not.
    private static string? Version(string descriptor) =>
        Yaml.Str(Yaml.Get(Yaml.Load(descriptor), Stamp));
}
