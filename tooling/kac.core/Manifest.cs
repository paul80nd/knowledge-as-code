using YamlDotNet.RepresentationModel;

namespace kac.core;

// One rule from a manifest: a set of path globs that all resolve to one layer.
//
// `To` is where the files a rule names land in a corpus, and is null where they land on the path they
// were read from. The template manifest is written against the repository holding the template, so a
// file authored at `template/knowledge-as-code.md` reaches a corpus as `knowledge-as-code.md`, and a
// fork laying its repository out differently says so here rather than in the tool.
public record ManifestRule(IReadOnlyList<string> Patterns, string Layer, string? To = null);

public record Placement(string Layer, string Path);

public class Manifest
{
    // The layers a template manifest sorts its files into. The portability manifest names four of its
    // own, so this is the template's vocabulary rather than every manifest's.
    public const string Overlay = "overlay";
    public const string Seed = "seed";
    public const string Withheld = "withheld";
    public const string Removed = "removed";

    public List<ManifestRule> Rules = [];

    // The mechanism's version, which a sync stamps into the receiving corpus's descriptor. Read from the
    // reference rather than assumed, so a corpus taking from an older upstream records what it took.
    public int Version;

    // The oldest `kac` that can read this template, or null where the manifest names none. The template
    // is fetched rather than shipped inside the package, so the two version independently. An older tool
    // meeting a newer template stops on this rather than half-reading it.
    public string? MinimumTool;

    public static Manifest Load(string corpusRoot) =>
        LoadFrom(Path.Combine(corpusRoot, "tooling", "manifest.yaml"));

    // Two manifests are written in this shape and neither is the other's business: the portability
    // manifest says which of a corpus's files are shared with the framework, and the template manifest
    // says which files a corpus is made of. What they share is the grammar (ordered rules of globs,
    // first match winning), so the reader takes a path rather than assuming one.
    public static Manifest LoadFrom(string manifestFile)
    {
        var m = new Manifest();
        var root = Yaml.LoadFile(manifestFile);
        if (int.TryParse(Yaml.Str(Yaml.Get(root, "version")), out var version)) m.Version = version;
        m.MinimumTool = Yaml.Str(Yaml.Get(root, "minimum-tool"));
        if (Yaml.Get(root, "rules") is YamlSequenceNode rules)
            foreach (var rule in rules.Children)
            {
                var pathNode = Yaml.Get(rule, "path");
                var patterns = pathNode is YamlSequenceNode
                    ? Yaml.StrList(pathNode)
                    : Yaml.Str(pathNode) is { } single
                        ? [single]
                        : [];

                // Read from the key being there rather than from its value, so `to: ""` says the corpus
                // root and is not mistaken for a rule that named no destination at all.
                var to = Yaml.Get(rule, "to") is { } toNode ? Yaml.Str(toNode) ?? "" : null;
                var layer = Yaml.Str(Yaml.Get(rule, "layer"));
                if (patterns.Count > 0 && layer is not null)
                    m.Rules.Add(new ManifestRule(patterns, layer, to));
            }

        return m;
    }

    // First rule with a matching glob wins, mirroring the manifest's own "evaluated in order"
    // contract. Returns null when nothing matches. The check reports that as an error, since the
    // manifest is meant to resolve every file (its final rule is a catch-all).
    public Placement? Place(string relPath)
    {
        foreach (var rule in Rules)
        foreach (var pattern in rule.Patterns)
            if (Glob.IsMatch(relPath, pattern))
                return new Placement(rule.Layer, Destination(relPath, pattern, rule.To));
        return null;
    }

    // The layer alone, for a caller that only sorts files and never writes them.
    public string? Resolve(string relPath) => Place(relPath)?.Layer;

    // The patterns a rule's files land on, which are its own patterns with `to:` applied to each. A
    // check reading the corpus side asks what a corpus is allowed to hold there, and this is how it
    // gets that from the same rule that decided what was sent.
    public static IEnumerable<string> Destinations(ManifestRule rule) =>
        rule.Patterns.Select(p => Destination(p, p, rule.To));

