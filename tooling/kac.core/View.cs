using System.Text;
using System.Text.Json.Nodes;

namespace kac.core;

/// <summary>
/// A file <c>bundle</c> generates into the plugin from the export, shaped for one moment of the work. A fresh bundle
/// writes the same bytes from the same export. <c>docs/maintaining/plugin.md</c> says what each view contains.
/// </summary>
public static class View
{
    /// <summary>Every standard rule by title, id and status, for a session about to write code.</summary>
    public const string BuildIndex = "build-index";

    /// <summary>Every standard rule's obligations, and the controls that check each rule, for a reviewer.</summary>
    public const string Review = "review";

    public static readonly IReadOnlyList<string> Known = [BuildIndex, Review];

    /// <summary>
    /// What a view is rendered from. <c>Included</c> is every component that survived the trim, so a view names only a
    /// skill the plugin ships.
    /// </summary>
    public sealed record Source(
        ExportManifest Manifest,
        IReadOnlyList<BundleFile> Export,
        IReadOnlyList<PluginComponent> Included,
        string CorpusRoot);

    /// <summary>Throws for a name outside <see cref="Known"/>, which <c>Bundler.Plan</c> has already refused.</summary>
    public static string Render(string view, Source source) => view switch
    {
        BuildIndex => RenderBuildIndex(source),
        Review => RenderReview(source),
        _ => throw new ArgumentOutOfRangeException(nameof(view), view, "no such view")
    };

    private const string Standards = "standards";
    private const string Controls = "controls";

    /// <summary>
    /// One rule as a standards part line states it at shape 2. A component declaring the view pins that shape, so
    /// these keys are the shape's own.
    /// </summary>
    private sealed record Rule(
        string Id, string Record, string Title, string Status, IReadOnlyList<string> Obligations);

    private sealed record Control(
        string Id, string Title, string Status, string Mechanism, IReadOnlyList<string> Verifies);

    private static string RenderBuildIndex(Source source)
    {
        var text = new StringBuilder();
        text.Append($"# Rules the standards of {Corpus(source)} state\n\n");
        text.Append("Before you write or change code, read each rule whose title bears on the change. ");
        text.Append(ReadFullText(source)).Append("\n\n");
        text.Append("Each line gives a rule's title, then its id and the status of the standard stating it.\n\n");

        foreach (var rule in Rules(source))
            text.Append($"- {rule.Title} (`{rule.Id}`, {rule.Status})\n");

        return text.ToString();
    }

    private static string RenderReview(Source source)
    {
        var controls = ControlsOf(source);

        var text = new StringBuilder();
        text.Append($"# Review a change against the standards of {Corpus(source)}\n\n");
        text.Append("Check the change against every rule below, and cite each finding by the rule's id.\n\n");
        text.Append("- **MUST** and **MUST NOT** state a requirement. "
                    + "Report a change that breaks one as a violation.\n");
        text.Append("- **SHOULD**, **SHOULD NOT** and **MAY** state a default. A change may depart from one by giving "
                    + "its reason. Report the departure and its reason as a judgement call.\n");
        text.Append(controls is null
            ? "- This corpus exports no controls, so check every rule yourself.\n"
            : "- A rule's controls say what already checks it, and how. Check a rule yourself where no control is "
              + "listed, or where each one listed is `not-enforced`.\n");
        text.Append("\nEach rule gives the status of the standard stating it.\n");

        var titles = RecordTitles(source);
        string? record = null;

        foreach (var rule in Rules(source))
        {
            if (rule.Record != record)
            {
                record = rule.Record;
                text.Append(titles.TryGetValue(record, out var title)
                    ? $"\n## {record}: {title}\n"
                    : $"\n## {record}\n");
            }

            text.Append($"\n### {rule.Title}\n\nCite as `{rule.Id}`. Status: {rule.Status}.\n\n");

            foreach (var obligation in rule.Obligations)
                text.Append("- ").Append(obligation.Replace("\n", "\n  ", StringComparison.Ordinal)).Append('\n');

            if (controls is null) continue;

            var checking = controls.Where(c => c.Verifies.Contains(rule.Record) || c.Verifies.Contains(rule.Id))
                .Select(c => $"`{c.Id}` {c.Title} (`{c.Mechanism}`, {c.Status})")
                .ToList();

            text.Append(rule.Obligations.Count > 0 ? "\n" : "");
            text.Append(checking.Count == 0 ? "Controls: none.\n" : $"Controls: {string.Join("; ", checking)}.\n");
        }

        return text.ToString();
    }

    private static string Corpus(Source source) => source.Manifest.Corpus ?? "this corpus";

