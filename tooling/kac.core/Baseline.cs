namespace kac.core;

/// <summary>The corpus's records as they stood at the revision <c>validate --since</c> names.</summary>
/// <param name="Ref">The revision as the caller spelled it, for a message to name.</param>
/// <param name="Docs">Only the records whose file differs between that revision and the working tree.</param>
public sealed record Baseline(string Ref, IReadOnlyList<Doc> Docs)
{
    /// <summary>
    /// Reads that revision and the working tree, and no history between them. Only a file git reports changed since
    /// <paramref name="reference"/> is read, since any other pairs with itself. The problem names the ref where git
    /// cannot resolve it, which includes a shallow clone lacking it and a tree outside git.
    /// </summary>
    public static (Baseline? Baseline, string? Problem) Read(string corpusRoot, string reference, Schema schema,
        CorpusDescriptor descriptor)
    {
        var unresolved = $"validate: --since names '{reference}', and git cannot resolve it to a commit here. Check "
                         + "the ref, then fetch the full history, with `git fetch --unshallow` or `fetch-depth: 0` "
                         + "on the checkout.";

        // A ref opening with a hyphen would reach git as an option.
        if (reference.Length == 0 || reference.StartsWith('-')) return (null, unresolved);

        if (Git.Run(corpusRoot, "rev-parse", "--verify", "--quiet", reference + "^{commit}")?.Trim() is not
            { Length: > 0 } commit)
            return (null, unresolved);

        // Both list paths relative to the corpus root, and only those beneath it.
        var listed = Git.Attempt(corpusRoot, ["ls-tree", "-r", "--name-only", "-z", commit]);
        var changed = Git.Attempt(corpusRoot, ["diff", "--name-only", "--relative", "-z", commit, "--"]);
        if (listed is not { Ok: true } || changed is not { Ok: true })
            return (null, $"validate: git could not list the files at '{reference}', and said: "
                          + (listed is { Ok: false } ? listed.Error : changed?.Error ?? "nothing").Trim());

        var differ = Split(changed.Output).ToHashSet(StringComparer.Ordinal);
        var tree = new Tree(Split(listed.Output).Where(differ.Contains).ToHashSet(StringComparer.Ordinal),
            rel => (Git.Run(corpusRoot, "show", $"{commit}:./{rel}") ?? "").Replace("\r\n", "\n"));

        return (new Baseline(reference, Corpus.Load(tree, schema, descriptor).Docs), null);
    }

    private static IEnumerable<string> Split(string listing) =>
        listing.Split('\0', StringSplitOptions.RemoveEmptyEntries);
}
