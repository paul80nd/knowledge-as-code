using kac.core;

namespace kac.tests;

public class ReportsTests
{
    [Fact]
    public void A_name_no_report_answers_to_comes_to_nothing()
        => Assert.Null(Reports.Plan("invented", Corpus(), [], Stamp));

    [Fact]
    public void A_report_ends_every_line_with_a_line_feed_whatever_its_body_was_built_with()
        => Assert.Equal("---\nx: 1\n---\n\n# T\n\na\nb\n", new ReportPlan("n", "T", "x: 1\n", "a\r\nb\r\n").Text);

    [Fact]
    public void A_clause_a_standard_implements_reads_covered()
        => Assert.Contains("| `STORE` | MUST | `std-SEC` |  |  |  | covered | |", Coverage());

    [Fact]
    public void A_clause_nothing_implements_reads_uncovered()
        => Assert.Contains("| `ROTATE` | SHOULD |  |  |  |  | uncovered | |", Coverage());

    [Fact]
    public void A_deviation_departing_from_a_clause_is_named_beside_it()
        => Assert.Contains("| `LEAKED` | MUST |  | `dev-no-scanner` |  |  | uncovered | |", Coverage());

    [Fact]
    public void A_closed_deviation_is_left_out_of_the_clause_it_departed_from()
        => Assert.DoesNotContain("dev-old-scanner", Coverage());

    [Fact]
    public void A_draft_deviation_is_left_out_until_it_is_in_force()
        => Assert.DoesNotContain("dev-new-scanner", Coverage());

    [Fact]
    public void A_coverage_report_states_that_it_lists_only_a_deviation_in_force()
        => Assert.Contains("The `Deviations` column lists a deviation only while its `status` is `active`.",
            Coverage());

    [Fact]
    public void An_inherited_deviation_in_force_is_named_beside_the_clause()
        => Assert.Contains("| `HOLD` | MUST | `gp:std-OLD` | `gp:dev-OPEN` |  |  | covered | |", Inheriting());

    [Fact]
    public void An_inherited_closed_deviation_is_left_out()
        => Assert.DoesNotContain("dev-SHUT", Inheriting());

    [Fact]
    public void A_deviation_stating_no_status_is_left_out()
        => Assert.DoesNotContain("dev-unstated", Coverage());

    [Fact]
    public void Two_in_force_values_are_stated_as_the_rule_and_not_as_one_value()
    {
        var body = Inheriting(inForce: "closed");

        Assert.Contains("only while its `status` is the value its type declares in `in-force:`.", body);
        Assert.DoesNotContain("only while its `status` is `active`.", body);
    }

    [Fact]
    public void A_control_reaches_a_clause_through_the_standard_covering_it()
        => Assert.Contains("| `STORE` | MUST | `std-SEC` |  |  |", Coverage());

    [Fact]
    public void A_clause_key_a_second_policy_also_uses_is_offered_as_a_pair()
        => Assert.Contains("| `SHARED` | MUST NOT |  |  |  | `pol-KEYS.SHARED` | uncovered | |", Coverage());

    [Fact]
    public void A_report_states_the_corpus_it_read()
        => Assert.Contains("This reads `test-corpus` and what it imports.", Coverage());

    [Fact]
    public void A_coverage_report_states_that_no_column_says_a_clause_is_verified()
        => Assert.Contains("No column here says a clause is verified.", Coverage());

    [Fact]
    public void A_framework_report_states_that_no_column_says_a_clause_meets_what_it_cites()
    {
        var body = Plan("frameworks").Body;

        Assert.Contains("No column here says a clause meets the reference it cites.", body);
        Assert.DoesNotContain("No column here says a clause is verified.", body);
    }

    [Fact]
    public void A_framework_report_states_that_it_counts_the_citations_written_here()
        => Assert.Contains("the citations counted below are the ones written here",
            Plan("frameworks").Body);

