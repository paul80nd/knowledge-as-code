using YamlDotNet.RepresentationModel;

namespace kac.core;
// ---------------------------------------------------------------------------
// Portability manifest & mechanism sync state
// ---------------------------------------------------------------------------

// One rule from .tooling/manifest.yaml: a set of path globs that all resolve to one layer.
public record ManifestRule(IReadOnlyList<string> Patterns, string Layer);

public class Manifest
{
    public List<ManifestRule> Rules = [];

    // The mechanism's version, which a sync stamps into the receiving corpus's descriptor. Read from the
    // reference rather than assumed, so a corpus taking from an older upstream records what it took.
    public int Version;

    public static Manifest Load(string repoRoot)
    {
        var m = new Manifest();
        var root = Yaml.LoadFile(Path.Combine(repoRoot, ".tooling", "manifest.yaml"));
        if (int.TryParse(Yaml.Str(Yaml.Get(root, "version")), out var version)) m.Version = version;
        if (Yaml.Get(root, "rules") is YamlSequenceNode rules)
            foreach (var rule in rules.Children)
            {
                var pathNode = Yaml.Get(rule, "path");
                var patterns = pathNode is YamlSequenceNode
                    ? Yaml.StrList(pathNode)
                    : Yaml.Str(pathNode) is { } single ? [single] : [];
                var layer = Yaml.Str(Yaml.Get(rule, "layer"));
                if (patterns.Count > 0 && layer is not null)
                    m.Rules.Add(new ManifestRule(patterns, layer));
            }
        return m;
    }

    // First rule with a matching glob wins, mirroring the manifest's own "evaluated in order"
    // contract. Returns null when nothing matches — which the check reports as an error, since the
    // manifest is meant to resolve every file (its final rule is a catch-all).
    public string? Resolve(string relPath)
    {
        foreach (var rule in Rules)
        foreach (var pattern in rule.Patterns)
            if (Glob.IsMatch(relPath, pattern))
                return rule.Layer;
        return null;
    }
}

public record AcceptedDivergence(string Path, string? Reason);

public class CorpusDescriptor
{
    public string Role = "";
    public string? UpstreamUrl;
    public readonly List<AcceptedDivergence> Accepted = [];

    // The types this corpus has adopted, named as the schema names them. This is a statement of intent
    // rather than a description: the corpus says which of the framework's types it wants, and everything
    // else follows from that — what is generated, what a sync brings down, and what `validate` holds the
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

    public static CorpusDescriptor Load(string repoRoot)
    {
        var path = Path.Combine(repoRoot, ".corpus.yaml");
        var descriptor = new CorpusDescriptor();
        if (!File.Exists(path)) return descriptor;

        var root = Yaml.LoadFile(path);
        descriptor.Role = Yaml.Str(Yaml.Get(root, "role")) ?? "";
        var url = Yaml.Str(Yaml.Get(Yaml.Get(root, "upstream"), "url"));
        descriptor.UpstreamUrl = string.IsNullOrWhiteSpace(url) ? null : url;

        if (Yaml.Get(root, "types") is YamlSequenceNode types)
            descriptor.Types = [.. types.Children.Select(Yaml.Str).OfType<string>()];

        if (Yaml.Get(root, "accepted-divergences") is YamlSequenceNode seq)
            foreach (var item in seq.Children)
                if (Yaml.Str(Yaml.Get(item, "path")) is { } p)
                    descriptor.Accepted.Add(new AcceptedDivergence(p, Yaml.Str(Yaml.Get(item, "reason"))));

        return descriptor;
    }

    // Record what a sync took: the upstream's mechanism version, where it came from, and when.
    //
    // This rewrites three lines rather than re-serialising the file, because the descriptor is mostly
    // commentary. Someone opens it to read what each role means and when a divergence is worth
    // accepting, and a YAML round-trip would throw all of that away. Rewriting a line does drop any
    // trailing comment on it, which is right: that comment described the value the sync just replaced.
    public static void Stamp(string repoRoot, int mechanismVersion, string syncedFrom, string syncedOn)
    {
        var path = Path.Combine(repoRoot, ".corpus.yaml");
        var lines = File.Exists(path)
            ? new List<string>(Files.ReadLf(path).Split('\n'))
            : [];

        // A descriptor with no `upstream:` block has never been synced. Open one rather than fail — the
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
            foreach (var (key, value) in Stamped(mechanismVersion, syncedFrom, syncedOn))
                if (lines[i].TrimStart().StartsWith(key + ":", StringComparison.Ordinal))
                {
                    lines[i] = Line(key, value);
                    written.Add(key);
                }

        // Keys the block never held go in at its head, where they read as part of it.
        lines.InsertRange(start + 1, Stamped(mechanismVersion, syncedFrom, syncedOn)
            .Where(s => !written.Contains(s.key))
            .Select(s => Line(s.key, s.value)));

        File.WriteAllText(path, string.Join('\n', lines).TrimEnd('\n') + "\n");

        static string Line(string key, string value) => $"  {key + ":",-18} {value}";

        static (string key, string value)[] Stamped(int version, string from, string on) =>
            [("mechanism-version", version.ToString()), ("synced-from", from), ("synced-on", $"\"{on}\"")];

        static bool IsTopLevelKey(string line) => line.Length > 0 && !char.IsWhiteSpace(line[0]) && line[0] != '#';
    }
}
