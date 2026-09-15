using System.Text.RegularExpressions;
using kac.core;
using Xunit.Sdk;

namespace kac.tests;

// A generated block inside a hand-written documentation page, and the splice that keeps it honest.
//
// A hand-written table drifts from what it describes inside two pull requests and nothing notices, so a page states
// the parts it cannot be trusted to restate and a test writes them. `Markers` is the same pair `kac generate` writes
// between, so a page of the documentation and a page a corpus holds are filled by one piece of code.
//
// A page carrying no block at all is given an empty pair under its first heading, which is what a command page added
// to `docs/cli/` arrives as. A page already carrying one is not: its author placed those markers under the headings
// that introduce them, and a page with several blocks gives this no way to tell which heading a missing one belongs
// under. Guessing puts a table above the prose that introduces it and every test still passes, so a missing block on
// such a page stops the run and asks for the markers back.
//
// A block that opens and never closes stops the run too. `SpliceBlock` hands back a page it could not splice
// untouched, and an untouched page is what the caller reads as up to date, so the block would freeze where it stood.
// Writing a second pair above it is worse again: the orphan and the content under it stay on the page, and
// `Markers.Authored` reads everything past an unmatched marker as prose somebody wrote. Which of the two markers went
// is a question for whoever deleted one.
internal static partial class GeneratedPage
{
    // The first heading of a page, under which a missing marker pair is inserted.
    [GeneratedRegex(@"^# .*$", RegexOptions.Multiline)]
    private static partial Regex Heading();

    // Any generated block, whatever its name. What tells a page that has never carried one from a page that has.
    [GeneratedRegex(@"^<!-- BEGIN GENERATED: ", RegexOptions.Multiline)]
    private static partial Regex AnyBlock();

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
