using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace kac.core;

// One file, and the bytes it holds. Bytes, because most of what a bundle writes it did not author: the
// plugin tree and the export are copied through untouched, and a copy that decoded and re-encoded would
// be a copy with an opinion.
//
// `Executable` travels with the bytes because one file in the plugin tree is run. A hook is a command.
// A command copied without its permission bit installs and then fails at the first session, complaining
// about permissions and never about the corpus. Windows has no such bit, so a hook ships as a POSIX
// script beside a `.cmd` twin, and the shell there picks one.
public sealed record BundleFile(string Path, byte[] Content, bool Executable = false);

// One component the plugin manifest declares, as the manifest states it. `Requires` names the record
// types the component reads, each optionally with the shape version it reads that type at, as
// `glossary@1`. Held as the manifest wrote them, because `bundle.json` reports them back. A component
// naming no type is unconditional and always travels.
//
// `Announce` is whether the breadcrumb names this component at the start of a session. False by
// default, because a skill somebody asks for by name costs nothing to leave unannounced and the
// breadcrumb is read at every start, resume, clear and compact. `docs/cli/bundle.md` says which
// skills earn the line.
public sealed record PluginComponent(
    string Path, IReadOnlyList<string> Requires, string? Note, bool Announce = false);

// A component left out, and the type whose absence left it out. The reason is carried, because it is
// the one thing the assembled plugin cannot say about itself.
public sealed record TrimmedComponent(string Path, IReadOnlyList<string> Requires, string Reason);

// What a bundle comes to. Named before anything is written, as `ExportPlan` and the generator's plan
// are, so a test can ask what a bundle would contain without a filesystem.
//
// `Files` are named relative to `Dist.Root`. A bundle writes two things under it: the plugin directory
// and the marketplace pointing at it. One list of addresses keeps `Write` from deciding where anything
// goes a second time.
//
// `Problems` is what stops the run. A plan carrying one is not written, for the reason
// `docs/cli/bundle.md` gives.
public sealed record BundlePlan(
    IReadOnlyList<BundleFile> Files,
    string PluginName,
    string? Version,
    IReadOnlyList<PluginComponent> Included,
    IReadOnlyList<TrimmedComponent> Trimmed,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> Problems);

// The two trees a bundle is assembled from, read once and passed in. `PluginTree` is `.plugin/` and
// `Export` is what `kac export` left, each named relative to its own root.
//
// Nothing else is read, and the corpus is never loaded. Everything a bundle has to decide is a fact
// about the export it was handed: which version to stamp, and which components the data can support.
// The export is also the only thing that will travel. Assemble against the corpus and the plugin ships
// a skill for a type the export happened not to carry.
public sealed record BundleSource(IReadOnlyList<BundleFile> PluginTree, IReadOnlyList<BundleFile> Export);

public static class Bundler
{
    // The tree a plugin is built from: the manifest, and the components the manifest declares. Shared
    // across every corpus running the framework bar the manifest itself, which each corpus owns.
    public const string SourceDir = ".plugin";

    // The shape of `bundle.json`, versioned as the export's own manifest is and for the same reason:
    // it is a document something other than this tool will one day read.
    public const int RecordVersion = 1;

    // Where the plugin manifest sits inside the plugin tree, and where it will sit inside the bundle.
    // The directory name is Claude Code's, not this tool's, so a component placed inside it loads wrong.
    public const string ManifestFile = ".claude-plugin/plugin.json";

    // What the bundle records about itself, at the plugin root rather than inside `.claude-plugin/`,
    // which holds the manifest and nothing else.
    public const string RecordFile = "bundle.json";

    // Whether a corpus path sits in the plugin tree. `Update` asks it, to withhold the shared half of
    // that tree from a corpus reading it from somewhere else.
    public static bool InSourceTree(string rel) =>
        rel.Equals(SourceDir, StringComparison.Ordinal)
        || rel.StartsWith(SourceDir + "/", StringComparison.Ordinal);

