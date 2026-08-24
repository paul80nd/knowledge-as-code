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
    // names every target a descriptor may state. Both bases are null where the corpus publishes nowhere,
    // which is the one target needing neither.
    public string PublishingTarget { get; init; } = Publishing.None;
    public string? HumanBase { get; init; }
    public string? RawBase { get; init; }
}

// What creating a corpus comes to. Every list names paths in the corpus about to exist, except `Copied`,
// which also carries where each file was read from.
public sealed record NewPlan(
    IReadOnlyList<PlannedFile> Copied,
    IReadOnlyList<ComposedFile> Composed,
    IReadOnlyList<string> Declined,
    IReadOnlyList<string> Unclassified)
{
    // Every path the creation writes, in the order a listing reads them. What a golden snapshots, and
    // what a caller reports.
    public IEnumerable<string> Paths =>
        Copied.Select(f => f.To).Concat(Composed.Select(f => f.Path)).OrderBy(p => p, StringComparer.Ordinal);

    // A template whose own manifest cannot place its own tree. The creation stops rather than guessing:
    // an unplaced file is a defect upstream, and taking it anyway is how a corpus receives a file nobody
    // meant to send.
    public bool TemplateIsUnsound => Unclassified.Count > 0;
}

public static class New
{
    private const string DescriptorFile = ".corpus.yaml";
    private const string ReadmeFile = "README.md";

    // The version a corpus starts on. Semantically versioned and moved by hand, so the tool supplies a
    // first number and never a later one.
    private const string FirstContentVersion = "0.1.0";

    // What creating a corpus from this template comes to.
    //
    // `manifest` is the template's, because the boundary arrives with the files it describes. `declines`
    // answers whether a destination belongs to a type the corpus did not adopt; `DeclinesTypes` builds
    // one from the template's schema.
    public static NewPlan Plan(IReadOnlySet<string> templateFiles, Manifest manifest, NewAnswers answers,
        Upstream upstream, Func<string, bool> declines)
    {
        var copied = new List<PlannedFile>();
        var declined = new List<string>();
        var unclassified = new List<string>();

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
                declined.Add(placement.Path);
                continue;
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
        declined.Sort(StringComparer.Ordinal);
        return new NewPlan(copied, composed, declined, unclassified);
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
        {
            var source = Path.Combine(templateRoot, file.From);
            var target = Path.Combine(corpusRoot, file.To);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(source, target, overwrite: true);
            CarryMode(source, target);
        }

        foreach (var file in plan.Composed)
        {
            var target = Path.Combine(corpusRoot, file.Path);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.WriteAllText(target, file.Content);
        }

        return [.. plan.Paths];
    }

    // The mode the template holds a file under, carried to the copy. `.plugin/hooks/breadcrumb` is
    // executable, and a hook arriving without its bit fails silently rather than reporting anything.
    //
    // Read from the source rather than named here, so a template making a second file executable needs no
    // change. Windows has no mode to read, and git there records none either.
    private static void CarryMode(string source, string target)
    {
        if (OperatingSystem.IsWindows()) return;
        File.SetUnixFileMode(target, File.GetUnixFileMode(source));
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
        sb.Append("# This file is the corpus's own: nothing syncs it and nothing reconciles it. `kac` finds"
                  + " the corpus by it.\n");
        sb.Append("# https://paul80nd.github.io/knowledge-as-code/corpus-descriptor/ says what every key"
                  + " here means.\n\n");

        sb.Append("# The format of this file, which the tool owns rather than the corpus.\n");
        sb.Append($"descriptor-version: {CorpusDescriptor.Format}\n\n");

        sb.Append("# What this corpus calls itself, and the version of what it knows. Move"
                  + " `content-version` by hand.\n");
        sb.Append($"corpus: {Scalar(answers.Name)}\n");
        sb.Append($"content-version: \"{FirstContentVersion}\"\n\n");

        sb.Append("# `consumer` takes the framework from a source, and holds none of the machinery that"
                  + " proves the tool.\n");
        sb.Append($"role: {CorpusDescriptor.Consumer}\n\n");

        sb.Append("# How this corpus is published. One of: "
                  + $"{string.Join(" | ", Publishing.Targets)}.\n");
        sb.Append($"publishing-target: {answers.PublishingTarget}\n");
        if (answers.HumanBase is { Length: > 0 } human && answers.RawBase is { Length: > 0 } raw)
        {
            sb.Append("\n# Where the published form is served from. A person follows the first, and an"
                      + " agent fetches the second.\n");
            sb.Append("publishing:\n");
            sb.Append($"  human-base: {Scalar(human)}\n");
            sb.Append($"  raw-base: {Scalar(raw)}\n");
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

    // A scalar quoted where YAML would otherwise read it as something other than a name. A plain scalar
    // cannot open on an indicator or hold `": "`, and a folder called `1.5` would load as a number.
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
