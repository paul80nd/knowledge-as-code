using System.Text.Json.Nodes;

namespace kac.core;

// One record another corpus published, as the export under `.imports/` states it.
//
// Everything a check asks of a local record, answered from data rather than from a parsed document. The
// producer's corpus is not here and never will be: what a consumer holds is the export, so a question
// the export cannot answer is one no check may ask across a boundary.
//
// `Parts` is empty for a type keeping none, and `KeepsParts` is what tells that apart from a record that
// happens to carry none.
public sealed record ImportedRecord(
    string Id, string Type, string Path, bool KeepsParts, IReadOnlyList<string> Parts);

// One corpus this one imported, unpacked under `.imports/<shortcode>/`.
//
// `Link` is the producer's own template for a published record, taken from their manifest. A consumer
// building a link from its own publishing would address its own repository and resolve to nothing.
public sealed record Import(
    string Shortcode, string Corpus, string Version, string? Link, IReadOnlyList<ImportedRecord> Records);

// What `.imports/` answered for what `consumes:` declared. `NotRestored` names each shortcode whose
// declaration stands and whose folder does not, which `validate` reports rather than works around.
public sealed record ImportGraph(IReadOnlyList<Import> Imports, IReadOnlyList<string> NotRestored)
{
    // A corpus standing on its own, which is the ordinary case and pays nothing for this.
    public static readonly ImportGraph None = new([], []);
}

// The exports a corpus imported, read from the folders `kac restore` unpacked them into.
//
// **`.imports/` is not the corpus.** `Tree` lists what git tracks and a restored artefact is not
// committed, so nothing here comes through it. That separation is the point rather than an awkwardness:
// an import is somebody else's published record, and a check that walked it as though it were local
// would hold this corpus to a rule the other corpus answers for.
//
// The reading is a pair of functions for the reason `Tree` takes the same pair. What a corpus resolves
// against stays decidable from a set of strings.
public static class Imports
{
    // Every declared import that is on disk, and the shortcode of each that is not.
    //
    // `names` answers the file names directly inside one folder under `.imports/`, and null where there
    // is no folder there. `read` answers one file's text, and null where there is no file.
    public static ImportGraph Load(
        IReadOnlyList<Consumed> declared,
        Func<string, IReadOnlyList<string>?> names,
        Func<string, string?> read)
    {
        var imports = new List<Import>();
        var missing = new List<string>();

        foreach (var entry in declared)
        {
            // A declaration too broken to name a folder is `restore`'s to refuse, and it says so in
            // sentences this pass has no business repeating.
            if (entry.Shortcode is not { } shortcode) continue;

            if (Read(shortcode, names, read) is { } import) imports.Add(import);
            else missing.Add(shortcode);
        }

        return new ImportGraph(imports, missing);
    }

    // One folder read whole, or null where it holds no manifest. The manifest is the last file a restore
    // writes, so a folder carrying one carries the records it describes.
    private static Import? Read(
        string shortcode, Func<string, IReadOnlyList<string>?> names, Func<string, string?> read)
    {
        if (read($"{shortcode}/{Exporter.ManifestFile}") is not { } text) return null;
        if (JsonRead.Parse(text) is not { } manifest) return null;

        var records = new List<ImportedRecord>();

        foreach (var declared in manifest["types"] as JsonArray ?? [])
        {
            var type = JsonRead.Object(declared);
            if (JsonRead.Str(type?["type"]) is not { } key) continue;
            if (JsonRead.Str(type?["dir"]) is not { } dir) continue;

            var partsFile = JsonRead.Str(type?["partsFile"]);
            var parts = partsFile is null
                ? []
                : PartsIn(read($"{shortcode}/{partsFile}"));

            records.AddRange(
                RecordsIn(shortcode, key, dir, partsFile, partsFile is not null, parts, names, read));
        }

        return new Import(
            shortcode,
            JsonRead.Str(manifest["corpus"]) ?? shortcode,
            JsonRead.Str(manifest["contentVersion"]) ?? "",
            JsonRead.Str(JsonRead.Object(manifest["publishing"])?["humanTemplate"]),
            records);
    }

    // Every record one type's folder holds. A record is a file named for its id, and the parts file
    // sitting beside them is not one.
    private static List<ImportedRecord> RecordsIn(
        string shortcode, string type, string dir, string? partsFile, bool keepsParts,
        Dictionary<string, List<string>> parts,
        Func<string, IReadOnlyList<string>?> names, Func<string, string?> read)
    {
        var found = new List<ImportedRecord>();
        var partsName = partsFile is null ? null : partsFile[(partsFile.LastIndexOf('/') + 1)..];

        foreach (var name in names($"{shortcode}/{dir}") ?? [])
        {
            if (!name.EndsWith(".json", StringComparison.Ordinal)) continue;
            if (name == partsName) continue;

            var record = JsonRead.Parse(read($"{shortcode}/{dir}/{name}") ?? "");
            if (JsonRead.Str(JsonRead.Object(record?["fields"])?["id"]) is not { } id) continue;

            found.Add(new ImportedRecord(
                id, type, JsonRead.Str(record?["path"]) ?? "", keepsParts,
                parts.GetValueOrDefault(id, [])));
        }

        return found;
    }

    // The parts each record carries, in the order the file lists them. A parts file is one JSON object
    // per line, so a line this build cannot read is skipped rather than failing the whole import: the
    // producer's export is held to its own shape by the producer's own build.
    private static Dictionary<string, List<string>> PartsIn(string? text)
    {
        var parts = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        if (text is null) return parts;

        foreach (var line in text.Split('\n'))
        {
            if (line.Length == 0) continue;
            if (JsonRead.Parse(line) is not { } part) continue;
            if (JsonRead.Str(part["record"]) is not { } record) continue;
            if (JsonRead.Str(part["part"]) is not { } id) continue;

            if (!parts.TryGetValue(record, out var carried)) parts[record] = carried = [];
            carried.Add(id);
        }

        return parts;
    }

    // How the folders are actually read, which nothing but a run against a real corpus uses.
    public static ImportGraph Load(string corpusRoot, IReadOnlyList<Consumed> declared)
    {
        var root = Path.Combine(corpusRoot, Restore.ImportsDir);

        return Load(
            declared,
            folder => Directory.Exists(At(folder))
                ? [.. Directory.EnumerateFiles(At(folder)).Select(Path.GetFileName).OfType<string>()]
                : null,
            file => File.Exists(At(file)) ? Files.ReadLf(At(file)) : null);

        string At(string relative) => Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
    }
}
