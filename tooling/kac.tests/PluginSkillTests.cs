using kac.core;
using YamlDotNet.RepresentationModel;

// The frontmatter of every skill in the plugin tree, read the way the thing that installs it will read it.
//
// `claude plugin validate` is what CI asks, and it needs the Claude Code CLI. Nothing else in this repository opens a
// `SKILL.md` at all: `kac` copies the tree through as bytes, so a skill whose frontmatter does not parse bundles
// cleanly, installs, and loads with no metadata at all. That failure reached CI once, over a `: ` inside a plain
// scalar, which `technical-writing` warns about and no local run could see.

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

    // A skill loads by its `name` and is reached by its `description`. Both are lost together, and in silence, where
    // the block they sit in does not parse.
    [Theory]
    [MemberData(nameof(Each))]
    public void Its_frontmatter_parses_and_names_the_skill(string skill)
    {
        var frontmatter = Frontmatter(Path.Combine(Skills, skill, "SKILL.md"));

        Assert.Equal(skill, Yaml.Str(Yaml.Get(frontmatter, "name")));
        Assert.False(string.IsNullOrWhiteSpace(Yaml.Str(Yaml.Get(frontmatter, "description"))));
    }

    // The block between the first two `---` lines, parsed. A file with no block at all fails here rather than
    // returning an empty mapping that would pass every assertion above it.
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
