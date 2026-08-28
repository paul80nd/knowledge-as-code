using kac.core;

// In-process unit tests for `shortcode`: the shorthand another corpus cites this one by, and every
// spelling it may not take.
//
// The `shortcode` fixture covers a corpus that gets it wrong through the CLI, and covers the id once,
// which is all the coverage gate asks. Every other spelling lives here, where a case is one string.

namespace kac.tests;

public class ShortcodeTests
{
    // A key written with no value never reaches here: the loader reads an empty scalar as absent, which
    // `ManifestTests` holds it to.
    [Fact]
    public void A_corpus_declaring_no_shortcode_is_not_asked_anything()
        => Assert.Empty(Shortcode(null));

    [Theory]
    [InlineData("eng")]
    [InlineData("ex")]
    [InlineData("pay16")]
    [InlineData("acme2026")]
    public void A_lower_case_shortcode_of_letters_and_digits_is_silent(string shortcode)
        => Assert.Empty(Shortcode(shortcode));

    [Fact]
    public void A_shortcode_of_one_character_is_too_short()
        => Assert.Contains("is too short", Assert.Single(Shortcode("e")).Message);

    [Fact]
    public void A_shortcode_of_nine_characters_is_too_long()
        => Assert.Contains("is too long", Assert.Single(Shortcode("acme20264")).Message);

    [Theory]
    [InlineData("Eng")]
    [InlineData("9ng")]
    [InlineData("-ng")]
    public void A_shortcode_opening_on_anything_but_a_lower_case_letter_is_refused(string shortcode)
        => Assert.Contains("does not open on a lower-case letter", Assert.Single(Shortcode(shortcode)).Message);

    // A hyphen is the one to hold, and it is why `.` and `:` are not the only marks refused.
    // `Document.cs` collects a colon-separated citation by matching an id on the left of the colon, and a
    // hyphen inside a shortcode is what would make a scoped reference look like one.
    [Theory]
    [InlineData("my-corp")]
    [InlineData("eng.uk")]
    [InlineData("engUK")]
    public void A_shortcode_carrying_anything_but_a_lower_case_letter_or_a_digit_is_refused(string shortcode)
        => Assert.Contains("carries something other than", Assert.Single(Shortcode(shortcode)).Message);

    [Fact]
    public void A_shortcode_a_type_uses_as_its_id_prefix_names_the_type()
        => Assert.Contains("is the id prefix of 'standards'", Assert.Single(Shortcode("std")).Message);

    // The prefixes are read from the schema, so a spelling no declared type uses is free.
    [Fact]
    public void A_shortcode_no_type_uses_is_left_alone()
        => Assert.Empty(Shortcode("adr2"));

    // The prefix comparison ignores case, so correcting the casing alone does not leave the second
    // refusal waiting on the next run.
    [Fact]
    public void A_miscased_type_prefix_is_reported_as_both()
    {
        var findings = Shortcode("STD");

        Assert.Equal(2, findings.Count);
        Assert.Contains(findings, f => f.Message.Contains("does not open on a lower-case letter"));
        Assert.Contains(findings, f => f.Message.Contains("is the id prefix of 'standards'"));
    }

    // Two types, so that one spelling is taken and the rest are free. Nothing here is stood up: the pass
    // reads the descriptor and never the listing.
    private static List<Finding> Shortcode(string? shortcode)
    {
        var schema = new Schema
        {
            ByFolder = new Dictionary<string, TypeSchema>(StringComparer.Ordinal)
            {
                ["adrs"] = new() { Key = "adrs", TypeName = "adrs", Folder = "adrs", IdPrefix = "adr" },
                ["standards"] = new()
                {
                    Key = "standards", TypeName = "standards", Folder = "standards", IdPrefix = "std"
                }
            }
        };

        var tree = new Tree(new HashSet<string>(StringComparer.Ordinal), _ => "", _ => false);
        var descriptor = new CorpusDescriptor { Shortcode = shortcode };

        return
        [
            .. Validator.CheckAll(Corpus.Load(tree, schema, descriptor))
                .Where(f => f.Check.Value == "shortcode")
        ];
    }
}