    // A shared tree and the corpus's own, read as one. A file the corpus holds wins, so a corpus taking
    // the shared tree may still write one component of its own at home and have that one travel.
    //
    // The manifest is never taken from the shared tree. It carries the name a plugin installs under, so
    // two corpora sharing a tree would otherwise ship one name between them. A corpus that wrote none of
    // its own is told by `Plan`, which refuses a tree holding no manifest at all.
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

        // Where the export is put inside the plugin, read from the manifest. The skills address it
        // through `${CLAUDE_PLUGIN_ROOT}/<corpusRoot>/…` by that same name. A default here would quietly
        // disagree with the words a corpus wrote in its own skill.
        var corpusRoot = JsonRead.Str(JsonRead.Object(manifest["metadata"])?["corpusRoot"]);
        if (corpusRoot is null)
            return Stop(problems,
                $"{ManifestFile} states no metadata.corpusRoot, so there is nowhere to put the export. "
                + "It is the directory the plugin's skills address the export through.");

        // The export lands under `corpusRoot` inside the plugin, so a plugin tree already holding that
        // directory is refused rather than merged. `docs/cli/bundle.md` says why.
        if (source.PluginTree.FirstOrDefault(f => Owns(corpusRoot, f.Path)) is { } clash)
            return Stop(problems,
                $"{ManifestFile} names metadata.corpusRoot '{corpusRoot}', and the plugin tree already holds "
                + $"{clash.Path}. The export is copied there, so one would overwrite the other.");

        var exportManifest = JsonRead.Parse(Text(source.Export, Exporter.ManifestFile));
        if (exportManifest is null)
            return Stop(problems,
                $"the export holds no readable {Exporter.ManifestFile}. Run the export first: kac export");

        // A plugin with no name is refused. The name is what a marketplace installs by and what a user
        // types, and it is the corpus's own name rather than a second one to keep in step. Asked of the
        // export, because that is where `Rewrite` takes it from.
        var pluginName = JsonRead.Str(exportManifest["corpus"]);
        if (pluginName is null)
            return Stop(problems,
                $"the export names no corpus, and that name is what a plugin is installed by. Write "
                + "`corpus:` in .corpus.yaml and export again.");

        // The shape the export declares, held against the shape this build knows how to read. A
        // mismatch is refused, and both numbers are named. `docs/cli/bundle.md` says what a
        // silent one would produce, and how two builds of this tool come to disagree.
        var declaredFormat = JsonRead.Int(exportManifest["formatVersion"]);
        if (declaredFormat != Exporter.FormatVersion)
            return Stop(problems,
                $"the export declares format version {declaredFormat?.ToString() ?? "none"} and this tool reads "
                + $"version {Exporter.FormatVersion}. Rebuild it: kac export");

        // What the export carried and the shape each type is at, which is what decides the trimming
        // below. A type the corpus adopted and exported nothing for is absent here, so a component
        // reading it would find nothing.
        var carried = Types(exportManifest);

        var declared = Components(JsonRead.Object(manifest["metadata"])?["components"]);
        var included = new List<PluginComponent>();
        var trimmed = new List<TrimmedComponent>();

        // An absent type and a shape the component cannot read are answered differently, because they
        // are different states. A missing type trims the component and leaves a plugin that does less.
        // A type present at another shape leaves the component reading files whose keys have moved, and
        // that is a plugin returning nothing where it should return an answer. `docs/cli/bundle.md`
        // says why one is a trim and the other stops the run.
        foreach (var component in declared)
        {
            var missing = new List<string>();

            foreach (var entry in component.Requires)
            {
                var (type, shape) = Need(entry);
                var held = carried.FirstOrDefault(c => string.Equals(c.Type, type, StringComparison.Ordinal));
                if (held.Type is null)
                {
                    missing.Add(type);
                    continue;
                }

                if (shape is null) continue;

                if (!int.TryParse(shape, out var wanted))
                    problems.Add($"{ManifestFile} declares '{component.Path}' against '{entry}', and a shape "
                                 + "version is a whole number. Write it as '<type>@<version>'.");
                else if (wanted != held.Shape)
                    problems.Add($"{ManifestFile} declares '{component.Path}' against '{type}' shape version "
                                 + $"{wanted}, and the export carries version {held.Shape}. Write the component "
                                 + "against the shape the export carries, or bundle an export built at "
                                 + $"version {wanted}.");
            }

            if (missing.Count == 0)
                included.Add(component);
            else
                trimmed.Add(new TrimmedComponent(component.Path, component.Requires,
                    $"the export carries no {string.Join(" or ", missing)}"));
        }

