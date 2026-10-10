using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace kac.core;

public static class Yaml
{
    public static YamlNode LoadFile(string path) => Load(File.ReadAllText(path));

    public static YamlNode Load(string text)
    {
        var stream = new YamlStream();
        stream.Load(new StringReader(text));
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

    // A plain `null` or `~` reads as absent, so the schema's `folder: null` is no folder rather than one named
    // "null". Plain style only, so a quoted "null" is still the string.
    public static string? Str(YamlNode? node)
    {
        if (node is not YamlScalarNode sc) return null;
        return sc is { Style: ScalarStyle.Plain, Value: "" or "~" or "null" or "Null" or "NULL" } ? null : sc.Value;
    }

    public static bool Bool(YamlNode? node)
        => Bool(node, false);

    // YAML 1.1 spellings count, so a schema writing `no` or `off` switches something off. Anything else falls back.
    public static bool Bool(YamlNode? node, bool fallback)
        => (node as YamlScalarNode)?.Value?.ToLowerInvariant() switch
        {
            "true" or "yes" or "on" => true,
            "false" or "no" or "off" => false,
            _ => fallback
        };

    public static int Int(YamlNode? node, int fallback)
        => int.TryParse((node as YamlScalarNode)?.Value, out var v) ? v : fallback;

    // Null where the key is absent, because a floor of zero is not the absence of one.
    public static int? NullableInt(YamlNode? node)
        => int.TryParse((node as YamlScalarNode)?.Value, out var v) ? v : null;

    public static List<string> StrList(YamlNode? node)
    {
        var result = new List<string>();
        if (node is YamlSequenceNode seq)
            foreach (var item in seq.Children)
                if (item is YamlScalarNode s)
                    result.Add(s.Value ?? "");
        return result;
    }

    // A scalar's value exactly as written, with none of the readings `Str` applies. Frontmatter is read through
    // this, so a finding can quote back what the author typed.
    public static string? Raw(YamlNode node) => (node as YamlScalarNode)?.Value;

    // The node's line in the document. The parser counts from 1 within the frontmatter block, and `frontStart` is
    // the line the block opens on. A node with no position falls back to `frontStart`.
    public static int? LineOf(YamlNode node, int frontStart)
        => node.Start.Line > 0 ? (int)node.Start.Line + frontStart - 1 : frontStart;
}
