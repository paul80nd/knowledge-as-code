using kac.core;

// The rule compares a value on one record with the same value on another, so every scenario needs at
// least two records. These hand it the graph directly. A fixture would repeat a whole mini-corpus for
// each shape.

namespace kac.tests;

public class DependencyCriticalityTests
{
    [Fact]
    public void A_record_depending_on_something_graded_above_it_is_left_alone()
        => Assert.Empty(Check(("svc-a", "supporting", ["svc-b"]), ("svc-b", "critical", [])));

    [Fact]
    public void Two_records_graded_alike_are_left_alone()
        => Assert.Empty(Check(("svc-a", "critical", ["svc-b"]), ("svc-b", "critical", [])));

    [Fact]
    public void A_record_graded_above_what_it_depends_on_is_a_warning()
    {
        var found = Check(("svc-a", "critical", ["svc-b"]), ("svc-b", "important", []));

        Assert.Equal("dependency-criticality", Single(found).Check.Value);
        Assert.Equal(Sev.Warning, Single(found).Severity);
        Assert.Equal("services/svc-a.md", Single(found).File);
    }

    // Both grades and the id, because the reader judging the edge weighs two values on two documents.
    [Fact]
    public void The_message_names_the_field_the_target_and_both_grades()
        => Assert.Equal(
            "'depends-on' cites 'svc-b', graded 'important' where this record is 'critical'. Regrade "
            + "one of the two, or write down what degrades when it is gone.",
            Single(Check(("svc-a", "critical", ["svc-b"]), ("svc-b", "important", []))).Message);

    // One entry of a list is at fault, so the finding lands on that entry and not on the field above it.
    [Fact]
    public void Each_finding_names_the_line_the_entry_sits_on()
    {
        var found = Check(
            ("svc-a", "critical", ["svc-b", "svc-c", "svc-d"]),
            ("svc-b", "critical", []),
            ("svc-c", "important", []),
            ("svc-d", "supporting", []));

        Assert.Equal([5, 6], found.Select(f => f.Line));
    }

    // Two places down is the same fault as one.
    [Fact]
    public void A_grade_two_places_above_the_target_is_reported()
        => Assert.Single(Check(("svc-a", "critical", ["svc-b"]), ("svc-b", "supporting", [])));

    // The ranking is the enum's to declare. Without it the values are a set, and a set has no order.
    [Fact]
    public void An_enum_that_declares_no_ranking_is_not_compared()
        => Assert.Empty(Check(Type(Unranked, DependsOn),
            ("svc-a", "critical", ["svc-b"]), ("svc-b", "supporting", [])));

    // The graph is read off the schema, as `no-dependency-cycles` reads it: the field whose `ref:` names
    // the type's own key.
    [Fact]
    public void A_field_pointing_at_another_type_is_not_walked()
        => Assert.Empty(Check(Type(Criticality, Stores),
            ("svc-a", "critical", ["svc-b"]), ("svc-b", "supporting", [])));

    // `ref-resolves` reports an id nothing declares. There is no grade at the far end to read.
    [Fact]
    public void An_edge_pointing_at_nothing_is_not_this_rules_business()
        => Assert.Empty(Check(("svc-a", "critical", ["svc-missing"])));

    // `enum` reports a value outside the range. Comparing against it would turn one malformed value
    // into a finding on every record citing it.
    [Fact]
    public void A_grade_outside_the_range_is_passed_over_at_either_end()
    {
        Assert.Empty(Check(("svc-a", "vital", ["svc-b"]), ("svc-b", "supporting", [])));
        Assert.Empty(Check(("svc-a", "critical", ["svc-b"]), ("svc-b", "incidental", [])));
    }

    [Fact]
    public void A_record_with_no_grade_is_passed_over()
        => Assert.Empty(Check(("svc-a", null, ["svc-b"]), ("svc-b", "supporting", [])));

    private static readonly FieldSpec Criticality = new()
    {
        Name = "criticality",
        Type = "enum",
        Values = ["critical", "important", "supporting"],
        Ordered = true
    };

    private static readonly FieldSpec Unranked = new()
    {
        Name = "criticality",
        Type = "enum",
        Values = ["critical", "important", "supporting"]
    };

    private static readonly FieldSpec DependsOn = new()
        { Name = "depends-on", Type = "list", Of = "id", Refs = ["services"] };

    private static readonly FieldSpec Stores = new()
        { Name = "depends-on", Type = "list", Of = "id", Refs = ["data"] };

    private static TypeSchema Type(FieldSpec graded, FieldSpec edge) => new()
    {
        Key = "services",
        Folder = "services",
        FieldOrder = [graded.Name, edge.Name],
        Fields = new Dictionary<string, FieldSpec> { [graded.Name] = graded, [edge.Name] = edge }
    };

    private static List<Finding> Check(params (string Id, string? Grade, string[] Targets)[] records)
        => Check(Type(Criticality, DependsOn), records);

    // The grade and the edges are written under whichever fields the type declares, so the same graph
    // can be laid on a ranked field and on an unranked one.
    private static List<Finding> Check(TypeSchema type,
        params (string Id, string? Grade, string[] Targets)[] records)
    {
        var schema = new Schema { ByFolder = new Dictionary<string, TypeSchema> { ["services"] = type } };
        var graded = type.FieldOrder[0];
        var edge = type.FieldOrder[1];

        var docs = records
            .Select(r => Doc.Parse($"services/{r.Id}.md",
                $"---\nid: {r.Id}\n{graded}:{Written(r.Grade)}\n{edge}:\n"
                + string.Concat(r.Targets.Select(t => $"  - {t}\n"))
                + $"---\n\n# {r.Id}\n", schema))
            .OfType<Doc>()
            .ToList();
        Assert.Equal(records.Length, docs.Count);

        var byId = docs.ToDictionary(d => d.Scalar("id"), d => d, StringComparer.OrdinalIgnoreCase);

        var found = new List<Finding>();

        new DependencyCriticality().Check(new CorpusRuleContext(docs, byId, Empty, type,
            new RuleSpec { Id = new RuleId("dependency-criticality") },
            new Dictionary<string, string>(StringComparer.Ordinal), null,
            (at, c, m, l) => found.Add(new Finding(at.Rel, l, Sev.Error, c, m)),
            (at, c, m, l) => found.Add(new Finding(at.Rel, l, Sev.Warning, c, m))));
        return found;
    }

    // A record with no grade is written as a bare key, which is how a corpus states an absent value.
    private static string Written(string? grade) => grade is null ? "" : $" {grade}";

    // The rule reads the records and never the tree, so an empty corpus of files is the honest thing to
    // hand it.
    private static readonly Tree Empty = new(new HashSet<string>(StringComparer.Ordinal), _ => "");

    private static Finding Single(List<Finding> found) => Assert.Single(found);
}
