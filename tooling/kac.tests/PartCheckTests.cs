using kac.core;

namespace kac.tests;

public class PartCheckTests
{
    private static PartSpec Table() =>
        new(PartSpec.Table, "^[A-Z]{3,8}$", ["MUST", "MUST NOT"], ["SHOULD"])
            { Section = "Clauses", Noun = "clause", Columns = ["Id", "Clause"] };

    private static PartSpec Headings() =>
        new(PartSpec.Headings, "", [], []) { Section = "Terms", Noun = "term", Level = 3 };

    private const string Header = "## Clauses\n\n| Id | Clause |\n|----|--------|\n";

    [Fact]
    public void A_section_with_no_table_says_what_to_write()
    {
        var found = Run("## Clauses\n\nProse where a table should be.\n");
        Assert.Equal("clause-table", Assert.Single(found).Check.Value);
        Assert.Contains("headed 'Id | Clause'", Assert.Single(found).Message);
    }

    [Fact]
    public void A_mis_headed_table_names_both_headings_and_stops()
    {
        var found = Run("## Clauses\n\n| Ref | Rule |\n|-----|------|\n| `LOGS` | not a modal in sight |\n");
        Assert.Equal("clause-table", Assert.Single(found).Check.Value);
    }

    [Fact]
    public void An_empty_table_binds_nobody()
    {
        var found = Run(Header);
        Assert.Equal("clause-table", Assert.Single(found).Check.Value);
        Assert.Contains("binds nothing binds nobody", Assert.Single(found).Message);
    }

    [Fact]
    public void A_document_without_the_section_is_left_to_required_section()
        => Assert.Empty(Run("## Context\n\nNothing to do with clauses.\n"));

    [Fact]
    public void A_type_declaring_no_parts_is_not_asked_about_them()
        => Assert.Empty(Run(Header + "| `LOGS` | **MUST** be retained. |\n", null, "policies"));

    [Fact]
    public void A_row_that_opens_with_no_modal_is_not_an_obligation()
    {
        var found = Run(Header + "| `LOGS` | Audit logs are retained for a year. |\n");
        Assert.Equal("clause-modal", Assert.Single(found).Check.Value);
        Assert.Contains("MUST, MUST NOT, SHOULD", Assert.Single(found).Message);
    }

    [Fact]
    public void A_binding_modal_written_plain_is_reported()
    {
        var found = Run(Header + "| `LOGS` | MUST be retained for a year. |\n");
        Assert.Equal("clause-modal", Assert.Single(found).Check.Value);
        Assert.Contains("Write it bold", Assert.Single(found).Message);
    }

    [Fact]
    public void An_advisory_modal_written_bold_is_reported()
    {
        var found = Run(Header + "| `LOGS` | **SHOULD** be retained for a year. |\n");
        Assert.Contains("Write it plain", Assert.Single(found).Message);
    }

    [Fact]
    public void A_longer_modal_is_recognised_before_the_one_that_prefixes_it()
        => Assert.Empty(Run(Header + "| `LOGS` | **MUST NOT** leave the tenancy. |\n"));

    [Fact]
    public void A_word_a_modal_only_prefixes_is_not_a_second_modal()
        => Assert.Empty(Run(Header + "| `SHIFT` | **MUST** be MUSTERED before the shift. |\n"));

    [Fact]
    public void A_second_modal_in_one_row_is_two_obligations_sharing_an_id()
    {
        var found = Run(Header + "| `LOGS` | **MUST** be retained and SHOULD be indexed. |\n");
        Assert.Equal("clause-compound", Assert.Single(found).Check.Value);
        Assert.Contains("carries a second 'SHOULD'", Assert.Single(found).Message);
    }

    [Fact]
    public void A_modal_named_in_a_code_span_is_not_a_second_modal()
        => Assert.Empty(Run(Header + "| `DEVI` | **MUST** record a departure from a `MUST` or `MUST NOT`. |\n"));

    [Fact]
    public void A_bold_second_modal_beside_a_code_span_is_still_two_obligations()
    {
        var found = Run(Header + "| `LOGS` | **MUST** name `SHOULD` and **MUST** be retained. |\n");
        Assert.Equal("clause-compound", Assert.Single(found).Check.Value);
        Assert.Contains("carries a second 'MUST'", Assert.Single(found).Message);
    }

    [Fact]
    public void A_keyword_named_in_a_code_span_does_not_open_the_clause()
    {
        var found = Run(Header + "| `LOGS` | `SHOULD` go. |\n");
        Assert.Equal("clause-modal", Assert.Single(found).Check.Value);
        Assert.Contains("does not open with a modal", Assert.Single(found).Message);
    }

