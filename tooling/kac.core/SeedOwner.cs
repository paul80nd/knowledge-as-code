using System.Text.RegularExpressions;

namespace kac.core;

/// <summary>
/// The owner written into a seeded record, and where it came from. <c>FromGit</c> is false where git gave no email and
/// the name the OS user logged in as stood in.
/// </summary>
public sealed record Author(string Owner, bool FromGit);

/// <summary>
/// The author of a corpus written as the <c>owner</c> of each record it is seeded with, in place of the template's own.
/// An overlay file is identical in every corpus, so it keeps the owner it was sent with.
/// </summary>
public static partial class SeedOwner
{
    /// <summary>The <c>owner</c> pattern in <c>.schema/_universal.yaml</c>.</summary>
    [GeneratedRegex("^(human|role):[a-z0-9.-]+$")]
    private static partial Regex Pattern();

    [GeneratedRegex("[^a-z0-9.-]+")]
    private static partial Regex Refused();

    /// <summary>The frontmatter at the head of a page, up to and including its closing fence.</summary>
    [GeneratedRegex(@"\A---\r?\n(?:.*\r?\n)*?---(?:\r?\n|\z)")]
    private static partial Regex Frontmatter();

    [GeneratedRegex(@"^owner:[^\r\n]*", RegexOptions.Multiline)]
    private static partial Regex OwnerLine();

    public static bool Valid(string owner) => Pattern().IsMatch(owner);

    /// <summary>
    /// Whether a planned file is a record seeded into the corpus. An underscore marks a type folder's template and
    /// index, which are not records.
    /// </summary>
    public static bool Reaches(PlannedFile file) =>
        file.Layer == Manifest.Seed
        && file.To.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
        && !Path.GetFileName(file.To).StartsWith('_');

    /// <summary>
    /// The person at the keyboard, as <c>human:</c> and the local part of <paramref name="email"/>. Where that is
    /// empty, <paramref name="user"/> stands in. A character the owner pattern refuses becomes a hyphen.
    /// </summary>
    public static Author Guess(string? email, string user)
    {
        var local = email?.Split('@')[0] ?? "";
        if (Name(local) is { Length: > 0 } fromGit) return new Author("human:" + fromGit, FromGit: true);

        return new Author("human:" + (Name(user) is { Length: > 0 } fromUser ? fromUser : "author"), FromGit: false);

        static string Name(string raw) => Refused().Replace(raw.Trim().ToLowerInvariant(), "-").Trim('-', '.');
    }

    /// <summary>Git reads the repository's own <c>user.email</c> ahead of the global one.</summary>
    public static Author Of(string dir) =>
        Guess(Git.Run(dir, "config", "user.email")?.Trim(), Environment.UserName);

    /// <summary>
    /// The page with the <c>owner</c> its frontmatter states replaced by <paramref name="owner"/>. A page with no such
    /// line is returned as it was sent, and every line ending is kept.
    /// </summary>
    public static string Owned(string markdown, string owner)
    {
        if (Frontmatter().Match(markdown) is not { Success: true } head) return markdown;

        var rewritten = OwnerLine().Replace(head.Value, "owner: " + owner, 1);
        return rewritten + markdown[head.Length..];
    }

    /// <summary>The owner the frontmatter states, or null where it states none.</summary>
    public static string? Held(string markdown) =>
        Frontmatter().Match(markdown) is { Success: true } head
        && OwnerLine().Match(head.Value) is { Success: true } line
            ? line.Value["owner:".Length..].Trim()
            : null;
}