        if (problems.Count > 0) return new BundlePlan([], "", null, [], [], [], problems);

        // A plugin with nothing left in it is still assembled. Refusing would leave a corpus unable to
        // build the thing that would have told it why. The empty plugin is the report: it installs, does
        // nothing, and `bundle.json` beside it names every component that was dropped.
        if (included.Count == 0)
            warnings.Add(declared.Count == 0
                ? $"{ManifestFile} declares no components under metadata.components, so the plugin carries "
                  + "the export and nothing that reads it."
                : $"every component was trimmed. The plugin carries the export and nothing that reads it. "
                  + $"{RecordFile} names each one and the type it needed.");

        // The plugin's version is the corpus content version, taken from the export.
        // `docs/cli/bundle.md` says why, and why the format version stays put.
        var version = JsonRead.Str(exportManifest["contentVersion"]);
        if (version is null)
            warnings.Add("the export states no contentVersion, so the plugin manifest carries no version "
                         + "and nothing will install it. Set content-version in .corpus.yaml.");

        // The plugin tree, less every subtree a trimmed component owns, and less the manifest, which is
        // rewritten below. A path no component owns is unconditional and travels whatever the corpus
        // adopted.
        var files = (from file in source.PluginTree
            where file.Path != ManifestFile
            where !trimmed.Any(t => Owns(t.Path, file.Path))
            select file with { Path = $"{Dist.PluginDir}/{file.Path}" }).ToList();

        // The export, copied byte for byte and edited by nothing. `bundle` writes its own account of the
        // run beside it rather than into it. The two copies of the export stay comparable, so a
        // difference between them is a defect rather than something to interpret.
        files.AddRange(source.Export.Select(file => file with { Path = $"{Dist.PluginDir}/{corpusRoot}/{file.Path}" }));

        var travelling = Rewrite(manifest, exportManifest, version, included);
        files.Add(Utf8($"{Dist.PluginDir}/{ManifestFile}", Serialize(travelling)));

        // The breadcrumb, rendered here because everything it states is settled here. Asking the
        // surviving files rather than the components is what ties it to the hook directory.
        // `docs/cli/bundle.md` says what that settles.
        var breadcrumbDir = $"{Dist.PluginDir}/{Breadcrumb.RenderedFile[..Breadcrumb.RenderedFile.LastIndexOf('/')]}";
        if (files.Any(f => Owns(breadcrumbDir, f.Path)))
            files.Add(Utf8($"{Dist.PluginDir}/{Breadcrumb.RenderedFile}",
                Breadcrumb.Render(exportManifest, source.Export, included)));

        files.Add(Utf8($"{Dist.PluginDir}/{RecordFile}",
            Serialize(new BundleRecord(
                RecordVersion, pluginName, version ?? JsonRead.Str(manifest["version"]), corpusRoot,
                new BundleExport(
                    JsonRead.Int(exportManifest["formatVersion"]), JsonRead.Str(exportManifest["corpus"]),
                    JsonRead.Str(exportManifest["contentVersion"]), [.. carried.Select(c => c.Type)]),
                [.. included.Select(c => new BundleIncluded(c.Path, c.Requires, c.Note))],
                [.. trimmed.Select(t => new BundleTrimmed(t.Path, t.Requires, t.Reason))]))));

        files.Add(Utf8(Dist.MarketplaceRel, Marketplace(travelling, pluginName)));

