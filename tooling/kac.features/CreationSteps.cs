using System.Diagnostics;
using kac.core;
using Xunit;
using Reqnroll;

namespace kac.features;

[Binding]
public sealed class CreationSteps
{
    // The scenario's own tree, and the folder inside it the corpus is created in. Two levels, because one
    // scenario writes a descriptor above the folder and the level it writes to has to be this scenario's
    // to delete. Written into the shared temp root, it would be a corpus above every scenario after it.
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

    // Above the folder rather than in it, because the framework reaching a subfolder of a corpus is the
    // same fault as it reaching the corpus itself, and it is the quieter of the two.
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

    // Committed, so the tree is clean and the check that stops a dirty one does not fire. What this pins
    // is that the warning about a folder holding files is a warning and never a second refusal.
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

    // Read from the corpus that was written rather than from the exit code, because a corpus adopting a
    // subset still fails `schema-dispatch` and exits non-zero. Creation.feature says why.
    [Then("no link fails to resolve")]
    public void ThenNoLinkFailsToResolve()
    {
        var dangling = Validator.CheckAll(Corpus.Load(_folder))
            .Where(f => f.Check.Value == "link-resolves")
            .Select(f => $"{f.File}: {f.Message}")
            .ToList();

        Assert.True(dangling.Count == 0, string.Join("\n", dangling));
    }

    // The unlinking has to reach the comparison as well as the write. Held to the template as authored, a
    // corpus that declined types reads as behind on every seed page it holds, and a full update puts back
    // the links its own `types:` says it cannot follow.
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

    // Everything the folder holds, `.git` aside, so a refusal can be held to having written none of it.
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
