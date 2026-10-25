using System.Text.Json;
using kac.core;

namespace kac.tests;

public class PullRequestCommentTests
{
    private const string Marker = "<!-- kac validate: example-dogfooding -->";

    private const string Findings = "### `kac validate`: 1 error, 0 warnings\n\n| table |\n";

    private const string Clean = "### `kac validate`: the corpus validated with no errors or warnings\n";

    [Fact]
    public void A_GitHub_run_for_a_pull_request_names_its_repository_and_number()
    {
        var pr = PullRequest.From(CiPlatform.GitHub, Env(("GITHUB_REPOSITORY", "paul80nd/knowledge-as-code"),
                ("GITHUB_API_URL", "https://api.github.com"), ("GITHUB_EVENT_PATH", "/event.json")),
            path => path == "/event.json" ? """{ "pull_request": { "number": 1183 } }""" : null);

        Assert.Equal(new PullRequest(CiPlatform.GitHub, "https://api.github.com/repos/paul80nd/knowledge-as-code",
            "1183", "GITHUB_TOKEN"), pr);
    }

    [Fact]
    public void A_GitHub_run_for_a_push_is_no_pull_request()
        => Assert.Null(PullRequest.From(CiPlatform.GitHub,
            Env(("GITHUB_REPOSITORY", "o/r"), ("GITHUB_EVENT_PATH", "/event.json")),
            _ => """{ "ref": "refs/heads/main" }"""));

    [Theory]
    [InlineData("""{ "pull_request": null }""")]
    [InlineData("""[ 1, 2 ]""")]
    [InlineData("""{ "pull_request": { "number": "seven" } }""")]
    [InlineData("not json")]
    public void A_GitHub_event_payload_of_another_shape_is_no_pull_request(string payload)
        => Assert.Null(PullRequest.From(CiPlatform.GitHub,
            Env(("GITHUB_REPOSITORY", "o/r"), ("GITHUB_EVENT_PATH", "/event.json")), _ => payload));

    [Fact]
    public void A_GitHub_run_with_no_API_URL_uses_the_public_one()
        => Assert.StartsWith("https://api.github.com/repos/o/r",
            PullRequest.From(CiPlatform.GitHub, Env(("GITHUB_REPOSITORY", "o/r"), ("GITHUB_EVENT_PATH", "/e")),
                _ => """{ "pull_request": { "number": 7 } }""")?.Api);

    [Fact]
    public void An_Azure_Pipelines_run_for_a_pull_request_names_its_repository_and_number()
    {
        var pr = PullRequest.From(CiPlatform.AzurePipelines,
            Env(("SYSTEM_COLLECTIONURI", "https://dev.azure.com/acme/"), ("SYSTEM_TEAMPROJECT", "Know How"),
                ("BUILD_REPOSITORY_ID", "abc-123"), ("SYSTEM_PULLREQUEST_PULLREQUESTID", "42")), _ => null);

        Assert.Equal(new PullRequest(CiPlatform.AzurePipelines,
            "https://dev.azure.com/acme/Know%20How/_apis/git/repositories/abc-123/pullRequests/42", "42",
            "SYSTEM_ACCESSTOKEN"), pr);
    }

    [Fact]
    public void An_Azure_Pipelines_run_that_is_not_a_pull_request_build_is_no_pull_request()
        => Assert.Null(PullRequest.From(CiPlatform.AzurePipelines,
            Env(("SYSTEM_COLLECTIONURI", "https://dev.azure.com/acme/"), ("SYSTEM_TEAMPROJECT", "p"),
                ("BUILD_REPOSITORY_ID", "r")), _ => null));

