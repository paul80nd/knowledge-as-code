using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace kac.core;

// Bytes, because the plugin tree and the export are copied through untouched. `Executable` travels with them:
// a hook copied without its permission bit installs and then fails at the first session.
public sealed record BundleFile(string Path, byte[] Content, bool Executable = false);

// `Requires` names the record types the component reads, each optionally at a shape version, as `glossary@1`.
// A component naming no type supports the ones that do, and is trimmed when none of them survives unless it is
// `Standalone`. `Announce` puts it in the session breadcrumb. `docs/design/plugin.md` says which earn it.
public sealed record PluginComponent(
    string Path,
    IReadOnlyList<string> Requires,
    string? Note,
    bool Announce = false,
    bool Standalone = false);

// The reason is carried, because the assembled plugin cannot say why a component is missing.
public sealed record TrimmedComponent(string Path, IReadOnlyList<string> Requires, string Reason);

// Named before anything is written, so a test can ask what a bundle would contain without a filesystem.
// `Files` are relative to `Dist.Root`. A plan with `Problems` is not written. `docs/cli/bundle.md` says why.
public sealed record BundlePlan(
    IReadOnlyList<BundleFile> Files,
    string PluginName,
    string? Version,
    IReadOnlyList<PluginComponent> Included,
    IReadOnlyList<TrimmedComponent> Trimmed,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> Problems);

// `PluginTree` is `.plugin/` and `Export` is what `kac export` left, each relative to its own root. The
// corpus is never loaded: assembling against it would ship a skill for a type the export did not carry.
public sealed record BundleSource(IReadOnlyList<BundleFile> PluginTree, IReadOnlyList<BundleFile> Export);

public static class Bundler
{
    // Shared by every corpus running the framework, bar the manifest, which each corpus owns.
    public const string SourceDir = ".plugin";

    // The shape of `bundle.json`, versioned because something other than this tool will read it.
    public const int RecordVersion = 1;

    // Where the plugin manifest sits inside the plugin tree, and where it will sit inside the bundle.
    // The directory name is Claude Code's, not this tool's, so a component placed inside it loads wrong.
    public const string ManifestFile = ".claude-plugin/plugin.json";

    // At the plugin root, because `.claude-plugin/` holds the manifest and nothing else.
    public const string RecordFile = "bundle.json";

    public static bool InSourceTree(string rel) =>
        rel.Equals(SourceDir, StringComparison.Ordinal)
        || rel.StartsWith(SourceDir + "/", StringComparison.Ordinal);

    // A file the corpus holds wins over the shared one. The manifest never comes from the shared tree, because
    // it carries the name a plugin installs under.
    public static IReadOnlyList<BundleFile> Merge(
        IReadOnlyList<BundleFile> shared, IReadOnlyList<BundleFile> own)
    {
        var byPath = new Dictionary<string, BundleFile>(StringComparer.Ordinal);

        foreach (var file in shared)
            if (!file.Path.Equals(ManifestFile, StringComparison.Ordinal))
                byPath[file.Path] = file;

        foreach (var file in own) byPath[file.Path] = file;

        return [.. byPath.Values.OrderBy(f => f.Path, StringComparer.Ordinal)];
    }

