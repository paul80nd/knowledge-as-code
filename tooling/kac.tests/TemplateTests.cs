using kac.core;

// The template is the set of files `manifest.yaml` names, read from wherever in this repository they
// are authored, and `example/` holds a materialised copy of the part a corpus receives. Two copies of
// one file is the arrangement the tool exists to manage, and it is also the arrangement that goes
// quietly wrong: an edit lands in whichever copy the author had open, and nothing about the other one
// looks different afterwards.
//
// So these hold the overlaid half to being the same bytes, in both directions. They read the repository
// rather than a value built in the test, as `DocumentationTests` does and for the same reason: the
// fault is a file drifting where nobody is looking.
//
// What they do not reach is the seeded half. A seed is a starting point the corpus rewrites, and one it
// deletes outright is a corpus declining a starting point it did not want. `example/` runs this
// repository's own CI rather than the starter it was sent, so even a seed's arrival cannot be asserted.

namespace kac.tests;

public class TemplateTests
{
    private static readonly string Root = Repo.Root;
    private static readonly string Corpus = Path.Combine(Root, "example");

    private static readonly Manifest Layers = Manifest.LoadFrom(Path.Combine(Root, "manifest.yaml"));

    // The same rules read from the corpus's side: each rule's patterns rewritten to where its files land,
    // in the order they were written. A corpus file has to be sorted by the rule that sent it, and only
    // the order says which that was: `.plugin/.claude-plugin/plugin.json` is a seed carved out ahead of
    // the overlay claiming the folder around it.
    private static readonly Manifest AsReceived = new()
    {
        Rules = [.. Layers.Rules.Select(r => new ManifestRule([.. Manifest.Destinations(r)], r.Layer))]
    };

    // Every file a tree holds, as the tool itself asks: `git ls-files` respects `.gitignore`, so a
    // build directory or an export is never mistaken for content, and a file added but not yet
    // committed is still seen.
    private static List<string> Files(string root) =>
        GitFiles.Tracked(root) ?? GitFiles.Walk(root, "*", ".git");

    // What the repository sends to a corpus under one layer: each source path beside where it lands.
    //
    // A file landing on the path it was read from is shared with the corpora in this repository rather
    // than copied into them. `.schema/` and the travelling skills sit at this root, above both of them,
    // and there is no second copy to hold anything against. A corpus created elsewhere receives its own.
    private static List<(string From, string To)> Sent(string layer) =>
    [
        .. Files(Root)
            .Select(rel => (From: rel, Placed: Layers.Place(rel)))
            .Where(x => x.Placed?.Layer == layer)
            .Select(x => (x.From, x.Placed!.Path))
            .Where(x => x.From != x.Path)
    ];

    private static bool Same(string from, string to) =>
        File.ReadAllBytes(Path.Combine(Root, from))
            .SequenceEqual(File.ReadAllBytes(Path.Combine(Corpus, to)));

    // A corpus that took this template holds the overlaid file the template sends. `example/` is such a
    // corpus, so an overlaid file absent there is a file no corpus would receive.
    [Fact]
    public void Every_overlaid_file_reaches_the_corpus()
    {
        var absent = Sent(Manifest.Overlay)
            .Where(x => !File.Exists(Path.Combine(Corpus, x.To)))
            .Select(x => x.To)
            .ToList();

        Assert.True(absent.Count == 0, Report("are overlaid and absent from example/", absent));
    }

    // And reaches it unchanged. An edit made in the corpus is the one this catches: it is the copy an
    // author has open, and the next overlay would take it back without saying so.
    [Fact]
    public void Every_overlaid_file_reaches_the_corpus_unchanged()
    {
        var drifted = Sent(Manifest.Overlay)
            .Where(x => File.Exists(Path.Combine(Corpus, x.To)))
            .Where(x => !Same(x.From, x.To))
            .Select(x => x.To)
            .ToList();

        Assert.True(drifted.Count == 0, Report("differ from where they are authored", drifted));
    }

    // The other direction. A file added to the corpus inside an area the rules call overlaid is a
    // framework change written in the wrong tree: it would reach no other corpus, and nothing in the
    // corpus reads as though anything is missing.
    [Fact]
    public void The_corpus_holds_no_overlaid_file_the_template_lacks()
    {
        var sent = Sent(Manifest.Overlay).Select(x => x.To).ToHashSet(StringComparer.Ordinal);

        var unshared = Files(Corpus)
            .Where(rel => AsReceived.Resolve(rel) == Manifest.Overlay)
            .Where(rel => !sent.Contains(rel))
            .ToList();

        Assert.True(unshared.Count == 0, Report("are overlaid in example/ and sent by nothing", unshared));
    }

    // A failing guard names the files. The whole point is to say where the copies parted, and a bare
    // count leaves whoever reads it running the comparison again by hand.
    private static string Report(string what, List<string> paths) =>
        $"{paths.Count} file(s) {what}:\n  " + string.Join("\n  ", paths);
}
