namespace kac.core;

/// <summary>The corpus's records as they stood at the revision <c>validate --since</c> names.</summary>
/// <param name="Ref">The revision as the caller spelled it, for a message to name.</param>
public sealed record Baseline(string Ref, IReadOnlyList<Doc> Docs)
{
    /// <summary>
    /// Reads that revision and the working tree, and no history between them. A file git reports unchanged since
    /// <paramref name="reference"/> is read from the working tree. The problem names the ref where git cannot resolve
    /// it, which includes a shallow clone lacking it and a tree outside git.
    /// </summary>
    public static (Baseline? Baseline, string? Problem) Read(string corpusRoot, string reference, Schema schema,
        CorpusDescriptor descriptor)
    {
        var problem = $"validate: --since names '{reference}', and git cannot resolve it to a commit here. Check "
                      + "the ref, then fetch the full history, with `git fetch --unshallow` or `fetch-depth: 0` "
                      + "on the checkout.";

        // A ref opening with a hyphen would reach git as an option.
        if (reference.Length == 0 || reference.StartsWith('-')) return (null, problem);

        if (Git.Run(corpusRoot, "rev-parse", "--verify", "--quiet", reference + "^{commit}")?.Trim() is not
            { Length: > 0 } commit)
            return (null, problem);

        // Both list paths relative to the corpus root, and only those beneath it.
        var listed = Git.Run(corpusRoot, "ls-tree", "-r", "--name-only", "-z", commit);
        var changed = Git.Run(corpusRoot, "diff", "--name-only", "--relative", "-z", commit, "--");
        if (listed is null || changed is null) return (null, problem);

        var differ = Split(changed).ToHashSet(StringComparer.Ordinal);
        var tree = new Tree(Split(listed).ToHashSet(StringComparer.Ordinal), rel =>
        {
            var now = Path.Combine(corpusRoot, rel);
            return differ.Contains(rel) || !File.Exists(now)
                ? (Git.Run(corpusRoot, "show", $"{commit}:./{rel}") ?? "").Replace("\r\n", "\n")
                : Files.ReadLf(now);
        });

        return (new Baseline(reference, Corpus.Load(tree, schema, descriptor).Docs), null);
    }

    private static IEnumerable<string> Split(string listing) =>
        listing.Split('\0', StringSplitOptions.RemoveEmptyEntries);
}
