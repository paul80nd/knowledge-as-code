using kac.core;

namespace kac.tests;

public class RuleExprTests
{
    [Fact]
    public void Entries_match_reads_every_object_in_a_list()
        => Assert.False(Eval("entries_match('verified', 'by', '^human:')",
            "id: fix-0001\nverified:\n  - { at: 2026-06-12T09:00:00Z, by: human:alex.doe }\n"
            + "  - { at: 2026-06-13T09:00:00Z, by: role:head-of-engineering }\n"));

    [Fact]
    public void Entries_match_is_true_where_every_object_matches()
        => Assert.True(Eval("entries_match('verified', 'by', '^human:')",
            "id: fix-0001\nverified:\n  - { at: 2026-06-12T09:00:00Z, by: human:alex.doe }\n"));

    [Fact]
    public void Entries_match_reads_the_one_object_a_mapping_holds()
        => Assert.False(Eval("entries_match('generated', 'by', '^kac/')",
            "id: rpt-coverage\ngenerated: { at: 2026-06-12T09:00:00Z, by: someone }\n"));

    [Fact]
    public void Entries_match_is_true_where_the_field_is_absent()
        => Assert.True(Eval("entries_match('verified', 'by', '^human:')", "id: fix-0001"));

    [Fact]
    public void Entries_match_is_true_where_an_object_does_not_carry_the_key()
        => Assert.True(Eval("entries_match('verified', 'by', '^human:')",
            "id: fix-0001\nverified:\n  - { at: 2026-06-12T09:00:00Z }\n"));

    [Fact]
    public void Entries_unique_is_false_where_one_actor_is_written_twice()
        => Assert.False(Eval("entries_unique('verified', 'by')",
            "id: fix-0001\nverified:\n  - { at: 2026-06-12T09:00:00Z, by: human:alex.doe }\n"
            + "  - { at: 2026-09-04T11:30:00Z, by: human:alex.doe }\n"));

    [Fact]
    public void Entries_unique_is_true_where_each_actor_is_written_once()
        => Assert.True(Eval("entries_unique('verified', 'by')",
            "id: fix-0001\nverified:\n  - { at: 2026-06-12T09:00:00Z, by: human:alex.doe }\n"
            + "  - { at: 2026-09-04T11:30:00Z, by: symptom-sweep/1.4.0 }\n"));

    [Fact]
    public void Entries_unique_is_true_where_the_field_is_absent()
        => Assert.True(Eval("entries_unique('verified', 'by')", "id: fix-0001"));

    [Fact]
    public void Entries_unique_is_true_where_no_object_carries_the_key()
        => Assert.True(Eval("entries_unique('verified', 'by')",
            "id: fix-0001\nverified:\n  - { at: 2026-06-12T09:00:00Z }\n"
            + "  - { at: 2026-09-04T11:30:00Z }\n"));

    private static Facts FactsFor(string frontmatter, string body, DateOnly? today = null)
    {
        var doc = Required.Parsed("adrs/0001-a-title.md",
            $"---\n{frontmatter}\n---\n\n# A title\n\n{body}", new Schema());
        return new Facts(doc, today ?? Required.Today);
    }

    private static bool Eval(string expr, string frontmatter = "id: adr-0001", string body = "Some prose.",
        DateOnly? today = null) =>
        RuleExpr.Eval(RuleExpr.Compile(expr), FactsFor(frontmatter, body, today));

    [Fact]
    public void Field_reads_a_frontmatter_scalar()
        => Assert.True(Eval("field('status') == 'deprecated'", "id: adr-0001\nstatus: deprecated"));

    /// <summary>
    /// The literal is <c>Required.Today</c> written out, so a move of that day fails here rather than everywhere at
    /// once.
    /// </summary>
    [Fact]
    public void Today_is_the_run_day_as_an_iso_date()
        => Assert.True(Eval("today() == '2026-06-15'"));

