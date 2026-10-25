using kac.core;

namespace kac.tests;

public class RefCheckTests
{
    [Fact]
    public void A_target_of_the_type_the_declaration_names_passes()
        => Assert.Empty(Refs(Field("supersedes", "adrs"), "adr-0002"));

    [Fact]
    public void A_target_of_another_type_is_reported_naming_both()
    {
        var found = Refs(Field("supersedes", "adrs"), "svc-catalogue");

        Assert.Equal("ref-resolves", Assert.Single(found).Check.Value);
        Assert.Equal("'supersedes' points at 'svc-catalogue', which is a Service, not an ADR.",
            Assert.Single(found).Message);
    }

    [Fact]
    public void A_declaration_naming_several_types_admits_each_of_them()
        => Assert.Empty(Refs(Field("departs-from", "adrs", "services"), "svc-catalogue"));

    [Fact]
    public void A_declaration_naming_several_types_offers_them_all_when_none_matches()
        => Assert.Equal("'departs-from' points at 'dat-borrowers', which is Data, not an ADR or a Service.",
            Assert.Single(Refs(Field("departs-from", "adrs", "services"), "dat-borrowers")).Message);

    [Fact]
    public void A_type_whose_label_is_a_mass_noun_is_named_without_an_article()
    {
        Assert.Equal("'data-stores' points at 'svc-catalogue', which is a Service, not Data.",
            Assert.Single(Refs(Field("data-stores", "data"), "svc-catalogue")).Message);
        Assert.Equal("'supersedes' points at 'dat-borrowers', which is Data, not an ADR.",
            Assert.Single(Refs(Field("supersedes", "adrs"), "dat-borrowers")).Message);
    }

    [Fact]
    public void A_target_that_does_not_exist_is_reported_as_absent()
        => Assert.Equal("'supersedes' points at 'adr-0099', which does not exist.",
            Assert.Single(Refs(Field("supersedes", "adrs"), "adr-0099")).Message);

    [Fact]
    public void A_ref_at_a_folder_no_schema_covers_still_asks_that_the_target_exists()
        => Assert.Equal("'supersedes' points at 'adr-0099', which does not exist.",
            Assert.Single(Refs(Field("supersedes", "widgets"), "adr-0099")).Message);

    [Fact]
    public void A_field_whose_every_type_this_corpus_declined_admits_nothing()
        => Assert.Equal(
            "'supersedes' points at 'svc-catalogue', which is a Service. The field points at 'widgets', "
            + "which this corpus did not adopt.",
            Assert.Single(Refs(Field("supersedes", "widgets"), "svc-catalogue")).Message);

    [Fact]
    public void A_field_naming_a_declined_type_beside_an_adopted_one_still_names_the_type_it_wanted()
        => Assert.Equal("'supersedes' points at 'svc-catalogue', which is a Service, not an ADR.",
            Assert.Single(Refs(Field("supersedes", "widgets", "adrs"), "svc-catalogue")).Message);

    [Fact]
    public void A_wrong_type_target_on_a_reciprocal_field_is_reported_once()
        => Assert.Equal("ref-resolves",
            Assert.Single(Refs(Reciprocating("supersedes", "adrs"), "svc-catalogue")).Check.Value);

    [Fact]
    public void A_target_of_the_right_type_is_still_held_to_pointing_back()
        => Assert.Equal("reciprocal", Assert.Single(Refs(Reciprocating("supersedes", "adrs"), "adr-0002")).Check.Value);

    [Fact]
    public void A_field_requiring_a_part_admits_an_id_that_names_one()
        => Assert.Empty(Implements("pol-VURM.TIMEBOX"));

    [Fact]
    public void A_field_requiring_a_part_refuses_the_record_named_whole()
    {
        var found = Assert.Single(Implements("pol-VURM"));

        Assert.Equal("ref-resolves", found.Check.Value);
        Assert.Equal("'implements' points at 'pol-VURM' whole, and this field names a clause. Write "
                     + "'pol-VURM.<clause>', one entry per clause. A bare id reads as every clause covered.",
            found.Message);
    }

    [Fact]
    public void A_target_of_the_wrong_type_is_reported_as_that_rather_than_as_a_missing_part()
        => Assert.Equal("'implements' points at 'adr-0002', which is an ADR, not a Policy.",
            Assert.Single(Implements("adr-0002")).Message);

