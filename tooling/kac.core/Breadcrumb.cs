using System.Text;
using System.Text.Json.Nodes;

namespace kac.core;

// A few lines injected at the start of a session, saying which corpus is installed, how much of it
// there is and what to ask. `docs/design/plugin.md` says why it is rendered here rather than computed
// at runtime, and why nothing in the render names a record type.
public static class Breadcrumb
{
    // Where the rendered file lands inside the plugin: in the hook directory, beside the two scripts
    // that print it. `Bundler` decides whether it is written at all, from that directory surviving.
    public const string RenderedFile = "hooks/breadcrumb.txt";

    // The breadcrumb as it will be printed. `exportFiles` is the export as it will travel, which is
    // where the record names come from: the manifest counts records without naming them, and a count
    // alone does not tell a reader which contexts are covered.
    public static string Render(
        ExportManifest exportManifest,
        IReadOnlyList<BundleFile> exportFiles,
        IReadOnlyList<PluginComponent> included)
    {
        var lines = new List<string>();

        var corpus = exportManifest.Corpus ?? "A knowledge corpus";
        var version = exportManifest.ContentVersion;
        var taken = exportManifest.GeneratedAt is { Length: >= 10 } stamp ? stamp[..10] : null;

        lines.Add(
            $"{corpus}{(version is null ? "" : $" {version}")} travels with this session as data"
            + $"{(taken is null ? "" : $", exported {taken}")}.");

        var sources = exportManifest.Sources.Select(s => s.Shortcode).Where(c => c.Length > 0).ToList();

        foreach (var type in exportManifest.Types)
        foreach (var held in Contributions(type, exportFiles, sources))
            lines.Add(Line(type.Type, held));

        // What to ask, named from the components that actually survived the trim rather than from a
        // name written here. A plugin carrying no skill has nothing to point at, and says nothing.
        //
        // Only the components whose manifest entry says `announce`. The line exists to create a
        // question a session would not otherwise think to ask, and a skill somebody asks for by name
        // already has its question. `docs/design/plugin.md` argues the split.
        //
        // What it warns against is answering from memory, which is the one failure every announcing
        // skill shares. Naming the question instead would name a type, and the skills that announce
        // ask three different ones: what a word means, what a rule requires, what we committed to.
        var skills = included
            .Where(c => c.Announce)
            .Where(c => c.Path.StartsWith("skills/", StringComparison.Ordinal))
            .Select(c => c.Path["skills/".Length..])
            .Where(name => name.Length > 0 && !name.Contains('/', StringComparison.Ordinal))
            .ToList();

        if (skills.Count > 0)
            lines.Add($"Ask the {Join(skills)} skill{(skills.Count > 1 ? "s" : "")} before you answer "
                      + "from what you already know.");

        return string.Join("\n", lines) + "\n";
    }

    // The most things one line will name, the remainder counted among them. What a session pays for one
    // line is fixed here rather than by however many records the corpus behind it holds: a line names
    // six titles whether it counts three records or three hundred, and every line is read at every
    // start, resume, clear and compact.
    //
    // The bound is per line, so a type merged from several corpora costs a line each. That is the
    // price of saying whose the records are, and it grows with how many corpora a corpus consumes
    // rather than with how much any of them wrote.
    //
    // Six because the names are doing a job that stops at a handful. A reader scanning three or six
    // titles learns which contexts a type covers; one scanning two hundred has been handed the corpus
    // where a breadcrumb was meant.
    private const int MostNamed = 6;

    // The key the maps below file this corpus's own records under. Empty because no shortcode can be,
    // so nothing a source is called collides with it.
    private const string Mine = "";

    // How much of one type came from one corpus, and which of its records those are. `Source` is the
    // shortcode of the corpus that wrote them, and null where this corpus wrote them itself.
    private sealed record Contribution(string? Source, int Records, int Parts, IReadOnlyList<string> Names);

