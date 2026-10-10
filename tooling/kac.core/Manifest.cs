using YamlDotNet.RepresentationModel;

namespace kac.core;

// One rule from a manifest: a set of path globs that all resolve to one layer. `To` is where the rule's files land in a
// corpus, and null where they land on the path they were read from. `Ci` names the CI system the files serve, and is
// null for files every corpus receives.
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

    // The template's version, which an update stamps into the receiving corpus's descriptor.
    public int Version;

    // The oldest `kac` that can read this template, or null where the manifest names none. An older tool meeting a
    // newer template stops on this.
    public string? MinimumTool;

    public const string FileName = "manifest.yaml";

    // Read from a path, because a template repository keeps its manifest at the root or in the folder `upstream.path`
    // names.
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

                // Read from the key being there, so `to: ""` means the corpus root and not a rule naming no
                // destination.
                var to = Yaml.Get(rule, "to") is { } toNode ? Yaml.Str(toNode) ?? "" : null;
                var layer = Yaml.Str(Yaml.Get(rule, "layer"));
                var ci = Yaml.Str(Yaml.Get(rule, "ci"));
                if (patterns.Count > 0 && layer is not null)
                    m.Rules.Add(new ManifestRule(patterns, layer, to, ci));
            }

        return m;
    }

    // The first rule with a matching glob wins, as the manifest's "evaluated in order" contract says. Null where
    // nothing matches, which the check reports as an error.
    public Placement? Place(string relPath)
    {
        foreach (var rule in Rules)
        foreach (var pattern in rule.Patterns)
            if (Glob.IsMatch(relPath, pattern))
                return new Placement(rule.Layer, Destination(relPath, pattern, rule.To), rule.Ci);
        return null;
    }

    public string? Resolve(string relPath) => Place(relPath)?.Layer;

    // The patterns a rule's files land on: its own patterns with `to:` applied to each.
    public static IEnumerable<string> Destinations(ManifestRule rule) =>
        rule.Patterns.Select(p => Destination(p, p, rule.To));

    // Where a matched file lands. `to:` replaces the pattern's directory prefix, which is everything up to and
    // including the last `/` before its first wildcard. A pattern with no directory prefix lands on `to:` itself.
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

// One file a corpus holds differently from the framework, on purpose. Nothing in the tool reads the reason.
public record SkippedFile(string Path, string? Reason);

// One corpus this one consumes, as its entry in `consumes:` states it. `Version` is the range the corpus meant, and
// `Resolved` is what the last restore took. Every field is nullable because this is what the file said, and
// `Restore.Plan` refuses an entry short of a key by name.
public record Consumed(
    string? Corpus,
    string? Shortcode,
    string? Version,
    string? Resolved,
    string? Source);

public class CorpusDescriptor
{
    // The format `.corpus.yaml` is written in, which an update stamps alongside what it took.
    public const int Format = 6;

    // Keys the descriptor once used, beside what each is called now. The tool names the old key, the new one and the
    // file, and rewrites nothing. A key is named by the block it sits in. `New` is null where the key was dropped, and
    // `Gone` says so instead.
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

    // The corpora this one consumes, in the order the descriptor lists them, and empty for a corpus standing on its
    // own. `docs/corpus-descriptor.md` sets this beside `upstream:`.
    public readonly List<Consumed> Consumes = [];

    // Where `kac bundle` reads the plugin tree from, relative to this corpus's root, and null where the corpus keeps
    // its own `.plugin/`. `docs/cli/bundle.md` says how the shared tree and this corpus's own are merged.
    public string? PluginFrom;

    // What this corpus calls itself. An export states it, so a consumer holding several exports can tell whose
    // vocabulary it is reading.
    public string? Name;

    // The shorthand another corpus cites this one by, as the `eng` in `eng:pol-VURM.TIMEBOX`, and null where the corpus
    // declares none. `docs/framework/metadata.md` carries the notation.
    public string? Shortcode;

