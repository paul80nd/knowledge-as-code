using System.Diagnostics;
using kac.core;
using Xunit;
using Reqnroll;

namespace kac.features;

[Binding]
public sealed class CreationSteps(Workspace ws)
{
    /// <summary>
    /// Two levels, so a descriptor written above the folder is still inside this scenario's tree and deleted with it.
    /// </summary>
    private string _root = "";
    private string _folder = "";
    private string _template = "";
    private int _before;

    private const string DeviationLink = "[deviation](../deviations.md)";

    [AfterScenario]
    public void Clean()
    {
        if (_root.Length > 0) Harness.Delete(_root);
    }

    [Given("an empty folder")]
    public void GivenAnEmptyFolder()
    {
        _root = Path.Combine(Path.GetTempPath(), "kac-creation-" + Guid.NewGuid().ToString("N"));
        _folder = Path.Combine(_root, "corpus");
        Directory.CreateDirectory(_folder);
    }

    [Given("a git repository")]
    public void GivenAGitRepository()
    {
        GivenAnEmptyFolder();
        Git("init", "-q", "-b", "main", ".");
    }

    /// <summary>Set in the repository's own config, which git reads ahead of the global one.</summary>
    [Given(@"a git repository whose author is ""(.*)""")]
    public void GivenAGitRepositoryWhoseAuthorIs(string email)
    {
        GivenAGitRepository();
        Git("config", "user.email", email);
    }

    [Given("a corpus descriptor above it")]
    public void GivenACorpusDescriptorAboveIt()
    {
        File.WriteAllText(Path.Combine(_root, ".corpus.yaml"), "corpus: the-one-above\n");
        _before = Count();
    }

    [Given("a file changed since the last commit")]
    public void GivenAFileChangedSinceTheLastCommit()
    {
        File.WriteAllText(Path.Combine(_folder, "notes.md"), "committed\n");
        Commit();
        File.WriteAllText(Path.Combine(_folder, "notes.md"), "changed\n");
        _before = Count();
    }

    /// <summary>
    /// Committed, so the dirty-tree refusal does not fire and the folder-not-empty warning is what runs.
    /// </summary>
    [Given(@"it holds a committed ""(.*)""")]
    public void GivenItHoldsACommitted(string name)
    {
        File.WriteAllText(Path.Combine(_folder, name), "ours\n");
        Commit();
        _before = Count();
    }

    /// <summary>
    /// A copy of this repository's template, beside the folder and outside its repository, so the edit leaves the tree
    /// clean.
    /// </summary>
    [Given("a template whose standards template links the deviations page by reference")]
    public void GivenATemplateWhoseStandardsTemplateLinksTheDeviationsPageByReference()
    {
        _template = Path.Combine(_root, "template-copy");
        var manifest = Manifest.LoadFrom(Path.Combine(Harness.RepoRoot, Manifest.FileName));
        var tracked = GitFiles.Tracked(Harness.RepoRoot)
                      ?? throw new InvalidOperationException("kac.features: the repository has no git listing.");

        // The manifest withholds itself, and `new` reads it from the template's root.
        foreach (var path in tracked.Where(p => p == Manifest.FileName
                                                || manifest.Resolve(p) is Manifest.Overlay or Manifest.Seed))
        {
            var to = Path.Combine(_template, path.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(to) ?? _template);
            File.Copy(Path.Combine(Harness.RepoRoot, path), to);
        }

        var page = Path.Combine(_template, "template", "standards", "_template.md");
        var inline = File.ReadAllText(page);
        Assert.Contains(DeviationLink, inline);
        File.WriteAllText(page,
            inline.Replace(DeviationLink, "[deviation][deviations]", StringComparison.Ordinal)
            + "\n[deviations]: ../deviations.md\n");
    }

    [When(@"I create a corpus there from that template adopting ""(.*)""")]
    public void WhenICreateACorpusThereFromThatTemplateAdopting(string types)
        => ws.Exit = Creation.Create(_folder, from: _template, types: types);

    [When("I create a corpus there")]
    public void WhenICreateACorpusThere() => ws.Exit = Creation.Create(_folder);

    [When("I create a corpus from a template that is not there")]
    public void WhenICreateACorpusFromATemplateThatIsNotThere() =>
        ws.Exit = Creation.Create(_folder, "https://example.invalid/no/such/repository.git");

    [When("I create a corpus there with nobody to answer")]
    public void WhenICreateACorpusThereWithNobodyToAnswer() => ws.Exit = Creation.Create(_folder, yes: false);

    [When("I create a corpus there with a kac older than the template needs")]
    public void WhenICreateACorpusThereWithAKacOlderThanTheTemplateNeeds()
        => ws.Exit = Creation.Create(_folder, tool: Creation.TooOld);

    [When(@"I create a corpus there with kac ""(.*)""")]
    public void WhenICreateACorpusThereWithKac(string tool) => ws.Exit = Creation.Create(_folder, tool: tool);

