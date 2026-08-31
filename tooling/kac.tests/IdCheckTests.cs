using kac.core;

// In-process unit tests for the id and the filename that carries it. Three styles, each asking two
// questions behind two check ids: is this id the shape the type declares, and does it name the file
// it sits in. The coverage gate reads ids, so a fixture tripping `id-format` on one style leaves the
// other two branches green without ever having run them. These are where the styles are told apart.

namespace kac.tests;

public class IdCheckTests
{
    private static TypeSchema Numbered() => new()
        { Folder = "adrs", IdPrefix = "adr", IdStyle = "numbered", IdWidth = new(4, 4) };

    private static TypeSchema Mnemonic() => new()
        { Folder = "policies", IdPrefix = "pol", IdStyle = "mnemonic", IdWidth = new(4, 4) };

    private static TypeSchema Slug() => new()
        { Folder = "tools", IdPrefix = "tol", IdStyle = "slug" };

    // A mnemonic as long as the word it names, filed under a topical slug that carries none of it.
    private static TypeSchema Span() => new()
    {
        Folder = "standards", IdPrefix = "std", IdStyle = "mnemonic", IdWidth = new(2, 7),
        FilenameCarriesId = false
    };

    // The two part sources, each with the id shape its own declaration gives a part. A policy's clauses
    // are written to a pattern; a glossary's terms are the anchors their headings slug to.
    private static TypeSchema Clauses() => new()
    {
        Folder = "policies", IdPrefix = "pol", IdStyle = "mnemonic", IdWidth = new(4, 4),
        Parts = new PartSpec(PartSpec.Table, "^[A-Z]{3,8}$", ["MUST"], [])
    };

    private static TypeSchema Terms() => new()
    {
        Folder = "glossary", IdPrefix = "gls", IdStyle = "slug",
        Parts = new PartSpec(PartSpec.Headings, "", [], []) { Section = "Terms", Level = 3 }
    };

    [Fact]
    public void An_id_under_the_wrong_prefix_is_reported_and_stops()
    {
        var found = Run("xyz-0006", "adrs/0006-a.md", Numbered());
        Assert.Equal("id-prefix", Assert.Single(found).Check.Value);
    }

    [Theory]
    [InlineData("adr-7")]     // too few digits
    [InlineData("adr-00071")] // too many
    [InlineData("adr-007a")]  // not digits
    public void A_numbered_id_is_the_declared_width_in_digits(string id)
        => Assert.Equal("id-format", Assert.Single(Run(id, "adrs/0007-a.md", Numbered())).Check.Value);

    [Theory]
    [InlineData("pol-VU")]   // too short
    [InlineData("pol-vurm")] // lower-case
    [InlineData("pol-1URM")] // opens with a digit
    public void A_mnemonic_id_is_upper_case_and_opens_with_a_letter(string id)
        => Assert.Equal("id-format", Assert.Single(Run(id, "policies/vurm-a.md", Mnemonic())).Check.Value);

    [Theory]
    [InlineData("std-PR")]      // the floor
    [InlineData("std-SECRET")]
    [InlineData("std-TESTING")] // the ceiling
    public void A_mnemonic_anywhere_inside_a_declared_span_is_silent(string id)
        => Assert.Empty(Run(id, "standards/version-control.md", Span()));

    [Theory]
    [InlineData("std-P")]        // one short of the floor
    [InlineData("std-TESTINGS")] // one past the ceiling
    public void A_mnemonic_outside_a_declared_span_is_reported_against_the_width(string id)
    {
        var found = Assert.Single(Run(id, "standards/version-control.md", Span()));
        Assert.Equal("id-format", found.Check.Value);
        Assert.Contains("followed by 2 to 7 upper-case", found.Message);
    }

    [Theory]
    [InlineData("tol-Site_Server")] // capitals and an underscore
    [InlineData("tol-site server")] // a space
    [InlineData("tol-Ripgrep")]
    public void A_slug_id_is_lower_case_letters_digits_and_hyphens(string id)
        => Assert.Equal("id-format", Assert.Single(Run(id, "tools/site-server.md", Slug())).Check.Value);

    // The filename a malformed id fails to match is not worth saying while the id itself is unreadable.
    [Fact]
    public void A_malformed_id_is_not_also_held_to_the_filename()
        => Assert.Equal("id-format", Assert.Single(Run("adr-7", "adrs/0004-a.md", Numbered())).Check.Value);

    [Theory]
    [InlineData("adr-0009", "adrs/0004-missing.md")]
    [InlineData("pol-DEVI", "policies/pipe-disagrees.md")]
    [InlineData("tol-names-another-tool", "tools/id-disagrees.md")]
    public void An_id_naming_a_different_document_than_its_file_is_reported(string id, string rel)
        => Assert.Equal("id-matches-filename", Assert.Single(Run(id, rel, TypeFor(rel))).Check.Value);

