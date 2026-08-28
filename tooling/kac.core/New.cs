using System.Text;

namespace kac.core;

// The `new` engine: what standing a corpus up in an empty folder comes to.
//
// Deciding and doing stay apart, as they do either side of `GeneratedFiles`. `Plan` names every file a
// creation writes, composes the two no template can supply, and touches nothing. `Apply` carries that
// plan out. So a creation is decidable from a listing and a manifest, with no template on disk and no
// network, which is what lets the unit tests ask about a corpus nobody wrote.
//
// `docs/cli/new.md` is the reference for the order the command runs in and for what each answer means.

// One file a creation copies: where the template holds it, where it lands, and the layer that sent it.
public sealed record PlannedFile(string From, string To, string Layer);

// One file a creation writes rather than copies. A descriptor cannot be copied without carrying somebody
// else's name in it, and the template's own `README.md` describes the template.
public sealed record ComposedFile(string Path, string Content);

// Where the framework came from, as the descriptor records it.
//
// `Url` is what `--from` was given, written back unresolved: a corpus takes from wherever it was told,
// and an absolute path resolved here would be a fact about the machine that ran the command. `Ref` and
// `Commit` are null where the template was read from a folder, which has no ref to follow and no commit
// to resolve.
public sealed record Upstream(
    string Url,
    string? Path,
    string? Ref,
    string? Commit,
    int TemplateVersion,
    string TakenOn);

// The continuous integration systems a corpus can be created for, which is what `--ci` is held to and
// what a template's `ci:` may name. Read from here by both, so a system the tool cannot offer is refused
// at the flag and reported in the template, rather than quietly taking a starter nobody can run.
public static class CiSystem
{
    public const string GitHub = "github";
    public const string AzureDevOps = "azure-devops";
    public const string None = "none";

    public static readonly IReadOnlyList<string> All = [GitHub, AzureDevOps, None];
}

// What the invocation was told, however it was told: a flag, a prompt, or a default.
public sealed record NewAnswers
{
    // What the corpus calls itself, defaulting to the name of the folder it is created in.
    public required string Name { get; init; }

    // The types the corpus adopts, named as the schema names them. Always written out, even where the
    // answer was every type the template declares. A corpus created by `new` has made the decision, and
    // the descriptor records it so that validation can hold the corpus to it.
    public required IReadOnlyList<string> Types { get; init; }

    // How the corpus is published, and where the published form is served from. `Publishing.Targets`
    // names every target a descriptor may state. The base is null where the corpus publishes nowhere,
    // which is the one target needing none.
    public string PublishingTarget { get; init; } = Publishing.None;
    public string? Base { get; init; }

    // Which continuous integration system the corpus is built by. Asked apart from publishing, because a
    // corpus can be built by one system and read on another.
    public string Ci { get; init; } = CiSystem.None;
}

// What `new` found in the folder before it asked anything.
//
// Everything that can stop a run is read here, so that nobody answers six questions and is then told the
// folder was not empty. What each state comes to is the command's to decide: two of them are a refusal,
// and two are a question.
public sealed record Ground
{
    // The corpus this folder already sits in, or null. A descriptor at or above the working directory
    // means the framework is here already, and taking a newer one into it is `update`.
    public string? Corpus { get; init; }

    // Whether git reads this folder as a repository, and whether that repository holds changes its last
    // commit does not. `Dirty` is null where there is no repository to ask, or where git could not
    // answer, and a tree nobody can ask about is not the same as a clean one.
    public bool Repository { get; init; }
    public bool? Dirty { get; init; }

    // What the folder already holds, `.git` aside. Named rather than counted: the warning is worth
    // reading only where it says what a creation is about to be mixed in with.
    public IReadOnlyList<string> Holds { get; init; } = [];
}

// What creating a corpus comes to. Every list names paths in the corpus about to exist, except `Copied`,
// which also carries where each file was read from.
public sealed record NewPlan(
    IReadOnlyList<PlannedFile> Copied,
    IReadOnlyList<ComposedFile> Composed,
    IReadOnlyList<string> DeclinedTypes,
    IReadOnlyList<string> DeclinedCi,
    IReadOnlyList<string> Unclassified,
    IReadOnlyList<string> UnknownCi)
{
    // Every path the creation writes, in the order a listing reads them. What a golden snapshots, and
    // what a caller reports.
    public IEnumerable<string> Paths =>
        Copied.Select(f => f.To).Concat(Composed.Select(f => f.Path)).OrderBy(p => p, StringComparer.Ordinal);

    // A template this tool cannot read the whole of: a file its own manifest cannot place, or a rule
    // serving a continuous integration system the tool does not offer. The creation stops rather than
    // guessing, because each of those is a defect upstream, and acting anyway means a corpus receives a
    // file nobody meant to send or loses one nobody meant to withhold.
    public bool TemplateIsUnsound => Unclassified.Count > 0 || UnknownCi.Count > 0;
}