    /// <summary>Committed, so the update meets the clean tree a merged Dependabot pull request leaves.</summary>
    [When(@"the corpus moves its tool manifest to kac ""(.*)""")]
    public void WhenTheCorpusMovesItsToolManifestToKac(string version)
    {
        var manifest = At(".config/dotnet-tools.json");
        var held = File.ReadAllText(manifest);
        Assert.Contains("\"version\": \"0.99.0\"", held);
        File.WriteAllText(manifest, held.Replace("\"version\": \"0.99.0\"", $"\"version\": \"{version}\"",
            StringComparison.Ordinal));
        Commit();
    }

    [When(@"I create a corpus there named ""(.*)""")]
    public void WhenICreateACorpusThereNamed(string name) => ws.Exit = Creation.Create(_folder, name: name);

    [When(@"I create a corpus there owned by ""(.*)""")]
    public void WhenICreateACorpusThereOwnedBy(string owner) => ws.Exit = Creation.Create(_folder, owner: owner);

    [When(@"I create a corpus there built by ""(.*)""")]
    public void WhenICreateACorpusThereBuiltBy(string ci) => ws.Exit = Creation.Create(_folder, ci: ci);

    [When(@"I create a corpus there adopting ""(.*)""")]
    public void WhenICreateACorpusThereAdopting(string types)
        => ws.Exit = Creation.Create(_folder, types: types);

    [Then("no link fails to resolve")]
    public void ThenNoLinkFailsToResolve()
    {
        var dangling = Validator.CheckAll(Corpus.Load(_folder), Harness.Today)
            .Where(f => f.Check.Value is "link-resolves" or "template-folder-resolves")
            .Select(f => $"{f.File}: {f.Message}")
            .ToList();

        Assert.True(dangling.Count == 0, string.Join("\n", dangling));
    }

    [Then("a full update finds it in step")]
    public void ThenAFullUpdateFindsItInStep()
        => Assert.Equal(0, Creation.WouldChangeUnderFull(_folder));

    [Then(@"""(.*)"" links to ""(.*)""")]
    public void ThenLinksTo(string page, string target)
        => Assert.Contains($"]({target})", File.ReadAllText(Path.Combine(_folder, page)));

    [Then("the folder holds nothing new")]
    public void ThenTheFolderHoldsNothingNew() => Assert.Equal(_before, Count());

    [Then("the folder is a git repository")]
    public void ThenTheFolderIsAGitRepository()
        => Assert.True(Directory.Exists(Path.Combine(_folder, ".git")));

    [Then("the folder is not a git repository")]
    public void ThenTheFolderIsNotAGitRepository()
        => Assert.False(Directory.Exists(Path.Combine(_folder, ".git")));

    [Then(@"""(.*)"" in the new corpus says ""(.*)""")]
    public void ThenInTheNewCorpusSays(string path, string text)
        => Assert.Contains(text, File.ReadAllText(At(path)));

    [Then(@"""(.*)"" in the new corpus states no ""(.*)"" block")]
    public void ThenInTheNewCorpusStatesNoBlock(string path, string key)
        => Assert.DoesNotContain(File.ReadAllLines(At(path)),
            line => line.StartsWith(key + ":", StringComparison.Ordinal));

    [Then(@"the new corpus holds ""(.*)""")]
    public void ThenTheNewCorpusHolds(string path) => Assert.True(File.Exists(At(path)), $"no {path}");

    [Then(@"the new corpus has no ""(.*)""")]
    public void ThenTheNewCorpusHasNo(string path) => Assert.False(File.Exists(At(path)), $"{path} is there");

    /// <summary>Passes on Windows, which keeps no mode bits.</summary>
    [Then(@"""(.*)"" in the new corpus can be run")]
    public void ThenInTheNewCorpusCanBeRun(string path)
        => Assert.True(OperatingSystem.IsWindows()
                       || (File.GetUnixFileMode(At(path)) & UnixFileMode.UserExecute) != 0, $"{path} cannot be run");

    [Then(@"""(.*)"" is still there")]
    public void ThenIsStillThere(string name)
        => Assert.True(File.Exists(Path.Combine(_folder, name)));

    private string At(string path) => Path.Combine(_folder, path.Replace('/', Path.DirectorySeparatorChar));

    private int Count() =>
        Directory.EnumerateFileSystemEntries(_folder).Count(e => Path.GetFileName(e) != ".git");

    private void Commit()
    {
        Git("add", "-A");
        Git("-c", "user.email=test@example.com", "-c", "user.name=Test", "-c", "commit.gpgsign=false",
            "commit", "-qm", "fixture");
    }

    private void Git(params string[] args)
    {
        var psi = new ProcessStartInfo("git")
        {
            WorkingDirectory = _folder,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        foreach (var a in args) psi.ArgumentList.Add(a);

        using var p = Process.Start(psi);
        p?.WaitForExit();
    }
}
