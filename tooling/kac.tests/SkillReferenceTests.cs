// The skills reference, held to the three files that decide which tree each skill lives in.
//
// A hand-written table drifts from what it describes inside two pull requests and nothing notices. Adding one skill
// to the plugin already made four pages wrong in a single change, and only a grep found them.
//
// `KAC_UPDATE_DOCS=1` rewrites the blocks instead of asserting on them, as it does for the CLI reference. Read the
// diff afterwards: the update blesses a regression as happily as a fix.

using kac.core;

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public class SkillReferenceTests
{
    private static bool Updating => Environment.GetEnvironmentVariable("KAC_UPDATE_DOCS") == "1";

    // Each table, and the block on the page that holds it.
    public static TheoryData<string> Blocks() => ["skills-in-the-plugin", "skills-in-a-corpus", "skills-here"];

    [Fact]
    public void Every_skill_the_repository_holds_appears_in_exactly_one_table()
    {
        var listed = SkillReference.InThePlugin()
            .Concat(SkillReference.InACorpus())
            .Concat(SkillReference.InThisRepository())
            .Select(skill => skill.Name)
            .ToList();

        var held = Directory.EnumerateDirectories(Path.Combine(Repo.Root, ".claude", "skills"))
            .Concat(Directory.EnumerateDirectories(Path.Combine(Repo.Root, "template", ".plugin", "skills")))
            .Select(Path.GetFileName)
            .ToList();

        Assert.Equal(held.Order(StringComparer.Ordinal), listed.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void A_skill_that_travels_is_not_also_listed_as_staying_here()
    {
        var travelling = SkillReference.InACorpus().Select(skill => skill.Name).ToHashSet(StringComparer.Ordinal);

        foreach (var skill in SkillReference.InThisRepository())
            Assert.DoesNotContain(skill.Name, travelling);
    }

    [Theory]
    [MemberData(nameof(Blocks))]
    public void The_page_states_the_tree_the_manifests_declare(string block)
    {
        var skills = block switch
        {
            "skills-in-the-plugin" => SkillReference.InThePlugin(),
            "skills-in-a-corpus" => SkillReference.InACorpus(),
            _ => SkillReference.InThisRepository(),
        };

        var page = Files.ReadLf(SkillReference.Page);
        var wanted = GeneratedPage.Replaced(page, block, SkillReference.Table(skills));

        if (page == wanted) return;

        if (Updating) File.WriteAllText(SkillReference.Page, wanted);

        Assert.True(Updating,
            $"the '{block}' table is stale in docs/skills.md. "
            + "Run the suite again with KAC_UPDATE_DOCS=1 and read the diff.");
    }
}
