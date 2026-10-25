using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace kac.core;

/// <summary>One request to a platform's API. <c>Body</c> is JSON, or null for a <c>GET</c>.</summary>
public sealed record HttpCall(string Method, string Url, string? Body);

/// <summary>
/// What the API answered. <c>Status</c> is 0 and <c>Problem</c> says why where no answer came back at all.
/// </summary>
public sealed record HttpAnswer(int Status, string? Body, string? Problem)
{
    public bool Ok => Status is >= 200 and < 300;
}

/// <summary>What posting the comment came to. <c>Ok</c> is false for a note the reader should act on.</summary>
public sealed record Commented(bool Ok, string Message);

/// <summary>
/// The pull request a CI run belongs to. <c>Api</c> is the base every request is built on: the repository on GitHub,
/// and the pull request itself on Azure Pipelines. <c>TokenVariable</c> names where the pipeline passes the token.
/// </summary>
public sealed record PullRequest(CiPlatform Platform, string Api, string Number, string TokenVariable)
{
    /// <summary>
    /// Null where the run is not for a pull request. GitHub names it in the event payload at
    /// <c>GITHUB_EVENT_PATH</c>, which <paramref name="read"/> returns or answers null for. Azure Pipelines names it in
    /// <c>SYSTEM_PULLREQUEST_PULLREQUESTID</c>.
    /// </summary>
    public static PullRequest? From(CiPlatform platform, Func<string, string?> variable, Func<string, string?> read)
    {
        switch (platform)
        {
            case CiPlatform.GitHub:
                if (variable("GITHUB_REPOSITORY") is not { Length: > 0 } repository) return null;
                if (variable("GITHUB_EVENT_PATH") is not { Length: > 0 } eventPath) return null;
                if (NumberIn(read(eventPath)) is not { } number) return null;

                var api = variable("GITHUB_API_URL") is { Length: > 0 } url ? url : "https://api.github.com";
                return new PullRequest(platform, $"{api.TrimEnd('/')}/repos/{repository}", number, "GITHUB_TOKEN");

            case CiPlatform.AzurePipelines:
                if (variable("SYSTEM_PULLREQUEST_PULLREQUESTID") is not { Length: > 0 } id) return null;
                if (variable("SYSTEM_COLLECTIONURI") is not { Length: > 0 } collection) return null;
                if (variable("SYSTEM_TEAMPROJECT") is not { Length: > 0 } project) return null;
                if (variable("BUILD_REPOSITORY_ID") is not { Length: > 0 } repo) return null;

                return new PullRequest(platform,
                    $"{collection.TrimEnd('/')}/{Uri.EscapeDataString(project)}/_apis/git/repositories/"
                    + $"{Uri.EscapeDataString(repo)}/pullRequests/{Uri.EscapeDataString(id)}", id,
                    "SYSTEM_ACCESSTOKEN");

            default:
                return null;
        }
    }

