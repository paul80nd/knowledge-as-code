using System.Text;
using System.Text.Json.Nodes;

namespace kac.core;

// A few lines injected at the start of a session, saying which corpus is installed, how much of it
// there is and what to ask. `tooling/features/bundle.md` says why it is rendered here rather than
// computed at runtime, and why nothing in the render names a record type.
public static class Breadcrumb
{
    // Where the rendered file lands inside the plugin: in the hook directory, beside the two scripts
    // that print it. `Bundler` decides whether it is written at all, from that directory surviving.
    public const string RenderedFile = "hooks/breadcrumb.txt";

    // The breadcrumb as it will be printed. `exportFiles` is the export as it will travel, which is
    // where the record names come from: the manifest counts records without naming them, and a count
    // alone does not tell a reader which contexts are covered.
    public static string Render(
        JsonObject exportManifest,
        IReadOnlyList<BundleFile> exportFiles,
        IReadOnlyList<PluginComponent> included)
    {
        var lines = new List<string>();

        var corpus = JsonRead.Str(exportManifest["corpus"]) ?? "A knowledge corpus";
        var version = JsonRead.Str(exportManifest["contentVersion"]);
        var taken = JsonRead.Str(exportManifest["generatedAt"]) is { Length: >= 10 } stamp ? stamp[..10] : null;

        lines.Add(
            $"{corpus}{(version is null ? "" : $" {version}")} travels with this session as data"
            + $"{(taken is null ? "" : $", exported {taken}")}.");

        foreach (var type in Types(exportManifest))
            lines.Add(Line(type, Names(type, exportFiles)));

        // What to ask, named from the components that actually survived the trim rather than from a
        // name written here. A plugin carrying no skill has nothing to point at, and says nothing.
        var skills = included
            .Where(c => c.Path.StartsWith("skills/", StringComparison.Ordinal))
            .Select(c => c.Path["skills/".Length..])
            .Where(name => name.Length > 0 && !name.Contains('/', StringComparison.Ordinal))
            .ToList();

        if (skills.Count > 0)
            lines.Add($"Ask the {Join(skills)} skill{(skills.Count > 1 ? "s" : "")} before you infer "
                      + "what a word here means.");

        return string.Join("\n", lines) + "\n";
    }

    // The most things one line will name, the remainder counted among them. What a session pays for this
    // text is fixed here rather than by however many records a corpus happens to hold: a type is named in
    // one line whether it carries three records or three hundred, and the line is read at every start,
    // resume, clear and compact.
    //
    // Six because the names are doing a job that stops at a handful. A reader scanning three or six
    // titles learns which contexts a type covers; one scanning two hundred has been handed the corpus
    // where a breadcrumb was meant.
    private const int MostNamed = 6;

    // One type's line: how much of it there is, and which records it is held in.
    private static string Line(ExportedType type, IReadOnlyList<string> names)
    {
        var held = $"{type.Records} record{(type.Records == 1 ? "" : "s")}";
        var body = type.Parts > 0
            ? $"{type.Parts} entr{(type.Parts == 1 ? "y" : "ies")} across {held}"
            : held;

        var named = Named(names);
        return named.Count > 0
            ? $"{type.Type}. {body}: {Join(named)}."
            : $"{type.Type}. {body}.";
    }

    // The names as the line will carry them: all of them where a type holds few, and the first of them
    // followed by a count of what is left where it holds many.
    //
    // The remainder is named rather than dropped. A list cut short silently reads as the whole of what
    // the type covers, which is the one thing a reader would carry away wrongly. It is the count,
    // not the titles, that the line exists to make.
    private static IReadOnlyList<string> Named(IReadOnlyList<string> names)
    {
        if (names.Count <= MostNamed) return names;

        var named = names.Take(MostNamed - 1).ToList();
        named.Add($"{names.Count - named.Count} more");
        return named;
    }

    // The records of one type, named as the export names them. Read from the per-record files rather
    // than from the flat parts file: a part line carries the record's id and never its title, and an id
    // is what a reader decodes where a title is what they recognise.
    private static IReadOnlyList<string> Names(ExportedType type, IReadOnlyList<BundleFile> exportFiles)
    {
        var prefix = type.Dir + "/";
        var names = new List<string>();

        foreach (var file in exportFiles)
        {
            // A record file, which is every `.json` in the type's directory bar the flat file the
            // manifest names. Both tests are asked: today the flat file is `.jsonl` and the extension
            // alone would settle it, but the name is the manifest's to choose and this reads it.
            if (!file.Path.StartsWith(prefix, StringComparison.Ordinal)) continue;
            if (file.Path == type.PartsFile) continue;
            if (!file.Path.EndsWith(".json", StringComparison.Ordinal)) continue;

            var record = JsonRead.Parse(new UTF8Encoding(false).GetString(file.Content));
            var fields = record?["fields"] as JsonObject;

            // Title, then id, then the filename. A type whose `export:` block withholds both still gets
            // a name in the breadcrumb rather than a gap where one record should be.
            names.Add(JsonRead.Str(fields?["title"])
                      ?? JsonRead.Str(fields?["id"])
                      ?? Path.GetFileNameWithoutExtension(file.Path));
        }

        return names;
    }

    // The types the export carried, read back into the record the exporter wrote them from. Restating
    // that shape here would be a second account of one document, free to drift from the first.
    private static List<ExportedType> Types(JsonObject exportManifest)
    {
        var types = new List<ExportedType>();
        if (exportManifest["types"] is not JsonArray declared) return types;

        foreach (var node in declared)
        {
            if (node is not JsonObject entry) continue;
            if (JsonRead.Str(entry["type"]) is not { } key) continue;

            types.Add(new ExportedType(
                key,
                JsonRead.Int(entry["records"]) ?? 0,
                JsonRead.Int(entry["parts"]) ?? 0,
                JsonRead.Str(entry["dir"]) ?? key,
                JsonRead.Str(entry["partsFile"])));
        }

        return types;
    }

    // An English list, because the breadcrumb is read by a person as often as by an agent and a
    // comma-separated tail reads as a fragment where the last pair is joined.
    private static string Join(IReadOnlyList<string> items) =>
        items.Count switch
        {
            0 => "",
            1 => items[0],
            _ => string.Join(", ", items.Take(items.Count - 1)) + " and " + items[^1]
        };
}
