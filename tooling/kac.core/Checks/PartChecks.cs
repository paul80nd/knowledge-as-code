namespace kac.core;

// A record's addressable parts — the children something else may cite by name. The type declares where
// they live and what holds them to shape, so nothing here is specific to policies or to glossaries
// except by way of that declaration.
//
// Three questions run across both sources, each read off whatever that source writes its parts in.
// First, the section holds at least one part. A record promising parts and carrying none says nothing
// where it undertook to say something, which `clause-table` reads off an empty table and `part-none`
// off a section with no headings. Second, each part offers an id, and no two parts of a record share
// one, because a citation reaching two children names neither. Third, a part written as a heading
// carries something beneath it, since a heading is an address and a container both. That third one is
// `part-empty`, and the table source needs no equivalent: a row with an empty clause cell is stopped by
// `clause-modal` before anyone asks.
//
// Everything else below belongs to the table source, whose rows carry cells a heading has none of: the
// table can be missing or mis-headed, and each row carries the modal that says whether it obliges. A
// heading is its own id and offers no cells to be wrong, so a type sourcing headings stops there.
//
// Runs only for a type whose schema declares a `parts:` block, and only once the section itself is
// present: a missing section is `required-section`'s to report, and saying it twice would make one fault
// look like two.
public static class PartChecks
{
    public static void Check(Doc d, TypeSchema t, Report report)
    {
        if (t.Parts is not { } spec) return;
        if (!d.Sections.Any(s => string.Equals(s.Title, spec.Section, StringComparison.OrdinalIgnoreCase))) return;

        if (spec.Source == PartSpec.Table && !TableIsSound(d, spec, report)) return;

        if (spec.Source == PartSpec.Headings && !SectionIsPopulated(d, spec, report)) return;

        CheckAddresses(d, spec, report);

        if (spec.Source == PartSpec.Headings) CheckBodies(d, spec, report);
        if (spec.Source == PartSpec.Table) CheckObligations(d, spec, report);
    }

    // Reported and stopped, for the reason a broken table is: the checks below read parts, and a section
    // holding none gives them nothing to say.
    //
    // The message is built from the declaration, so it names the section, the word the type calls its
    // parts and the level to write them at, whatever type is asking.
    private static bool SectionIsPopulated(Doc d, PartSpec spec, Report report)
    {
        if (d.Parts.Count > 0) return true;

        report.Err(new CheckId("part-none"),
            $"the '## {spec.Section}' section holds no {spec.Noun}s — write each one as an H{spec.Level} heading.",
            d.PartSectionLine);
        return false;
    }

    // Every part is asked, including the last, whose body runs to the end of the document rather than to
    // a heading that closes it.
    //
    // `Md.HasContent` reads the source as written rather than the rendered blocks, which is what makes a
    // horizontal rule or an em dash standing in for the definition count as nothing written. The
    // heading is quoted into the message, because a record holds many entries and the author needs to
    // know which one is empty.
    private static void CheckBodies(Doc d, PartSpec spec, Report report)
    {
        foreach (var row in d.Parts.Where(row => !Md.HasContent(row.Body(d.Text))))
            report.Err(new CheckId("part-empty"),
                $"{spec.Noun} '{Md.Snippet(row.Text)}' has nothing under it — write it or delete the heading.",
                row.Line);
    }

    // A missing table, a mis-headed one and an empty one are three different faults the parser has
    // already told apart. Each is reported and stops the pass, because nothing beneath a broken table
    // can be judged.
    private static bool TableIsSound(Doc d, PartSpec spec, Report report)
    {
        var headers = string.Join(" | ", spec.Columns);

        if (d.PartTableHeaders is null)
        {
            report.Err(new CheckId("clause-table"),
                $"the '## {spec.Section}' section holds no table — write one row per obligation, headed '{headers}'.",
                d.PartSectionLine);
            return false;
        }

        if (!d.PartTableHeaders.SequenceEqual(spec.Columns, StringComparer.Ordinal))
        {
            report.Err(new CheckId("clause-table"),
                $"the clause table is headed '{string.Join(" | ", d.PartTableHeaders)}' — it must be headed "
                + $"'{headers}'.", d.PartTableLine);
            return false;
        }

        if (d.Parts.Count == 0)
        {
            report.Err(new CheckId("clause-table"),
                "the clause table has no rows — a record that binds nothing binds nobody.", d.PartTableLine);
            return false;
        }

        return true;
    }

