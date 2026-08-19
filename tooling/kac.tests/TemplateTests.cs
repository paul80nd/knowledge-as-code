using kac.core;

// `template/` is the master copy of everything a corpus is made of, and `example/` holds a
// materialised copy of the part it receives. Two copies of one file is the arrangement the tool
// exists to manage, and it is also the arrangement that goes quietly wrong: an edit lands in
// whichever copy the author had open, and nothing about the other one looks different afterwards.
//
// So these hold the overlaid half of the two trees to being the same bytes, in both directions.
// They read the repository rather than a value built in the test, as `DocumentationTests` does and
// for the same reason: the fault is a file drifting where nobody is looking.
//
// What they do not reach is the seeded half. A seed is a starting point the corpus rewrites, so a
// difference there is the arrangement working rather than failing.

namespace kac.tests;

public class TemplateTests
{
    private const string Overlay = "overlay";

    private static readonly string Root = RepoRoot();
    private static readonly string Template = Path.Combine(Root, "template");
    private static readonly string Corpus = Path.Combine(Root, "example");

    private static readonly Manifest Layers =
        Manifest.LoadFrom(Path.Combine(Template, "manifest.yaml"));

    // Every file a tree holds, as the tool itself asks: `git ls-files` respects `.gitignore`, so a
    // build directory or an export is never mistaken for content, and a file added but not yet
    // committed is still seen.
    private static List<string> Files(string root) =>
        GitFiles.Tracked(root) ?? GitFiles.Walk(root, "*", ".git");

    // The files in a tree that the template's rules say every corpus receives again on every update.
    private static IEnumerable<string> Overlaid(string root) =>
        Files(root).Where(rel => Layers.Resolve(rel) == Overlay);

    private static bool Same(string rel) =>
        File.ReadAllBytes(Path.Combine(Template, rel))
            .SequenceEqual(File.ReadAllBytes(Path.Combine(Corpus, rel)));

    // The manifest's final rule is a catch-all, so a file resolving to nothing means the rules
    // stopped being read — not that a file was forgotten.
    [Fact]
    public void Every_file_in_the_template_resolves_to_a_layer()
    {
        var unclassified = Files(Template).Where(rel => Layers.Resolve(rel) is null).ToList();

        Assert.True(unclassified.Count == 0, Report("resolve to no layer", unclassified));
    }

    // A corpus that took this template holds the overlaid file the template holds. `example/` is a
    // corpus that took it, so an overlaid file missing there is a file no corpus would receive.
    [Fact]
    public void Every_overlaid_file_reaches_the_corpus()
    {
        var absent = Overlaid(Template)
            .Where(rel => !File.Exists(Path.Combine(Corpus, rel)))
            .ToList();

        Assert.True(absent.Count == 0, Report("are overlaid and absent from example/", absent));
    }

    // And reaches it unchanged. An edit made in the corpus is the one this catches: it is the copy
    // an author has open, and the next overlay would take it back without saying so.
    [Fact]
    public void Every_overlaid_file_reaches_the_corpus_unchanged()
    {
        var drifted = Overlaid(Template)
            .Where(rel => File.Exists(Path.Combine(Corpus, rel)))
            .Where(rel => !Same(rel))
            .ToList();

        Assert.True(drifted.Count == 0, Report("differ between template/ and example/", drifted));
    }

    // The other direction. A file added to the corpus where the template's rules call it overlaid
    // is a framework change written in the wrong tree: it would reach no other corpus, and nothing
    // in the corpus reads as though anything is missing.
    [Fact]
    public void The_corpus_holds_no_overlaid_file_the_template_lacks()
    {
        var unshared = Overlaid(Corpus)
            .Where(rel => !File.Exists(Path.Combine(Template, rel)))
            .ToList();

        Assert.True(unshared.Count == 0, Report("are overlaid and absent from template/", unshared));
    }

    // A failing guard names the files. The whole point is to say where the copies parted, and a bare
    // count leaves whoever reads it running the comparison again by hand.
    private static string Report(string what, List<string> paths) =>
        $"{paths.Count} file(s) {what}:\n  " + string.Join("\n  ", paths);

    // The repository, found by the solution at its root — the tree carrying the template and the corpus at
    // once, which is the only place both questions above can be asked.
    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "kac.slnx")))
            dir = dir.Parent;

        return dir?.FullName ?? throw new InvalidOperationException(
            "no 'kac.slnx' above the test assembly — these tests read the repository they ship in.");
    }
}
