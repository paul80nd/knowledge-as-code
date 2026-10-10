using System.Text.RegularExpressions;
using kac.core;
using Xunit.Sdk;

namespace kac.tests;

/// <summary>
/// A generated block inside a hand-written page, between the <c>Markers</c> pair. A page with no blocks gets an empty
/// pair under its first heading; one with other blocks is refused, as nothing says which heading the new one goes
/// under. An unclosed block is refused because <c>SpliceBlock</c> returns that page untouched, which reads as up to
/// date.
/// </summary>
internal static partial class GeneratedPage
{
    /// <summary>The first heading of a page, under which a missing marker pair is inserted.</summary>
    [GeneratedRegex(@"^# .*$", RegexOptions.Multiline)]
    private static partial Regex Heading();

    [GeneratedRegex(@"^<!-- BEGIN GENERATED: ", RegexOptions.Multiline)]
    private static partial Regex AnyBlock();

    /// <summary>The page is returned unchanged where it already holds <c>body</c>.</summary>
    internal static string Replaced(string page, string name, string body)
    {
        var begin = Markers.Begin(name);
        var end = Markers.End(name);
        var from = page.IndexOf(begin, StringComparison.Ordinal);

        if (from >= 0 && page.IndexOf(end, from, StringComparison.Ordinal) < 0)
            throw new XunitException(
                $"kac.tests: the block '{name}' opens and never closes. Put its '{end}' line back, or "
                + $"delete its '{begin}' line and let the block be written again.");

        if (from < 0)
        {
            if (AnyBlock().IsMatch(page))
                throw new XunitException(
                    $"kac.tests: the page carries generated blocks and none of them is '{name}'. Put its "
                    + $"'{begin}' and '{end}' lines back, under the heading that introduces it. Only a page "
                    + "carrying no block at all is given one automatically, because nothing here can tell which "
                    + "heading a missing block belonged under.");

            var heading = Heading().Match(page);
            if (!heading.Success)
                throw new XunitException(
                    $"kac.tests: the page carrying '{name}' has no heading to put a generated block under.");

            var at = heading.Index + heading.Length;
            page = page[..at] + $"\n\n{begin}\n{end}" + page[at..];
        }

        return Markers.SpliceBlock(page, name, body);
    }
}