    // A type's lines: this corpus's own records first, then one for each corpus it consumes that wrote
    // some of them. A merged type adds several corpora's records into one count, and a reader handed
    // that number under their own corpus's name goes looking for records nobody here wrote.
    //
    // A type nothing was merged into keeps the single line it always had, because the loop below finds
    // no source claiming any of it.
    private static IEnumerable<Contribution> Contributions(
        ExportedType type,
        IReadOnlyList<BundleFile> exportFiles,
        IReadOnlyList<string> sources)
    {
        var names = NamesBySource(type, exportFiles);
        var parts = PartsBySource(type, exportFiles);

        var inherited = new List<Contribution>();

        foreach (var code in sources)
        {
            var held = Held(names, code);
            var count = parts.GetValueOrDefault(code);

            if (held.Count > 0 || count > 0) inherited.Add(new Contribution(code, held.Count, count, held));
        }

        // The manifest counts a type whole and the files are the only account of whose the records are,
        // so each answers the half it holds. Whatever no source claims belongs to this corpus. The floor
        // is there because a disagreement between the two accounts must not print a negative count.
        var mine = new Contribution(
            null,
            Math.Max(0, type.Records - inherited.Sum(c => c.Records)),
            Math.Max(0, type.Parts - inherited.Sum(c => c.Parts)),
            Held(names, Mine));

        if (inherited.Count == 0 || mine.Records > 0 || mine.Parts > 0)
            yield return mine;

        foreach (var contribution in inherited)
            yield return contribution;
    }

    private static IReadOnlyList<string> Held(IReadOnlyDictionary<string, List<string>> names, string source) =>
        names.TryGetValue(source, out var held) ? held : [];

    // One line: whose records these are, how much of the type they make up, and which records hold them.
    //
    // A corpus reached through another sends its part lines and not its record files, so its line has
    // entries to count and no record to count them across. "across 0 records" would read as an export
    // that lost them on the way.
    private static string Line(string type, Contribution held)
    {
        var records = $"{held.Records} record{(held.Records == 1 ? "" : "s")}";
        var entries = $"{held.Parts} entr{(held.Parts == 1 ? "y" : "ies")}";

        var body = (held.Parts > 0, held.Records > 0) switch
        {
            (true, true) => $"{entries} across {records}",
            (true, false) => entries,
            _ => records
        };

        var label = held.Source is null ? type : $"{type} (from {held.Source})";
        var named = Named(held.Names);

        return named.Count > 0
            ? $"{label}. {body}: {Join(named)}."
            : $"{label}. {body}.";
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

    // The records of one type, named as the export names them and keyed by the corpus that wrote them.
    // A record this corpus wrote sits directly in the type's directory, and one it consumes sits under
    // the shortcode of the corpus that published it.
    //
    // Read from the per-record files rather than from the flat parts file: a part line carries the
    // record's id and never its title, and an id is what a reader decodes where a title is what they
    // recognise.
    private static Dictionary<string, List<string>> NamesBySource(
        ExportedType type,
        IReadOnlyList<BundleFile> exportFiles)
    {
        var prefix = type.Dir + "/";
        var names = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        foreach (var file in exportFiles)
        {
            // A record file, which is every `.json` in the type's directory bar the flat file the
            // manifest names. Both tests are asked: today the flat file is `.jsonl` and the extension
            // alone would settle it, but the name is the manifest's to choose and this reads it.
            if (!file.Path.StartsWith(prefix, StringComparison.Ordinal)) continue;
            if (file.Path == type.PartsFile) continue;
            if (!file.Path.EndsWith(".json", StringComparison.Ordinal)) continue;

            var within = file.Path[prefix.Length..];
            var slash = within.IndexOf('/', StringComparison.Ordinal);
            var source = slash < 0 ? Mine : within[..slash];

            var record = JsonRead.Parse(new UTF8Encoding(false).GetString(file.Content));
            var fields = record?["fields"] as JsonObject;

            // Title, then id, then the filename. A type whose `export:` block withholds both still gets
            // a name in the breadcrumb rather than a gap where one record should be.
            var name = JsonRead.Str(fields?["title"])
                       ?? JsonRead.Str(fields?["id"])
                       ?? Path.GetFileNameWithoutExtension(file.Path);

            if (!names.TryGetValue(source, out var held)) names[source] = held = [];
            held.Add(name);
        }

        return names;
    }

    // How many parts each corpus contributed, counted off the flat file. A merged line names its
    // producer under `shortcode` and a line this corpus wrote carries no such key, so the file is the
    // only account of the split: the manifest counts the merged type whole.
    private static Dictionary<string, int> PartsBySource(
        ExportedType type,
        IReadOnlyList<BundleFile> exportFiles)
    {
        var counted = new Dictionary<string, int>(StringComparer.Ordinal);
        if (exportFiles.FirstOrDefault(f => f.Path == type.PartsFile) is not { } flat) return counted;

        foreach (var line in new UTF8Encoding(false).GetString(flat.Content)
                     .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var source = JsonRead.Str(JsonRead.Parse(line)?["shortcode"]) ?? Mine;
            counted[source] = counted.GetValueOrDefault(source) + 1;
        }

        return counted;
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
