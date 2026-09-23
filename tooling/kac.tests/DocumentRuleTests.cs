using kac.core;

// A rule that needs C# is a class with one method, so each of its ways to fail can be driven directly
// rather than only through a whole corpus. That matters more here than elsewhere: the coverage gate
// reads check ids, not branches, so a rule reporting three faults under one id is green once a fixture
// trips any one of them. These tests are what hold the other two honest.

namespace kac.tests;

public class DocumentRuleTests
{
    [Fact]
    public void Every_registered_rule_has_a_distinct_id_and_reports_something()
    {
        Assert.Equal(DocumentRules.All.Count, DocumentRules.ByRuleId.Count);
        Assert.All(DocumentRules.All, r => Assert.NotEmpty(r.Emits));
    }

    // What each emitted id *means* is `_checks.yaml`'s to say.
    [Fact]
    public void Every_emitted_id_is_a_usable_check_id()
    {
        Assert.All(DocumentRules.All.SelectMany(r => r.Emits), e => Assert.NotEmpty(e.Value));
    }

    [Fact]
    public void A_document_with_no_block_quote_is_told_the_Y_statement_is_absent()
    {
        var found = Run(new YStatementPresent(), Adr("Nothing follows the H1 but prose."));
        Assert.Equal("y-statement", Single(found).Check.Value);
        Assert.Equal("no Y-statement block-quote follows the H1.", Single(found).Message);
    }

    [Fact]
    public void A_block_quote_short_of_its_moves_is_told_which_ones()
    {
        var found = Run(new YStatementPresent(),
            Adr("> **In the context of** a summary, **we decided** to drop two moves, **to achieve**\n"
                + "> brevity, **accepting** that it is no longer a Y-statement."));

        Assert.Equal("y-statement", Single(found).Check.Value);
        Assert.StartsWith("Y-statement is missing \"facing\" and \"rather than\".", Single(found).Message);
    }

    [Fact]
    public void A_single_absent_move_is_named_without_the_list_wording()
    {
        var found = Run(new YStatementPresent(),
            Adr("> **In the context of** a summary, **facing** one missing move, **we decided** to leave\n"
                + "> it out, **to achieve** coverage, **accepting** the warning."));

        Assert.StartsWith("Y-statement is missing \"rather than\".", Single(found).Message);
    }

    [Fact]
    public void A_Y_statement_past_the_ceiling_is_told_the_count_and_the_limit()
    {
        var body = "> " + string.Join(" ", Enumerable.Repeat("word", 40)) + " **in the context of** x,"
                   + " **facing** y, **we decided** z, **rather than** w, **to achieve** v, **accepting** u.";
        var found = Run(new YStatementPresent(), Adr(body),
            new RuleSpec { Id = new RuleId("y-statement-present"), MaxWords = 20 });

        Assert.Equal("y-statement", Single(found).Check.Value);
        Assert.Contains("keep it under 20.", Single(found).Message);
    }

    // A corpus can tune the ceiling without a release, and the default applies only where the schema
    // declares none.
    [Fact]
    public void The_ceiling_is_the_schemas_and_a_Y_statement_within_it_is_silent()
    {
        const string body =
            "> **In the context of** a fixture, **facing** a ceiling, **we decided** to stay under it,\n"
            + "> **rather than** over, **to achieve** silence, **accepting** nothing.";

        Assert.Empty(Run(new YStatementPresent(), Adr(body)));
        Assert.NotEmpty(Run(new YStatementPresent(), Adr(body),
            new RuleSpec { Id = new RuleId("y-statement-present"), MaxWords = 5 }));
    }