    [Fact]
    public void Every_run_stamps_what_produced_it_and_what_it_answered_for()
    {
        var plan = Plan("coverage");

        Assert.Contains("generated:\n  at: 2026-08-08T10:00:00Z\n  by: kac/9.9.9\n", plan.Frontmatter);
        Assert.Contains("  - { resource: test-corpus, version: \"2.1.0\" }", plan.Frontmatter);
    }

    [Fact]
    public void Every_run_stamps_the_report_it_was_run_under_and_the_tool_that_wrote_it()
    {
        Assert.Contains("  report: coverage\n  tool: kac/9.9.9\n", Plan("coverage").Frontmatter);
        Assert.Contains("  report: frameworks\n  tool: kac/9.9.9\n", Plan("frameworks").Frontmatter);
    }

    [Fact]
    public void A_corpus_its_descriptor_does_not_name_is_cited_as_this_corpus()
        => Assert.Equal([new ReportSource("this corpus", "2.1.0")],
            Reports.Sources(new CorpusDescriptor { ContentVersion = "2.1.0" }, []));

    [Fact]
    public void The_stamp_names_the_release_without_the_commit_the_build_carries()
        => Assert.Equal("kac/0.24.0", ReportStamp.ForTool("0.24.0+f7e3108a720ad698197fe8c08f8f6c51323777ea",
            "2026-08-08T10:00:00Z", []).By);

    [Fact]
    public void The_stamp_takes_a_version_with_no_build_metadata_whole()
        => Assert.Equal("kac/0.24.0", ReportStamp.ForTool("0.24.0", "2026-08-08T10:00:00Z", []).By);

    [Fact]
    public void The_frontmatter_leaves_the_id_the_owner_and_the_verification_open()
    {
        var plan = Plan("coverage");

        Assert.StartsWith("id:\n", plan.Frontmatter);
        Assert.Contains("\nowner:\n", plan.Frontmatter);
        Assert.Contains("\nverified:\n", plan.Frontmatter);
    }

    [Fact]
    public void The_frontmatter_calls_a_fresh_report_a_draft()
        => Assert.Contains("\nstatus: draft\n", Plan("coverage").Frontmatter);

    [Fact]
    public void The_frontmatter_parses_as_a_yaml_mapping()
    {
        var parsed = Yaml.Load(Plan("coverage").Frontmatter);

        Assert.Equal("report", Yaml.Str(Yaml.Get(parsed, "type")));
        Assert.Null(Yaml.Str(Yaml.Get(parsed, "owner")));
    }

    [Fact]
    public void The_report_names_the_judgement_it_declined_to_make()
    {
        var body = Coverage();

        Assert.DoesNotContain("Out of scope", body);
        Assert.Contains("The tool prints two verdicts and a person writes the rest.", body);
    }

    [Fact]
    public void The_totals_count_what_the_rows_say()
        => Assert.Contains("| **Total** | **5** | **1** | **4** |", Coverage());

    [Fact]
    public void A_framework_reference_two_clauses_cite_is_one_row()
        => Assert.Contains("| A.5.17 | 2 | `pol-SCRT.STORE`, `pol-SCRT.ROTATE` | `pol-SCRT` | |",
            Plan("frameworks").Body);

    [Fact]
    public void A_framework_report_names_the_standing_the_register_files_it_under()
        => Assert.Contains("| ISO 27001:2022 | Obliged | 1 | 0 |", Plan("frameworks").Body);

    [Fact]
    public void A_framework_section_repeats_the_standing_and_links_the_register_entry()
        => Assert.Contains("Filed under `Obliged` in [`frameworks.md`](../frameworks.md#iso-27001).",
            Plan("frameworks").Body);

    [Fact]
    public void A_reference_carries_the_number_of_clauses_citing_it()
        => Assert.Contains("| A.5.17 | 2 |", Plan("frameworks").Body);

