using kac.core;

// A report answers for a version of a corpus, and this is what holds that against the version in front
// of the reader. The fixture turns the id green; these are the branches a fixture would only duplicate.

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

    // A report ahead of the corpus is somebody's mistake rather than staleness, and a different message
    // would be needed to say anything useful about it. This rule reports the one direction it is about.
    [Fact]
    public void A_report_ahead_of_the_corpus_is_left_alone()
        => Assert.Empty(Run("0.3.0", ("eng", "0.2.0")));

    // A corpus this one does not read is a comparison that cannot be made. A report carried in from
    // elsewhere names corpora nothing here consumes.
    [Fact]
    public void A_source_naming_a_corpus_this_one_does_not_read_is_left_alone()
        => Assert.Empty(Run("0.1.0", ("other", "0.2.0")));

    // The finding points at the entry rather than at the field, because a report answering for three
    // corpora has three lines and only one of them moved.
    [Fact]
    public void The_finding_points_at_the_source_entry()
        => Assert.Equal(8, Assert.Single(Run("0.1.0", ("eng", "0.2.0"))).Line);

    // A version neither side can order is left alone. `VersionRange.Newer` answers false for it, which
    // is the same answer `Admits` gives, and reporting a comparison nobody made would be worse.
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
            known.ToDictionary(k => k.Corpus, k => k.Version, StringComparer.Ordinal),
            (at, c, m, l) => found.Add(new Finding(at.Rel, l, Sev.Error, c, m)),
            (at, c, m, l) => found.Add(new Finding(at.Rel, l, Sev.Warning, c, m))));

        return found;
    }
}
