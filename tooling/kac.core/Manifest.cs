using YamlDotNet.RepresentationModel;

namespace kac.core;

// One rule from a manifest: a set of path globs that all resolve to one layer.
//
// `To` is where the files a rule names land in a corpus, and is null where they land on the path they
// were read from. The template manifest is written against the repository holding the template, so a
// file authored at `template/knowledge-as-code.md` reaches a corpus as `knowledge-as-code.md`, and a
// fork laying its repository out differently says so here rather than in the tool.
// `Ci` names the continuous integration system a rule's files serve, and is null for the files every
// corpus receives whatever it builds on. A corpus runs on one system, so the starter for another is a
// file it would delete unread, and a GitHub workflow reaching a corpus that chose Azure DevOps is one
// that runs uninvited.
public record ManifestRule(IReadOnlyList<string> Patterns, string Layer, string? To = null, string? Ci = null);

public record Placement(string Layer, string Path, string? Ci = null);

public class Manifest
{
    // The layers a manifest sorts its files into, and the whole of what a rule may declare.
    public const string Overlay = "overlay";
    public const string Seed = "seed";
    public const string Withheld = "withheld";
    public const string Removed = "removed";

    public List<ManifestRule> Rules = [];

    // The template's version, which an update stamps into the receiving corpus's descriptor. Read from
    // the template rather than assumed, so a corpus taking from an older upstream records what it took.
    public int Version;

    // The oldest `kac` that can read this template, or null where the manifest names none. The template
    // is fetched rather than shipped inside the package, so the two version independently. An older tool
    // meeting a newer template stops on this rather than half-reading it.
    public string? MinimumTool;

    // What a template's manifest is called, wherever the repository serving it keeps the file.
    public const string FileName = "manifest.yaml";

    // Read from a path rather than from a corpus root. The manifest is the template's, and a template
    // repository keeps it wherever it likes: at the root, or in the folder `upstream.path` names.
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
                var ci = Yaml.Str(Yaml.Get(rule, "ci"));
                if (patterns.Count > 0 && layer is not null)
                    m.Rules.Add(new ManifestRule(patterns, layer, to, ci));
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
                return new Placement(rule.Layer, Destination(relPath, pattern, rule.To), rule.Ci);
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

// One corpus this one consumes, as its entry in `consumes:` states it.
//
// `Version` is the range the corpus meant and `Resolved` is what the last restore actually took. Both
// sit on this one entry rather than in a lock file beside the descriptor, so `.corpus.yaml` stays the
// one description of what a corpus is.
//
// Every field is nullable because this is what the file said. An entry short of anything a restore
// needs is refused by name in `Restore.Plan`, where the message can say which key is missing and
// which entry it is missing from.
public record Consumed(
    string? Corpus, string? Shortcode, string? Version, string? Resolved, string? Source);

public class CorpusDescriptor
{
    // The format `.corpus.yaml` is written in. The tool's own number: a corpus cannot know the shape a
    // newer tool writes, so an update stamps this alongside what it took.
    public const int Format = 1;

    // Keys the descriptor once used, beside what each is called now. The tool names the old key, the new
    // one and the file, and rewrites nothing: a corpus that has taken a copy is a repository someone owns.
    //
    // A key is named by the block it sits in, so `upstream.mechanism-version` is found where it lives
    // rather than at the root. `New` is null where the key was dropped, and `Gone` says so instead.
    private static readonly (string Section, string Old, string? New, string Gone)[] Renamed =
    [
        ("", "version", "descriptor-version", ""),
        ("", "accepted-divergences", "skip", ""),
        ("", "role", null,
            "it said whether a corpus carried the tests that prove the tool, which no corpus does."),
        ("upstream", "mechanism-version", "template-version", ""),
        ("upstream", "synced-on", "taken-on", ""),
        ("upstream", "synced-from", null,
            "where the framework is taken from is `upstream.url`, and what was taken is `upstream.commit`.")
    ];

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

    // The corpora this one consumes, in the order the descriptor lists them. Empty for a corpus standing
    // on its own, which is the first-class case and the one that needs no restore at all.
    //
    // A different relationship from `upstream:` above, and deliberately a different key. `upstream:` is
    // one framework flowing down as files this corpus receives; this is a graph of records this corpus
    // reads and never holds. `docs/corpus-descriptor.md` sets the two side by side.
    public readonly List<Consumed> Consumes = [];