    /// <summary><c>pull_request.number</c> from a GitHub event payload, or null where it has none.</summary>
    private static string? NumberIn(string? payload)
    {
        if (payload is null) return null;
        try
        {
            using var doc = JsonDocument.Parse(payload);
            return doc.RootElement.ValueKind is JsonValueKind.Object
                   && doc.RootElement.TryGetProperty("pull_request", out var pr)
                   && pr.ValueKind is JsonValueKind.Object
                   && pr.TryGetProperty("number", out var n)
                   && n.ValueKind is JsonValueKind.Number && n.TryGetInt32(out var number)
                ? number.ToString(System.Globalization.CultureInfo.InvariantCulture)
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

/// <summary>
/// Keeps one comment per corpus on a pull request, found again by a hidden marker naming the corpus. It never deletes
/// one. <paramref name="send"/> is null where the pipeline passed no token.
/// </summary>
public sealed class PullRequestComment(PullRequest pullRequest, Func<HttpCall, HttpAnswer>? send)
{
    /// <summary>
    /// Updates the corpus's earlier comment to <paramref name="summary"/>, or posts one where
    /// <paramref name="hasFindings"/>. Null where a clean run found no earlier comment, so nothing was written.
    /// </summary>
    public Commented? Post(string corpus, string summary, bool hasFindings)
    {
        var at = $"pull request #{pullRequest.Number}";
        if (send is null)
            return new Commented(false, $"validate: {pullRequest.TokenVariable} is not set, so the findings were not "
                                        + $"posted to {at}.");

        var marker = $"<!-- kac validate: {corpus} -->";
        var body = marker + "\n" + summary;

        Earlier? earlier;
        try
        {
            var (found, refused) = Find(send, marker);
            if (refused is not null) return Failed(at, refused);
            earlier = found;
        }
        catch (Exception e) when (e is JsonException or InvalidOperationException or KeyNotFoundException)
        {
            return new Commented(false, $"validate: could not post the findings to {at}: the comments it listed were "
                                        + "not the JSON expected.");
        }

        if (earlier is null && !hasFindings) return null;

        var call = (pullRequest.Platform, earlier) switch
        {
            (CiPlatform.GitHub, { } id) => new HttpCall("PATCH", $"{pullRequest.Api}/issues/comments/{id.Comment}",
                Json(w => w.WriteString("body", body))),
            (CiPlatform.GitHub, null) => new HttpCall("POST",
                $"{pullRequest.Api}/issues/{pullRequest.Number}/comments", Json(w => w.WriteString("body", body))),
            (_, { } id) => new HttpCall("PATCH",
                $"{pullRequest.Api}/threads/{id.Thread}/comments/{id.Comment}?api-version=7.1",
                Json(w => w.WriteString("content", body))),
            _ => new HttpCall("POST", $"{pullRequest.Api}/threads?api-version=7.1", Json(NewThread(body)))
        };

        var answer = send(call);
        if (!answer.Ok) return Failed(at, answer);

        // An Azure Pipelines thread is closed while its corpus is clean and active while it has findings.
        if (pullRequest.Platform is CiPlatform.AzurePipelines && earlier is not null)
        {
            var status = send(new HttpCall("PATCH", $"{pullRequest.Api}/threads/{earlier.Thread}?api-version=7.1",
                Json(w => w.WriteNumber("status", hasFindings ? Active : Closed))));
            if (!status.Ok) return Failed(at, status);
        }

        return new Commented(true, earlier is null
            ? $"validate: posted the findings to {at}."
            : $"validate: updated the findings on {at}.");
    }

    /// <summary>
    /// The real API, with the token on every request. A pipeline variable can end in a line break, which a header
    /// refuses, so the token is trimmed. Nothing it returns repeats the token.
    /// </summary>
    public static Func<HttpCall, HttpAnswer> Over(HttpClient client, CiPlatform platform, string token) => call =>
    {
        try
        {
            using var request = new HttpRequestMessage(new HttpMethod(call.Method), call.Url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim());
            request.Headers.UserAgent.ParseAdd("kac");
            if (platform is CiPlatform.GitHub)
            {
                request.Headers.Accept.ParseAdd("application/vnd.github+json");
                request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
            }

            if (call.Body is not null)
                request.Content = new StringContent(call.Body, Encoding.UTF8, "application/json");

            using var response = client.Send(request);
            using var reader = new StreamReader(response.Content.ReadAsStream());
            return new HttpAnswer((int)response.StatusCode, reader.ReadToEnd(),
                response.IsSuccessStatusCode ? null : response.ReasonPhrase);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or IOException
                                      or InvalidOperationException or FormatException)
        {
            return new HttpAnswer(0, null, e.Message.TrimEnd('.'));
        }
    };

    /// <summary>How many pages of 100 comments GitHub is asked for before the search gives up.</summary>
    private const int MaxPages = 30;

    /// <summary>The Azure Pipelines thread statuses the comment moves between.</summary>
    private const int Active = 1, Closed = 4;

    private sealed record Earlier(string Thread, string Comment);

    /// <summary>
    /// The comment opening on <paramref name="marker"/>, or null where none does, and the answer that refused the
    /// listing. GitHub pages its comments 100 at a time, and on Azure Pipelines the marker opens a thread's first
    /// comment.
    /// </summary>
    /// <exception cref="JsonException">The listing was not JSON.</exception>
    /// <exception cref="InvalidOperationException">The listing was JSON of another shape.</exception>
    /// <exception cref="KeyNotFoundException">The listing lacked a key it always has.</exception>
    private (Earlier? Found, HttpAnswer? Refused) Find(Func<HttpCall, HttpAnswer> api, string marker)
    {
        if (pullRequest.Platform is CiPlatform.GitHub)
        {
            for (var page = 1; page <= MaxPages; page++)
            {
                var answer = api(new HttpCall("GET",
                    $"{pullRequest.Api}/issues/{pullRequest.Number}/comments?per_page=100&page={page}", null));
                if (!answer.Ok) return (null, answer);

                using var doc = JsonDocument.Parse(answer.Body ?? "[]");
                var comments = doc.RootElement.EnumerateArray().ToList();
                foreach (var c in comments)
                    if (c.TryGetProperty("body", out var text) && text.GetString() is { } body
                                                              && body.StartsWith(marker, StringComparison.Ordinal))
                        return (new Earlier("", c.GetProperty("id").GetRawText()), null);

                if (comments.Count < 100) return (null, null);
            }

            return (null, new HttpAnswer(0, null, $"it has more than {MaxPages * 100} comments to look through"));
        }

        var threads = api(new HttpCall("GET", $"{pullRequest.Api}/threads?api-version=7.1", null));
        if (!threads.Ok) return (null, threads);

        using var listing = JsonDocument.Parse(threads.Body ?? "{}");
        foreach (var thread in listing.RootElement.GetProperty("value").EnumerateArray())
        {
            if (Deleted(thread)) continue;
            if (!thread.TryGetProperty("comments", out var comments) || comments.GetArrayLength() == 0) continue;

            var first = comments[0];
            if (Deleted(first)) continue;
            if (first.TryGetProperty("content", out var content) && content.GetString() is { } text
                                                                && text.StartsWith(marker, StringComparison.Ordinal))
                return (new Earlier(thread.GetProperty("id").GetRawText(), first.GetProperty("id").GetRawText()),
                    null);
        }

        return (null, null);

        static bool Deleted(JsonElement e) =>
            e.TryGetProperty("isDeleted", out var deleted) && deleted.ValueKind is JsonValueKind.True;
    }

    /// <summary>An active thread with one text comment, which is what the Azure Pipelines UI posts.</summary>
    private static Action<Utf8JsonWriter> NewThread(string body) => w =>
    {
        w.WriteStartArray("comments");
        w.WriteStartObject();
        w.WriteNumber("parentCommentId", 0);
        w.WriteString("content", body);
        w.WriteNumber("commentType", 1);
        w.WriteEndObject();
        w.WriteEndArray();
        w.WriteNumber("status", Active);
    };

    private static string Json(Action<Utf8JsonWriter> write)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            write(writer);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(buffer.ToArray());
    }

    /// <summary>
    /// A token the API refused is usually a fork's read-only one, so a 401, a 403 or a 404 names the fork case.
    /// </summary>
    private static Commented Failed(string at, HttpAnswer answer)
    {
        var problem = answer.Status == 0
            ? answer.Problem ?? "no answer"
            : $"{answer.Status}" + (answer.Problem is { } reason ? $" {reason}" : "");
        var fork = answer.Status is 401 or 403 or 404
            ? " A pull request from a fork gets a token that cannot write to it."
            : "";

        return new Commented(false, $"validate: could not post the findings to {at}: {problem}.{fork}");
    }
}
