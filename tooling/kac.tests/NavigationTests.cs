// `mkdocs build --strict` mentions an unlisted page at INFO and exits 0, and says nothing at all about
// order. Both faults are silent there, so `mkdocs.yml` is read against `docs/` here instead.

using System.Text.RegularExpressions;

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public partial class NavigationTests
{
    private static readonly string[] Nav = File.ReadAllLines(Path.Combine(Repo.Root, "mkdocs.yml"));

    /// <summary>
    /// A nav entry names its page last, whether or not it carries a title: <c>- cli/index.md</c>, <c>- validate:
    /// cli/x.md</c>.
    /// </summary>
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

    [Fact]
    public void Every_redirect_lands_on_a_page_in_the_nav()
    {
        var listed = Listed().ToHashSet(StringComparer.Ordinal);

        Assert.All(Redirects(), redirect => Assert.Contains(redirect.To, listed));
    }

    /// <summary>
    /// A line inside <c>redirect_maps:</c> that is not a redirect fails here, so a malformed one is never skipped.
    /// </summary>
    private static List<(string From, string To)> Redirects()
    {
        var redirects = new List<(string From, string To)>();
        var indent = -1;

        foreach (var line in Nav)
        {
            if (indent < 0)
            {
                if (line.TrimStart().StartsWith("redirect_maps:", StringComparison.Ordinal))
                    indent = line.Length - line.TrimStart().Length;
                continue;
            }

            if (line.Trim().Length == 0 || line.TrimStart().StartsWith('#')) continue;
            if (line.Length - line.TrimStart().Length <= indent) break;

            var redirect = Redirect().Match(line);
            Assert.True(redirect.Success, $"Not a redirect: {line.Trim()}");
            redirects.Add((redirect.Groups["from"].Value, redirect.Groups["to"].Value));
        }

        Assert.True(indent >= 0, "mkdocs.yml declares no redirect_maps:");
        return redirects;
    }

    /// <summary>
    /// One redirect: <c>'design/x.md': 'reference/x.md'</c>. An anchor on the target is dropped, since the nav
    /// lists pages.
    /// </summary>
    [GeneratedRegex(@"^\s+'(?<from>[\w./-]+\.md)':\s*'(?<to>[\w./-]+\.md)(#[\w-]+)?'\s*$")]
    private static partial Regex Redirect();

    /// <summary>The block runs to the next top-level key, and <c>nav:</c> is the last thing in the file.</summary>
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
