using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;

namespace kac.core;

// What a request came back with: the bytes, or why there are none. Exactly one of the two is set.
//
// `Status` is the code the server answered with, and zero where nothing answered at all. It is carried
// because one code means something different depending on what was asked: a 404 from a version listing
// is a package nobody has published yet, and a 404 from anywhere else is an address that is wrong. Only
// the caller knows which question it asked.
public sealed record Fetched(byte[]? Body, int Status, string? Problem);

// A registry's side of the bargain `kac pack` opened: the package is a `.nupkg`, so what serves it is a
// NuGet feed, and this is the part of that protocol a restore uses.
//
// Three requests, and no NuGet client. A feed publishes a service index naming its resources; one of
// those is the flat container, which lists a package's versions and serves the file itself at addresses
// built from the id and the version. All three are plain GETs returning JSON or bytes, which is what
// made borrowing the envelope worth it: moving to another registry is a change to `source:` in one
// descriptor.
//
// Every method takes the service index URL a corpus declared and never a URL derived once and kept, so a
// caller cannot address the wrong feed by holding a stale base. The base each source resolved to is
// remembered here instead, so a restore of four dependencies from one feed asks for the index once.
public sealed class Registry(Func<string, Fetched> get)
{
    // The environment variable naming the bearer token. A registry serving a private feed refuses an
    // anonymous read, GitHub Packages among them, and a token belongs in the environment rather than in
    // an option: a flag lands in shell history and in the transcript of every CI run that echoes its
    // command line.
    public const string TokenVariable = "KAC_REGISTRY_TOKEN";

    // What the service index calls the resource this reads. The version suffix is the resource's own
    // and is matched as a prefix, because a feed may serve several revisions of one resource and the
    // addresses this builds are the same in each.
    private const string FlatContainer = "PackageBaseAddress/3.0.0";

    private readonly Dictionary<string, string> _bases = new(StringComparer.Ordinal);

    // What one lookup came to. `Value` is null exactly where `Problem` is set.
    public sealed record Answer<T>(T? Value, string? Problem) where T : class;

    // Every version of `id` the feed holds, in whatever order it lists them, or why it could not be
    // asked. The caller is choosing the highest version a range admits, so it orders them itself rather
    // than trusting a feed to have done it.
    public Answer<IReadOnlyList<string>> Versions(string source, string id)
    {
        var flat = Base(source);
        if (flat.Value is not { } root) return new Answer<IReadOnlyList<string>>(null, flat.Problem);

        var index = get($"{root}{Lower(id)}/index.json");

        // A package nobody has published yet, which is a corpus depending on one that has not shipped.
        // The caller reports it as no version satisfying the range, which is the sentence worth reading.
        //
        // A private feed answers an anonymous read the same way, so the two are not tellable apart here.
        // `Restore` names the token where it reports an empty listing.
        if (index is { Body: null, Status: 404 })
            return new Answer<IReadOnlyList<string>>([], null);

        if (index.Body is null)
            return new Answer<IReadOnlyList<string>>(null,
                $"could not ask {source} which versions of {id} it holds: {index.Problem}");

        var versions = JsonRead.Parse(Text(index.Body))?["versions"] as JsonArray;
        return new Answer<IReadOnlyList<string>>(
            [.. (versions ?? []).Select(JsonRead.Str).OfType<string>()], null);
    }

    // The package file itself, at one version.
    public Answer<byte[]> Package(string source, string id, string version)
    {
        var flat = Base(source);
        if (flat.Value is not { } root) return new Answer<byte[]>(null, flat.Problem);

        var lower = Lower(id);
        var file = get($"{root}{lower}/{Lower(version)}/{lower}.{Lower(version)}.nupkg");

        return file.Body is null
            ? new Answer<byte[]>(null, $"could not fetch {id} {version} from {source}: {file.Problem}")
            : new Answer<byte[]>(file.Body, null);
    }

