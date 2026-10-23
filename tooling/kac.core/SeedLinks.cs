using System.Text;
using System.Text.RegularExpressions;

namespace kac.core;

/// <summary>
/// The types a corpus turned down, by the two names a link to one of them writes: the page and the folder. They differ
/// where a type declares a <c>page:</c> that does not match its <c>folder:</c>. <c>Pages</c> maps each page to the
/// type's folder, which is the name <c>types:</c> spells.
/// </summary>
public sealed record DeclinedNames(IReadOnlyDictionary<string, string> Pages, IReadOnlySet<string> Folders)
{
    public static DeclinedNames None { get; } =
        new(new Dictionary<string, string>(StringComparer.Ordinal), new HashSet<string>(StringComparer.Ordinal));

    public bool IsEmpty => Pages.Count == 0 && Folders.Count == 0;
}

/// <summary>
/// A line of a seed page that lost its link to a declined type and kept the words. <c>Number</c> counts from 1 on the
/// page as written. <c>Types</c> lists each declined type the line linked, in the order it linked them.
/// </summary>
public sealed record UnlinkedLine(int Number, IReadOnlyList<string> Types);

/// <summary>A seed page as written into a corpus, with each line on it that lost a link.</summary>
public sealed record UnlinkedPage(string Path, IReadOnlyList<UnlinkedLine> Lines);

