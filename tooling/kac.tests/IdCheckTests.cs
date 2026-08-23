using kac.core;

// In-process unit tests for the id and the filename that carries it. Three styles, each asking two
// questions behind two check ids: is this id the shape the type declares, and does it name the file
// it sits in. The coverage gate reads ids, so a fixture tripping `id-format` on one style leaves the
// other two branches green without ever having run them. These are where the styles are told apart.

namespace kac.tests;

public class IdCheckTests
{
    private static TypeSchema Numbered() => new()
        { Folder = "adrs", IdPrefix = "adr", IdStyle = "numbered", IdWidth = 4 };

    private static TypeSchema Mnemonic() => new()
        { Folder = "policies", IdPrefix = "pol", IdStyle = "mnemonic", IdWidth = 4 };

    private static TypeSchema Slug() => new()
        { Folder = "tools", IdPrefix = "tol", IdStyle = "slug" };

    // The two part sources, each with the id shape its own declaration gives a part. A policy's clauses
    // are written to a pattern; a glossary's terms are the anchors their headings slug to.
    private static TypeSchema Clauses() => new()
    {
        Folder = "policies", IdPrefix = "pol", IdStyle = "mnemonic", IdWidth = 4,
        Parts = new PartSpec(PartSpec.Table, "^[A-Z]{3,8}$", ["MUST"], [])
    };

    private static TypeSchema Terms() => new()
    {
        Folder = "glossary", IdPrefix = "gls", IdStyle = "slug",
        Parts = new PartSpec(PartSpec.Headings, "", [], []) { Section = "Terms", Level = 3 }
    };

    // -- the prefix, which is the type --

    [Fact]
    public void An_id_under_the_wrong_prefix_is_reported_and_stops()
    {
        var found = Run("xyz-0006", "adrs/0006-a.md", Numbered());
        Assert.Equal("id-prefix", Assert.Single(found).Check.Value);
    }

    // -- the shape of the discriminator, style by style --

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
    [InlineData("tol-Site_Server")] // capitals and an underscore
    [InlineData("tol-site server")] // a space
    [InlineData("tol-Ripgrep")]
    public void A_slug_id_is_lower_case_letters_digits_and_hyphens(string id)
        => Assert.Equal("id-format", Assert.Single(Run(id, "tools/site-server.md", Slug())).Check.Value);

    // The shape is asked first, so a malformed id is one finding. The filename it fails to match is not
    // worth saying while the id itself is unreadable.
    [Fact]
    public void A_malformed_id_is_not_also_held_to_the_filename()
        => Assert.Equal("id-format", Assert.Single(Run("adr-7", "adrs/0004-a.md", Numbered())).Check.Value);

    // -- agreement with the filename --

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

    // A slug is compared entire, where the other two styles match a leading segment. That is the whole
    // difference: `svc-search` in `search-service.md` names another document.
    [Fact]
    public void A_slug_is_compared_whole_and_not_as_a_prefix()
        => Assert.Equal("id-matches-filename",
            Assert.Single(Run("tol-site", "tools/site-server.md", Slug())).Check.Value);

    // Where the filename carries no discriminator in the style's shape it has failed filename-pattern,
    // and one broken name earns one finding.
    [Theory]
    [InlineData("adr-0004", "adrs/no-digits-at-all.md")]
    [InlineData("pol-VURM", "policies/toolong-a.md")]
    [InlineData("tol-bad-name", "tools/Bad_Name.md")]
    public void A_filename_that_carries_no_discriminator_is_left_to_filename_pattern(string id, string rel)
        => Assert.Empty(Run(id, rel, TypeFor(rel)));

    // -- what slug-max measures --

    private const string TooLong = "slug-that-is-definitely-way-too-long";

    // The same stem under two styles. A numbered type's `0003-` is a discriminator the author did not
    // choose and is not counted; under a slug type the whole stem is the name they did choose.
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

    [Fact]
    public void A_slug_within_the_limit_is_silent()
        => Assert.Empty(Filename("tools/ripgrep.md", Slug()));

    // -- is this label an id? --

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

    // Anything else is prose in brackets, and warns as `bracket-literal`.
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

    // -- and is it a part of one? --

    // A part is addressed as `<record>.<part>`, which is the form a citation uses and now the form a link
    // to a part uses. The record half canonicalises as any id does; the part half is judged against the
    // type's own `parts:` block, because only that says what a part id looks like.
    [Theory]
    [InlineData("pol-scrt.TIMEBOX", "pol-SCRT.TIMEBOX")] // the record half is written back upper
    [InlineData("POL-SCRT.TIMEBOX", "pol-SCRT.TIMEBOX")]
    [InlineData("gls-Search.title", "gls-search.title")] // a heading part is the anchor it slugs to
    public void A_part_shaped_label_is_recognised_and_given_its_canonical_form(string label, string expected)
    {
        Assert.True(IdChecks.TryCanonicalId(label, SchemaWith(Clauses(), Terms()), out var canonical));
        Assert.Equal(expected, canonical);
    }

    // The part half has to fit the declaration, and a type declaring no parts has none to be named. That
    // is what keeps `pol-DEVI.md` and a filename in brackets out of this.
    [Theory]
    [InlineData("pol-SCRT.timebox")]  // the declared pattern is upper-case
    [InlineData("pol-SCRT.md")]       // a file extension is not a clause
    [InlineData("gls-search.Title")]  // a heading part is its slug, and a slug is lower-case
    [InlineData("adr-0001.context")]  // the type declares no parts at all
    [InlineData("pol-SCRT.")]         // nothing after the dot
    [InlineData(".TIMEBOX")]          // nothing before it
    public void A_part_that_does_not_fit_its_type_s_declaration_is_prose(string label)
        => Assert.False(IdChecks.TryCanonicalId(label, SchemaWith(Clauses(), Terms(), Numbered()), out _));

    // -- which id does a link cite? --

    [Theory]
    [InlineData("0007-a-decision.md", "adr-0007")] // relative, from a document in the folder
    [InlineData("/adrs/0007-a-decision.md", "adr-0007")]
    [InlineData("../adrs/0007-a-decision.md", "adr-0007")]
    [InlineData("0007-a-decision.md#context", "adr-0007")] // a fragment addresses within the target
    [InlineData("0007-a-decision?raw=1", "adr-0007")]      // the extension may be left off
    public void A_link_to_a_numbered_record_is_read_as_its_id(string target, string expected)
        => Assert.Equal(expected, Cite(target, "adrs/0001-first.md", Numbered()));

    // The filename carries the mnemonic lower-case; the id it cites is the upper-case form.
    [Fact]
    public void A_link_to_a_mnemonic_record_is_written_back_upper_case()
        => Assert.Equal("pol-VURM", Cite("/policies/vurm-vulnerability.md", "adrs/0001-first.md", Mnemonic()));

    // The style with nothing distinctive in the filename to recognise, and so the one the folder is
    // load-bearing for.
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

    // -- driving the checks --

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
