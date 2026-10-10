namespace kac.core;

// One type of a consumed corpus, as that corpus published it. Lines and records stay text, so a consumer
// stamps its producer's shortcode onto the id keys and carries the rest through untouched. `Json.cs` says
// why the four key names are read from the manifest.
public sealed record InheritedType(
    string Type,
    int ShapeVersion,
    string Dir,
    string? PartsFile,
    string? RecordKey,
    string? PartKey,
    string? IdKey,
    string? SeeAlsoKey,
    IReadOnlyDictionary<string, string> Sections,
    IReadOnlyList<string> PartLines,
    IReadOnlyList<InheritedRecord> Records);

// `Producer` is the corpus that wrote the record, which for an inherited one is further up the chain.
public sealed record InheritedRecord(string Producer, string Name, string Content);

// `Publishing`, `Tracker` and `ReposBase` are the producer's own, because a record of theirs is read and
// reported in their repository. `Sources` is what the producer inherited in turn, carried forward so a chain
// of any depth needs no code.
public sealed record InheritedCorpus(
    string Shortcode,
    int FormatVersion,
    string? Corpus,
    string? ContentVersion,
    ExportPublishing Publishing,
    string? ReposBase,
    ExportTracker Tracker,
    IReadOnlyList<ExportSource> Sources,
    IReadOnlyList<InheritedType> Types);

// The exports a corpus consumes, read as the bytes an export carries. `Imports.cs` reads the same folders
// for `validate`.
public static class Inherited
{
    // `names` lists the files directly inside a folder under `.imports/`, `folders` the folders inside one, and
    // `read` returns one file's text. Each returns null where there is nothing there. An entry with no shortcode
    // is skipped, because `validate` reports it.
    public static (IReadOnlyList<InheritedCorpus> Carried, IReadOnlyList<string> Missing) Read(
        IReadOnlyList<Consumed> declared,
        Func<string, IReadOnlyList<string>?> names,
        Func<string, IReadOnlyList<string>?> folders,
        Func<string, string?> read)
    {
        var carried = new List<InheritedCorpus>();
        var missing = new List<string>();

        foreach (var entry in declared)
        {
            if (entry.Shortcode is not { } shortcode) continue;

            if (One(shortcode, names, folders, read) is { } corpus) carried.Add(corpus);
            else missing.Add(shortcode);
        }

        return (carried, missing);
    }

    // Null where the folder has no manifest. A restore writes the manifest last, so a folder with one has every
    // file it describes.
    private static InheritedCorpus? One(
        string shortcode,
        Func<string, IReadOnlyList<string>?> names,
        Func<string, IReadOnlyList<string>?> folders,
        Func<string, string?> read)
    {
        if (Exporter.ReadManifest(read($"{shortcode}/{Exporter.ManifestFile}")) is not { } manifest) return null;

        var types = new List<InheritedType>();

        foreach (var declared in manifest.Types)
            types.Add(new InheritedType(
                declared.Type,
                declared.ShapeVersion,
                declared.Dir,
                declared.PartsFile,
                declared.RecordKey,
                declared.PartKey,
                declared.IdKey,
                declared.SeeAlsoKey,
                declared.Sections,
                Lines(declared.PartsFile is null ? null : read($"{shortcode}/{declared.PartsFile}")),
                RecordsIn(shortcode, declared.Dir, declared.PartsFile, names, folders, read)));

        return new InheritedCorpus(
            shortcode,
            manifest.FormatVersion,
            manifest.Corpus,
            manifest.ContentVersion,
            manifest.Publishing,
            manifest.ReposBase,
            manifest.Tracker,
            manifest.Sources,
            types);
    }

    private static List<string> Lines(string? text) =>
        text is null ? [] : [.. text.Split('\n').Where(l => l.Length > 0)];

    // A record is a `.json` named for its id, and the parts file beside them is not one. Inherited records sit
    // one folder deep, named for their writer. See docs/design/export.md.
    private static List<InheritedRecord> RecordsIn(
        string shortcode, string dir, string? partsFile,
        Func<string, IReadOnlyList<string>?> names,
        Func<string, IReadOnlyList<string>?> folders,
        Func<string, string?> read)
    {
        var found = new List<InheritedRecord>();
        var partsName = partsFile?[(partsFile.LastIndexOf('/') + 1)..];
        var root = $"{shortcode}/{dir}";

        Take(shortcode, root);

        foreach (var producer in (folders(root) ?? []).OrderBy(n => n, StringComparer.Ordinal))
            Take(producer, $"{root}/{producer}");

        return found;

        void Take(string producer, string folder)
        {
            foreach (var name in (names(folder) ?? []).OrderBy(n => n, StringComparer.Ordinal))
            {
                if (!name.EndsWith(".json", StringComparison.Ordinal)) continue;
                if (name == partsName) continue;
                if (read($"{folder}/{name}") is not { } content) continue;

                found.Add(new InheritedRecord(producer, name, content));
            }
        }
    }

    public static (IReadOnlyList<InheritedCorpus> Carried, IReadOnlyList<string> Missing) Read(
        string corpusRoot, IReadOnlyList<Consumed> declared)
    {
        var root = Path.Combine(corpusRoot, Restore.ImportsDir);

        return Read(
            declared,
            folder => Directory.Exists(At(folder))
                ? [.. Directory.EnumerateFiles(At(folder)).Select(Path.GetFileName).OfType<string>()]
                : null,
            folder => Directory.Exists(At(folder))
                ? [.. Directory.EnumerateDirectories(At(folder)).Select(Path.GetFileName).OfType<string>()]
                : null,
            file => File.Exists(At(file)) ? Files.ReadLf(At(file)) : null);

        string At(string relative) => Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
    }
}
