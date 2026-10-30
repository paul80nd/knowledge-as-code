using System.Text;
using System.Text.Json.Nodes;

namespace kac.core;

/// <summary>
/// A few lines injected at the start of a session, saying which corpus is installed, how much of it there is and what
/// to ask. <c>docs/maintaining/plugin.md</c> says why it is rendered at bundle time.
/// </summary>
public static class Breadcrumb
{
    /// <summary>
    /// Where the rendered file lands inside the plugin, beside the two hook scripts that print it. <c>Bundler</c>
    /// writes it only where that directory survived the trim.
    /// </summary>
    public const string RenderedFile = "hooks/breadcrumb.txt";

    /// <summary>
    /// The breadcrumb as it will be printed. <c>exportFiles</c> is the export as it will travel, which is where the
    /// record names come from, because the manifest counts records without naming them.
    /// </summary>
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

        // What to ask, named from the surviving components by the role each announces. A plugin carrying no such skill
        // says nothing. `docs/maintaining/plugin.md` explains the split.
        var routers = Announcing(included, Router);
        var lookups = Announcing(included, Lookup);
        var writeBacks = Announcing(included, WriteBack);

        if (routers.Count + lookups.Count + writeBacks.Count == 0)
            return string.Join("\n", lines) + "\n";

        lines.Add("These records are the organisation's recorded knowledge, so check them before you answer from what "
                  + "you already know.");

        var knownNeed = $"a lookup skill such as {Join(lookups)} for a known need";

        if (routers.Count > 0)
            lines.Add($"Ask {Skills(routers)} when you do not know which record answers a question"
                      + (lookups.Count > 0 ? $", and {knownNeed}." : "."));
        else if (lookups.Count > 0)
            lines.Add($"Ask {knownNeed}.");

        if (writeBacks.Count > 0)
            lines.Add($"When your change alters something these records describe, say so with {Skills(writeBacks)}.");

        return string.Join("\n", lines) + "\n";
    }

    /// <summary>The role of a skill that sends a question with no known place to the reader that answers it.</summary>
    public const string Router = "router";

    /// <summary>The role of a skill that reads one type, for a need the session can already place.</summary>
    public const string Lookup = "lookup";

    /// <summary>The role of a skill that reports a change the records no longer describe.</summary>
    public const string WriteBack = "write-back";

    /// <summary>
    /// A component's <c>announce</c> read as a role. <c>true</c> reads as a lookup, and any other value that is not a
    /// string announces nothing.
    /// </summary>
    public static string? Role(JsonNode? announce) => JsonRead.Bool(announce) ? Lookup : JsonRead.Str(announce);

    /// <summary>The skill names of the surviving components announcing <paramref name="role"/>.</summary>
    private static List<string> Announcing(IReadOnlyList<PluginComponent> included, string role) =>
        [
            .. included
                .Where(c => string.Equals(c.Announce, role, StringComparison.Ordinal))
                .Where(c => c.Path.StartsWith("skills/", StringComparison.Ordinal))
                .Select(c => c.Path["skills/".Length..])
                .Where(name => name.Length > 0 && !name.Contains('/', StringComparison.Ordinal))
        ];

    private static string Skills(IReadOnlyList<string> names) =>
        $"the {Join(names)} skill{(names.Count > 1 ? "s" : "")}";

    /// <summary>
    /// The most things one line will name, the remainder counted among them. It fixes what a line costs a session,
    /// whatever the size of the corpus behind it.
    /// </summary>
    private const int MostNamed = 6;

    /// <summary>The key this corpus's own records are filed under. Empty, because no shortcode can be.</summary>
    private const string Mine = "";

    /// <summary>
    /// How much of one type came from one corpus, and which of its records those are. <c>Source</c> is null where this
    /// corpus wrote them.
    /// </summary>
    private sealed record Contribution(string? Source, int Records, int Parts, IReadOnlyList<string> Names);

    /// <summary>
    /// A type's lines: this corpus's own records first, then one for each corpus it consumes that wrote some of them.
    /// </summary>
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

        // Whatever no source claims belongs to this corpus. The floor stops a disagreement between the manifest and
        // the files printing a negative count.
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

    /// <summary>
    /// One line: whose records these are, how much of the type they make up, and which records hold them. Either count
    /// can be zero, and "across 0 records" is never printed.
    /// </summary>
    private static string Line(string type, Contribution held)
    {
        var records = Counted.Of(held.Records, "record");
        var entries = Counted.Of(held.Parts, "entry", "entries");

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

    /// <summary>
    /// The names as the line will carry them: all of them where a type holds few, and the first few followed by a count
    /// of the rest where it holds many.
    /// </summary>
    private static IReadOnlyList<string> Named(IReadOnlyList<string> names)
    {
        if (names.Count <= MostNamed) return names;

        var named = names.Take(MostNamed - 1).ToList();
        named.Add($"{names.Count - named.Count} more");
        return named;
    }

    /// <summary>
    /// The records of one type, named as the export names them and keyed by the corpus that wrote them. Read from the
    /// per-record files, because a part line carries the record's id and never its title.
    /// </summary>
    private static Dictionary<string, List<string>> NamesBySource(
        ExportedType type,
        IReadOnlyList<BundleFile> exportFiles)
    {
        var prefix = type.Dir + "/";
        var names = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        foreach (var file in exportFiles)
        {
            // A record file is every `.json` in the type's directory bar the two flat files the manifest names.
            if (!file.Path.StartsWith(prefix, StringComparison.Ordinal)) continue;
            if (file.Path == type.PartsFile || file.Path == type.FrameworksFile) continue;
            if (!file.Path.EndsWith(".json", StringComparison.Ordinal)) continue;

            var within = file.Path[prefix.Length..];
            var slash = within.IndexOf('/', StringComparison.Ordinal);
            var source = slash < 0 ? Mine : within[..slash];

            var record = JsonRead.Parse(new UTF8Encoding(false).GetString(file.Content));
            var fields = record?["fields"] as JsonObject;

            // Title, then id, then the filename, so a type withholding both still names the record.
            var name = JsonRead.Str(fields?["title"])
                       ?? JsonRead.Str(fields?["id"])
                       ?? Path.GetFileNameWithoutExtension(file.Path);

            if (!names.TryGetValue(source, out var held)) names[source] = held = [];
            held.Add(name);
        }

        return names;
    }

    /// <summary>
    /// How many parts each corpus contributed, counted off the flat file, which is the only account of the split: a
    /// merged line carries <c>shortcode</c>.
    /// </summary>
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

    private static string Join(IReadOnlyList<string> items) =>
        items.Count switch
        {
            0 => "",
            1 => items[0],
            _ => string.Join(", ", items.Take(items.Count - 1)) + " and " + items[^1]
        };
}
