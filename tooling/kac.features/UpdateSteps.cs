using System.Diagnostics;
using kac.core;
using Reqnroll;
using Xunit;

namespace kac.features;

/// <summary>
/// Each scenario stands a corpus up from this repository and commits it, so <c>git status</c> is what a run wrote.
/// </summary>
[Binding]
public sealed class UpdateSteps(Workspace ws)
{
    private string _root = "";

    private string Folder => Path.Combine(_root, "corpus");

    [AfterScenario]
    public void Clean()
    {
        if (_root.Length > 0) Harness.Delete(_root);
    }

    [Given(@"a corpus to update, adopting ""(.*)""")]
    public void GivenACorpusToUpdateAdopting(string types)
    {
        _root = Path.Combine(Path.GetTempPath(), "kac-update-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Folder);
        Git("init", "-q", "-b", "main", ".");
        Assert.Equal(0, Creation.Create(Folder, types: types));
        Commit();
    }

    [Given(@"the corpus to update commits a change to ""(.*)""")]
    public void GivenTheCorpusToUpdateCommitsAChangeTo(string path)
    {
        File.AppendAllText(At(path), "\nEdited by the corpus.\n");
        Commit();
    }

    [Given(@"the corpus to update commits a record ""(.*)""")]
    public void GivenTheCorpusToUpdateCommitsARecord(string path)
    {
        File.WriteAllText(At(path), "# A record\n");
        Commit();
    }

    [Given(@"the corpus to update commits ""(.*)"" in its descriptor")]
    public void GivenTheCorpusToUpdateCommitsInItsDescriptor(string line)
    {
        File.AppendAllText(At(".corpus.yaml"), $"\n{line}\n");
        Commit();
    }

    [Given(@"the corpus to update holds an uncommitted change to ""(.*)""")]
    public void GivenTheCorpusToUpdateHoldsAnUncommittedChangeTo(string path)
        => File.AppendAllText(At(path), "\nNot committed.\n");

    [When("I update the corpus")]
    public void WhenIUpdateTheCorpus() => Run(new UpdateRequest());

    [When("I check the corpus for an update")]
    public void WhenICheckTheCorpusForAnUpdate() => Run(new UpdateRequest { Check = true });

    [When(@"I update the corpus under the ""(.*)"" policy")]
    public void WhenIUpdateTheCorpusUnderThePolicy(string policy) => Run(new UpdateRequest { Policy = policy });

    [When(@"I update the corpus adding ""(.*)""")]
    public void WhenIUpdateTheCorpusAdding(string type) => Run(new UpdateRequest { AddType = type });

    [When(@"I update the corpus dropping ""(.*)"" with --yes")]
    public void WhenIUpdateTheCorpusDroppingWithYes(string type)
        => Run(new UpdateRequest { DropType = type, Yes = true });

    [When(@"I update the corpus dropping ""(.*)"" with nobody to answer")]
    public void WhenIUpdateTheCorpusDroppingWithNobodyToAnswer(string type)
        => Run(new UpdateRequest { DropType = type });

    [Then("the update left the corpus as it was")]
    public void ThenTheUpdateLeftTheCorpusAsItWas() => Assert.Equal("", Status());

    [Then(@"the update changed ""(.*)""")]
    public void ThenTheUpdateChanged(string path) => Assert.Contains(path, Status());

    [Then(@"the update left ""(.*)"" alone")]
    public void ThenTheUpdateLeftAlone(string path) => Assert.DoesNotContain(path, Status());

    [Then(@"the updated corpus holds ""(.*)""")]
    public void ThenTheUpdatedCorpusHolds(string path) => Assert.True(File.Exists(At(path)), $"no {path}");

    [Then(@"the updated corpus has no ""(.*)""")]
    public void ThenTheUpdatedCorpusHasNo(string path)
        => Assert.False(File.Exists(At(path)) || Directory.Exists(At(path)), $"{path} is there");

    [Then(@"""(.*)"" in the updated corpus no longer says ""(.*)""")]
    public void ThenInTheUpdatedCorpusNoLongerSays(string path, string text)
        => Assert.DoesNotContain(text, File.ReadAllText(At(path)));

    [Then(@"""(.*)"" in the updated corpus still says ""(.*)""")]
    public void ThenInTheUpdatedCorpusStillSays(string path, string text)
        => Assert.Contains(text, File.ReadAllText(At(path)));

    [Then(@"the updated corpus adopts ""(.*)""")]
    public void ThenTheUpdatedCorpusAdopts(string type) => Assert.Contains(type, Adopted());

    [Then(@"the updated corpus does not adopt ""(.*)""")]
    public void ThenTheUpdatedCorpusDoesNotAdopt(string type) => Assert.DoesNotContain(type, Adopted());

    private void Run(UpdateRequest request) => ws.Exit = Creation.Update(Folder, request);

    private List<string> Adopted() =>
        CorpusDescriptor.Load(Folder).Types ?? throw new InvalidOperationException("the descriptor states no types");

    private string At(string path) => Path.Combine(Folder, path.Replace('/', Path.DirectorySeparatorChar));

    /// <summary>Includes untracked files, so a file the run created counts as written.</summary>
    private string Status() => Git("status", "--porcelain", "--untracked-files=all").Trim();

    private void Commit()
    {
        Git("add", "-A");
        Git("commit", "-qm", "fixture");
    }

    /// <summary>Under an identity of its own, because the machine running the specs may have none configured.</summary>
    private string Git(params string[] args)
    {
        var psi = new ProcessStartInfo("git")
        {
            WorkingDirectory = Folder,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        foreach (var a in (string[])["-c", "user.email=test@example.com", "-c", "user.name=Test",
                     "-c", "commit.gpgsign=false", .. args])
            psi.ArgumentList.Add(a);

        using var p = Process.Start(psi) ?? throw new InvalidOperationException("git did not start");
        var output = p.StandardOutput.ReadToEnd();
        p.StandardError.ReadToEnd();
        p.WaitForExit();
        return output;
    }
}