    public static BundlePlan Plan(BundleSource source)
    {
        var problems = new List<string>();
        var warnings = new List<string>();

        var manifestText = Text(source.PluginTree, ManifestFile);
        if (manifestText is null)
            return Stop(problems, $"the plugin tree holds no {ManifestFile}, so there is nothing to assemble.");

        var manifest = JsonRead.Parse(manifestText);
        if (manifest is null)
            return Stop(problems, $"{ManifestFile} is not a JSON object.");

        // Read from the manifest, because the skills address the export by that name. A default would disagree
        // with them.
        var corpusRoot = JsonRead.Str(JsonRead.Object(manifest["metadata"])?["corpusRoot"]);
        if (corpusRoot is null)
            return Stop(problems,
                $"{ManifestFile} states no metadata.corpusRoot, so there is nowhere to put the export. "
                + "It is the directory the plugin's skills address the export through.");

        if (source.PluginTree.FirstOrDefault(f => Owns(corpusRoot, f.Path)) is { } clash)
            return Stop(problems,
                $"{ManifestFile} names metadata.corpusRoot '{corpusRoot}', and the plugin tree already holds "
                + $"{clash.Path}. The export is copied there, so one would overwrite the other.");

        var exportManifest = Exporter.ReadManifest(Text(source.Export, Exporter.ManifestFile));
        if (exportManifest is null)
            return Stop(problems,
                $"the export holds no readable {Exporter.ManifestFile}. Run the export first: kac export");

        // Asked of the export, because that is where `Rewrite` takes the name from.
        var pluginName = exportManifest.Corpus;
        if (pluginName is null)
            return Stop(problems,
                $"the export names no corpus, and that name is what a plugin is installed by. Write "
                + "`corpus:` in .corpus.yaml and export again.");

        if (exportManifest.FormatVersion != Exporter.FormatVersion)
            return Stop(problems,
                $"the export declares format version {Declared(exportManifest.FormatVersion)} and this tool "
                + $"reads version {Exporter.FormatVersion}. Rebuild it: kac export");

        var carried = exportManifest.Types;

        var declared = Components(JsonRead.Object(manifest["metadata"])?["components"]);
        var included = new List<PluginComponent>();
        var trimmed = new List<TrimmedComponent>();

        // A missing type trims the component. A type at another shape stops the run, because the component would
        // read keys that have moved. `docs/cli/bundle.md` says why.
        foreach (var component in declared)
        {
            var missing = new List<string>();

            foreach (var entry in component.Requires)
            {
                var (type, shape) = Need(entry);
                var held = carried.FirstOrDefault(c => string.Equals(c.Type, type, StringComparison.Ordinal));
                if (held is null)
                {
                    missing.Add(type);
                    continue;
                }

                if (shape is null) continue;

                if (!int.TryParse(shape, out var wanted))
                    problems.Add($"{ManifestFile} declares '{component.Path}' against '{entry}', and a shape "
                                 + "version is a whole number. Write it as '<type>@<version>'.");
                else if (wanted != held.ShapeVersion)
                    problems.Add($"{ManifestFile} declares '{component.Path}' against '{type}' shape version "
                                 + $"{wanted}, and the export carries version {held.ShapeVersion}. Write the component "
                                 + "against the shape the export carries, or bundle an export built at "
                                 + $"version {wanted}.");
            }

            if (missing.Count == 0)
                included.Add(component);
            else
                trimmed.Add(new TrimmedComponent(component.Path, component.Requires,
                    $"the export carries no {string.Join(" or ", missing)}"));
        }

        // A component naming no type supports the ones that do, so it follows them out when none survives. A
        // `standalone` one stays.
        if (!included.Any(c => c.Requires.Count > 0))
            foreach (var supporting in included.Where(c => c.Requires.Count == 0 && !c.Standalone).ToList())
            {
                included.Remove(supporting);
                trimmed.Add(new TrimmedComponent(supporting.Path, supporting.Requires,
                    "no component it supports survived"));
            }

        if (problems.Count > 0) return new BundlePlan([], "", null, [], [], [], problems);

        // An empty plugin is still assembled, because its `bundle.json` is what tells the corpus why.
        if (included.Count == 0)
            warnings.Add(declared.Count == 0
                ? $"{ManifestFile} declares no components under metadata.components, so the plugin carries "
                  + "the export and nothing that reads it."
                : $"every component was trimmed. The plugin carries the export and nothing that reads it. "
                  + $"{RecordFile} names each one and the type it needed.");

        // `docs/cli/bundle.md` says why the plugin takes the content version and not the format version.
        var version = exportManifest.ContentVersion;
        if (version is null)
            warnings.Add("the export states no contentVersion, so the plugin manifest carries no version "
                         + "and nothing will install it. Set content-version in .corpus.yaml.");

        // A path no component owns is unconditional and travels whatever the corpus adopted.
        var files = (from file in source.PluginTree
            where file.Path != ManifestFile
            where !trimmed.Any(t => Owns(t.Path, file.Path))
            select file with { Path = $"{Dist.PluginDir}/{file.Path}" }).ToList();

        // The export is copied byte for byte, so the two copies of it stay comparable.
        files.AddRange(source.Export.Select(file => file with { Path = $"{Dist.PluginDir}/{corpusRoot}/{file.Path}" }));

        var travelling = Rewrite(manifest, exportManifest, version, included);
        files.Add(Utf8($"{Dist.PluginDir}/{ManifestFile}", Serialize(travelling)));

        // Asks the surviving files, not the components, which ties it to the hook directory. See
        // `docs/design/plugin.md`.
        var breadcrumbDir = $"{Dist.PluginDir}/{Breadcrumb.RenderedFile[..Breadcrumb.RenderedFile.LastIndexOf('/')]}";
        if (files.Any(f => Owns(breadcrumbDir, f.Path)))
            files.Add(Utf8($"{Dist.PluginDir}/{Breadcrumb.RenderedFile}",
                Breadcrumb.Render(exportManifest, source.Export, included)));

        files.Add(Utf8($"{Dist.PluginDir}/{RecordFile}",
            Serialize(new BundleRecord(
                RecordVersion, pluginName, version ?? JsonRead.Str(manifest["version"]), corpusRoot,
                new BundleExport(
                    exportManifest.FormatVersion, exportManifest.Corpus,
                    exportManifest.ContentVersion, [.. carried.Select(c => c.Type)]),
                [.. included.Select(c => new BundleIncluded(c.Path, c.Requires, c.Note))],
                [.. trimmed.Select(t => new BundleTrimmed(t.Path, t.Requires, t.Reason))]))));

        files.Add(Utf8(Dist.MarketplaceRel, Marketplace(travelling, pluginName)));

        return new BundlePlan(
            [.. files.OrderBy(f => f.Path, StringComparer.Ordinal)],
            pluginName, version ?? JsonRead.Str(manifest["version"]), included, trimmed, warnings, problems);
    }

