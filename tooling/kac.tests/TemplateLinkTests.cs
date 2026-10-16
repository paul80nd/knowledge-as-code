using System.Text.RegularExpressions;
using kac.core;

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public partial class TemplateLinkTests
{
    /// <summary>An inline Markdown link, with the anchor split off.</summary>
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
    public void A_seed_page_never_links_a_named_record_in_another_types_folder(string rel)
    {
        var owner = Owner(rel);

        var reaching = Local(Targets(Read(rel)))
            .Where(t => t.Contains('/', StringComparison.Ordinal))
            .Select(t => t[..t.IndexOf('/', StringComparison.Ordinal)])
            .Where(folder => Declared.ByFolder.ContainsKey(folder) && folder != owner)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        Assert.True(reaching.Count == 0,
            $"{rel} links a record in {string.Join(", ", reaching)}. no other corpus holds that record. "
            + "link the type's root page, or write the target with a placeholder.");
    }

    [GeneratedRegex(@"^\[[^\]\r\n]+\]:\s*(?<target>\S+)", RegexOptions.Multiline)]
    private static partial Regex Definition();

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

    /// <summary>
    /// Every line inside a fenced block. The fence is read at the start of a line, indented or not, which is how a
    /// fence nested in a list item opens.
    /// </summary>
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

    /// <summary>
    /// A placeholder target is how a seed links another type's records. <c>SeedLinks</c> removes its line where a
    /// corpus declined the type.
    /// </summary>
    private static IEnumerable<string> Local(IEnumerable<string> targets) =>
        targets
            .Where(t => !LinkChecks.IsExternal(t) && !Placeholder.In(t))
            .Select(Climbed);

    private static List<string> Named(IEnumerable<string> targets) =>
    [
        .. Local(targets)
            .Select(t => t.Split('#')[0])
            .Where(t => Declared.ByFolder.Values.Any(x => string.Equals(x.Page, t, StringComparison.Ordinal)))
            .Distinct(StringComparer.Ordinal)
    ];

    /// <summary>LF-normalised, so a line count and a fence are the same on a CRLF checkout.</summary>
    private static string Read(string rel) => Files.ReadLf(Path.Combine(Repo.Root, rel));

    /// <summary>
    /// A target with every leading <c>../</c> taken off, so a link climbing twice is read the same way as one climbing
    /// once rather than resolving to <c>..</c> and passing.
    /// </summary>
    private static string Climbed(string target)
    {
        while (target.StartsWith("../", StringComparison.Ordinal)) target = target[3..];
        return target;
    }

    /// <summary>
    /// The type's folder this seed may reach into. A file inside a type's folder owns the folder holding it. A root
    /// page owns the folder of the type whose page it is, by lookup, because a type may declare a <c>page:</c> that
    /// does not match its <c>folder:</c>. A page no type claims owns nothing.
    /// </summary>
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
