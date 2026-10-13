using System.Text;

namespace kac.core;

// `Plan` decides and touches nothing, and `Apply` carries the plan out, so a creation is decidable from
// a listing and a manifest with no template on disk. `docs/use/cli/new.md` is the reference for the order.

public sealed record PlannedFile(string From, string To, string Layer);

/// <summary>Written rather than copied: a copied descriptor carries somebody else's name.</summary>
public sealed record ComposedFile(string Path, string Content);

/// <summary>
/// <c>Url</c> is written back unresolved: an absolute path would be a fact about the machine that ran the command.
/// <c>Ref</c> and <c>Commit</c> are null where the template was read from a folder.
/// </summary>
public sealed record Upstream(
    string Url,
    string? Path,
    string? Ref,
    string? Commit,
    int TemplateVersion,
    string TakenOn);

/// <summary>Both <c>--ci</c> and a template's <c>ci:</c> are checked against this list.</summary>
public static class CiSystem
{
    public const string GitHub = "github";
    public const string AzureDevOps = "azure-devops";
    public const string None = "none";

    public static readonly IReadOnlyList<string> All = [GitHub, AzureDevOps, None];
}

public sealed record NewAnswers
{
    public required string Name { get; init; }

    /// <summary>
    /// Always written out, even where every type was adopted, so validation can hold the corpus to it.
    /// </summary>
    public required IReadOnlyList<string> Types { get; init; }

    /// <summary>The base is null where the corpus publishes nowhere.</summary>
    public string PublishingTarget { get; init; } = Publishing.None;
    public string? Base { get; init; }

    public string Ci { get; init; } = CiSystem.None;
}

/// <summary>Everything that can stop a run is read before anything is asked.</summary>
public sealed record Ground
{
    /// <summary>A descriptor at or above the working directory means this is <c>update</c>'s job.</summary>
    public string? Corpus { get; init; }

    /// <summary>
    /// <c>Dirty</c> is null where there is no repository or git could not answer, which is not the same as clean.
    /// </summary>
    public bool Repository { get; init; }
    public bool? Dirty { get; init; }

    public IReadOnlyList<string> Holds { get; init; } = [];
}

public sealed record NewPlan(
    IReadOnlyList<PlannedFile> Copied,
    IReadOnlyList<ComposedFile> Composed,
    IReadOnlyList<string> DeclinedTypes,
    IReadOnlyList<string> DeclinedCi,
    TemplateFaults Faults)
{
    /// <summary>In ordinal order, which is what a golden snapshots.</summary>
    public IEnumerable<string> Paths =>
        Copied.Select(f => f.To).Concat(Composed.Select(f => f.Path)).OrderBy(p => p, StringComparer.Ordinal);
}

public static class New
{
    private const string DescriptorFile = ".corpus.yaml";
    private const string ReadmeFile = "README.md";

    /// <summary>Moved by hand afterwards, so the tool supplies a first number and never a later one.</summary>
    private const string FirstContentVersion = "0.1.0";

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

    /// <summary>
    /// A tool meeting a manifest it is too old for stops rather than ignoring keys it does not know. Build metadata is
    /// dropped, so <c>0.6.0+abc123</c> and <c>0.6.0</c> are one version. A version neither side can parse does not stop
    /// a run.
    /// </summary>
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

