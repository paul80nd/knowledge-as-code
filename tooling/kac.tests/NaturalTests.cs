// In-process unit tests for the natural comparer behind the `list-order` check. The fixtures pin
// the check's behaviour on real documents; this pins the ordering rule itself, including the edges
// no frontmatter in the corpus reaches yet.

using kac.core;

namespace kac.tests;

public class NaturalTests
{
    [Theory]
    [InlineData("alerting", "logging", -1)] // plain alphabetical
    [InlineData("logging", "alerting", 1)]
    [InlineData("access-control", "access-control", 0)]
    [InlineData("api", "apiary", -1)]                               // a prefix sorts first
    [InlineData("ISO27001:2022 A.8.7", "ISO27001:2022 A.8.29", -1)] // digits compare as numbers
    [InlineData("ISO27001:2022 A.5.4", "ISO27001:2022 A.5.36", -1)] // ...where bytes would invert
    [InlineData("ISO27001:2022 A.5.36", "ISO27001:2022 A.8.2", -1)] // the earlier run decides
    [InlineData("7", "108", -1)]                                    // digit count first
    [InlineData("42", "42", 0)]
    // Leading zeros carry no magnitude, so neither of these pairs is decided by the digit run; the
    // ordinal tie-break then settles them, which keeps the order total rather than reporting equal.
    [InlineData("adr-0007", "adr-7", -1)]
    [InlineData("Alerting", "alerting", -1)]
    [InlineData("Beta", "alpha", 1)] // case never outranks the letter
    public void Compare_orders_as_a_reader_would(string a, string b, int expected)
        => Assert.Equal(expected, Math.Sign(Natural.Compare(a, b)));

    [Theory]
    [InlineData("alerting", "logging")]
    [InlineData("ISO27001:2022 A.8.7", "ISO27001:2022 A.8.29")]
    [InlineData("Alerting", "alerting")]
    public void Compare_is_antisymmetric(string a, string b)
        => Assert.Equal(-Math.Sign(Natural.Compare(a, b)), Math.Sign(Natural.Compare(b, a)));
}