    [Fact]
    public void GitHub_with_findings_and_no_earlier_comment_posts_one()
    {
        var fake = new FakeApi(("GET", "/issues/1183/comments?per_page=100&page=1", 200, "[]"),
            ("POST", "/issues/1183/comments", 201, "{}"));

        var said = GitHubComment(fake).Post("example-dogfooding", Findings, hasFindings: true);

        Assert.Equal(["GET", "POST"], fake.Calls.Select(c => c.Method));
        Assert.Equal(Marker + "\n" + Findings, Field(fake.Calls[1].Body, "body"));
        Assert.Equal(new Commented(true, "validate: posted the findings to pull request #1183."), said);
    }

    [Fact]
    public void GitHub_with_an_earlier_comment_updates_it()
    {
        var fake = new FakeApi(
            ("GET", "/issues/1183/comments?per_page=100&page=1", 200,
                $$"""[ { "id": 5, "body": "unrelated" }, { "id": 9, "body": "{{Marker}}\nold" } ]"""),
            ("PATCH", "/issues/comments/9", 200, "{}"));

        var said = GitHubComment(fake).Post("example-dogfooding", Findings, hasFindings: true);

        Assert.Equal(["GET", "PATCH"], fake.Calls.Select(c => c.Method));
        Assert.Equal("https://api.github.com/repos/o/r/issues/comments/9", fake.Calls[1].Url);
        Assert.Equal(new Commented(true, "validate: updated the findings on pull request #1183."), said);
    }

    [Fact]
    public void GitHub_finds_an_earlier_comment_on_a_later_page()
    {
        var page = "[" + string.Join(",", Enumerable.Range(1, 100).Select(i => $$"""{ "id": {{i}}, "body": "x" }"""))
                   + "]";
        var fake = new FakeApi(("GET", "page=1", 200, page),
            ("GET", "page=2", 200, $$"""[ { "id": 300, "body": "{{Marker}}" } ]"""),
            ("PATCH", "/issues/comments/300", 200, "{}"));

        GitHubComment(fake).Post("example-dogfooding", Findings, hasFindings: true);

        Assert.Equal(["GET", "GET", "PATCH"], fake.Calls.Select(c => c.Method));
    }

    [Fact]
    public void A_comment_for_another_corpus_is_not_updated()
    {
        var fake = new FakeApi(
            ("GET", "page=1", 200, """[ { "id": 9, "body": "<!-- kac validate: example-library -->" } ]"""),
            ("POST", "/issues/1183/comments", 201, "{}"));

        GitHubComment(fake).Post("example-dogfooding", Findings, hasFindings: true);

        Assert.Equal("POST", fake.Calls[^1].Method);
    }

    [Fact]
    public void A_comment_that_only_quotes_the_marker_is_not_updated()
    {
        var fake = new FakeApi(
            ("GET", "page=1", 200, $$"""[ { "id": 9, "body": "Why does it say {{Marker}}?" } ]"""),
            ("POST", "/issues/1183/comments", 201, "{}"));

        GitHubComment(fake).Post("example-dogfooding", Findings, hasFindings: true);

        Assert.Equal("POST", fake.Calls[^1].Method);
    }

    [Fact]
    public void GitHub_stops_listing_after_a_bounded_number_of_pages()
    {
        var page = "[" + string.Join(",", Enumerable.Range(1, 100).Select(i => $$"""{ "id": {{i}}, "body": "x" }"""))
                   + "]";

        var calls = 0;

        // A server that never runs out of comments, until a thousand calls have been made.
        var said = new PullRequestComment(GitHubPr, _ => ++calls < 1000
                ? new HttpAnswer(200, page, null)
                : new HttpAnswer(200, "[]", null))
            .Post("example-dogfooding", Findings, hasFindings: true);

        Assert.False(said?.Ok);
        Assert.True(calls < 1000);
    }

    [Fact]
    public void A_clean_run_with_no_earlier_comment_writes_nothing()
    {
        var fake = new FakeApi(("GET", "page=1", 200, "[]"));

        var said = GitHubComment(fake).Post("example-dogfooding", Clean, hasFindings: false);

        Assert.Equal(["GET"], fake.Calls.Select(c => c.Method));
        Assert.Null(said);
    }