public static class New
{
    private const string DescriptorFile = ".corpus.yaml";
    private const string ReadmeFile = "README.md";

    // The version a corpus starts on. Semantically versioned and moved by hand, so the tool supplies a
    // first number and never a later one.
    private const string FirstContentVersion = "0.1.0";

    // Read the folder `new` was run in, before anything is asked and before anything is written.
    public static Ground Survey(string dir)
    {
        var repository = Git.Run(dir, "rev-parse --is-inside-work-tree")?.Trim() == "true";

        return new Ground
        {
            Corpus = Corpus.FindRoot(dir),
            Repository = repository,
            Dirty = repository ? Git.Dirty(dir) : null,
            Holds = Holdings(dir)
        };
    }

    // What a folder holds, `.git` aside, or nothing at all where the folder is not there yet.
    private static IReadOnlyList<string> Holdings(string dir)
    {
        if (!Directory.Exists(dir)) return [];

        return
        [
            .. Directory.EnumerateFileSystemEntries(dir)
                .Select(Path.GetFileName)
                .OfType<string>()
                .Where(name => name != ".git")
                .OrderBy(name => name, StringComparer.Ordinal)
        ];
    }

    // Why this tool cannot read this template, or null where it can.
    //
    // The template is fetched rather than shipped inside the package, so the two version independently. A
    // tool meeting a manifest it is too old for stops here rather than half-reading it: every key it does
    // not know is one it would ignore in silence, and a rule it ignores is a file a corpus loses.
    //
    // `tool` is the running version, which the entry point reads off its own assembly. Build metadata is
    // dropped from it, so `0.6.0+abc123` and `0.6.0` are one version. A version neither side can parse
    // does not stop a run: the tool's own is a build stamp, and refusing over it would ground a tool that
    // works. `verb` opens the message, because `new` and `update` both read a template.
    public static string? TooOldFor(string? minimum, string tool, string verb)
    {
        if (minimum is not { Length: > 0 }) return null;

        if (!Version.TryParse(Release(minimum), out var wanted))
            return $"{verb}: the template declares minimum-tool '{minimum}', which is not a version.";

        if (!Version.TryParse(Release(tool), out var running)) return null;

        return running >= wanted
            ? null
            : $"{verb}: this template needs kac {minimum} or newer, and this is {tool}. "
              + "update the tool, or name an older ref with --ref.";

        // The release the version names, without the build metadata or the pre-release tag beside it.
        static string Release(string version) => version.Split('+', '-')[0];
    }

    // What creating a corpus from this template comes to.
    //
    // `manifest` is the template's, because the boundary arrives with the files it describes. `declines`
    // answers whether a destination belongs to a type the corpus did not adopt; `DeclinesTypes` builds
    // one from the template's schema.
    public static NewPlan Plan(IReadOnlySet<string> templateFiles, Manifest manifest, NewAnswers answers,
        Upstream upstream, Func<string, bool> declines)
    {
        var copied = new List<PlannedFile>();
        var declinedTypes = new List<string>();
        var declinedCi = new List<string>();
        var unclassified = new List<string>();
        var unknownCi = new List<string>();

        foreach (var from in templateFiles.OrderBy(f => f, StringComparer.Ordinal))
        {
            if (manifest.Place(from) is not { } placement)
            {
                unclassified.Add(from);
                continue;
            }

            // `withheld` is the template's own machinery and reaches no corpus. `removed` is a tombstone
            // for a file an update deletes, so writing one into a corpus being created would hand it a
            // file the first update takes straight back.
            if (placement.Layer is Manifest.Withheld or Manifest.Removed) continue;

            if (declines(placement.Path))
            {
                declinedTypes.Add(placement.Path);
                continue;
            }

            // A starter for a system this corpus does not build on. Not written rather than written and
            // deleted unread, and a GitHub workflow is the one that would otherwise run uninvited.
            if (placement.Ci is { } ci)
            {
                if (!CiSystem.All.Contains(ci, StringComparer.Ordinal))
                {
                    if (!unknownCi.Contains(ci, StringComparer.Ordinal)) unknownCi.Add(ci);
                    continue;
                }

                if (!ci.Equals(answers.Ci, StringComparison.Ordinal))
                {
                    declinedCi.Add(placement.Path);
                    continue;
                }
            }

            // A template that sends its own descriptor sends its own name, publishing bases and adopted
            // types with it. Composed always wins here, where a `README.md` the template sends does not.
            if (placement.Path == DescriptorFile) continue;

            copied.Add(new PlannedFile(from, placement.Path, placement.Layer));
        }

        var composed = new List<ComposedFile> { new(DescriptorFile, Descriptor(answers, upstream)) };

        // The template's own `README.md` is withheld, so a corpus taking everything would arrive with
        // none. A template that does send one has written it for a corpus to keep, and that copy wins.
        if (copied.All(f => f.To != ReadmeFile))
            composed.Add(new ComposedFile(ReadmeFile, Readme(answers)));

        copied.Sort((a, b) => string.CompareOrdinal(a.To, b.To));
        declinedTypes.Sort(StringComparer.Ordinal);
        declinedCi.Sort(StringComparer.Ordinal);
        unknownCi.Sort(StringComparer.Ordinal);
        return new NewPlan(copied, composed, declinedTypes, declinedCi, unclassified, unknownCi);
    }

