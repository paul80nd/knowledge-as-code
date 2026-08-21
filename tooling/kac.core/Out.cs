using Spectre.Console;

// ---------------------------------------------------------------------------
// Out — where a verb's human-readable output goes.
// ---------------------------------------------------------------------------

namespace kac.core;

// The console every verb writes through. Two of them, because stdout carries what a command produced
// and stderr carries why it stopped, and a caller redirecting one must still see the other.
//
// Machine-readable output does not come through here. `--json` is a contract a pipeline parses, so it
// is written straight to the stream and never through a renderer that could wrap or restyle it.
public static class Out
{
    // A redirected stream has no width to report, and Spectre answers 80. That is a guess about a
    // terminal nobody is looking at: it wraps a finding message mid-word and squeezes a check id in
    // half. Answering with a width no line reaches leaves redirected output as one line per fact,
    // which is what a golden captures and what `grep` expects. A real terminal reports its own.
    private const int RedirectedWidth = 1000;

    private static IAnsiConsole? _std;
    private static IAnsiConsole? _err;

    public static IAnsiConsole Std => _std ??= Make(Console.Out, Console.IsOutputRedirected);
    public static IAnsiConsole Err => _err ??= Make(Console.Error, Console.IsErrorRedirected);

    // Plain text, written as given. Markup is not read here, so a message quoting '[ADR-0004]' or a
    // regex reaches the reader intact rather than throwing on an unbalanced tag.
    public static void Line(string text = "") => Std.WriteLine(text);

    public static void ErrLine(string text = "") => Err.WriteLine(text);

    // Markup the caller composed. Every value interpolated into one needs `EscapeMarkup()`, for the
    // reason above: the check messages are full of square brackets.
    public static void Markup(string markup) => Std.MarkupLine(markup);

    public static void ErrMarkup(string markup) => Err.MarkupLine(markup);

    public static void Write(Spectre.Console.Rendering.IRenderable renderable) => Std.Write(renderable);

    public static void ErrWrite(Spectre.Console.Rendering.IRenderable renderable) => Err.Write(renderable);

    // Strip the colour from both consoles. `NO_COLOR` in the environment already does this — Spectre
    // reads it — so this is what `--no-color` carries, for a caller who cannot set a variable.
    public static void NoColor()
    {
        Std.Profile.Capabilities.ColorSystem = ColorSystem.NoColors;
        Err.Profile.Capabilities.ColorSystem = ColorSystem.NoColors;
    }

    private static IAnsiConsole Make(TextWriter writer, bool redirected)
    {
        var console = AnsiConsole.Create(new AnsiConsoleSettings { Out = new AnsiConsoleOutput(writer) });
        if (redirected) console.Profile.Width = RedirectedWidth;
        return console;
    }
}
