using kac.core;

namespace kac.tests;

public class ReportStaleTests
{
    [Fact]
    public void A_report_behind_the_corpus_is_reported()
        => Assert.Equal(
            "this answers for 'eng' at 0.1.0, and the corpus is at 0.2.0. Check the report still holds "
            + "and raise the version, or run it again.",
            Assert.Single(Run("0.1.0", ("eng", "0.2.0"))).Message);

    [Fact]
    public void A_report_at_the_corpus_version_is_left_alone()
        => Assert.Empty(Run("0.2.0", ("eng", "0.2.0")));

    [Fact]
    public void A_report_ahead_of_the_corpus_is_left_alone()
        => Assert.Empty(Run("0.3.0", ("eng", "0.2.0")));

    [Fact]
    public void A_source_naming_a_corpus_this_one_does_not_read_is_left_alone()
        => Assert.Empty(Run("0.1.0", ("other", "0.2.0")));

    [Fact]
    public void The_finding_points_at_the_source_entry()
        => Assert.Equal(9, Assert.Single(Run("0.1.0", ("eng", "0.2.0"))).Line);

    [Fact]
    public void A_version_the_tool_cannot_order_is_left_alone()
        => Assert.Empty(Run("whenever", ("eng", "0.2.0")));

    private static List<Finding> Run(string answersFor, params (string Corpus, string Version)[] known)
    {
        var text = "---\nid: rpt-coverage\ntype: report\ntier: descriptive\nstatus: active\n"
                   + "owner: human:alex.doe\ngenerated: { at: 2026-09-08T10:00:00Z, by: kac/0.24.0 }\n"
                   + $"sources:\n  - {{ resource: eng, version: \"{answersFor}\" }}\n"
                   + "verified:\n  - { at: 2026-09-08T11:00:00Z, by: human:alex.doe }\n---\n\n"
                   + "# Clause coverage\n\n`Report: rpt-coverage` `ACTIVE`\n";

        var schema = new Schema
        {
            ByFolder = new Dictionary<string, TypeSchema>
            {
                ["reports"] = new() { Key = "reports", IdPrefix = "rpt" }
            }
        };

        var doc = Doc.Parse("reports/coverage.md", text, schema);
        Assert.NotNull(doc);

        var found = new List<Finding>();
        new ReportStale().Check(new CorpusRuleContext(
            [doc], new Dictionary<string, Doc> { ["rpt-coverage"] = doc },
            new Tree(new HashSet<string>([doc.Rel], StringComparer.Ordinal), _ => text),
            doc.TypeOf(), new RuleSpec { Id = new RuleId("report-stale") },
            known.ToDictionary(k => k.Corpus, k => k.Version, StringComparer.Ordinal), null,
            (at, c, m, l) => found.Add(new Finding(at.Rel, l, Sev.Error, c, m)),
            (at, c, m, l) => found.Add(new Finding(at.Rel, l, Sev.Warning, c, m))));

        return found;
    }
}
