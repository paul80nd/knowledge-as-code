using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;

namespace kac.core;

// What a request came back with: the bytes, or why there are none. Exactly one of the two is set. `Status` is zero
// where nothing answered, and only the caller knows whether a 404 means an unpublished package or a wrong address.
public sealed record Fetched(byte[]? Body, int Status, string? Problem);

// A folder of `.nupkg` files, which NuGet calls a local feed. Two functions, so a restore stays decidable without a
// filesystem. `Names` is null where there is no folder, and `Read` is null where a file could not be read.
public sealed record FolderFeed(Func<string, IReadOnlyList<string>?> Names, Func<string, byte[]?> Read);

// Where a package comes from: which versions of a corpus a source holds, and the bytes of one. A source is a NuGet feed
// or a local folder, and `Restore` never learns which. Every method takes the declared source, and the base each one
// resolved to is remembered here, so a feed's index is asked for once.
public sealed class Registry(Func<string, Fetched> get, FolderFeed folder)
{
    // The environment variable naming the bearer token. A token belongs in the environment, because a flag lands in
    // shell history and CI logs.
    public const string TokenVariable = "KAC_REGISTRY_TOKEN";

    // What the service index calls the resource this reads. Matched as a prefix, because a feed may serve several
    // revisions of one resource.
    private const string FlatContainer = "PackageBaseAddress/3.0.0";

    private readonly Dictionary<string, string> _bases = new(StringComparer.Ordinal);

    // What one lookup came to. `Value` is null exactly where `Problem` is set.
    public sealed record Answer<T>(T? Value, string? Problem) where T : class
    {
        // The invariant above, told to the compiler, so a caller that branches on it reads the other
        // half without asserting anything. Every construction below holds it.
        [MemberNotNullWhen(true, nameof(Problem))]
        [MemberNotNullWhen(false, nameof(Value))]
        public bool Failed => Problem is not null;
    }

    // Every version of `id` the feed holds, in whatever order it lists them, or why it could not be asked. The caller
    // orders them itself.
    public Answer<IReadOnlyList<string>> Versions(string source, string id)
    {
        if (!IsFeed(source)) return FolderVersions(source, id);

        var flat = Base(source);
        if (flat.Value is not { } root) return new Answer<IReadOnlyList<string>>(null, flat.Problem);

        var index = get($"{root}{Lower(id)}/index.json");

        if (index is { Body: null, Status: 404 })
            return new Answer<IReadOnlyList<string>>([], null);

        if (index.Body is null)
            return new Answer<IReadOnlyList<string>>(null,
                $"could not ask {source} which versions of {id} it holds: {index.Problem}");

        var versions = JsonRead.Parse(Text(index.Body))?["versions"] as JsonArray;
        return new Answer<IReadOnlyList<string>>(
            [.. (versions ?? []).Select(JsonRead.Str).OfType<string>()], null);
    }

    public Answer<byte[]> Package(string source, string id, string version)
    {
        if (!IsFeed(source)) return FolderPackage(source, id, version);

        var flat = Base(source);
        if (flat.Value is not { } root) return new Answer<byte[]>(null, flat.Problem);

        var lower = Lower(id);
        var file = get($"{root}{lower}/{Lower(version)}/{lower}.{Lower(version)}.nupkg");

        return file.Body is null
            ? new Answer<byte[]>(null, $"could not fetch {id} {version} from {source}: {file.Problem}")
            : new Answer<byte[]>(file.Body, null);
    }

    // A registry is addressed over HTTP and anything else is a path, so an unsupported scheme reads as a missing
    // folder.
    private static bool IsFeed(string source) =>
        source.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
        || source.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

    // What an empty version listing means, which differs for each form of source.
    public static string Absent(string source) =>
        IsFeed(source)
            ? "That registry holds no version of it at all, or holds them behind a token this run did "
              + $"not have. Set {TokenVariable} where the feed is private."
            : "That folder holds no package for it. Run `kac export` and then `kac pack` in the "
              + "producing corpus.";

    // Every version a folder holds, read off the names `kac pack` wrote.
    private Answer<IReadOnlyList<string>> FolderVersions(string source, string id)
    {
        if (folder.Names(source) is not { } names)
            return new Answer<IReadOnlyList<string>>(null, NoFolder(source));

        return new Answer<IReadOnlyList<string>>(
            [.. names.Select(n => VersionIn(n, id)).OfType<string>()], null);
    }

    private Answer<byte[]> FolderPackage(string source, string id, string version)
    {
        if (folder.Names(source) is not { } names)
            return new Answer<byte[]>(null, NoFolder(source));

        var name = names.FirstOrDefault(n =>
            string.Equals(VersionIn(n, id), version, StringComparison.OrdinalIgnoreCase));

        if (name is null)
            return new Answer<byte[]>(null, $"{source} holds no package for {id} {version}.");

        var body = folder.Read(Path.Combine(source, name));
        return body is null
            ? new Answer<byte[]>(null, $"could not read {name} from {source}.")
            : new Answer<byte[]>(body, null);
    }

    // The version inside a package's file name, and null where the name belongs to another package or to none. A
    // version opens on a digit, which keeps `Acme.Core.Abstractions` from reading as a version of `Acme.Core`.
    private static string? VersionIn(string name, string id)
    {
        const string suffix = ".nupkg";
        var prefix = id + ".";

        if (!name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) return null;
        if (!name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return null;

        var version = name[prefix.Length..^suffix.Length];
        return version.Length > 0 && version[0] is >= '0' and <= '9' ? version : null;
    }

    private static string NoFolder(string source) =>
        $"there is no folder at {source}. `source:` names either a registry's service index or a folder "
        + "holding the packages a producer built.";

    // Where this feed serves packages from, discovered once per source.
    //
    // The address is asked of the feed rather than assembled from its host. Every registry lays its flat container out
    // somewhere different (GitHub Packages puts it under `/download`, and nuget.org on a host of its own), and the
    // service index is the one place each of them says so.
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

    // How a fetch is actually performed. Tests hand `Registry` a function of their own. The token is read from the
    // environment here, the only place it is used, and travels to every `https://` source and to none over plain HTTP.
    // `docs/cli/restore.md` says so where somebody sets the variable.
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
            catch (Exception e) when (e is HttpRequestException or TaskCanceledException or IOException
                                          or InvalidOperationException or UriFormatException
                                          or NotSupportedException)
            {
                return new Fetched(null, 0, e.Message.TrimEnd('.'));
            }
        };
    }

    // How a folder is actually read. A relative `source:` resolves against the corpus root, as `upstream.url` does, and
    // an absolute path is taken as it stands.
    public static FolderFeed OnDisk(string corpusRoot)
    {
        return new FolderFeed(
            source => Directory.Exists(At(source))
                ? [.. Directory.EnumerateFiles(At(source)).Select(Path.GetFileName).OfType<string>()]
                : null,
            path => File.Exists(At(path)) ? File.ReadAllBytes(At(path)) : null);

        string At(string relative) => Path.Combine(corpusRoot, relative);
    }

    private static string Unauthorised(int status) =>
        status is 401 or 403 or 404
            ? $". A private feed needs a token in {TokenVariable}"
            : "";

    private static string Lower(string value) => value.ToLowerInvariant();

    private static string Text(byte[] body) => new UTF8Encoding(false).GetString(body).TrimStart('﻿');
}
