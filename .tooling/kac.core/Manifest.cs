using YamlDotNet.RepresentationModel;

namespace kac.core;
// ---------------------------------------------------------------------------
// Portability manifest & mechanism sync state
// ---------------------------------------------------------------------------

// One rule from knowledge-as-code/manifest.yaml: a set of path globs that all resolve to one layer.
public record ManifestRule(IReadOnlyList<string> Patterns, string Layer);

public class Manifest
{
    public List<ManifestRule> Rules = [];

    public static Manifest Load(string repoRoot)
    {
        var m = new Manifest();
        var root = Yaml.LoadFile(Path.Combine(repoRoot, "knowledge-as-code", "manifest.yaml"));
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

public class MechanismLock
{
    public string Role = "";
    public string? UpstreamUrl;
    public readonly List<AcceptedDivergence> Accepted = [];

    // The types this corpus has adopted, named as the schema names them. This is a statement of intent
    // rather than a description: the corpus says which of the framework's types it wants, and everything
    // else follows from that — what is generated, what a sync brings down, and what `validate` holds the
    // corpus to having stood up.
    //
    // Null where the lock says nothing, which is not the same as an empty list. A corpus that has not
    // declared yet is read off its own folders instead, so adopting the key is a change a corpus makes
    // when it is ready rather than one the tool forces on the version that arrives without it.
    public List<string>? Types;

    // Whether a type is this corpus's. An undeclared corpus answers yes to everything and leaves the
    // question to the filesystem; the callers that ask are the ones that already know what is on disk.
    public bool Adopted(string type) => Types is null || Types.Contains(type, StringComparer.Ordinal);

    public static MechanismLock Load(string repoRoot)
    {
        var path = Path.Combine(repoRoot, ".mechanism.lock");
        var lockFile = new MechanismLock();
        if (!File.Exists(path)) return lockFile;

        var root = Yaml.LoadFile(path);
        lockFile.Role = Yaml.Str(Yaml.Get(root, "role")) ?? "";
        var url = Yaml.Str(Yaml.Get(Yaml.Get(root, "upstream"), "url"));
        lockFile.UpstreamUrl = string.IsNullOrWhiteSpace(url) ? null : url;

        if (Yaml.Get(root, "types") is YamlSequenceNode types)
            lockFile.Types = [.. types.Children.Select(Yaml.Str).OfType<string>()];

        if (Yaml.Get(root, "accepted-divergences") is YamlSequenceNode seq)
            foreach (var item in seq.Children)
                if (Yaml.Str(Yaml.Get(item, "path")) is { } p)
                    lockFile.Accepted.Add(new AcceptedDivergence(p, Yaml.Str(Yaml.Get(item, "reason"))));

        return lockFile;
    }
}