    [Fact]
    public void A_cell_holding_only_a_keyword_in_a_code_span_states_no_obligation()
    {
        var found = Run(Header + "| `LOGS` | `MUST` |\n");
        Assert.Equal("clause-modal", Assert.Single(found).Check.Value);
        Assert.Contains("does not open with a modal", Assert.Single(found).Message);
    }

    [Fact]
    public void A_keyword_in_a_code_span_does_not_open_the_clause_when_the_same_word_follows()
    {
        var found = Run(Header + "| `LOGS` | `SHOULD` SHOULD go. |\n");
        Assert.Equal("clause-modal", Assert.Single(found).Check.Value);
        Assert.Contains("does not open with a modal", Assert.Single(found).Message);
    }

    [Fact]
    public void A_modal_completed_by_a_code_span_does_not_open_the_clause()
    {
        var found = Run(Header + "| `LOGS` | **MUST** `NOT` leave. |\n");
        Assert.Equal("clause-modal", Assert.Single(found).Check.Value);
        Assert.Contains("does not open with a modal", Assert.Single(found).Message);
    }

    [Fact]
    public void A_bold_modal_behind_a_code_span_does_not_open_the_clause()
    {
        var found = Run(Header + "| `LOGS` | `X` **MUST** do y. |\n");
        Assert.Equal("clause-modal", Assert.Single(found).Check.Value);
        Assert.Contains("does not open with a modal", Assert.Single(found).Message);
    }

    [Fact]
    public void A_code_span_after_the_opening_modal_leaves_the_modal_whole()
        => Assert.Empty(Run(Header + "| `LOGS` | **MUST** `retain` logs. |\n"));

    [Fact]
    public void An_id_that_is_not_a_code_span_is_a_word_rather_than_a_handle()
    {
        var found = Run(Header + "| LOGS | **MUST** be retained. |\n");
        Assert.Equal("clause-id-format", Assert.Single(found).Check.Value);
        Assert.Contains("Write it as `LOGS`", Assert.Single(found).Message);
    }

    [Fact]
    public void An_id_outside_the_types_pattern_names_the_pattern()
    {
        var found = Run(Header + "| `lo` | **MUST** be retained. |\n");
        Assert.Contains("does not match ^[A-Z]{3,8}$", Assert.Single(found).Message);
    }

    [Fact]
    public void An_id_used_twice_makes_a_citation_of_it_ambiguous()
    {
        var found = Run(Header
                        + "| `LOGS` | **MUST** be retained. |\n"
                        + "| `LOGS` | **MUST** be indexed. |\n");
        var one = Assert.Single(found);
        Assert.Equal("part-id-unique", one.Check.Value);
        Assert.Contains("two clauses here address as 'LOGS'", one.Message);
    }

    [Fact]
    public void A_table_out_of_order_is_reported_against_the_first_row_that_breaks_it()
    {
        var found = Run(Header
                        + "| `AAA` | **MUST** be first. |\n"
                        + "| `BBB` | SHOULD be last. |\n"
                        + "| `CCC` | **MUST** be first too. |\n"
                        + "| `DDD` | **MUST** and again. |\n");

        var order = found.Where(f => f.Check.Value == "clause-order").ToList();
        Assert.Single(order);
        Assert.Contains("'CCC' is a 'MUST' but follows a 'SHOULD'", order[0].Message);
    }

    [Fact]
    public void A_table_grouped_binding_before_advisory_is_silent()
        => Assert.Empty(Run(Header
                            + "| `AAA` | **MUST** be retained. |\n"
                            + "| `BBB` | **MUST NOT** leave the tenancy. |\n"
                            + "| `CCC` | SHOULD be indexed. |\n"));

    [Fact]
    public void A_colon_separated_citation_names_the_form_to_write()
    {
        var found = Notation("Answering `pol-VURM:TIMEBOX` in full.\n");
        var one = Assert.Single(found);
        Assert.Equal("part-ref", one.Check.Value);
        Assert.Contains("'pol-VURM.TIMEBOX'", one.Message);
    }

    [Fact]
    public void A_dot_separated_citation_passes_the_notation_check()
    {
        Assert.Empty(Notation("Answering `pol-VURM.TIMEBOX` in full.\n"));
    }

