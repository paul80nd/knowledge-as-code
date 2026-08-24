// Unit tests for how `new` settles its answers: what a flag decides, what a person is asked, what
// `--yes` defaults to, and what a run with nobody to ask refuses. The console is on the other side of
// `IAsker`, so every one of these runs without a terminal.

using kac.core;

namespace kac.tests;

public class AskingTests
{
    private static readonly IReadOnlyList<string> Declared = ["adrs", "glossary", "policies"];

    // An asker that answers with whatever it was built with, and records what it was asked.
    private sealed class Scripted(
        string? text = null,
        string? choice = null,
        IReadOnlyList<string>? many = null,
        bool confirm = true) : IAsker
    {
        public readonly List<string> Asked = [];

        public string Text(string question, string fallback)
        {
            Asked.Add(question);
            return text ?? fallback;
        }

        public string Choose(string question, IReadOnlyList<string> options, string fallback)
        {
            Asked.Add(question);
            return choice ?? fallback;
        }

        public IReadOnlyList<string> ChooseMany(string question, IReadOnlyList<string> options)
        {
            Asked.Add(question);
            return many ?? options;
        }

        public bool Confirm(string question)
        {
            Asked.Add(question);
            return confirm;
        }
    }

    private static Asking.Answered Resolve(NewRequest request, IAsker? asker = null, string? origin = null) =>
        Asking.Resolve(request, "folder-name", Declared, origin, asker);

    private static NewRequest Flagged() => new()
    {
        Name = "acme", Types = Asking.AllTypes, Publishing = Publishing.None, Ci = CiSystem.None
    };

    // -- what settles an answer --

    [Fact]
    public void A_flag_given_is_never_asked_for()
    {
        var asker = new Scripted();
        var answered = Resolve(Flagged(), asker);

        Assert.Equal("acme", answered.Answers!.Name);
        Assert.Empty(asker.Asked);
    }

    [Fact]
    public void An_answer_with_no_flag_is_asked_for()
    {
        var asker = new Scripted(text: "typed-in");
        var answered = Resolve(new NewRequest(), asker);

        Assert.Equal("typed-in", answered.Answers!.Name);
        Assert.Contains(asker.Asked, q => q.Contains("called"));
    }

    [Fact]
    public void Yes_takes_the_default_for_everything_unasked()
    {
        var answers = Resolve(new NewRequest { Yes = true }).Answers!;

        Assert.Equal("folder-name", answers.Name);
        Assert.Equal(Declared, answers.Types);
        Assert.Equal(Publishing.None, answers.PublishingTarget);
        Assert.Equal(CiSystem.None, answers.Ci);
    }

    [Fact]
    public void A_run_with_nobody_to_ask_names_every_flag_it_needed()
    {
        var problem = Resolve(new NewRequest { Name = "acme" }).Problem;

        Assert.Contains("no terminal to ask on", problem);
        Assert.Contains("--types, --publishing, --ci", problem);
        Assert.DoesNotContain("--name", problem);
    }

    [Fact]
    public void A_run_given_every_flag_needs_no_terminal()
        => Assert.Null(Resolve(Flagged()).Problem);

    [Fact]
    public void A_corpus_needs_a_name()
        => Assert.Contains("a corpus needs a name", Resolve(Flagged() with { Name = "  " }).Problem);

    // -- types --

    [Fact]
    public void All_names_every_type_the_template_declares()
        => Assert.Equal(Declared, Resolve(Flagged()).Answers!.Types);

    // Kept in the order the schema declares them, whatever order the flag named them in, so two corpora
    // adopting the same types write the same descriptor.
    [Fact]
    public void A_named_subset_is_kept_in_the_order_the_schema_declares()
        => Assert.Equal(["adrs", "policies"],
            Resolve(Flagged() with { Types = "policies, adrs" }).Answers!.Types);

    [Fact]
    public void A_type_the_template_does_not_declare_is_named_back()
    {
        var problem = Resolve(Flagged() with { Types = "adrs,widgets" }).Problem;

        Assert.Contains("no type called 'widgets'", problem);
        Assert.Contains("adrs, glossary, policies", problem);
    }

    [Fact]
    public void A_corpus_may_adopt_no_type_at_all()
        => Assert.Empty(Resolve(new NewRequest(), new Scripted(many: [])).Answers!.Types);

    // -- the two vocabularies --

    [Fact]
    public void A_publishing_target_the_tool_cannot_act_on_is_refused()
    {
        var problem = Resolve(Flagged() with { Publishing = "confluence" }).Problem;

        Assert.Contains("'confluence' is not a target", problem);
        Assert.Contains("azure-devops-wiki, github, mkdocs, none", problem);
    }

    [Fact]
    public void A_ci_system_the_tool_does_not_offer_is_refused()
        => Assert.Contains("'jenkins' is not a system", Resolve(Flagged() with { Ci = "jenkins" }).Problem);

    // -- the publishing bases --

    [Fact]
    public void A_corpus_publishing_nowhere_is_asked_for_no_bases()
    {
        var asker = new Scripted();
        var answers = Resolve(Flagged(), asker).Answers!;

        Assert.Null(answers.HumanBase);
        Assert.Null(answers.RawBase);
        Assert.DoesNotContain(asker.Asked, q => q.Contains("read a record"));
    }

    [Fact]
    public void The_bases_are_filled_in_from_the_repositorys_own_remote()
    {
        var answers = Resolve(Flagged() with { Publishing = Publishing.GitHub, Yes = true },
            origin: "git@github.com:acme/corpus.git").Answers!;

        Assert.Equal("https://github.com/acme/corpus/blob", answers.HumanBase);
        Assert.Equal("https://raw.githubusercontent.com/acme/corpus", answers.RawBase);
    }

    [Fact]
    public void A_repository_with_no_remote_states_no_bases()
    {
        var answers = Resolve(Flagged() with { Publishing = Publishing.GitHub, Yes = true }).Answers!;

        Assert.Null(answers.HumanBase);
        Assert.Null(answers.RawBase);
    }
}
