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

    /// <summary>
    /// A plain <c>null</c> or <c>~</c> reads as absent, so the schema's <c>folder: null</c> is no folder rather than
    /// one named "null". Plain style only, so a quoted "null" is still the string.
    /// </summary>
    public static string? Str(YamlNode? node)
    {
        if (node is not YamlScalarNode sc) return null;
        return sc is { Style: ScalarStyle.Plain, Value: "" or "~" or "null" or "Null" or "NULL" } ? null : sc.Value;
    }

    public static bool Bool(YamlNode? node)
        => Bool(node, false);

    /// <summary>
    /// YAML 1.1 spellings count, so a schema writing <c>no</c> or <c>off</c> switches something off. Anything else
    /// falls back.
    /// </summary>
    public static bool Bool(YamlNode? node, bool fallback)
        => (node as YamlScalarNode)?.Value?.ToLowerInvariant() switch
        {
            "true" or "yes" or "on" => true,
            "false" or "no" or "off" => false,
            _ => fallback
        };

    public static int Int(YamlNode? node, int fallback)
        => int.TryParse((node as YamlScalarNode)?.Value, out var v) ? v : fallback;

    /// <summary>Null where the key is absent, because a floor of zero is not the absence of one.</summary>
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

    /// <summary>
    /// A scalar's value exactly as written, with none of the readings <c>Str</c> applies. Frontmatter is read through
    /// this, so a finding can quote back what the author typed.
    /// </summary>
    public static string? Raw(YamlNode node) => (node as YamlScalarNode)?.Value;

    /// <summary>
    /// The node's line in the document. The parser counts from 1 at the line below the opening <c>---</c>, and
    /// <c>frontStart</c> is the line that <c>---</c> sits on. A node with no position falls back to <c>frontStart</c>.
    /// </summary>
    public static int LineOf(YamlNode node, int frontStart)
        => node.Start.Line > 0 ? (int)node.Start.Line + frontStart : frontStart;
}