    // Where `kac bundle` reads the plugin tree from, relative to this corpus's root.
    //
    // Null where the corpus keeps its own `.plugin/`, which is the ordinary case and the only one a
    // corpus standing alone has. A value shares one tree between several corpora in a repository, and
    // `update` then withholds the shared half rather than writing a copy of it here.
    // `docs/cli/bundle.md` says how the shared tree and this corpus's own are merged.
    public string? PluginFrom;

    // What this corpus calls itself. An export states it so that a consumer holding several exports can
    // tell whose vocabulary it is reading, which the folder it vendored the files into may not say.
    public string? Name;

    // The shorthand another corpus cites this one by, as the `eng` in `eng:pol-VURM.TIMEBOX`. The
    // producer owns it, so a consumer citing this corpus spells it this way and never invents an alias
    // of its own. `docs/framework/metadata.md` carries the notation and why the value never changes.
    //
    // Null where the corpus has not declared one, which is every corpus nothing cites yet. Declaring is
    // what a corpus does when something is about to cite it, so nobody invents a value they may later
    // want back.
    public string? Shortcode;

    // How long a shortcode may be. `.schema/_checks.yaml` argues each part of the spelling under
    // `shortcode`.
    public const int ShortcodeMin = 2;
    public const int ShortcodeMax = 8;

    // How a shortcode is spelled wrong, or null where it is spelled correctly. The wording completes
    // "shortcode 'x' ...", and names the first fault alone: an author fixing it re-runs the check.
    //
    // Here rather than beside the check that reports it, because two things ask. `validate` holds a
    // corpus to the shortcode it declares, and `restore` holds a consumer to the one it files an import
    // under: a value that is not a shortcode is a folder name that is not one either.
    public static string? ShortcodeFault(string shortcode)
    {
        if (shortcode.Length < ShortcodeMin) return "is too short";
        if (shortcode.Length > ShortcodeMax) return "is too long";
        if (!char.IsAsciiLetterLower(shortcode[0])) return "does not open on a lower-case letter";

        return shortcode.Any(c => !char.IsAsciiLetterLower(c) && !char.IsAsciiDigit(c))
            ? "carries something other than a lower-case letter or a digit"
            : null;
    }

    // How this corpus is published, and where the published form is served from. The target names a set
    // of link-building rules and `Publishing` holds them; the base is the only part a corpus supplies,
    // because it is the only part that differs between two corpora on one target.
    //
    // One base, and it is the URL a person opens to browse the corpus: the GitHub repository, the Azure
    // Repos repository, or the Azure DevOps wiki. Where an agent reads a record's source from is derived
    // from it rather than stated beside it, because only GitHub ever served that from a second host.
    //
    // Both are null where the descriptor states none, and a corpus publishing nowhere states neither. An
    // export from one carries no links rather than links built on an empty base.
    public string? PublishingTarget;
    public string? Base;

    // Where the corpus root sits inside the published repository, where it is not the root itself.
    //
    // A corpus kept in a subdirectory cannot say this through its base. The commit the links resolve
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
    // Each is null where the descriptor has not stated one. Only the corpus can say what it knows, so the
    // tool never supplies `ContentVersion`, and an update stamps the other two.
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

    public static CorpusDescriptor Load(string corpusRoot)
    {
        var path = Path.Combine(corpusRoot, ".corpus.yaml");
        var descriptor = new CorpusDescriptor();
        if (!File.Exists(path)) return descriptor;

        var root = Yaml.LoadFile(path);

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
        descriptor.Shortcode = Blank(Yaml.Str(Yaml.Get(root, "shortcode")));
        descriptor.PublishingTarget = Yaml.Str(Yaml.Get(root, "publishing-target"));
        var publishing = Yaml.Get(root, "publishing");
        descriptor.Base = Yaml.Str(Yaml.Get(publishing, "base"));
        descriptor.PathPrefix = Yaml.Str(Yaml.Get(publishing, "path-prefix"));
        descriptor.ExportExclude.AddRange(Yaml.StrList(Yaml.Get(Yaml.Get(root, "export"), "exclude")));
        descriptor.PluginFrom = Blank(Yaml.Str(Yaml.Get(Yaml.Get(root, "plugin"), "from")));

        if (Yaml.Get(root, "types") is YamlSequenceNode types)
            descriptor.Types = [.. types.Children.Select(Yaml.Str).OfType<string>()];

        if (Yaml.Get(root, "consumes") is YamlSequenceNode consumes)
            foreach (var item in consumes.Children)
                descriptor.Consumes.Add(new Consumed(
                    Blank(Yaml.Str(Yaml.Get(item, "corpus"))),
                    Blank(Yaml.Str(Yaml.Get(item, "shortcode"))),
                    Blank(Yaml.Str(Yaml.Get(item, "version"))),
                    Blank(Yaml.Str(Yaml.Get(item, "resolved"))),
                    Blank(Yaml.Str(Yaml.Get(item, "source")))));

        if (Yaml.Get(root, "skip") is YamlSequenceNode seq)
            foreach (var item in seq.Children)
                if (Yaml.Str(Yaml.Get(item, "path")) is { } p)
                    descriptor.Skipped.Add(new SkippedFile(p, Yaml.Str(Yaml.Get(item, "reason"))));

        return descriptor;

        // A key written with no value parses as an empty scalar, which is a corpus saying nothing rather
        // than saying "". `examples/library/.corpus.yaml` writes `path:` bare, with the reason in a comment.
        static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
    }

