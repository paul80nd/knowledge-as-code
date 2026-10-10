namespace kac.core;

/// <summary>
/// A template this tool cannot read the whole of: a file its own manifest cannot place, and a rule serving a continuous
/// integration system the tool does not offer. Either one stops <c>new</c> and <c>update</c>.
/// </summary>
public sealed record TemplateFaults(IReadOnlyList<string> Unclassified, IReadOnlyList<string> UnknownCi)
{
    public bool Unsound => Unclassified.Count > 0 || UnknownCi.Count > 0;

    /// <summary>Sorted, so <c>new</c> and <c>update</c> name the systems in the same order.</summary>
    public static TemplateFaults Of(IEnumerable<string> unclassified, IEnumerable<string> unknownCi) =>
        new([.. unclassified], [.. unknownCi.Order(StringComparer.Ordinal)]);
}
