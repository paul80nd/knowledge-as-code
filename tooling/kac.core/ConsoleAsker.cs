using Spectre.Console;

namespace kac.core;

/// <summary>Every prompt goes through <c>Out.Terminal</c>, so <c>--no-color</c> reaches it.</summary>
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

    /// <summary>
    /// <c>NotRequired</c> is what lets a corpus tick nothing. Without it Spectre refuses to return an empty selection,
    /// and adopting no type is a state the descriptor holds.
    /// </summary>
    public IReadOnlyList<string> ChooseMany(string question, IReadOnlyList<string> options)
    {
        var prompt = new MultiSelectionPrompt<string>().Title(question).NotRequired();
        foreach (var option in options) prompt.AddChoice(option).Select();

        return Out.Terminal.Prompt(prompt);
    }

    public bool Confirm(string question, bool fallback = true) =>
        Out.Terminal.Prompt(new ConfirmationPrompt(question) { DefaultValue = fallback });

    /// <summary>The default goes first, because a selection prompt opens on its first choice.</summary>
    private static IEnumerable<string> Ordered(IReadOnlyList<string> options, string fallback) =>
        options.Contains(fallback, StringComparer.Ordinal)
            ? [fallback, .. options.Where(o => !o.Equals(fallback, StringComparison.Ordinal))]
            : options;
}