    [Fact]
    public void A_clause_a_grandparent_s_standard_implements_reads_covered()
        => Assert.Contains("| `HOLD` | MUST | `gp:std-OLD` |", Inheriting());

    [Fact]
    public void A_clause_a_rule_covers_is_covered_by_that_rule()
        => Assert.Contains("| `STORE` | MUST | `std-VLT.VAULT` |", RuleCoverage());

    [Fact]
    public void A_control_reaches_a_clause_through_the_rule_covering_it()
    {
        var body = RuleCoverage();

        Assert.Contains("| `STORE` | MUST | `std-VLT.VAULT` |  | `ctl-vault` |", body);
        Assert.Contains("| `LEAKED` | MUST | `std-VLT.REVOKE` |  | `ctl-revoke` |", body);
    }

    [Fact]
    public void A_control_naming_a_standard_whole_reaches_every_rule_in_it()
        => Assert.Contains("| `STORE` | MUST | `std-VLT.VAULT` |  | `ctl-vault`, `ctl-whole` |",
            RuleCoverage(whole: true));

    [Fact]
    public void A_not_enforced_control_is_left_out_of_the_rule_it_names()
        => Assert.Contains("| `STORE` | MUST | `std-VLT.VAULT` |  | `ctl-vault` |",
            RuleCoverage(control: Control("ctl-extra", "std-VLT.VAULT", mechanism: "not-enforced")));

    [Fact]
    public void A_planned_control_is_left_out_until_it_is_in_force()
        => Assert.Contains("| `STORE` | MUST | `std-VLT.VAULT` |  | `ctl-vault` |",
            RuleCoverage(control: Control("ctl-extra", "std-VLT.VAULT", status: "planned")));

    [Fact]
    public void An_inherited_control_in_force_is_named_beside_the_clause()
        => Assert.Contains("| `HOLD` | MUST | `gp:std-OLD` | `gp:dev-OPEN` | `gp:ctl-RUN` |",
            Inheriting(controls: true));

    [Fact]
    public void An_inherited_not_enforced_control_is_left_out()
        => Assert.DoesNotContain("ctl-NONE", Inheriting(controls: true));

    [Fact]
    public void An_inherited_planned_control_is_left_out()
        => Assert.DoesNotContain("ctl-SOON", Inheriting(controls: true));

    [Fact]
    public void An_inherited_rule_covering_a_clause_is_named_with_its_producer()
        => Assert.Contains("| `HOLD` | MUST | `gp:std-OLD.STAY` |", Inheriting(rules: true));

    [Fact]
    public void A_coverage_report_states_that_a_control_vouches_for_the_rule_it_names()
        => Assert.Contains("`Controls` lists each control naming a rule in `Covered by`", Coverage());

    [Fact]
    public void A_coverage_report_states_that_it_lists_only_a_control_in_force_that_checks_something()
        => Assert.Contains("The `Controls` column lists a control only while its `status` is `active` and its "
                           + "`mechanism` is not `not-enforced`. The controls index of each corpus lists every "
                           + "control it wrote.", RuleCoverage());

    [Fact]
    public void A_consumer_that_declines_the_clause_type_reports_the_clauses_it_inherits()
        => Assert.Contains("| `HOLD` | MUST | `gp:std-OLD` |", Declining(Grandparent("active")));

    [Fact]
    public void An_inherited_clause_type_its_producer_renamed_is_still_read()
        => Assert.Contains("| `KEEP` | SHOULD |", Declining(Imported("obligations",
            """{"id":"obl-RET.KEEP","record":"obl-RET","part":"KEEP","level":"SHOULD","clause":"Keep it."}""")));

    [Fact]
    public void An_inherited_glossary_files_none_of_its_terms_as_a_clause()
        => Assert.Contains("| **Total** | **0** | **0** | **0** |", Declining(Glossary()));

    [Fact]
    public void The_imported_bullet_names_each_producer_and_leaves_its_version_to_sources()
        => Assert.Contains("* `eng`, example-engineering.", Declining(Glossary()));

