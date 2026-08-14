using kac.core;

// In-process unit tests for how `identity-type` names the type the reader is actually looking at. The
// fixture corpus covers the check on a policy, where the article is the ordinary one; what needs a
// declaration of its own is a label that reads differently — an initialism, and a plural that is the
// same word as its singular.

namespace kac.tests;

public class IdentityLineTests
{
    [Fact]
    public void A_label_read_letter_by_letter_takes_the_article_its_first_letter_wants()
        => Assert.Equal("identity line says 'Standard', but this is an ADR.",
            IdentityType(Type("adrs", "ADR", "ADRs"), "adr-0001"));

    [Fact]
    public void An_ordinary_label_takes_the_article_its_first_sound_wants()
        => Assert.Equal("identity line says 'Standard', but this is a Service.",
            IdentityType(Type("services", "Service", "Services"), "svc-catalogue"));

    // A type whose singular and plural are one word is a mass noun, and "a Data" is the reading that
    // gives it away.
    [Fact]
    public void A_label_whose_plural_is_the_same_word_takes_no_article()
        => Assert.Equal("identity line says 'Standard', but this is Data.",
            IdentityType(Type("data", "Data", "Data"), "dat-borrowers"));

    // -- driving the check --

    private static TypeSchema Type(string key, string label, string plural) =>
        new() { Key = key, Folder = key, Label = label, LabelPlural = plural };

    // The whole document pass runs, because the identity line is read where a reader meets it: beneath
    // the H1, against the type the folder resolves to. The pass has plenty else to say about a document
    // this bare, so the finding under test is picked out of what it produced. Nothing here links
    // anywhere, which is what leaves the repository root free to be any real directory.
    private static string IdentityType(TypeSchema type, string id)
    {
        var schema = new Schema { ByFolder = new Dictionary<string, TypeSchema> { [type.Key] = type } };
        var text = $"---\nid: {id}\n---\n\n# A record\n\n`Standard: {id}` `DRAFT`\n";
        var doc = Doc.Parse($"{type.Folder}/{id}.md", text, schema);
        Assert.NotNull(doc);

        var found = new List<Finding>();
        Validator.CheckDocument(doc, schema, Directory.GetCurrentDirectory(), found);
        return Assert.Single(found, x => x.Check.Value == "identity-type").Message;
    }
}
