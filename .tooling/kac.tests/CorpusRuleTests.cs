using kac.core;

// A corpus rule is handed every record at once, which makes it the hardest kind to drive from a
// fixture: one graph per scenario, and a scenario is a whole mini-wiki. These tests hand it graphs
// directly, so the shapes a fixture would only repeat — a longer loop, a loop entered from two sides,
// an edge pointing nowhere — are each one method.

namespace kac.tests;

public class CorpusRuleTests
{
    // -- the registry --

    [Fact]
    public void Every_registered_rule_has_a_distinct_id_and_reports_something()
    {
        Assert.Equal(CorpusRules.All.Count, CorpusRules.ByRuleId.Count);
        Assert.All(CorpusRules.All, r => Assert.NotEmpty(r.Emits));
    }

    // As on the document side: what an emitted id means is `_checks.yaml`'s to say, not this test's.
    [Fact]
    public void Every_emitted_id_is_a_usable_check_id()
    {
        Assert.All(CorpusRules.All.SelectMany(r => r.Emits), Assert.NotEmpty);
    }

    // -- no-dependency-cycles --

    [Fact]
    public void A_graph_with_no_loop_is_left_alone()
        => Assert.Empty(Cycles(
            ("svc-a", ["svc-b", "svc-c"]),
            ("svc-b", ["svc-c"]),
            ("svc-c", [])));

    [Fact]
    public void Two_records_pointing_at_each_other_are_a_cycle()
    {
        var found = Cycles(("svc-a", ["svc-b"]), ("svc-b", ["svc-a"]));

        Assert.Equal("dependency-cycle", Single(found).Check);
        Assert.Equal(Sev.Warning, Single(found).Severity);
        Assert.Equal("'depends-on' forms a cycle: svc-a → svc-b → svc-a.", Single(found).Message);
    }

    // The route names every record on the loop, in the order the edges run, which is the whole reason
    // this is a class: a reader deciding whether a cycle was meant needs to see what it runs through.
    [Fact]
    public void A_longer_loop_names_each_record_on_it_in_order()
        => Assert.Equal("'depends-on' forms a cycle: svc-a → svc-b → svc-c → svc-a.",
            Single(Cycles(("svc-a", ["svc-b"]), ("svc-b", ["svc-c"]), ("svc-c", ["svc-a"]))).Message);

    // The warning goes on the lowest id on the loop rather than wherever the walk entered it, so the
    // record that carries it follows from the corpus rather than from the order the files were read.
    [Fact]
    public void The_finding_is_reported_against_the_lowest_id_on_the_loop()
        => Assert.Equal("services/svc-b.md",
            Single(Cycles(("svc-c", ["svc-b"]), ("svc-b", ["svc-c"]))).File);

    // Two ways into one loop is one loop. Rotating the route to open at the lowest id is what makes the
    // two spellings of it the same string, and the same string is what recognises them as one.
    [Fact]
    public void A_loop_reachable_from_two_sides_is_reported_once()
        => Assert.Equal("'depends-on' forms a cycle: svc-b → svc-c → svc-b.",
            Single(Cycles(
                ("svc-a", ["svc-b", "svc-c"]),
                ("svc-b", ["svc-c"]),
                ("svc-c", ["svc-b"]))).Message);

    // Records hanging off a loop are not on it, and naming them would send the reader to edges that
    // break nothing.
    [Fact]
    public void A_record_leading_into_a_loop_is_not_named_on_it()
        => Assert.Equal("'depends-on' forms a cycle: svc-b → svc-c → svc-b.",
            Single(Cycles(
                ("svc-a", ["svc-b"]),
                ("svc-b", ["svc-c"]),
                ("svc-c", ["svc-b"]),
                ("svc-d", []))).Message);

    [Fact]
    public void A_record_depending_on_itself_is_a_cycle()
        => Assert.Equal("'depends-on' forms a cycle: svc-a → svc-a.",
            Single(Cycles(("svc-a", ["svc-a"]))).Message);

    [Fact]
    public void Two_separate_loops_are_two_findings()
        => Assert.Equal(2, Cycles(
            ("svc-a", ["svc-b"]),
            ("svc-b", ["svc-a"]),
            ("svc-c", ["svc-d"]),
            ("svc-d", ["svc-c"])).Count);

    // An id nothing carries is `ref-resolves`'s to report. Walking it as an edge would either invent a
    // second complaint about it or drop it in silence, and the first is not this rule's to make.
    [Fact]
    public void An_edge_pointing_at_nothing_is_not_this_rules_business()
        => Assert.Empty(Cycles(("svc-a", ["svc-missing"]), ("svc-b", ["svc-a"])));

    // The graph is read off the schema: the field whose `ref:` names the type's own key. A field
    // pointing at another type is a different graph and is not walked, however it is spelled.
    [Fact]
    public void A_field_pointing_at_another_type_is_not_walked()
    {
        var stores = new FieldSpec
            { Name = "data-stores", Type = "list", Of = "id", Refs = ["data"] };

        Assert.Empty(Cycles(Type(stores), ("svc-a", ["svc-b"]), ("svc-b", ["svc-a"])));
    }

    // -- driving the rule --

    private static readonly FieldSpec DependsOn = new()
        { Name = "depends-on", Type = "list", Of = "id", Refs = ["services"] };

    private static TypeSchema Type(FieldSpec field) => new()
    {
        Key = "services",
        Folder = "services",
        FieldOrder = [field.Name],
        Fields = new Dictionary<string, FieldSpec> { [field.Name] = field }
    };

    private static List<Finding> Cycles(params (string Id, string[] Targets)[] records)
        => Cycles(Type(DependsOn), records);

    // The frontmatter is written under whichever field the type declares, so the same graph can be laid
    // on a field that references this type and on one that does not.
    private static List<Finding> Cycles(TypeSchema type, params (string Id, string[] Targets)[] records)
    {
        var schema = new Schema { ByFolder = new Dictionary<string, TypeSchema> { ["services"] = type } };
        var field = type.FieldOrder[0];

        var docs = records
            .Select(r => Doc.Parse($"services/{r.Id}.md",
                $"---\nid: {r.Id}\n{field}: [{string.Join(", ", r.Targets)}]\n---\n\n# {r.Id}\n", schema))
            .OfType<Doc>()
            .ToList();
        Assert.Equal(records.Length, docs.Count);

        var byId = docs.ToDictionary(d => d.FrontScalar("id")!, d => d, StringComparer.OrdinalIgnoreCase);

        var found = new List<Finding>();
        void Report(Sev severity, Doc at, string check, string message, int? line)
            => found.Add(new Finding(at.Rel, line, severity, check, message));

        new NoDependencyCycles().Check(new CorpusRuleContext(docs, byId, type,
            new RuleSpec { Id = "no-dependency-cycles" },
            (at, c, m, l) => Report(Sev.Error, at, c, m, l),
            (at, c, m, l) => Report(Sev.Warning, at, c, m, l)));
        return found;
    }

    private static Finding Single(List<Finding> found) => Assert.Single(found);
}
