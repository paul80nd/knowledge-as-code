namespace kac.core;

public static class PartChecks
{
    public static void Check(Doc d, TypeSchema t, Report report)
    {
        if (t.Parts is not { } spec) return;
        // A missing section is `required-section`'s to report.
        if (!d.Sections.Any(s => string.Equals(s.Title, spec.Section, StringComparison.OrdinalIgnoreCase))) return;

        if (spec.Source == PartSpec.Table && !TableIsSound(d, spec, report)) return;

        if (spec.Source == PartSpec.Headings && !SectionIsPopulated(d, spec, report)) return;

        CheckAddresses(d, spec, report);

        if (spec.Source == PartSpec.Headings)
        {
            CheckBodies(d, spec, report);
            CheckBullets(d, spec, report);
        }

        if (spec.Source == PartSpec.Table) CheckObligations(d, spec, report);
    }

    private static bool SectionIsPopulated(Doc d, PartSpec spec, Report report)
    {
        if (d.Parts.Count > 0) return true;

        report.Err(new CheckId("part-none"),
            $"the '## {spec.Section}' section holds no {spec.Noun}s. Write each one as an H{spec.Level} heading.",
            d.PartSectionLine);
        return false;
    }

    /// <summary>
    /// <c>Md.HasContent</c> reads the source as written, so a horizontal rule or an em dash standing in for a
    /// definition counts as nothing written.
    /// </summary>
    private static void CheckBodies(Doc d, PartSpec spec, Report report)
    {
        foreach (var row in d.Parts.Where(row => !Md.HasContent(row.Body(d.Text))))
            report.Err(new CheckId("part-empty"),
                $"{spec.Noun} '{Md.Snippet(row.Text)}' has nothing under it. Write it or delete the heading.",
                row.Line);
    }

    /// <summary>Each fault stops the pass, because nothing beneath a broken table can be judged.</summary>
    private static bool TableIsSound(Doc d, PartSpec spec, Report report)
    {
        var headers = string.Join(" | ", spec.Columns);

        if (d.PartTableHeaders is null)
        {
            report.Err(new CheckId("clause-table"),
                $"the '## {spec.Section}' section holds no table. Write one row per obligation, headed '{headers}'.",
                d.PartSectionLine);
            return false;
        }

        if (!d.PartTableHeaders.SequenceEqual(spec.Columns, StringComparer.Ordinal))
        {
            report.Err(new CheckId("clause-table"),
                $"the clause table is headed '{string.Join(" | ", d.PartTableHeaders)}'. It must be headed "
                + $"'{headers}'.", d.PartTableLine);
            return false;
        }

        if (d.Parts.Count == 0)
        {
            report.Err(new CheckId("clause-table"),
                "the clause table has no rows: a record that binds nothing binds nobody.", d.PartTableLine);
            return false;
        }

        return true;
    }