    // The address itself, asked of both sources.
    //
    // A table row's id is written by the author, so it can be written as something other than a code
    // span or fail the type's pattern. Both are `clause-id-format`, and neither can be asked of a
    // heading: a heading is its own id, and the slug admits whatever the heading says. A
    // heading that slugs to nothing — punctuation alone — offers no address and is passed over, which is
    // the same answer a link to it would get from `fragment-resolves`.
    //
    // Uniqueness is asked of both, ordinally, because `pol-SCRT.LOGS` and `pol-SCRT.logs` differing only
    // in case is not two parts a reader could tell apart either.
    private static void CheckAddresses(Doc d, PartSpec spec, Report report)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var row in d.Parts)
        {
            if (row.Id is null)
            {
                if (spec.Source == PartSpec.Table)
                    report.Err(new CheckId("clause-id-format"),
                        $"clause id '{Md.Snippet(row.IdText)}' is not a code span — write it as "
                        + $"`{Md.Snippet(row.IdText)}`.", row.Line);
                continue;
            }

            if (spec is { Source: PartSpec.Table, IdPatternRegex: { } idPattern } && !idPattern.IsMatch(row.Id))
                report.Err(new CheckId("clause-id-format"),
                    $"clause id '{row.Id}' does not match {spec.IdPattern}.", row.Line);

            if (!seen.Add(row.Id))
                report.Err(new CheckId("part-id-unique"),
                    $"two {spec.Noun}s here address as '{row.Id}' — a citation of it names both and reaches neither.",
                    row.Line);
        }
    }

    // What a clause table's rows carry beyond an address: the modal that says whether the row obliges,
    // how it is written, and where it sits among the rest.
    private static void CheckObligations(Doc d, PartSpec spec, Report report)
    {
        var highest = 0;
        var disordered = false;

        foreach (var row in d.Parts)
        {
            // The modal is the binding level, so a row without one is a sentence rather than an
            // obligation and nothing below it can be judged either.
            var modal = spec.ModalsLongestFirst.FirstOrDefault(m => row.Text.StartsWith(m, StringComparison.Ordinal));
            if (modal is null)
            {
                report.Err(new CheckId("clause-modal"),
                    $"clause '{Md.Snippet(row.Text)}' does not open with a modal — write one of "
                    + $"{string.Join(", ", spec.Levels)}.", row.Line);
                continue;
            }

            // Bold carries the binding level visually, and the reader skimming a long table reads the
            // weight before the words. A binding modal that is not bold reads as advice; an advisory one
            // that is bold reads as an obligation. Both are the wrong document.
            var binds = spec.Binding.Contains(modal, StringComparer.Ordinal);
            if (binds && !string.Equals(row.BoldLead, modal, StringComparison.Ordinal))
                report.Err(new CheckId("clause-modal"), $"'{modal}' binds — write it bold, `**{modal}**`.", row.Line);
            else if (!binds && row.BoldLead is not null)
                report.Err(new CheckId("clause-modal"),
                    $"'{modal}' does not bind — write it plain, not bold.", row.Line);

            // A second modal in the same row is two obligations sharing one id, so a citation of it can
            // only ever name half of what it means.
            var rest = row.Text[modal.Length..];
            if (spec.ModalsLongestFirst.FirstOrDefault(m => rest.Contains(m, StringComparison.Ordinal)) is { } second)
                report.Warn(new CheckId("clause-compound"),
                    $"clause '{row.Id ?? row.IdText}' carries a second '{second}' — one obligation per clause, or the "
                    + "citation is ambiguous.", row.Line);

            // Reported once, against the first row that breaks the grouping: a table sorted wholly the
            // wrong way would otherwise report every row after the first.
            var rank = spec.Rank(modal);
            if (rank < highest && !disordered)
            {
                report.Warn(new CheckId("clause-order"),
                    $"clause '{row.Id ?? row.IdText}' is a '{modal}' but follows a '{spec.Levels[highest]}' — group "
                    + $"the table {string.Join(", ", spec.Levels)}.", row.Line);
                disordered = true;
            }

            highest = Math.Max(highest, rank);
        }
    }

    // The third way a citation fails, beside naming no document and naming no part: separating the two
    // with a colon. Reported under `part-ref` because a reader meets one question — does this citation
    // reach the thing it claims to — and the separator is the first way of answering no.
    //
    // Every document is asked, including the ones offering no parts of their own: a citation is written
    // where the part is answered, which is rarely a record of the same type. The other two ways need
    // `byId` and run over the corpus. This one is legible in the single file, so it sits here.
    //
    // Left unchecked it is silent. The parser reads a citation by its separator, so a colon-separated
    // one is never collected, the resolution checks never see it, and the build passes on a reference
    // the reader has every reason to trust.
    public static void CheckNotation(Doc d, Report report)
    {
        foreach (var (citation, line) in d.ColonCitations)
            report.Err(new CheckId("part-ref"),
                $"'{citation}' separates the two halves with a colon — write '{citation.Replace(':', '.')}'.", line);
    }
}
