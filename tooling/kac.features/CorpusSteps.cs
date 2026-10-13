using Reqnroll;
using Xunit;

namespace kac.features;

/// <summary>The steps every command's scenarios share: the corpus they start from, and what a run left on disk.</summary>
[Binding]
public sealed class CorpusSteps(Workspace ws)
{
    [Given("the (.*) fixture corpus")]
    public void GivenTheFixtureCorpus(string name) => ws.Assemble(name);

    [Given(@"""(.*)"" is deleted")]
    public void GivenIsDeleted(string path)
    {
        var full = ws.PathOf(path);
        if (Directory.Exists(full)) Directory.Delete(full, recursive: true);
        else File.Delete(full);
    }

    [Then("it refuses")]
    public void ThenItRefuses() => Assert.Equal(1, ws.Exit);

    /// <summary>A check that ran and found a fault, where <c>it refuses</c> is a command that stopped before acting.</summary>
    [Then("it fails")]
    public void ThenItFails() => Assert.Equal(1, ws.Exit);

    [Then("it succeeds")]
    public void ThenItSucceeds() => Assert.Equal(0, ws.Exit);

    [Then("nothing is written")]
    public void ThenNothingIsWritten() => Assert.Empty(ws.Changed());

    [Then(@"^nothing outside \.dist/ changed$")]
    public void ThenNothingOutsideDistChanged()
        => Assert.DoesNotContain(ws.Changed(), p => !p.StartsWith(".dist/", StringComparison.Ordinal));

    [Then(@"only ""(.*)"" changed")]
    public void ThenOnlyChanged(string path) => Assert.Equal([path], ws.Changed());

    [Then(@"""(.*)"" exists")]
    public void ThenExists(string path) => Assert.True(File.Exists(ws.PathOf(path)), $"no {path}");

    [Then(@"""(.*)"" does not exist")]
    public void ThenDoesNotExist(string path)
        => Assert.False(File.Exists(ws.PathOf(path)) || Directory.Exists(ws.PathOf(path)), $"{path} is there");

    [Then(@"""(.*)"" contains ""(.*)""")]
    public void ThenContains(string path, string text) => Assert.Contains(text, ws.Read(path));

    [Then(@"""(.*)"" does not contain ""(.*)""")]
    public void ThenDoesNotContain(string path, string text) => Assert.DoesNotContain(text, ws.Read(path));
}
