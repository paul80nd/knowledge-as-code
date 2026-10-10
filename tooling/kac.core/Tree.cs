namespace kac.core;

/// <summary>
/// Every path the corpus holds, and a way to read one. The listing comes from git (see <c>Corpus.AllFiles</c>), so a
/// check answers the same in a fresh clone as on the machine that created the file. Reading is a <c>Func</c>, so a test
/// builds a tree over a dictionary.
/// </summary>
public sealed class Tree(IReadOnlySet<string> paths, Func<string, string> read, Func<string, bool>? onDisk = null)
{
    public bool Exists(string rel) => paths.Contains(Normalise(rel));

    /// <summary>
    /// Whether the file is present, tracked or not. Ask this only where a file's absence and its untracked state are
    /// separate faults, as with a type's <c>_template.md</c>. Ask <c>Exists</c> everywhere else. A tree built from
    /// values answers from its listing.
    /// </summary>
    public bool OnDisk(string rel) => onDisk?.Invoke(Normalise(rel)) ?? Exists(rel);

    /// <summary>An empty directory is not a folder the corpus has, because git cannot track one.</summary>
    public bool HasFolder(string folder)
    {
        var prefix = Normalise(folder) + "/";
        return paths.Any(p => p.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Every path the corpus holds that matches a glob (see <c>Glob</c>), in ordinal order, so a caller reporting
    /// against several names them in the same order every time.
    /// </summary>
    public IEnumerable<string> Match(string pattern) =>
        paths.Where(p => Glob.IsMatch(p, pattern)).Order(StringComparer.Ordinal);

    /// <summary>
    /// What the file holds, with line endings already normalised. Whether it is there at all is <c>Exists</c> or
    /// <c>OnDisk</c>.
    /// </summary>
    public string Read(string rel) => read(Normalise(rel));

    private static string Normalise(string path) => path.Replace('\\', '/');
}
