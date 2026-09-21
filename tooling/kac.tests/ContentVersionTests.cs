using kac.core;

// A corpus moves its `content-version` in the pull request that changes what the corpus knows.
// `validate` cannot see that: the stamp and the records are each correct on their own, and only the two
// revisions together show one moving without the other. So this reads the diff, as `VerificationTests`
// does.
//
// The reach is a whole record of a type declaring an `export:` block, in a corpus under `examples/`. It
// is wider than the obligation in one direction and narrower in another, and `ctl-0011` names both:
// `std-VERS` exempts a change confined to a section the export leaves out, and it asks for a move where
// a `.schema/` edit or a bundled skill reaches a record.

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

        var corpora = Corpora();

        // Both sets are read off the schema and the tree, so a type that stops exporting and a corpus
        // added under `examples/` are covered with nothing to remember. An empty one is this guard gone
        // quiet, and green is what it would report from then on.
        Assert.True(exported.Count > 0,
            "no type declares an 'export:' block, so this guard is checking nothing. Either the key was "
            + "renamed, or the types declaring it have gone.");

        Assert.True(corpora.Count > 0,
            $"no folder under '{RecordPath.Published}/' keeps a {Descriptor}, so this guard is checking "
            + "nothing. Either the corpora moved, or the descriptor was renamed.");

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
            if (RecordPath.CorpusOf(path, corpora, exported) is not { } corpus) continue;

            if (!changed.TryGetValue(corpus, out var records))
                changed[corpus] = records = new SortedSet<string>(StringComparer.Ordinal);

            records.Add(path);
        }

        var unmoved = changed
            .Where(pair => !Rose(pair.Key, before))
            .Select(pair => $"{RecordPath.Published}/{pair.Key} at {Stated(pair.Key)}\n      "
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
            .EnumerateDirectories(Path.Combine(Repo.Root, RecordPath.Published))
            .Where(dir => File.Exists(Path.Combine(dir, Descriptor)))
            .Select(dir => new DirectoryInfo(dir).Name)
            .ToHashSet(StringComparer.Ordinal);

    // Whether this corpus states a higher stamp than it did at `before`. A stamp edited downwards is
    // reported alongside one nobody touched, because a registry orders what it is given and a consumer
    // resolving a range would take the older content. A corpus this branch created has no descriptor at
    // `before`, so there is nothing to compare and its first stamp stands.
    private static bool Rose(string corpus, string before) =>
        Git.Run(Repo.Root, $"show {before}:{RecordPath.Published}/{corpus}/{Descriptor}") is not { } then
        || VersionRange.Newer(Stated(corpus) ?? "", Version(then) ?? "");

    // The stamp this corpus states now. Null where the descriptor states none, which `pack` refuses and
    // no corpus under `examples/` does.
    private static string? Stated(string corpus) =>
        Version(Files.ReadLf(Path.Combine(Repo.Root, RecordPath.Published, corpus, Descriptor)));

    private static string? Version(string descriptor) =>
        Yaml.Str(Yaml.Get(Yaml.Load(descriptor), Stamp));
}
