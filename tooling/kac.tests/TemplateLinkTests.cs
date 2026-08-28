using System.Text.RegularExpressions;
using kac.core;

// The links a seed page may carry, held at the source.
//
// A seed reaches a corpus that has adopted some of the types and turned the rest down. `SeedLinks` unlinks
// a reference to a declined type's page as the page is written, which is what lets a type page keep the
// cross-references that make a full corpus navigable. That repair only works on a link whose text is the
// type's own noun.
//
// A link into a type's folder has no such repair. Its text names a record, so unlinking it would leave a
// sentence pointing at nothing, and every corpus deletes the records it inherits anyway. There is nowhere
// to fix that but here.
//
// A target holding a placeholder is exempt, on the same reading `LinkChecks` takes for one: it addresses
// no page in any corpus, so there is nothing to dangle.
//
// Two more forms have no repair either, for the opposite reason: the unlinking reads raw Markdown, so a
// reference definition carries no `](` for it to match and a fenced example carries one it should have
// left alone. Both are refused here rather than taught to the transform.

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public partial class TemplateLinkTests
{
    // An inline Markdown link, with the anchor split off: what is asked below is which folder the target
    // opens, and a heading inside the page has no bearing on that.
    [GeneratedRegex(@"\[(?<text>[^\]\r\n]+)\]\((?<target>[^)\r\n#]+)(?:#[^)\r\n]*)?\)")]
    private static partial Regex Link();

    private static readonly Schema Declared = Schema.Load(Repo.Root);

    public static TheoryData<string> Seeds()
    {
        var manifest = Manifest.LoadFrom(Path.Combine(Repo.Root, Manifest.FileName));
        var data = new TheoryData<string>();

        foreach (var full in Directory.EnumerateFiles(Path.Combine(Repo.Root, "template"), "*.md",
                     SearchOption.AllDirectories))
        {
            var rel = Path.GetRelativePath(Repo.Root, full).Replace('\\', '/');
            if (manifest.Place(rel) is { Layer: Manifest.Seed }) data.Add(rel);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void A_seed_page_never_links_into_another_types_folder(string rel)
    {
        var owner = Owner(rel);

        var reaching = Local(Targets(Read(rel)))
            .Where(t => t.Contains('/', StringComparison.Ordinal))
            .Select(t => t[..t.IndexOf('/', StringComparison.Ordinal)])
            .Where(folder => Declared.ByFolder.ContainsKey(folder) && folder != owner)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        Assert.True(reaching.Count == 0,
            $"{rel} links into {string.Join(", ", reaching)}. a corpus that declined the type holds no "
            + "such folder, and the link cannot be unlinked into a word. name the type and drop the link.");
    }

    // A reference link's definition, which the unlinking never matches: it looks for `](`, and a definition
    // separates its label from its target with a colon.
    [GeneratedRegex(@"^\[[^\]\r\n]+\]:\s*(?<target>\S+)", RegexOptions.Multiline)]
    private static partial Regex Definition();

    // Every target a page addresses, whichever form carries it. The folder question is the same for both:
    // a corpus that declined the type holds no folder, and neither form has a repair once it is there.
    private static IEnumerable<string> Targets(string markdown) =>
        Link().Matches(markdown).Select(m => m.Groups["target"].Value)
            .Concat(Definition().Matches(markdown).Select(m => m.Groups["target"].Value));

    [Theory]
    [MemberData(nameof(Seeds))]
    public void A_seed_page_never_defines_a_reference_link_to_a_type_page(string rel)
    {
        var pages = Named(Definition().Matches(Read(rel)).Select(m => m.Groups["target"].Value));

        Assert.True(pages.Count == 0,
            $"{rel} defines a reference link to {string.Join(", ", pages)}. the unlinking matches an inline "
            + "link and nothing else, so this one dangles in a corpus that declined the type. write it "
            + "inline, or name the type without linking it.");
    }

    [Theory]
    [MemberData(nameof(Seeds))]
    public void A_seed_page_never_shows_a_link_to_a_type_page_inside_a_fence(string rel)
    {
        var pages = Named(Fenced(Read(rel).Split('\n'))
            .SelectMany(line => Link().Matches(line).Select(m => m.Groups["target"].Value)));

        Assert.True(pages.Count == 0,
            $"{rel} shows a link to {string.Join(", ", pages)} inside a fenced block. the unlinking reads "
            + "the page as text and would rewrite the example. name a type that cannot be declined, or "
            + "drop the link from the example.");
    }

    // Every line inside a fenced block. The fence is read at the start of a line, indented or not, which
    // is how a fence nested in a list item opens.
    private static IEnumerable<string> Fenced(IEnumerable<string> lines)
    {
        var inside = false;

        foreach (var line in lines)
        {
            if (line.TrimStart().StartsWith("```", StringComparison.Ordinal))
            {
                inside = !inside;
                continue;
            }

            if (inside) yield return line;
        }
    }

    // The targets that address a file in the corpus, each climbed back to a corpus-root path. An external
    // link addresses somebody else's site, and a placeholder addresses no page in any corpus.
    private static IEnumerable<string> Local(IEnumerable<string> targets) =>
        targets
            .Where(t => !LinkChecks.IsExternal(t) && !Placeholder.In(t))
            .Select(Climbed);

    // The type pages among a run of link targets, by the page each type declares.
    private static List<string> Named(IEnumerable<string> targets) =>
    [
        .. Local(targets)
            .Select(t => t.Split('#')[0])
            .Where(t => Declared.ByFolder.Values.Any(x => string.Equals(x.Page, t, StringComparison.Ordinal)))
            .Distinct(StringComparer.Ordinal)
    ];

    // LF-normalised, so a line count and a fence are the same on a CRLF checkout.
    private static string Read(string rel) => Files.ReadLf(Path.Combine(Repo.Root, rel));

    // A target with every leading `../` taken off, so a link climbing twice is read the same way as one
    // climbing once rather than resolving to `..` and passing.
    private static string Climbed(string target)
    {
        while (target.StartsWith("../", StringComparison.Ordinal)) target = target[3..];
        return target;
    }

    // The type's folder this seed may reach into, because the page and the folder arrive together or not
    // at all. A file inside a type's folder owns the folder holding it.
    //
    // A root page owns the folder of the type whose page it is, and that is a lookup rather than its own
    // name. A type may declare a `page:` that does not match its `folder:`, which is the same reason
    // `SeedLinks.Declined` keys on the page. A page no type claims owns nothing.
    private static string? Owner(string rel)
    {
        var parts = rel.Split('/');
        if (parts.Length > 2) return parts[^2];

        foreach (var (name, type) in Declared.ByFolder)
            if (string.Equals(type.Page, parts[^1], StringComparison.Ordinal))
                return string.IsNullOrEmpty(type.Folder) ? name : type.Folder;

        return null;
    }
}