    [Theory]
    [InlineData("2026-06-14", false)]
    [InlineData("2026-06-15", true)]
    [InlineData("2026-06-16", true)]
    public void A_date_field_compares_against_today(string reviewBy, bool stillInside)
        => Assert.Equal(stillInside,
            Eval("field('review-by') >= today()", $"id: dev-a-slug\nreview-by: \"{reviewBy}\""));

    [Fact]
    public void Present_is_false_for_a_bare_key_as_well_as_a_missing_one()
    {
        Assert.False(Eval("present('owner')", "id: adr-0001\nowner:"));
        Assert.False(Eval("present('owner')", "id: adr-0001"));
        Assert.True(Eval("present('owner')", "id: adr-0001\nowner: alex.doe"));
    }

    [Fact]
    public void Present_reads_a_list_as_well_as_a_scalar()
    {
        Assert.True(Eval("present('derived-from')", "id: std-ERRORS\nderived-from: [ adr-0001 ]"));
        Assert.False(Eval("present('derived-from')", "id: std-ERRORS\nderived-from: []"));
        Assert.False(Eval("present('derived-from')", "id: std-ERRORS\nderived-from:"));
    }

    [Fact]
    public void Entries_counts_a_list_and_reads_a_scalar_as_one()
    {
        Assert.True(Eval("entries('implemented-by') == 2",
            "id: ofr-refund\nimplemented-by: [ svc-payment-api, svc-payment-ledger ]"));
        Assert.True(Eval("entries('implemented-by') == 1", "id: ofr-refund\nimplemented-by: svc-payment-api"));
    }

    [Fact]
    public void Entries_is_zero_for_a_bare_key_as_well_as_a_missing_one()
    {
        Assert.True(Eval("entries('implemented-by') == 0", "id: ofr-refund\nimplemented-by:"));
        Assert.True(Eval("entries('implemented-by') == 0", "id: ofr-refund"));
        Assert.True(Eval("entries('implemented-by') == 0", "id: ofr-refund\nimplemented-by: []"));
    }

    private const string TwoAlignments =
        "id: pol-0001\naligns-with:\n  - { framework: ISO 27001:2022, clause: A.5.1 }\n"
        + "  - { framework: NIST CSF 2.0, clause: GV.PO-01 }";

    [Fact]
    public void Entries_counts_a_list_of_objects()
        => Assert.True(Eval("entries('aligns-with') == 2", TwoAlignments));

    [Fact]
    public void Present_reads_a_list_of_objects()
        => Assert.True(Eval("present('aligns-with')", TwoAlignments));

    [Theory]
    [InlineData("generated: { at: 2026-06-12T09:00:00Z, by: kac/0.24.0 }")]
    [InlineData("generated:\n  at: 2026-06-12T09:00:00Z\n  by: kac/0.24.0")]
    [InlineData("generated: { at: 2026-06-12T09:00:00Z }")]
    public void Present_reads_a_filled_in_object(string field)
    {
        Assert.True(Eval("present('generated')", $"id: rpt-coverage\n{field}"));
        Assert.True(Eval("entries('generated') == 1", $"id: rpt-coverage\n{field}"));
    }

    [Theory]
    [InlineData("{{generated}}")]
    [InlineData("{{ generated }}")]
    [InlineData("{{generated.at}}")]
    [InlineData("{}")]
    public void Present_is_false_for_an_unquoted_placeholder_or_an_empty_mapping(string value)
    {
        Assert.False(Eval("present('generated')", $"id: rpt-coverage\ngenerated: {value}"));
        Assert.True(Eval("entries('generated') == 0", $"id: rpt-coverage\ngenerated: {value}"));
    }

    /// <summary><c>required-field</c> reads each of these as absent, and <c>present()</c> agrees with it.</summary>
    [Theory]
    [InlineData("~")]
    [InlineData("null")]
    [InlineData("\"\"")]
    [InlineData("—")]
    public void Present_is_false_for_a_value_written_as_empty(string empty)
        => Assert.False(Eval("present('owner')", $"id: adr-0001\nowner: {empty}"));

