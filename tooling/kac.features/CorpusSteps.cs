using System.Text.Json.Nodes;
using Reqnroll;
using Xunit;

namespace kac.features;

/// <summary>
/// The steps every command's scenarios share: the corpus they start from, and what a run left on disk.
/// </summary>
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

    /// <summary>
    /// <paramref name="key"/> is a dotted path through nested objects, and <paramref name="value"/> is JSON.
    /// </summary>
    [Given(@"""(.*)"" states ""(.*)"" as (.*)")]
    public void GivenStatesAs(string path, string key, string value)
    {
        var json = ws.ReadJson(path);
        var (parent, last) = Walk(json, key);
        parent[last] = JsonNode.Parse(value);
        ws.WriteJson(path, json);
    }

    [Given(@"""(.*)"" states no ""(.*)""")]
    public void GivenStatesNo(string path, string key)
    {
        var json = ws.ReadJson(path);
        var (parent, last) = Walk(json, key);
        Assert.True(parent.Remove(last), $"{path} states no '{key}' to remove");
        ws.WriteJson(path, json);
    }

    [Given(@"""(.*)"" is left from an earlier run")]
    public void GivenIsLeftFromAnEarlierRun(string path) => ws.Write(path, "{}\n");

    [Given(@"""(.*)"" ends with ""(.*)""")]
    public void GivenEndsWith(string path, string line) => ws.Write(path, ws.Read(path) + "\n" + line + "\n");

    [Given(@"""(.*)"" is replaced with ""(.*)""")]
    public void GivenIsReplacedWith(string path, string content) => ws.Write(path, content);

    [Then("it refuses")]
    public void ThenItRefuses() => Assert.Equal(1, ws.Exit);

    /// <summary>
    /// A check that ran and found a fault, where <c>it refuses</c> is a command that stopped before acting.
    /// </summary>
    [Then("it fails")]
    public void ThenItFails() => Assert.Equal(1, ws.Exit);

    [Then("it succeeds")]
    public void ThenItSucceeds() => Assert.Equal(0, ws.Exit);

    [Then("nothing is written")]
    public void ThenNothingIsWritten() => Assert.Empty(ws.Changed());

    [Then(@"^nothing outside \.dist/ changed$")]
    public void ThenNothingOutsideDistChanged()
        => Assert.DoesNotContain(ws.Changed(), p => !p.StartsWith(".dist/", StringComparison.Ordinal));

    [Then(@"nothing under ""(.*)"" changed")]
    public void ThenNothingUnderChanged(string folder)
        => Assert.DoesNotContain(ws.Changed(), p => p.StartsWith(folder + "/", StringComparison.Ordinal));

    [Then(@"only ""(.*)"" changed")]
    public void ThenOnlyChanged(string path) => Assert.Equal([path], ws.Changed());

    [Then(@"""(.*)"" exists")]
    public void ThenExists(string path) => Assert.True(File.Exists(ws.PathOf(path)), $"no {path}");

    [Then(@"""(.*)"" does not exist")]
    public void ThenDoesNotExist(string path)
        => Assert.False(File.Exists(ws.PathOf(path)) || Directory.Exists(ws.PathOf(path)), $"{path} is there");

    [Then(@"""(.*)"" contains ""(.*)""")]
    public void ThenContains(string path, string text) => Assert.Contains(text, ws.Read(path));

    [Then(@"""(.*)"" states ""(.*)"" as (.*)")]
    public void ThenStatesAs(string path, string key, string value)
    {
        var (parent, last) = Walk(ws.ReadJson(path), key);
        var actual = parent[last];
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(value), actual),
            $"{path} states '{key}' as {actual?.ToJsonString() ?? "null"}");
    }

    [Then(@"""(.*)"" states no ""(.*)""")]
    public void ThenStatesNo(string path, string key)
    {
        var (parent, last) = Walk(ws.ReadJson(path), key);
        Assert.False(parent.ContainsKey(last), $"{path} states '{key}'");
    }

    /// <summary>Compared as bytes, so a copy that normalised line endings fails.</summary>
    [Then(@"""(.*)"" is a copy of ""(.*)""")]
    public void ThenIsACopyOf(string copy, string original)
    {
        var want = ws.Snapshot(original);
        var got = ws.Snapshot(copy);
        Assert.NotEmpty(want);
        Assert.Equal(want.Keys, got.Keys);
        Assert.All(want, kv => Assert.True(kv.Value.AsSpan().SequenceEqual(got[kv.Key]), $"{copy}/{kv.Key} differs"));
    }

    [Then(@"""(.*)"" does not contain ""(.*)""")]
    public void ThenDoesNotContain(string path, string text) => Assert.DoesNotContain(text, ws.Read(path));

    private static (JsonObject Parent, string Last) Walk(JsonObject json, string key)
    {
        var steps = key.Split('.');
        var parent = json;
        foreach (var step in steps[..^1])
            parent = parent[step] as JsonObject
                     ?? throw new InvalidOperationException($"no object at '{step}' in '{key}'");
        return (parent, steps[^1]);
    }
}
