using kac.core;

// `validate` sees one revision, so this reads the diff, as `VerificationTests` does. `ctl-0011` names
// where its reach differs from `std-VERS`.

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

        // An empty set is this guard gone quiet, and green is what it would report from then on.
        Assert.True(exported.Count > 0,
            "no type declares an 'export:' block, so this guard is checking nothing. Either the key was "
            + "renamed, or the types declaring it have gone.");

        Assert.True(corpora.Count > 0,
            $"no folder under '{RecordPath.Published}/' keeps a {Descriptor}, so this guard is checking "
            + "nothing. Either the corpora moved, or the descriptor was renamed.");

        var changed = new SortedDictionary<string, SortedSet<string>>(StringComparer.Ordinal);

        // Every status, because adding, rewriting, renaming and deleting a record each change what the export
        // publishes.
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

    private static IReadOnlySet<string> Corpora() =>
        Directory
            .EnumerateDirectories(Path.Combine(Repo.Root, RecordPath.Published))
            .Where(dir => File.Exists(Path.Combine(dir, Descriptor)))
            .Select(dir => new DirectoryInfo(dir).Name)
            .ToHashSet(StringComparer.Ordinal);

    /// <summary>
    /// A stamp edited downwards has not risen. A corpus this branch created has no descriptor at <c>before</c>, so its
    /// first stamp stands.
    /// </summary>
    private static bool Rose(string corpus, string before) =>
        Git.Run(Repo.Root, $"show {before}:{RecordPath.Published}/{corpus}/{Descriptor}") is not { } then
        || VersionRange.Newer(Stated(corpus) ?? "", Version(then) ?? "");

    /// <summary>Null where the descriptor states none.</summary>
    private static string? Stated(string corpus) =>
        Version(Files.ReadLf(Path.Combine(Repo.Root, RecordPath.Published, corpus, Descriptor)));

    private static string? Version(string descriptor) =>
        Yaml.Str(Yaml.Get(Yaml.Load(descriptor), Stamp));
}