    [Fact]
    public void Section_matches_a_heading_whatever_its_case()
        => Assert.True(Eval("section('context')", body: "## Context\n\nProse."));

    [Fact]
    public void First_section_is_empty_where_the_document_has_no_h2()
    {
        Assert.True(Eval("first_section() == 'Symptoms'", body: "## Symptoms\n\nx\n\n## Escalation\n\ny"));
        Assert.True(Eval("first_section() == ''", body: "No headings at all."));
    }

    [Fact]
    public void Section_count_counts_the_heading_and_ignores_its_casing()
    {
        Assert.True(Eval("section_count('Symptom') == 2", body: "## Symptom\n\na\n\n## symptom\n\nb"));
        Assert.True(Eval("section_count('Symptom') == 1", body: "## Symptom\n\na\n\n## Cause\n\nb"));
        Assert.True(Eval("section_count('Symptom') == 0", body: "## Cause\n\nb"));
    }

    [Fact]
    public void Field_matches_reads_the_frontmatter_that_matches_cannot()
    {
        const string front = "id: nfr-0001\nmeasured-by: Monitored where practical.";
        Assert.True(Eval("field_matches('measured-by', '(?i)where practical')", front, "Body text."));
        Assert.False(Eval("field_matches('measured-by', 'p95')", front, "p95 appears in the body only."));
        Assert.False(Eval("field_matches('nope', '.')", front, "Body text."));
    }

    [Fact]
    public void Words_counts_prose_and_not_frontmatter()
        => Assert.True(Eval("words() == 6", "id: adr-0001\nowner: alex.doe", "One two three four."));

    [Fact]
    public void Links_counts_the_links_the_body_carries()
        => Assert.True(Eval("links() == 2", body: "See [a](/a.md) and [b](/b.md)."));

    [Theory]
    [InlineData("field('nope') == 'x'", false)]
    [InlineData("field('nope') != 'x'", true)]
    [InlineData("field('nope') < 'x'", false)]
    [InlineData("field('nope') >= 'x'", false)]
    [InlineData("field('nope') == field('also-nope')", false)]
    [InlineData("field('nope') != field('also-nope')", true)]
    public void A_comparison_against_an_absent_field_is_false_and_its_negation_true(string expr, bool expected)
        => Assert.Equal(expected, Eval(expr));

    [Fact]
    public void A_guarded_rule_is_satisfied_when_the_field_it_guards_is_absent()
    {
        const string rule = "present('detected-at') and present('occurred-at') "
                            + "implies field('detected-at') >= field('occurred-at')";

        // Neither present.
        Assert.True(Eval(rule));
        Assert.True(Eval(rule, "id: adr-0001\ndetected-at: \"2026-06-12\"\noccurred-at: \"2026-06-11\""));
        Assert.False(Eval(rule, "id: adr-0001\ndetected-at: \"2026-06-10\"\noccurred-at: \"2026-06-11\""));
    }

    [Fact]
    public void A_span_is_callable_from_an_expression()
        => Assert.True(Eval("span('occurred-at', 'restored-at') == 'PT40M'",
            "id: pmt-0001\noccurred-at: 2026-06-12T09:00:00Z\nrestored-at: 2026-06-12T09:40:00Z"));

    /// <summary>
    /// What <c>span()</c> promises a rule that calls it.
    /// </summary>
    [Theory]
    [InlineData("2026-06-03T21:14:00Z", "2026-06-03T21:26:00Z", "PT12M")]
    [InlineData("2026-08-12T18:05:00Z", "2026-08-12T22:25:00Z", "PT4H20M")]
    [InlineData("2026-06-12T09:00:00Z", "2026-06-13T11:00:00Z", "PT26H")]
    [InlineData("2026-06-12T09:00:00Z", "2026-06-12T09:00:00Z", "PT0S")]
    [InlineData("2026-06-12T09:00:00Z", "2026-06-12T09:00:07Z", "PT7S")]
    public void A_span_is_hours_minutes_and_seconds(string from, string to, string expected)
        => Assert.Equal(expected, Instants.Span(from, to));