    // The moves are matched on the rendered text, so the bold that marks them in the corpus is optional
    // and a move followed by punctuation is still found. Whole words only, or `facing` would be found
    // inside `surfacing`.
    [Theory]
    [InlineData("in the context of a, facing b, we decided c, rather than d, to achieve e, accepting f.", 0)]
    [InlineData("IN THE CONTEXT OF a, FACING b, WE DECIDED c, RATHER THAN d, TO ACHIEVE e, ACCEPTING f.", 0)]
    [InlineData("in the context of surfacing a problem we decided c rather than d to achieve e accepting f", 1)]
    public void Moves_are_matched_as_whole_words_and_without_case(string text, int missing)
        => Assert.Equal(missing, YStatementPresent.MissingMoves(text).Count);

    [Fact]
    public void An_alternative_left_open_is_quoted_back_and_a_settled_one_is_not()
    {
        var found = Run(new AlternativesHaveVerdicts(),
            Adr("## Alternatives Considered\n\n"
                + "* **A message queue** — we might explore this in a future revision.\n"
                + "* **A second database** — rejected: one is enough.\n"));

        Assert.Equal("alternatives-verdict", Single(found).Check.Value);
        Assert.Contains("A message queue", Single(found).Message);
    }

    // The bullets are found by heading.
    [Fact]
    public void Bullets_outside_the_section_are_left_alone()
        => Assert.Empty(Run(new AlternativesHaveVerdicts(),
            Adr("## Context\n\n* **A message queue** — we might explore this in a future revision.\n")));

    [Theory]
    [InlineData("**Kafka** — rejected: too heavy for this.", true)]
    [InlineData("**Kafka** — we use RabbitMQ instead.", true)]
    [InlineData("**Kafka** — we might explore this later.", false)]
    public void A_verdict_is_an_outcome_word_or_a_contrastive_cue(string bullet, bool settled)
        => Assert.Equal(settled, AlternativesHaveVerdicts.HasVerdict(bullet));

    [Fact]
    public void Entries_in_order_are_left_alone()
        => Assert.Empty(Run(new TermsAreAlphabetical(),
            Adr("## Terms\n\n### Borrower\n\nOne.\n\n### Item\n\nTwo.\n\n### Title\n\nThree.")));

    [Fact]
    public void An_entry_out_of_place_names_itself_and_where_it_belongs()
    {
        var found = Run(new TermsAreAlphabetical(),
            Adr("## Terms\n\n### Borrower\n\nOne.\n\n### Item\n\nTwo.\n\n### Branch\n\nThree."));

        Assert.Equal("terms-alphabetical", Single(found).Check.Value);
        Assert.Equal("'Branch' is out of order: it belongs before 'Item'.", Single(found).Message);
    }

    // Casing is the entry's own, and a glossary holds `ADR` beside `Borrower`. So ordering by code
    // point would report every initialism as misplaced where a reader scans them as fine.
    [Fact]
    public void Casing_does_not_decide_the_order()
        => Assert.Empty(Run(new TermsAreAlphabetical(),
            Adr("## Terms\n\n### ADR\n\nOne.\n\n### Borrower\n\nTwo.\n\n### corpus\n\nThree.")));

    // Each entry is judged against the one before it.
    [Fact]
    public void Every_entry_out_of_place_is_reported()
        => Assert.Equal(2, Run(new TermsAreAlphabetical(),
            Adr("## Terms\n\n### Item\n\nOne.\n\n### Borrower\n\nTwo.\n\n### Adr\n\nThree.")).Count);

    [Fact]
    public void A_changelog_running_newest_first_is_left_alone()
        => Assert.Empty(Run(new ChangelogNewestFirst(),
            Adr("## Changelog\n\n- 2026-09-14: third.\n- 2026-09-02: second.\n- 2026-08-20: initial version.")));

    [Fact]
    public void An_entry_appended_to_the_foot_names_itself_and_the_entry_above_it()
    {
        var found = Run(new ChangelogNewestFirst(),
            Adr("## Changelog\n\n- 2026-09-02: second.\n- 2026-08-20: initial version.\n- 2026-09-14: third."));

        Assert.Equal("changelog-order", Single(found).Check.Value);
        Assert.Equal("changelog entry '2026-09-14' is out of order: it belongs above '2026-09-02'.",
            Single(found).Message);
    }