        return new BundlePlan(
            [.. files.OrderBy(f => f.Path, StringComparer.Ordinal)],
            pluginName, version ?? JsonRead.Str(manifest["version"]), included, trimmed, warnings, problems);
    }

    // Replace the bundle whole: delete the two directories it owns, then write. The export does the same,
    // and for the same reason: a component dropped from the manifest must not stay readable in an artefact
    // nobody reviews. Both name their own directories rather than `.dist/`, because the export is under the
    // same root and a bundle is not entitled to take it.
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
            Directory.CreateDirectory(Path.GetDirectoryName(full)!);
            File.WriteAllBytes(full, file.Content);
            if (file.Executable) MakeExecutable(full);
            written.Add($"{Dist.Root}/{file.Path}");
        }

        return written;
    }

    // A directory tree read into the shape `Plan` takes, named relative to its own root. A missing
    // directory returns null, so the caller can tell "no plugin tree" from "an empty one" without asking twice.
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

    // Give the copy the execute bit for everyone, not for the owner alone. A hook is a command the
    // plugin's own manifest names, so a copy only its owner could run would still fail for a plugin
    // installed into a shared cache.
    private static void MakeExecutable(string path)
    {
        if (OperatingSystem.IsWindows()) return;

        File.SetUnixFileMode(path,
            File.GetUnixFileMode(path)
            | UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute);
    }

    // The plugin manifest as it will travel: who this plugin is written from the export, the trimmed
    // components gone, and every other key exactly as the corpus wrote it.
    //
    // **Identity is generated and never inherited.** A corpus copies this file from a template, and a
    // template naming an author, a licence and a repository hands every corpus that copies it somebody
    // else's identity to publish under. So each of those keys is written from what the corpus declared,
    // and a key the corpus declared nothing for is removed rather than left standing. `docs/cli/bundle.md`
    // says which key each one comes from.
    //
    // What is left is the corpus's own declaration: which components it ships, and anything else it
    // wrote. The document is edited as a DOM rather than mapped onto a record, because a corpus may
    // write keys this tool has never heard of, and reading it into a shape known here would delete them
    // without a word.
    private static JsonObject Rewrite(
        JsonObject manifest, JsonObject exportManifest, string? version, List<PluginComponent> included)
    {
        var copy = (JsonObject)manifest.DeepClone();
        if (version is not null) copy["version"] = version;

        var about = JsonRead.Object(exportManifest["about"]);
        var name = JsonRead.Str(exportManifest["corpus"]);
        if (name is not null) copy["name"] = name;

        State(copy, "displayName", JsonRead.Str(about?["displayName"]));
        State(copy, "license", JsonRead.Str(about?["license"]));

        // A corpus naming nobody is filed under its own name, as `Packer.Nuspec` files one under its own
        // id and for the same reason: the field is asked for, and the honest answer to "who wrote this"
        // is the corpus rather than whoever wrote the template it copied. A licence is not asked for, so
        // a corpus that chose none asserts none.
        Set(copy, "author",
            JsonRead.Object(about?["author"])?.DeepClone()
            ?? (name is null ? null : new JsonObject { ["name"] = name }));

        // Both name the same place, which is where the corpus's source lives. A plugin manifest asks for
        // them separately and the export states it once.
        var home = JsonRead.Str(JsonRead.Object(exportManifest["publishing"])?["base"]);
        State(copy, "homepage", home);
        State(copy, "repository", home);

        // Said by the corpus where it said anything, and otherwise a sentence naming what a reader is
        // installing. A description is what a marketplace lists, so leaving none is worse than a plain one.
        State(copy, "description",
            JsonRead.Str(about?["description"])
            ?? (name is null ? null : $"The {name} knowledge corpus, and the skills that read it."));

        // The types the export actually carried, so a plugin never advertises a type its corpus declined.
        // The framework's own name leads, because that is what somebody searches a marketplace for.
        var keywords = Types(exportManifest).Select(t => t.Type).ToList();
        Set(copy, "keywords",
            keywords.Count == 0 ? null : new JsonArray([.. keywords.Prepend("knowledge-as-code")
                .Select(k => (JsonNode)JsonValue.Create(k)!)]));

        if (JsonRead.Object(copy["metadata"]) is { } metadata && metadata["components"] is JsonArray components)
        {
            var kept = new JsonArray();
            foreach (var node in components)
                if (node is JsonObject c && included.Any(i => i.Path == JsonRead.Str(c["path"])))
                    kept.Add(c.DeepClone());

            metadata["components"] = kept;
        }

        // Ordered so a reader meets the plugin's identity before its declarations, whatever order the
        // keys were written or added in. A key this tool has never heard of keeps its place after the
        // ones named here, which is where the corpus wrote it.
        var ordered = new JsonObject();
        foreach (var key in Order.Where(k => copy.ContainsKey(k)))
        {
            ordered[key] = copy[key]!.DeepClone();
            copy.Remove(key);
        }

        foreach (var (key, value) in copy) ordered[key] = value?.DeepClone();
        return ordered;
    }

    // The keys of a plugin manifest, in the order one is read. Identity first, then what the corpus
    // declares it ships.
    private static readonly string[] Order =
    [
        "$schema", "name", "displayName", "version", "description", "author", "homepage", "repository",
        "license", "keywords", "metadata"
    ];

    // One key stated where the corpus stated it, and gone where it did not. A key left standing is a key
    // inherited from whatever template the corpus copied, which is the fault this exists to prevent.
    private static void Set(JsonObject manifest, string key, JsonNode? value)
    {
        if (value is null) manifest.Remove(key);
        else manifest[key] = value;
    }

    private static void State(JsonObject manifest, string key, string? value) =>
        Set(manifest, key, value is { Length: > 0 } said ? JsonValue.Create(said) : null);

    // The marketplace offering the plugin, so there is something to install it from. One definition
    // serves both ways of reaching it: a path while the plugin is being proved, and a published
    // branch afterwards. It can, because a marketplace addresses its plugins relative to itself and
    // so names no host. It takes the plugin's own name, because the marketplace is what a reader types to
    // install from. A name qualified by where this copy sits would be wrong as soon as the copy moved.
    //
    // `.dist/` is the marketplace root and the plugin sits beneath it as `./plugin`, for the reason
    // `Dist.Root` gives.
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

    // The types the export carried and the shape each is at, read off its manifest in the order it
    // lists them. A type that contributed no record is absent from that list.
    //
    // A type stating no shape reads as version 0, which no type declares, so a component naming a
    // version against it is refused rather than matched by accident.
    private static List<(string Type, int Shape)> Types(JsonObject exportManifest)
    {
        var types = new List<(string, int)>();
        if (exportManifest["types"] is not JsonArray declared) return types;

        foreach (var node in declared)
            if (JsonRead.Object(node) is { } entry && JsonRead.Str(entry["type"]) is { } key)
                types.Add((key, JsonRead.Int(entry["shapeVersion"]) ?? 0));

        return types;
    }

    // A `requires` entry split into the type and the shape version the component reads it at, which is
    // null where the entry names none. A bare `glossary` needs a glossary in the export and opens none
    // of its files; `glossary@1` reads the keys of a term line. Those are different needs, and a
    // breadcrumb naming the first must not be refused over a change to the second.
    //
    // The version is carried as written rather than parsed here, so an entry nobody can read is
    // reported against the manifest that holds it.
    private static (string Type, string? Shape) Need(string entry)
    {
        var at = entry.IndexOf('@');
        return at < 0 ? (entry, null) : (entry[..at], entry[(at + 1)..]);
    }

    // The components a manifest declares. A component with no `path` is skipped: the path is what a
    // trim acts on, and one without it could be neither included nor left out.
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
                JsonRead.Bool(component["announce"])));
        }

        return components;
    }

    // Whether a component owns a path: the path itself, or anything beneath it. Both, because a
    // component may be a directory of skills or a single file such as a hook definition.
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

    // The manifest as it is written out. Its own serialiser, because the document is a DOM carrying keys
    // this tool has never heard of rather than a record it could map.
    private static string Serialize(JsonObject manifest) => manifest.ToJsonString(Indented) + "\n";

    private static string Serialize(BundleRecord record) =>
        JsonSerializer.Serialize(record, KacJson.Relaxed.BundleRecord) + "\n";

    private static string Serialize(MarketplaceManifest manifest) =>
        JsonSerializer.Serialize(manifest, KacJson.Relaxed.MarketplaceManifest) + "\n";
}
