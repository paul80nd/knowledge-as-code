using kac.core;

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public class ReceivedLinkTests
{
    private static readonly Schema Declared = Schema.Load(Repo.Root);

    [Theory]
    [InlineData("[the tool's guidance](../tooling/CLAUDE.md)")]
    [InlineData("[the tool's guidance][guide]\n\n[guide]: ../tooling/CLAUDE.md")]
    [InlineData("[guide][]\n\n[guide]: ../tooling/CLAUDE.md")]
    public void A_received_file_linking_a_withheld_file_is_reported(string link)
    {
        var root = Fixture(link);

        var found = Assert.Single(Dangling(root));

        Assert.Contains(".schema/README.md", found, StringComparison.Ordinal);
        Assert.Contains("../tooling/CLAUDE.md", found, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("[another schema page](_types.md)")]
    [InlineData("[another schema page](./%5Ftypes.md#types)")]
    [InlineData("[a heading on this page](#schema)")]
    [InlineData("[the site](https://example.com/tooling/CLAUDE.md)")]
    [InlineData("`[the tool's guidance](../tooling/CLAUDE.md)`")]
    [InlineData("```markdown\n[the tool's guidance](../tooling/CLAUDE.md)\n```")]
    public void A_received_file_linking_what_a_corpus_receives_is_not_reported(string link)
    {
        var root = Fixture(link);
        File.WriteAllText(Path.Combine(root, ".schema", "_types.md"), "# Types\n");

        Assert.Empty(Dangling(root));
    }

    [Fact]
    public void No_received_file_links_a_path_no_corpus_receives()
    {
        var dangling = Dangling(Repo.Root);

        Assert.True(dangling.Count == 0,
            "a corpus receives these links and nothing they lead to. Link a file the manifest sends, or link the "
            + "published page by URL.\n" + string.Join('\n', dangling));
    }

    /// <summary>
    /// Every relative link in a Markdown file the manifest sends, resolved from where the file lands in a corpus
    /// against everything the manifest sends beside it. A placeholder target is filled in by whoever copies the page,
    /// so it passes.
    /// </summary>
    private static IReadOnlyList<string> Dangling(string root)
    {
        var manifest = Manifest.LoadFrom(Path.Combine(root, Manifest.FileName));
        var listing = new HashSet<string>(GitFiles.Tracked(root) ?? GitFiles.Walk(root, "*", ".git"),
            StringComparer.Ordinal);
        var sent = (
            from rel in listing
            let placement = manifest.Place(rel)
            where placement is { Layer: Manifest.Overlay or Manifest.Seed }
            select (From: rel, To: placement.Path)).ToList();

        // `kac new` writes the descriptor and the README itself, so every corpus has them whatever the manifest says.
        var received = new Tree(
            sent.Select(f => f.To).Append(".corpus.yaml").Append("README.md").ToHashSet(StringComparer.Ordinal),
            _ => "");
        var here = new Tree(listing, _ => "");

        var dangling = new List<string>();
        foreach (var (from, to) in sent.Where(f => f.From.EndsWith(".md", StringComparison.Ordinal)))
        {
            if (Doc.Parse(from, Files.ReadLf(Path.Combine(root, from)), Declared, requireFrontmatter: false)
                is not { } doc) continue;

            var targets = doc.Links.Select(l => l.Target).Concat(doc.DefinedTargets.Values)
                .Select(Uri.UnescapeDataString)
                .Where(t => t.Split('#')[0].Length > 0 && !LinkChecks.IsExternal(t) && !Placeholder.In(t))
                .Distinct(StringComparer.Ordinal);

            foreach (var target in targets.Where(t => LinkChecks.Resolve(received, to, t) is null))
            {
                var named = LinkChecks.Resolve(here, from, target);
                dangling.Add($"{from} links '{target}', and no corpus receives the file it names. " + (named is null
                    ? "It names no file in this repository either."
                    : $"In this repository it names '{named}', which {Manifest.FileName} places in "
                      + $"{manifest.Resolve(named) ?? "no layer"}."));
            }
        }

        return dangling;
    }

    /// <summary>
    /// A template repository in miniature: <c>.schema/</c> reaches every corpus, and <c>tooling/</c> reaches none.
    /// </summary>
    private static string Fixture(string link)
    {
        var root = Directory.CreateTempSubdirectory().FullName;
        Directory.CreateDirectory(Path.Combine(root, ".schema"));
        Directory.CreateDirectory(Path.Combine(root, "tooling"));

        File.WriteAllText(Path.Combine(root, Manifest.FileName),
            "version: 1\nrules:\n  - {path: \".schema/**\", layer: overlay}\n  - {path: \"**\", layer: withheld}\n");
        File.WriteAllText(Path.Combine(root, ".schema", "README.md"), $"# Schema\n\n{link}\n");
        File.WriteAllText(Path.Combine(root, "tooling", "CLAUDE.md"), "# The tool\n");
        return root;
    }
}