    [Fact]
    public void A_clean_run_with_an_earlier_comment_updates_it_to_say_there_are_none()
    {
        var fake = new FakeApi(("GET", "page=1", 200, $$"""[ { "id": 9, "body": "{{Marker}}\nold" } ]"""),
            ("PATCH", "/issues/comments/9", 200, "{}"));

        GitHubComment(fake).Post("example-dogfooding", Clean, hasFindings: false);

        Assert.Equal(Marker + "\n" + Clean, Field(fake.Calls[1].Body, "body"));
    }

    [Fact]
    public void A_post_that_fails_says_so()
    {
        var fake = new FakeApi(("GET", "page=1", 200, "[]"),
            ("POST", "/issues/1183/comments", 403, null));

        var said = GitHubComment(fake).Post("example-dogfooding", Findings, hasFindings: true);

        Assert.False(said?.Ok);
        Assert.StartsWith("validate: could not post the findings to pull request #1183: 403.", said?.Message);
    }

    [Fact]
    public void A_post_refused_for_its_token_names_the_fork_case()
    {
        var fake = new FakeApi(("GET", "page=1", 200, "[]"), ("POST", "/issues/1183/comments", 403, null));

        Assert.EndsWith("A pull request from a fork gets a token that cannot write to it.",
            GitHubComment(fake).Post("example-dogfooding", Findings, hasFindings: true)?.Message);
    }

    [Fact]
    public void A_listing_with_no_answer_does_not_name_the_fork_case()
        => Assert.Equal(
            "validate: could not post the findings to pull request #1183: connection refused.",
            GitHubComment(new FakeApi(("GET", "page=1", 0, null)))
                .Post("example-dogfooding", Findings, hasFindings: true)?.Message);

    [Theory]
    [InlineData("<html>a proxy's page</html>")]
    [InlineData("""{ "message": "not a list" }""")]
    public void A_listing_that_is_not_the_JSON_expected_writes_nothing(string body)
    {
        var fake = new FakeApi(("GET", "page=1", 200, body));

        var said = GitHubComment(fake).Post("example-dogfooding", Findings, hasFindings: true);

        Assert.Single(fake.Calls);
        Assert.Equal("validate: could not post the findings to pull request #1183: the comments it listed were not "
                     + "the JSON expected.", said?.Message);
    }

    [Fact]
    public void A_listing_that_fails_writes_nothing()
    {
        var fake = new FakeApi(("GET", "page=1", 0, null));

        var said = GitHubComment(fake).Post("example-dogfooding", Findings, hasFindings: true);

        Assert.Single(fake.Calls);
        Assert.False(said?.Ok);
    }

    [Fact]
    public void No_token_is_a_note_and_no_call()
    {
        var said = new PullRequestComment(GitHubPr, send: null).Post("example-dogfooding", Findings, hasFindings: true);

        Assert.Equal(new Commented(false,
            "validate: GITHUB_TOKEN is not set, so the findings were not posted to pull request #1183."), said);
    }

    [Fact]
    public void No_token_on_a_clean_run_says_nothing()
    {
        var said = new PullRequestComment(GitHubPr, send: null).Post("example-dogfooding", Findings, hasFindings: false);

        Assert.Null(said);
    }

    [Fact]
    public void Azure_Pipelines_with_findings_and_no_earlier_thread_creates_one()
    {
        var fake = new FakeApi(("GET", "/threads?api-version=7.1", 200, """{ "value": [] }"""),
            ("POST", "/threads?api-version=7.1", 200, "{}"));

        var said = new PullRequestComment(AzurePr, fake.Send).Post("example-dogfooding", Findings, hasFindings: true);

        using var body = JsonDocument.Parse(fake.Calls[1].Body ?? "{}");
        var comment = body.RootElement.GetProperty("comments")[0];
        Assert.Equal(Marker + "\n" + Findings, comment.GetProperty("content").GetString());
        Assert.Equal(0, comment.GetProperty("parentCommentId").GetInt32());
        Assert.Equal(1, comment.GetProperty("commentType").GetInt32());
        Assert.Equal(1, body.RootElement.GetProperty("status").GetInt32());
        Assert.True(said?.Ok);
    }