    // Whether a destination belongs to a type the corpus declined: the type's schema file, its root page,
    // and everything in its folder.
    //
    // Judged on where a file lands rather than on where the template holds it. A template serving its
    // pages from a subfolder places them at a corpus's root, so the source path says nothing about which
    // type a file belongs to.
    public static Func<string, bool> DeclinesTypes(Schema schema, IReadOnlyList<string> adopted)
    {
        var files = new HashSet<string>(StringComparer.Ordinal);
        var folders = new List<string>();

        foreach (var (name, type) in schema.ByFolder)
        {
            if (adopted.Contains(name, StringComparer.Ordinal)) continue;
            files.Add($".schema/{name}.yaml");
            if (!string.IsNullOrEmpty(type.Page)) files.Add(type.Page);
            folders.Add((string.IsNullOrEmpty(type.Folder) ? name : type.Folder) + "/");
        }

        return rel => files.Contains(rel) || folders.Any(f => rel.StartsWith(f, StringComparison.Ordinal));
    }

    // Carry the plan out, and answer with every path written.
    //
    // The plan decided all of it, so this asks the template nothing beyond the bytes and the mode of each
    // file it was told to copy.
    public static IReadOnlyList<string> Apply(NewPlan plan, string templateRoot, string corpusRoot)
    {
        foreach (var file in plan.Copied)
            Files.Copy(Path.Combine(templateRoot, file.From), Path.Combine(corpusRoot, file.To));

        foreach (var file in plan.Composed)
        {
            var target = Path.Combine(corpusRoot, file.Path);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.WriteAllText(target, file.Content);
        }

        return [.. plan.Paths];
    }

    // `.corpus.yaml` as a corpus receives it: what the invocation was told, and the block saying where the
    // framework came from.
    //
    // Written rather than copied, because a copied descriptor carries somebody else's name, publishing
    // bases and adopted types. Each key takes a line of comment and the reference carries the rest, so a
    // corpus that has to change one has somewhere to read before it does.
    public static string Descriptor(NewAnswers answers, Upstream upstream)
    {
        var sb = new StringBuilder();

        sb.Append("# What this corpus is, and where it stands against the framework it took.\n");
        sb.Append("#\n");
        sb.Append("# https://paul80nd.github.io/knowledge-as-code/corpus-descriptor/ says what every key"
                  + " here means.\n\n");

        sb.Append("# The format of this file, which the tool owns rather than the corpus.\n");
        sb.Append($"descriptor-version: {CorpusDescriptor.Format}\n\n");

        sb.Append("# What this corpus calls itself, and the version of what it knows. Move"
                  + " `content-version` by hand.\n");
        sb.Append($"corpus: {Scalar(answers.Name)}\n");
        sb.Append($"content-version: \"{FirstContentVersion}\"\n\n");

        // Written bare, and never filled in here. A shortcode is what another corpus cites this one by,
        // and it cannot be changed once anything has, so it is declared when something is about to cite
        // this corpus rather than invented at creation.
        sb.Append("# The shorthand another corpus cites this one by. Fill it in when one does, and never"
                  + " change it after.\n");
        sb.Append("shortcode:\n\n");

        // Written bare for the same reason `shortcode` is: each is a claim about somebody, and a value
        // supplied here would be inherited rather than chosen. A plugin built from a corpus that has
        // stated none carries none, so the corpus asserts nothing it did not say.
        sb.Append("# What this corpus is, for a reader who meets it as a package or an installed plugin\n");
        sb.Append("# rather than as a repository. Each is carried into both, and a key left bare is"
                  + " carried into neither.\n");
        sb.Append("display-name:\n");
        sb.Append("description:\n");
        sb.Append("license:\n");
        sb.Append("author:\n");
        sb.Append("  name:\n");
        sb.Append("  url:\n\n");

        sb.Append("# How this corpus is published. One of: "
                  + $"{string.Join(" | ", Publishing.Targets)}.\n");
        sb.Append($"publishing-target: {answers.PublishingTarget}\n");
        if (answers.Base is { Length: > 0 } published)
        {
            sb.Append("\n# Where the published corpus is browsed. An agent reads a record's source from"
                      + " the same place, through a client that authenticates.\n");
            sb.Append("publishing:\n");
            sb.Append($"  base: {Scalar(published)}\n");
        }

        sb.Append("\n# Where this corpus takes the framework from, and what the last take resolved to.\n");
        sb.Append("upstream:\n");
        sb.Append($"  url: {Scalar(upstream.Url)}\n");
        sb.Append(Optional("path", upstream.Path));
        sb.Append(Optional("ref", upstream.Ref));
        sb.Append(Optional("commit", upstream.Commit));
        sb.Append($"  template-version: {upstream.TemplateVersion}\n");
        sb.Append($"  taken-on: \"{upstream.TakenOn}\"\n\n");

        sb.Append("# How far an update goes. `cautious` writes a seed only where this corpus has none.\n");
        sb.Append($"update-policy: {CorpusDescriptor.Cautious}\n\n");

        sb.Append("# The types this corpus adopted. Validation and index generation cover these and no"
                  + " others.\n");
        sb.Append("types:\n");
        foreach (var type in answers.Types) sb.Append($"  - {Scalar(type)}\n");

        sb.Append("\n# What `kac export` leaves behind. Empty means a consumer reads what this corpus"
                  + " actually holds.\n");
        sb.Append("export:\n");
        sb.Append("  exclude: []\n\n");

        sb.Append("# Files this corpus holds differently from the framework, each with the reason it"
                  + " does.\n");
        sb.Append("skip: []\n");

        return sb.ToString();
    }

