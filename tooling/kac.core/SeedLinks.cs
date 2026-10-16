using System.Text;
using System.Text.RegularExpressions;

namespace kac.core;

/// <summary>
/// The types a corpus turned down, by the two names a link to one of them writes: the page and the folder. They differ
/// where a type declares a <c>page:</c> that does not match its <c>folder:</c>.
/// </summary>
public sealed record DeclinedNames(IReadOnlySet<string> Pages, IReadOnlySet<string> Folders)
{
    public static DeclinedNames None { get; } =
        new(new HashSet<string>(StringComparer.Ordinal), new HashSet<string>(StringComparer.Ordinal));

    public bool IsEmpty => Pages.Count == 0 && Folders.Count == 0;
}

/// <summary>
/// The cross-references a corpus receives on the seed pages it is sent. A link to a declined type's page is taken out
/// and its text left standing. A line linking into a declined type's folder is removed whole, with the definition
/// behind it. Seeds only: an overlay file is identical in every corpus, and <c>framework-names-types</c> refuses such a
/// link there outright.
/// </summary>
public static partial class SeedLinks
{
    /// <summary>A link to a type's root page, from the corpus root or from inside a type's own folder.</summary>
    [GeneratedRegex(@"\[(?<text>[^\]\r\n]+)\]\((?:\.\./)?(?<page>[A-Za-z0-9][A-Za-z0-9._-]*)\.md(?:#[^)\r\n]*)?\)")]
    private static partial Regex PageLink();

    /// <summary>
    /// A path into a type's folder. Its text names a record, so there is no word to leave standing once it goes.
    /// </summary>
    [GeneratedRegex(@"\]\((?:\.\./)*(?<folder>[A-Za-z0-9][A-Za-z0-9._-]*)/[^)\r\n]*\)")]
    private static partial Regex FolderLink();

    [GeneratedRegex(@"^ {0,3}\[(?<label>[^\]\r\n]+)\]:[ \t]*(?:\.\./)*(?<folder>[A-Za-z0-9][A-Za-z0-9._-]*)/",
        RegexOptions.Multiline)]
    private static partial Regex FolderDefinition();

    /// <summary>Each line with its own ending, so a page keeps the line endings it was checked out with.</summary>
    [GeneratedRegex(@"[^\r\n]*(?:\r\n|\n|\r|$)")]
    private static partial Regex Line();

    public static DeclinedNames Declined(Schema schema, IReadOnlyList<string> adopted)
    {
        var pages = new HashSet<string>(StringComparer.Ordinal);
        var folders = new HashSet<string>(StringComparer.Ordinal);

        foreach (var (folder, type) in schema.ByFolder)
        {
            if (adopted.Contains(folder, StringComparer.Ordinal)) continue;
            folders.Add(folder);
            if (!string.IsNullOrEmpty(type.Page)) pages.Add(Path.GetFileNameWithoutExtension(type.Page));
        }

        return new DeclinedNames(pages, folders);
    }

    /// Whether this file is one the unlinking reads: a seed, and Markdown.
    public static bool Reaches(PlannedFile file) =>
        file.Layer == Manifest.Seed && file.To.EndsWith(".md", StringComparison.OrdinalIgnoreCase);

    /// One seed page as a corpus that declined those types receives it.
    ///
    /// The text of each unlinked reference is kept exactly as it was written, so the noun the sentence was
    /// built around is the noun that remains.
    public static string Unlinked(string markdown, DeclinedNames declined) =>
        declined.IsEmpty
            ? markdown
            : PageLink().Replace(WithoutFolderLines(markdown, declined.Folders), m =>
                declined.Pages.Contains(m.Groups["page"].Value) ? m.Groups["text"].Value : m.Value);

    /// <summary>
    /// A line goes where it defines a label into a declined folder, cites one of those labels, or links into such a
    /// folder inline. Labels match case-insensitively, as Markdown matches them. A line that went from between two
    /// blank lines takes one of them with it.
    /// </summary>
    private static string WithoutFolderLines(string markdown, IReadOnlySet<string> folders)
    {
        var labels = FolderDefinition().Matches(markdown)
            .Where(m => folders.Contains(m.Groups["folder"].Value))
            .Select(m => $"[{m.Groups["label"].Value}]")
            .ToList();

        var kept = new StringBuilder(markdown.Length);
        var lastKeptBlank = true;
        var dropped = false;

        foreach (Match line in Line().Matches(markdown))
        {
            if (line.Length == 0) continue;
            var text = line.Value.TrimEnd('\r', '\n');

            if (FolderLink().Matches(text).Any(f => folders.Contains(f.Groups["folder"].Value))
                || labels.Any(l => text.Contains(l, StringComparison.OrdinalIgnoreCase)))
            {
                dropped = true;
                continue;
            }

            var blank = string.IsNullOrWhiteSpace(text);
            if (blank && dropped && lastKeptBlank) continue;

            kept.Append(line.Value);
            lastKeptBlank = blank;
            dropped = false;
        }

        return kept.ToString();
    }

    /// Copy one planned file into a corpus, taking out the links its owner cannot follow. Only a page the unlinking
    /// changed is written a second time, so every other file keeps the line endings it was checked out with.
    public static void Receive(PlannedFile file, string templateRoot, string corpusRoot, DeclinedNames declined)
    {
        var target = Path.Combine(corpusRoot, file.To);
        Files.Copy(Path.Combine(templateRoot, file.From), target);
        if (!Reaches(file)) return;

        var sent = File.ReadAllText(target);
        var received = Unlinked(sent, declined);
        if (received != sent) File.WriteAllText(target, received);
    }
}
