namespace kac.core;

// Where `new` and `update` read a template from.
//
// The template is fetched rather than carried inside the package, so the tool and the framework version
// apart. `docs/cli/new.md` argues that, and argues the clone over an HTTP fetch: an Azure DevOps
// repository answers a raw GET with a sign-in redirect, where `git clone` uses the credential helper the
// person already has. The clone hands back a commit for free, and git is already required, because a
// corpus is a repository.
public sealed class TemplateSource : IDisposable
{
    // The folder holding `manifest.yaml`. Every path the manifest names is read from here, so a fork
    // keeping its manifest below the repository root says so with `--path` and nothing else moves.
    public required string Root { get; init; }

    // What the ref resolved to, or null where the template was read from a folder. A folder has no ref to
    // follow, so the descriptor records the take without a commit rather than with one that was guessed.
    public string? Commit { get; init; }

    // The clone to remove afterwards, or null where nothing was cloned. It sits above `Root` whenever
    // `--path` named a subfolder, so the two are carried apart.
    private string? Clone { get; init; }

    // A temporary clone is worth nothing once the corpus is written, and a folder somebody passed is not
    // the tool's to delete.
    public void Dispose()
    {
        if (Clone is not null) Discard(Clone);
    }

    // A removal that fails is left alone. Git leaves read-only files behind on Windows, and a temporary
    // folder outliving a run is not a reason to fail a run that worked.
    private static void Discard(string dir)
    {
        try
        {
            Directory.Delete(dir, recursive: true);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    // What a fetch came to: the template, or why there is none. Exactly one of the two is set.
    public sealed record Fetch(TemplateSource? Source, string? Problem);

    // Read the template `from` names, which is either a folder on this machine or a repository to clone.
    //
    // A folder is used where it sits, and is the offline escape hatch as well as what the tool's own
    // tests read. Everything else is cloned shallow at `gitRef`, into a folder of its own under `into`.
    //
    // `prompt` says whether git may ask for a credential. False where nobody is watching, so a clone that
    // needs a password fails rather than waiting for one nobody can type. `verb` opens each problem, so a
    // reader meets the message as the tail of the command they ran.
    public static Fetch Read(string verb, string from, string? gitRef, string? path, string into, bool prompt)
    {
        if (Directory.Exists(from))
            return new Fetch(new TemplateSource { Root = Below(from, path) }, null);

        var clone = Path.Combine(into, "kac-template-" + Guid.NewGuid().ToString("n")[..12]);
        Directory.CreateDirectory(clone);

        // Quoted, because a path is as likely to hold a space as a URL is to hold none. `--` closes the
        // option list, so a repository whose name opens on a hyphen is read as a repository.
        var branch = gitRef is { Length: > 0 } r ? $"--branch \"{r}\" " : "";
        var environment = prompt ? null : new Dictionary<string, string> { ["GIT_TERMINAL_PROMPT"] = "0" };
        var run = Attempt($"clone --depth 1 {branch}-- \"{from}\" \"{clone}\"", environment);

        if (run is null)
        {
            Discard(clone);
            return new Fetch(null, $"{verb}: could not run git. the template is cloned rather than carried, "
                                   + "and a corpus is a git repository, so git has to be on the path.");
        }

        if (!run.Ok)
        {
            Discard(clone);
            var at = gitRef is { Length: > 0 } named ? $" at '{named}'" : "";
            return new Fetch(null, $"{verb}: could not clone {from}{at}. git said:\n{Indent(run.Error)}");
        }

        var root = Below(clone, path);
        if (!Directory.Exists(root))
        {
            Discard(clone);
            return new Fetch(null, $"{verb}: {from} holds no '{path}' folder, so there is no manifest to "
                                   + "read the template from.");
        }

        return new Fetch(
            new TemplateSource { Root = root, Commit = Git.Head(clone), Clone = clone },
            null);
    }

    // A template read whole: the manifest it carries, the schema it serves, and the types that schema
    // declares. `Problem` is set instead where any of that could not be read, which is the contract
    // `Fetch` states for its own pair.
    //
    // Disposable, because the fetch beneath it may have cloned. A take that reported a problem holds
    // nothing to remove, so disposing one costs nothing.
    public sealed class Taken(TemplateSource? source, Manifest? manifest, Schema? schema,
        IReadOnlyList<string>? declared, string? problem) : IDisposable
    {
        public string? Problem => problem;
        public string Root => source!.Root;
        public string? Commit => source!.Commit;
        public Manifest Manifest => manifest!;
        public Schema Schema => schema!;
        public IReadOnlyList<string> Declared => declared!;
        public IReadOnlySet<string> Files() => source!.Files();

        public void Dispose() => source?.Dispose();
    }

    // The template, and everything both verbs then ask of it: that it carries a manifest, that the
    // manifest admits this version of the tool, and what schema it serves.
    //
    // `from` is fetched and `named` is printed. The two differ for `update`, where a relative
    // `upstream.url` is resolved against the corpus root before the fetch, and the corpus is answered in
    // the terms it wrote rather than in the path they resolved to.
    public static Taken Take(string verb, string from, string named, string? gitRef, string? path,
        string into, bool prompt, string toolVersion)
    {
        var read = Read(verb, from, gitRef, path, into, prompt);
        if (read.Problem is { } unreachable) return new Taken(null, null, null, null, unreachable);

        var source = read.Source!;

        var manifestFile = Path.Combine(source.Root, Manifest.FileName);
        if (!File.Exists(manifestFile))
        {
            source.Dispose();
            return new Taken(null, null, null, null,
                $"{verb}: {named} holds no {Manifest.FileName}, so there is no template to read. "
                + "--path names the folder holding it, where it is not at the root.");
        }

        var held = Manifest.LoadFrom(manifestFile);
        if (New.TooOldFor(held.MinimumTool, toolVersion, verb) is { } tooOld)
        {
            source.Dispose();
            return new Taken(null, null, null, null, tooOld);
        }

        // The schema the template serves, which is the one account of what types there are to adopt.
        var serves = Schema.Load(Schema.FindRoot(source.Root) ?? source.Root);
        return new Taken(source, held, serves,
            [.. serves.ByFolder.Keys.OrderBy(k => k, StringComparer.Ordinal)], null);
    }

    // Every file the template holds, relative to `Root` and forward-slashed.
    //
    // Read through git where the template is a repository, so what a clone ignores the manifest never
    // sees: a folder passed with `--from` is somebody's working tree, and `bin/`, `obj/` and `.dist/` are
    // in it. `Walk` is the fallback for a folder that is not a repository, which is what a fixture is.
    public IReadOnlySet<string> Files() =>
        (GitFiles.Tracked(Root) ?? GitFiles.Walk(Root, "*", ".git")).ToHashSet(StringComparer.Ordinal);

    private static string Below(string root, string? path) =>
        path is { Length: > 0 } ? Path.Combine(root, path.Replace('/', Path.DirectorySeparatorChar)) : root;

    // Run the clone from a directory that exists whatever the arguments say. The command names both ends
    // itself, so the working directory decides nothing.
    private static GitRun? Attempt(string args, IReadOnlyDictionary<string, string>? environment) =>
        Git.Attempt(Path.GetTempPath(), args, environment);

    // git's own account of the failure, set in from the message carrying it. Its first line is usually
    // the whole answer, and the rest is a hint worth keeping.
    private static string Indent(string error) =>
        string.Join('\n', error.TrimEnd().Split('\n').Select(l => "  " + l.TrimEnd()));
}