    /// <summary>
    /// The lookup skill reading standards where one survived the trim, and the rules file inside the plugin where none
    /// did.
    /// </summary>
    private static string ReadFullText(Source source)
    {
        var skill = source.Included
            .Where(c => c.Path.StartsWith("skills/", StringComparison.Ordinal))
            .FirstOrDefault(c => c.Requires.Any(r => r.Split('@')[0] == Standards));

        if (skill is not null)
            return $"Ask the {skill.Path["skills/".Length..]} skill for a rule's obligations, by its id.";

        var file = Type(source, Standards)?.PartsFile;
        return $"Read a rule's obligations in `{source.CorpusRoot}/{file}` inside this plugin, by its id.";
    }

    private static ExportedType? Type(Source source, string type) =>
        source.Manifest.Types.FirstOrDefault(t => t.Type == type);

    /// <summary>In the order the parts file states them: this corpus's own, then each consumed corpus's.</summary>
    private static List<Rule> Rules(Source source)
    {
        var type = Type(source, Standards);
        if (type?.PartsFile is not { } path || Text(source, path) is not { } lines) return [];

        var idKey = type.IdKey ?? "id";
        var recordKey = type.RecordKey ?? "record";
        var rules = new List<Rule>();

        foreach (var line in lines.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (JsonRead.Parse(line) is not { } read) continue;
            if (JsonRead.Str(read[idKey]) is not { } id) continue;

            var obligations = (read["obligations"] as JsonArray ?? [])
                .Select(o => JsonRead.Str(JsonRead.Object(o)?["text"]))
                .OfType<string>()
                .ToList();

            rules.Add(new Rule(id, JsonRead.Str(read[recordKey]) ?? id, JsonRead.Str(read["title"]) ?? id,
                JsonRead.Str(read["status"]) ?? "no status", obligations));
        }

        return rules;
    }

    /// <summary>
    /// Null where the export carries no controls. A consumed corpus's control is filed under its shortcode, and its ids
    /// are read with that shortcode, as <c>Exporter.Scoped</c> names them.
    /// </summary>
    private static List<Control>? ControlsOf(Source source)
    {
        if (Type(source, Controls) is not { } type) return null;

        var controls = new List<Control>();
        foreach (var (shortcode, fields) in RecordFields(source, type))
        {
            if (JsonRead.Str(fields["id"]) is not { } id) continue;

            var verifies = (fields["verifies"] as JsonArray ?? [])
                .Select(JsonRead.Str).OfType<string>()
                .Select(v => Scope(v, shortcode))
                .ToList();

            controls.Add(new Control(Scope(id, shortcode), JsonRead.Str(fields["title"]) ?? id,
                JsonRead.Str(fields["status"]) ?? "no status", JsonRead.Str(fields["mechanism"]) ?? "no mechanism",
                verifies));
        }

        return [.. controls.OrderBy(c => c.Id, StringComparer.Ordinal)];
    }

    private static Dictionary<string, string> RecordTitles(Source source)
    {
        var titles = new Dictionary<string, string>(StringComparer.Ordinal);
        if (Type(source, Standards) is not { } type) return titles;

        foreach (var (shortcode, fields) in RecordFields(source, type))
            if (JsonRead.Str(fields["id"]) is { } id && JsonRead.Str(fields["title"]) is { } title)
                titles.TryAdd(Scope(id, shortcode), title);

        return titles;
    }

    /// <summary>
    /// The <c>fields</c> of every record file in the type's directory, with the shortcode of the corpus that wrote it,
    /// or null for this corpus's own.
    /// </summary>
    private static IEnumerable<(string? Shortcode, JsonObject Fields)> RecordFields(Source source, ExportedType type)
    {
        var prefix = type.Dir + "/";

        foreach (var file in source.Export)
        {
            if (!file.Path.StartsWith(prefix, StringComparison.Ordinal)) continue;
            if (!file.Path.EndsWith(".json", StringComparison.Ordinal)) continue;

            var within = file.Path[prefix.Length..];
            var slash = within.IndexOf('/', StringComparison.Ordinal);

            var record = JsonRead.Parse(new UTF8Encoding(false).GetString(file.Content));
            if (JsonRead.Object(record?["fields"]) is { } fields)
                yield return (slash < 0 ? null : within[..slash], fields);
        }
    }

    private static string Scope(string id, string? shortcode) =>
        shortcode is null ? id : Exporter.Scoped(id, shortcode);

    private static string? Text(Source source, string path) =>
        source.Export.FirstOrDefault(f => f.Path == path) is { } file
            ? new UTF8Encoding(false).GetString(file.Content)
            : null;
}
