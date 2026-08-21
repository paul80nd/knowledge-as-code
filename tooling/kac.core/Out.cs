using Spectre.Console;
using Spectre.Console.Rendering;

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

    private static IAnsiConsole Std => _std ??= Make(Console.Out, Console.IsOutputRedirected);
    private static IAnsiConsole Err => _err ??= Make(Console.Error, Console.IsErrorRedirected);

    // Plain text, written as given. Markup is not read here, so a message quoting '[ADR-0004]' or a
    // regex reaches the reader intact rather than throwing on an unbalanced tag.
    public static void Line(string text = "") => Std.WriteLine(text);

    public static void ErrLine(string text) => Err.WriteLine(text);

    // Markup the caller composed. Every value interpolated into one needs `EscapeMarkup()`, for the
    // reason above: the check messages are full of square brackets.
    public static void Markup(string markup) => Std.MarkupLine(markup);

    // A grid, or anything else Spectre lays out. It is rendered into a buffer first, because Spectre
    // pads every row out to the width of its widest column: the check catalogue rendered straight to
    // the stream is 78 lines each padded to the longest summary among them. That is invisible in a
    // terminal and a file full of ragged trailing spaces the moment anybody redirects it. The padding
    // is plain spaces written after the colour has been reset, so trimming the end of each line takes
    // the spaces and nothing else, whether or not colour is on.
    public static void Write(IRenderable renderable)
    {
        var buffer = new StringWriter();
        var capture = AnsiConsole.Create(new AnsiConsoleSettings { Out = new AnsiConsoleOutput(buffer) });
        capture.Profile.Width = Std.Profile.Width;
        capture.Profile.Capabilities.Ansi = Std.Profile.Capabilities.Ansi;
        capture.Profile.Capabilities.ColorSystem = Std.Profile.Capabilities.ColorSystem;
        capture.Profile.Capabilities.Unicode = Std.Profile.Capabilities.Unicode;
        capture.Write(renderable);

        // Straight to the writer, because what the buffer holds is already rendered — handing it back
        // to a console would read its escapes as text to be escaped again.
        var writer = Std.Profile.Out.Writer;
        foreach (var line in buffer.ToString().TrimEnd('\n').Split('\n'))
            writer.WriteLine(line.TrimEnd());
    }

    // Strip the colour from both consoles. `NO_COLOR` in the environment already does this — Spectre
    // reads it — so this is what `--no-color` carries, for a caller who cannot set a variable. Both
    // leave bold alone, so the two ways of asking come to the same output.
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