    [Fact]
    public void An_inherited_rule_links_its_record_to_the_page_its_producer_published_and_leaves_the_part_unlinked()
        => Assert.Contains("| `HOLD` | MUST | [`gp:std-OLD`][gp:std-OLD]`.STAY` |",
            Inheriting(rules: true, linked: true));

    [Fact]
    public void An_inherited_policy_s_heading_links_its_page_and_its_clause_cells_stay_bare_keys()
    {
        var body = Inheriting(linked: true);

        Assert.Contains("\n### [gp:pol-OLD][gp:pol-OLD]\n", body);
        Assert.Contains("| `HOLD` | MUST | [`gp:std-OLD`][gp:std-OLD] | [`gp:dev-OPEN`][gp:dev-OPEN] |", body);
    }

    [Fact]
    public void Each_record_cited_is_defined_once_at_the_foot_of_the_report()
        => Assert.EndsWith(
            $"belong to whoever confirms this report.\n\n[gp:ctl-RUN]: {GpPage("ctl-RUN")}\n"
            + $"[gp:dev-OPEN]: {GpPage("dev-OPEN")}\n[gp:pol-OLD]: {GpPage("pol-OLD")}\n"
            + $"[gp:std-OLD]: {GpPage("std-OLD")}\n",
            Inheriting(rules: true, controls: true, linked: true));

    [Fact]
    public void A_local_record_links_to_its_file_from_the_folder_a_report_is_filed_in()
    {
        var body = Plan("coverage", reports: true).Body;

        Assert.Contains("| `STORE` | MUST | [`std-SEC`][std-SEC] |", body);
        Assert.Contains("| [`pol-SCRT`][pol-SCRT] | 4 | 1 | 3 |", body);
        Assert.Contains("| `SHARED` | MUST NOT |  |  |  | [`pol-KEYS`][pol-KEYS]`.SHARED` | uncovered | |", body);
        Assert.Contains("\n[std-SEC]: ../standards/sec.md\n", body);
    }

    [Fact]
    public void A_framework_report_links_each_policy_and_the_policy_of_each_clause()
        => Assert.Contains(
            "| A.5.17 | 2 | [`pol-SCRT`][pol-SCRT]`.STORE`, [`pol-SCRT`][pol-SCRT]`.ROTATE` "
            + "| [`pol-SCRT`][pol-SCRT] | |",
            Plan("frameworks", reports: true).Body);

    [Fact]
    public void A_report_citing_no_page_ends_on_its_last_section()
        => Assert.EndsWith("belong to whoever confirms this report.\n", Coverage());

    [Fact]
    public void An_import_stating_no_page_is_cited_bare_and_named_in_the_limits()
    {
        var body = Inheriting();

        Assert.Contains("| `HOLD` | MUST | `gp:std-OLD` | `gp:dev-OPEN` |", body);
        Assert.Contains("\n### gp:pol-OLD\n", body);
        Assert.Contains("Nothing imported from `gp` states the page it was published at, so its ids are not linked.",
            body);
    }

    [Fact]
    public void A_corpus_further_up_the_chain_stating_no_page_is_named_beside_an_import_that_states_its_own()
    {
        var eng = Grandparent("active");
        var held = new InheritedRecord("eng", "pol-HELD.json",
            """{"fields":{"id":"pol-HELD"},"links":{"human":"https://example.com/eng/pol-HELD.md"}}""");
        var mixed = eng with
        {
            Types = [.. eng.Types.Select(t => t.Type == "policies" ? t with { Records = [.. t.Records, held] } : t)]
        };

        var plan = Reports.Plan("coverage", Corpus(), [mixed], Stamp);
        Assert.NotNull(plan);

        Assert.Contains("Nothing imported from `gp` states the page it was published at", plan.Body);
        Assert.DoesNotContain("`eng` states the page", plan.Body);
    }