/// <summary>
/// The cross-references a corpus receives on the seed pages it is sent. A link to a declined type's page is taken out
/// and its text left standing, and a reference definition naming that page goes with it. A line linking into a
/// declined type's folder is removed whole, with the definition behind it. Seeds only: an overlay file is identical in
/// every corpus, and <c>framework-names-types</c> refuses such a link there outright.
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

    /// <summary>A reference definition naming a type's root page, by the paths <c>PageLink</c> reads.</summary>
    [GeneratedRegex(@"^ {0,3}\[(?<label>[^\]\r\n]+)\]:[ \t]*(?:\.\./)?(?<page>[A-Za-z0-9][A-Za-z0-9._-]*)\.md"
                    + @"(?:#[^\s]*)?(?:[ \t][^\r\n]*)?\r?$", RegexOptions.Multiline)]
    private static partial Regex PageDefinition();

    /// <summary>
    /// A reference link in its full, collapsed or shortcut form. An empty <c>label</c> group is the collapsed form, and
    /// a missing one the shortcut. Brackets followed by <c>(</c> are an inline link.
    /// </summary>
    [GeneratedRegex(@"(?<!\])\[(?<text>[^\[\]\r\n]+)\](?:\[(?<label>[^\[\]\r\n]*)\])?(?![(\[])")]
    private static partial Regex ReferenceLink();

    /// <summary>Each line with its own ending, so a page keeps the line endings it was checked out with.</summary>
    [GeneratedRegex(@"[^\r\n]*(?:\r\n|\n|\r|$)")]
    private static partial Regex Line();

    public static DeclinedNames Declined(Schema schema, IReadOnlyList<string> adopted)
    {
        var pages = new Dictionary<string, string>(StringComparer.Ordinal);
        var folders = new HashSet<string>(StringComparer.Ordinal);

        foreach (var (folder, type) in schema.ByFolder)
        {
            if (adopted.Contains(folder, StringComparer.Ordinal)) continue;
            folders.Add(folder);
            if (!string.IsNullOrEmpty(type.Page)) pages[Path.GetFileNameWithoutExtension(type.Page)] = folder;
        }

        return new DeclinedNames(pages, folders);
    }

    /// <summary>Whether this file is one the unlinking reads: a seed, and Markdown.</summary>
    public static bool Reaches(PlannedFile file) =>
        file.Layer == Manifest.Seed && file.To.EndsWith(".md", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// One seed page as a corpus that declined those types receives it. The text of each unlinked reference is kept
    /// exactly as it was written, so the noun the sentence was built around is the noun that remains.
    /// </summary>
    public static string Unlinked(string markdown, DeclinedNames declined) => Unlink(markdown, declined).Text;

    /// <summary>
    /// The page as received, and each line on it that kept the words of a link it lost. A line dropped whole is not
    /// among them, because nothing of it is left to read.
    /// </summary>
    public static (string Text, IReadOnlyList<UnlinkedLine> Lines) Unlink(string markdown, DeclinedNames declined)
    {
        if (declined.IsEmpty) return (markdown, []);

        var text = new StringBuilder(markdown.Length);
        var lines = new List<UnlinkedLine>();
        var number = 0;
        var labels = PageLabels(markdown, declined.Pages);

        foreach (Match line in Line().Matches(WithoutLines(markdown, declined.Folders, labels)))
        {
            if (line.Length == 0) continue;
            number++;

            var types = new List<string>();
            var inline = PageLink().Replace(line.Value, m =>
                declined.Pages.TryGetValue(m.Groups["page"].Value, out var type)
                    ? KeepText(m, type, types)
                    : m.Value);
            text.Append(ReferenceLink().Replace(inline, m =>
                labels.TryGetValue(Label(m), out var type) ? KeepText(m, type, types) : m.Value));
            if (types.Count > 0) lines.Add(new UnlinkedLine(number, types));
        }

        return (text.ToString(), lines);
    }

    /// <summary>The label a reference link cites. The collapsed and shortcut forms cite their own text.</summary>
    private static string Label(Match reference) =>
        reference.Groups["label"] is { Success: true, Length: > 0 } label ? label.Value : reference.Groups["text"].Value;

    /// <summary>The words a link to <paramref name="type"/> leaves standing. Adds the type to <paramref name="types"/>
    /// once.</summary>
    private static string KeepText(Match link, string type, List<string> types)
    {
        if (!types.Contains(type, StringComparer.Ordinal)) types.Add(type);
        return link.Groups["text"].Value;
    }

    /// <summary>Each label defined as a declined type's page, with that type's folder, matched as <c>WithoutLines</c>
    /// says.</summary>
    private static Dictionary<string, string> PageLabels(string markdown, IReadOnlyDictionary<string, string> pages)
    {
        var labels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match m in PageDefinition().Matches(markdown))
            if (pages.TryGetValue(m.Groups["page"].Value, out var type))
                labels.TryAdd(m.Groups["label"].Value, type);
        return labels;
    }

    /// <summary>
    /// A line goes where it defines a label into a declined folder, cites one of those labels, or links into such a
    /// folder inline. A line defining one of the page <paramref name="pageLabels"/> goes too, and the lines citing it
    /// stay. Labels match case-insensitively, as Markdown matches them. A line that went from between two blank lines
    /// takes one of them with it.
    /// </summary>
    private static string WithoutLines(string markdown, IReadOnlySet<string> folders,
        IReadOnlyDictionary<string, string> pageLabels)
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
                || labels.Any(l => text.Contains(l, StringComparison.OrdinalIgnoreCase))
                || (PageDefinition().Match(text) is { Success: true } page
                    && pageLabels.ContainsKey(page.Groups["label"].Value)))
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

    /// <summary>
    /// Copy one planned file into a corpus, taking out the links its owner cannot follow. Only a page the unlinking
    /// changed is written a second time, so every other file keeps the line endings it was checked out with. Returns
    /// null where no line kept the words of a link it lost.
    /// </summary>
    public static UnlinkedPage? Receive(PlannedFile file, string templateRoot, string corpusRoot,
        DeclinedNames declined)
    {
        var target = Path.Combine(corpusRoot, file.To);
        Files.Copy(Path.Combine(templateRoot, file.From), target);
        if (!Reaches(file)) return null;

        var sent = File.ReadAllText(target);
        var (received, lines) = Unlink(sent, declined);
        if (received != sent) File.WriteAllText(target, received);
        return lines.Count == 0 ? null : new UnlinkedPage(file.To, lines);
    }

    /// <summary>Each file in turn, keeping the pages that lost a link.</summary>
    public static IReadOnlyList<UnlinkedPage> ReceiveAll(IEnumerable<PlannedFile> files, string templateRoot,
        string corpusRoot, DeclinedNames declined) =>
        [.. files.Select(f => Receive(f, templateRoot, corpusRoot, declined)).OfType<UnlinkedPage>()];
}