    [Fact]
    public void Azure_Pipelines_with_an_earlier_thread_updates_its_first_comment()
    {
        var fake = new FakeApi(
            ("GET", "/threads?api-version=7.1", 200,
                "{ \"value\": [ { \"id\": 3, \"comments\": [ { \"id\": 1, \"content\": \"hello\" } ] }, "
                + $$"""{ "id": 8, "comments": [ { "id": 1, "content": "{{Marker}}\nold" } ] } ] }"""),
            ("PATCH", "/threads/8/comments/1?api-version=7.1", 200, "{}"));

        new PullRequestComment(AzurePr, fake.Send).Post("example-dogfooding", Findings, hasFindings: true);

        Assert.Equal(Marker + "\n" + Findings, Field(fake.Calls[1].Body, "content"));
    }

    [Fact]
    public void Azure_Pipelines_closes_the_thread_once_its_findings_have_gone()
    {
        var fake = AzureWithEarlierThread();

        new PullRequestComment(AzurePr, fake.Send).Post("example-dogfooding", Clean, hasFindings: false);

        Assert.Equal(["GET", "PATCH", "PATCH"], fake.Calls.Select(c => c.Method));
        Assert.Equal(Marker + "\n" + Clean, Field(fake.Calls[1].Body, "content"));
        Assert.EndsWith("/pullRequests/42/threads/8?api-version=7.1", fake.Calls[2].Url);
        Assert.Equal(4, Number(fake.Calls[2].Body, "status"));
    }

    [Fact]
    public void Azure_Pipelines_makes_the_thread_active_again_when_findings_return()
    {
        var fake = AzureWithEarlierThread();

        new PullRequestComment(AzurePr, fake.Send).Post("example-dogfooding", Findings, hasFindings: true);

        Assert.EndsWith("/pullRequests/42/threads/8?api-version=7.1", fake.Calls[2].Url);
        Assert.Equal(1, Number(fake.Calls[2].Body, "status"));
    }

    [Fact]
    public void Azure_Pipelines_says_so_when_the_thread_status_cannot_be_set()
    {
        var fake = new FakeApi(
            ("GET", "/threads?api-version=7.1", 200,
                $$"""{ "value": [ { "id": 8, "comments": [ { "id": 1, "content": "{{Marker}}" } ] } ] }"""),
            ("PATCH", "/threads/8/comments/1?api-version=7.1", 200, "{}"),
            ("PATCH", "/threads/8?api-version=7.1", 500, null));

        var said = new PullRequestComment(AzurePr, fake.Send).Post("example-dogfooding", Clean, hasFindings: false);

        Assert.False(said?.Ok);
    }

    [Fact]
    public void An_Azure_Pipelines_listing_of_another_shape_writes_nothing()
    {
        var fake = new FakeApi(("GET", "/threads?api-version=7.1", 200, """{ "message": "not a listing" }"""));

        var said = new PullRequestComment(AzurePr, fake.Send).Post("example-dogfooding", Findings, hasFindings: true);

        Assert.Single(fake.Calls);
        Assert.False(said?.Ok);
    }

    [Fact]
    public void Azure_Pipelines_passes_over_a_thread_whose_first_comment_was_deleted()
    {
        var fake = new FakeApi(
            ("GET", "/threads?api-version=7.1", 200,
                "{ \"value\": [ { \"id\": 8, "
                + $$"""
                    "comments": [ { "id": 1, "isDeleted": true, "content": "{{Marker}}" } ] } ] }
                    """),
            ("POST", "/threads?api-version=7.1", 200, "{}"));

        new PullRequestComment(AzurePr, fake.Send).Post("example-dogfooding", Findings, hasFindings: true);

        Assert.Equal("POST", fake.Calls[^1].Method);
    }

