using System.Globalization;
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

    /// <summary>The count the page opens on, spelled as prose spells it.</summary>
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

    /// <summary>
    /// Tracked files rather than a folder walk, so a worktree under <c>.claude/</c> and an untracked scratch file are
    /// somebody else's business.
    /// </summary>
    [Fact]
    public void No_other_file_states_that_count()
    {
        var count = Schema.Load(Repo.Root).ByFolder.Count;
        var tracked = GitFiles.Tracked(Repo.Root);

        Assert.NotNull(tracked);

        var elsewhere = tracked
            .Where(f => f.EndsWith(".md", StringComparison.Ordinal)
                        || f.EndsWith(".yaml", StringComparison.Ordinal))
            .Where(f => f != "docs/framework/types.md")
            .SelectMany(f => Restatements(File.ReadAllText(Path.Combine(Repo.Root, f)), count)
                .Select(line => $"{f}:{line}"))
            .ToList();

        Assert.True(elsewhere.Count == 0,
            $"{count} is the type count, and docs/framework/types.md is where it is stated. "
            + "Cite that page rather than repeating the number:\n  " + string.Join("\n  ", elsewhere));
    }

    [Theory]
    [InlineData("Eighteen knowledge types ship with the framework.")]
    [InlineData("The framework ships eighteen types.")]
    [InlineData("All 18 types ship with the framework.")]
    [InlineData("All 18 knowledge types ship with the framework.")]
    [InlineData("It is an eighteen-type framework.")]
    [InlineData("The framework ships eighteen\nknowledge types.")]
    [InlineData("The framework ships eighteen knowledge\n  types.")]
    [InlineData("# The framework ships eighteen\n# knowledge types.")]
    [InlineData("> The framework ships eighteen\n> knowledge types.")]
    [InlineData("The framework ships **18** types.")]
    [InlineData("The framework ships `18` types.")]
    [InlineData("The framework ships eighteen default types.")]
    [InlineData("The framework ships eighteen built-in knowledge types.")]
    public void A_sentence_stating_the_type_count_is_a_restatement(string text)
        => Assert.NotEmpty(Restatements(text, 18));

    [Theory]
    [InlineData("Eighteen standards bind this corpus.")]
    [InlineData("Eighteen standards bind this corpus, and each cites the types it covers.")]
    [InlineData("The corpus has 118 types.")]
    [InlineData("Eighteen of the rules read knowledge types.")]
    [InlineData("The list stops at eighteen\n\nTypes come next.")]
    public void A_sentence_counting_something_else_is_not_a_restatement(string text)
        => Assert.Empty(Restatements(text, 18));

    [Fact]
    public void A_restatement_is_reported_at_its_line()
        => Assert.Equal([4], Restatements("# Types\n\nThere are\n18 types.\n", 18));

    /// <summary>
    /// The line of each place <paramref name="text"/> says <paramref name="count"/> types ship, in words or digits.
    /// The phrase may cross a hyphen or one line break, and the next line may open with <c>#</c> or <c>&gt;</c>.
    /// </summary>
    private static List<int> Restatements(string text, int count)
    {
        const string gap = @"(?:-|[ \t]+|[ \t]*\r?\n[ \t#>]*)";
        var number = $"(?:{Spelled(count)}|{count.ToString(CultureInfo.InvariantCulture)})";

        return Regex.Matches(text,
                $@"(?<![\w.-]){number}[*`]*{gap}(?:(?:default|built-in|knowledge){gap})*types?\b",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
            .Select(m => text.AsSpan(0, m.Index).Count('\n') + 1)
            .ToList();
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
