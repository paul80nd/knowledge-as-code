using System.Text.RegularExpressions;
using kac.core;

// `docs/framework/types.md` introduces the types that ship with the framework, one line each, for somebody
// deciding whether to adopt it. The lines are hand-written: the schema's own `summary:` is the same altitude,
// and a page that generated them would be a second copy of a sentence the schema already owns.
//
// So what is held here are the names and the tier each sits under, and the wording is left alone. That is the
// stance `DocumentationTests` takes for `.schema/README.md`, and it catches the faults that matter: a type
// added, retired, renamed or moved between tiers, on a page nobody opens while doing any of those.

namespace kac.tests;

public partial class DefaultTypesTests
{
    private static readonly string Page =
        File.ReadAllText(Path.Combine(Repo.Root, "docs", "framework", "types.md"));

    // A tier's own H2, then every folder named in the table beneath it: `| **ADRs** | `adrs/` | …`.
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

    // Every (tier, folder) pair the page states, read off the heading each table sits under.
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