    [Theory]
    [InlineData(null, "2026-06-12T09:00:00Z")]
    [InlineData("2026-06-12T09:00:00Z", null)]
    [InlineData("2026-06-12", "2026-06-13T09:00:00Z")]
    [InlineData("2026-06-12T09:00:00Z", "2026-06-12T08:00:00Z")]
    public void A_span_nothing_can_measure_is_empty(string? from, string? to)
        => Assert.Equal("", Instants.Span(from, to));

    [Fact]
    public void A_gap_in_days_is_callable_from_an_expression()
        => Assert.True(Eval("days('accepted-on', 'review-by') == 184",
            "id: dev-a\naccepted-on: \"2026-03-07\"\nreview-by: \"2026-09-07\""));

    /// <summary>Six calendar months runs from 181 days to 184.</summary>
    [Theory]
    [InlineData("2026-01-07", "2026-07-07", 181)]
    [InlineData("2026-03-07", "2026-09-07", 184)]
    [InlineData("2026-09-07", "2027-03-07", 181)]
    [InlineData("2026-09-07", "2027-09-07", 365)]
    [InlineData("2026-06-12", "2026-06-13", 1)]
    public void A_gap_is_whole_days(string from, string to, int expected)
        => Assert.Equal(expected, Instants.Days(from, to));

    [Theory]
    [InlineData(null, "2026-06-12")]
    [InlineData("2026-06-12", null)]
    [InlineData("2026-06-12T09:00:00Z", "2026-06-13")]
    [InlineData("2026-06-13", "2026-06-12")]
    [InlineData("2026-06-12", "2026-06-12")]
    public void A_gap_nothing_can_measure_is_zero(string? from, string? to)
        => Assert.Equal(0, Instants.Days(from, to));

    /// <summary><c>Required.Today</c> is 2026-06-15, so the record states the day 92 before it.</summary>
    [Fact]
    public void An_age_in_days_is_callable_from_an_expression()
        => Assert.True(Eval("days_since('last-rehearsed') == 92",
            "id: prc-a\nlast-rehearsed: \"2026-03-15\""));

    /// <summary>
    /// Measured against <c>Required.Today</c>, 2026-06-15. A date the run has not reached measures nothing and is zero.
    /// </summary>
    [Theory]
    [InlineData("2026-03-15", 92)]
    [InlineData("2025-06-14", 366)]
    [InlineData("2026-06-15", 0)]
    [InlineData("2029-06-01", 0)]
    public void An_age_is_whole_days_up_to_the_run_day(string from, int expected)
        => Assert.True(Eval($"days_since('last-rehearsed') == {expected}",
            $"id: prc-a\nlast-rehearsed: \"{from}\""));

    [Fact]
    public void Iso_dates_order_as_text()
        => Assert.True(Eval("field('a') < field('b')", "id: adr-0001\na: \"2026-01-09\"\nb: \"2026-01-10\""));

    [Theory]
    [InlineData("present('id') or present('nope')", true)]
    [InlineData("present('id') and present('nope')", false)]
    [InlineData("present('nope') implies present('nope')", true)] // false implies anything
    [InlineData("present('id') implies present('nope')", false)]
    [InlineData("not present('nope')", true)]
    [InlineData("words() <= links() * 40", true)]
    [InlineData("(1 + 2) * 3 == 9", true)]
    [InlineData("1 + 2 * 3 == 7", true)] // precedence, not left to right
    [InlineData("4 / 0 == 0", true)]
    public void The_grammar_evaluates_as_declared(string expr, bool expected)
        => Assert.Equal(expected, Eval(expr, body: "One [link](/a.md)."));

