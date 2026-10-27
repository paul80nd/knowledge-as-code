using System.IO.Compression;
using System.Net;
using System.Text;
using kac.core;
using Reqnroll;
using Xunit;

namespace kac.features;

/// <summary>
/// A restore reads a folder source this class fills: the pack fixture, exported and packed at a version a scenario
/// names, into <c>feed/</c> inside the consuming corpus. <c>PrivateFeed</c> serves the same folder as a registry, and
/// nothing reaches the network.
/// </summary>
[Binding]
public sealed class RestoreSteps(Workspace ws)
{
    private const string Feed = "feed";

    private const string Producer = "example-libraries";

    /// <summary>Fails the scenario where a restore asks anything but the folder.</summary>
    private Registry Registry => new(
        url => throw new InvalidOperationException($"a restore spec reached for the network: {url}"),
        Registry.OnDisk(ws.Root));

    [Given(@"the producer is packed for restore at ""(.*)""")]
    public void GivenTheProducerIsPackedForRestoreAt(string version) => Pack(Producer, version);

    /// <summary>The pack fixture under another corpus name, and the same shortcode.</summary>
    [Given(@"""(.*)"" is packed for restore at ""(.*)""")]
    public void GivenIsPackedForRestoreAt(string corpus, string version) => Pack(corpus, version);

