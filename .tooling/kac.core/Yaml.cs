using YamlDotNet.RepresentationModel;

// ---------------------------------------------------------------------------
// YAML helpers over the representation model
// ---------------------------------------------------------------------------

namespace kac.core;

public static class Yaml
{
    public static YamlNode LoadFile(string path)
    {
        var stream = new YamlStream();
        stream.Load(new StringReader(File.ReadAllText(path)));
        return stream.Documents.Count > 0 ? stream.Documents[0].RootNode : new YamlMappingNode();
    }

    public static YamlNode? Get(YamlNode? node, string key)
    {
        if (node is YamlMappingNode map)
            foreach (var kv in map.Children)
                if (kv.Key is YamlScalarNode k && k.Value == key)
                    return kv.Value;
        return null;
    }

    public static IEnumerable<(string, YamlNode)> Map(YamlNode? node)
    {
        if (node is YamlMappingNode map)
            foreach (var kv in map.Children)
                yield return (((YamlScalarNode)kv.Key).Value ?? "", kv.Value);
    }

    public static string? Str(YamlNode? node) => (node as YamlScalarNode)?.Value;

    public static bool Bool(YamlNode? node)
        => (node as YamlScalarNode)?.Value?.ToLowerInvariant() is "true" or "yes";

    public static int Int(YamlNode? node, int fallback)
        => int.TryParse((node as YamlScalarNode)?.Value, out var v) ? v : fallback;

    public static List<string> StrList(YamlNode? node)
    {
        var result = new List<string>();
        if (node is YamlSequenceNode seq)
            foreach (var item in seq.Children)
                if (item is YamlScalarNode s)
                    result.Add(s.Value ?? "");
        return result;
    }
}
