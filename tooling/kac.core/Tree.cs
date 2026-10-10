namespace kac.core;

// Every path the corpus holds, and a way to read one. The listing comes from git (see `Corpus.AllFiles`), so a check
// answers the same in a fresh clone as on the machine that created the file. Reading is a `Func`, so a test builds a
// tree over a dictionary.
public sealed class Tree(IReadOnlySet<string> paths, Func<string, string> read, Func<string, bool>? onDisk = null)
{
    public bool Exists(string rel) => paths.Contains(Normalise(rel));

    // Whether the file is present, tracked or not. Ask this only where a file's absence and its untracked state are
    // separate faults, as with a type's `_template.md`. Ask `Exists` everywhere else. A tree built from values answers
    // from its listing.
    public bool OnDisk(string rel) => onDisk?.Invoke(Normalise(rel)) ?? Exists(rel);

    // An empty directory is not a folder the corpus has, because git cannot track one.
    public bool HasFolder(string folder)
    {
        var prefix = Normalise(folder) + "/";
        return paths.Any(p => p.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    // Every path the corpus holds that matches a glob (see `Glob`), in ordinal order, so a caller reporting against
    // several names them in the same order every time.
    public IEnumerable<string> Match(string pattern) =>
        paths.Where(p => Glob.IsMatch(p, pattern)).Order(StringComparer.Ordinal);

    // What the file holds, with line endings already normalised. Whether it is there at all is `Exists` or `OnDisk`.
    public string Read(string rel) => read(Normalise(rel));

    private static string Normalise(string path) => path.Replace('\\', '/');
}
