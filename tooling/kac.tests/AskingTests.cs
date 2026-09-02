// The console sits on the other side of `IAsker`, so every case here runs without a terminal.

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

        // What each confirm was offered as its bare-Enter answer. `fallback: false` is what makes Enter
        // mean "leave it alone" on a question standing in front of a deletion.
        public readonly List<bool> Fallbacks = [];

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

        public bool Confirm(string question, bool fallback = true)
        {
            Asked.Add(question);
            Fallbacks.Add(fallback);
            return confirm;
        }
    }

    private static Asking.Answered Resolve(NewRequest request, IAsker? asker = null, string? origin = null) =>
        Asking.Resolve(request, "folder-name", Declared, origin, asker);

    private static NewRequest Flagged() => new()
    {
        Name = "acme", Types = Asking.AllTypes, Publishing = Publishing.None, Ci = CiSystem.None
    };

    // Both ways of having nobody to ask, and the one way of having somebody. Read once per command, so
    // the two readings a command used to hold cannot disagree.
    [Theory]
    [InlineData(false, true, true)]    // a terminal, and nothing answered in advance
    [InlineData(true, true, false)]    // `--yes` answered everything, terminal or not
    [InlineData(false, false, false)]  // a pipeline, with nobody at a keyboard
    [InlineData(true, false, false)]
    public void Whether_there_is_anybody_to_ask(bool yes, bool interactive, bool asks)
        => Assert.Equal(asks, Asking.Asks(yes, interactive));

    [Fact]
    public void A_question_with_a_default_goes_ahead_where_there_is_nobody_to_ask()
        => Assert.Equal(Consent.Given, Asking.OrDefault(null, "Create it?"));

    [Fact]
    public void A_question_with_a_default_is_put_to_whoever_is_there()
    {
        var asker = new Scripted(confirm: false);

        Assert.Equal(Consent.Withheld, Asking.OrDefault(asker, "Create it?"));
        Assert.Equal(["Create it?"], asker.Asked);
    }

    [Fact]
    public void A_question_standing_in_front_of_a_deletion_stops_a_run_with_nobody_to_ask()
        => Assert.Equal(Consent.Unattended, Asking.OrRefuse(null, yes: false, "Give up glossary?"));

    // `--yes` is the caller answering in advance, which is the one thing that gets past this unattended.
    [Fact]
    public void A_question_standing_in_front_of_a_deletion_takes_yes_in_advance()
        => Assert.Equal(Consent.Given, Asking.OrRefuse(null, yes: true, "Give up glossary?"));

    [Fact]
    public void A_question_standing_in_front_of_a_deletion_is_put_to_whoever_is_there()
    {
        Assert.Equal(Consent.Withheld,
            Asking.OrRefuse(new Scripted(confirm: false), yes: false, "Give up glossary?"));
        Assert.Equal(Consent.Given,
            Asking.OrRefuse(new Scripted(confirm: true), yes: false, "Give up glossary?"));
    }

    // Each question is offered the answer a bare Enter gives, and the two kinds differ in it.
    [Fact]
    public void A_question_carries_the_answer_a_bare_Enter_gives()
    {
        var deletion = new Scripted();
        Asking.OrRefuse(deletion, yes: false, "Give up glossary?", fallback: false);
        Assert.Equal([false], deletion.Fallbacks);

        var asked = new Scripted();
        Asking.OrDefault(asked, "Create it?");
        Assert.Equal([true], asked.Fallbacks);
    }

    [Fact]
    public void A_flag_given_is_never_asked_for()
    {
        var asker = new Scripted();
        var answered = Resolve(Flagged(), asker);

        Assert.Equal("acme", answered.Resolved().Name);
        Assert.Empty(asker.Asked);
    }

    [Fact]
    public void An_answer_with_no_flag_is_asked_for()
    {
        var asker = new Scripted(text: "typed-in");
        var answered = Resolve(new NewRequest(), asker);

        Assert.Equal("typed-in", answered.Resolved().Name);
        Assert.Contains(asker.Asked, q => q.Contains("called"));
    }

    [Fact]
    public void Yes_takes_the_default_for_everything_unasked()
    {
        var answers = Resolve(new NewRequest { Yes = true }).Resolved();

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

    [Fact]
    public void All_names_every_type_the_template_declares()
        => Assert.Equal(Declared, Resolve(Flagged()).Resolved().Types);

    // Two corpora adopting the same types write the same descriptor.
    [Fact]
    public void A_named_subset_is_kept_in_the_order_the_schema_declares()
        => Assert.Equal(["adrs", "policies"],
            Resolve(Flagged() with { Types = "policies, adrs" }).Resolved().Types);

    [Fact]
    public void A_type_the_template_does_not_declare_is_named_back()
    {
        var problem = Resolve(Flagged() with { Types = "adrs,widgets" }).Problem;

        Assert.Contains("no type called 'widgets'", problem);
        Assert.Contains("adrs, glossary, policies", problem);
    }

    [Fact]
    public void A_corpus_may_adopt_no_type_at_all()
        => Assert.Empty(Resolve(new NewRequest(), new Scripted(many: [])).Resolved().Types);

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

    [Fact]
    public void A_corpus_publishing_nowhere_is_asked_for_no_base()
    {
        var asker = new Scripted();
        var answers = Resolve(Flagged(), asker).Resolved();

        Assert.Null(answers.Base);
        Assert.DoesNotContain(asker.Asked, q => q.Contains("published corpus is browsed"));
    }

    [Fact]
    public void The_base_is_filled_in_from_the_repositorys_own_remote()
    {
        var answers = Resolve(Flagged() with { Publishing = Publishing.GitHub, Yes = true },
            origin: "git@github.com:acme/corpus.git").Resolved();

        Assert.Equal("https://github.com/acme/corpus", answers.Base);
    }

    [Fact]
    public void A_repository_with_no_remote_states_no_base()
    {
        var answers = Resolve(Flagged() with { Publishing = Publishing.GitHub, Yes = true }).Resolved();

        Assert.Null(answers.Base);
    }
}
