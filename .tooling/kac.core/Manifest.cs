using YamlDotNet.RepresentationModel;

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
    public List<AcceptedDivergence> Accepted = [];

    public static MechanismLock Load(string repoRoot)
    {
        var path = Path.Combine(repoRoot, "knowledge-as-code", "mechanism.lock");
        var lockFile = new MechanismLock();
        if (!File.Exists(path)) return lockFile;

        var root = Yaml.LoadFile(path);
        lockFile.Role = Yaml.Str(Yaml.Get(root, "role")) ?? "";
        var url = Yaml.Str(Yaml.Get(Yaml.Get(root, "upstream"), "url"));
        lockFile.UpstreamUrl = string.IsNullOrWhiteSpace(url) ? null : url;

        if (Yaml.Get(root, "accepted-divergences") is YamlSequenceNode seq)
            foreach (var item in seq.Children)
                if (Yaml.Str(Yaml.Get(item, "path")) is { } p)
                    lockFile.Accepted.Add(new AcceptedDivergence(p, Yaml.Str(Yaml.Get(item, "reason"))));

        return lockFile;
    }
}
