namespace kac.tests;

public class VerificationComparisonTests
{
    [Fact]
    public void A_rewritten_resolution_under_an_unmoved_list_is_reported()
        => Assert.True(Verification.Unverified(
            Record("Restart the runner.", Paul),
            Record("Clear the cache and restart the runner.", Paul)));

    [Fact]
    public void A_rewritten_resolution_signed_again_is_left_alone()
        => Assert.False(Verification.Unverified(
            Record("Restart the runner.", Paul),
            Record("Clear the cache and restart the runner.", Paul, Alex)));

    /// <summary>The dates differ, which is what separates the two entries.</summary>
    [Fact]
    public void A_second_reading_by_the_same_person_is_left_alone()
        => Assert.False(Verification.Unverified(
            Record("Restart the runner.", Paul),
            Record("Clear the cache and restart the runner.", Paul, PaulAgain)));

    [Fact]
    public void A_frontmatter_key_moved_on_its_own_is_left_alone()
        => Assert.False(Verification.Unverified(
            Record("Restart the runner.", Paul),
            Record("Restart the runner.", Paul).Replace("status: active", "status: fixed-upstream")));

    /// <summary>The identity line restates the frontmatter's id and status, which nobody verifies as prose.</summary>
    [Fact]
    public void An_id_renamed_on_the_identity_line_is_left_alone()
        => Assert.False(Verification.Unverified(
            Record("Restart the runner.", Paul),
            Record("Restart the runner.", Paul).Replace("fix-runner", "fix-runner-stalls")));

    [Fact]
    public void A_record_nobody_had_verified_is_left_alone()
        => Assert.False(Verification.Unverified(
            Record("Restart the runner."),
            Record("Clear the cache and restart the runner.")));

    [Fact]
    public void A_verification_withdrawn_from_an_unchanged_body_is_left_alone()
        => Assert.False(Verification.Unverified(Record("Restart the runner.", Paul, Alex),
            Record("Restart the runner.", Paul)));

    /// <summary>
    /// Rider rewraps markdown here from <c>.editorconfig</c>, in files a session never opened. The words are the ones
    /// somebody verified, so the entry beside them still answers for the record.
    /// </summary>
    [Fact]
    public void A_reflowed_paragraph_is_left_alone()
        => Assert.False(Verification.Unverified(
            Record("Clear the cache and restart the runner.", Paul),
            Record("Clear the cache\nand restart the runner.", Paul)));

    [Fact]
    public void A_file_carrying_no_frontmatter_is_left_alone()
        => Assert.False(Verification.Unverified("# A page\n", "# A page rewritten\n"));

    private const string Paul = "{ at: 2026-09-09T08:14:30Z, by: human:paul.law }";
    private const string PaulAgain = "{ at: 2026-09-14T11:02:00Z, by: human:paul.law }";
    private const string Alex = "{ at: 2026-09-14T11:02:00Z, by: human:alex.doe }";

    private static string Record(string resolution, params string[] verified)
    {
        var entries = verified.Length == 0
            ? ""
            : "verified:\n" + string.Concat(verified.Select(v => $"  - {v}\n"));

        return "---\nid: fix-runner\ntype: fix\ntier: normative\nstatus: active\n"
               + "owner: human:paul.law\nsymptom-keywords: [ runner ]\n" + entries
               + "---\n\n# The runner stalls\n\n`Fix: fix-runner` `ACTIVE`\n\n## Resolution\n\n"
               + resolution + "\n";
    }
}