    // Where this feed serves packages from, discovered once per source.
    //
    // The address is asked of the feed rather than assembled from its host. Every registry lays its flat
    // container out somewhere different (GitHub Packages puts it under `/download`, and nuget.org on a
    // host of its own), and the service index is the one place each of them says so.
    private Answer<string> Base(string source)
    {
        if (_bases.TryGetValue(source, out var known)) return new Answer<string>(known, null);

        var index = get(source);
        if (index.Body is null)
            return new Answer<string>(null,
                $"could not read the registry at {source}: {index.Problem}. `source:` is the feed's "
                + "service index, which is the URL a push would be sent to.");

        if (JsonRead.Parse(Text(index.Body))?["resources"] is not JsonArray resources)
            return new Answer<string>(null,
                $"{source} answered with something other than a registry service index.");

        var address = resources
            .Select(JsonRead.Object)
            .Where(r => JsonRead.Str(r?["@type"])?.StartsWith(FlatContainer, StringComparison.Ordinal) is true)
            .Select(r => JsonRead.Str(r?["@id"]))
            .FirstOrDefault(id => id is not null);

        if (address is null)
            return new Answer<string>(null,
                $"{source} serves no {FlatContainer} resource, so there is no address to fetch a package "
                + "from.");

        var root = address.EndsWith('/') ? address : address + "/";
        _bases[source] = root;
        return new Answer<string>(root, null);
    }

    // How a fetch is actually performed, which nothing but a run against a real registry uses. Every
    // test hands `Registry` a function of its own, so what a restore comes to is decidable without a
    // network.
    //
    // The token is read from the environment here rather than passed down from the command, because
    // this is the only place it is used and carrying a credential through three signatures is three
    // more places for it to be logged.
    //
    // It travels to every `https://` source the descriptor names, and to none over plain HTTP. A corpus
    // consuming from two registries has one token between them, so a source is a host that gets to see
    // it: `docs/cli/restore.md` says so where somebody sets the variable.
    public static Func<string, Fetched> Over(HttpClient client)
    {
        var token = Environment.GetEnvironmentVariable(TokenVariable);

        return url =>
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                if (token is { Length: > 0 } && request.RequestUri?.Scheme == Uri.UriSchemeHttps)
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using var response = client.Send(request);
                using var body = response.Content.ReadAsStream();
                using var buffer = new MemoryStream();
                body.CopyTo(buffer);

                var status = (int)response.StatusCode;
                return response.IsSuccessStatusCode
                    ? new Fetched(buffer.ToArray(), status, null)
                    : new Fetched(null, status,
                        $"{status} {response.ReasonPhrase}" + Unauthorised(status));
            }
            // The network's own failures, and the three a `source:` nobody checked throws before a packet
            // is sent: a URL with no scheme, one no parser can read, and one naming a protocol this
            // client does not speak. All of them are the descriptor being wrong, and the caller has a
            // sentence ready that names the source and says what the key is for.
            catch (Exception e) when (e is HttpRequestException or TaskCanceledException or IOException
                                          or InvalidOperationException or UriFormatException
                                          or NotSupportedException)
            {
                return new Fetched(null, 0, e.Message.TrimEnd('.'));
            }
        };
    }

    // What a refusal usually means, said where somebody meets it. A private feed answers an anonymous
    // read with 401 and sometimes with 404, so the hint is attached to both rather than to the one that
    // reads as an authentication failure.
    private static string Unauthorised(int status) =>
        status is 401 or 403 or 404
            ? $". A private feed needs a token in {TokenVariable}"
            : "";

    // A registry compares an id and a version without regard to case, and the flat container addresses
    // both in lower case. A package published as `Example.Corpus` is fetched from `example.corpus`.
    private static string Lower(string value) => value.ToLowerInvariant();

    private static string Text(byte[] body) => new UTF8Encoding(false).GetString(body).TrimStart('﻿');
}