    [Theory]
    [InlineData("adr-0004", "adrs/0004-missing.md")]
    [InlineData("pol-VURM", "policies/vurm-a.md")] // upper in the id, lower in the filename
    [InlineData("tol-site-server", "tools/site-server.md")]
    public void An_id_agreeing_with_its_filename_is_silent(string id, string rel)
        => Assert.Empty(Run(id, rel, TypeFor(rel)));

    // A leading-segment match would accept `svc-search` in `search-service.md`, which names another
    // document.
    [Fact]
    public void A_slug_is_compared_whole_and_not_as_a_prefix()
        => Assert.Equal("id-matches-filename",
            Assert.Single(Run("tol-site", "tools/site-server.md", Slug())).Check.Value);

    // One broken name earns one finding.
    [Theory]
    [InlineData("adr-0004", "adrs/no-digits-at-all.md")]
    [InlineData("pol-VURM", "policies/toolong-a.md")]
    [InlineData("tol-bad-name", "tools/Bad_Name.md")]
    public void A_filename_that_carries_no_discriminator_is_left_to_filename_pattern(string id, string rel)
        => Assert.Empty(Run(id, rel, TypeFor(rel)));

    // `secret-handling.md` opens with six letters and a hyphen. Under a type that reads the head of a
    // filename, any span reaching six binds it to a `std-SECRET` nobody wrote.
    [Fact]
    public void A_filename_carrying_no_id_is_not_held_to_the_agreement()
        => Assert.Empty(Run("std-VCS", "standards/common/secret-handling.md", Span()));

    // A standard's filename is a topical slug, so a link to one is a link and cites nothing.
    [Fact]
    public void A_link_to_a_record_whose_filename_carries_no_id_cites_nothing()
        => Assert.Null(Cite("/standards/card-data.md", "controls/0001-a.md", Span()));

    private const string TooLong = "slug-that-is-definitely-way-too-long";

    // A numbered type's `0003-` is a discriminator the author did not choose, so it is not counted. Under
    // a slug type the whole stem is the name they did choose.
    [Fact]
    public void The_discriminator_is_excluded_from_the_slug_but_a_slug_id_is_measured_whole()
    {
        var underNumbered = Assert.Single(Filename($"adrs/0003-{TooLong}.md", Numbered()));
        Assert.Equal("slug-length", underNumbered.Check.Value);
        Assert.Contains($"slug '{TooLong}' is 36 characters", underNumbered.Message);

        var underSlug = Assert.Single(Filename($"tools/0003-{TooLong}.md", Slug()));
        Assert.Contains($"slug '0003-{TooLong}' is 41 characters", underSlug.Message);
    }

    [Fact]
    public void A_mnemonic_head_is_excluded_too()
        => Assert.Contains($"slug '{TooLong}' is 36 characters",
            Assert.Single(Filename($"policies/mexp-{TooLong}.md", Mnemonic())).Message);

    // Nothing is cut from the head where nothing there belongs to the id.
    [Fact]
    public void A_filename_carrying_no_id_is_measured_whole()
        => Assert.Contains($"slug 'secr-{TooLong}' is 41 characters",
            Assert.Single(Filename($"standards/secr-{TooLong}.md", Span())).Message);

    [Fact]
    public void A_slug_within_the_limit_is_silent()
        => Assert.Empty(Filename("tools/ripgrep.md", Slug()));

    // `label-canonical` is the difference between a label and the id as a document carries it.
    [Theory]
    [InlineData("adr-0001", "adr-0001")]
    [InlineData("ADR-0001", "adr-0001")] // the prefix is matched without case and written back lower
    [InlineData("pol-scrt", "pol-SCRT")] // a mnemonic is written upper
    [InlineData("POL-Scrt", "pol-SCRT")]
    [InlineData("tol-Ripgrep", "tol-ripgrep")]
    public void An_id_shaped_label_is_recognised_and_given_its_canonical_form(string label, string expected)
    {
        Assert.True(IdChecks.TryCanonicalId(label, SchemaWith(Numbered(), Mnemonic(), Slug()), out var canonical));
        Assert.Equal(expected, canonical);
    }

    // Prose in brackets warns as `bracket-literal`.
    [Theory]
    [InlineData("adr-001")]   // too few digits for the declared width
    [InlineData("adr-00011")] // too many
    [InlineData("adr-000a")]  // not digits
    [InlineData("pol-1SCR")]  // a mnemonic opens with a letter
    [InlineData("xyz-0001")]  // no type carries that prefix
    [InlineData("an unlinked placeholder")]
    [InlineData("-0001")] // nothing before the dash
    [InlineData("adr-")]  // nothing after it
    public void Anything_else_is_prose_in_brackets(string label)
        => Assert.False(IdChecks.TryCanonicalId(label, SchemaWith(Numbered(), Mnemonic(), Slug()), out _));