    [Fact]
    public void An_import_stating_its_pages_is_not_named_in_the_limits()
        => Assert.DoesNotContain("Nothing imported from", Inheriting(linked: true));

    private static string Coverage() => Plan("coverage").Body;

    private static InheritedCorpus Glossary() => Imported("glossary",
        """{"id":"gl-CORE.TERM","record":"gl-CORE","part":"TERM","title":"Term","definition":"A word."}""");

    /// <summary>One producer publishing one type, whose parts file is the single line given.</summary>
    private static InheritedCorpus Imported(string type, string line) =>
        new("eng", Exporter.FormatVersion, "example-engineering", "0.1.0",
            new ExportPublishing("github", null, null, null, null),
            null,
            Tracker.None,
            [],
            [
                new InheritedType(type, 1, type, $"{type}/parts.jsonl", "record", "part", "id", null,
                    new Dictionary<string, string>(StringComparer.Ordinal), [line], [])
            ]);

    /// <summary>A consumer whose schema adopts no type at all, and which writes nothing itself.</summary>
    private static string Declining(InheritedCorpus source)
    {
        var corpus = new LoadedCorpus
        {
            Schema = new Schema { ByFolder = new Dictionary<string, TypeSchema>(StringComparer.Ordinal) },
            Descriptor = new CorpusDescriptor { Name = "test-corpus", ContentVersion = "2.1.0" },
            Tree = new Tree(new HashSet<string>(StringComparer.Ordinal), _ => ""),
            Adopted = [],
            Docs = [],
            Templates = [],
            SkippedNoFrontmatter = 0
        };

        var plan = Reports.Plan("coverage", corpus, [source], Stamp);
        Assert.NotNull(plan);
        return plan.Body;
    }

    /// <summary>
    /// A standard whose two rules each cover one clause, a control naming each rule, with <paramref name="whole"/> a
    /// third naming the standard whole, and with <paramref name="control"/> one more as given.
    /// </summary>
    private static string RuleCoverage(bool whole = false, string? control = null)
    {
        var types = new Dictionary<string, TypeSchema>(StringComparer.Ordinal)
        {
            ["policies"] = PolicyType(),
            ["standards"] = RuleType(),
            ["controls"] = Refers("controls", "ctl", "control", "verifies", inForce: "active")
        };
        var schema = new Schema { ByFolder = types };

        const string vault =
            """
            ---
            id: std-VLT
            implements: [ pol-SCRT.LEAKED, pol-SCRT.STORE ]
            ---

            # Vault

            ## Rules

            ### VAULT: Secrets live in the vault

            - A service **MUST** read its secrets from the vault.

            _**Covers:** `pol-SCRT.STORE`_

            ### REVOKE: A leaked secret is revoked

            - A leaked secret **MUST** be revoked within the hour.

            _**Covers:** `pol-SCRT.LEAKED`_

            """;

        var sources = new List<(string, string)>
        {
            ("policies/scrt.md", Policy), ("standards/vault.md", vault),
            ("controls/vault.md", Control("ctl-vault", "std-VLT.VAULT")),
            ("controls/revoke.md", Control("ctl-revoke", "std-VLT.REVOKE"))
        };
        if (whole) sources.Add(("controls/whole.md", Control("ctl-whole", "std-VLT")));
        if (control is not null) sources.Add(("controls/extra.md", control));

        var docs = sources.Select(s => Doc.Parse(s.Item1, s.Item2, schema)).OfType<Doc>().ToList();
        Assert.Equal(sources.Count, docs.Count);

        var corpus = new LoadedCorpus
        {
            Schema = schema,
            Descriptor = new CorpusDescriptor { Name = "test-corpus", ContentVersion = "2.1.0" },
            Tree = new Tree(new HashSet<string>(docs.Select(d => d.Rel), StringComparer.Ordinal),
                rel => docs.FirstOrDefault(d => d.Rel == rel)?.Text ?? ""),
            Adopted = [.. types.Values],
            Docs = docs,
            Templates = [],
            SkippedNoFrontmatter = 0
        };

        var plan = Reports.Plan("coverage", corpus, [], Stamp);
        Assert.NotNull(plan);
        return plan.Body;
    }