    // What to tell an author whose descriptor still uses a renamed key, or null where none is in use.
    //
    // `update` stops on this. It would otherwise report on a file it has misread, and write a stamp
    // beside a key it does not read. The message carries the old key, the new one and the path, so the
    // fix is mechanical.
    public static string? RenamedKeyInUse(string corpusRoot)
    {
        var path = Path.Combine(corpusRoot, ".corpus.yaml");
        if (!File.Exists(path)) return null;

        var root = Yaml.LoadFile(path);
        foreach (var (section, old, replacement, gone) in Renamed)
        {
            var at = section.Length == 0 ? root : Yaml.Get(root, section);
            if (Yaml.Get(at, old) is null) continue;

            var name = section.Length == 0 ? old : $"{section}.{old}";
            return replacement is null
                ? $"update: {path} still says `{name}:`, which nothing reads. delete it: {gone}"
                : $"update: {path} still says `{name}:`. rename it to `{replacement}:`, which says what "
                  + "the value is about. this corpus states three versions: `descriptor-version` for the "
                  + "file's own format, `content-version` for what the corpus knows, and "
                  + "`upstream.template-version` for the template it took.";
        }

        return null;
    }

    // Rewrite the `types:` block, which is what `--add-type` and `--drop-type` come to in the descriptor.
    //
    // The items are replaced whole, because the list is the value, and everything around them is left as
    // it stands. Line-oriented for the reason `Stamp` is: most of this file is commentary, and a YAML
    // round-trip would throw all of it away.
    //
    // A descriptor stating no `types:` is left alone. That corpus has not declared, and opening the block
    // here would turn "these folders happen to be here" into a decision nobody made.
    public static void SetTypes(string corpusRoot, IReadOnlyList<string> types)
    {
        var path = Path.Combine(corpusRoot, ".corpus.yaml");
        if (!File.Exists(path)) return;

        var lines = new List<string>(Files.ReadLf(path).Split('\n'));
        var start = lines.FindIndex(l => l.StartsWith("types:", StringComparison.Ordinal));
        if (start < 0) return;

        var end = start + 1;
        while (end < lines.Count && lines[end].TrimStart().StartsWith("- ", StringComparison.Ordinal)) end++;

        lines.RemoveRange(start + 1, end - start - 1);
        lines.InsertRange(start + 1, types.Select(t => $"  - {t}"));

        File.WriteAllText(path, string.Join('\n', lines).TrimEnd('\n') + "\n");
    }

    // Record what an update took: the format this tool writes, the commit it resolved, the template's
    // version, and the day it arrived. The corpus's own `content-version` is untouched, because what the
    // corpus knows is not something an upstream can tell it.
    //
    // `commit` is null where the template was read from a folder. A folder has no ref to follow, so the
    // key is left as it stands rather than filled with a commit nobody resolved.
    //
    // This rewrites lines rather than re-serialising the file, because the descriptor is mostly
    // commentary. Someone opens it to read what each key means and when owning a file is worth declaring,
    // and a YAML round-trip would throw all of that away. Rewriting a line does drop any trailing comment
    // on it, which is right: that comment described the value the update just replaced.
    public static void Stamp(string corpusRoot, int templateVersion, string takenOn, string? commit = null)
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

        var stamped = Stamped(templateVersion, takenOn, commit);
        var written = new HashSet<string>(StringComparer.Ordinal);
        for (var i = start + 1; i < lines.Count && !IsTopLevelKey(lines[i]); i++)
            foreach (var (key, value) in stamped)
                if (lines[i].TrimStart().StartsWith(key + ":", StringComparison.Ordinal))
                {
                    lines[i] = Line(key, value);
                    written.Add(key);
                }

        // Keys the block never held go in at its head, where they read as part of it.
        lines.InsertRange(start + 1, stamped
            .Where(s => !written.Contains(s.key))
            .Select(s => Line(s.key, s.value)));

