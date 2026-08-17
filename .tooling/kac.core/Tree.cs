// ---------------------------------------------------------------------------
// The corpus as a listing
// ---------------------------------------------------------------------------

namespace kac.core;

// Every path the corpus holds, and a way to read one.
//
// The listing comes from git — see `Corpus.AllFiles`. A file the repository ignores is therefore not in
// the corpus, so these questions answer the same in a fresh clone as on the machine that created the
// file. Ask the filesystem instead and a check passes for whoever wrote it and fails in CI, by which
// time they have stopped looking.
//
// Reading is a `Func` rather than a root path, so the whole of it is decidable from values as
// `Mechanism.Classify` and `MechanismSync.Plan` already are. A test builds one over a dictionary; the
// tool builds one over the working tree. Every pass that reads the corpus comes through here, which is
// what lets a check be written against a corpus nobody wrote to disk.
public sealed class Tree(IReadOnlySet<string> paths, Func<string, string> read, Func<string, bool>? onDisk = null)
{
    // Whether the corpus holds this file.
    public bool Exists(string rel) => paths.Contains(Normalise(rel));

    // Whether the file is present, tracked or not — the question to ask about a file whose absence and
    // whose untracked state are separate faults. A type's `_template.md` is the case: `type-setup` says
    // the type is half set up where there is no template, and a template sitting untracked in the folder
    // is a document its author has yet to add rather than one they have yet to write.
    //
    // Everywhere else the listing is the right question, because what git does not track is in no clone.
    //
    // A corpus assembled from values supplies no answer and takes the listing's: everything it holds is
    // present, and nothing else is.
    public bool OnDisk(string rel) => onDisk is null ? Exists(rel) : onDisk(Normalise(rel));

    // Whether the corpus holds anything inside this folder. An empty directory is not a folder the corpus
    // has: git cannot track one, so counting it would make the answer depend on who is asking.
    public bool HasFolder(string folder)
    {
        var prefix = Normalise(folder) + "/";
        return paths.Any(p => p.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    // Every path the corpus holds that matches a glob, in ordinal order. The pattern is the manifest's —
    // see `Glob` — so `knowledge-as-code/*.md` names the markdown directly inside that folder, and
    // `**/*.md` names it at any depth.
    //
    // Ordered here, because a caller reporting against several of the files it names should name them in
    // the same order twice.
    public IEnumerable<string> Match(string pattern) =>
        paths.Where(p => Glob.IsMatch(p, pattern)).Order(StringComparer.Ordinal);

    // What the file holds, with line endings already normalised. It reads whatever is there; whether the
    // file is there at all is `Exists` or `OnDisk`, and which of those to ask is the caller's question.
    public string Read(string rel) => read(Normalise(rel));

    private static string Normalise(string path) => path.Replace('\\', '/');
}
