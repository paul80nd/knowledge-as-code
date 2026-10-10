using kac.core;
using YamlDotNet.RepresentationModel;

// Nothing else here opens a `SKILL.md`: `kac` copies it as bytes, so a skill whose frontmatter does not parse bundles,
// installs, and loads with no metadata. `claude plugin validate` catches it in CI only, as it needs the Claude Code
// CLI; a `: ` inside a plain scalar is the usual cause.

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public class PluginSkillTests
{
    private static readonly string Skills =
        Path.Combine(Repo.Root, "template", ".plugin", "skills");

    public static TheoryData<string> Each()
    {
        var data = new TheoryData<string>();
        foreach (var dir in Directory.EnumerateDirectories(Skills).Order(StringComparer.Ordinal))
            data.Add(Path.GetFileName(dir));

        return data;
    }

    [Theory]
    [MemberData(nameof(Each))]
    public void Its_frontmatter_parses_and_names_the_skill(string skill)
    {
        var frontmatter = Frontmatter(Path.Combine(Skills, skill, "SKILL.md"));

        Assert.Equal(skill, Yaml.Str(Yaml.Get(frontmatter, "name")));
        Assert.False(string.IsNullOrWhiteSpace(Yaml.Str(Yaml.Get(frontmatter, "description"))));
    }

    /// <summary>
    /// A file with no block at all fails here, rather than returning an empty mapping that would pass every assertion
    /// above it.
    /// </summary>
    private static YamlNode Frontmatter(string path)
    {
        var lines = Files.ReadLf(path).Split('\n');
        Assert.True(lines.Length > 0 && lines[0] == "---", $"{path} opens with no frontmatter block.");

        var close = Array.IndexOf(lines, "---", 1);
        Assert.True(close > 0, $"{path} opens a frontmatter block and never closes it.");

        var stream = new YamlStream();
        stream.Load(new StringReader(string.Join('\n', lines[1..close])));
        return stream.Documents[0].RootNode;
    }
}
