using System.Diagnostics;
using kac.core;
using Xunit;
using Reqnroll;

namespace kac.features;

[Binding]
public sealed class CreationSteps
{
    /// <summary>
    /// Two levels, so a descriptor written above the folder is still inside this scenario's tree and deleted with it.
    /// </summary>
    private string _root = "";
    private string _folder = "";
    private int _exit = -1;
    private int _before;

    [AfterScenario]
    public void Clean()
    {
        if (_root.Length == 0 || !Directory.Exists(_root)) return;
        try
        {
            Directory.Delete(_root, recursive: true);
        }
        catch (IOException)
        {
        }
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

    [When("I create a corpus there")]
    public void WhenICreateACorpusThere() => _exit = Creation.Create(_folder);

    [When("I create a corpus from a template that is not there")]
    public void WhenICreateACorpusFromATemplateThatIsNotThere() =>
        _exit = Creation.Create(_folder, "https://example.invalid/no/such/repository.git");

    [When(@"I create a corpus there adopting ""(.*)""")]
    public void WhenICreateACorpusThereAdopting(string types)
        => _exit = Creation.Create(_folder, types: types);

    [Then("no link fails to resolve")]
    public void ThenNoLinkFailsToResolve()
    {
        var dangling = Validator.CheckAll(Corpus.Load(_folder), Harness.Today)
            .Where(f => f.Check.Value == "link-resolves")
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

    [Then("it refuses")]
    public void ThenItRefuses() => Assert.NotEqual(0, _exit);

    [Then("it succeeds")]
    public void ThenItSucceeds() => Assert.Equal(0, _exit);

    [Then("the folder holds nothing new")]
    public void ThenTheFolderHoldsNothingNew() => Assert.Equal(_before, Count());

    [Then("the folder is a git repository")]
    public void ThenTheFolderIsAGitRepository()
        => Assert.True(Directory.Exists(Path.Combine(_folder, ".git")));

    [Then(@"""(.*)"" is still there")]
    public void ThenIsStillThere(string name)
        => Assert.True(File.Exists(Path.Combine(_folder, name)));

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
