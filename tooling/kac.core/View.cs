using System.Text;
using System.Text.Json.Nodes;

namespace kac.core;

/// <summary>
/// A file <c>bundle</c> generates into the plugin from the export, shaped for one moment of the work. A fresh bundle
/// writes the same bytes from the same export. <c>docs/maintaining/plugin.md</c> says what each view contains.
/// </summary>
public static class View
{
    /// <summary>Each standard's id and status, and its rules by title, for a session about to write code.</summary>
    public const string BuildIndex = "build-index";

    /// <summary>Each standard's status and controls, and its rules' obligations, for a reviewer.</summary>
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
    /// The status <c>.schema/standards.yaml</c> declares as <c>in-force</c>. The export does not state it, so it is
    /// fixed here for the <c>standards@2</c> every view component pins.
    /// </summary>
    private const string InForce = "active";

    /// <summary>
    /// One rule as a standards part line states it at shape 2. A component declaring the view pins that shape, so
    /// these keys are the shape's own.
    /// </summary>
    private sealed record Rule(
        string Id, string Record, string Title, string Status, IReadOnlyList<string> Obligations);

    private sealed record Control(
        string Id, string Title, string Status, string Mechanism, IReadOnlyList<string> Verifies);

    /// <summary>
    /// The hook pushes this text as it stands, so it states facts and gives no instruction. A rule's id is its
    /// standard's id and the slug of its title, so a line gives the title alone.
    /// </summary>
    private static string RenderBuildIndex(Source source)
    {
        var standards = StandardsOf(source);
        var rules = standards.Sum(s => s.Rules.Count);

        var under = standards.Count == 1 ? "the standard" : $"the {standards.Count} standards";

        var text = new StringBuilder();
        text.Append($"# Rules the standards of {Corpus(source)} state\n\n");
        text.Append($"{Stating(source, rules)}, under {under} below. ");
        text.Append(Binds).Append(' ').Append(ReadFullText(source)).Append("\n\n");
        text.Append("Each heading gives a standard's id, title and status. Each line under it gives a rule's title.\n");

        foreach (var standard in standards)
        {
            text.Append($"\n## {standard.Heading} ({standard.Status})\n\n");

            foreach (var rule in standard.Rules)
                text.Append($"- {rule.Title}\n");
        }

        return text.ToString();
    }

    /// <summary>
    /// What the build-index hook pushes where the index is too long to push: the counts, the view's path inside the
    /// plugin, and the way to read a rule's obligations.
    /// </summary>
    public static string BuildIndexPointer(Source source)
    {
        var standards = StandardsOf(source);
        var rules = standards.Sum(s => s.Rules.Count);
        var path = source.Included.FirstOrDefault(c => c.View == BuildIndex)?.Path ?? "views/build-index.md";

        return $"{Stating(source, rules)}, under {Count(standards.Count, "standard")}. {Binds} `{path}` inside this "
               + $"plugin lists every rule by title, under its standard. {ReadFullText(source)}\n";
    }

    /// <summary>The sentence carrying the leading word, binds, which the index and its pointer share.</summary>
    private const string Binds = "Each rule binds the code its title bears on.";

    private static string Stating(Source source, int rules) =>
        $"{Corpus(source)}{(source.Manifest.ContentVersion is { } version ? $" {version}" : "")} states "
        + $"{Count(rules, "rule")} for code in this repository";

    private static string Count(int n, string noun) => $"{n} {noun}{(n == 1 ? "" : "s")}";

    /// <summary>Opens on what each keyword, status and control means, stated as facts and with no instruction.</summary>
    private static string RenderReview(Source source)
    {
        var controls = ControlsOf(source);

        var text = new StringBuilder();
        text.Append($"# The rules a change is reviewed against, in the standards of {Corpus(source)}\n\n");
        text.Append("Each rule below gives the id a finding cites, then its obligations, one to a bullet.\n\n");
        text.Append("- **MUST** and **MUST NOT** state a requirement. A change that breaks one is a violation.\n");
        text.Append("- **SHOULD** and **SHOULD NOT** state the usual way. A change that departs from one and gives its "
                    + "reason conforms, and the departure is a judgement call.\n");
        text.Append("- **MAY** permits, and a change cannot break it.\n");
        text.Append($"- A standard is in force at status `{InForce}`. A finding under any other status is not in "
                    + "force.\n");
        text.Append(controls is null
            ? "- This corpus exports no controls, so no rule here has a check recorded against it.\n"
            : "- A standard's controls are listed with each one's mechanism and status. A control checks a rule only "
              + "where its status is `active` and its mechanism is `ci` or `runtime-alert`, and then only the "
              + "obligations named in its record's What it checks section. "
              + $"`{source.CorpusRoot}/{Type(source, Controls)?.Dir}/` inside this plugin has each control's record. "
              + "A person checks every other obligation in the review.\n");

        foreach (var standard in StandardsOf(source))
        {
            text.Append($"\n## {standard.Heading}\n\nStatus: {standard.Status}.\n");

            if (controls is not null)
            {
                var checking = controls
                    .Where(c => c.Verifies.Any(v => v == standard.Id || standard.Rules.Any(r => r.Id == v)))
                    .Select(c => $"`{c.Id}` {c.Title} (`{c.Mechanism}`, {c.Status})")
                    .ToList();

                text.Append(checking.Count == 0
                    ? "\nControls: none.\n"
                    : $"\nControls: {string.Join("; ", checking)}.\n");
            }

            foreach (var rule in standard.Rules)
            {
                text.Append($"\n### {rule.Title}\n\nCite as `{rule.Id}`.\n");
                text.Append(rule.Obligations.Count > 0 ? "\n" : "");

                foreach (var obligation in rule.Obligations)
                    text.Append("- ").Append(obligation.Replace("\n", "\n  ", StringComparison.Ordinal)).Append('\n');
            }
        }

        return text.ToString();
    }

    /// <summary>
    /// One standard with the rules it states. <c>Heading</c> is its id, then its title where the export has one.
    /// </summary>
    private sealed record Standard(string Id, string Heading, string Status, IReadOnlyList<Rule> Rules);

    /// <summary>
    /// In the order the parts file states each standard's first rule. Every rule line of one record states the
    /// record's status, so the first rule's status is the standard's.
    /// </summary>
    private static List<Standard> StandardsOf(Source source)
    {
        var titles = RecordTitles(source);

        return
        [
            .. Rules(source)
                .GroupBy(r => r.Record, StringComparer.Ordinal)
                .Select(g => new Standard(g.Key,
                    titles.TryGetValue(g.Key, out var title) ? $"{g.Key}: {title}" : g.Key,
                    g.First().Status,
                    [.. g]))
        ];
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
            return $"The {skill.Path["skills/".Length..]} skill returns a rule's obligations by its standard's id.";

        var file = Type(source, Standards)?.PartsFile;
        return $"`{source.CorpusRoot}/{file}` inside this plugin lists a rule's obligations, under its standard's id.";
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