    // Where a matched file lands. `to:` replaces the pattern's directory prefix, meaning everything up
    // to and including the last `/` before its first wildcard. So one rule relocates a whole folder and
    // keeps the shape inside it, and several single-file patterns sharing a folder relocate under one
    // `to:` rather than needing one apiece.
    //
    // A pattern opening on a wildcard, or naming a file at the root, has no directory prefix. It names
    // no one folder, so there is nothing to rewrite a tail against and the destination is `to:` itself.
    internal static string Destination(string relPath, string pattern, string? to)
    {
        if (to is null) return relPath;

        var star = pattern.IndexOf('*', StringComparison.Ordinal);
        var head = star < 0 ? pattern : pattern[..star];
        var prefix = head[..(head.LastIndexOf('/') + 1)];
        return prefix.Length > 0 && relPath.StartsWith(prefix, StringComparison.Ordinal)
            ? to + relPath[prefix.Length..]
            : to;
    }
}

// One file a corpus holds differently from the framework, and means to. The reason is for whoever opens
// the descriptor next, and nothing in the tool reads it.
public record SkippedFile(string Path, string? Reason);

public class CorpusDescriptor
{
    // The format `.corpus.yaml` is written in. The tool's own number: a corpus cannot know the shape a
    // newer mechanism writes, so a sync stamps this alongside what it took.
    public const int Format = 1;

    // Keys the descriptor once used, beside what each is called now. `version:` alone said the file's own
    // format, which is one of three versions the file states, so a reader met a number without knowing
    // which question it answered.
    //
    // A rename is the author's to make. No part of this file comes down from an upstream, and a corpus
    // that has taken a copy is a repository someone owns, so the tool names the old key, the new one and
    // the file, and stops. Rewriting it would save a single edit and break the file's own promise.
    // A key is named by the block it sits in, so `upstream.mechanism-version` is found where it lives
    // rather than at the root. `New` is null where the key was dropped and nothing replaced it.
    private static readonly (string Section, string Old, string? New)[] Renamed =
    [
        ("", "version", "descriptor-version"),
        ("", "accepted-divergences", "skip"),
        ("upstream", "mechanism-version", "template-version"),
        ("upstream", "synced-on", "taken-on"),
        ("upstream", "synced-from", null)
    ];

    public string Role = "";

    // Where the framework comes from, and what was last taken from it. `Path` is null where the manifest
    // sits at the upstream repository's root. `docs/corpus-descriptor.md` covers the rest of the block.
    public string? UpstreamUrl;
    public string? UpstreamPath;
    public string? UpstreamRef;
    public string? UpstreamCommit;
    public string? TakenOn;

    // How far an update goes. `docs/cli/update.md` argues the default.
    public const string Cautious = "cautious";
    public const string Full = "full";
    public static readonly IReadOnlyList<string> Policies = [Cautious, Full];
    public string UpdatePolicy = Cautious;

    // Files this corpus holds differently on purpose, neither read nor written in either direction.
    // `docs/cli/update.md` says what that buys a corpus.
    public readonly List<SkippedFile> Skipped = [];

    // What this corpus calls itself. An export states it so that a consumer holding several exports can
    // tell whose vocabulary it is reading, which the folder it vendored the files into may not say.
    public string? Name;

    // How this corpus is published, and where the published form is served from. The target names a set
    // of link-building rules and `Publishing` holds them; the two bases are the only part a corpus
    // supplies, because they are the only part that differs between two corpora on one target.
    //
    // Each is null where the descriptor states none, and a corpus publishing nowhere states none of the
    // three. An export from one carries no links rather than links built on an empty base.
    public string? PublishingTarget;
    public string? HumanBase;
    public string? RawBase;

    // Where the corpus root sits inside the published repository, where it is not the root itself.
    //
    // A corpus kept in a subdirectory cannot say this through its bases. The commit the links resolve
    // against goes between the base and the record's path, so a prefix folded into the base would land
    // on the wrong side of it. Null where the corpus is the repository, which is the ordinary case.
    public string? PathPrefix;

    // What an export leaves behind: `draft`, `overdue`, or neither. Empty by default, because a record
    // carrying its own state lets a consumer decide, and one filtered out downstream is invisible. The
    // corpus reads smaller and tidier than it is, with nothing saying anything was withheld.
    public readonly List<string> ExportExclude = [];

    // The two things an export can be told to leave behind. Read by the exporter from here, so a corpus
    // naming a third is told rather than having it ignored.
    public const string ExcludeDraft = "draft";
    public const string ExcludeOverdue = "overdue";
    public static readonly IReadOnlyList<string> Excludable = [ExcludeDraft, ExcludeOverdue];