    [Fact]
    public void A_control_citing_a_rule_is_reciprocated_by_its_standard_naming_the_control()
        => Assert.Empty(Verifies("std-CI.PERMS", "ctl-0001"));

    [Fact]
    public void A_control_citing_two_rules_of_one_standard_is_reciprocated_by_one_entry()
        => Assert.Empty(Verifies("[ std-CI.PERMS, std-CI.PIN ]", "ctl-0001"));

    [Fact]
    public void A_control_citing_a_standard_whole_is_told_to_name_the_rule()
    {
        var found = Assert.Single(Verifies("std-CI", "ctl-0001"));

        Assert.Equal("ref-resolves", found.Check.Value);
        Assert.Equal("'verifies' points at 'std-CI' whole, and this field names a rule. Write "
                     + "'std-CI.<rule>', one entry per rule. A bare id reads as every rule covered.",
            found.Message);
    }

    [Fact]
    public void A_control_citing_a_rule_the_standard_does_not_state_is_reported()
        => Assert.Equal("'verifies' points at a rule 'MISSING' that standards/workflows.md does not carry.",
            Assert.Single(Verifies("std-CI.MISSING", "ctl-0001")).Message);

    [Fact]
    public void A_standard_naming_a_control_that_cites_none_of_its_rules_is_not_reciprocated()
    {
        var found = Assert.Single(Verifies("[]", "ctl-0001"));

        Assert.Equal("reciprocal", found.Check.Value);
        Assert.Contains("must list 'verifies: std-CI'", found.Message);
    }

    [Fact]
    public void A_control_citing_a_rule_is_not_reciprocated_by_a_standard_naming_no_control()
    {
        var found = Assert.Single(Verifies("std-CI.PERMS", "[]"));

        Assert.Equal("reciprocal", found.Check.Value);
        Assert.Contains("must list 'verified-by: ctl-0001'", found.Message);
    }

    /// <summary>
    /// A control's <c>verifies:</c> and a standard's <c>verified-by:</c>, declared as the schema declares them, and a
    /// standard stating two rules whose headings open with their ids.
    /// </summary>
    private static List<Finding> Verifies(string verifies, string verifiedBy)
    {
        var forward = new FieldSpec
        {
            Name = "verifies", Type = "list", Of = "id", Refs = ["standards"], Reciprocal = "verified-by",
            PartRequired = true
        };
        var back = new FieldSpec
            { Name = "verified-by", Type = "list", Of = "id", Refs = ["controls"], Reciprocal = "verifies" };

        var controls = new TypeSchema
        {
            Key = "controls", Folder = "controls", Label = "Control", LabelPlural = "Controls",
            FieldOrder = ["verifies"], Fields = new Dictionary<string, FieldSpec> { ["verifies"] = forward }
        };
        var standards = new TypeSchema
        {
            Key = "standards", Folder = "standards", Label = "Standard", LabelPlural = "Standards",
            FieldOrder = ["verified-by"], Fields = new Dictionary<string, FieldSpec> { ["verified-by"] = back },
            Parts = new PartSpec(PartSpec.Headings, "^[A-Z][A-Z0-9]{1,6}$", ["MUST"], [])
                { Section = "Rules", Noun = "rule" }
        };

        var schema = new Schema
        {
            ByFolder = new Dictionary<string, TypeSchema> { ["controls"] = controls, ["standards"] = standards }
        };

        var docs = new[]
        {
            Doc.Parse("standards/workflows.md",
                $"---\nid: std-CI\nverified-by: {verifiedBy}\n---\n\n# std-CI\n\n## Rules\n\n"
                + "### PERMS: A job declares the permission it needs\n\n- A job **MUST** declare it.\n\n"
                + "### PIN: An action is pinned\n\n- An action **MUST** be pinned.\n", schema),
            Doc.Parse("controls/0001-zizmor.md",
                $"---\nid: ctl-0001\nverifies: {verifies}\n---\n\n# ctl-0001\n", schema)
        }.OfType<Doc>().ToList();

        // A document that did not parse would quietly shrink the corpus.
        Assert.Equal(2, docs.Count);

        var found = new List<Finding>();
        Validator.CheckCorpus(schema, docs, found);
        return found;
    }