    // Replaces the bundle whole, so a dropped component is not left behind. It deletes only its own two
    // directories, because the export shares `.dist/`.
    public static List<string> Write(string corpusRoot, BundlePlan plan)
    {
        foreach (var owned in new[] { Dist.Plugin, $"{Dist.Root}/{Dist.MarketplaceDir}" })
        {
            var dir = Path.Combine(corpusRoot, owned.Replace('/', Path.DirectorySeparatorChar));
            if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
        }

        var root = Path.Combine(corpusRoot, Dist.Root);
        var written = new List<string>();
        foreach (var file in plan.Files)
        {
            var full = Path.Combine(root, file.Path.Replace('/', Path.DirectorySeparatorChar));
            Files.OpenFolderFor(full);
            File.WriteAllBytes(full, file.Content);
            if (file.Executable) MakeExecutable(full);
            written.Add($"{Dist.Root}/{file.Path}");
        }

        return written;
    }

    // Null where the directory is missing, so a caller can tell no plugin tree from an empty one.
    public static IReadOnlyList<BundleFile>? Read(string root)
    {
        if (!Directory.Exists(root)) return null;

        return
        [
            .. Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
                .Select(f => new BundleFile(
                    Path.GetRelativePath(root, f).Replace('\\', '/'), File.ReadAllBytes(f), IsExecutable(f)))
                .OrderBy(f => f.Path, StringComparer.Ordinal)
        ];
    }

    // Whether the file is run rather than read. Windows has no such bit and .NET throws if you ask for
    // one there, so the question is not put. Nothing is lost by that. A file checked out on Windows never
    // carried the bit either. The `.cmd` twin beside each script needs no permission to be a command.
    private static bool IsExecutable(string path) =>
        !OperatingSystem.IsWindows()
        && (File.GetUnixFileMode(path) & UnixFileMode.UserExecute) != 0;

    // Execute for everyone, because a plugin may be installed into a shared cache.
    private static void MakeExecutable(string path)
    {
        if (OperatingSystem.IsWindows()) return;

        File.SetUnixFileMode(path,
            File.GetUnixFileMode(path)
            | UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute);
    }

    // The plugin manifest as it will travel. Identity keys are written from the export, and removed where the
    // corpus declared nothing, so no corpus publishes under its template's identity. `docs/cli/bundle.md` says
    // where each key comes from. Edited as a DOM, so a key this tool does not know survives.
    private static JsonObject Rewrite(
        JsonObject manifest, ExportManifest exportManifest, string? version, List<PluginComponent> included)
    {
        var copy = (JsonObject)manifest.DeepClone();
        if (version is not null) copy["version"] = version;

        var about = exportManifest.About;
        var name = exportManifest.Corpus;
        if (name is not null) copy["name"] = name;

        State(copy, "displayName", about.DisplayName);
        State(copy, "license", about.License);

        // A corpus naming nobody is filed under its own name, as `Packer.Nuspec` does. A licence is not required,
        // so none is asserted.
        Set(copy, "author", Author(about.Author) ?? (name is null ? null : new JsonObject { ["name"] = name }));

        var home = exportManifest.Publishing.Base;
        State(copy, "homepage", home);
        State(copy, "repository", home);

        State(copy, "description",
            about.Description
            ?? (name is null ? null : $"The {name} knowledge corpus, and the skills that read it."));

        // The types the export carried, so a plugin never advertises a type its corpus declined.
        var keywords = exportManifest.Types.Select(t => t.Type).ToList();
        Set(copy, "keywords",
            keywords.Count == 0
                ? null
                : new JsonArray([
                    .. keywords.Prepend("knowledge-as-code")
                        .Select(k => (JsonNode?)JsonValue.Create(k))
                ]));

        if (JsonRead.Object(copy["metadata"]) is { } metadata && metadata["components"] is JsonArray components)
        {
            var kept = new JsonArray();
            foreach (var node in components)
                if (node is JsonObject c && included.Any(i => i.Path == JsonRead.Str(c["path"])))
                    kept.Add(c.DeepClone());

            metadata["components"] = kept;
        }

        // Identity keys first. A key this tool does not know keeps its place after them.
        var ordered = new JsonObject();
        foreach (var key in Order.Where(k => copy.ContainsKey(k)))
        {
            ordered[key] = copy[key]?.DeepClone();
            copy.Remove(key);
        }

        foreach (var (key, value) in copy) ordered[key] = value?.DeepClone();
        return ordered;
    }