    // The entry moves to the top of the run it is newer than, not to the line above it.
    [Fact]
    public void An_entry_is_sent_above_the_first_entry_it_is_newer_than()
    {
        var found = Run(new ChangelogNewestFirst(),
            Adr("## Changelog\n\n- 2026-09-02: second.\n- 2026-08-30: also.\n- 2026-08-20: first.\n"
                + "- 2026-08-31: appended."));

        Assert.Equal("changelog entry '2026-08-31' is out of order: it belongs above '2026-08-30'.",
            Single(found).Message);
    }

    // Two entries on one day are one change written twice, and neither came before the other.
    [Fact]
    public void Two_entries_sharing_a_date_are_left_alone()
        => Assert.Empty(Run(new ChangelogNewestFirst(),
            Adr("## Changelog\n\n- 2026-09-02: one.\n- 2026-09-02: another.\n- 2026-08-20: initial version.")));

    // Each entry is judged against the one before it.
    [Fact]
    public void Every_entry_out_of_place_in_a_changelog_is_reported()
        => Assert.Equal(2, Run(new ChangelogNewestFirst(),
            Adr("## Changelog\n\n- 2026-08-20: one.\n- 2026-09-02: two.\n- 2026-09-14: three.")).Count);

    // A template's date is a placeholder, so it opens no entry and is passed over. Reporting one would
    // fail every corpus on the file it was sent to start from.
    [Fact]
    public void A_bullet_opening_on_something_other_than_a_date_is_passed_over()
        => Assert.Empty(Run(new ChangelogNewestFirst(),
            Adr("## Changelog\n\n- {{YYYY-MM-DD}}: initial version.\n- 2026-08-20: earlier.")));

    // The entries are found by heading.
    [Fact]
    public void Bullets_outside_the_changelog_are_left_alone()
        => Assert.Empty(Run(new ChangelogNewestFirst(),
            Adr("## Summary\n\n- 2026-08-20: one.\n- 2026-09-02: two.")));

    [Theory]
    [InlineData("2026-09-14: added a rule.", "2026-09-14")]
    [InlineData("  2026-09-14 : added a rule.", "2026-09-14")]
    [InlineData("{{YYYY-MM-DD}}: initial version.", null)]
    [InlineData("2026-9-14: added a rule.", null)]
    [InlineData("2026-09-14 added a rule.", null)]
    public void An_entry_opens_on_an_ISO_date_before_a_colon(string bullet, string? date)
        => Assert.Equal(date, ChangelogNewestFirst.OpeningDate(bullet));

    [Fact]
    public void A_bullet_binding_outside_the_parts_section_names_the_modal_and_quotes_itself()
    {
        var found = Run(new BindsOnlyUnderRules(),
            Standard("## Examples\n\n- A secret **MUST** come from the vault."), type: Standards());

        Assert.Equal("modal-outside-rules", Single(found).Check.Value);
        Assert.Equal("this bullet names 'MUST' outside `Rules`: \"A secret MUST come from the vault.\". No "
                     + "citation reaches a rule written here. Move the bullet under a `Rules` heading, or write "
                     + "the keyword in backticks where the bullet only names one.",
            Single(found).Message);
        Assert.Equal(Sev.Warning, Single(found).Severity);
    }

    [Fact]
    public void A_bullet_under_the_parts_section_is_left_alone()
        => Assert.Empty(Run(new BindsOnlyUnderRules(),
            Standard("## Rules\n\n### Secrets come from the vault\n\n- A secret **MUST** come from the vault."),
            type: Standards()));

