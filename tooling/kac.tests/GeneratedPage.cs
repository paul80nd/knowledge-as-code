using System.Text.RegularExpressions;
using kac.core;
using Xunit.Sdk;

namespace kac.tests;

// A generated block inside a hand-written documentation page, and the splice that keeps it honest.
//
// A hand-written table drifts from what it describes inside two pull requests and nothing notices, so a page states
// the parts it cannot be trusted to restate and a test writes them. `Markers` is the same pair `kac generate` writes
// between, so a reader meets one convention wherever a block appears.
//
// The block is inserted under the page's first heading where it is absent, so adding one is adding the test that
// writes it. An opened block that never closes is an error rather than an insertion: the page already states where
// the content belongs, and writing a second block would leave the reader two.
internal static partial class GeneratedPage
{
    // The first heading of a page, under which a missing marker pair is inserted.
    [GeneratedRegex(@"^# .*$", RegexOptions.Multiline)]
    private static partial Regex Heading();

    // The page with `name`'s block holding `body`. The page is returned unchanged where it already does.
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