    [Theory]
    [InlineData("eng:pol-VURM")]
    [InlineData("eng:pol-VURM.TIMEBOX")]
    [InlineData("Policy: pol-VURM")]
    [InlineData("ISO27001:2022 A.8.25")]
    public void A_colon_the_corpus_does_use_is_left_alone(string span)
    {
        Assert.Empty(Notation($"Written as `{span}` here.\n"));
    }

    [Fact]
    public void The_notation_is_checked_where_no_parts_are_declared()
    {
        Assert.Single(Notation("Answering `pol-VURM:TIMEBOX` in full.\n", null));
    }

    private const string Terms = "## Terms\n\n";

    [Fact]
    public void Headings_under_the_section_are_parts_and_nothing_else_is_asked()
        => Assert.Empty(Run(Terms + "### Corpus\n\nA body of records.\n\n### Drift\n\nCopies parting.\n",
            Headings(), "glossary"));

    [Fact]
    public void A_headings_address_is_the_anchor_it_slugs_to()
    {
        var doc = Parse(Terms + "### Identity line\n\nThe line beneath the H1.\n",
            Headings(), "glossary").Item1;

        var part = Assert.Single(doc.Parts);
        Assert.Equal("identity-line", part.Id);
        Assert.Equal("Identity line", part.Text);
    }

    [Fact]
    public void Two_headings_slugging_alike_collide()
    {
        var found = Run(Terms + "### Identity line\n\nOne.\n\n### Identity-line\n\nAnother.\n",
            Headings(), "glossary");

        var one = Assert.Single(found);
        Assert.Equal("part-id-unique", one.Check.Value);
        Assert.Contains("two terms here address as 'identity-line'", one.Message);
    }

    [Fact]
    public void A_heading_at_another_level_is_not_a_part()
    {
        var doc = Parse(Terms + "### Corpus\n\nA body of records.\n\n#### An aside\n\nMore.\n",
            Headings(), "glossary").Item1;

        Assert.Equal(["corpus"], doc.Parts.Select(p => p.Id));
    }

    [Fact]
    public void A_heading_outside_the_section_is_not_a_part()
    {
        var doc = Parse(Terms + "### Corpus\n\nA body.\n\n## Notes\n\n### Stray\n\nFiled wrongly.\n",
            Headings(), "glossary").Item1;

        Assert.Equal(["corpus"], doc.Parts.Select(p => p.Id));
    }

    [Fact]
    public void A_section_holding_no_headings_says_what_belongs_there()
    {
        var found = Run(Terms, Headings(), "glossary");

        var one = Assert.Single(found);
        Assert.Equal("part-none", one.Check.Value);
        Assert.Contains("the '## Terms' section holds no terms. Write each one as an H3 heading.", one.Message);
    }

    [Fact]
    public void A_section_holding_only_headings_at_another_level_holds_no_parts()
    {
        var found = Run(Terms + "#### Corpus\n\nA body of records.\n", Headings(), "glossary");
        Assert.Equal("part-none", Assert.Single(found).Check.Value);
    }

    [Fact]
    public void An_empty_section_is_not_also_reported_as_an_empty_entry()
        => Assert.DoesNotContain(Run(Terms, Headings(), "glossary"), f => f.Check.Value == "part-empty");

    [Fact]
    public void An_empty_table_is_not_also_reported_as_a_section_holding_nothing()
        => Assert.DoesNotContain(Run(Header), f => f.Check.Value == "part-none");

    [Fact]
    public void A_heading_with_nothing_under_it_names_the_entry()
    {
        var found = Run(Terms + "### Corpus\n\nA body of records.\n\n### Hollow\n\n### Drift\n\nCopies parting.\n",
            Headings(), "glossary");

        var one = Assert.Single(found);
        Assert.Equal("part-empty", one.Check.Value);
        Assert.Contains("term 'Hollow' has nothing under it", one.Message);
    }

    [Fact]
    public void The_last_heading_is_asked_the_same_question()
    {
        var found = Run(Terms + "### Corpus\n\nA body of records.\n\n### Hollow\n", Headings(), "glossary");
        Assert.Equal("part-empty", Assert.Single(found).Check.Value);
    }

    [Theory]
    [InlineData("---")]
    [InlineData("—")]
    [InlineData("*")]
    public void A_mark_standing_in_for_the_words_is_still_an_empty_entry(string body)
    {
        var found = Run(Terms + $"### Hollow\n\n{body}\n", Headings(), "glossary");
        Assert.Equal("part-empty", Assert.Single(found).Check.Value);
    }

