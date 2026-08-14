// ---------------------------------------------------------------------------
// The clause table
// ---------------------------------------------------------------------------

namespace kac.core;

// The clause table — the section where a policy actually binds. Every other section describes,
// qualifies or explains; these rows are the obligations themselves, and each carries an id so a
// standard, a control or a deviation can cite the one it answers rather than the whole document. The
// type declares the section, the columns, the id pattern and the modals, so nothing here is specific to
// policies except by way of that declaration.
//
// Runs only for a type whose schema declares a `clauses:` block, and only once the section itself is
// present: a missing section is `required-section`'s to report, and saying it twice would make one
// fault look like two. A missing table, a mis-headed one and an empty one are three different faults
// the parser has already told apart; each is reported and stops the pass, because nothing beneath a
// broken table can be judged.
public static class ClauseChecks
{
    public static void Check(Doc d, TypeSchema t, Action<string, string, int?> err,
        Action<string, string, int?> warn)
    {
        if (t.Clauses is not { } spec) return;
        if (!d.Sections.Any(s => string.Equals(s.Title, spec.Section, StringComparison.OrdinalIgnoreCase))) return;

        var headers = string.Join(" | ", spec.Columns);

        if (d.ClauseHeaders is null)
        {
            err("clause-table", $"the '## {spec.Section}' section holds no table — write one row per "
                                + $"obligation, headed '{headers}'.", d.ClauseSectionLine);
            return;
        }

        if (!d.ClauseHeaders.SequenceEqual(spec.Columns, StringComparer.Ordinal))
        {
            err("clause-table", $"the clause table is headed '{string.Join(" | ", d.ClauseHeaders)}' — "
                                + $"it must be headed '{headers}'.", d.ClauseTableLine);
            return;
        }

        if (d.Clauses.Count == 0)
        {
            err("clause-table", "the clause table has no rows — a policy that binds nothing binds nobody.",
                d.ClauseTableLine);
            return;
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        var highest = 0;
        var disordered = false;

        foreach (var row in d.Clauses)
        {
            CheckId(row, spec, seen, err);

            // The modal is the binding level, so a row without one is a sentence rather than an
            // obligation and nothing below it can be judged either.
            var modal = spec.ModalsLongestFirst.FirstOrDefault(m => row.Text.StartsWith(m, StringComparison.Ordinal));
            if (modal is null)
            {
                err("clause-modal", $"clause '{Md.Snippet(row.Text)}' does not open with a modal — write one of "
                                    + $"{string.Join(", ", spec.Levels)}.", row.Line);
                continue;
            }

            // Bold carries the binding level visually, and the reader skimming a long table reads the
            // weight before the words. A binding modal that is not bold reads as advice; an advisory one
            // that is bold reads as an obligation. Both are the wrong document.
            var binds = spec.Binding.Contains(modal, StringComparer.Ordinal);
            if (binds && !string.Equals(row.BoldLead, modal, StringComparison.Ordinal))
                err("clause-modal", $"'{modal}' binds — write it bold, `**{modal}**`.", row.Line);
            else if (!binds && row.BoldLead is not null)
                err("clause-modal", $"'{modal}' does not bind — write it plain, not bold.", row.Line);

            // A second modal in the same row is two obligations sharing one id, so a citation of it can
            // only ever name half of what it means.
            var rest = row.Text[modal.Length..];
            if (spec.ModalsLongestFirst.FirstOrDefault(m => rest.Contains(m, StringComparison.Ordinal)) is { } second)
                warn("clause-compound", $"clause '{row.IdSpan ?? row.IdText}' carries a second '{second}' — "
                                        + "one obligation per clause, or the citation is ambiguous.", row.Line);

            // Reported once, against the first row that breaks the grouping: a table sorted wholly the
            // wrong way would otherwise report every row after the first.
            var rank = spec.Rank(modal);
            if (rank < highest && !disordered)
            {
                warn("clause-order", $"clause '{row.IdSpan ?? row.IdText}' is a '{modal}' but follows a "
                                     + $"'{spec.Levels[highest]}' — group the table "
                                     + $"{string.Join(", ", spec.Levels)}.", row.Line);
                disordered = true;
            }

            highest = Math.Max(highest, rank);
        }
    }

    // The third way a citation fails, beside naming no document and naming no clause: separating the
    // two with a colon. Reported under `clause-ref` because a reader meets one question — does this
    // citation reach the obligation it claims to — and the separator is the first way of answering no.
    //
    // Every document is asked, including the ones declaring no clause table of their own: a citation is
    // written where the obligation is answered, which is a standard, a control or a deviation. The
    // other two ways need `byId` and run over the corpus. This one is legible in the single file, so it
    // sits here and a run narrowed to that file still reports it.
    //
    // Left unchecked it is silent. The parser reads a citation by its separator, so a colon-separated
    // one is never collected, the resolution checks never see it, and the build passes on a reference
    // the reader has every reason to trust.
    public static void CheckNotation(Doc d, Action<string, string, int?> err)
    {
        foreach (var (citation, line) in d.ColonCitations)
            err("clause-ref", $"'{citation}' separates its clause with a colon — write "
                              + $"'{citation.Replace(':', '.')}'.", line);
    }

    private static void CheckId(ClauseRow row, ClauseSpec spec, HashSet<string> seen,
        Action<string, string, int?> err)
    {
        // Written as a code span for the same reason the identity line's id is: it is a handle rather
        // than a word, and Md.PlainText cannot tell the two apart once the span is flattened.
        if (row.IdSpan is null)
        {
            err("clause-id-format", $"clause id '{Md.Snippet(row.IdText)}' is not a code span — write it as "
                                    + $"`{Md.Snippet(row.IdText)}`.", row.Line);
            return;
        }

        if (spec.IdPatternRegex is { } idPattern && !idPattern.IsMatch(row.IdSpan))
            err("clause-id-format", $"clause id '{row.IdSpan}' does not match {spec.IdPattern}.", row.Line);

        // Ordinal, because `pol-SCRT.LOGS` and `pol-SCRT.logs` differing only in case is not two clauses
        // a reader could tell apart either.
        if (!seen.Add(row.IdSpan))
            err("clause-id-unique", $"clause id '{row.IdSpan}' is used twice — a citation of it names "
                                    + "two obligations.", row.Line);
    }
}
