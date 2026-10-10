using System.Text.RegularExpressions;
using kac.core;

// `docs/framework/types.md` and `docs/framework/metadata.md` name every type by hand. This holds the
// names and the tier each sits under, and leaves the wording alone.

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public partial class DefaultTypesTests
{
    private static readonly string Page =
        File.ReadAllText(Path.Combine(Repo.Root, "docs", "framework", "types.md"));

    private static readonly string MetadataPage =
        File.ReadAllText(Path.Combine(Repo.Root, "docs", "framework", "metadata.md"));

    [GeneratedRegex(@"^## (?<tier>\w+)$", RegexOptions.Multiline)]
    private static partial Regex TierHeading();

    [GeneratedRegex(@"^\|\s*\*\*[^*]+\*\*\s*\|\s*`(?<folder>[^`/]+)/`", RegexOptions.Multiline)]
    private static partial Regex Row();

    [Fact]
    public void Every_type_the_schema_declares_is_introduced_under_its_own_tier()
        => Assert.Equal(
            Schema.Load(Repo.Root).ByFolder.Values
                .Select(t => (t.Tier, t.Folder))
                .OrderBy(x => x, Comparer<(string, string)>.Default),
            Listed().OrderBy(x => x, Comparer<(string, string)>.Default));

    [Fact]
    public void Every_type_the_schema_declares_is_listed_against_what_a_source_is()
    {
        var section = MetadataPage[MetadataPage.IndexOf("## Filling `sources`", StringComparison.Ordinal)..];
        var table = section[..section.IndexOf("\n## ", StringComparison.Ordinal)];

        var missing = Schema.Load(Repo.Root).ByFolder.Values
            .Select(t => t.Label)
            .Where(label => !Regex.IsMatch(table, $@"[|,] {Regex.Escape(label)}[,|\s]"))
            .Order(StringComparer.Ordinal)
            .ToList();

        Assert.True(missing.Count == 0,
            "docs/framework/metadata.md says what a source is for each type, and these are not in that table:\n  "
            + string.Join("\n  ", missing));
    }

    // The count the page opens on, spelled as prose spells it.
    private static readonly string[] Numbers =
    [
        "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten",
        "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen",
        "nineteen", "twenty",
    ];

    [Fact]
    public void The_page_opens_on_how_many_types_ship()
    {
        var word = Spelled(Schema.Load(Repo.Root).ByFolder.Count);

        Assert.Contains($"{char.ToUpperInvariant(word[0])}{word[1..]} knowledge types ship with the framework.",
            Page, StringComparison.Ordinal);
    }

    // Tracked files rather than a folder walk, so a worktree under `.claude/` and an untracked scratch
    // file are somebody else's business.
    [Fact]
    public void No_other_file_states_that_count()
    {
        var word = Spelled(Schema.Load(Repo.Root).ByFolder.Count);
        var tracked = GitFiles.Tracked(Repo.Root);

        Assert.NotNull(tracked);

        var elsewhere = tracked
            .Where(f => f.EndsWith(".md", StringComparison.Ordinal)
                        || f.EndsWith(".yaml", StringComparison.Ordinal))
            .Where(f => f != "docs/framework/types.md")
            .Where(f => File.ReadAllText(Path.Combine(Repo.Root, f))
                .Contains(word, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(elsewhere.Count == 0,
            $"'{word}' is the type count, and docs/framework/types.md is where it is stated. "
            + "Cite that page rather than repeating the number:\n  " + string.Join("\n  ", elsewhere));
    }

    private static string Spelled(int count) => count < Numbers.Length
        ? Numbers[count]
        : throw new InvalidOperationException($"kac.tests: no word for {count} types. Extend Numbers.");

    private static List<(string Tier, string Folder)> Listed()
    {
        var headings = TierHeading().Matches(Page);
        var listed = new List<(string, string)>();

        for (var i = 0; i < headings.Count; i++)
        {
            var from = headings[i].Index + headings[i].Length;
            var to = i + 1 < headings.Count ? headings[i + 1].Index : Page.Length;
            var tier = headings[i].Groups["tier"].Value.ToLowerInvariant();

            listed.AddRange(Row().Matches(Page[from..to]).Select(r => (tier, r.Groups["folder"].Value)));
        }

        return listed;
    }
}
