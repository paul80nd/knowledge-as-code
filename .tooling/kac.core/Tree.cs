// ---------------------------------------------------------------------------
// The corpus as a listing
// ---------------------------------------------------------------------------

namespace kac.core;

// Every path the corpus holds, and a way to read one.
//
// The listing is git's — see `Corpus.AllFiles` — so a file the repository ignores is not part of the
// corpus, and every question below is answered the same in a fresh clone as on the machine that happened
// to create the file. Asking the filesystem instead makes a check pass locally and fail in CI, which is
// the one failure a corpus tool cannot afford: it arrives after the author has stopped looking.
//
// Reading is a `Func` rather than a root path so the whole of it is decidable from values, as
// `Mechanism.Classify` and `MechanismSync.Plan` already are: a test builds one over a dictionary where
// the tool builds one over the working tree.
public sealed class Tree(IReadOnlySet<string> paths, Func<string, string> read)
{
    // Whether the corpus holds this file.
    public bool Exists(string rel) => paths.Contains(Normalise(rel));

    // Whether the corpus holds anything inside this folder. An empty directory is not a folder the corpus
    // has: git cannot track one, so counting it would make the answer depend on who is asking.
    public bool HasFolder(string folder)
    {
        var prefix = Normalise(folder) + "/";
        return paths.Any(p => p.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    // What the file holds, with line endings already normalised. Only ever called for a path the caller
    // has found in the listing.
    public string Read(string rel) => read(Normalise(rel));

    private static string Normalise(string path) => path.Replace('\\', '/');
}
