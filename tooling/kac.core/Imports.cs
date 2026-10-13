using System.Text.Json.Nodes;

namespace kac.core;

/// <summary>
/// A question the export cannot answer is one no check may ask across a boundary. <c>Scope</c> is the shortcode a
/// citation writes for the record. <c>KeepsParts</c> tells a type keeping no parts from a record carrying none.
/// </summary>
public sealed record ImportedRecord(
    string Scope,
    string Id,
    string Type,
    string Path,
    bool KeepsParts,
    IReadOnlyList<string> Parts);

/// <summary>
/// <c>Link</c> is the producer's own template: one built from this corpus's publishing would resolve to nothing.
/// </summary>
public sealed record Import(
    string Shortcode,
    string Corpus,
    string Version,
    string? Link,
    IReadOnlyList<ImportedRecord> Records);

/// <summary>
/// <c>Undeclared</c> holds the entries naming no shortcode, each named by its corpus or by its position.
/// </summary>
public sealed record ImportGraph(
    IReadOnlyList<Import> Imports,
    IReadOnlyList<string> NotRestored,
    IReadOnlyList<string> Undeclared)
{
    public static readonly ImportGraph None = new([], [], []);
}

/// <summary>
/// <c>.imports/</c> is not the corpus: <c>Tree</c> lists what git tracks, and a restored artefact is not committed. The
/// reading is a pair of functions, as <c>Tree</c> takes, so resolution is decidable from strings.
/// </summary>
public static class Imports
{
    /// <summary>
    /// Each of <c>names</c>, <c>folders</c> and <c>read</c> returns null where there is nothing there.
    /// </summary>
    public static ImportGraph Load(
        IReadOnlyList<Consumed> declared,
        Func<string, IReadOnlyList<string>?> names,
        Func<string, IReadOnlyList<string>?> folders,
        Func<string, string?> read)
    {
        var imports = new List<Import>();
        var missing = new List<string>();
        var nameless = new List<string>();

        for (var i = 0; i < declared.Count; i++)
        {
            // Reported here, because otherwise the entry would pass in silence. `restore` says why it is wrong.
            if (declared[i].Shortcode is not { } shortcode)
            {
                nameless.Add(declared[i].Corpus is { } corpus ? $"'{corpus}'" : $"entry {i + 1}");
                continue;
            }

            if (Read(shortcode, names, folders, read) is { } import) imports.Add(import);
            else missing.Add(shortcode);
        }

        return new ImportGraph(imports, missing, nameless);
    }

    /// <summary>The manifest is the last file a restore writes, so a folder carrying one carries its records.</summary>
    private static Import? Read(
        string shortcode,
        Func<string, IReadOnlyList<string>?> names,
        Func<string, IReadOnlyList<string>?> folders,
        Func<string, string?> read)
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

            // Read from what the producer published. `Json.cs` says why. An export carrying neither falls back to
            // the two names every type has used.
            var parts = partsFile is null
                ? []
                : PartsIn(
                    read($"{shortcode}/{partsFile}"),
                    JsonRead.Str(type?["recordKey"]) ?? "record",
                    JsonRead.Str(type?["partKey"]) ?? "part");

            records.AddRange(RecordsIn(shortcode, key, dir, partsFile, parts, names, folders, read));
        }

        return new Import(
            shortcode,
            JsonRead.Str(manifest["corpus"]) ?? shortcode,
            JsonRead.Str(manifest["contentVersion"]) ?? "",
            JsonRead.Str(JsonRead.Object(manifest["publishing"])?["humanTemplate"]),
            records);
    }

    /// <summary>
    /// Inherited records sit in a folder named for the corpus that wrote them. See the chain section of
    /// docs/reference/export.md.
    /// </summary>
    private static List<ImportedRecord> RecordsIn(
        string shortcode, string type, string dir, string? partsFile,
        Dictionary<string, List<string>> parts,
        Func<string, IReadOnlyList<string>?> names,
        Func<string, IReadOnlyList<string>?> folders,
        Func<string, string?> read)
    {
        var found = new List<ImportedRecord>();

        var keepsParts = partsFile is not null;
        var partsName = partsFile?[(partsFile.LastIndexOf('/') + 1)..];
        var root = $"{shortcode}/{dir}";

        Take(shortcode, root);

        foreach (var scope in (folders(root) ?? []).OrderBy(n => n, StringComparer.Ordinal))
            Take(scope, $"{root}/{scope}");

        return found;

        void Take(string scope, string folder)
        {
            foreach (var name in (names(folder) ?? []).OrderBy(n => n, StringComparer.Ordinal))
            {
                if (!name.EndsWith(".json", StringComparison.Ordinal)) continue;
                if (name == partsName) continue;

                var record = JsonRead.Parse(read($"{folder}/{name}") ?? "");
                if (JsonRead.Str(JsonRead.Object(record?["fields"])?["id"]) is not { } id) continue;

                // The parts file names an inherited record by its scoped id.
                var key = scope == shortcode ? id : Exporter.Scoped(id, scope);

                found.Add(new ImportedRecord(
                    scope, id, type, JsonRead.Str(record?["path"]) ?? "", keepsParts,
                    parts.GetValueOrDefault(key, [])));
            }
        }
    }

    /// <summary>
    /// A line this build cannot read is skipped: the producer's own build holds its export to its shape.
    /// </summary>
    private static Dictionary<string, List<string>> PartsIn(string? text, string recordKey, string partKey)
    {
        var parts = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        if (text is null) return parts;

        foreach (var line in text.Split('\n'))
        {
            if (line.Length == 0) continue;
            if (JsonRead.Parse(line) is not { } part) continue;
            if (JsonRead.Str(part[recordKey]) is not { } record) continue;
            if (JsonRead.Str(part[partKey]) is not { } id) continue;

            if (!parts.TryGetValue(record, out var carried)) parts[record] = carried = [];
            carried.Add(id);
        }

        return parts;
    }

    public static ImportGraph Load(string corpusRoot, IReadOnlyList<Consumed> declared)
    {
        var root = Path.Combine(corpusRoot, Restore.ImportsDir);

        return Load(
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
