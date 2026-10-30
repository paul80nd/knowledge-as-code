using System.Text.RegularExpressions;
using kac.core;

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public partial class DocumentationTests
{
    private static string Page(string name) =>
        File.ReadAllText(Path.Combine(Repo.Root, "docs", "reference", name));

    /// <summary>
    /// A row of the fact table opens with the call it documents: <c>| \</c>section_count('Title')\<c> | int |</c>.
    /// </summary>
    [GeneratedRegex(@"^\| `([a-z_]+)\(", RegexOptions.Multiline)]
    private static partial Regex FactRow();

    /// <summary>A row of the held-to table closes with the check it reports under.</summary>
    [GeneratedRegex(@"\| `(schema-[a-z-]+)`\s*\|\s*$", RegexOptions.Multiline)]
    private static partial Regex HeldToRow();

    /// <summary>
    /// The line above a transcript: <c>&lt;!-- transcript: examples/library, kac validate --&gt;</c>. The source is a
    /// folder of this repository, or <c>illustration</c> for output nobody can reproduce from one.
    /// </summary>
    [GeneratedRegex(@"^\s*<!-- transcript: (?<source>[^,]+), (?<command>.+) -->$")]
    private static partial Regex TranscriptMarker();

    [GeneratedRegex(@"^new: wrote (\d+) files? for ")]
    private static partial Regex NewWrote();

    [GeneratedRegex(@"^new: did not write (.+): this corpus is built by ")]
    private static partial Regex NewWithheld();

    private const string Illustration = "illustration";

    /// <summary>A <c>text</c> fence, and its marker, which is null where the nearest line above is not one.</summary>
    private sealed record Transcript(string At, Match? Marker, IReadOnlyList<string> Body)
    {
        public string Source => Marker?.Groups["source"].Value ?? "";
        public string Command => Marker?.Groups["command"].Value ?? "";
    }

    /// <summary>
    /// Every <c>text</c> fence on a page of the site and in the root <c>README.md</c>. A fence inside a generated block
    /// is the usage block, which <c>CliReferenceTests</c> holds to the parser, so it is left out.
    /// </summary>
    private static IEnumerable<Transcript> Transcripts()
    {
        var pages = Directory.EnumerateFiles(Path.Combine(Repo.Root, "docs"), "*.md", SearchOption.AllDirectories)
            .Append(Path.Combine(Repo.Root, "README.md"))
            .Order(StringComparer.Ordinal);

        foreach (var page in pages)
        {
            var lines = File.ReadAllLines(page);
            var rel = Path.GetRelativePath(Repo.Root, page).Replace('\\', '/');
            var generated = false;
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("<!-- BEGIN GENERATED", StringComparison.Ordinal)) generated = true;
                if (lines[i].StartsWith("<!-- END GENERATED", StringComparison.Ordinal)) generated = false;
                if (generated || lines[i].Trim() != "```text") continue;

                var above = i - 1;
                while (above >= 0 && lines[above].Trim().Length == 0) above--;
                var marker = above >= 0 ? TranscriptMarker().Match(lines[above]) : null;

                var end = i + 1;
                while (end < lines.Length && lines[end].Trim() != "```") end++;

                yield return new Transcript($"{rel}:{i + 1}", marker is { Success: true } ? marker : null,
                    [.. lines[(i + 1)..end].Select(l => l.Trim())]);
                i = end;
            }
        }
    }

    [Fact]
    public void The_fact_table_documents_every_callable_fact()
    {
        var documented = FactRow().Matches(Page("expressions.md"))
            .Select(m => m.Groups[1].Value)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal(
            RuleExpr.FunctionNames.Order(StringComparer.Ordinal),
            documented.Order(StringComparer.Ordinal));
    }

    /// <summary>
    /// A check renamed, retired or introduced is caught. An id growing a second way to fail is not, because nothing in
    /// the code tells one arm from another. The <c>schema-declarations</c> fixture pins those.
    /// </summary>
    [Fact]
    public void The_held_to_table_names_every_check_the_schema_pass_reports()
    {
        var declared = Schema.Load(Repo.Root).Checks
            .Select(c => c.Id.Value)
            .Where(id => id.StartsWith("schema-", StringComparison.Ordinal))
            .ToHashSet(StringComparer.Ordinal);

        var cited = HeldToRow().Matches(Page("held-to.md"))
            .Select(m => m.Groups[1].Value)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal(declared.Order(StringComparer.Ordinal), cited.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Every_transcript_names_the_folder_it_was_run_in_or_says_it_is_an_illustration()
    {
        var unsourced = Transcripts()
            .Where(t => t.Marker is null
                        || (t.Source != Illustration && !Directory.Exists(Path.Combine(Repo.Root, t.Source))))
            .Select(t => t.Marker is null ? $"{t.At}: no transcript marker" : $"{t.At}: no folder '{t.Source}'")
            .ToList();

        Assert.True(unsourced.Count == 0,
            "write '<!-- transcript: <folder>, <command> -->' above each, or 'illustration' for the folder:\n"
            + string.Join('\n', unsourced));
    }

    /// <summary>
    /// The fixture's tree is a <c>--ci github</c> run. A transcript of another run names what it withheld, so those
    /// files come off the count.
    /// </summary>
    [Fact]
    public void A_transcript_of_new_counts_the_files_its_fixture_holds()
    {
        var checkedTranscripts = 0;
        var wrong = new List<string>();
        foreach (var t in Transcripts().Where(t => t.Source != Illustration
                                                   && t.Command.StartsWith("kac new", StringComparison.Ordinal)))
        {
            var wrote = t.Body.Select(l => NewWrote().Match(l)).FirstOrDefault(m => m.Success);
            if (wrote is null) continue;

            var treePath = Path.Combine(Repo.Root, t.Source, "expected-tree.txt");
            if (!File.Exists(treePath))
            {
                wrong.Add($"{t.At}: '{t.Source}' has no expected-tree.txt to count");
                continue;
            }

            var tree = File.ReadAllLines(treePath).Where(l => l.Length > 0).ToHashSet(StringComparer.Ordinal);
            var withheld = t.Body.Select(l => NewWithheld().Match(l)).FirstOrDefault(m => m.Success)
                ?.Groups[1].Value.Split(", ") ?? [];
            var expected = tree.Count - withheld.Count(tree.Contains);

            checkedTranscripts++;
            if (int.Parse(wrote.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) != expected)
                wrong.Add($"{t.At}: prints {wrote.Groups[1].Value} files, and {t.Source} holds {expected}");
        }

        Assert.True(checkedTranscripts > 0, "no transcript of 'kac new' prints a count, so this test checks nothing.");
        Assert.True(wrong.Count == 0, string.Join('\n', wrong));
    }
}