    // The record half canonicalises as any id does. The part half is judged against the type's own `parts:`
    // block, because only that says what a part id looks like.
    [Theory]
    [InlineData("pol-scrt.TIMEBOX", "pol-SCRT.TIMEBOX")] // the record half is written back upper
    [InlineData("POL-SCRT.TIMEBOX", "pol-SCRT.TIMEBOX")]
    [InlineData("gls-Search.title", "gls-search.title")] // a heading part is the anchor it slugs to
    public void A_part_shaped_label_is_recognised_and_given_its_canonical_form(string label, string expected)
    {
        Assert.True(IdChecks.TryCanonicalId(label, SchemaWith(Clauses(), Terms()), out var canonical));
        Assert.Equal(expected, canonical);
    }

    // The declaration is what keeps a filename in brackets, such as `pol-DEVI.md`, out of this.
    [Theory]
    [InlineData("pol-SCRT.timebox")] // the declared pattern is upper-case
    [InlineData("pol-SCRT.md")]      // a file extension is not a clause
    [InlineData("gls-search.Title")] // a heading part is its slug, and a slug is lower-case
    [InlineData("adr-0001.context")] // the type declares no parts at all
    [InlineData("pol-SCRT.")]        // nothing after the dot
    [InlineData(".TIMEBOX")]         // nothing before it
    public void A_part_that_does_not_fit_its_type_s_declaration_is_prose(string label)
        => Assert.False(IdChecks.TryCanonicalId(label, SchemaWith(Clauses(), Terms(), Numbered()), out _));

    [Theory]
    [InlineData("0007-a-decision.md", "adr-0007")] // relative, from a document in the folder
    [InlineData("/adrs/0007-a-decision.md", "adr-0007")]
    [InlineData("../adrs/0007-a-decision.md", "adr-0007")]
    [InlineData("0007-a-decision.md#context", "adr-0007")] // a fragment addresses within the target
    [InlineData("0007-a-decision?raw=1", "adr-0007")]      // the extension may be left off
    public void A_link_to_a_numbered_record_is_read_as_its_id(string target, string expected)
        => Assert.Equal(expected, Cite(target, "adrs/0001-first.md", Numbered()));

    // The filename carries the mnemonic lower-case.
    [Fact]
    public void A_link_to_a_mnemonic_record_is_written_back_upper_case()
        => Assert.Equal("pol-VURM", Cite("/policies/vurm-vulnerability.md", "adrs/0001-first.md", Mnemonic()));

    // A slug filename carries nothing distinctive to recognise, so the folder is load-bearing here.
    [Theory]
    [InlineData("/tools/ripgrep.md", "tol-ripgrep")]
    [InlineData("ripgrep.md", "tol-ripgrep")]
    public void A_link_to_a_slug_record_is_read_as_its_id(string target, string expected)
        => Assert.Equal(expected, Cite(target, "tools/fd.md", Slug()));

    // A type page is not a record, and under a slug type its name is shaped exactly like one. The folder
    // is what tells them apart: `tools.md` sits beside the folder rather than in it.
    [Theory]
    [InlineData("/tools.md", "tools/fd.md")]
    [InlineData("../tools.md", "tools/fd.md")]
    [InlineData("/services/lending.md", "tools/fd.md")] // a record, but of another type
    [InlineData("https://example.com/tools/ripgrep.md", "tools/fd.md")]
    public void A_link_outside_the_type_s_folder_cites_no_id(string target, string fromRel)
        => Assert.Null(Cite(target, fromRel, Slug()));

    [Theory]
    [InlineData("/adrs.md")] // the type page, not a record
    [InlineData("/adrs/_template.md")]
    [InlineData("/adrs/007-too-few.md")]
    [InlineData("#a-heading-here")] // a fragment addressing this document
    public void A_link_to_anything_else_cites_no_id(string target)
        => Assert.Null(Cite(target, "adrs/0001-first.md", Numbered()));

    private static Schema SchemaWith(params TypeSchema[] types) =>
        new() { ByFolder = types.ToDictionary(t => t.Folder) };

    // The style follows the folder, so a theory can name a file and mean a type.
    private static TypeSchema TypeFor(string rel) => rel.Split('/')[0] switch
    {
        "adrs" => Numbered(),
        "policies" => Mnemonic(),
        _ => Slug()
    };

    private static string? Cite(string target, string fromRel, TypeSchema refType) =>
        IdChecks.IdFromLink(new LinkRef { Target = target }, fromRel, refType);

    private static List<Finding> Run(string id, string rel, TypeSchema t) => Collect(report =>
        IdChecks.Check(id, 1, rel, t, report));

    private static List<Finding> Filename(string rel, TypeSchema t) => Collect(report =>
        IdChecks.CheckFilename(rel, t, report));

    // These read an id and a filename, never a document, so the file a finding is against is left
    // empty.
    private static List<Finding> Collect(Action<Report> check)
    {
        var found = new List<Finding>();
        check(new Report("", found));
        return found;
    }
}
