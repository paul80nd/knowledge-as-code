namespace kac.core;

/// <summary>The corpus's records as they stood at the revision <c>validate --since</c> names.</summary>
/// <param name="Ref">The revision as the caller spelled it, for a message to name.</param>
/// <param name="Docs">Only the records whose file differs between that revision and the working tree.</param>
/// <param name="Moved">
/// Each file git reports as renamed since that revision, from its path then to its path now. A file git cannot see,
/// such as one never staged, is missing from it.
/// </param>
public sealed record Baseline(string Ref, IReadOnlyList<Doc> Docs, IReadOnlyDictionary<string, string> Moved)
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
        var changed = Git.Attempt(corpusRoot,
            ["diff", "--name-status", "--find-renames", "--relative", "-z", commit, "--"]);
        if (listed is not { Ok: true } || changed is not { Ok: true })
            return (null, $"validate: git could not list the files at '{reference}', and said: "
                          + (listed is { Ok: false } ? listed.Error : changed?.Error ?? "nothing").Trim());

        var (differ, moved) = Statuses(changed.Output);
        var tree = new Tree(Split(listed.Output).Where(differ.Contains).ToHashSet(StringComparer.Ordinal),
            rel => (Git.Run(corpusRoot, "show", $"{commit}:./{rel}") ?? "").Replace("\r\n", "\n"));

        return (new Baseline(reference, Corpus.Load(tree, schema, descriptor).Docs, moved), null);
    }

    /// <summary>
    /// Every path a <c>--name-status -z</c> listing names, and each rename in it. A rename's or a copy's entry is its
    /// status, then the path at the revision, then the path now. Every other entry is its status, then one path. A
    /// copy leaves its source in place, so it is no move.
    /// </summary>
    private static (HashSet<string> Paths, Dictionary<string, string> Moved) Statuses(string listing)
    {
        var paths = new HashSet<string>(StringComparer.Ordinal);
        var moved = new Dictionary<string, string>(StringComparer.Ordinal);
        var fields = Split(listing).ToList();

        for (var i = 0; i + 1 < fields.Count; i += 2)
        {
            var path = fields[i + 1];
            paths.Add(path);
            if (fields[i][0] is not ('R' or 'C') || i + 2 >= fields.Count) continue;

            if (fields[i][0] == 'R') moved[path] = fields[i + 2];
            paths.Add(fields[i + 2]);
            i++;
        }

        return (paths, moved);
    }

    private static IEnumerable<string> Split(string listing) =>
        listing.Split('\0', StringSplitOptions.RemoveEmptyEntries);
}