    private static string Control(string id, string verifies, string status = "active", string mechanism = "ci") =>
        $"---\nid: {id}\nstatus: {status}\nverifies: {verifies}\nmechanism: {mechanism}\n---\n\n# {id}\n";

    private static string Inheriting(string inForce = "active", bool rules = false, bool controls = false,
        bool linked = false)
    {
        var plan = Reports.Plan("coverage", Corpus(rules), [Grandparent(inForce, rules, controls, linked)], Stamp);
        Assert.NotNull(plan);
        // Body keeps the \r\n AppendLine writes on Windows, and only Text normalises it.
        return plan.Body.ReplaceLineEndings("\n");
    }

    /// <summary>
    /// What eng published: gp's clause on a line eng already stamped, and gp's record file keeping the bare ids gp
    /// wrote, in the folder eng filed it under. With <paramref name="controls"/>, three controls verify gp's standard:
    /// one running, one <c>not-enforced</c> and one planned. With <paramref name="linked"/>, every record gp wrote
    /// states the page gp published it at.
    /// </summary>
    private static InheritedCorpus Grandparent(string inForce, bool rules = false, bool controls = false,
        bool linked = false) =>
        new("eng", Exporter.FormatVersion, "example-engineering", "0.1.0",
            new ExportPublishing("github", null, null, null, null),
            null,
            Tracker.None,
            [],
            [
                new InheritedType("policies", 1, "policies", "policies/clauses.jsonl",
                    "record", "part", "id", null,
                    new Dictionary<string, string>(StringComparer.Ordinal),
                    [
                        """
                            {"id":"gp:pol-OLD.HOLD","record":"gp:pol-OLD","part":"HOLD","level":"MUST",
                            "clause":"Stay put."}
                            """.ReplaceLineEndings("")
                    ],
                    [Gp("pol-OLD", "", linked)]),
                new InheritedType("standards", 1, "standards", rules ? "standards/rules.jsonl" : null,
                    "record", "part", "id", null,
                    new Dictionary<string, string>(StringComparer.Ordinal),
                    rules
                        ?
                        [
                            """
                                {"id":"gp:std-OLD.STAY","record":"gp:std-OLD","part":"STAY",
                                "covers":["pol-OLD.HOLD"],"shortcode":"gp"}
                                """.ReplaceLineEndings("")
                        ]
                        : [],
                    [Gp("std-OLD", ""","implements":["pol-OLD.HOLD"]""", linked)]),
                new InheritedType("deviations", 1, "deviations", null, null, null, null, null,
                    new Dictionary<string, string>(StringComparer.Ordinal),
                    [],
                    [
                        Gp("dev-OPEN", ""","status":"active","departs-from":["pol-OLD.HOLD"]""", linked),
                        Gp("dev-SHUT", ""","status":"closed","departs-from":["pol-OLD.HOLD"]""", linked)
                    ],
                    InForce: inForce),
                new InheritedType("controls", 1, "controls", null, null, null, null, null,
                    new Dictionary<string, string>(StringComparer.Ordinal),
                    [],
                    controls
                        ?
                        [
                            GpControl("RUN", "active", "ci", linked),
                            GpControl("NONE", "active", "not-enforced", linked),
                            GpControl("SOON", "planned", "ci", linked)
                        ]
                        : [],
                    InForce: "active")
            ]);

    private static InheritedRecord GpControl(string key, string status, string mechanism, bool linked) =>
        Gp($"ctl-{key}", $",\"status\":\"{status}\",\"verifies\":[\"std-OLD\"],\"mechanism\":\"{mechanism}\"",
            linked);