    // BCP 14 makes capitals normative, so an unbolded keyword is the same intention written weakly.
    // `part-modal` reads both forms for that reason, and this reads both for it.
    [Fact]
    public void A_keyword_written_in_plain_capitals_outside_the_section_is_reported()
    {
        var found = Run(new BindsOnlyUnderRules(),
            Standard("## Examples\n\n- A secret MUST come from the vault."), type: Standards());

        Assert.StartsWith("this bullet names 'MUST' outside `Rules`", Single(found).Message);
    }

    // The rule tells an author a clause landed where nothing can cite it, and a clause is a bullet.
    [Fact]
    public void A_paragraph_naming_a_modal_is_never_reported()
        => Assert.Empty(Run(new BindsOnlyUnderRules(),
            Standard("## Rationale and provenance\n\nA session met a **MUST**-shaped instruction here."),
            type: Standards()));

    // A keyword inside backticks is being named rather than used, which is the reading `Md.Bullets`
    // already gives a code span under a part heading.
    [Fact]
    public void A_modal_in_a_code_span_is_left_alone()
        => Assert.Empty(Run(new BindsOnlyUnderRules(),
            Standard("## Examples\n\n- Write `MUST` in bold capitals."), type: Standards()));

    // A nested list is one bullet's workings, so its points are not obligations of their own.
    [Fact]
    public void A_nested_bullet_is_left_alone()
        => Assert.Empty(Run(new BindsOnlyUnderRules(),
            Standard("## Examples\n\n- What the rule asks for:\n    - a secret **MUST** come from the vault."),
            type: Standards()));

    // The parts section is read from the type, so a second type adopting this rule is a line of YAML.
    [Fact]
    public void The_binding_section_is_the_one_the_type_declares()
    {
        var body = "## Rules\n\n- A secret **MUST** come from the vault.\n\n## Clauses\n\n- And **MUST** stay.";
        var found = Run(new BindsOnlyUnderRules(), Standard(body), type: Standards("Clauses"));

        Assert.Equal("A secret MUST come from the vault.", Quoted(Single(found).Message));
    }

    // A type declaring no modals declares no way for a bullet to bind, which is how a glossary sources
    // headings and is asked none of this.
    [Fact]
    public void A_type_declaring_no_modals_is_asked_nothing()
        => Assert.Empty(Run(new BindsOnlyUnderRules(),
            Standard("## Examples\n\n- A secret **MUST** come from the vault."),
            type: new TypeSchema
            {
                Parts = new PartSpec(PartSpec.Headings, "", [], []) { Section = "Terms" }
            }));

    [Fact]
    public void Every_bullet_binding_outside_the_section_is_reported()
        => Assert.Equal(2, Run(new BindsOnlyUnderRules(),
            Standard("## Examples\n\n- A secret **MUST** come from the vault.\n- A log **MUST NOT** carry one."),
            type: Standards()).Count);

    // The whole word, so the `MUST` inside `MUSTER` is not one. `PartSpec.ModalNamed` draws that line
    // and this rule reads it from there.
    [Fact]
    public void A_word_a_modal_only_prefixes_is_not_a_modal()
        => Assert.Empty(Run(new BindsOnlyUnderRules(),
            Standard("## Examples\n\n- The reviewers MUSTER once a week."), type: Standards()));

    // The longest modal first, so a `MUST NOT` is never reported as a `MUST`.
    [Fact]
    public void A_two_word_modal_is_named_in_full()
    {
        var found = Run(new BindsOnlyUnderRules(),
            Standard("## Examples\n\n- A log **MUST NOT** carry a secret."), type: Standards());

        Assert.StartsWith("this bullet names 'MUST NOT' outside", Single(found).Message);
    }

    // An advisory modal is as unreachable outside the section as a binding one, and the message never
    // says it binds: `MAY` recommends, and a type declaring it says so in `parts.advisory:`.
    [Fact]
    public void An_advisory_modal_outside_the_section_is_reported_without_being_called_binding()
    {
        var found = Run(new BindsOnlyUnderRules(),
            Standard("## Examples\n\n- A client **MAY** retry once."), type: Standards());

        Assert.StartsWith("this bullet names 'MAY' outside `Rules`", Single(found).Message);
        Assert.DoesNotContain("bind", Single(found).Message, StringComparison.Ordinal);
    }