    [Fact]
    public void A_token_ending_in_a_line_break_is_sent_without_it()
    {
        var handler = new Recorder();
        var send = PullRequestComment.Over(new HttpClient(handler), CiPlatform.GitHub, "a-token\n");

        var answer = send(new HttpCall("GET", "https://api.github.com/repos/o/r/issues/1/comments", null));

        Assert.Equal(200, answer.Status);
        Assert.Equal("Bearer a-token", handler.Authorization);
    }

    [Fact]
    public void Azure_Pipelines_passes_over_a_deleted_thread()
    {
        var fake = new FakeApi(
            ("GET", "/threads?api-version=7.1", 200,
                "{ \"value\": [ { \"id\": 8, \"isDeleted\": true, "
                + $$"""
                    "comments": [ { "id": 1, "content": "{{Marker}}" } ] } ] }
                    """),
            ("POST", "/threads?api-version=7.1", 200, "{}"));

        new PullRequestComment(AzurePr, fake.Send).Post("example-dogfooding", Findings, hasFindings: true);

        Assert.Equal("POST", fake.Calls[^1].Method);
    }

    private static readonly PullRequest GitHubPr =
        new(CiPlatform.GitHub, "https://api.github.com/repos/o/r", "1183", "GITHUB_TOKEN");

    private static readonly PullRequest AzurePr = new(CiPlatform.AzurePipelines,
        "https://dev.azure.com/acme/p/_apis/git/repositories/r/pullRequests/42", "42", "SYSTEM_ACCESSTOKEN");

    private static PullRequestComment GitHubComment(FakeApi fake) => new(GitHubPr, fake.Send);

    private static Func<string, string?> Env(params (string Name, string Value)[] set) =>
        name => set.FirstOrDefault(s => s.Name == name).Value;

    private static FakeApi AzureWithEarlierThread() => new(
        ("GET", "/threads?api-version=7.1", 200,
            $$"""{ "value": [ { "id": 8, "comments": [ { "id": 1, "content": "{{Marker}}\nold" } ] } ] }"""),
        ("PATCH", "/threads/8/comments/1?api-version=7.1", 200, "{}"),
        ("PATCH", "/threads/8?api-version=7.1", 200, "{}"));

    private static int Number(string? json, string name)
    {
        using var doc = JsonDocument.Parse(json ?? "{}");
        return doc.RootElement.GetProperty(name).GetInt32();
    }

    private static string? Field(string? json, string name)
    {
        using var doc = JsonDocument.Parse(json ?? "{}");
        return doc.RootElement.GetProperty(name).GetString();
    }

    /// <summary>Answers every request with an empty list, and keeps the authorisation it was sent.</summary>
    private sealed class Recorder : HttpMessageHandler
    {
        public string? Authorization { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken) => Task.FromResult(Send(request, cancellationToken));

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Authorization = request.Headers.Authorization?.ToString();
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent("[]") };
        }
    }

    /// <summary>
    /// Answers each call with the first unused reply whose method matches and whose path the URL contains, and records
    /// every call it was sent.
    /// </summary>
    private sealed class FakeApi(params (string Method, string Path, int Status, string? Body)[] replies)
    {
        private readonly List<(string Method, string Path, int Status, string? Body)> unused = [.. replies];

        public List<HttpCall> Calls { get; } = [];

        public HttpAnswer Send(HttpCall call)
        {
            Calls.Add(call);
            var at = unused.FindIndex(r =>
                r.Method == call.Method && call.Url.Contains(r.Path, StringComparison.Ordinal));
            if (at < 0) return new HttpAnswer(0, null, $"no reply scripted for {call.Method} {call.Url}");

            var reply = unused[at];
            unused.RemoveAt(at);
            return new HttpAnswer(reply.Status, reply.Body, reply.Status == 0 ? "connection refused" : null);
        }
    }
}
