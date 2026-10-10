using System.Diagnostics.CodeAnalysis;

namespace kac.core;

// Where `new` and `update` read a template from. The template is cloned with git, because an Azure DevOps repository
// answers a raw GET with a sign-in redirect. `docs/cli/new.md` says why.
public sealed class TemplateSource : IDisposable
{
    // The folder holding `manifest.yaml`, which every path the manifest names is read from.
    public required string Root { get; init; }

    // What the ref resolved to, or null where the template was read from a folder.
    public string? Commit { get; init; }

    // The clone to remove afterwards, or null where nothing was cloned. It sits above `Root` whenever `--path` named a
    // subfolder.
    private string? Clone { get; init; }

    // A folder somebody passed is not the tool's to delete.
    public void Dispose()
    {
        if (Clone is not null) Discard(Clone);
    }

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
    public sealed record Fetch(TemplateSource? Source, string? Problem)
    {
        // `Source` is null exactly where `Problem` is set, told to the compiler so a caller that
        // branches on it reads the other half without asserting anything.
        [MemberNotNullWhen(true, nameof(Problem))]
        [MemberNotNullWhen(false, nameof(Source))]
        public bool Failed => Problem is not null;
    }

    // Read the template `from` names: a folder, used where it sits, or a repository cloned shallow at `gitRef` under
    // `into`. `prompt` says whether git may ask for a credential, so an unattended clone fails rather than waits.
    // `verb` opens each problem.
    public static Fetch Read(string verb, string from, string? gitRef, string? path, string into, bool prompt)
    {
        if (Directory.Exists(from))
            return new Fetch(new TemplateSource { Root = Below(from, path) }, null);

        var clone = Path.Combine(into, "kac-template-" + Guid.NewGuid().ToString("n")[..12]);
        Directory.CreateDirectory(clone);

        // Quoted, because a path may hold a space. `--` closes the option list, so a repository whose name opens on a
        // hyphen is read as one.
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

    // A template read whole: its manifest, its schema, and the types the schema declares. `Problem` is set instead
    // where any of that could not be read.
    public sealed class Taken(
        TemplateSource? source,
        Manifest? manifest,
        Schema? schema,
        IReadOnlyList<string>? declared,
        string? problem) : IDisposable
    {
        // Each value below is set exactly where `Problem` is not. Reaching one on a failed take is a defect in the
        // caller.
        private static T Held<T>(T? value, string what) where T : class =>
            value ?? throw new InvalidOperationException(
                $"this take reported a problem and holds no {what}. read Problem first.");

        public string? Problem => problem;
        public string Root => Held(source, "template").Root;
        public string? Commit => Held(source, "template").Commit;
        public Manifest Manifest => Held(manifest, "manifest");
        public Schema Schema => Held(schema, "schema");
        public IReadOnlyList<string> Declared => Held(declared, "list of declared types");
        public IReadOnlySet<string> Files() => Held(source, "template").Files();

        public void Dispose() => source?.Dispose();
    }

    // The template, checked to carry a manifest that admits this version of the tool, with the schema it serves.
    // `from` is fetched and `named` is printed: for `update`, a relative `upstream.url` is resolved before the fetch
    // and reported as written.
    public static Taken Take(string verb, string from, string named, string? gitRef, string? path,
        string into, bool prompt, string toolVersion)
    {
        var read = Read(verb, from, gitRef, path, into, prompt);
        if (read.Failed) return new Taken(null, null, null, null, read.Problem);

        var source = read.Source;

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

        var serves = Schema.LoadNearest(source.Root);
        return new Taken(source, held, serves,
            [.. serves.ByFolder.Keys.OrderBy(k => k, StringComparer.Ordinal)], null);
    }

    // Every file the template holds, relative to `Root` and forward-slashed. Read through git where the template is a
    // repository, so what a clone ignores never reaches the manifest. `Walk` is the fallback for a folder that is not
    // a repository.
    public IReadOnlySet<string> Files() =>
        (GitFiles.Tracked(Root) ?? GitFiles.Walk(Root, "*", ".git")).ToHashSet(StringComparer.Ordinal);

    private static string Below(string root, string? path) =>
        path is { Length: > 0 } ? Path.Combine(root, path.Replace('/', Path.DirectorySeparatorChar)) : root;

    // The command names both ends itself, so the working directory decides nothing.
    private static GitRun? Attempt(string args, IReadOnlyDictionary<string, string>? environment) =>
        Git.Attempt(Path.GetTempPath(), args, environment);

    // git's own account of the failure, indented under the message carrying it.
    private static string Indent(string error) =>
        string.Join('\n', error.TrimEnd().Split('\n').Select(l => "  " + l.TrimEnd()));
}