    // A bullet bolding a whole sentence is the misplaced rule this looks for. `part-modal` reports the
    // same shape inside the section for carrying no modal, so neither reading lets it through.
    [Fact]
    public void A_modal_inside_a_wider_bold_run_is_reported()
    {
        var found = Run(new BindsOnlyUnderRules(),
            Standard("## Examples\n\n- **A secret MUST come from the vault.**"), type: Standards());

        Assert.StartsWith("this bullet names 'MUST' outside `Rules`", Single(found).Message);
    }

    // A block quote is somebody else's words, and quoting a rule is naming one.
    [Fact]
    public void A_bullet_inside_a_block_quote_is_left_alone()
        => Assert.Empty(Run(new BindsOnlyUnderRules(),
            Standard("## Examples\n\n> - A secret **MUST** come from the vault."), type: Standards()));

    [Fact]
    public void A_definition_repeating_its_term_names_the_term_and_quotes_the_sentence()
    {
        var found = Run(new DefinitionsDoNotRepeatTheTerm(),
            Glossary("### Catalogue\n\nThe catalogue a reader searches for a title."), type: GlossaryType());

        Assert.Equal("definition-circular", Single(found).Check.Value);
        Assert.Equal("the definition of 'Catalogue' repeats the term: \"The catalogue a reader searches for a "
                     + "title.\". Define it in words a reader already has, and link the entry those words "
                     + "belong to.",
            Single(found).Message);
        Assert.Equal(Sev.Warning, Single(found).Severity);
    }

    [Fact]
    public void A_definition_in_other_words_is_left_alone()
        => Assert.Empty(Run(new DefinitionsDoNotRepeatTheTerm(),
            Glossary("### Catalogue\n\nThe list a reader searches for a title."), type: GlossaryType()));

    // An entry naming its own term in backticks defines it in the words around it, so reading the
    // source would report nearly every entry that mentions a path or a command.
    [Fact]
    public void The_term_inside_a_code_span_is_passed_over()
        => Assert.Empty(Run(new DefinitionsDoNotRepeatTheTerm(),
            Glossary("### Export\n\nWhat `kac export` writes: a flat file per type."), type: GlossaryType()));

    // A link's target is not prose, and a path naming the term is the same case as a code span.
    [Fact]
    public void The_term_inside_a_link_target_is_passed_over()
        => Assert.Empty(Run(new DefinitionsDoNotRepeatTheTerm(),
            Glossary("### Plugin\n\nWhat an agent installs, assembled under [the folder](../plugin/README.md)."),
            type: GlossaryType()));

    // By the second sentence the reader has the meaning, so using the word there is how an entry reads.
    [Fact]
    public void A_later_sentence_using_the_term_is_passed_over()
        => Assert.Empty(Run(new DefinitionsDoNotRepeatTheTerm(),
            Glossary("### Page\n\nThe file at the root of a type's folder. A page has no frontmatter."),
            type: GlossaryType()));

    // `Also`, `Avoid` and `Not` answer other questions about the term, and each names it freely.
    [Fact]
    public void A_labelled_line_repeating_the_term_is_not_the_definition()
        => Assert.Empty(Run(new DefinitionsDoNotRepeatTheTerm(),
            Glossary("### Record\n\nOne document a corpus keeps.\n\n**Not:** a record of a call."),
            type: GlossaryType()));

    // Whole words and no stemming. Precision is what makes the finding worth acting on.
    [Fact]
    public void A_definition_using_the_plural_of_its_term_is_passed_over()
        => Assert.Empty(Run(new DefinitionsDoNotRepeatTheTerm(),
            Glossary("### Record\n\nOne of the records a corpus keeps."), type: GlossaryType()));