    // What a person calls this corpus, and what it is. Both travel into the plugin manifest and the package
    // description. A corpus stating neither gets a sentence built from its own name.
    public string? DisplayName;
    public string? Description;

    // Who publishes this corpus, and under what licence. Null where the corpus has not said, so nothing is asserted
    // about somebody who did not say it.
    public string? AuthorName;
    public string? AuthorUrl;
    public string? License;

    // How long a shortcode may be. `.schema/_checks.yaml` gives the reason for each part of the spelling under
    // `shortcode`.
    public const int ShortcodeMin = 2;
    public const int ShortcodeMax = 8;

    // How a shortcode is spelled wrong, or null where it is spelled correctly. The wording completes "shortcode 'x'
    // ..." and names the first fault alone. `validate` and `restore` both ask it.
    public static string? ShortcodeFault(string shortcode)
    {
        if (shortcode.Length < ShortcodeMin) return "is too short";
        if (shortcode.Length > ShortcodeMax) return "is too long";
        if (!char.IsAsciiLetterLower(shortcode[0])) return "does not open on a lower-case letter";

        return shortcode.Any(c => !char.IsAsciiLetterLower(c) && !char.IsAsciiDigit(c))
            ? "carries something other than a lower-case letter or a digit"
            : null;
    }

    // How this corpus is published, and the URL a person opens to browse it. `Publishing` holds the link-building rules
    // for each target. Both are null where the descriptor states none, and an export from such a corpus carries no
    // links.
    public string? PublishingTarget;
    public string? Base;

    // Where to report a problem with the framework itself: `kac`, the schema, the template or a skill. Takes the same
    // values `PublishingTarget` does, and is `none` where the descriptor states none. `docs/corpus-descriptor.md` sets
    // this beside `upstream:`.
    public string FrameworkTarget = Publishing.None;
    public string? FrameworkBase;
    public string? FrameworkArea;

    // Where work about this corpus's own records is filed. Null where the descriptor states no block, and `Tracker.Own`
    // then derives the pair from `publishing:`.
    public string? TrackerTarget;
    public string? TrackerBase;

    // The Azure DevOps area path a filed work item belongs to, written below the project and separated by `/`, or null
    // for the project's default area. `Tracker.For` drops it beside any other target.
    public string? TrackerArea;

    // Where the corpus root sits inside the published repository, and null where the corpus is the repository. Kept
    // apart from the base, because the commit a link resolves against goes between the two.
    public string? PathPrefix;

    // Where this estate's code repositories are addressed from: a service's `repos` entry resolves to
    // `<ReposBase>/<entry>`. Null where the corpus states none, and `mirrors-repo-links` then checks nothing.
    public string? ReposBase;

    // What an export leaves behind: `draft`, `overdue`, or neither. Empty by default.
    public readonly List<string> ExportExclude = [];

    // The two things an export can be told to leave behind. The exporter reads them from here, so a corpus naming a
    // third is told.
    public const string ExcludeDraft = "draft";
    public const string ExcludeOverdue = "overdue";
    public static readonly IReadOnlyList<string> Excludable = [ExcludeDraft, ExcludeOverdue];

    // The three versions the descriptor states, each null where it states none. `DescriptorVersion` is this file's
    // format and `TemplateVersion` the template last taken, and an update stamps both. `ContentVersion` is the corpus's
    // own semantic version, which only a person writes.
    public int? DescriptorVersion;
    public string? ContentVersion;
    public int? TemplateVersion;

    // The types this corpus has adopted, named as the schema names them. Null where the descriptor says nothing, which
    // differs from an empty list: the types are then read off the corpus's own folders.
    public List<string>? Types;

