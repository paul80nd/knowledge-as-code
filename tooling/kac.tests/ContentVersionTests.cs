using System.Text;
using kac.core;

// `validate` sees one revision, so this compares two, as `VerificationTests` compares two records. `ctl-0011` names
// where its reach differs from `std-VERS`.

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

        var touched = Touched(before);

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

    [Fact]
    public void A_corpus_whose_resolved_lock_moved_has_moved_its_content_version()
    {
        var before = Diff.Against();
        if (before is null) return;

        var unmoved = new List<string>();

        foreach (var corpus in Corpora().Order(StringComparer.Ordinal))
        {
            var folder = $"{RecordPath.Published}/{corpus}";
            if (Git.Run(Repo.Root, "show", $"{before}:{folder}/{Descriptor}") is not { } then) continue;

            var now = Files.ReadLf(Path.Combine(Repo.Root, folder, Descriptor));
            var was = Locks(then);
            var locked = Locks(now);

            // An entry added or removed moves its lock from or to nothing.
            var moved = was.Keys.Union(locked.Keys)
                .Order(StringComparer.Ordinal)
                .Where(key => was.GetValueOrDefault(key) != locked.GetValueOrDefault(key))
                .Select(key => $"{key} from {was.GetValueOrDefault(key) ?? "nothing"} to "
                               + $"{locked.GetValueOrDefault(key) ?? "nothing"}")
                .ToList();

            if (moved.Count == 0 || VersionRange.Newer(Version(now) ?? "", Version(then) ?? "")) continue;

            unmoved.Add($"{folder} at {Version(now)}\n      " + string.Join("\n      ", moved));
        }

        Assert.True(unmoved.Count == 0,
            $"these corpora moved a resolved: lock under consumes: and left {Stamp} where it was:\n  "
            + string.Join("\n  ", unmoved)
            + $"\nMove {Stamp} in each {Descriptor} by the part its producer moved. std-VERS states the rule.");
    }

    [Fact]
    public void A_corpus_whose_bundle_takes_a_changed_component_has_moved_its_content_version()
    {
        var before = Diff.Against();
        if (before is null) return;

        var touched = Touched(before).ToHashSet(StringComparer.Ordinal);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var unmoved = new List<string>();

        foreach (var corpus in Corpora().Order(StringComparer.Ordinal))
        {
            var folder = $"{RecordPath.Published}/{corpus}";
            var root = Path.Combine(Repo.Root, folder);
            var from = CorpusDescriptor.Parse(Files.ReadLf(Path.Combine(root, Descriptor))).PluginFrom;

            var ownDir = $"{folder}/{Bundler.SourceDir}";
            var own = Bundler.Read(Path.Combine(root, Bundler.SourceDir)) ?? [];
            var sharedDir = from is null ? null : RepoRelative(Path.Combine(root, from));
            var shared = sharedDir is null ? [] : Bundler.Read(Path.Combine(Repo.Root, sharedDir)) ?? [];
            var tree = sharedDir is null ? own : Bundler.Merge(shared, own);

            // The merged tree keeps the very file it read, so a shared file the corpus overrides drops out here.
            var changed = Edited(touched, ownDir, own)
                .Concat(sharedDir is null ? [] : Edited(touched, sharedDir, shared))
                .Where(file => tree.Any(kept => ReferenceEquals(kept, file)))
                .ToList();
            if (changed.Count == 0) continue;

            // A corpus this branch created has no stamp to compare, and its first stamp stands.
            if (Git.Run(Repo.Root, "show", $"{before}:{folder}/{Descriptor}") is not { } then) continue;

            var plan = Bundler.Plan(new BundleSource(tree, [.. Exported(folder, today).Select(Bytes)]));
            if (plan.Problems.Count > 0)
                throw new InvalidOperationException(
                    $"{folder} cannot be bundled, so its plugin was not compared:\n  "
                    + string.Join("\n  ", plan.Problems));

            var bundled = plan.Files.Select(file => file.Path).ToHashSet(StringComparer.Ordinal);
            var reached = changed
                .Where(file => bundled.Contains($"{Dist.PluginDir}/{file.Path}"))
                .Select(file => file.Path)
                .Order(StringComparer.Ordinal)
                .ToList();

            if (reached.Count == 0 || VersionRange.Newer(Stated(folder) ?? "", Version(then) ?? "")) continue;

            unmoved.Add($"{folder} at {Stated(folder)}\n      "
                        + string.Join("\n      ", reached.Take(ListedPerCorpus))
                        + (reached.Count > ListedPerCorpus
                            ? $"\n      and {reached.Count - ListedPerCorpus} more"
                            : ""));
        }

        Assert.True(unmoved.Count == 0,
            $"these corpora bundle a skill, a hook or a manifest this branch changed, and left {Stamp} where it "
            + "was:\n  "
            + string.Join("\n  ", unmoved)
            + $"\nMove {Stamp} in each {Descriptor}. std-VERS says which part of it a change moves.");
    }

    /// <summary>
    /// Every path the branch added, changed, renamed or deleted, committed or not, at both ends of a rename. Each of
    /// those can change an export or a bundle.
    /// </summary>
    private static List<string> Touched(string before) =>
        Diff.Changed(before, "AMRD")
            .SelectMany(file => new[] { file.Was, file.Now })
            .Concat(Diff.Untracked())
            .ToList();

    /// <summary>Each file of <c>tree</c>, read from <c>dir</c>, whose path the branch touched.</summary>
    private static IEnumerable<BundleFile> Edited(
        IReadOnlySet<string> touched, string dir, IReadOnlyList<BundleFile> tree) =>
        tree.Where(file => touched.Contains($"{dir}/{file.Path}"));

    /// <summary>Forward-slashed.</summary>
    private static string RepoRelative(string path) =>
        Path.GetRelativePath(Repo.Root, Path.GetFullPath(path)).Replace('\\', '/');

    /// <summary>
    /// The export <c>kac export</c> would write from the working tree. Each corpus it consumes is built the same way
    /// from its folder here, and merged in as <c>kac restore</c> would leave it.
    /// </summary>
    /// <exception cref="InvalidOperationException">Where a corpus it consumes has no folder here.</exception>
    private static IReadOnlyList<ExportFile> Exported(string folder, DateOnly today)
    {
        var descriptor = CorpusDescriptor.Parse(Files.ReadLf(Path.Combine(Repo.Root, folder, Descriptor)));
        var restored = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var entry in descriptor.Consumes)
        {
            var producer = Producer(entry.Corpus)
                           ?? throw new InvalidOperationException(
                               $"{folder} consumes {entry.Corpus}, and no corpus under '{RecordPath.Published}/' "
                               + "is called that, so its bundle was not compared.");

            foreach (var file in Exported(producer, today))
                restored[$"{entry.Shortcode}/{file.Path}"] = file.Content;
        }

        var (inherited, _) = Inherited.Read(
            descriptor.Consumes,
            dir => [.. Inside(restored, dir).Where(name => !name.Contains('/', StringComparison.Ordinal))],
            dir => [.. Inside(restored, dir)
                .Where(name => name.Contains('/', StringComparison.Ordinal))
                .Select(name => name[..name.IndexOf('/', StringComparison.Ordinal)])
                .Distinct(StringComparer.Ordinal)],
            restored.GetValueOrDefault);

        return PublishedExport.Built(Working(folder), today, inherited);
    }

    /// <summary>Every path under <c>dir</c>, at any depth, relative to it.</summary>
    private static IEnumerable<string> Inside(Dictionary<string, string> files, string dir) =>
        files.Keys
            .Where(path => path.StartsWith(dir + "/", StringComparison.Ordinal))
            .Select(path => path[(dir.Length + 1)..]);

    /// <summary>The folder of the corpus here that calls itself <c>name</c>, or null.</summary>
    private static string? Producer(string? name) =>
        Corpora()
            .Select(corpus => $"{RecordPath.Published}/{corpus}")
            .FirstOrDefault(folder =>
                CorpusDescriptor.Parse(Files.ReadLf(Path.Combine(Repo.Root, folder, Descriptor))).Name == name);

    /// <summary>An export file as <c>Bundler</c> reads one off the disk, encoded as UTF-8.</summary>
    private static BundleFile Bytes(ExportFile file) => new(file.Path, Encoding.UTF8.GetBytes(file.Content));

    /// <summary>Each <c>consumes:</c> entry's <c>resolved:</c> lock, by the corpus it names.</summary>
    private static Dictionary<string, string?> Locks(string descriptor) =>
        CorpusDescriptor.Parse(descriptor).Consumes
            .GroupBy(entry => entry.Corpus ?? entry.Shortcode ?? "", StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First().Resolved, StringComparer.Ordinal);

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