    /// <summary>
    /// A record gp wrote, filed under gp. <paramref name="fields"/> follows the id inside <c>fields</c>, so it opens
    /// with a comma. With <paramref name="linked"/>, it states the page gp published it at.
    /// </summary>
    private static InheritedRecord Gp(string id, string fields, bool linked)
    {
        var links = linked ? $"{{\"human\":\"{GpPage(id)}\"}}" : "null";
        return new InheritedRecord("gp", $"{id}.json", $"{{\"fields\":{{\"id\":\"{id}\"{fields}}},\"links\":{links}}}");
    }

    private static string GpPage(string id) => $"https://example.com/gp/{id}.md";

    private static ReportPlan Plan(string name, bool reports = false)
    {
        var plan = Reports.Plan(name, Corpus(reports: reports), [], Stamp);
        Assert.NotNull(plan);
        return plan;
    }

    private static readonly ReportStamp Stamp = new("kac/9.9.9", "2026-08-08T10:00:00Z",
        [new ReportSource("test-corpus", "2.1.0")]);

    private const string Policy =
        """
        ---
        id: pol-SCRT
        type: policy
        tier: normative
        status: active
        owner: human:alex.doe
        ---

        # Secrets

        `Policy: pol-SCRT` `ACTIVE`

        ## Clauses

        | Id       | Clause                          | Alignment               |
        |----------|---------------------------------|-------------------------|
        | `STORE`  | **MUST** hold secrets in a vault. | [ISO 27001:2022].A.5.17 |
        | `ROTATE` | SHOULD rotate them yearly.      | [ISO 27001:2022].A.5.17 |
        | `LEAKED` | **MUST** revoke a leaked one.   |                         |
        | `SHARED` | **MUST NOT** share an account.  |                         |

        [ISO 27001:2022]: ../frameworks.md#iso-27001

        """;

    private const string Second =
        """
        ---
        id: pol-KEYS
        type: policy
        tier: normative
        status: active
        owner: human:alex.doe
        ---

        # Keys

        `Policy: pol-KEYS` `ACTIVE`

        ## Clauses

        | Id       | Clause                         | Alignment |
        |----------|--------------------------------|-----------|
        | `SHARED` | **MUST NOT** share a key pair. |           |

        """;

    private const string Standard =
        """
        ---
        id: std-SEC
        type: standard
        tier: normative
        status: active
        owner: human:alex.doe
        implements: [ pol-SCRT.STORE ]
        ---

        # Secret handling

        `Standard: std-SEC` `ACTIVE`

        """;

    private const string Deviation =
        """
        ---
        id: dev-no-scanner
        type: deviation
        tier: normative
        status: active
        owner: human:alex.doe
        departs-from: [ pol-SCRT.LEAKED ]
        ---

        # No secret scanner

        `Deviation: dev-no-scanner` `ACTIVE`

        """;

    private const string Closed =
        """
        ---
        id: dev-old-scanner
        type: deviation
        tier: normative
        status: closed
        owner: human:alex.doe
        departs-from: [ pol-SCRT.LEAKED ]
        ---

        # An old scanner

        `Deviation: dev-old-scanner` `CLOSED`

        """;

    private const string Draft =
        """
        ---
        id: dev-new-scanner
        type: deviation
        tier: normative
        status: draft
        owner: human:alex.doe
        departs-from: [ pol-SCRT.LEAKED ]
        ---

        # A new scanner

        `Deviation: dev-new-scanner` `DRAFT`

        """;

    private const string Unstated =
        """
        ---
        id: dev-unstated
        type: deviation
        tier: normative
        owner: human:alex.doe
        departs-from: [ pol-SCRT.LEAKED ]
        ---

        # A deviation stating no status

        """;

    private const string Register =
        """
        # Frameworks

        ## Obliged

        ### ISO 27001

        Registered against it.
        """;

