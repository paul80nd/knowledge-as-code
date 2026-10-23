using kac.core;

// `validate` sees one revision, so this compares two exports, as `VerificationTests` compares two records. `ctl-0011`
// names where its reach differs from `std-VERS`.

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public class ContentVersionTests
{
    private const string Descriptor = ".corpus.yaml";

    private const string Stamp = "content-version";

    private const string SchemaDir = ".schema";

    /// <summary>A failure lists this many moved export files per corpus, and counts the rest.</summary>
    private const int ListedPerCorpus = 5;

    [Fact]
    public void A_corpus_whose_export_changed_has_moved_its_content_version()
    {
        var before = Diff.Against();
        if (before is null) return;

        var corpora = Corpora();

        // An empty set is this guard gone quiet, and green is what it would report from then on.
        Assert.True(corpora.Count > 0,
            $"no folder under '{RecordPath.Published}/' keeps a {Descriptor}, so this guard is checking "
            + "nothing. Either the corpora moved, or the descriptor was renamed.");

        // Every status, because adding, rewriting, renaming and deleting a file can each change an export.
        var touched = Diff.Changed(before, "AMRD")
            .SelectMany(file => new[] { file.Was, file.Now })
            .Concat(Diff.Untracked())
            .ToList();

        var schemaMoved = touched.Any(path => path.StartsWith(SchemaDir + "/", StringComparison.Ordinal));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var unmoved = new List<string>();

        foreach (var corpus in corpora.Order(StringComparer.Ordinal))
        {
            var folder = $"{RecordPath.Published}/{corpus}";

            // Identical inputs to one exporter write identical bytes, so a corpus nothing touched is not built.
            if (!schemaMoved && !touched.Any(path => path.StartsWith(folder + "/", StringComparison.Ordinal)))
                continue;

            // A corpus this branch created has no export to compare, and its first stamp stands.
            if (Git.Run(Repo.Root, "show", $"{before}:{folder}/{Descriptor}") is not { } then) continue;

            var moved = PublishedExport.Moved(Committed(before, folder), Working(folder), today);

            // A stamp edited downwards has not risen.
            if (moved.Count == 0 || VersionRange.Newer(Stated(folder) ?? "", Version(then) ?? "")) continue;

            unmoved.Add($"{folder} at {Stated(folder)}\n      "
                        + string.Join("\n      ", moved.Take(ListedPerCorpus))
                        + (moved.Count > ListedPerCorpus ? $"\n      and {moved.Count - ListedPerCorpus} more" : ""));
        }

        Assert.True(unmoved.Count == 0,
            $"these corpora changed what their export publishes and left {Stamp} where it was:\n  "
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
    /// <c>folder</c> as git stored it at <c>commit</c>, with the <c>.schema/</c> of that commit. Each file is read when
    /// the load asks for it, one <c>git show</c> apiece.
    /// </summary>
    private static Revision Committed(string commit, string folder)
    {
        var prefix = folder + "/";
        var paths = Listed("ls-tree", "-r", "-z", "--name-only", commit, "--", prefix)
            .Where(path => path.StartsWith(prefix, StringComparison.Ordinal))
            .Select(path => path[prefix.Length..])
            .ToHashSet(StringComparer.Ordinal);

        var schema = Listed("ls-tree", "-z", "--name-only", commit, "--", SchemaDir + "/")
            .Where(path => path.EndsWith(".yaml", StringComparison.Ordinal))
            .ToDictionary(path => path[(SchemaDir.Length + 1)..], path => Show(commit, path),
                StringComparer.Ordinal);

        return new Revision($"{folder} at {commit}", new Tree(paths, rel => Show(commit, prefix + rel)), schema);
    }

    /// <summary>
    /// <c>folder</c> as the working tree has it, listed the way <c>kac</c> lists a corpus, with the working tree's
    /// <c>.schema/</c>.
    /// </summary>
    private static Revision Working(string folder)
    {
        var root = Path.Combine(Repo.Root, folder);
        var paths = (GitFiles.Tracked(root)
                     ?? throw new InvalidOperationException($"git could not list {folder}, so nothing was compared."))
            .Select(path => path.Replace('\\', '/'))
            .ToHashSet(StringComparer.Ordinal);

        var schema = Directory.GetFiles(Path.Combine(Repo.Root, SchemaDir), "*.yaml")
            .ToDictionary(file => Path.GetFileName(file), File.ReadAllText, StringComparer.Ordinal);

        return new Revision($"{folder} in the working tree",
            new Tree(paths, rel => Files.ReadLf(Path.Combine(root, rel))), schema);
    }

    private static IEnumerable<string> Listed(params string[] args) =>
        Asked(args).Split('\0', StringSplitOptions.RemoveEmptyEntries);

    /// <summary>Line endings normalised, as <c>Files.ReadLf</c> reads the working tree.</summary>
    private static string Show(string commit, string path) => Asked("show", $"{commit}:{path}").Replace("\r\n", "\n");

    /// <exception cref="InvalidOperationException">Where git could not answer.</exception>
    private static string Asked(params string[] args) =>
        Git.Run(Repo.Root, args)
        ?? throw new InvalidOperationException($"git {string.Join(' ', args)} failed, so nothing was compared.");

    /// <summary>Null where the descriptor states none.</summary>
    private static string? Stated(string folder) =>
        Version(Files.ReadLf(Path.Combine(Repo.Root, folder, Descriptor)));

    private static string? Version(string descriptor) =>
        Yaml.Str(Yaml.Get(Yaml.Load(descriptor), Stamp));
}
