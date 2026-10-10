namespace kac.core;

// A template this tool cannot read the whole of: a file its own manifest cannot place, and a rule serving
// a continuous integration system the tool does not offer. Either one stops `new` and `update`.
public sealed record TemplateFaults(IReadOnlyList<string> Unclassified, IReadOnlyList<string> UnknownCi)
{
    public bool Unsound => Unclassified.Count > 0 || UnknownCi.Count > 0;

    // Sorted, so `new` and `update` name the systems in the same order.
    public static TemplateFaults Of(IEnumerable<string> unclassified, IEnumerable<string> unknownCi) =>
        new([.. unclassified], [.. unknownCi.Order(StringComparer.Ordinal)]);
}