    [Fact]
    public void A_table_row_is_not_asked_whether_a_heading_holds_anything()
        => Assert.DoesNotContain(Run(Header + "| `LOGS` | **MUST** be retained. |\n"),
            f => f.Check.Value == "part-empty");

    /// <summary>
    /// A heading source that declares modals, which is what turns the bullets beneath a heading into obligations.
    /// <c>Headings()</c> above declares none.
    /// </summary>
    private static PartSpec Rules() =>
        new(PartSpec.Headings, "", ["MUST", "MUST NOT"], ["SHOULD", "SHOULD NOT", "MAY"])
            { Section = "Rules", Noun = "rule", Level = 3 };

    private const string Group = "## Rules\n\n### Every secret is read at run time\n\n";

    [Fact]
    public void A_bullet_with_no_modal_lists_the_ones_to_write()
    {
        var found = Run(Group + "- A service reads its secrets from the vault.\n", Rules(), "standards");

        var one = Assert.Single(found);
        Assert.Equal("part-modal", one.Check.Value);
        Assert.Contains("MUST, MUST NOT, SHOULD, SHOULD NOT, MAY", one.Message);
    }

    [Fact]
    public void A_modal_mid_sentence_binds_the_bullet()
        => Assert.Empty(Run(Group + "- A service **MUST** read its secrets from the vault.\n",
            Rules(), "standards"));

    [Fact]
    public void An_advisory_modal_binds_the_bullet_too()
        => Assert.Empty(Run(Group + "- A service **SHOULD NOT** log the request body.\n", Rules(), "standards"));

    [Fact]
    public void A_keyword_left_in_plain_capitals_is_reported()
    {
        var found = Run(Group + "- A service MUST read its secrets from the vault.\n", Rules(), "standards");

        var one = Assert.Single(found);
        Assert.Equal("part-modal", one.Check.Value);
        Assert.Contains("Write it `**MUST**`", one.Message);
    }

    [Fact]
    public void A_plain_prohibition_is_named_whole()
    {
        var found = Run(Group + "- A service MUST NOT log the request body.\n", Rules(), "standards");
        Assert.Contains("'MUST NOT' is written plain", Assert.Single(found).Message);
    }

    [Fact]
    public void A_plain_keyword_is_not_also_reported_as_no_keyword()
    {
        var found = Run(Group + "- A service MUST read its secrets from the vault.\n", Rules(), "standards");
        Assert.Contains("written plain", Assert.Single(found).Message);
    }

    [Fact]
    public void A_keyword_inside_a_code_span_is_being_named_rather_than_used()
        => Assert.Empty(Run(Group + "- A rule **MUST** open with `MUST` or `SHOULD`.\n", Rules(), "standards"));

    [Fact]
    public void A_longer_word_beginning_with_a_modal_is_not_one()
        => Assert.Empty(Run(Group + "- A reviewer **MUST** MUSTER the evidence.\n", Rules(), "standards"));

    [Fact]
    public void A_heading_gathering_prose_and_no_bullet_binds_nothing()
    {
        var found = Run(Group + "Secrets come from the vault.\n", Rules(), "standards");

        var one = Assert.Single(found);
        Assert.Equal("part-modal", one.Check.Value);
        Assert.Contains("has no bullet under it", one.Message);
    }

    [Fact]
    public void A_heading_with_nothing_under_it_is_left_to_part_empty()
        => Assert.DoesNotContain(Run(Group, Rules(), "standards"), f => f.Check.Value == "part-modal");

    [Fact]
    public void A_list_nested_in_a_bullet_is_reported()
    {
        var found = Run(Group + "- A service **MUST** read its secrets from the vault.\n"
                        + "  - The vault is the one the platform team runs.\n"
                        + "  - The token is short-lived.\n", Rules(), "standards");

        var one = Assert.Single(found);
        Assert.Equal("part-nested", one.Check.Value);
        Assert.Equal(12, one.Line);
        Assert.Contains("'Every secret is read at run time'", one.Message);
    }

    [Fact]
    public void A_list_nested_under_a_term_is_not_reported()
        => Assert.Empty(Run("## Terms\n\n### Corpus\n\n- A body of records.\n  - One repository.\n",
            Headings(), "glossary"));

    [Fact]
    public void A_heading_source_declaring_no_modals_is_asked_nothing()
        => Assert.Empty(Run("## Terms\n\n### Corpus\n\n- A body of records.\n", Headings(), "glossary"));

