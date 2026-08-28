using System.Text.RegularExpressions;

namespace kac.core;

// The cross-references a corpus receives on the seed pages it is sent.
//
// A type's root page and its `_template.md` name the other types and link to them. A corpus that declined
// one of those types holds no page for the link to reach, so the link is taken out and its own text left
// standing. The sentence still names the type. It no longer promises a page.
//
// Seeds only, and the layer is the whole argument. An overlay file is byte-identical in every corpus, so
// a link resolving in one says nothing about where it is read, and `framework-names-types` refuses it
// outright. A seed differs from corpus to corpus, which is what makes a link worth keeping wherever it
// does resolve.
public static partial class SeedLinks
{
    // A link to a type's root page, in the two forms a seed writes: from the corpus root, and from inside
    // a type's own folder. An anchor is swallowed with the rest, because a page a corpus does not hold
    // has no headings either.
    //
    // A path *into* a type's folder is left alone on purpose. Its text names a record rather than a type,
    // so there is no word to leave standing, and `TemplateLinkTests` refuses one at the source instead.
    [GeneratedRegex(@"\[(?<text>[^\]\r\n]+)\]\((?:\.\./)?(?<page>[A-Za-z0-9][A-Za-z0-9._-]*)\.md(?:#[^)\r\n]*)?\)")]
    private static partial Regex PageLink();

    /// The page names of the types this corpus turned down, as a link to one of them writes it.
    ///
    /// Keyed on the page rather than on the type, because the page is what a link carries and a type may
    /// name one that does not match its folder.
    public static IReadOnlySet<string> Declined(Schema schema, IReadOnlyList<string> adopted)
    {
        var pages = new HashSet<string>(StringComparer.Ordinal);

        foreach (var (name, type) in schema.ByFolder)
        {
            if (adopted.Contains(name, StringComparer.Ordinal)) continue;
            if (string.IsNullOrEmpty(type.Page)) continue;
            pages.Add(Path.GetFileNameWithoutExtension(type.Page));
        }

        return pages;
    }

    /// Whether this file is one the unlinking reads: a seed, and Markdown.
    public static bool Reaches(PlannedFile file) =>
        file.Layer == Manifest.Seed && file.To.EndsWith(".md", StringComparison.OrdinalIgnoreCase);

    /// One seed page as a corpus that declined those types receives it.
    ///
    /// The text of each unlinked reference is kept exactly as it was written, so the noun the sentence was
    /// built around is the noun that remains.
    public static string Unlinked(string markdown, IReadOnlySet<string> declined) =>
        declined.Count == 0
            ? markdown
            : PageLink().Replace(markdown, m =>
                declined.Contains(m.Groups["page"].Value) ? m.Groups["text"].Value : m.Value);

    /// Copy one planned file into a corpus, taking out the links its owner cannot follow.
    ///
    /// The copy goes first and carries the mode across, so a file the unlinking does not reach is written
    /// exactly as `Files.Copy` would write it. Only a page that actually changed is written a second time,
    /// which is what keeps the line endings the template was checked out with.
    public static void Receive(PlannedFile file, string templateRoot, string corpusRoot,
        IReadOnlySet<string> declined)
    {
        var target = Path.Combine(corpusRoot, file.To);
        Files.Copy(Path.Combine(templateRoot, file.From), target);
        if (!Reaches(file)) return;

        var sent = File.ReadAllText(target);
        var received = Unlinked(sent, declined);
        if (received != sent) File.WriteAllText(target, received);
    }
}