        File.WriteAllText(path, string.Join('\n', lines).TrimEnd('\n') + "\n");
        return;

        static string Line(string key, string value) => $"  {key + ":",-18} {value}";

        static (string key, string value)[] Stamped(int version, string on, string? head)
        {
            var keys = new List<(string, string)>();
            if (head is { Length: > 0 }) keys.Add(("commit", head));
            keys.Add(("template-version", version.ToString()));
            keys.Add(("taken-on", $"\"{on}\""));
            return [.. keys];
        }

    }

    // Record what a restore resolved: the version each consumed corpus was taken at, written onto that
    // corpus's own entry beside the range it resolved from. `versions` is keyed by the corpus name, and
    // an entry the map does not name is left as it stands.
    //
    // Line-oriented for the reason `Stamp` is, and the reason is stronger here: an entry carries a range
    // somebody chose and often a comment saying why, and a YAML round-trip would return the range without
    // the argument for it.
    //
    // An entry already carrying `resolved:` has that line rewritten. One that does not takes a new line
    // at the end of its own keys, so the lock reads as part of the entry it belongs to.
    public static void SetResolved(string corpusRoot, IReadOnlyDictionary<string, string> versions)
    {
        var path = Path.Combine(corpusRoot, ".corpus.yaml");
        if (!File.Exists(path) || versions.Count == 0) return;

        var before = Files.ReadLf(path);
        var lines = new List<string>(before.Split('\n'));
        var start = lines.FindIndex(l => l.StartsWith("consumes:", StringComparison.Ordinal));
        if (start < 0) return;

        // Walked from the end, so an insertion never moves a line this loop has yet to read.
        var entries = Entries(lines, start);
        for (var i = entries.Count - 1; i >= 0; i--)
        {
            var entry = entries[i];
            if (entry.Corpus is not { } name || !versions.TryGetValue(name, out var version)) continue;

            var line = $"{new string(' ', entry.Indent)}resolved: {Quoted(version)}";
            if (entry.Resolved is { } at) lines[at] = line;
            else lines.Insert(entry.End, line);
        }

        // Written only where a line changed. A restore that found every import current would otherwise
        // rewrite the descriptor on every run, and a file nothing edited should not report as edited.
        var after = string.Join('\n', lines).TrimEnd('\n') + "\n";
        if (!after.Equals(before, StringComparison.Ordinal)) File.WriteAllText(path, after);
    }

    // Where each entry of the `consumes:` block sits: the corpus it names, the column its keys are
    // written at, the line holding its `resolved:` if it has one, and the line after its last key.
    private static List<(string? Corpus, int Indent, int? Resolved, int End)> Entries(
        List<string> lines, int start)
    {
        var entries = new List<(string? Corpus, int Indent, int? Resolved, int End)>();
        string? corpus = null;
        var indent = 0;
        int? resolved = null;
        var end = start + 1;

        for (var i = start + 1; i < lines.Count && !IsTopLevelKey(lines[i]); i++)
        {
            var trimmed = lines[i].TrimStart();
            if (trimmed.Length == 0 || trimmed.StartsWith('#')) continue;

            // A dash opens an entry and carries that entry's first key, so the column after it is where
            // every key of the entry is written.
            if (trimmed.StartsWith("- ", StringComparison.Ordinal))
            {
                if (indent > 0) entries.Add((corpus, indent, resolved, end));
                corpus = null;
                resolved = null;
                indent = lines[i].Length - trimmed.Length + 2;
                trimmed = trimmed[2..];
            }
            else if (indent == 0) continue;

            if (Key(trimmed, "corpus") is { } named) corpus = named;
            if (trimmed.StartsWith("resolved:", StringComparison.Ordinal)) resolved = i;
            end = i + 1;
        }

        if (indent > 0) entries.Add((corpus, indent, resolved, end));
        return entries;

        // What a key on this line says, or null where the line says something else. The value is read
        // as YAML would read it, so a version written in quotes comes back without them.
        static string? Key(string line, string key) =>
            line.StartsWith(key + ":", StringComparison.Ordinal)
                ? line[(key.Length + 1)..].Trim().Trim('"', '\'') is { Length: > 0 } v ? v : null
                : null;
    }

    // A version written so YAML reads it as a string. `0.1.0` parses as one either way, and `1.0` does
    // not, so the quotes are what stop a two-part version arriving back as a number.
    private static string Quoted(string version) => $"\"{version}\"";

    private static bool IsTopLevelKey(string line) =>
        line.Length > 0 && !char.IsWhiteSpace(line[0]) && line[0] != '#';
}
