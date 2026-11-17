using System.Text;
using kac.core;

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public class RouterListingTests
{
    private static readonly string Plugin = Path.Combine(Repo.Root, "template", ".plugin");

    /// <summary>
    /// A corpus adopting every type lists each reference file's trigger at once, which is the longest listing any
    /// adopter can produce. <c>kac new --types all</c> stands one up, so a trigger lengthened past the cap fails its
    /// first bundle.
    /// </summary>
    [Fact]
    public void A_corpus_exporting_every_type_bundles_within_the_listing_cap()
    {
        var tree = Directory.EnumerateFiles(Plugin, "*", SearchOption.AllDirectories)
            .Select(f => new BundleFile(Path.GetRelativePath(Plugin, f).Replace('\\', '/'), File.ReadAllBytes(f)))
            .ToList();

        var plan = Bundler.Plan(new BundleSource(tree, [new BundleFile(Exporter.ManifestFile, Manifest())]));

        Assert.Empty(plan.Problems);
    }

    private static byte[] Manifest()
    {
        var types = string.Join(",", Schema.Load(Repo.Root).ByFolder.Select(t =>
            $$"""{"type":"{{t.Key}}","shapeVersion":{{t.Value.DeclaredExport.Version}},"records":1}"""));

        return Encoding.UTF8.GetBytes(
            $$"""
              {
                "formatVersion": {{Exporter.FormatVersion}},
                "corpus": "every-type",
                "contentVersion": "1.0.0",
                "types": [{{types}}]
              }
              """);
    }
}
