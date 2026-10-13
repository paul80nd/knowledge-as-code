using System.IO.Compression;
using System.Xml.Linq;
using kac.core;
using Reqnroll;
using Xunit;

namespace kac.features;

[Binding]
public sealed class PackSteps(Workspace ws)
{
    private static readonly XNamespace Nuspec = "http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd";

    [Given("the corpus to pack states:")]
    public void GivenTheCorpusToPackStates(string yaml)
        => ws.Write(".corpus.yaml", ws.Read(".corpus.yaml").TrimEnd('\n') + "\n" + yaml + "\n");

    [When("I pack")]
    public void WhenIPack() => ws.Run(root => Commands.Pack(root, null));

    [When(@"I pack for the repository ""(.*)""")]
    public void WhenIPackForTheRepository(string url) => ws.Run(root => Commands.Pack(root, url));

    [Then(@"the package holds ""(.*)""")]
    public void ThenThePackageHolds(string entry) => Assert.Contains(entry, Entries().Keys);

    /// <summary>
    /// Compared as bytes, in both directions, so an entry the export lacks fails as one it holds does.
    /// </summary>
    [Then(@"the package holds ""(.*)"" under ""(.*)"" byte for byte")]
    public void ThenThePackageHoldsUnderByteForByte(string folder, string prefix)
    {
        var want = ws.Snapshot(folder);
        var got = Entries()
            .Where(e => e.Key.StartsWith(prefix, StringComparison.Ordinal))
            .ToDictionary(e => e.Key[prefix.Length..], e => e.Value, StringComparer.Ordinal);

        Assert.NotEmpty(want);
        Assert.Equal(want.Keys, got.Keys.Order(StringComparer.Ordinal));
        Assert.All(want, kv => Assert.True(kv.Value.AsSpan().SequenceEqual(got[kv.Key]), $"{prefix}{kv.Key} differs"));
    }

    [Then("packing again writes the same package")]
    public void ThenPackingAgainWritesTheSamePackage()
    {
        var path = Package();
        var first = File.ReadAllBytes(path);
        Assert.Equal(0, Commands.Pack(ws.Root, null));
        Assert.True(first.AsSpan().SequenceEqual(File.ReadAllBytes(path)), "a second pack wrote different bytes");
    }

    [Then(@"the package's nuspec states ""(.*)"" as ""(.*)""")]
    public void ThenThePackagesNuspecStatesAs(string element, string value)
        => Assert.Equal(value, Metadata().Element(Nuspec + element)?.Value);

    [Then(@"the package's nuspec states no ""(.*)""")]
    public void ThenThePackagesNuspecStatesNo(string element) => Assert.Null(Metadata().Element(Nuspec + element));

    [Then(@"the package's nuspec states the repository ""(.*)""")]
    public void ThenThePackagesNuspecStatesTheRepository(string url)
        => Assert.Equal(url, Metadata().Element(Nuspec + "repository")?.Attribute("url")?.Value);

    /// <summary>The one <c>.nupkg</c> the run wrote, so a scenario names the file once.</summary>
    private string Package() => Assert.Single(Directory.GetFiles(ws.PathOf(Dist.Package), "*.nupkg"));

    private Dictionary<string, byte[]> Entries()
    {
        using var zip = ZipFile.OpenRead(Package());
        var entries = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        foreach (var entry in zip.Entries)
        {
            using var stream = entry.Open();
            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            entries[entry.FullName] = buffer.ToArray();
        }

        return entries;
    }

    private XElement Metadata()
    {
        var nuspec = Assert.Single(Entries(), e => e.Key.EndsWith(".nuspec", StringComparison.Ordinal));
        using var stream = new MemoryStream(nuspec.Value);
        return XDocument.Load(stream).Root?.Element(Nuspec + "metadata")
               ?? throw new InvalidOperationException($"{nuspec.Key} has no metadata element");
    }
}