    // The three versions the descriptor states, each named for what it versions.
    //
    // `DescriptorVersion` is this file's format and `TemplateVersion` is the shape of the template the
    // corpus last took, both counts the tool understands. `ContentVersion` is the corpus's own: what its
    // records mean, semantically versioned, bumped by hand and read by whatever publishes an export. It
    // stays a string because it is a version and not a count, and nothing but a person writes it.
    //
    // Each is null where the descriptor has not stated one, and `mechanism --check` reports that as not
    // declared. Only the corpus can say what it knows, so the tool never supplies a number here.
    public int? DescriptorVersion;
    public string? ContentVersion;
    public int? TemplateVersion;

    // The types this corpus has adopted, named as the schema names them. This is a statement of intent
    // rather than a description: the corpus says which of the framework's types it wants. Everything
    // else follows from that: what is generated, what a sync brings down, and what `validate` holds the
    // corpus to having stood up.
    //
    // Null where the descriptor says nothing, which is not the same as an empty list. A corpus that has
    // not declared yet is read off its own folders instead, so adopting the key is a change a corpus
    // makes when it is ready rather than one the tool forces on the version that arrives without it.
    public List<string>? Types;

    // Whether a type is this corpus's. An undeclared corpus answers yes to everything and leaves the
    // question to the filesystem; the callers that ask are the ones that already know what is on disk.
    public bool Adopted(string type) => Types is null || Types.Contains(type, StringComparer.Ordinal);

    // Whether this corpus carries the layer that proves the mechanism. A consumer takes a tool already
    // proven upstream, so a fixture tree it will never run sits between its readers and the code they
    // came for. Every other role answers for the tool and holds the tests that prove it. A descriptor
    // naming no role answers yes, as `Adopted` does: a corpus that has said nothing is held to everything.
    public bool Verifies => !Role.Equals("consumer", StringComparison.Ordinal);

    public static CorpusDescriptor Load(string corpusRoot)
    {
        var path = Path.Combine(corpusRoot, ".corpus.yaml");
        var descriptor = new CorpusDescriptor();
        if (!File.Exists(path)) return descriptor;

        var root = Yaml.LoadFile(path);
        descriptor.Role = Yaml.Str(Yaml.Get(root, "role")) ?? "";

        var upstream = Yaml.Get(root, "upstream");
        var url = Yaml.Str(Yaml.Get(upstream, "url"));
        descriptor.UpstreamUrl = string.IsNullOrWhiteSpace(url) ? null : url;
        descriptor.UpstreamPath = Blank(Yaml.Str(Yaml.Get(upstream, "path")));
        descriptor.UpstreamRef = Blank(Yaml.Str(Yaml.Get(upstream, "ref")));
        descriptor.UpstreamCommit = Blank(Yaml.Str(Yaml.Get(upstream, "commit")));
        descriptor.TakenOn = Blank(Yaml.Str(Yaml.Get(upstream, "taken-on")));

        descriptor.DescriptorVersion = Yaml.NullableInt(Yaml.Get(root, "descriptor-version"));
        descriptor.ContentVersion = Yaml.Str(Yaml.Get(root, "content-version"));
        descriptor.TemplateVersion = Yaml.NullableInt(Yaml.Get(upstream, "template-version"));
        descriptor.UpdatePolicy = Blank(Yaml.Str(Yaml.Get(root, "update-policy"))) ?? Cautious;

        descriptor.Name = Yaml.Str(Yaml.Get(root, "corpus"));
        descriptor.PublishingTarget = Yaml.Str(Yaml.Get(root, "publishing-target"));
        var publishing = Yaml.Get(root, "publishing");
        descriptor.HumanBase = Yaml.Str(Yaml.Get(publishing, "human-base"));
        descriptor.RawBase = Yaml.Str(Yaml.Get(publishing, "raw-base"));
        descriptor.PathPrefix = Yaml.Str(Yaml.Get(publishing, "path-prefix"));
        descriptor.ExportExclude.AddRange(Yaml.StrList(Yaml.Get(Yaml.Get(root, "export"), "exclude")));

        if (Yaml.Get(root, "types") is YamlSequenceNode types)
            descriptor.Types = [.. types.Children.Select(Yaml.Str).OfType<string>()];

        if (Yaml.Get(root, "skip") is YamlSequenceNode seq)
            foreach (var item in seq.Children)
                if (Yaml.Str(Yaml.Get(item, "path")) is { } p)
                    descriptor.Skipped.Add(new SkippedFile(p, Yaml.Str(Yaml.Get(item, "reason"))));

        return descriptor;

        // A key written with no value parses as an empty scalar, which is a corpus saying nothing rather
        // than saying "". `example/.corpus.yaml` writes `url:` bare, with the reason in a comment.
        static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
    }

