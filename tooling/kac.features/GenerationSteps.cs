using System.Text.RegularExpressions;
using kac.core;
using Reqnroll;
using Xunit;

namespace kac.features;

[Binding]
public sealed partial class GenerationSteps(Workspace ws)
{
    [Given(@"the generated blocks in ""(.*)"" are emptied")]
    public void GivenTheGeneratedBlocksAreEmptied(string path)
    {
        var text = ws.Read(path);
        var emptied = Block().Replace(text, "$1\n$2");
        Assert.NotEqual(text, emptied);
        ws.Write(path, emptied);
    }

    [Given(@"the descriptor adopts ""(.*)"" as well")]
    public void GivenTheDescriptorAdoptsAsWell(string type)
    {
        var descriptor = ws.Read(".corpus.yaml");
        Assert.Contains("types:\n", descriptor);
        ws.Write(".corpus.yaml", descriptor.Replace("types:\n", $"types:\n  - {type}\n"));
    }

    [When("I generate")]
    public void WhenIGenerate() => ws.Run(root => Commands.Generate(root, check: false));

    [When("I check the generated files")]
    public void WhenICheckTheGeneratedFiles() => ws.Run(root => Commands.Generate(root, check: true));

    [Then("the generated files are fresh")]
    public void ThenTheGeneratedFilesAreFresh() => Assert.Equal(0, Commands.Generate(ws.Root, check: true));

    [GeneratedRegex(@"(<!-- BEGIN GENERATED: [^>]+ -->)[\s\S]*?(<!-- END GENERATED: [^>]+ -->)")]
    private static partial Regex Block();
}
