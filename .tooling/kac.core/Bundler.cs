using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

// ---------------------------------------------------------------------------
// Bundle — the export and the plugin tree assembled into something installable
// ---------------------------------------------------------------------------

namespace kac.core;

// One file, and the bytes it holds. Bytes rather than text because most of what a bundle writes it did
// not author: the plugin tree and the export are copied through untouched, and a copy that decoded and
// re-encoded would be a copy with an opinion.
public sealed record BundleFile(string Path, byte[] Content);

// One component the plugin manifest declares, as the manifest states it. `Requires` names the record
// types the component reads; a component naming none is unconditional and always travels.
public sealed record PluginComponent(string Path, IReadOnlyList<string> Requires, string? Note);

// A component left out, and the type whose absence left it out. The reason is carried rather than
// recomputed, because it is the one thing the assembled plugin cannot say about itself.
public sealed record TrimmedComponent(string Path, IReadOnlyList<string> Requires, string Reason);

// What a bundle comes to. Named before anything is written, as `ExportPlan` and the generator's plan
// are, so a test can ask what a bundle would contain without a filesystem.
//
// `Files` are named relative to `Dist.Root`, because a bundle writes two things under it — the plugin
// directory and the marketplace pointing at it — and one list of addresses is what keeps `Write` from
// deciding where anything goes a second time.
//
// `Problems` is what stops the run. A plan carrying one is not written: the alternative is a plugin
// assembled around a missing answer, which installs and fails later somewhere less obvious.
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
// Nothing else is read. The corpus is not loaded: everything a bundle has to decide — which version to
// stamp, which components the data can support — is a fact about the export it was handed, and an
// export is the only thing that will actually travel. A bundle assembled against the corpus rather than
// against the export would ship a skill for a type the export happened not to carry.
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
    // The directory name is Claude Code's, not this tool's — a component placed inside it loads wrong.
    public const string ManifestFile = ".claude-plugin/plugin.json";

    // What the bundle records about itself, at the plugin root rather than inside `.claude-plugin/`,
    // which holds the manifest and nothing else.
    public const string RecordFile = "bundle.json";

    // What a bundle comes to, given the two trees it is assembled from.
    public static BundlePlan Plan(BundleSource source)
    {
        var problems = new List<string>();
        var warnings = new List<string>();

        var manifestText = Text(source.PluginTree, ManifestFile);
        if (manifestText is null)
            return Stop(problems, $"the plugin tree holds no {ManifestFile}, so there is nothing to assemble.");

        var manifest = Parse(manifestText);
        if (manifest is null)
            return Stop(problems, $"{ManifestFile} is not a JSON object.");

        // A manifest with no name is refused rather than given one. The name is what a marketplace
        // installs by and what a user types, so inventing it here would produce something that installs
        // under a name nobody chose.
        var pluginName = Str(manifest["name"]);
        if (pluginName is null)
            return Stop(problems, $"{ManifestFile} states no name, which is what a plugin is installed by.");

        // Where the export is put inside the plugin. Read rather than assumed, because the skills address
        // it through `${CLAUDE_PLUGIN_ROOT}/<corpusRoot>/…` by that same name — a default here would be
        // this tool quietly disagreeing with the words a corpus wrote in its own skill.
        var corpusRoot = Str(Object(manifest["metadata"])?["corpusRoot"]);
        if (corpusRoot is null)
            return Stop(problems,
                $"{ManifestFile} states no metadata.corpusRoot, so there is nowhere to put the export. "
                + "It is the directory the plugin's skills address the export through.");

        // The export lands under `corpusRoot` inside the plugin, so a plugin tree already holding that
        // directory would have its files silently replaced by the copy — or replace it. Refused, because
        // whichever won, the loser would be missing from an artefact nobody reviews.
        if (source.PluginTree.FirstOrDefault(f => Owns(corpusRoot, f.Path)) is { } clash)
            return Stop(problems,
                $"{ManifestFile} names metadata.corpusRoot '{corpusRoot}', and the plugin tree already holds "
                + $"{clash.Path}. The export is copied there, so one would overwrite the other.");

        var exportManifest = Parse(Text(source.Export, Exporter.ManifestFile));
        if (exportManifest is null)
            return Stop(problems,
                $"the export holds no readable {Exporter.ManifestFile}. Run the export first: ./kac export");

        // What the export actually carried, which is what decides the trimming below. A type the corpus
        // adopted and exported nothing for is absent here, and a component reading it would find nothing.
        var carried = Types(exportManifest);

        var declared = Components(Object(manifest["metadata"])?["components"]);
        var included = new List<PluginComponent>();
        var trimmed = new List<TrimmedComponent>();

        foreach (var component in declared)
        {
            var missing = component.Requires.Where(r => !carried.Contains(r)).ToList();
            if (missing.Count == 0)
                included.Add(component);
            else
                trimmed.Add(new TrimmedComponent(component.Path, component.Requires,
                    $"the export carries no {string.Join(" or ", missing)}"));
        }

        // A plugin with nothing left in it is still assembled. Refusing would leave a corpus unable to
        // build the thing that would have told it why, and the empty plugin is itself the report: it
        // installs, does nothing, and `bundle.json` beside it names every component that was dropped.
        if (included.Count == 0)
            warnings.Add(declared.Count == 0
                ? $"{ManifestFile} declares no components under metadata.components, so the plugin carries "
                  + "the export and nothing that reads it."
                : $"every component was trimmed — the plugin carries the export and nothing that reads it. "
                  + $"{RecordFile} names each one and the type it needed.");

        // The plugin's version is the corpus content version, taken from the export rather than from
        // `.corpus.yaml`, so the number on the plugin is the number of the data inside it. The export
        // format version stays where it is, in the export's own manifest.
        var version = Str(exportManifest["contentVersion"]);
        if (version is null)
            warnings.Add("the export states no contentVersion, so the plugin keeps the version its own "
                         + "manifest states. Set content-version in .corpus.yaml.");

        // The plugin tree, less every subtree a trimmed component owns, and less the manifest — which is
        // rewritten below rather than copied. A path under no component's is unconditional and travels
        // whatever the corpus adopted.
        var files = (from file in source.PluginTree
            where file.Path != ManifestFile
            where !trimmed.Any(t => Owns(t.Path, file.Path))
            select file with { Path = $"{Dist.PluginDir}/{file.Path}" }).ToList();

        // The export, copied byte for byte and edited by nothing. `bundle` writes its own account of the
        // run beside it rather than into it, so the two copies of the export stay comparable and a
        // difference between them is a defect rather than a thing to interpret.
        files.AddRange(source.Export.Select(file => file with { Path = $"{Dist.PluginDir}/{corpusRoot}/{file.Path}" }));

        files.Add(Utf8($"{Dist.PluginDir}/{ManifestFile}",
            Rewrite(manifest, version, included)));

        files.Add(Utf8($"{Dist.PluginDir}/{RecordFile}",
            Serialize(new BundleRecord(
                RecordVersion, pluginName, version ?? Str(manifest["version"]), corpusRoot,
                new BundleExport(
                    Int(exportManifest["formatVersion"]), Str(exportManifest["corpus"]),
                    Str(exportManifest["contentVersion"]), [.. carried]),
                [.. included.Select(c => new BundleIncluded(c.Path, c.Requires, c.Note))],
                [.. trimmed.Select(t => new BundleTrimmed(t.Path, t.Requires, t.Reason))]))));

        files.Add(Utf8(Dist.MarketplaceRel, Marketplace(manifest, pluginName)));

        return new BundlePlan(
            [.. files.OrderBy(f => f.Path, StringComparer.Ordinal)],
            pluginName, version ?? Str(manifest["version"]), included, trimmed, warnings, problems);
    }

    // Replace the bundle whole: delete the two directories it owns, then write. The same reasoning as
    // the export's — a component dropped from the manifest must not stay readable in an artefact nobody
    // reviews — and the same reason it names its directories rather than `.dist/`: the export is under
    // the same root and a bundle is not entitled to take it.
    public static List<string> Write(string repoRoot, BundlePlan plan)
    {
        foreach (var owned in new[] { Dist.Plugin, $"{Dist.Root}/{Dist.MarketplaceDir}" })
        {
            var dir = Path.Combine(repoRoot, owned.Replace('/', Path.DirectorySeparatorChar));
            if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
        }

        var root = Path.Combine(repoRoot, Dist.Root);
        var written = new List<string>();
        foreach (var file in plan.Files)
        {
            var full = Path.Combine(root, file.Path.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(full)!);
            File.WriteAllBytes(full, file.Content);
            written.Add($"{Dist.Root}/{file.Path}");
        }

        return written;
    }

    // A directory tree read into the shape `Plan` takes, named relative to its own root. Missing returns
    // null, which is how the caller tells "no plugin tree" from "an empty one" without asking twice.
    public static IReadOnlyList<BundleFile>? Read(string root)
    {
        if (!Directory.Exists(root)) return null;

        return
        [
            .. Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
                .Select(f => new BundleFile(
                    Path.GetRelativePath(root, f).Replace('\\', '/'), File.ReadAllBytes(f)))
                .OrderBy(f => f.Path, StringComparer.Ordinal)
        ];
    }

    // The plugin manifest as it will travel: the version replaced, the trimmed components gone, and
    // every other key exactly as the corpus wrote it.
    //
    // The document is edited as a DOM rather than mapped onto a record, because `plugin.json` is the
    // corpus's own file and a corpus may write keys this tool has never heard of. Reading it into a
    // shape known here and writing that back out would delete them without a word.
    private static string Rewrite(JsonObject manifest, string? version, List<PluginComponent> included)
    {
        var copy = (JsonObject)manifest.DeepClone();
        if (version is not null) copy["version"] = version;

        if (Object(copy["metadata"]) is { } metadata && metadata["components"] is JsonArray components)
        {
            var kept = new JsonArray();
            foreach (var node in components)
                if (node is JsonObject c && included.Any(i => i.Path == Str(c["path"])))
                    kept.Add(c.DeepClone());

            metadata["components"] = kept;
        }

        return copy.ToJsonString(Indented) + "\n";
    }

    // The local marketplace, so the plugin can be installed from a path while it is being proved.
    //
    // `.dist/` is the marketplace root and the plugin sits beneath it as `./plugin`. That is not a
    // preference: a marketplace resolves a plugin source against the directory holding
    // `.claude-plugin/`, and refuses a source containing `..` — so a marketplace beside the plugin
    // could not point at it.
    private static string Marketplace(JsonObject manifest, string pluginName) =>
        Serialize(new MarketplaceManifest(
            "https://anthropic.com/claude-code/marketplace.schema.json",
            $"{pluginName}-local",
            $"A local marketplace holding the {pluginName} plugin, for installing it from a path.",
            new MarketplaceOwner(Owner(manifest) ?? pluginName),
            [
                new MarketplacePlugin(
                    pluginName,
                    Str(manifest["description"]) ?? $"The {pluginName} plugin, built from this corpus.",
                    $"./{Dist.PluginDir}")
            ]));

    // Who the marketplace belongs to, taken from whoever the plugin says wrote it. `author` is a string
    // in some manifests and an object with a `name` in others, and both are read rather than one being
    // declared correct.
    private static string? Owner(JsonObject manifest) =>
        Str(manifest["author"]) ?? Str(Object(manifest["author"])?["name"]);

    // The types the export carried, read off its manifest. A type that contributed no record is absent
    // from that list, which is exactly the state a component reading it would find nothing in.
    private static HashSet<string> Types(JsonObject exportManifest)
    {
        var types = new HashSet<string>(StringComparer.Ordinal);
        if (exportManifest["types"] is not JsonArray declared) return types;

        foreach (var node in declared)
            if (Str(Object(node)?["type"]) is { } key)
                types.Add(key);

        return types;
    }

    // The components a manifest declares. A component with no `path` is skipped: the path is what a
    // trim acts on, and one without it could be neither included nor left out.
    private static List<PluginComponent> Components(JsonNode? node)
    {
        var components = new List<PluginComponent>();
        if (node is not JsonArray declared) return components;

        foreach (var entry in declared)
        {
            if (Object(entry) is not { } component) continue;
            if (Str(component["path"]) is not { } path) continue;

            var requires = component["requires"] is JsonArray r
                ? r.Select(Str).OfType<string>().ToList()
                : [];

            components.Add(new PluginComponent(path, requires, Str(component["note"])));
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

    private static JsonObject? Parse(string? text)
    {
        if (text is null) return null;

        try
        {
            return JsonNode.Parse(text) as JsonObject;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static JsonObject? Object(JsonNode? node) => node as JsonObject;

    // A JSON value read as a string, or null where it is absent, blank or some other type. The manifest
    // is the corpus's own file and nothing validates it before this reads it, so every read of it
    // survives a value of the wrong shape rather than throwing over one.
    private static string? Str(JsonNode? node) =>
        node is JsonValue value && value.TryGetValue<string>(out var s) && s.Length > 0 ? s : null;

    private static int? Int(JsonNode? node) =>
        node is JsonValue value && value.TryGetValue<int>(out var i) ? i : null;

    // How the plugin manifest is written back out. Stated here rather than borrowed from `KacJson`,
    // because this is a DOM write rather than a serialized record: the two rules that matter are that a
    // person reads the file, and that an em dash in a corpus's own description reaches them as itself.
    private static readonly JsonSerializerOptions Indented = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static string Serialize(BundleRecord record) =>
        JsonSerializer.Serialize(record, KacJson.Relaxed.BundleRecord) + "\n";

    private static string Serialize(MarketplaceManifest manifest) =>
        JsonSerializer.Serialize(manifest, KacJson.Relaxed.MarketplaceManifest) + "\n";
}
