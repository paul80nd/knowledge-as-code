using kac.core;

// What `kac report` comes to, asked of a corpus nobody wrote to disk.
//
// `ReportPlan` is a value for the reason `ExportPlan` is: the command prints what this answers, so the
// division between what the tool states and what it leaves open is decidable without a filesystem. The
// golden suite exercises no report, because a fixture would only restate these against bytes.

namespace kac.tests;

public class ReportsTests
{
    [Fact]
    public void A_name_no_report_answers_to_comes_to_nothing()
        => Assert.Null(Reports.Plan("invented", Corpus(), [], Stamp));

    // The whole of what the tool decides about a clause. A standard whose `implements:` names it is
    // covered, and everything else is uncovered, whatever the reason.
    [Fact]
    public void A_clause_a_standard_implements_reads_covered()
        => Assert.Contains("| `STORE` | MUST | `std-SEC` |  |  |  | covered | |", Coverage());

    [Fact]
    public void A_clause_nothing_implements_reads_uncovered()
        => Assert.Contains("| `ROTATE` | SHOULD |  |  |  |  | uncovered | |", Coverage());

    // A deviation's `departs-from` is the other edge into a clause, and it says a departure was decided
    // rather than that the clause is now covered. So the row carries both and the verdict stays as it
    // was.
    [Fact]
    public void A_deviation_departing_from_a_clause_is_named_beside_it()
        => Assert.Contains("| `LEAKED` | MUST |  | `dev-no-scanner` |  |  | uncovered | |", Coverage());

    // A control verifies a standard rather than a clause, so it reaches the row through whatever covers
    // it. The report says as much in its own limits.
    [Fact]
    public void A_control_reaches_a_clause_through_the_standard_covering_it()
        => Assert.Contains("| `STORE` | MUST | `std-SEC` |  |  |", Coverage());

    // The same key in a second policy, offered rather than asserted. Two policies reaching for one word
    // by coincidence is the other reading, and only a person can tell them apart.
    [Fact]
    public void A_clause_key_a_second_policy_also_uses_is_offered_as_a_pair()
        => Assert.Contains("| `SHARED` | MUST NOT |  |  |  | `pol-KEYS.SHARED` | uncovered | |", Coverage());

    // Both reports say what they could not see, because a reader meets the numbers without the command
    // beside them.
    [Fact]
    public void A_report_states_the_corpus_it_read()
        => Assert.Contains("This reads `test-corpus` and what it imports.", Coverage());

    // Each report writes its own. A control vouching for a whole standard is a fact about coverage, and
    // over a framework report it would answer a question nobody reading that page had asked.
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

    // The `Alignment` cell an export leaves behind, which is what makes this report local-only and is
    // stated nowhere else a reader of the page would meet it.
    [Fact]
    public void A_framework_report_states_that_it_counts_the_citations_written_here()
        => Assert.Contains("the citations counted below are the ones written here",
            Plan("frameworks").Body);

    // The stamp is the tool's, because only the tool knows both halves of it.
    [Fact]
    public void Every_run_stamps_what_produced_it_and_what_it_answered_for()
    {
        var plan = Plan("coverage");

        Assert.Contains("generated: { at: 2026-08-08T10:00:00Z, by: kac/9.9.9 }", plan.Frontmatter);
        Assert.Contains("  - { resource: test-corpus, version: \"2.1.0\" }", plan.Frontmatter);
    }

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

    // The one thing the tool refuses to print. `Gap` and `Out of scope` are judgements about an estate,
    // and the report says so where a reader would otherwise take an empty column for an answer.
    [Fact]
    public void The_report_names_the_judgement_it_declined_to_make()
    {
        var body = Coverage();

        Assert.DoesNotContain("Out of scope", body);
        Assert.Contains("The tool prints two verdicts and a person writes the rest.", body);
    }

    // Counted from the rows, so the table and the totals cannot disagree.
    [Fact]
    public void The_totals_count_what_the_rows_say()
        => Assert.Contains("| **Total** | **5** | **1** | **4** |", Coverage());

    // The framework report reads the cells rather than the roll-up, and keys a reference so that a
    // control six clauses cite is one row.
    [Fact]
    public void A_framework_reference_two_clauses_cite_is_one_row()
        => Assert.Contains("| A.5.17 | 2 | `pol-SCRT.STORE`, `pol-SCRT.ROTATE` | `pol-SCRT` | |",
            Plan("frameworks").Body);

    [Fact]
    public void A_framework_report_names_the_standing_the_register_files_it_under()
        => Assert.Contains("| ISO 27001:2022 | Obliged | 1 | 0 |", Plan("frameworks").Body);

    // The standing again over the table it governs, and a link to the entry that placed it. A reader
    // works down one framework at a time and the totals table is by then off the screen.
    [Fact]
    public void A_framework_section_repeats_the_standing_and_links_the_register_entry()
        => Assert.Contains("Filed under `Obliged` in [`frameworks.md`](../frameworks.md#iso-27001).",
            Plan("frameworks").Body);

    // The count beside the clauses, so a reference one clause cites reads as `1` without counting the
    // cell next to it. `Cited once` in the totals is the same question asked of the whole framework.
    [Fact]
    public void A_reference_carries_the_number_of_clauses_citing_it()
        => Assert.Contains("| A.5.17 | 2 |", Plan("frameworks").Body);

    private static string Coverage() => Plan("coverage").Body;

    private static ReportPlan Plan(string name)
    {
        var plan = Reports.Plan(name, Corpus(), [], Stamp);
        Assert.NotNull(plan);
        return plan;
    }

    private static readonly ReportStamp Stamp = new("kac/9.9.9", "2026-08-08T10:00:00Z",
        [new ReportSource("test-corpus", "2.1.0")]);

    // Two policies, a standard covering one clause, a deviation departing from another, and a control
    // verifying the standard. Every edge a coverage report walks, and no more of the corpus than that.
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

    private const string Register =
        """
        # Frameworks

        ## Obliged

        ### ISO 27001

        Registered against it.
        """;

    private static LoadedCorpus Corpus()
    {
        var types = new Dictionary<string, TypeSchema>(StringComparer.Ordinal)
        {
            ["policies"] = PolicyType(),
            ["standards"] = Refers("standards", "std", "standard", "implements"),
            ["deviations"] = Refers("deviations", "dev", "deviation", "departs-from")
        };

        var schema = new Schema { ByFolder = types };

        var docs = new List<Doc>();
        foreach (var (rel, text) in new[]
                 {
                     ("policies/scrt.md", Policy), ("policies/keys.md", Second),
                     ("standards/sec.md", Standard), ("deviations/no-scanner.md", Deviation)
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

    // A type whose only interesting declaration is the field naming clauses. `Edges` reads the field off
    // the type, so a type that does not declare it contributes nothing however its records are written.
    private static TypeSchema Refers(string key, string prefix, string typeName, string field) => new()
    {
        Key = key,
        TypeName = typeName,
        Folder = key,
        IdPrefix = prefix,
        Fields = new Dictionary<string, FieldSpec>(StringComparer.Ordinal)
        {
            [field] = new() { Name = field, Type = "list", Of = "id" }
        }
    };
}
