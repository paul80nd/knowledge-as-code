using kac.core;

namespace kac.tests;

public class AnnotationTests
{
    [Fact]
    public void A_warning_under_GitHub_is_a_warning_command_on_its_file_and_line()
        => Assert.Equal("::warning file=examples/dog-fooding/adrs/0001-first.md,line=4,title=retire-not-delete::"
                        + "retire it instead.",
            GitHub.Line(Warning("adrs/0001-first.md", 4, "retire-not-delete", "retire it instead.")));

    [Fact]
    public void An_error_under_GitHub_is_an_error_command()
        => Assert.StartsWith("::error file=",
            GitHub.Line(Of(Sev.Error, "adrs/0001-first.md", 1, "required-field", "missing 'owner'.")));

    [Fact]
    public void A_finding_with_no_line_under_GitHub_names_the_file_alone()
        => Assert.Equal("::warning file=examples/dog-fooding/.corpus.yaml,title=min-records::too few records.",
            GitHub.Line(Warning(".corpus.yaml", null, "min-records", "too few records.")));

    [Fact]
    public void An_info_finding_is_no_annotation()
        => Assert.Null(GitHub.Line(Of(Sev.Info, ".corpus.yaml", null, "import-capped", "capped at 1.x.")));

    [Fact]
    public void GitHub_escapes_a_percent_and_a_line_break_in_the_message()
        => Assert.EndsWith("::50%25 of it%0D%0Aand: more, still.",
            GitHub.Line(Warning("a.md", 1, "c", "50% of it\r\nand: more, still.")));

    [Fact]
    public void GitHub_escapes_a_colon_and_a_comma_in_a_property()
        => Assert.Equal("::warning file=examples/dog-fooding/a%3Ab%2Cc%25.md,line=1,title=c::m.",
            GitHub.Line(Warning("a:b,c%.md", 1, "c", "m.")));

    [Fact]
    public void A_finding_in_the_schema_is_placed_by_where_the_schema_is()
        => Assert.StartsWith("::warning file=.schema/adrs.yaml,",
            GitHub.Line(Warning(".schema/adrs.yaml", 3, "schema-unread-key", "unread.")));

    [Fact]
    public void A_warning_under_Azure_Pipelines_is_a_logissue_command_on_its_file_and_line()
        => Assert.Equal("##vso[task.logissue type=warning;sourcepath=examples/dog-fooding/adrs/0001-first.md;"
                        + "linenumber=4;code=retire-not-delete]retire it instead.",
            Azure.Line(Warning("adrs/0001-first.md", 4, "retire-not-delete", "retire it instead.")));

    [Fact]
    public void An_error_with_no_line_under_Azure_Pipelines_names_the_file_alone()
        => Assert.Equal("##vso[task.logissue type=error;sourcepath=examples/dog-fooding/.corpus.yaml;"
                        + "code=min-records]too few records.",
            Azure.Line(Of(Sev.Error, ".corpus.yaml", null, "min-records", "too few records.")));

    [Fact]
    public void Azure_Pipelines_escapes_a_percent_and_a_line_break_in_the_message()
        => Assert.EndsWith("]50%AZP25 of it%0D%0Aand; [x] more.",
            Azure.Line(Warning("a.md", 1, "c", "50% of it\r\nand; [x] more.")));

    [Fact]
    public void Azure_Pipelines_escapes_a_semicolon_and_a_closing_bracket_in_a_property()
        => Assert.StartsWith("##vso[task.logissue type=warning;sourcepath=examples/dog-fooding/a%3Bb%5Dc%AZP25.md;",
            Azure.Line(Warning("a;b]c%.md", 1, "c", "m.")));

    [Theory]
    [InlineData("GITHUB_ACTIONS", "true", CiPlatform.GitHub)]
    [InlineData("TF_BUILD", "True", CiPlatform.AzurePipelines)]
    [InlineData("TF_BUILD", "true", CiPlatform.AzurePipelines)]
    public void The_platform_is_the_one_its_variable_declares(string variable, string value, CiPlatform platform)
        => Assert.Equal(platform, Annotator.Detect(name => name == variable ? value : null));