    /// <summary>A heading source declaring an <c>id-pattern</c>, which makes each heading open with its id.</summary>
    private static PartSpec IdentifiedRules() =>
        new(PartSpec.Headings, "^[A-Z][A-Z0-9]{1,6}$", ["MUST", "MUST NOT"], ["SHOULD", "SHOULD NOT", "MAY"])
            { Section = "Rules", Noun = "rule", Level = 3 };

    private const string Obligation = "- A job **MUST** declare the permission it needs.\n";

    [Fact]
    public void A_rule_heading_opens_with_the_id_a_citation_names()
    {
        var doc = Parse("## Rules\n\n### PERMS: A job declares the permission it needs\n\n" + Obligation,
            IdentifiedRules(), "standards").Item1;

        var part = Assert.Single(doc.Parts);
        Assert.Equal("PERMS", part.Id);
        Assert.Equal("A job declares the permission it needs", part.Text);
    }

    [Fact]
    public void A_rule_heading_s_anchor_is_the_slug_of_the_whole_heading()
    {
        var doc = Parse("## Rules\n\n### PERMS: A job declares the permission it needs\n\n" + Obligation,
            IdentifiedRules(), "standards").Item1;

        Assert.Equal("perms-a-job-declares-the-permission-it-needs", Assert.Single(doc.Parts).Anchor);
    }

    [Fact]
    public void A_term_heading_is_addressed_and_anchored_by_its_slug()
    {
        var doc = Parse(Terms + "### Identity line\n\nThe line beneath the H1.\n", Headings(), "glossary").Item1;

        var part = Assert.Single(doc.Parts);
        Assert.Equal(part.Id, part.Anchor);
    }

    [Fact]
    public void A_rule_heading_with_no_id_is_reported()
    {
        var found = Run("## Rules\n\n### A job declares the permission it needs\n\n" + Obligation,
            IdentifiedRules(), "standards");

        var one = Assert.Single(found);
        Assert.Equal("part-id-format", one.Check.Value);
        Assert.Contains("'A job declares the permission it needs' opens with no id", one.Message);
    }

    [Theory]
    [InlineData("perms")]
    [InlineData("P")]
    [InlineData("PERMISSIONS")]
    public void A_rule_id_outside_the_pattern_is_reported(string id)
    {
        var found = Run($"## Rules\n\n### {id}: A job declares the permission it needs\n\n" + Obligation,
            IdentifiedRules(), "standards");

        Assert.Equal("part-id-format", Assert.Single(found).Check.Value);
    }

    [Fact]
    public void Two_rules_sharing_an_id_collide()
    {
        var found = Run("## Rules\n\n### PERMS: A job declares the permission it needs\n\n" + Obligation
                        + "\n### PERMS: A token is scoped to its job\n\n" + Obligation,
            IdentifiedRules(), "standards");

        Assert.Equal("part-id-unique", Assert.Single(found).Check.Value);
    }

    [Fact]
    public void A_term_heading_holding_a_colon_keeps_it()
    {
        var doc = Parse(Terms + "### Scope: the corpus\n\nA body.\n", Headings(), "glossary").Item1;

        var part = Assert.Single(doc.Parts);
        Assert.Equal("scope-the-corpus", part.Id);
        Assert.Equal("Scope: the corpus", part.Text);
    }

    private static List<Finding> Run(string body) => Run(body, Table(), "policies");

    private static List<Finding> Run(string body, PartSpec? spec, string folder)
    {
        var (doc, type) = Parse(body, spec, folder);

        var found = new List<Finding>();
        PartChecks.Check(doc, type, new Report(doc.Rel, found));
        return found;
    }

    private static List<Finding> Notation(string body) => Notation(body, Table());

    private static List<Finding> Notation(string body, PartSpec? spec)
    {
        var (doc, _) = Parse(body, spec, "policies");

        var found = new List<Finding>();
        PartChecks.CheckNotation(doc, new Report(doc.Rel, found));
        return found;
    }

    /// <summary>
    /// The schema carries the type's prefix, because the parser tells a citation from a filename of the same shape by
    /// asking whether the half before the dot opens with one.
    /// </summary>
    private static (Doc, TypeSchema) Parse(string body, PartSpec? spec, string folder)
    {
        var type = new TypeSchema { Folder = folder, IdPrefix = folder == "glossary" ? "gls" : "pol", Parts = spec };
        var schema = new Schema { ByFolder = new Dictionary<string, TypeSchema> { [folder] = type } };
        var doc = Doc.Parse($"{folder}/scrt-security.md", $"---\nid: pol-SCRT\n---\n\n# A record\n\n{body}", schema);
        Assert.NotNull(doc);
        return (doc, type);
    }
}
