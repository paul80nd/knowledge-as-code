using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using kac.core;
using Xunit.Sdk;

namespace kac.tests;

/// <summary>The three trees a skill can live in, read from the files that already decide which is which.</summary>
internal static partial class SkillReference
{
    internal static readonly string Page = Path.Combine(Repo.Root, "docs", "skills.md");

    private static readonly string Own = Path.Combine(Repo.Root, ".claude", "skills");
    private static readonly string Bundled = Path.Combine(Repo.Root, "template", ".plugin");

    /// <summary>
    /// A skill's frontmatter description, up to the first key that follows it. YAML folds the value over several lines,
    /// so the block is taken whole and squeezed rather than read line by line.
    /// </summary>
    [GeneratedRegex(@"^description:\s*(?<body>.*?)(?=^\w|\z)", RegexOptions.Multiline | RegexOptions.Singleline)]
    private static partial Regex Description();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    private static readonly Lazy<IReadOnlyList<Skill>> Overlay = new(ReadOverlay);

    /// <summary>
    /// The overlay rule's patterns name a skill directory each, as <c>.claude/skills/&lt;name&gt;/**</c>.
    /// </summary>
    [GeneratedRegex(@"^\.claude/skills/(?<name>[^/]+)/\*\*$")]
    private static partial Regex Overlaid();

    internal record Skill(string Name, string Summary, string? Requires);

    /// <summary>
    /// The skills that travel inside a plugin, in the order <c>plugin.json</c> declares them. The order is the
    /// bundler's, so a component added to the manifest appears where its author put it.
    /// </summary>
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
                : component?["standalone"]?.GetValue<bool>() == true
                    ? "standalone"
                    : "supporting";

            skills.Add(new Skill(name, Summary(Path.Combine(Bundled, "skills", name, "SKILL.md")), reads));
        }

        return skills;
    }

    internal static IReadOnlyList<Skill> InACorpus() => Overlay.Value;

    private static IReadOnlyList<Skill> ReadOverlay()
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

    internal static IReadOnlyList<Skill> InThisRepository()
    {
        var travelling = InACorpus().Select(skill => skill.Name).ToHashSet(StringComparer.Ordinal);

        return
        [
            .. Directory.EnumerateDirectories(Own)
                .Select(directory => new DirectoryInfo(directory).Name)
                .Where(name => !travelling.Contains(name))
                .Order(StringComparer.Ordinal)
                .Select(name => new Skill(name, Summary(Path.Combine(Own, name, "SKILL.md")), null))
        ];
    }

    /// <summary>
    /// The first sentence of a skill's <c>description:</c>, which is what that skill answers. A trailing full stop is
    /// dropped because the cell is a phrase rather than a sentence.
    /// </summary>
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

    /// <summary>
    /// One table, rendered by the one renderer this repository has. <c>Gfm</c> states why the separator takes the form
    /// it does.
    /// </summary>
    internal static string Table(IReadOnlyList<Skill> skills)
    {
        var bundled = skills.Any(skill => skill.Requires is not null);

        List<string> headers = bundled ? ["Skill", "What it answers", "Requires"] : ["Skill", "What it answers"];
        var rows = skills
            .Select(skill => bundled
                ? new List<string> { Cell($"`{skill.Name}`"), Cell(skill.Summary), Cell(skill.Requires ?? "") }
                : [Cell($"`{skill.Name}`"), Cell(skill.Summary)])
            .ToList();

        return Gfm.RenderTable(headers, rows);
    }

    private static string Cell(string text) => Gfm.Escape(text);
}
