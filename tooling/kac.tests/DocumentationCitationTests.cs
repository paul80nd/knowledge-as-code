using System.Text.RegularExpressions;
using kac.core;

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public partial class DocumentationCitationTests
{
    /// <summary>
    /// A code span holding <c>Type.Member</c>, or a <c>File.ext</c>. Both open upper case, which is what tells them
    /// from <c>.corpus.yaml</c>, <c>raw.githubusercontent.com</c> and every other dotted thing a page writes in a span.
    /// </summary>
    [GeneratedRegex(@"`(?<type>[A-Z][A-Za-z0-9]*)\.(?<member>[A-Za-z][A-Za-z0-9]*)`")]
    private static partial Regex Symbol();

    /// <summary>Partials all declare the name, so a member is looked for in every file that does.</summary>
    [GeneratedRegex(@"\b(?:class|record|struct|interface|enum)\s+(?<name>[A-Za-z][A-Za-z0-9]*)")]
    private static partial Regex Declaration();

    /// <summary>The space is what tells <c>kac export</c> from <c>kac.core</c>.</summary>
    [GeneratedRegex(@"\bkac (?<verb>[a-z][a-z-]*)")]
    private static partial Regex Invocation();

    [GeneratedRegex(@"\]\((?<target>[^)\s]+)\)")]
    private static partial Regex Link();

    [GeneratedRegex(@"```.*?```|`[^`\n]+`", RegexOptions.Singleline)]
    private static partial Regex Code();

    /// <summary>
    /// <c>Schema.feature</c> and <c>Schema.cs</c> would otherwise both read as a member of the <c>Schema</c> class.
    /// </summary>
    private static readonly string[] Extensions =
        ["cs", "md", "feature", "yaml", "yml", "json", "jsonl", "csproj", "slnx", "txt", "css", "svg", "png"];

    /// <summary>
    /// <c>cli xmldoc</c> is hidden from <c>--help</c> and prints the command model the usage blocks are generated from.
    /// <c>tooling/README.md</c> is where it is explained.
    /// </summary>
    private static readonly string[] Hidden = ["cli"];

    [Fact]
    public void Every_symbol_a_page_cites_is_one_the_tool_declares()
    {
        var declarations = Declarations();
        var sources = Sources().ToList();
        var unresolved = new List<string>();

        foreach (var (page, line, text) in Lines())
        {
            foreach (Match m in Symbol().Matches(text))
            {
                var type = m.Groups["type"].Value;
                var member = m.Groups["member"].Value;

                // A page or a Gherkin spec is left to `CommentCitationTests` and to `mkdocs build --strict`.
                if (Extensions.Contains(member))
                {
                    if (member is not "cs" || sources.Any(f => Path.GetFileName(f) == $"{type}.cs")) continue;
                    unresolved.Add($"{page}:{line} cites {type}.cs, which is no file under tooling/");
                    continue;
                }

                // A name this repository never declares belongs to a package or a namespace, as `System.CommandLine`
                // does. So deleting a whole type passes, and only deleting a member of one that survives is caught.
                if (!declarations.TryGetValue(type, out var files)) continue;

                if (files.Any(f => Regex.IsMatch(File.ReadAllText(f), $@"\b{Regex.Escape(member)}\b"))) continue;

                unresolved.Add($"{page}:{line} cites {type}.{member}, and {type} has no {member}");
            }
        }

        Assert.True(unresolved.Count == 0,
            "a page cites a symbol the tool does not declare:\n  " + string.Join("\n  ", unresolved));
    }

    [Fact]
    public void Every_file_a_page_links_is_a_file_the_repository_holds()
    {
        var dead = new List<string>();

        foreach (var (page, line, text) in Lines())
        foreach (Match m in Link().Matches(text))
        {
            var target = m.Groups["target"].Value.Split('#')[0];
            if (target.Length == 0 || target.StartsWith("http", StringComparison.Ordinal)
                                   || target.StartsWith("mailto:", StringComparison.Ordinal)) continue;

            var from = Files.FolderOf(Path.Combine(Repo.Root, page));
            if (File.Exists(Path.Combine(from, target)) || Directory.Exists(Path.Combine(from, target))) continue;

            dead.Add($"{page}:{line} links {target}, which nothing answers to");
        }

        Assert.True(dead.Count == 0,
            "a page links a file the repository does not hold:\n  " + string.Join("\n  ", dead));
    }

    [Fact]
    public void Every_command_a_page_types_is_one_the_parser_accepts()
    {
        var verbs = CliReference.Verbs().Select(v => v.Name).Concat(Hidden).ToHashSet(StringComparer.Ordinal);
        var unknown = new List<string>();

        foreach (var (page, line, text) in Lines())
        foreach (Match code in Code().Matches(text))
        foreach (Match m in Invocation().Matches(code.Value))
        {
            var verb = m.Groups["verb"].Value;
            if (verbs.Contains(verb)) continue;

            unknown.Add($"{page}:{line} types `kac {verb}`, which the parser does not accept");
        }

        Assert.True(unknown.Count == 0,
            "a page types a command the parser does not accept:\n  " + string.Join("\n  ", unknown));
    }

    /// <summary>
    /// The changelog is left out. A released entry is fixed to its release, so it goes on naming a verb long after the
    /// verb was replaced.
    /// </summary>
    private static IEnumerable<(string Page, int Line, string Text)> Lines()
    {
        foreach (var file in Pages())
        {
            var line = 0;
            foreach (var text in File.ReadLines(file))
            {
                line++;
                yield return (Path.GetRelativePath(Repo.Root, file).Replace('\\', '/'), line, text);
            }
        }
    }

    private static IEnumerable<string> Pages() =>
        Directory.EnumerateFiles(Path.Combine(Repo.Root, "docs"), "*.md", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(Path.Combine(Repo.Root, "tooling"), "*.md", SearchOption.AllDirectories))
            .Append(Path.Combine(Repo.Root, "README.md"))
            .Where(f => Path.GetFileName(f) != "CHANGELOG.md")
            .Where(f => !f.Contains(Path.Combine("tests", "fixtures"), StringComparison.Ordinal))
            .Where(NotBuildOutput);

    private static IEnumerable<string> Sources() =>
        Directory.EnumerateFiles(Path.Combine(Repo.Root, "tooling"), "*.cs", SearchOption.AllDirectories)
            .Where(NotBuildOutput);

    private static bool NotBuildOutput(string file) =>
        !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
        && !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}");

    private static Dictionary<string, List<string>> Declarations()
    {
        var declarations = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        foreach (var file in Sources())
        foreach (Match m in Declaration().Matches(File.ReadAllText(file)))
        {
            var name = m.Groups["name"].Value;
            if (!declarations.TryGetValue(name, out var files)) declarations[name] = files = [];
            if (!files.Contains(file)) files.Add(file);
        }

        return declarations;
    }
}
