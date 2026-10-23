namespace kac.tests;

public class RecordPathTests
{
    private static readonly HashSet<string> Types = new(StringComparer.Ordinal) { "standards", "policies" };

    [Fact]
    public void A_record_of_a_named_type_is_a_record()
        => Assert.True(RecordPath.IsRecord("standards/naming.md", Types));

    [Fact]
    public void A_record_of_a_type_nobody_named_is_not()
        => Assert.False(RecordPath.IsRecord("data/rooms.md", Types));
}