    // A term spelling its last character with punctuation gets no boundary there. `\b` after `#` sits
    // between two non-word characters and fails, so `C#` would never match its own definition.
    [Fact]
    public void A_term_ending_in_punctuation_is_still_matched()
    {
        var found = Run(new DefinitionsDoNotRepeatTheTerm(),
            Glossary("### C#\n\nThe language C# is written in."), type: GlossaryType());

        Assert.StartsWith("the definition of 'C#' repeats the term", Single(found).Message);
    }

    [Fact]
    public void Every_entry_defining_itself_is_reported()
        => Assert.Equal(2, Run(new DefinitionsDoNotRepeatTheTerm(),
            Glossary("### Catalogue\n\nThe catalogue a reader searches.\n\n### Digest\n\nThe digest a session "
                     + "arrives holding."), type: GlossaryType()).Count);

    [Theory]
    [InlineData("A page, with no frontmatter. It states what the type contains.", "A page, with no frontmatter.")]
    [InlineData("One sentence and no more", "One sentence and no more")]
    [InlineData("What is a page? The file at the root.", "What is a page?")]
    public void A_definition_is_read_up_to_its_first_full_stop(string text, string sentence)
        => Assert.Equal(sentence, DefinitionsDoNotRepeatTheTerm.FirstSentence(text));

    [Fact]
    public void An_entry_binding_the_reader_names_itself_and_the_modal()
    {
        var found = Run(new EntriesStateNoRequirement(),
            Glossary("### Retention\n\nHow long the estate keeps a record. A team **MUST** delete one after "
                     + "seven years."), type: GlossaryType());

        Assert.Equal("entry-requirement", Single(found).Check.Value);
        Assert.Equal("'Retention' writes a bold 'MUST', and a definition states no requirement. Move the "
                     + "obligation into a standard and link to it from here.",
            Single(found).Message);
        Assert.Equal(Sev.Warning, Single(found).Severity);
    }

    // Bold is what binds under BCP 14, and a glossary is where a keyword is defined. `Standard` names
    // `MUST` in the course of saying what a standard is.
    [Fact]
    public void A_keyword_written_in_plain_capitals_inside_an_entry_is_passed_over()
        => Assert.Empty(Run(new EntriesStateNoRequirement(),
            Glossary("### Keyword\n\nA word BCP 14 makes normative, such as MUST, written in capitals."),
            type: GlossaryType()));

    // An obligation under a labelled line is as misplaced as one above it, and the fix is the same.
    [Fact]
    public void A_modal_inside_a_labelled_line_is_reported()
    {
        var found = Run(new EntriesStateNoRequirement(),
            Glossary("### Retention\n\nHow long the estate keeps a record.\n\n**Not:** deletion, which a team "
                     + "**MUST** run every year."), type: GlossaryType());

        Assert.StartsWith("'Retention' writes a bold 'MUST'", Single(found).Message);
    }

    // The longest modal first, so a prohibition is never reported as an obligation.
    [Fact]
    public void A_two_word_modal_inside_an_entry_is_named_in_full()
    {
        var found = Run(new EntriesStateNoRequirement(),
            Glossary("### Secret\n\nA credential a log **MUST NOT** carry."), type: GlossaryType());

        Assert.StartsWith("'Secret' writes a bold 'MUST NOT'", Single(found).Message);
    }

    // The entries are the type's own reading, so prose outside the terms section is not one.
    [Fact]
    public void A_modal_outside_the_terms_section_is_left_to_another_check()
        => Assert.Empty(Run(new EntriesStateNoRequirement(),
            Glossary("### Retention\n\nHow long the estate keeps a record.",
                scope: "A reader **MUST** start here."), type: GlossaryType()));

    // The whole word, so the `MUST` inside `MUSTER` is not one.
    [Fact]
    public void A_word_a_modal_only_prefixes_inside_an_entry_is_not_a_modal()
        => Assert.Empty(Run(new EntriesStateNoRequirement(),
            Glossary("### Review\n\nThe hour the **MUSTER** takes."), type: GlossaryType()));

