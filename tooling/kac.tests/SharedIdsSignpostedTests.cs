using System.Text.RegularExpressions;
using kac.core;

namespace kac.tests;

public class SharedIdsSignpostedTests
{
    [Fact]
    public void A_pair_signposted_from_one_side_is_reported_on_the_side_that_does_not_answer()
    {
        var found = Assert.Single(Run(
            Policy("ACCS", ("DUTIES", "**MUST** split a change from its approval ([pol-AGNT].DUTIES says more)")),
            Policy("AGNT", ("DUTIES", "**MUST NOT** approve its own change"))));

        Assert.Equal("pair-one-way", found.Check.Value);
        Assert.Equal(Sev.Error, found.Severity);
        Assert.Equal("policies/agnt.md", found.File);
        Assert.Equal(17, found.Line);
        Assert.Equal(
            "clause 'DUTIES' shares its id with pol-ACCS.DUTIES, which signposts it, and does not signpost that "
            + "clause back. Close this clause with ([pol-ACCS].DUTIES and what the reader gains there).",
            found.Message);
    }

    [Fact]
    public void A_pair_signposted_from_both_sides_is_left_alone()
        => Assert.Empty(Run(
            Policy("ACCS", ("DUTIES", "**MUST** split a change from its approval ([pol-AGNT].DUTIES says more)")),
            Policy("AGNT", ("DUTIES", "**MUST NOT** approve its own change ([pol-ACCS].DUTIES says more)"))));

    [Fact]
    public void A_signpost_to_a_clause_of_another_id_makes_no_pair()
        => Assert.Empty(Run(
            Policy("OBSV", ("SECRETS", "**MUST NOT** log a secret ([pol-SCRT].LOGS says the same)")),
            Policy("SCRT", ("LOGS", "**MUST NOT** write a secret to a log"))));

    [Fact]
    public void A_signpost_back_to_another_clause_does_not_answer_the_pair()
    {
        var found = Assert.Single(Run(
            Policy("ACCS", ("DUTIES", "**MUST** split a change from its approval ([pol-AGNT].DUTIES says more)"),
                ("LEAST", "**MUST** grant the least access a task needs")),
            Policy("AGNT", ("DUTIES", "**MUST NOT** approve its own change ([pol-ACCS].LEAST is wider)"))));

        Assert.Equal("pair-one-way", found.Check.Value);
        Assert.Equal("policies/agnt.md", found.File);
    }

    /// <summary>The other corpus cannot write the other end, so a scoped signpost pairs with nothing here.</summary>
    [Fact]
    public void A_scoped_signpost_does_not_pair_with_a_local_policy()
        => Assert.Equal("shared-id-unsignposted", Assert.Single(Run(
            Policy("AGNT", ("DUTIES", "**MUST NOT** approve its own change ([eng:pol-ACCS].DUTIES says more)")),
            Policy("ACCS", ("DUTIES", "**MUST** split a change from its approval")))).Check.Value);

    /// <summary><c>part-id-unique</c> reports an id used twice in one policy.</summary>
    [Fact]
    public void An_id_repeated_inside_one_policy_is_left_alone()
        => Assert.Empty(Run(
            Policy("DUPE", ("SAME", "**MUST** do one thing"), ("SAME", "**MUST** do another thing"))));

    [Fact]
    public void An_id_repeated_in_another_policy_with_no_signpost_is_a_warning_on_the_first_policy()
    {
        var found = Assert.Single(Run(
            Policy("TRUS", ("INVENT", "**MUST** keep an inventory of the components we ship")),
            Policy("DATA", ("INVENT", "**MUST** keep an inventory of the personal data we hold"))));

        Assert.Equal("shared-id-unsignposted", found.Check.Value);
        Assert.Equal(Sev.Warning, found.Severity);
        Assert.Equal("policies/data.md", found.File);
        Assert.Equal(17, found.Line);
        Assert.Equal(
            "clause 'INVENT' shares its id with pol-TRUS.INVENT, and neither clause signposts the other. Where they "
            + "state one obligation, close each with a signpost to the other. Where they do not, give one of them "
            + "another id.",
            found.Message);
    }