    /// <summary>
    /// With <paramref name="rules"/>, the standards type is declared as the schema declares it. With
    /// <paramref name="reports"/>, the schema declares the report type, whose folder a local link climbs out of.
    /// </summary>
    private static LoadedCorpus Corpus(bool rules = false, bool reports = false)
    {
        var types = new Dictionary<string, TypeSchema>(StringComparer.Ordinal)
        {
            ["policies"] = PolicyType(),
            ["standards"] = rules ? RuleType() : Refers("standards", "std", "standard", "implements"),
            ["deviations"] = Refers("deviations", "dev", "deviation", "departs-from", inForce: "active")
        };

        if (reports) types["reports"] = new TypeSchema { Key = "reports", TypeName = "report", Folder = "reports" };

        var schema = new Schema { ByFolder = types };

        var docs = new List<Doc>();
        foreach (var (rel, text) in new[]
                 {
                     ("policies/scrt.md", Policy), ("policies/keys.md", Second),
                     ("standards/sec.md", Standard), ("deviations/no-scanner.md", Deviation),
                     ("deviations/old-scanner.md", Closed), ("deviations/new-scanner.md", Draft),
                     ("deviations/unstated.md", Unstated)
                 })
        {
            var doc = Doc.Parse(rel, text, schema);
            Assert.NotNull(doc);
            docs.Add(doc);
        }

        var files = new HashSet<string>(docs.Select(d => d.Rel), StringComparer.Ordinal) { "frameworks.md" };

        return new LoadedCorpus
        {
            Schema = schema,
            Descriptor = new CorpusDescriptor { Name = "test-corpus", ContentVersion = "2.1.0" },
            Tree = new Tree(files, rel => rel == "frameworks.md"
                ? Register
                : docs.FirstOrDefault(d => d.Rel == rel)?.Text ?? ""),
            Adopted = [.. types.Values],
            Docs = docs,
            Templates = [],
            SkippedNoFrontmatter = 0
        };
    }

    /// <summary>
    /// A standard as the schema declares it: rules whose headings open with their ids, each closing on a <c>Covers</c>
    /// line, which the export carries as <c>covers</c>.
    /// </summary>
    private static TypeSchema RuleType()
    {
        var implements = new FieldSpec { Name = "implements", Type = "list", Of = "id", MirrorsCitations = "Covers" };

        return new TypeSchema
        {
            Key = "standards",
            TypeName = "standard",
            Folder = "standards",
            IdPrefix = "std",
            Fields = new Dictionary<string, FieldSpec>(StringComparer.Ordinal) { ["implements"] = implements },
            DeclaredFields = [implements],
            Parts = new PartSpec(PartSpec.Headings, "^[A-Z][A-Z0-9]{1,6}$", ["MUST"], [])
                { Section = "Rules", Noun = "rule" },
            Export = new ExportSpec { Line = [("id", "part.id"), ("covers", "part.citations.Covers")] }
        };
    }

    private static TypeSchema PolicyType() => new()
    {
        Key = "policies",
        TypeName = "policy",
        Folder = "policies",
        IdPrefix = "pol",
        RequiredSections = ["Clauses"],
        Parts = new PartSpec(PartSpec.Table, "", ["MUST", "MUST NOT"], ["SHOULD"])
            { Section = "Clauses", Noun = "clause", Columns = ["Id", "Clause", "Alignment"] }
    };

    private static TypeSchema Refers(string key, string prefix, string typeName, string field,
        string? inForce = null)
    {
        var fields = new Dictionary<string, FieldSpec>(StringComparer.Ordinal)
        {
            [field] = new() { Name = field, Type = "list", Of = "id" }
        };

        if (inForce is not null)
            fields["status"] = new()
                { Name = "status", Type = "enum", Values = ["active", "draft", "closed"], InForce = inForce };

        return new TypeSchema { Key = key, TypeName = typeName, Folder = key, IdPrefix = prefix, Fields = fields };
    }
}
