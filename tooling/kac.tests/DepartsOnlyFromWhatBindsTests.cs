using kac.core;

namespace kac.tests;

public class DepartsOnlyFromWhatBindsTests
{
    [Fact]
    public void A_clause_stating_only_SHOULD_is_reported()
    {
        var found = Assert.Single(Run("pol-SCRT.ROTATE"));

        Assert.Equal("departure-needs-none", found.Check.Value);
        Assert.Equal(Sev.Warning, found.Severity);
        Assert.Equal("deviations/batch-key.md", found.File);
        Assert.Equal(
            "`departs-from` names 'pol-SCRT.ROTATE', a clause stating SHOULD and no MUST or MUST NOT, so departing "
            + "from it needs no deviation. Drop the entry and give the reason in the change that departs, or name "
            + "the clause that binds.",
            found.Message);
    }

    [Fact]
    public void A_clause_stating_MUST_NOT_is_left_alone()
        => Assert.Empty(Run("pol-SCRT.STORE"));

    [Fact]
    public void A_rule_whose_bullets_state_no_MUST_is_reported()
    {
        var found = Assert.Single(Run("std-CI.CACHE"));

        Assert.Equal(
            "`departs-from` names 'std-CI.CACHE', a rule stating SHOULD, MAY and no MUST or MUST NOT, so departing "
            + "from it needs no deviation. Drop the entry and give the reason in the change that departs, or name "
            + "the rule that binds.",
            found.Message);
    }

    [Fact]
    public void A_rule_with_one_MUST_among_its_bullets_is_left_alone()
        => Assert.Empty(Run("std-CI.PERMS"));

    /// <summary><c>part-modal</c> reports the bullet with no modal, and this check stays out of its way.</summary>
    [Fact]
    public void A_rule_with_a_bullet_stating_no_modal_is_left_alone()
        => Assert.Empty(Run("std-CI.LOOSE"));

    [Fact]
    public void An_imported_clause_stating_only_SHOULD_is_reported()
    {
        var found = Assert.Single(Run("eng:pol-DEVI.DEBT"));

        Assert.StartsWith(
            "`departs-from` names 'eng:pol-DEVI.DEBT', a clause stating SHOULD and no MUST", found.Message);
    }

    [Fact]
    public void An_imported_clause_stating_MUST_is_left_alone()
        => Assert.Empty(Run("eng:pol-DEVI.OWNER"));

    [Fact]
    public void An_imported_rule_whose_bullets_state_no_MUST_is_reported()
        => Assert.Single(Run("eng:std-API.PAGING"));

    [Fact]
    public void An_imported_part_whose_line_states_no_level_is_left_alone()
        => Assert.Empty(Run("eng:pol-DEVI.CLOSE"));

    [Fact]
    public void The_literal_none_is_left_alone()
        => Assert.Empty(Run("none"));

    [Fact]
    public void A_closed_deviation_is_left_alone()
        => Assert.Empty(Run("pol-SCRT.ROTATE", status: "closed"));

    [Fact]
    public void Each_entry_is_judged_on_its_own()
        => Assert.Equal(2, Run("pol-SCRT.ROTATE", "pol-SCRT.STORE", "std-CI.CACHE").Count);

    private static List<Finding> Run(params string[] departsFrom) => Run(departsFrom, "active");

    private static List<Finding> Run(string departsFrom, string status) => Run([departsFrom], status);