    /// <summary>A policy replacing a retired one may keep its clause ids.</summary>
    [Fact]
    public void A_retired_policy_pairs_with_nothing()
        => Assert.Empty(Run(
            Policy("TRUS", ("INVENT", "**MUST** keep an inventory ([pol-DATA].INVENT says more)")),
            Policy("DATA", "retired", ("INVENT", "**MUST** keep an inventory of the data we hold"))));

    [Fact]
    public void Each_policy_repeating_an_id_is_paired_with_every_other()
        => Assert.Equal(3, Run(
            Policy("AAAA", ("RECORD", "**MUST** keep a record")),
            Policy("BBBB", ("RECORD", "**MUST** keep a record")),
            Policy("CCCC", ("RECORD", "**MUST** keep a record"))).Count);

    [Fact]
    public void A_type_whose_parts_are_headings_is_misconfigured()
        => Assert.Equal(
            "it reads each signpost from a row of a parts table, and this type writes its parts as headings. Declare "
            + "it on a type whose parts are a table.",
            Assert.Single(new SharedIdsSignposted().Misconfigured(Context(Types.ByFolder["standards"], [], []))));

    [Fact]
    public void A_type_whose_parts_are_a_table_is_configured()
        => Assert.Empty(new SharedIdsSignposted().Misconfigured(Context(Types.ByFolder["policies"], [], [])));

    private static List<Finding> Run(params Doc[] docs)
    {
        var found = new List<Finding>();
        new SharedIdsSignposted().Check(Context(Types.ByFolder["policies"], docs, found));
        return found;
    }

    private static CorpusRuleContext Context(TypeSchema type, IReadOnlyList<Doc> docs, List<Finding> found) =>
        new(docs, docs.ToDictionary(d => d.FrontScalar("id") ?? "", StringComparer.OrdinalIgnoreCase),
            new Tree(new HashSet<string>(StringComparer.Ordinal), _ => ""),
            type, new RuleSpec { Id = new RuleId("shared-ids-signposted") },
            new Dictionary<string, string>(StringComparer.Ordinal), null,
            (at, c, m, l) => found.Add(new Finding(at.Rel, l, Sev.Error, c, m)),
            (at, c, m, l) => found.Add(new Finding(at.Rel, l, Sev.Warning, c, m)))
        {
            Schema = Types
        };

    /// <summary>The first clause row sits on line 17, and each row after it one line lower.</summary>
    private static Doc Policy(string mnemonic, params (string Id, string Text)[] clauses)
        => Policy(mnemonic, "active", clauses);

    private static Doc Policy(string mnemonic, string status, params (string Id, string Text)[] clauses)
    {
        var id = $"pol-{mnemonic}";
        var rows = string.Concat(clauses.Select(c => $"| `{c.Id}` | {c.Text} |\n"));
        var definitions = string.Concat(clauses
            .SelectMany(c => Regex.Matches(c.Text, @"\[((?:[a-z]+:)?pol-[A-Z]+)\]"))
            .Select(m => m.Groups[1].Value)
            .Distinct(StringComparer.Ordinal)
            .Select(label => $"[{label}]: {label}.md\n"));

        var doc = Doc.Parse($"policies/{mnemonic.ToLowerInvariant()}.md",
            $"---\nid: {id}\ntype: policy\ntier: normative\nstatus: {status}\nowner: human:alex.doe\n---\n\n"
            + $"# {mnemonic}\n\n`Policy: {id}` `{status.ToUpperInvariant()}`\n\n## Clauses\n\n"
            + "| Id | Clause |\n|----|--------|\n" + rows + "\n" + definitions, Types);
        Assert.NotNull(doc);
        return doc;
    }

    private static readonly Schema Types = new()
    {
        ByFolder = new Dictionary<string, TypeSchema>
        {
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