    // The ranges this corpus states, keyed as a field's `values: $corpus.<name>` names them. See
    // docs/corpus-descriptor.md.
    public readonly Dictionary<string, IReadOnlyList<string>> Enums = new(StringComparer.Ordinal);

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
        // Trimmed, because a folded scalar keeps the newline that closed it and these values are written into JSON.
        descriptor.DisplayName = Said(Yaml.Str(Yaml.Get(root, "display-name")));
        descriptor.Description = Said(Yaml.Str(Yaml.Get(root, "description")));
        descriptor.License = Said(Yaml.Str(Yaml.Get(root, "license")));
        var author = Yaml.Get(root, "author");
        descriptor.AuthorName = Said(Yaml.Str(Yaml.Get(author, "name")));
        descriptor.AuthorUrl = Said(Yaml.Str(Yaml.Get(author, "url")));
        descriptor.ReposBase = Blank(Yaml.Str(Yaml.Get(root, "repos-base")));
        descriptor.PublishingTarget = Yaml.Str(Yaml.Get(root, "publishing-target"));
        var publishing = Yaml.Get(root, "publishing");
        descriptor.Base = Yaml.Str(Yaml.Get(publishing, "base"));
        descriptor.PathPrefix = Yaml.Str(Yaml.Get(publishing, "path-prefix"));
        var framework = Yaml.Get(root, "framework");
        descriptor.FrameworkTarget = Blank(Yaml.Str(Yaml.Get(framework, "target"))) ?? Publishing.None;
        descriptor.FrameworkBase = Blank(Yaml.Str(Yaml.Get(framework, "base")));
        descriptor.FrameworkArea = Blank(Yaml.Str(Yaml.Get(framework, "area")));
        var tracker = Yaml.Get(root, "tracker");
        descriptor.TrackerTarget = Blank(Yaml.Str(Yaml.Get(tracker, "target")));
        descriptor.TrackerBase = Blank(Yaml.Str(Yaml.Get(tracker, "base")));
        descriptor.TrackerArea = Blank(Yaml.Str(Yaml.Get(tracker, "area")));
        descriptor.ExportExclude.AddRange(Yaml.StrList(Yaml.Get(Yaml.Get(root, "export"), "exclude")));
        descriptor.PluginFrom = Blank(Yaml.Str(Yaml.Get(Yaml.Get(root, "plugin"), "from")));

        if (Yaml.Get(root, "types") is YamlSequenceNode types)
            descriptor.Types = [.. types.Children.Select(Yaml.Str).OfType<string>()];

        foreach (var (name, node) in Yaml.Map(Yaml.Get(root, "enums")))
            descriptor.Enums[name] = Yaml.StrList(node);

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

        // A key written with no value parses as an empty scalar, which means the corpus said nothing.
        static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

