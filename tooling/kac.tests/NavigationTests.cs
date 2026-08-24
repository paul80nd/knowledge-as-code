// `mkdocs.yml` states the site's order by hand, and each page now links to the one before and after it. Two things go
// quietly wrong there. A page written and never listed is published and reachable from nothing. A command added to the
// parser leaves the reference in an order that no longer matches the tool.
//
// `mkdocs build --strict` catches neither. It mentions an unlisted page at INFO and exits 0, and it says nothing at
// all about order.

using System.Text.RegularExpressions;

namespace kac.tests;

public partial class NavigationTests
{
    private static readonly string[] Nav = File.ReadAllLines(Path.Combine(Repo.Root, "mkdocs.yml"));

    // A nav entry names its page last, whether or not it carries a title: `- cli/index.md`, `- validate: cli/x.md`.
    [GeneratedRegex(@"([\w./-]+\.md)\s*$")]
    private static partial Regex Entry();

    [Fact]
    public void Every_page_in_docs_is_listed_in_the_nav()
    {
        var docs = Path.Combine(Repo.Root, "docs");
        var written = Directory.EnumerateFiles(docs, "*.md", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(docs, f).Replace('\\', '/'))
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal(written.Order(StringComparer.Ordinal), Listed().Order(StringComparer.Ordinal));
    }

    [Fact]
    public void The_cli_reference_reads_in_the_order_the_parser_declares()
    {
        var wanted = new[] { "cli/index.md" }
            .Concat(CliReference.Pages().Select(page => $"cli/{page}.md"))
            .ToList();

        Assert.Equal(wanted, [.. Listed().Where(page => page.StartsWith("cli/", StringComparison.Ordinal))]);
    }

    // Every page the nav names, in the order it names them. The block runs to the next top-level key, of which there
    // is none today: `nav:` is the last thing in the file.
    private static List<string> Listed()
    {
        var listed = new List<string>();
        var inNav = false;

        foreach (var line in Nav)
        {
            if (line.StartsWith("nav:", StringComparison.Ordinal))
            {
                inNav = true;
                continue;
            }

            if (!inNav) continue;
            if (line.Length > 0 && !char.IsWhiteSpace(line[0]) && line[0] != '#') break;

            if (Entry().Match(line) is { Success: true } entry) listed.Add(entry.Groups[1].Value);
        }

        return listed;
    }
}