    private void Pack(string corpus, string version)
    {
        var producer = Harness.Assemble("pack");
        try
        {
            var descriptor = Path.Combine(producer, ".corpus.yaml");
            const string stated = "content-version: \"2.3.4\"";
            var named = $"corpus: {Producer}\n";
            var text = File.ReadAllText(descriptor);
            Assert.Contains(stated, text);
            Assert.Contains(named, text);
            File.WriteAllText(descriptor, text
                .Replace(stated, $"content-version: \"{version}\"")
                .Replace(named, $"corpus: {corpus}\n"));

            Assert.Equal(0, ExportSteps.Export(producer));
            Assert.Equal(0, Commands.Pack(producer, null));

            var packed = Path.Combine(producer, Dist.Package.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(ws.PathOf(Feed));
            foreach (var file in Directory.EnumerateFiles(packed, "*.nupkg"))
                File.Copy(file, Path.Combine(ws.PathOf(Feed), Path.GetFileName(file)));
        }
        finally
        {
            Harness.Delete(producer);
        }
    }

    [Given(@"the corpus restores the producer as ""(.*)"" at ""(.*)""")]
    public void GivenTheCorpusRestoresTheProducerAsAt(string shortcode, string range)
        => Consume(Producer, shortcode, range, Feed);

    [Given(@"the corpus restores ""(.*)"" as ""(.*)"" at ""(.*)"" from ""(.*)""")]
    public void GivenTheCorpusRestoresAsAtFrom(string corpus, string shortcode, string range, string source)
        => Consume(corpus, shortcode, range, source);

    [Given(@"the restore lock reads ""(.*)""")]
    public void GivenTheRestoreLockReads(string version)
        => ws.Write(".corpus.yaml", ws.Read(".corpus.yaml") + $"    resolved: \"{version}\"\n");

    [Given(@"the restore range is changed to ""(.*)""")]
    public void GivenTheRestoreRangeIsChangedTo(string range)
    {
        var lines = ws.Read(".corpus.yaml").Split('\n');
        var at = Array.FindIndex(lines, l => l.TrimStart().StartsWith("version:", StringComparison.Ordinal));
        Assert.True(at >= 0, ".corpus.yaml states no version to change");
        lines[at] = $"    version: \"{range}\"";
        ws.Write(".corpus.yaml", string.Join('\n', lines));
    }

    [Given("it has been restored")]
    public void GivenItHasBeenRestored() => Assert.Equal(0, Commands.Restore(ws.Root, Registry));

    /// <summary>Adds an entry past the 16MB cap to the payload, where <c>restore</c> reads it.</summary>
    [Given(@"the packed ""(.*)"" holds an entry past the restore cap")]
    public void GivenThePackedHoldsAnEntryPastTheRestoreCap(string version)
    {
        var package = Directory.EnumerateFiles(ws.PathOf(Feed), $"*.{version}.nupkg").Single();
        using var archive = ZipFile.Open(package, ZipArchiveMode.Update);
        using var stream = archive.CreateEntry($"{Packer.PayloadDir}/padding.bin").Open();
        var chunk = new byte[1024 * 1024];
        for (var i = 0; i < 17; i++) stream.Write(chunk);
    }

    [When("I restore")]
    public void WhenIRestore() => ws.Run(root => Commands.Restore(root, Registry));

    /// <summary>
    /// Through <c>Registry.Over</c>, with an environment naming the token alone. The feed is served in memory under
    /// either scheme, and answers only a request carrying the token as a bearer token.
    /// </summary>
    [When(@"I restore from a private feed with the registry token ""(.*)""")]
    public void WhenIRestoreFromAPrivateFeedWithTheRegistryToken(string token)
        => RestoreFromAPrivateFeed($"Bearer {token}", (Registry.TokenVariable, token));

    /// <summary>
    /// Through <c>Registry.Over</c>, with an environment naming the user and the token. The feed answers only a
    /// request carrying both as Basic auth.
    /// </summary>
    [When(@"I restore from a private feed with the registry user ""(.*)"" and token ""(.*)""")]
    public void WhenIRestoreFromAPrivateFeedWithTheRegistryUserAndToken(string user, string token)
        => RestoreFromAPrivateFeed(
            $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{token}"))}",
            (Registry.UserVariable, user), (Registry.TokenVariable, token));

    [Then(@"the restore locks the producer at ""(.*)""")]
    public void ThenTheRestoreLocksTheProducerAt(string version)
        => Assert.Contains($"resolved: \"{version}\"", ws.Read(".corpus.yaml"));

    [Then(@"""(.*)"" was restored at ""(.*)""")]
    public void ThenWasRestoredAt(string shortcode, string version)
        => Assert.Equal(new Imported(Producer, version), Restore.Installed(ws.Root, shortcode));

    private void Consume(string corpus, string shortcode, string range, string source)
    {
        var descriptor = ws.Read(".corpus.yaml");
        if (!descriptor.Contains("consumes:\n", StringComparison.Ordinal)) descriptor += "\nconsumes:\n";
        ws.Write(".corpus.yaml", descriptor
                                 + $"  - corpus: {corpus}\n    shortcode: {shortcode}\n"
                                 + $"    version: \"{range}\"\n    source: {source}\n");
    }

    private void RestoreFromAPrivateFeed(string authorization, params (string Name, string Value)[] env)
    {
        var get = Registry.Over(new HttpClient(new PrivateFeed(ws.PathOf(Feed), authorization)),
            name => env.FirstOrDefault(e => e.Name == name).Value);

        ws.Run(root => Commands.Restore(root, new Registry(get, Registry.OnDisk(root))));
    }

    /// <summary>
    /// A registry serving the packages in <c>feed/</c> at any host and scheme, refusing every request whose
    /// <c>Authorization</c> header is not the one it was given.
    /// </summary>
    private sealed class PrivateFeed(string folder, string authorization) : HttpMessageHandler
    {
        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Headers.Authorization?.ToString() != authorization)
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);

            var uri = request.RequestUri ?? throw new InvalidOperationException("a request with no address");
            var path = uri.AbsolutePath;

            if (path == "/v3/index.json")
                return Json($$"""{"resources":[{"@id":"{{uri.Scheme}}://{{uri.Authority}}/flat/",""" +
                            """ "@type":"PackageBaseAddress/3.0.0"}]}""");

            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts is ["flat", var id, "index.json"])
            {
                var versions = Directory.EnumerateFiles(folder, $"{id}.*.nupkg")
                    .Select(f => Path.GetFileName(f)[(id.Length + 1)..^".nupkg".Length])
                    .Select(v => $"\"{v}\"");
                return Json($"{{\"versions\":[{string.Join(',', versions)}]}}");
            }

            // The registry asks for every name in lower case, which is how `pack` writes them.
            if (parts is ["flat", _, _, var name] && File.Exists(Path.Combine(folder, name)))
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(File.ReadAllBytes(Path.Combine(folder, name)))
                };

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken) => Task.FromResult(Send(request, cancellationToken));

        private static HttpResponseMessage Json(string body) =>
            new(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
    }
}