    // What to tell an author whose descriptor still uses a renamed key, or null where none is in use.
    //
    // Both halves of `mechanism` stop on this. A check would otherwise report on a file it has misread,
    // and a sync would write a stamp beside a key it does not read. The message carries the old key, the
    // new one and the path, so the fix is mechanical.
    public static string? RenamedKeyInUse(string corpusRoot)
    {
        var path = Path.Combine(corpusRoot, ".corpus.yaml");
        if (!File.Exists(path)) return null;

        var root = Yaml.LoadFile(path);
        foreach (var (section, old, replacement) in Renamed)
        {
            var at = section.Length == 0 ? root : Yaml.Get(root, section);
            if (Yaml.Get(at, old) is null) continue;

            var name = section.Length == 0 ? old : $"{section}.{old}";
            return replacement is null
                ? $"mechanism: {path} still says `{name}:`, which nothing reads. Delete it. Where the "
                  + "framework was taken from is `upstream.url`, and what was taken is `upstream.commit`."
                : $"mechanism: {path} still says `{name}:`. Rename it to `{replacement}:`, which says what "
                  + "the value is about. This corpus states three versions: `descriptor-version` for the "
                  + "file's own format, `content-version` for what the corpus knows, and "
                  + "`upstream.template-version` for the template it took.";
        }

        return null;
    }

    // Record what a sync took: the format this tool writes, the template's version, and the day it
    // arrived. The corpus's own `content-version` is untouched, because what the corpus knows is not
    // something an upstream can tell it.
    //
    // `upstream.commit` is left alone. A sync reads a directory rather than a git ref, so it has no
    // commit to record, and writing one it guessed at would be worse than the key staying empty.
    //
    // This rewrites three lines rather than re-serialising the file, because the descriptor is mostly
    // commentary. Someone opens it to read what each key means and when a divergence is worth accepting,
    // and a YAML round-trip would throw all of that away. Rewriting a line does drop any trailing comment
    // on it, which is right: that comment described the value the sync just replaced.
    public static void Stamp(string corpusRoot, int templateVersion, string takenOn)
    {
        var path = Path.Combine(corpusRoot, ".corpus.yaml");
        var lines = File.Exists(path)
            ? new List<string>(Files.ReadLf(path).Split('\n'))
            : [];

        // The file's own format, which the tool owns and the corpus does not. A descriptor without the key
        // takes it above its first key, below whatever header comment stands there.
        const string formatKey = "descriptor-version:";
        var format = lines.FindIndex(l => l.StartsWith(formatKey, StringComparison.Ordinal));
        if (format >= 0) lines[format] = $"{formatKey} {Format}";
        else
        {
            var first = lines.FindIndex(IsTopLevelKey);
            lines.Insert(first < 0 ? lines.Count : first, $"{formatKey} {Format}");
        }

        // A descriptor with no `upstream:` block has never been synced. Open one rather than fail: the
        // corpus is recording where it takes from for the first time, which is what the block is for.
        var start = lines.FindIndex(l => l.StartsWith("upstream:", StringComparison.Ordinal));
        if (start < 0)
        {
            if (lines.Count > 0) lines.Add("");
            lines.Add("upstream:");
            start = lines.Count - 1;
        }

        var written = new HashSet<string>(StringComparer.Ordinal);
        for (var i = start + 1; i < lines.Count && !IsTopLevelKey(lines[i]); i++)
            foreach (var (key, value) in Stamped(templateVersion, takenOn))
                if (lines[i].TrimStart().StartsWith(key + ":", StringComparison.Ordinal))
                {
                    lines[i] = Line(key, value);
                    written.Add(key);
                }

        // Keys the block never held go in at its head, where they read as part of it.
        lines.InsertRange(start + 1, Stamped(templateVersion, takenOn)
            .Where(s => !written.Contains(s.key))
            .Select(s => Line(s.key, s.value)));

        File.WriteAllText(path, string.Join('\n', lines).TrimEnd('\n') + "\n");
        return;

        static string Line(string key, string value) => $"  {key + ":",-18} {value}";

        static (string key, string value)[] Stamped(int version, string on) =>
            [("template-version", version.ToString()), ("taken-on", $"\"{on}\"")];

        static bool IsTopLevelKey(string line) => line.Length > 0 && !char.IsWhiteSpace(line[0]) && line[0] != '#';
    }
}
