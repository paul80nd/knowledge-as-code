using Spectre.Console;

namespace kac.core;

// The console half of `IAsker`: how a question is drawn and how a keystroke is read. It sits on this side
// of the boundary with `Commands` and `Out`, and nothing in the tool decides anything here.
//
// Every prompt is asked on `Out.Terminal`, so a question is coloured the way every other line is and
// `--no-color` reaches it.
public sealed class ConsoleAsker : IAsker
{
    public string Text(string question, string fallback)
    {
        var prompt = new TextPrompt<string>(question).AllowEmpty();
        if (fallback.Length > 0) prompt.DefaultValue(fallback);

        return Out.Terminal.Prompt(prompt);
    }

    public string Choose(string question, IReadOnlyList<string> options, string fallback) =>
        Out.Terminal.Prompt(
            new SelectionPrompt<string>()
                .Title(question)
                .AddChoices(Ordered(options, fallback)));

    // Everything ticked, because declining is the exception. `NotRequired` is what lets a corpus tick
    // nothing: adopting no type is a state the descriptor holds and validation acts on.
    public IReadOnlyList<string> ChooseMany(string question, IReadOnlyList<string> options)
    {
        var prompt = new MultiSelectionPrompt<string>().Title(question).NotRequired();
        foreach (var option in options) prompt.AddChoice(option).Select();

        return Out.Terminal.Prompt(prompt);
    }

    public bool Confirm(string question) => Out.Terminal.Prompt(new ConfirmationPrompt(question));

    // The default offered first, because a selection prompt opens on its first choice and the answer
    // most people want should be the one already under the cursor.
    private static IEnumerable<string> Ordered(IReadOnlyList<string> options, string fallback) =>
        options.Contains(fallback, StringComparer.Ordinal)
            ? [fallback, .. options.Where(o => !o.Equals(fallback, StringComparison.Ordinal))]
            : options;
}