    private static readonly string[] Order =
    [
        "$schema", "name", "displayName", "version", "description", "author", "homepage", "repository",
        "license", "keywords", "metadata"
    ];

    // Removes a key the corpus did not state, so nothing is inherited from the template it copied.
    private static void Set(JsonObject manifest, string key, JsonNode? value)
    {
        if (value is null) manifest.Remove(key);
        else manifest[key] = value;
    }

    // A manifest stating no version reads as 0, so it is reported as none.
    private static string Declared(int version) => version == 0 ? "none" : version.ToString();

    // Null where the corpus named nobody. Both keys are written even when empty, as the export states them.
    private static JsonObject? Author(ExportAuthor? author) =>
        author is null ? null : new JsonObject { ["name"] = author.Name, ["url"] = author.Url };

    private static void State(JsonObject manifest, string key, string? value) =>
        Set(manifest, key, value is { Length: > 0 } said ? JsonValue.Create(said) : null);

    // One definition serves a local path and a published branch, because a marketplace addresses its plugins
    // relative to itself. It takes the plugin's own name, which is what a reader types to install.
    private static string Marketplace(JsonObject travelling, string pluginName) =>
        Serialize(new MarketplaceManifest(
            "https://anthropic.com/claude-code/marketplace.schema.json",
            pluginName,
            $"A marketplace holding the {pluginName} plugin.",
            new MarketplaceOwner(Owner(travelling) ?? pluginName),
            [
                new MarketplacePlugin(
                    pluginName,
                    JsonRead.Str(travelling["description"]) ?? $"The {pluginName} plugin, built from this corpus.",
                    $"./{Dist.PluginDir}")
            ]));

    // Who the marketplace belongs to, taken from whoever the plugin says wrote it. `author` is a string
    // in some manifests and an object with a `name` in others. Both are read rather than one declared
    // correct.
    private static string? Owner(JsonObject manifest) =>
        JsonRead.Str(manifest["author"]) ?? JsonRead.Str(JsonRead.Object(manifest["author"])?["name"]);

    // The shape is null where the entry names none. A bare `glossary` needs the type and opens none of its
    // files, so a shape change must not refuse it. The version stays a string, so a bad one is reported.
    private static (string Type, string? Shape) Need(string entry)
    {
        var at = entry.IndexOf('@');
        return at < 0 ? (entry, null) : (entry[..at], entry[(at + 1)..]);
    }

    // A component with no `path` is skipped, because a trim acts on the path.
    private static List<PluginComponent> Components(JsonNode? node)
    {
        var components = new List<PluginComponent>();
        if (node is not JsonArray declared) return components;

        foreach (var entry in declared)
        {
            if (JsonRead.Object(entry) is not { } component) continue;
            if (JsonRead.Str(component["path"]) is not { } path) continue;

            var requires = component["requires"] is JsonArray r
                ? r.Select(JsonRead.Str).OfType<string>().ToList()
                : [];

            components.Add(new PluginComponent(path, requires, JsonRead.Str(component["note"]),
                JsonRead.Bool(component["announce"]), JsonRead.Bool(component["standalone"])));
        }

        return components;
    }

    // The path itself or anything beneath it, because a component may be a directory or a single file.
    private static bool Owns(string component, string path) =>
        string.Equals(component, path, StringComparison.Ordinal)
        || path.StartsWith(component + "/", StringComparison.Ordinal);

    private static BundlePlan Stop(List<string> problems, string problem)
    {
        problems.Add(problem);
        return new BundlePlan([], "", null, [], [], [], problems);
    }

    private static BundleFile Utf8(string path, string content) =>
        new(path, new UTF8Encoding(false).GetBytes(content));

    private static string? Text(IReadOnlyList<BundleFile> files, string path) =>
        files.FirstOrDefault(f => f.Path == path) is { } file
            ? new UTF8Encoding(false).GetString(file.Content).TrimStart('\uFEFF')
            : null;

    // How the plugin manifest is written back out. Stated here rather than borrowed from `KacJson`,
    // because this is a DOM write rather than a serialized record. Two rules matter: a person reads the
    // file, and an em dash in a corpus's own description reaches them as itself.
    private static readonly JsonSerializerOptions Indented = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static string Serialize(JsonObject manifest) => manifest.ToJsonString(Indented) + "\n";

    private static string Serialize(BundleRecord record) =>
        JsonSerializer.Serialize(record, KacJson.Relaxed.BundleRecord) + "\n";

    private static string Serialize(MarketplaceManifest manifest) =>
        JsonSerializer.Serialize(manifest, KacJson.Relaxed.MarketplaceManifest) + "\n";
}