    [Theory]
    [InlineData("field('status')", "yes/no")] // not a question
    [InlineData("words()", "yes/no")]
    [InlineData("wordcount() > 3", "not a fact")] // unknown function
    [InlineData("field()", "1 argument")]         // wrong arity
    [InlineData("field('a', 'b')", "1 argument")]
    [InlineData("words() == 'three'", "cannot compare")]          // number against text
    [InlineData("field('a') * 2 > 3", "wants a number")]          // arithmetic on text
    [InlineData("present('a') < present('b')", "cannot compare")] // ordering yes/no answers
    [InlineData("words() >", "expected a value")]                 // truncated
    [InlineData("words() 3", "unexpected")]                       // two expressions
    [InlineData("field('unterminated", "unterminated string")]
    [InlineData("words() # 3", "unexpected character")]
    [InlineData("present", "must be called with parentheses")]
    [InlineData("words() < 99999999999", "too large for a whole number")] // digits an int cannot hold
    public void A_defective_expression_fails_to_compile(string expr, string expectedInMessage)
    {
        var ex = Assert.Throws<RuleExprException>(() => RuleExpr.Compile(expr));
        Assert.Contains(expectedInMessage, ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void A_chained_comparison_does_not_parse()
        => Assert.Throws<RuleExprException>(() => RuleExpr.Compile("1 < words() < 40"));

    [Fact]
    public void Matches_sees_inside_a_fenced_code_block()
        => Assert.True(Eval("matches('AKIA[0-9A-Z]{6}')",
            body: "Configure it:\n\n```\nAWS_KEY=AKIA123456\n```\n"));

    [Fact]
    public void Words_does_not_see_what_matches_does()
        => Assert.True(Eval("words() == 4 and matches('secret')",
            body: "Configure it:\n\n```\nTOKEN=secret\n```\n"));

    [Fact]
    public void Matches_sees_markdown_syntax()
    {
        Assert.True(Eval(@"matches('\*\*MUST\*\*')", body: "You **MUST** do this."));
        Assert.False(Eval(@"matches('\*\*MUST\*\*')", body: "You MUST do this."));
    }

    [Fact]
    public void Matches_does_not_see_the_frontmatter()
        => Assert.False(Eval("matches('alex.doe')", "id: adr-0001\nowner: alex.doe", "Prose with no name in it."));

    [Fact]
    public void Section_matches_is_bounded_by_the_next_heading()
    {
        const string body = "## Steps\n\nDo the thing.\n\n## Verification\n\nTypically it worked.\n";
        Assert.False(Eval("section_matches('Steps', 'Typically')", body: body));
        Assert.True(Eval("section_matches('Verification', 'Typically')", body: body));
    }

    [Fact]
    public void Section_matches_is_false_where_the_section_is_absent()
        => Assert.False(Eval("section_matches('Nowhere', 'anything')", body: "## Steps\n\nDo the thing."));

    [Fact]
    public void Section_matches_finds_the_heading_whatever_its_case()
        => Assert.True(Eval("section_matches('steps', 'thing')", body: "## Steps\n\nDo the thing."));

    [Theory]
    [InlineData("matches()", "1 argument")]
    [InlineData("section_matches('Steps')", "2 argument")]
    [InlineData("matches(3)", "wants text")]
    public void The_text_probe_is_type_checked_like_any_other_fact(string expr, string expected)
    {
        var ex = Assert.Throws<RuleExprException>(() => RuleExpr.Compile(expr));
        Assert.Contains(expected, ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// A doubled quote is one quote, as in YAML and SQL. The pattern below is the character class ["'].
    /// </summary>
    [Fact]
    public void A_doubled_quote_is_one_quote()
    {
        const string quoteClass = "matches('[\"'']')";

        Assert.True(Eval(quoteClass, body: "It's here."));
        Assert.True(Eval(quoteClass, body: "A \"quote\"."));
        Assert.False(Eval(quoteClass, body: "Nothing quoted."));
    }

    [Fact]
    public void A_string_left_open_by_an_escaped_quote_still_fails_to_compile()
        => Assert.Throws<RuleExprException>(() => RuleExpr.Compile("matches('a''"));
}
