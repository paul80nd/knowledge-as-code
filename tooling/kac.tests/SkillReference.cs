using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using kac.core;
using Xunit.Sdk;

namespace kac.tests;

// The three trees a skill can live in, read from the files that already decide which is which.
//
// A skill travels where it governs a surface a corpus holds. `plugin.json` states the bundled set, `manifest.yaml`
// states the overlaid one, and what is left in `.claude/skills/` never leaves. Nothing else states the split, so a
// page restating it by hand would be a fourth answer that can disagree with the other three.
//
// The summary is the first sentence of each skill's own `description:`. A `note` in `plugin.json` says what a
// component reads, which is the maintainer's question rather than the reader's, and only ten of the sixteen have one.
internal static partial class SkillReference
{
    internal static readonly string Page = Path.Combine(Repo.Root, "docs", "skills.md");

    private static readonly string Own = Path.Combine(Repo.Root, ".claude", "skills");
    private static readonly string Bundled = Path.Combine(Repo.Root, "template", ".plugin");

    // A skill's frontmatter description, up to the first key that follows it. YAML folds the value over several
    // lines, so the block is taken whole and squeezed rather than read line by line.
    [GeneratedRegex(@"^description:\s*(?<body>.*?)(?=^\w|\z)", RegexOptions.Multiline | RegexOptions.Singleline)]
    private static partial Regex Description();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    // The overlay rule's patterns name a skill directory each, as `.claude/skills/<name>/**`.
    [GeneratedRegex(@"^\.claude/skills/(?<name>[^/]+)/\*\*$")]
    private static partial Regex Overlaid();

    // One row of a table: the skill, what it answers, and for a bundled one what its component requires.
    internal record Skill(string Name, string Summary, string? Requires);

    // The ten that travel inside a plugin, in the order `plugin.json` declares them. The order is the bundler's, so
    // a component added to the manifest appears where its author put it.
    internal static IReadOnlyList<Skill> InThePlugin()
    {
        var manifest = JsonRead.Parse(Files.ReadLf(Path.Combine(Bundled, ".claude-plugin", "plugin.json")));
        var components = manifest?["metadata"]?["components"] as JsonArray
                         ?? throw new XunitException("kac.tests: the plugin manifest declares no metadata.components.");

        var skills = new List<Skill>();
        foreach (var component in components)
        {
            var path = component?["path"]?.GetValue<string>() ?? "";
            if (!path.StartsWith("skills/", StringComparison.Ordinal)) continue;

            var name = path["skills/".Length..];
            var requires = (component?["requires"] as JsonArray ?? [])
                .Select(r => $"`{r?.GetValue<string>()}`")
                .ToList();

            // An empty `requires` reads no export, and `standalone` says which of two reasons that is. The words
            // are `docs/design/plugin.md`'s, so a reader meets one vocabulary across both pages.
            var reads = requires.Count > 0
                ? string.Join(", ", requires)
                : component?["standalone"]?.GetValue<bool>() == true ? "standalone" : "supporting";

            skills.Add(new Skill(name, Summary(Path.Combine(Bundled, "skills", name, "SKILL.md")), reads));
        }

        return skills;
    }

    // The ones a corpus receives into its own working tree, named by the overlay rule that sends them.
    internal static IReadOnlyList<Skill> InACorpus()
    {
        var manifest = Manifest.LoadFrom(Path.Combine(Repo.Root, "manifest.yaml"));
        var names = manifest.Rules
            .Where(rule => rule.Layer == "overlay")
            .SelectMany(rule => rule.Patterns)
            .Select(pattern => Overlaid().Match(pattern))
            .Where(match => match.Success)
            .Select(match => match.Groups["name"].Value)
            .ToList();

        return [.. names.Select(name => new Skill(name, Summary(Path.Combine(Own, name, "SKILL.md")), null))];
    }

    // What is left in this repository's own tree once the overlay has taken its share. Derived rather than listed,
    // so a skill added here appears without this file changing.
    internal static IReadOnlyList<Skill> InThisRepository()
    {
        var travelling = InACorpus().Select(skill => skill.Name).ToHashSet(StringComparer.Ordinal);

        // `EnumerateDirectories` returns a path with no trailing separator, so every entry has a last segment.
        return
        [
            .. Directory.EnumerateDirectories(Own)
                .Select(directory => new DirectoryInfo(directory).Name)
                .Where(name => !travelling.Contains(name))
                .Order(StringComparer.Ordinal)
                .Select(name => new Skill(name, Summary(Path.Combine(Own, name, "SKILL.md")), null))
        ];
    }

    // The first sentence of a skill's `description:`, which is what that skill answers. A trailing full stop is
    // dropped because the cell is a phrase rather than a sentence.
    private static string Summary(string skillFile)
    {
        var frontmatter = Files.ReadLf(skillFile).Split("---");
        if (frontmatter.Length < 2)
            throw new XunitException($"kac.tests: {skillFile} opens on no frontmatter.");

        var match = Description().Match(frontmatter[1]);
        if (!match.Success)
            throw new XunitException($"kac.tests: {skillFile} states no description.");

        var described = Whitespace().Replace(match.Groups["body"].Value, " ").Trim();
        var stop = described.IndexOf(". ", StringComparison.Ordinal);

        return (stop < 0 ? described : described[..stop]).TrimEnd('.');
    }

    // One table, rendered. A bundled table carries the third column and the other two do not.
    internal static string Table(IReadOnlyList<Skill> skills)
    {
        var rows = skills.Select(skill => skill.Requires is null
            ? new[] { $"`{skill.Name}`", skill.Summary }
            : [$"`{skill.Name}`", skill.Summary, skill.Requires]).ToList();

        var headers = skills.Any(skill => skill.Requires is not null)
            ? ["Skill", "What it answers", "Requires"]
            : new[] { "Skill", "What it answers" };

        var widths = headers
            .Select((header, i) => rows.Select(row => row[i].Length).Append(header.Length).Max())
            .ToArray();

        var table = new StringBuilder();
        table.Append(Row(headers, widths));
        table.Append('|').AppendJoin('|', widths.Select(w => new string('-', w + 2))).Append("|\n");
        foreach (var row in rows) table.Append(Row(row, widths));

        return table.ToString().TrimEnd('\n');
    }

    private static string Row(IReadOnlyList<string> cells, IReadOnlyList<int> widths)
    {
        var line = new StringBuilder("|");
        for (var i = 0; i < cells.Count; i++) line.Append(' ').Append(cells[i].PadRight(widths[i])).Append(" |");

        return line.Append('\n').ToString();
    }
}