    // `README.md` as a corpus receives it: the corpus's name, what it holds, and how to run the tool
    // against it. A starting point rather than a document, and the corpus's own from the moment it lands.
    //
    // It arrives carrying the markers for every block `README.md` is declared to hold, and says so. That
    // file is the one a corpus may decline a block on, by deleting the markers, so a README written
    // without them would decline on every corpus's behalf without anybody choosing to.
    public static string Readme(NewAnswers answers)
    {
        var sb = new StringBuilder();

        sb.Append($"# {answers.Name}\n\n");
        sb.Append($"{answers.Name} is a knowledge corpus: plain markdown in git, where every document has"
                  + " a type and\nevery type has a schema.\n\n");
        sb.Append("Rewrite this page to say what your corpus is for.\n");

        // Asked of the declaration rather than named here, so a second block on this file arrives with its
        // markers and needs no change. `GeneratedFiles` is the one account of what `generate` writes.
        var blocks = GeneratedFiles.Blocks([]).Where(f => f.Path == ReadmeFile).SelectMany(f => f.Blocks);
        foreach (var block in blocks)
        {
            sb.Append($"\n## The knowledge types\n\n");
            sb.Append($"`kac generate` writes the table below. Delete both markers to decline it.\n\n");
            sb.Append($"{Generator.Begin(block)}\n{Generator.End(block)}\n");
        }

        sb.Append("\n## Working here\n\n");
        sb.Append("Run `kac validate` to check this corpus against `.schema/`. Run `kac generate` to"
                  + " rebuild the indexes\nand the blocks above.\n\n");
        sb.Append("[`knowledge-as-code.md`](knowledge-as-code.md) says why the framework is built this"
                  + " way.\n[`CLAUDE.md`](CLAUDE.md) is what an agent working here reads.\n");

        return sb.ToString();
    }

    // A key inside the upstream block whose value the take could not answer. Written bare, which YAML
    // reads as absent, so the key is there to fill in rather than there holding an empty string.
    private static string Optional(string key, string? value) =>
        value is { Length: > 0 } ? $"  {key}: {Scalar(value)}\n" : $"  {key}:\n";

    // The words a YAML reader takes for a boolean or for nothing at all, rather than for a name. Held
    // wider than the 1.2 core schema, which reads only the first pair: a folder called `no` or `off` is a
    // corpus whose name loads as `false` under a reader on 1.1, and quoting costs the name nothing.
    private static readonly string[] Reserved =
        ["true", "false", "y", "n", "yes", "no", "on", "off", "null", "~"];

    private static string Scalar(string value)
    {
        var plain = value.Length > 0
                    && !value.StartsWith(' ')
                    && !value.EndsWith(' ')
                    && !"-?:,[]{}#&*!|>'\"%@`".Contains(value[0])
                    && !value.Contains(": ", StringComparison.Ordinal)
                    && !value.Contains(" #", StringComparison.Ordinal)
                    && !Reserved.Contains(value, StringComparer.OrdinalIgnoreCase)
                    && !double.TryParse(value, out _);

        return plain ? value : "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }
}
