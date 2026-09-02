namespace kac.core;

// A template this tool cannot read the whole of: a file its own manifest cannot place, and a rule serving
// a continuous integration system the tool does not offer.
//
// `new` and `update` collect the same two, because a template is unsound for the same reasons whichever
// verb read it. Both plans carry one of these, so the command that reports them writes one message.
//
// Either one stops the run rather than being worked around, because each is a defect upstream, and acting
// anyway means a corpus receives a file nobody meant to send or loses one nobody meant to withhold.
public sealed record TemplateFaults(IReadOnlyList<string> Unclassified, IReadOnlyList<string> UnknownCi)
{
    public bool Unsound => Unclassified.Count > 0 || UnknownCi.Count > 0;

    // Both plans walk the template in path order, so a system is met wherever its first rule sits. Named
    // in sorted order instead, because a reader should meet them the same way whichever verb they ran.
    //
    // Both lists are copied, so what a caller still holds can neither reorder nor extend what is reported.
    public static TemplateFaults Of(IEnumerable<string> unclassified, IEnumerable<string> unknownCi) =>
        new([.. unclassified], [.. unknownCi.Order(StringComparer.Ordinal)]);
}
