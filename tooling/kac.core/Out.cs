using Spectre.Console;
using Spectre.Console.Rendering;

namespace kac.core;

/// <summary>
/// Stdout carries what a command produced and stderr why it stopped. <c>--json</c> does not come through here: it goes
/// straight to the stream, so no renderer can wrap it.
/// </summary>
public static class Out
{
    /// <summary>
    /// A redirected stream has no width, and Spectre's guess of 80 wraps a message mid-word. A width no line reaches
    /// keeps redirected output to one line per fact, which a golden captures and <c>grep</c> expects.
    /// </summary>
    private const int RedirectedWidth = 1000;

    private static IAnsiConsole? s_std;
    private static IAnsiConsole? s_err;

    private static IAnsiConsole Std => s_std ??= Make(Console.Out, Console.IsOutputRedirected);
    private static IAnsiConsole Err => s_err ??= Make(Console.Error, Console.IsErrorRedirected);

    /// <summary>The same console as every other line, so <c>--no-color</c> reaches a prompt.</summary>
    public static IAnsiConsole Terminal => Std;

    /// <summary>
    /// Whether there is somebody to ask. Spectre's own <c>Interactive</c> answers for the terminal it would draw on and
    /// not for the keyboard, so a redirected stdin passes it and reads back nothing.
    /// </summary>
    public static bool Interactive =>
        Std.Profile.Capabilities.Interactive && !System.Console.IsInputRedirected;

    /// <summary>Markup is not read, so a message quoting '[ADR-0004]' or a regex reaches the reader intact.</summary>
    public static void Line(string text = "") => Std.WriteLine(text);

    public static void ErrLine(string text) => Err.WriteLine(text);

    /// <summary>
    /// Every interpolated value needs <c>EscapeMarkup()</c>: the check messages are full of square brackets.
    /// </summary>
    public static void Markup(string markup) => Std.MarkupLine(markup);

    public static void ErrMarkup(string markup) => Err.MarkupLine(markup);

    /// <summary>
    /// A grid, or anything else Spectre lays out. Buffered first, because Spectre pads every row to its widest column,
    /// and trimmed per line: the padding is plain spaces written after the colour reset, so the trim takes only it.
    /// </summary>
    public static void Write(IRenderable renderable)
    {
        var buffer = new StringWriter();
        var capture = AnsiConsole.Create(new AnsiConsoleSettings { Out = new AnsiConsoleOutput(buffer) });
        capture.Profile.Width = Std.Profile.Width;
        capture.Profile.Capabilities.Ansi = Std.Profile.Capabilities.Ansi;
        capture.Profile.Capabilities.ColorSystem = Std.Profile.Capabilities.ColorSystem;
        capture.Profile.Capabilities.Unicode = Std.Profile.Capabilities.Unicode;
        capture.Write(renderable);

        // Straight to the writer, because what the buffer holds is already rendered. Handing it back
        // to a console would read its escapes as text to be escaped again.
        var writer = Std.Profile.Out.Writer;
        foreach (var line in buffer.ToString().TrimEnd('\n').Split('\n'))
            writer.WriteLine(line.TrimEnd());
    }

    /// <summary>Spectre already honours <c>NO_COLOR</c>, so this serves <c>--no-color</c>.</summary>
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
