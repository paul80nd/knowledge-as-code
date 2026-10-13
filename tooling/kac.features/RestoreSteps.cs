using System.IO.Compression;
using kac.core;
using Reqnroll;
using Xunit;

namespace kac.features;

/// <summary>
/// A restore reads a folder source this class fills: the pack fixture, exported and packed at a version a scenario
/// names, into <c>feed/</c> inside the consuming corpus. Nothing reaches the network.
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
    public void GivenTheProducerIsPackedForRestoreAt(string version)
    {
        var producer = Harness.Assemble("pack");
        try
        {
            var descriptor = Path.Combine(producer, ".corpus.yaml");
            const string stated = "content-version: \"2.3.4\"";
            var text = File.ReadAllText(descriptor);
            Assert.Contains(stated, text);
            File.WriteAllText(descriptor, text.Replace(stated, $"content-version: \"{version}\""));

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
}
