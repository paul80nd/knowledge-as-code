using System.Text.RegularExpressions;

namespace kac.core;

/// <summary>
/// The cross-references a corpus receives on the seed pages it is sent. A link to a type the corpus declined is taken
/// out and its text left standing. Seeds only: an overlay file is identical in every corpus, and
/// <c>framework-names-types</c> refuses such a link there outright.
/// </summary>
public static partial class SeedLinks
{
    /// <summary>
    /// A link to a type's root page, from the corpus root or from inside a type's own folder. A path into a type's
    /// folder is left alone, and <c>TemplateLinkTests</c> refuses one at the source.
    /// </summary>
    [GeneratedRegex(@"\[(?<text>[^\]\r\n]+)\]\((?:\.\./)?(?<page>[A-Za-z0-9][A-Za-z0-9._-]*)\.md(?:#[^)\r\n]*)?\)")]
    private static partial Regex PageLink();

    /// The page names of the types this corpus turned down, as a link to one of them writes it.
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

    /// Copy one planned file into a corpus, taking out the links its owner cannot follow. Only a page the unlinking
    /// changed is written a second time, so every other file keeps the line endings it was checked out with.
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
