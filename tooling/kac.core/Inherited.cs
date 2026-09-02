namespace kac.core;

// One type of a consumed corpus, as that corpus published it.
//
// The lines and the records arrive as text rather than as parsed objects. A consumer merging them stamps
// its producer's shortcode onto the keys that hold an id and carries the rest through untouched, so what
// a producer wrote is what a consumer publishes.
//
// The four key names are the producing type's own words, read from its manifest. `Json.cs` says why a
// type publishes them and what assuming a spelling would cost.
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

// One record file of a consumed corpus: the name it was published under, and its bytes.
public sealed record InheritedRecord(string Name, string Content);

// One corpus this one consumes, read for what an export has to carry of it.
//
// `Publishing` is the producer's own block. A record of theirs is read at their commit, under their path
// prefix, in their repository, and a consumer's own block gets all three wrong.
//
// `FormatVersion` is the envelope the producer wrote. A consumer merging its files has to read every key
// it stamps, so an envelope this build does not know is refused rather than merged around.
//
// `Sources` is what this corpus inherited in turn. A grandparent's records arrive inside its child's
// export already labelled, and their address lives in the child's `sources` rather than anywhere this
// corpus could work out. Carrying the list forward is what makes a chain of any depth cost no code.
public sealed record InheritedCorpus(
    string Shortcode,
    int FormatVersion,
    string? Corpus,
    string? ContentVersion,
    ExportPublishing Publishing,
    IReadOnlyList<ExportSource> Sources,
    IReadOnlyList<InheritedType> Types);

// The exports a corpus consumes, read as the bytes an export will carry rather than as the facts a check
// asks. `Imports.cs` is the other reader, and it projects the same folders down to what `validate` needs.
//
// Two readers because they answer two questions. A check asks whether a citation resolves, which is a
// fact about ids. An export asks what to publish, which is the files themselves. One reader answering
// both would hand each caller most of what it wanted and a little of what it did not.
//
// The reading is a pair of functions for the reason `Tree` and `Imports` take the same pair. What an
// export comes to stays decidable from a set of strings.
public static class Inherited
{
    // Every declared import that is on disk, and the shortcode of each that is not.
    //
    // `names` answers the file names directly inside one folder under `.imports/`, and null where there
    // is no folder there. `read` answers one file's text, and null where there is no file.
    //
    // An entry naming no shortcode is skipped rather than reported. It has no folder to look in, so
    // nothing could be read for it, and `validate` is what names a declaration that cannot resolve.
    public static (IReadOnlyList<InheritedCorpus> Carried, IReadOnlyList<string> Missing) Read(
        IReadOnlyList<Consumed> declared,
        Func<string, IReadOnlyList<string>?> names,
        Func<string, string?> read)
    {
        var carried = new List<InheritedCorpus>();
        var missing = new List<string>();

        foreach (var entry in declared)
        {
            if (entry.Shortcode is not { } shortcode) continue;

            if (One(shortcode, names, read) is { } corpus) carried.Add(corpus);
            else missing.Add(shortcode);
        }

        return (carried, missing);
    }

    // One folder read whole, or null where it holds no manifest. The manifest is the last file a restore
    // writes, so a folder carrying one carries the files it describes.
    private static InheritedCorpus? One(
        string shortcode, Func<string, IReadOnlyList<string>?> names, Func<string, string?> read)
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
                RecordsIn(shortcode, declared.Dir, declared.PartsFile, names, read)));

        return new InheritedCorpus(
            shortcode,
            manifest.FormatVersion,
            manifest.Corpus,
            manifest.ContentVersion,
            manifest.Publishing,
            manifest.Sources,
            types);
    }

    // A parts file as its lines, with the blank one a trailing newline leaves taken out. Nothing here
    // parses a line: a consumer stamps the keys it was told hold an id and carries the rest through.
    private static List<string> Lines(string? text) =>
        text is null ? [] : [.. text.Split('\n').Where(l => l.Length > 0)];

    // Every record file one type's folder holds. A record is a `.json` named for its id, and the parts
    // file sitting beside them is not one.
    private static List<InheritedRecord> RecordsIn(
        string shortcode, string dir, string? partsFile,
        Func<string, IReadOnlyList<string>?> names, Func<string, string?> read)
    {
        var found = new List<InheritedRecord>();
        var partsName = partsFile?[(partsFile.LastIndexOf('/') + 1)..];

        foreach (var name in (names($"{shortcode}/{dir}") ?? []).OrderBy(n => n, StringComparer.Ordinal))
        {
            if (!name.EndsWith(".json", StringComparison.Ordinal)) continue;
            if (name == partsName) continue;
            if (read($"{shortcode}/{dir}/{name}") is not { } content) continue;

            found.Add(new InheritedRecord(name, content));
        }

        return found;
    }

    // How the folders are actually read, which nothing but a run against a real corpus uses.
    public static (IReadOnlyList<InheritedCorpus> Carried, IReadOnlyList<string> Missing) Read(
        string corpusRoot, IReadOnlyList<Consumed> declared)
    {
        var root = Path.Combine(corpusRoot, Restore.ImportsDir);

        return Read(
            declared,
            folder => Directory.Exists(At(folder))
                ? [.. Directory.EnumerateFiles(At(folder)).Select(Path.GetFileName).OfType<string>()]
                : null,
            file => File.Exists(At(file)) ? Files.ReadLf(At(file)) : null);

        string At(string relative) => Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
    }
}
