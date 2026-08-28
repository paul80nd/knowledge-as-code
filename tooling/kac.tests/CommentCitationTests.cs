// Citing a page rather than restating it holds one account of an argument, and leaves a path that rots
// when the page moves. Nothing reads a comment, so the paths are resolved here instead.

using System.Text.RegularExpressions;

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public partial class CommentCitationTests
{
    // A markdown path inside a `//` comment. The anchor is dropped: a fragment addresses within the page, and which
    // headings that page carries is not this test's question.
    [GeneratedRegex(@"(?<path>[A-Za-z0-9_./-]+\.md)(?:#[\w-]+)?")]
    private static partial Regex Citation();

    [GeneratedRegex(@"^\s*//")]
    private static partial Regex CommentLine();

    // The folders a citation is written from. A comment also writes filenames a reader is meant to recognise rather
    // than open, `_template.md` and `pol-DEVI.md` among them, and those name a shape rather than a page. Rooting the
    // test here is what tells the two apart without a list of every example the tool has ever printed.
    private static readonly string[] Roots =
        ["docs/", "tooling/", "template/", "examples/", ".schema/", ".github/", ".azuredevops/"];

    [Fact]
    public void Every_page_a_comment_cites_is_a_page_the_repository_holds()
    {
        var missing = new List<string>();

        foreach (var file in Sources())
        {
            var line = 0;
            foreach (var text in File.ReadLines(file))
            {
                line++;
                if (!CommentLine().IsMatch(text)) continue;

                foreach (Match m in Citation().Matches(text))
                {
                    var cited = m.Groups["path"].Value;
                    if (!IsCitation(cited) || Resolves(file, cited)) continue;

                    missing.Add($"{Path.GetRelativePath(Repo.Root, file).Replace('\\', '/')}:{line} cites {cited}");
                }
            }
        }

        Assert.True(missing.Count == 0,
            "a comment cites a page nothing answers to:\n  " + string.Join("\n  ", missing));
    }

    private static IEnumerable<string> Sources() =>
        Directory.EnumerateFiles(Path.Combine(Repo.Root, "tooling"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                        && !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"));

    // Whether the path is one a reader could follow: written from a folder of this repository, or up out of the file's
    // own. `CLAUDE.md` is named from both directions and is the guidance page every folder carries.
    private static bool IsCitation(string cited) =>
        Roots.Any(r => cited.StartsWith(r, StringComparison.Ordinal))
        || cited.StartsWith("../", StringComparison.Ordinal)
        || cited.EndsWith("CLAUDE.md", StringComparison.Ordinal);

    // Two readings, because comments write both. A path from the repository root is what a `docs/` citation means, and
    // a path from the file's own folder is what `../../CLAUDE.md` means. Either resolving is enough.
    private static bool Resolves(string file, string cited) =>
        File.Exists(Path.Combine(Repo.Root, cited))
        || File.Exists(Path.Combine(Path.GetDirectoryName(file)!, cited));
}
