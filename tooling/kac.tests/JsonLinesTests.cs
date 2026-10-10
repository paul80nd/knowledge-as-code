using System.Text.Json;
using kac.core;

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public class JsonLinesTests
{
    [Fact]
    public void Every_line_of_a_jsonl_file_holds_one_complete_json_object()
    {
        var broken = new List<string>();

        foreach (var rel in Files())
        {
            var line = 0;
            foreach (var text in File.ReadLines(Path.Combine(Repo.Root, rel)))
            {
                line++;
                if (Fault(text) is not { } fault) continue;

                // The first fault in the file and no more. A pretty-printer breaks every line.
                broken.Add($"{rel}:{line} {fault}");
                break;
            }

            if (line == 0) broken.Add($"{rel} holds no lines.");
        }

        Assert.True(broken.Count == 0,
            "a .jsonl file is no longer one object per line:\n  " + string.Join("\n  ", broken));
    }

    /// <summary>
    /// What is wrong with the line, or null where nothing is. The kind is asked because a file can parse and still not
    /// be JSON Lines: the objects gathered into one array is valid JSON on a single line.
    /// </summary>
    private static string? Fault(string text)
    {
        try
        {
            using var doc = JsonDocument.Parse(text);
            return doc.RootElement.ValueKind == JsonValueKind.Object
                ? null
                : $"holds a {doc.RootElement.ValueKind} where an object belongs.";
        }
        catch (JsonException e)
        {
            return $"does not parse: {e.Message}";
        }
    }

    /// <summary>
    /// Every .jsonl git would list, so a fixture added and not yet staged counts. What a .gitignore covers does not:
    /// each corpus's own .dist/ and .imports/ stay out, and <c>kac export</c> rebuilds those on every run. The walk is
    /// the fallback for a tree with no git in it.
    /// </summary>
    private static IEnumerable<string> Files() =>
        (GitFiles.Tracked(Repo.Root) ?? GitFiles.Walk(Repo.Root, "*.jsonl", ".git"))
        .Where(rel => rel.EndsWith(".jsonl", StringComparison.Ordinal));
}