        static string Release(string version) => version.Split('+', '-')[0];
    }

    /// <summary>
    /// <c>declines</c> answers whether a destination belongs to a type the corpus did not adopt. <c>DeclinesTypes</c>
    /// builds one from the template's schema.
    /// </summary>
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

            // `withheld` reaches no corpus, and a `removed` tombstone would be deleted again by the first update.
            if (placement.Layer is Manifest.Withheld or Manifest.Removed) continue;

            if (declines(placement.Path))
            {
                declinedTypes.Add(placement.Path);
                continue;
            }

            // Not written at all, so a GitHub workflow for another system never runs uninvited.
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

            // The composed descriptor always wins over one the template sends.
            if (placement.Path == DescriptorFile) continue;

            copied.Add(new PlannedFile(from, placement.Path, placement.Layer));
        }

        var composed = new List<ComposedFile> { new(DescriptorFile, Descriptor(answers, upstream)) };

        // A `README.md` the template sends wins over the composed one.
        if (copied.All(f => f.To != ReadmeFile))
            composed.Add(new ComposedFile(ReadmeFile, Readme(answers)));

        copied.Sort((a, b) => string.CompareOrdinal(a.To, b.To));
        declinedTypes.Sort(StringComparer.Ordinal);
        declinedCi.Sort(StringComparer.Ordinal);
        return new NewPlan(copied, composed, declinedTypes, declinedCi,
            TemplateFaults.Of(unclassified, unknownCi));
    }

    /// <summary>Judged on where a file lands, because a template may serve its pages from a subfolder.</summary>
    public static Func<string, bool> DeclinesTypes(Schema schema, IReadOnlyList<string> adopted)
    {
        var files = new HashSet<string>(StringComparer.Ordinal);
        var folders = new List<string>();

        foreach (var (name, type) in schema.ByFolder)
        {
            if (adopted.Contains(name, StringComparer.Ordinal)) continue;
            files.Add($".schema/{name}.yaml");
            if (!string.IsNullOrEmpty(type.Page)) files.Add(type.Page);
            folders.Add(type.Folder + "/");
        }

        return rel => files.Contains(rel) || folders.Any(f => rel.StartsWith(f, StringComparison.Ordinal));
    }

    /// <summary>
    /// <c>declined</c> is for <c>SeedLinks</c>, which takes out a seed page's links to types this corpus turned down.
    /// </summary>
    public static IReadOnlyList<string> Apply(NewPlan plan, string templateRoot, string corpusRoot,
        IReadOnlySet<string> declined)
    {
        foreach (var file in plan.Copied) SeedLinks.Receive(file, templateRoot, corpusRoot, declined);

        foreach (var file in plan.Composed)
        {
            var target = Path.Combine(corpusRoot, file.Path);
            Files.OpenFolderFor(target);
            File.WriteAllText(target, file.Content);
        }

        return [.. plan.Paths];
    }

    /// <summary>Each key takes a line of comment, so a corpus changing one has somewhere to read first.</summary>
    public static string Descriptor(NewAnswers answers, Upstream upstream)
    {
        var sb = new StringBuilder();

        sb.Append("# What this corpus is, and where it stands against the framework it took.\n");
        sb.Append("#\n");
        sb.Append("# https://paul80nd.github.io/knowledge-as-code/use/corpus-descriptor/ says what every key"
                  + " here means.\n\n");

        sb.Append("# The format of this file, which the tool owns rather than the corpus.\n");
        sb.Append($"descriptor-version: {CorpusDescriptor.Format}\n\n");

        sb.Append("# What this corpus calls itself, and the version of what it knows. Move"
                  + " `content-version` by hand.\n");
        sb.Append($"corpus: {Scalar(answers.Name)}\n");
        sb.Append($"content-version: \"{FirstContentVersion}\"\n\n");

        // Never filled in here: a shortcode cannot change once something cites it.
        sb.Append("# The shorthand another corpus cites this one by. Fill it in when one does, and never"
                  + " change it after.\n");
        sb.Append("shortcode:\n\n");

        // Written bare: a value supplied here would be inherited rather than chosen.
        sb.Append("# What this corpus is, carried into the package it packs and the plugin it"
                  + " bundles.\n");
        sb.Append("# A key left bare is carried into neither.\n");
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

        // Written bare, because only the corpus knows where its code is.
        sb.Append("\n# Where this estate's code repositories are addressed from. A `repos` entry on a"
                  + " service is appended to it.\n");
        sb.Append("repos-base:\n");

        sb.Append("\n# Where this corpus takes the framework from, and what the last take resolved to.\n");
        sb.Append("upstream:\n");
        sb.Append($"  url: {Scalar(upstream.Url)}\n");
        sb.Append(Optional("path", upstream.Path));
        sb.Append(Optional("ref", upstream.Ref));
        sb.Append(Optional("commit", upstream.Commit));
        sb.Append($"  template-version: {upstream.TemplateVersion}\n");
        sb.Append($"  taken-on: \"{upstream.TakenOn}\"\n\n");

        sb.Append("# Where to report a problem with the framework itself: `kac`, the schema, the"
                  + " template or a skill.\n");
        sb.Append("# This is a tracker rather than a folder, so it is asked separately from"
                  + " `upstream.url` above.\n");
        var tracker = Tracker(upstream);
        sb.Append("framework:\n");
        sb.Append($"  target: {Target(tracker)}\n");
        sb.Append($"  base: {Scalar(tracker)}\n\n");

        sb.Append("# How far an update goes. `cautious` writes a seed only where this corpus has none.\n");
        sb.Append($"update-policy: {CorpusDescriptor.Cautious}\n\n");

        sb.Append("# The types this corpus adopted. Validation and index generation cover these and no"
                  + " others.\n");
        sb.Append("types:\n");
        foreach (var type in answers.Types) sb.Append($"  - {Scalar(type)}\n");

        // Left empty: the values come from an estate not yet described, and `corpus-enum-undeclared` asks for
        // them when the first record needs them.
        sb.Append("\n# The ranges this corpus states, for the fields whose values are its estate's rather"
                  + " than the framework's.\n");
        sb.Append("# Each type's page says how to reach its own. A field drawing on one is judged against"
                  + " what you write here.\n");
        sb.Append("enums:\n\n");

        sb.Append("# What `kac export` leaves behind. Empty means a consumer reads what this corpus"
                  + " actually holds.\n");
        sb.Append("export:\n");
        sb.Append("  exclude: []\n\n");

        sb.Append("# Files this corpus holds differently from the framework, each with the reason it"
                  + " does.\n");
        sb.Append("skip: []\n");

        return sb.ToString();
    }

    /// <summary>
    /// Carries the markers for every block <c>README.md</c> is declared to hold. A README without them would decline
    /// those blocks on every corpus's behalf.
    /// </summary>
    public static string Readme(NewAnswers answers)
    {
        var sb = new StringBuilder();

        sb.Append($"# {answers.Name}\n\n");
        sb.Append($"{answers.Name} is a knowledge corpus: plain markdown in git, where every document has"
                  + " a type and\nevery type has a schema.\n\n");
        sb.Append("Rewrite this page to say what your corpus is for.\n");

        // Asked of the declaration, so a second block on this file needs no change here.
        var blocks = GeneratedFiles.Blocks([]).Where(f => f.Path == ReadmeFile).SelectMany(f => f.Blocks);
        foreach (var block in blocks)
        {
            sb.Append($"\n## The knowledge types\n\n");
            sb.Append($"`kac generate` writes the table below. Delete both markers to decline it.\n\n");
            sb.Append($"{Markers.Begin(block)}\n{Markers.End(block)}\n");
        }

        sb.Append("\n## Working here\n\n");
        sb.Append("Run `kac validate` to check this corpus against `.schema/`. Run `kac generate` to"
                  + " rebuild the indexes\nand the blocks above.\n\n");
        sb.Append("[`knowledge-as-code.md`](knowledge-as-code.md) says why the framework is built this"
                  + " way.\n[`CLAUDE.md`](CLAUDE.md) is what an agent working here reads.\n");

        return sb.ToString();
    }

    /// <summary>Written bare, which YAML reads as absent.</summary>
    private static string Optional(string key, string? value) =>
        value is { Length: > 0 } ? $"  {key}: {Scalar(value)}\n" : $"  {key}:\n";

    /// <summary>
    /// A null commit tells a folder from a clone: <c>TemplateSource.Read</c> resolves one for a clone alone. Reading
    /// the URL instead would call an ssh remote a folder. A folder's corpus files on the framework's own repository.
    /// </summary>
    private static string Tracker(Upstream upstream) =>
        upstream.Commit is { Length: > 0 } ? upstream.Url : Asking.DefaultFrom;

    /// <summary>
    /// Any host other than GitHub or Azure DevOps is written as <c>none</c> for the corpus to answer.
    /// </summary>
    private static string Target(string tracker) =>
        tracker.Contains("github.com", StringComparison.OrdinalIgnoreCase) ? Publishing.GitHub
        : tracker.Contains("dev.azure.com", StringComparison.OrdinalIgnoreCase)
          || tracker.Contains("visualstudio.com", StringComparison.OrdinalIgnoreCase) ? Publishing.AzureDevOps
        : Publishing.None;

    /// <summary>
    /// The words a YAML reader takes for a boolean or for nothing at all, rather than for a name. Held wider than the
    /// 1.2 core schema, which reads only the first pair: a folder called <c>no</c> or <c>off</c> is a corpus whose name
    /// loads as <c>false</c> under a reader on 1.1, and quoting costs the name nothing.
    /// </summary>
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