    private static List<Finding> Run(string[] departsFrom, string status)
    {
        var deviation = Parse("deviations/batch-key.md",
            $"---\nid: dev-batch-key\ntype: deviation\ntier: normative\nstatus: {status}\n"
            + $"departs-from: [ {string.Join(", ", departsFrom)} ]\nowner: human:alex.doe\n---\n\n"
            + "# The batch importer reads a key from the environment\n\n"
            + $"`Deviation: dev-batch-key` `{status.ToUpperInvariant()}`\n");

        var policy = Parse("policies/secrets.md",
            "---\nid: pol-SCRT\ntype: policy\ntier: normative\nstatus: active\nowner: human:alex.doe\n---\n\n"
            + "# Secrets\n\n`Policy: pol-SCRT` `ACTIVE`\n\n## Clauses\n\n"
            + "| Id | Clause |\n|----|--------|\n"
            + "| `STORE` | **MUST NOT** be written into a repository. |\n"
            + "| `ROTATE` | SHOULD be rotated every quarter. |\n");

        var standard = Parse("standards/ci.md",
            "---\nid: std-CI\ntype: standard\ntier: normative\nstatus: active\nowner: human:alex.doe\n---\n\n"
            + "# CI\n\n`Standard: std-CI` `ACTIVE`\n\n## Rules\n\n"
            + "### PERMS: A job declares its permissions\n\n"
            + "- A job **SHOULD** name each permission.\n- A job **MUST** declare `permissions`.\n\n"
            + "### CACHE: A job caches its dependencies\n\n"
            + "- A job **SHOULD** cache its packages.\n- A job **MAY** cache its build output.\n\n"
            + "### LOOSE: A job is quick\n\n"
            + "- A job **SHOULD** finish in ten minutes.\n- A job is quick.\n");

        var docs = new List<Doc> { deviation, policy, standard };
        var byId = docs.ToDictionary(d => d.FrontScalar("id") ?? "", StringComparer.OrdinalIgnoreCase);

        var found = new List<Finding>();
        new DepartsOnlyFromWhatBinds().Check(new CorpusRuleContext(
            docs, byId,
            new Tree(new HashSet<string>(StringComparer.Ordinal), _ => ""),
            deviation.TypeOf(), new RuleSpec { Id = new RuleId("departs-only-from-what-binds") },
            new Dictionary<string, string>(StringComparer.Ordinal), null,
            (at, c, m, l) => found.Add(new Finding(at.Rel, l, Sev.Error, c, m)),
            (at, c, m, l) => found.Add(new Finding(at.Rel, l, Sev.Warning, c, m)))
        {
            Schema = Types,
            Imports = Imported
        });

        return found;
    }

    private static Doc Parse(string rel, string text)
    {
        var doc = Doc.Parse(rel, text, Types);
        Assert.NotNull(doc);
        return doc;
    }

    /// <summary>The levels as an export publishes them: one per clause, and one per bullet of a rule.</summary>
    private static readonly ImportGraph Imported = new(
    [
        new Import("eng", "example-engineering", "1.0.0",
        [
            new ImportedRecord("eng", "pol-DEVI", "policies", true, ["OWNER", "DEBT", "CLOSE"])
            {
                Levels = new Dictionary<string, IReadOnlyList<string?>>(StringComparer.Ordinal)
                {
                    ["OWNER"] = ["MUST"],
                    ["DEBT"] = ["SHOULD"]
                }
            },
            new ImportedRecord("eng", "std-API", "standards", true, ["PAGING"])
            {
                Levels = new Dictionary<string, IReadOnlyList<string?>>(StringComparer.Ordinal)
                {
                    ["PAGING"] = ["SHOULD", "SHOULD NOT"]
                }
            }
        ])
    ], [], []);

    private static readonly Schema Types = new()
    {
        ByFolder = new Dictionary<string, TypeSchema>
        {
            ["deviations"] = new() { Key = "deviations", IdPrefix = "dev" },
            ["standards"] = new()
            {
                Key = "standards",
                IdPrefix = "std",
                Parts = new PartSpec(PartSpec.Headings, "^[A-Z][A-Z0-9]{1,6}$", ["MUST", "MUST NOT"],
                    ["SHOULD", "SHOULD NOT", "MAY"]) { Section = "Rules", Level = 3, Noun = "rule" }
            },
            ["policies"] = new()
            {
                Key = "policies",
                IdPrefix = "pol",
                Parts = new PartSpec(PartSpec.Table, "", ["MUST", "MUST NOT"], ["SHOULD", "COULD"])
                {
                    Section = "Clauses", Noun = "clause"
                }
            }
        }
    };
}