    [Fact]
    public void Every_bold_modal_inside_an_entry_is_reported()
        => Assert.Equal(2, Run(new EntriesStateNoRequirement(),
            Glossary("### Retention\n\nA team **MUST** delete a record, and **MUST NOT** keep a copy."),
            type: GlossaryType()).Count);

    // The bullet the message quotes, read back out of it.
    private static string Quoted(string message) =>
        message[(message.IndexOf('"') + 1)..message.LastIndexOf('"')];

    private static List<Finding> Run(IDocumentRule rule, Doc doc, RuleSpec? spec = null, TypeSchema? type = null)
    {
        var found = new List<Finding>();

        // `RuleContext` asks for a type and most rules below read none, so it is handed a bare stand-in
        // rather than the document's own. Parsing against an empty schema leaves that null. A rule
        // reading the type's own declarations is passed one, which is what `Standards` below is.
        rule.Check(new RuleContext(doc, type ?? new TypeSchema(), spec ?? new RuleSpec { Id = rule.RuleId },
            new Report(doc.Rel, found)));
        return found;
    }

    private static Finding Single(List<Finding> found) => Assert.Single(found);

    // A rule reads a parsed document and nothing else, so the fixture is the markdown itself. The
    // schema is empty because no rule here asks anything of the type, which is why `Run` above can
    // hand the context a stand-in.
    private static Doc Adr(string body)
    {
        var text = $"---\nid: adr-0001\nstatus: accepted\n---\n\n# A title\n\n`ADR: adr-0001` `ACCEPTED`\n\n{body}\n";
        var doc = Doc.Parse("adrs/0001-a-title.md", text, new Schema());
        Assert.NotNull(doc);
        return doc;
    }

    // A standard as the schema declares one: parts are the headings under `Rules`, and the modals are
    // the BCP 14 keywords the type narrows to. Written out rather than loaded, so a test states the
    // declaration it depends on.
    private static TypeSchema Standards(string section = "Rules") => new()
    {
        Parts = new PartSpec(PartSpec.Headings, "", ["MUST", "MUST NOT"], ["SHOULD", "SHOULD NOT", "MAY"])
        {
            Section = section,
            Noun = "rule"
        }
    };

    // A glossary as the schema declares one: entries are the H3s under `Terms`, each carrying the three
    // labelled lines beneath it. Written out rather than loaded, so a test states the declaration it
    // depends on.
    private static TypeSchema GlossaryType() => new()
    {
        Parts = new PartSpec(PartSpec.Headings, "", [], [])
        {
            Section = "Terms",
            Noun = "term",
            Asides = ["Also", "Avoid", "Not"]
        }
    };

    // Both glossary rules read the document's parts, and the parser fills those in only where the
    // schema declares the type. So this parse is given one, where `Adr` above is given an empty schema.
    private static Doc Glossary(string body, string scope = "One context and no other.")
    {
        var text = $"---\nid: gls-test\nstatus: draft\n---\n\n# A title\n\n`Glossary: gls-test` `DRAFT`\n\n"
                   + $"## Scope\n\n{scope}\n\n## Terms\n\n{body}\n";

        var schema = new Schema
        {
            ByFolder = new Dictionary<string, TypeSchema>(StringComparer.Ordinal) { ["glossary"] = GlossaryType() }
        };

        var doc = Doc.Parse("glossary/a-title.md", text, schema);
        Assert.NotNull(doc);
        return doc;
    }

    private static Doc Standard(string body)
    {
        var text = $"---\nid: std-TEST\nstatus: active\n---\n\n# A title\n\n"
                   + $"`Standard: std-TEST` `ACTIVE`\n\n{body}\n";
        var doc = Doc.Parse("standards/common/a-title.md", text, new Schema());
        Assert.NotNull(doc);
        return doc;
    }
}