    [Theory]
    [InlineData("GITHUB_ACTIONS", "false")]
    [InlineData("GITHUB_ACTIONS", "")]
    [InlineData("TF_BUILD", "False")]
    [InlineData("CI", "true")]
    public void A_run_no_platform_declares_is_local(string variable, string value)
        => Assert.Null(Annotator.Detect(name => name == variable ? value : null));

    [Fact]
    public void The_comment_body_is_a_table_of_the_errors_and_warnings_under_their_counts()
        => Assert.Equal(
            "### ❌ `kac validate` for example-dogfooding: 1 error, 2 warnings\n"
            + "\n"
            + "| Severity | File | Check | Message |\n"
            + "|----------|------|-------|---------|\n"
            + "| ❌ error | `examples/dog-fooding/adrs/0001-first.md:1` | `required-field` | missing 'owner'. |\n"
            + "| ⚠️ warning | `examples/dog-fooding/adrs/0001-first.md:4` | `retire-not-delete` | retire it. |\n"
            + "| ⚠️ warning | `examples/dog-fooding/services/kac.md` | `critical-service-is-alerted` | "
            + "emits nothing. |\n",
            GitHub.Table("example-dogfooding",
            [
                Warning("services/kac.md", null, "critical-service-is-alerted", "emits nothing."),
                Of(Sev.Info, ".corpus.yaml", null, "import-capped", "capped at 1.x."),
                Warning("adrs/0001-first.md", 4, "retire-not-delete", "retire it."),
                Of(Sev.Error, "adrs/0001-first.md", 1, "required-field", "missing 'owner'.")
            ]));

    [Fact]
    public void A_run_with_warnings_alone_heads_its_comment_body_with_the_warning_icon()
        => Assert.StartsWith("### ⚠️ `kac validate` for example-dogfooding: 0 errors, 1 warning\n",
            GitHub.Table("example-dogfooding", [Warning("a.md", 1, "c", "m.")]));

    [Fact]
    public void A_run_with_no_error_or_warning_has_a_one_line_comment_body()
        => Assert.Equal(
            "### ✅ `kac validate` for example-dogfooding: the corpus validated with no errors or warnings\n",
            GitHub.Table("example-dogfooding",
                [Of(Sev.Info, ".corpus.yaml", null, "import-capped", "capped at 1.x.")]));

    [Theory]
    [InlineData("example-dogfooding", "examples/dog-fooding/", "example-dogfooding")]
    [InlineData(null, "examples/dog-fooding/", "examples/dog-fooding")]
    [InlineData(null, "", "this corpus")]
    [InlineData("", "examples/dog-fooding/", "examples/dog-fooding")]
    public void A_corpus_is_named_by_its_descriptor_then_its_path_then_as_this_corpus(
        string? declared, string prefix, string name)
        => Assert.Equal(name, (GitHub with { CorpusPrefix = prefix }).Named(declared));

    [Fact]
    public void The_comment_body_escapes_a_pipe_and_a_line_break_in_a_cell()
        => Assert.EndsWith("| `examples/dog-fooding/a\\|b.md:1` | `c` | one \\| two<br>three |\n",
            GitHub.Table("example-dogfooding", [Warning("a|b.md", 1, "c", "one | two\r\nthree")]));

    private static readonly Annotator Azure = new(CiPlatform.AzurePipelines, "examples/dog-fooding/", "");

    private static readonly Annotator GitHub = new(CiPlatform.GitHub, "examples/dog-fooding/", "");

    private static Finding Warning(string file, int? line, string check, string message) =>
        Of(Sev.Warning, file, line, check, message);

    private static Finding Of(Sev severity, string file, int? line, string check, string message) =>
        new(file, line, severity, new CheckId(check), message);
}