    private static void CheckAddresses(Doc d, PartSpec spec, Report report)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var row in d.Parts)
        {
            if (row.Id is null)
            {
                if (spec.Source == PartSpec.Table)
                    report.Err(new CheckId("clause-id-format"),
                        $"clause id '{Md.Snippet(row.IdText)}' is not a code span. Write it as "
                        + $"`{Md.Snippet(row.IdText)}`.", row.Line);
                continue;
            }

            if (spec is { Source: PartSpec.Table, IdPatternRegex: { } idPattern } && !idPattern.IsMatch(row.Id))
                report.Err(new CheckId("clause-id-format"),
                    $"clause id '{row.Id}' does not match {spec.IdPattern}.", row.Line);

            if (!seen.Add(row.Id))
                report.Err(new CheckId("part-id-unique"),
                    $"two {spec.Noun}s here address as '{row.Id}': a citation of it names both and reaches neither.",
                    row.Line);
        }
    }

    /// <summary>
    /// A part with nothing under it is passed over, because <c>part-empty</c> has already reported it.
    /// </summary>
    private static void CheckBullets(Doc d, PartSpec spec, Report report)
    {
        if (spec.Binding.Count == 0) return;

        foreach (var part in d.Parts.Where(p => Md.HasContent(p.Body(d.Text))))
        {
            var bullets = Md.Bullets(d.Ast, part.BodyStart, part.BodyEnd).ToList();

            if (bullets.Count == 0)
            {
                report.Err(new CheckId("part-modal"),
                    $"{spec.Noun} '{Md.Snippet(part.Text)}' has no bullet under it, so it binds nobody. Write "
                    + $"one bullet per obligation, each with one of {string.Join(", ", spec.Levels)}.",
                    part.Line);
                continue;
            }

            // One bullet is one obligation. A nested list is detail nothing addresses or exports, so its points go
            // unasked.
            foreach (var line in Md.NestedLists(d.Ast, part.BodyStart, part.BodyEnd))
                report.Err(new CheckId("part-nested"),
                    $"{spec.Noun} '{Md.Snippet(part.Text)}' has a list inside a bullet. Write each point as a "
                    + "bullet of its own with its own modal, or fold it into the sentence above it.", line);

            foreach (var (text, plain, bold, line) in bullets)
            {
                // The unbolded keyword first, so a bullet with only that is told once what is wrong with it.
                if (spec.ModalNamed(plain) is { } loose)
                    report.Err(new CheckId("part-modal"),
                        $"'{loose}' is written plain here, and bold is what binds. Write it `**{loose}**`.", line);
                else if (!bold.Any(b => spec.Levels.Contains(b, StringComparer.Ordinal)))
                    report.Err(new CheckId("part-modal"),
                        $"{spec.Noun} '{Md.Snippet(text)}' has no modal, so nothing says how hard it binds. Write "
                        + $"one of {string.Join(", ", spec.Levels)} in bold, or move the line to prose.", line);
            }
        }
    }

    private static void CheckObligations(Doc d, PartSpec spec, Report report)
    {
        var highest = 0;
        var disordered = false;

        foreach (var row in d.Parts)
        {
            var modal = spec.Modal(row.Text);
            if (modal is null)
            {
                report.Err(new CheckId("clause-modal"),
                    $"clause '{Md.Snippet(row.Text)}' does not open with a modal. Write one of "
                    + $"{string.Join(", ", spec.Levels)}.", row.Line);
                continue;
            }

            var binds = spec.Binding.Contains(modal, StringComparer.Ordinal);
            if (binds && !string.Equals(row.BoldLead, modal, StringComparison.Ordinal))
                report.Err(new CheckId("clause-modal"), $"'{modal}' binds. Write it bold, `**{modal}**`.", row.Line);
            else if (!binds && row.BoldLead is not null)
                report.Err(new CheckId("clause-modal"),
                    $"'{modal}' does not bind. Write it plain, not bold.", row.Line);

            // `ModalNamed` asks for the whole word, so the `MUST` inside `MUSTERED` is not a second modal.
            var rest = row.Text[modal.Length..];
            if (spec.ModalNamed(rest) is { } second)
                report.Warn(new CheckId("clause-compound"),
                    $"clause '{row.Id ?? row.IdText}' carries a second '{second}': one obligation per clause, or the "
                    + "citation is ambiguous.", row.Line);

            // Reported once, against the first row breaking the grouping.
            var rank = spec.Rank(modal);
            if (rank < highest && !disordered)
            {
                report.Warn(new CheckId("clause-order"),
                    $"clause '{row.Id ?? row.IdText}' is a '{modal}' but follows a '{spec.Levels[highest]}'. Group "
                    + $"the table {string.Join(", ", spec.Levels)}.", row.Line);
                disordered = true;
            }

            highest = Math.Max(highest, rank);
        }
    }

    /// <summary>
    /// Asked of every document, because a citation is rarely written in a record of the same type as the part it names.
    /// </summary>
    public static void CheckNotation(Doc d, Report report)
    {
        foreach (var (citation, line) in d.ColonCitations)
            report.Err(new CheckId("part-ref"),
                $"'{citation}' separates the two halves with a colon. Write '{citation.Replace(':', '.')}'.", line);
    }
}