    private static FieldSpec Field(string name, params string[] refs) =>
        new() { Name = name, Type = "id", Refs = refs };

    private static FieldSpec Reciprocating(string name, params string[] refs) =>
        new() { Name = name, Type = "id", Refs = refs, Reciprocal = "superseded-by" };

    /// <summary>
    /// The labels are the real ones, because a message names a type from its label. An id outside the cast is the
    /// target that does not exist.
    /// </summary>
    private static List<Finding> Refs(FieldSpec field, string target)
    {
        var adrs = new TypeSchema
        {
            Key = "adrs", Folder = "adrs", Label = "ADR", LabelPlural = "ADRs", FieldOrder = [field.Name],
            Fields = new Dictionary<string, FieldSpec> { [field.Name] = field }
        };
        var services = new TypeSchema
            { Key = "services", Folder = "services", Label = "Service", LabelPlural = "Services" };
        var data = new TypeSchema { Key = "data", Folder = "data", Label = "Data", LabelPlural = "Data" };

        var schema = new Schema
        {
            ByFolder = new Dictionary<string, TypeSchema>
                { ["adrs"] = adrs, ["services"] = services, ["data"] = data }
        };

        string[] cast = ["adrs/adr-0002.md", "services/svc-catalogue.md", "data/dat-borrowers.md"];

        var docs = cast
            .Select(rel => Parse(schema, rel, Path.GetFileNameWithoutExtension(rel), null))
            .Prepend(Parse(schema, "adrs/adr-0001.md", "adr-0001", $"{field.Name}: {target}"))
            .OfType<Doc>()
            .ToList();
        // A document that did not parse would quietly shrink the corpus.
        Assert.Equal(cast.Length + 1, docs.Count);

        var found = new List<Finding>();
        Validator.CheckCorpus(schema, docs, found);
        return found;
    }

    /// <summary>
    /// A standard carries <c>implements:</c> and a policy keeps clauses for it to name. The ADR is the target of the
    /// wrong type, and its type keeps no parts.
    /// </summary>
    private static List<Finding> Implements(string target)
    {
        var field = new FieldSpec
            { Name = "implements", Type = "list", Of = "id", Refs = ["policies"], PartRequired = true };

        var standards = new TypeSchema
        {
            Key = "standards", Folder = "standards", Label = "Standard", LabelPlural = "Standards",
            FieldOrder = ["implements"],
            Fields = new Dictionary<string, FieldSpec> { ["implements"] = field }
        };
        var policies = new TypeSchema
        {
            Key = "policies", Folder = "policies", Label = "Policy", LabelPlural = "Policies",
            Parts = new PartSpec(PartSpec.Table, "", ["MUST"], []) { Section = "Clauses", Noun = "clause" }
        };
        var adrs = new TypeSchema { Key = "adrs", Folder = "adrs", Label = "ADR", LabelPlural = "ADRs" };

        var schema = new Schema
        {
            ByFolder = new Dictionary<string, TypeSchema>
                { ["standards"] = standards, ["policies"] = policies, ["adrs"] = adrs }
        };

        var docs = new[]
        {
            Doc.Parse("policies/vurm-remediation.md",
                $"---\nid: pol-VURM\n---\n\n# pol-VURM\n\n## Clauses\n\n| Id | Clause |\n|----|--------|\n"
                + "| `TIMEBOX` | **MUST** be timeboxed |\n", schema),
            Doc.Parse("adrs/adr-0002.md", "---\nid: adr-0002\n---\n\n# adr-0002\n", schema),
            Doc.Parse("standards/testing.md",
                $"---\nid: std-TEST\nimplements:\n  - {target}\n---\n\n# std-TEST\n", schema)
        }.OfType<Doc>().ToList();

        // A document that did not parse would quietly shrink the corpus.
        Assert.Equal(3, docs.Count);

        var found = new List<Finding>();
        Validator.CheckCorpus(schema, docs, found);
        return found;
    }

    private static Doc? Parse(Schema schema, string rel, string id, string? extra) =>
        Doc.Parse(rel, $"---\nid: {id}\n{(extra is null ? "" : extra + "\n")}---\n\n# {id}\n", schema);
}
