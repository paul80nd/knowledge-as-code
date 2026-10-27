using System.Net;
using System.Text;
using kac.core;

namespace kac.tests;

public class RegistryTests
{
    private const string Feed = "https://feed.example/v3/index.json";

    [Fact]
    public void A_registry_user_sends_the_token_as_basic_auth_with_that_user()
    {
        var handler = new Recorder();

        Fetch(handler, Feed, (Registry.UserVariable, "me"), (Registry.TokenVariable, "s3cret"));

        Assert.Equal($"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes("me:s3cret"))}", handler.Authorization);
    }

    [Fact]
    public void A_token_with_no_registry_user_is_sent_as_a_bearer_token()
    {
        var handler = new Recorder();

        Fetch(handler, Feed, (Registry.TokenVariable, "s3cret"));

        Assert.Equal("Bearer s3cret", handler.Authorization);
    }

    [Fact]
    public void A_registry_user_with_no_token_sends_no_authorisation()
    {
        var handler = new Recorder();

        Fetch(handler, Feed, (Registry.UserVariable, "me"));

        Assert.Null(handler.Authorization);
    }

    [Theory]
    [InlineData("me")]
    [InlineData(null)]
    public void Plain_HTTP_is_sent_no_authorisation(string? user)
    {
        var handler = new Recorder();

        Fetch(handler, "http://feed.example/v3/index.json",
            (Registry.UserVariable, user), (Registry.TokenVariable, "s3cret"));

        Assert.Null(handler.Authorization);
    }

    [Fact]
    public void A_refused_read_names_the_token_and_the_registry_user()
    {
        var fetched = Fetch(new Recorder(HttpStatusCode.Unauthorized), Feed);

        Assert.Equal(401, fetched.Status);
        Assert.Contains(Registry.TokenVariable, fetched.Problem, StringComparison.Ordinal);
        Assert.Contains(Registry.UserVariable, fetched.Problem, StringComparison.Ordinal);
    }

    [Fact]
    public void An_empty_listing_from_a_feed_names_the_token_and_the_registry_user()
    {
        var absent = Registry.Absent(Feed);

        Assert.Contains(Registry.TokenVariable, absent, StringComparison.Ordinal);
        Assert.Contains(Registry.UserVariable, absent, StringComparison.Ordinal);
    }

    private static Fetched Fetch(Recorder handler, string url, params (string Name, string? Value)[] set)
        => Registry.Over(new HttpClient(handler), name => set.FirstOrDefault(s => s.Name == name).Value)(url);

    /// <summary>
    /// Answers every request with one status and an empty body, and keeps the authorisation it was sent.
    /// </summary>
    private sealed class Recorder(HttpStatusCode status = HttpStatusCode.OK) : HttpMessageHandler
    {
        public string? Authorization { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken) => Task.FromResult(Send(request, cancellationToken));

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Authorization = request.Headers.Authorization?.ToString();
            return new HttpResponseMessage(status) { Content = new ByteArrayContent([]) };
        }
    }
}