        static string? Said(string? value) => Blank(value)?.Trim();
    }

    // What to tell an author whose descriptor still uses a renamed key, or null where none is in use. `update` stops on
    // it.
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

    // Rewrite the `types:` block, replacing its items whole and leaving everything around them. Line-oriented for the
    // reason `Stamp` is. A descriptor stating no `types:` is left alone.
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

    // Record what an update took: the format this tool writes, the commit it resolved, the template's version, and the
    // day it arrived. `content-version` is left alone. `commit` is null where the template was read from a folder, and
    // the key is then left as it stands.
    //
    // This rewrites lines rather than re-serialising the file, because the descriptor is mostly commentary. Someone
    // opens it to read what each key means and when owning a file is worth declaring, and a YAML round-trip would throw
    // all of that away. Rewriting a line does drop any trailing comment on it, which is right: that comment described
    // the value the update just replaced.
    public static void Stamp(string corpusRoot, int templateVersion, string takenOn, string? commit = null)
    {
        var path = Path.Combine(corpusRoot, ".corpus.yaml");
        var lines = File.Exists(path)
            ? new List<string>(Files.ReadLf(path).Split('\n'))
            : [];

        // A descriptor without the key takes it above its first key, below whatever header comment stands there.
        const string formatKey = "descriptor-version:";
        var format = lines.FindIndex(l => l.StartsWith(formatKey, StringComparison.Ordinal));
        if (format >= 0) lines[format] = $"{formatKey} {Format}";
        else
        {
            var first = lines.FindIndex(IsTopLevelKey);
            lines.Insert(first < 0 ? lines.Count : first, $"{formatKey} {Format}");
        }

        // A descriptor with no `upstream:` block has never been synced, so this opens one.
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

        static string Line(string key, string value) => $"  {key}: {value}";

        static (string key, string value)[] Stamped(int version, string on, string? head)
        {
            var keys = new List<(string, string)>();
            if (head is { Length: > 0 }) keys.Add(("commit", head));
            keys.Add(("template-version", version.ToString()));
            keys.Add(("taken-on", $"\"{on}\""));
            return [.. keys];
        }
    }

    // Record the version each consumed corpus was resolved at, on that corpus's own entry. An entry the map does not
    // name is left as it stands. Line-oriented for the reason `Stamp` is. Returns the corpora whose lock was written,
    // because a flow-style sequence the YAML loader accepts is a shape this pass does not read.
    public static IReadOnlyList<string> SetResolved(
        string corpusRoot, IReadOnlyDictionary<string, string> versions)
    {
        var written = new List<string>();
        var path = Path.Combine(corpusRoot, ".corpus.yaml");
        if (!File.Exists(path) || versions.Count == 0) return written;

        var before = Files.ReadLf(path);
        var lines = new List<string>(before.Split('\n'));
        var start = lines.FindIndex(l => l.StartsWith("consumes:", StringComparison.Ordinal));
        if (start < 0) return written;

        // Walked from the end, so an insertion never moves a line this loop has yet to read.
        var entries = Entries(lines, start);
        for (var i = entries.Count - 1; i >= 0; i--)
        {
            var entry = entries[i];
            if (entry.Corpus is not { } name || !versions.TryGetValue(name, out var version)) continue;

            var line = $"{new string(' ', entry.Indent)}resolved: {Quoted(version)}";
            if (entry.Resolved is { } at) lines[at] = line;
            else lines.Insert(entry.End, line);
            written.Add(name);
        }

        // Written only where a line changed, so a restore that found every import current leaves the file untouched.
        var after = string.Join('\n', lines).TrimEnd('\n') + "\n";
        if (!after.Equals(before, StringComparison.Ordinal)) File.WriteAllText(path, after);

        return written;
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

            // A dash opens an entry and usually carries its first key, so the column after it is where every key of the
            // entry is written. A dash standing alone gives the same column.
            if (trimmed.StartsWith("- ", StringComparison.Ordinal) || trimmed == "-")
            {
                if (indent > 0) entries.Add((corpus, indent, resolved, end));
                corpus = null;
                resolved = null;
                indent = lines[i].Length - trimmed.Length + 2;
                trimmed = trimmed.Length > 1 ? trimmed[2..] : "";
            }
            else if (indent == 0) continue;

            if (Key(trimmed, "corpus") is { } named) corpus = named;
            if (trimmed.StartsWith("resolved:", StringComparison.Ordinal)) resolved = i;
            end = i + 1;
        }

        if (indent > 0) entries.Add((corpus, indent, resolved, end));
        return entries;

        // What a key on this line says, or null where the line says something else. A trailing comment is dropped and
        // quotes come off, so the value matches what the YAML loader parsed.
        static string? Key(string line, string key)
        {
            if (!line.StartsWith(key + ":", StringComparison.Ordinal)) return null;

            var value = line[(key.Length + 1)..];

            var comment = value.IndexOf(" #", StringComparison.Ordinal);
            if (comment >= 0) value = value[..comment];

            return value.Trim().Trim('"', '\'') is { Length: > 0 } v ? v : null;
        }
    }

    private static string Quoted(string version) => $"\"{version}\"";

    private static bool IsTopLevelKey(string line) =>
        line.Length > 0 && !char.IsWhiteSpace(line[0]) && line[0] != '#';
}
