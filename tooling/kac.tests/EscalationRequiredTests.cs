using kac.core;

// A diagnosis tree strands a reader one branch at a time, so the rule reads branches and the message
// quotes the one at fault. The fixtures turn the two ids green; these are the branches a fixture would
// only duplicate, and the forms the three runbooks in this repository actually write.

namespace kac.tests;

public class EscalationRequiredTests
{
    private const string Routes = "* **Yes** → go to [Resolution](#resolution).\n";
    private const string Escalates = "* **No** → [escalate](#escalation).\n";

    [Fact]
    public void A_tree_whose_branches_all_route_is_left_alone()
        => Assert.Empty(Run(Routes + Escalates));

    [Fact]
    public void A_branch_that_links_nowhere_is_reported()
        => Assert.Equal(
            "this diagnosis branch routes nowhere: \"Yes → restart the consumer.\". Send it to "
            + "`[Resolution](#resolution)` or to `[escalate](#escalation)`, or write `continue`.",
            Assert.Single(Run("* **Yes** → restart the consumer.\n" + Escalates,
                c => c.Check == new CheckId("diagnosis-dead-end"))).Message);

    // The message quotes the branch with its runs of space collapsed. `**Yes**` and the arrow arrive as
    // separate literals, so the text is joined rather than read off one run.
    [Fact]
    public void The_message_quotes_the_branch_with_single_spaces()
        => Assert.Contains("\"Yes → restart the consumer.\"",
            Assert.Single(Run("* **Yes** → restart the consumer.\n" + Escalates,
                c => c.Check == new CheckId("diagnosis-dead-end"))).Message);

    [Fact]
    public void The_finding_points_at_the_branch()
        => Assert.Equal(18, Assert.Single(Run("* **Yes** → restart the consumer.\n" + Escalates,
            c => c.Check == new CheckId("diagnosis-dead-end"))).Line);

    // `continue` falls through to the next question, which is a route and not a dead end.
    [Fact]
    public void A_branch_that_falls_through_to_a_later_question_is_left_alone()
        => Assert.Empty(Run("* **Yes** → continue.\n" + Escalates
                            + "\n**Is it the other thing?**\n\n" + Routes + Escalates));

    [Fact]
    public void A_branch_that_falls_through_under_the_last_question_is_reported()
        => Assert.Equal(
            "this diagnosis branch says `continue` under the last question: \"Yes → continue.\". "
            + "Nothing follows it, so link it to `#resolution` or to `#escalation`.",
            Assert.Single(Run("* **Yes** → continue.\n" + Escalates,
                c => c.Check == new CheckId("diagnosis-dead-end"))).Message);

    // `restore-cannot-find-version.md` writes the label this way.
    [Fact]
    public void An_escalation_link_labelled_in_capitals_is_read()
        => Assert.Empty(Run(Routes + "* **No** → [Escalate](#escalation).\n"));

    // `goldens-disagree.md` writes the link mid-sentence, so the branch is read for a link and not for
    // what it ends on.
    [Fact]
    public void A_link_before_the_end_of_the_branch_is_read()
        => Assert.Empty(Run("* **Yes** → go to [Resolution](#resolution) and start at step 4.\n" + Escalates));

    [Fact]
    public void A_tree_that_never_escalates_is_reported()
        => Assert.Equal(
            "no branch of this diagnosis reaches `#escalation`, so a reader the tree does not answer has "
            + "nowhere to go. Close one branch with `[escalate](#escalation)`.",
            Assert.Single(Run(Routes + "* **No** → go to [Resolution](#resolution).\n")).Message);

    // `database-connections-exhausted.md` writes its diagnosis as prose. Whether a tree is owed at all
    // is a question about the section, and this rule reads the branches under it.
    [Fact]
    public void A_diagnosis_written_as_prose_is_left_alone()
        => Assert.Empty(Run("Check the pool size against the connection count.\n"));

    [Fact]
    public void A_runbook_with_no_diagnosis_is_left_alone()
        => Assert.Empty(RunWhole("# A runbook\n\n`Runbook: rbk-x` `ACTIVE`\n\n## Resolution\n\nRestart it.\n"));

    // The rule reads one section, so a tree written under another heading is not its business.
    [Fact]
    public void A_list_outside_Diagnosis_is_left_alone()
        => Assert.Empty(RunWhole("# A runbook\n\n`Runbook: rbk-x` `ACTIVE`\n\n## Diagnosis\n\n"
                                 + Routes + Escalates + "\n## Resolution\n\n* **Yes** → restart the consumer.\n"));

    private static List<Finding> Run(string diagnosis, Func<Finding, bool>? where = null)
    {
        var found = RunWhole("# A runbook\n\n`Runbook: rbk-x` `ACTIVE`\n\n## Diagnosis\n\n"
                             + "**Is it the thing?**\n\n" + diagnosis);
        return where is null ? found : found.Where(where).ToList();
    }

    private static List<Finding> RunWhole(string body)
    {
        var text = "---\nid: rbk-x\ntype: runbook\ntier: procedural\nstatus: active\n"
                   + "severity: sev2\nowner: human:alex.doe\n---\n\n" + body;

        var schema = new Schema
        {
            ByFolder = new Dictionary<string, TypeSchema>
            {
                ["runbooks"] = new() { Key = "runbooks", IdPrefix = "rbk" }
            }
        };

        var doc = Required.Parsed("runbooks/a-runbook.md", text, schema);

        var found = new List<Finding>();
        new EscalationRequired().Check(new RuleContext(
            doc, doc.TypeOf(), new RuleSpec { Id = new RuleId("escalation-required") },
            new Report(doc.Rel, found)));

        return found;
    }
}
