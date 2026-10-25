using kac.core;

namespace kac.tests;

public class AcceptancesTests
{
    private const string Rel = "services/kac.md";

    private static readonly IReadOnlyList<CheckDef> Catalogue =
    [
        new(new CheckId("critical-service-is-alerted"), Sev.Warning, "A critical service emits something."),
        new(new CheckId("list-order"), Sev.Warning, "Lists read in order."),
        new(new CheckId("h1"), Sev.Error, "The document has an H1."),
        new(new CheckId("import-capped"), Sev.Info, "An import is capped."),
        new(Acceptances.Stale, Sev.Warning, "An entry matches a warning."),
        new(Acceptances.Refused, Sev.Error, "An entry names a warning.")
    ];

    [Fact]
    public void An_accepted_warning_is_reported_as_info_with_the_reason()
    {
        var found = Apply(Accepting("critical-service-is-alerted", "kac emits nothing at run time."),
            Warning("critical-service-is-alerted", 6));

        var finding = Assert.Single(found);
        Assert.Equal(Sev.Info, finding.Severity);
        Assert.Equal("critical-service-is-alerted", finding.Check.Value);
        Assert.Equal("accepted: kac emits nothing at run time.", finding.Message);
        Assert.Equal(6, finding.Line);
    }

    [Fact]
    public void One_entry_accepts_every_finding_of_its_check_on_the_record()
    {
        var found = Apply(Accepting("list-order", "Kept in the order the steps run."),
            Warning("list-order", 4), Warning("list-order", 9));

        Assert.Equal(2, found.Count);
        Assert.All(found, f => Assert.Equal(Sev.Info, f.Severity));
    }

    [Fact]
    public void An_entry_leaves_the_same_warning_on_another_record_standing()
    {
        var elsewhere = new Finding("services/other.md", 6, Sev.Warning, new CheckId("critical-service-is-alerted"),
            "this service is `critical` and emits nothing when it fails.");

        var found = Apply(Accepting("critical-service-is-alerted", "kac emits nothing at run time."),
            Warning("critical-service-is-alerted", 6), elsewhere);

        Assert.Equal(Sev.Warning, Assert.Single(found, f => f.File == "services/other.md").Severity);
    }

    [Fact]
    public void An_entry_leaves_an_error_under_its_check_standing()
    {
        var error = new Finding(Rel, 6, Sev.Error, new CheckId("list-order"), "an error under a warning's id.");

        var found = Apply(Accepting("list-order", "Kept in the order the steps run."), error);

        Assert.Contains(error, found);
        Assert.Single(found, f => f.Check == Acceptances.Stale);
    }

    [Fact]
    public void An_entry_matching_no_warning_is_stale_and_names_the_check()
    {
        var finding = Assert.Single(Apply(Accepting("list-order", "Kept in the order the steps run.")));

        Assert.Equal(Sev.Warning, finding.Severity);
        Assert.Equal(Acceptances.Stale, finding.Check);
        Assert.Equal(5, finding.Line);
        Assert.Equal("no 'list-order' warning is left on this record for this entry to accept. "
                     + "Remove the entry from 'accepts'.", finding.Message);
    }

    [Fact]
    public void A_second_entry_for_the_same_check_is_stale()
    {
        var doc = Record("""
                         accepts:
                           - { check: list-order, reason: "Kept in the order the steps run." }
                           - { check: list-order, reason: "Said twice." }
                         """);

        var found = Apply(doc, Warning("list-order", 4));

        Assert.Equal(Sev.Info, Assert.Single(found, f => f.Check.Value == "list-order").Severity);
        Assert.Equal(6, Assert.Single(found, f => f.Check == Acceptances.Stale).Line);
    }

    [Fact]
    public void An_entry_naming_a_check_that_reports_errors_is_an_error()
    {
        var error = new Finding(Rel, null, Sev.Error, new CheckId("h1"), "missing H1.");

        var found = Apply(Accepting("h1", "The page has no title."), error);

        Assert.Contains(error, found);
        var refused = Assert.Single(found, f => f.Check == Acceptances.Refused);
        Assert.Equal(Sev.Error, refused.Severity);
        Assert.Equal(5, refused.Line);
        Assert.Equal("'h1' reports errors, and 'accepts' takes a warning only. Fix what it reports, and remove "
                     + "the entry.", refused.Message);
    }

    [Fact]
    public void An_entry_naming_a_check_that_reports_info_is_an_error()
    {
        var refused = Assert.Single(Apply(Accepting("import-capped", "Capped on purpose.")));

        Assert.Equal(Acceptances.Refused, refused.Check);
        Assert.Equal("'import-capped' reports info, and 'accepts' takes a warning only. Remove the entry.",
            refused.Message);
    }

    [Fact]
    public void An_entry_naming_an_unknown_check_is_an_error()
    {
        var refused = Assert.Single(Apply(Accepting("critical-servce-is-alerted", "A typo.")));

        Assert.Equal(Acceptances.Refused, refused.Check);
        Assert.Equal(Sev.Error, refused.Severity);
        Assert.Equal("'critical-servce-is-alerted' is not a check `kac validate` reports. `kac checks` lists "
                     + "every check id.", refused.Message);
    }

    [Fact]
    public void An_entry_naming_acceptance_stale_is_an_error()
    {
        var refused = Assert.Single(Apply(Accepting("acceptance-stale", "Keep the entry.")));

        Assert.Equal(Acceptances.Refused, refused.Check);
        Assert.Equal("'acceptance-stale' cannot be accepted. Remove the entry it reports, and then this one.",
            refused.Message);
    }

    [Fact]
    public void An_entry_missing_its_check_or_its_reason_is_left_to_entry_key()
    {
        var doc = Record("""
                         accepts:
                           - { check: list-order }
                           - { reason: "Names no check." }
                         """);

        var warning = Warning("list-order", 4);
        Assert.Equal([warning], Apply(doc, warning));
    }

    [Fact]
    public void A_record_with_no_accepts_leaves_its_findings_alone()
    {
        var warning = Warning("list-order", 4);
        Assert.Equal([warning], Apply(Record("tags: [ b, a ]"), warning));
    }

    private static Finding Warning(string check, int line) =>
        new(Rel, line, Sev.Warning, new CheckId(check), $"a '{check}' warning.");

    /// <summary>The one entry sits on line 5, below the opening fence, <c>id</c>, <c>type</c> and the key.</summary>
    private static Doc Accepting(string check, string reason) =>
        Record($"accepts:\n  - {{ check: {check}, reason: \"{reason}\" }}");

    private static Doc Record(string front)
    {
        var type = new TypeSchema { Key = "services", Folder = "services", Label = "Service", LabelPlural = "Services" };
        var schema = new Schema { ByFolder = new Dictionary<string, TypeSchema> { [type.Key] = type } };
        var doc = Doc.Parse(Rel, $"---\nid: svc-kac\ntype: service\n{front}\n---\n\n# kac\n", schema);
        Assert.NotNull(doc);
        return doc;
    }

    private static List<Finding> Apply(Doc doc, params Finding[] findings)
    {
        var all = findings.ToList();
        Acceptances.Apply([doc], Catalogue, all);
        return all;
    }
}
