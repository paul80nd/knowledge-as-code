using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace kac.tests;

// The generated blocks of the CLI reference: a usage block at the head of each command's page, and the table of
// commands on the overview that indexes them.
//
// `Spectre.Console.Cli` carries a hidden `cli xmldoc` command that prints its command model, so a page shows the usage
// the tool accepts and not a second statement of it. The model is asked of the built `kac`, because one assembled here
// would prove the renderer and nothing else.
//
// Two properties of that output are worked around below. It is hard-wrapped to eighty columns even when redirected,
// which drops a newline where a description had a space, so every description is put back onto one line. And it names
// no global option, because `--help` and `--version` belong to the parser. The overview carries those.
internal static partial class CliReference
{
    // The folder holding one page per command, and the overview that indexes them.
    internal static readonly string Cli = Path.Combine(Repo.Root, "docs", "cli");
    internal static readonly string Index = Path.Combine(Cli, "index.md");

    private static readonly Lazy<IReadOnlyList<Verb>> Model = new(Read);

    internal static string BeginMarker(string block) => $"<!-- BEGIN GENERATED: {block} -->";
    internal static string EndMarker(string block) => $"<!-- END GENERATED: {block} -->";

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    // A command page opens `# `verb` what running it does`, and the overview's table is built from both halves.
    [GeneratedRegex(@"^# `(?<verb>[a-z]+)` (?<does>.+)$")]
    private static partial Regex PageHeading();

    // Every verb the parser declares, in the order it declares them. Read once, however many tests ask.
    internal static IReadOnlyList<Verb> Verbs() => Model.Value;

    // Every page of the reference, in the order the parser declares its commands.
    internal static IReadOnlyList<string> Pages() => [.. Verbs().Select(v => v.Name)];

    // The overview's index of the commands, one row each, taking what a command does from that command's own heading.
    // The heading is the one statement of it, and a second wording here would be the copy that goes stale.
    internal static string CommandTable()
    {
        var rows = Pages().Select(page =>
        {
            var heading = PageHeading().Match(File.ReadLines(Path.Combine(Cli, page + ".md")).First());
            if (!heading.Success)
                throw new InvalidOperationException(
                    $"kac.tests: docs/cli/{page}.md opens on no `# `verb` what running it does` heading.");

            var does = heading.Groups["does"].Value.TrimEnd('.');
            return (Command: Cell($"[`{page}`]({page}.md)"),
                Does: Cell(char.ToUpperInvariant(does[0]) + does[1..] + "."));
        }).ToList();

        var left = Math.Max("Command".Length, rows.Max(r => r.Command.Length));
        var right = Math.Max("What it does".Length, rows.Max(r => r.Does.Length));

        var table = new StringBuilder();
        table.Append($"| {"Command".PadRight(left)} | {"What it does".PadRight(right)} |\n");
        table.Append($"|{new string('-', left + 2)}|{new string('-', right + 2)}|\n");
        foreach (var row in rows)
            table.Append($"| {row.Command.PadRight(left)} | {row.Does.PadRight(right)} |\n");

        return table.ToString();
    }

    // The block body for one verb: the invocation it accepts, then a row per option.
    internal static string Render(Verb verb)
    {
        var invocation = new StringBuilder($"kac {verb.Name}");
        foreach (var option in verb.Options)
        {
            var flag = option.Value is null ? $"--{option.Long}" : $"--{option.Long} <{option.Value}>";
            invocation.Append(option.Required ? $" {flag}" : $" [{flag}]");
        }

        var block = new StringBuilder();
        block.Append("```text\n").Append(invocation).Append("\n```\n");

        if (verb.Options.Count == 0) return block.ToString();

        var rows = verb.Options
            .Select(o => (Option: Cell(o.Value is null ? $"`--{o.Long}`" : $"`--{o.Long} <{o.Value}>`"),
                Does: Cell(o.Description)))
            .ToList();

        // Fixed column widths, as the corpus generator writes them. A description that changes length then moves one
        // row and leaves the rest of the table alone.
        var left = Math.Max("Option".Length, rows.Max(r => r.Option.Length));
        var right = Math.Max("What it does".Length, rows.Max(r => r.Does.Length));

        block.Append('\n');
        block.Append($"| {"Option".PadRight(left)} | {"What it does".PadRight(right)} |\n");
        block.Append($"|{new string('-', left + 2)}|{new string('-', right + 2)}|\n");
        foreach (var row in rows)
            block.Append($"| {row.Option.PadRight(left)} | {row.Does.PadRight(right)} |\n");

        return block.ToString();
    }

    private static IReadOnlyList<Verb> Read()
    {
        var xml = XDocument.Parse(XmlDoc());

        return
        [
            .. xml.Root!.Elements("Command")
                .Select(c => new Verb(
                    c.Attribute("Name")!.Value,
                    c.Element("Parameters")?.Elements("Option")
                        .Select(o => new Option(
                            o.Attribute("Long")!.Value,
                            o.Attribute("Value")!.Value is "NULL" ? null : o.Attribute("Value")!.Value,
                            o.Attribute("Required")!.Value is "true",
                            OneLine(o.Element("Description")?.Value)))
                        .OrderBy(o => o.Long, StringComparer.Ordinal).ToList()
                    ?? []))
        ];
    }

    private static string Cell(string text) => text.Replace("|", "\\|", StringComparison.Ordinal);

    private static string OneLine(string? text) => Whitespace().Replace(text?.Trim() ?? string.Empty, " ");

    // The built `kac`. Its path is stamped into this assembly by kac.tests.csproj, and the project reference beside
    // that is what guarantees a build has produced one.
    private static string XmlDoc()
    {
        var kac = Repo.KacAssembly;
        if (!File.Exists(kac))
            throw new InvalidOperationException($"kac.tests: no built kac at '{kac}'. Build kac.slnx first.");

        var run = Process.Start(new ProcessStartInfo("dotnet", [kac, "cli", "xmldoc"])
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            // A runner naming itself in the environment turns colour back on, and escapes in the XML would render
            // into the page.
            Environment = { ["NO_COLOR"] = "1" },
        })!;

        var stdout = run.StandardOutput.ReadToEnd();
        var stderr = run.StandardError.ReadToEnd();
        run.WaitForExit();

        if (run.ExitCode != 0)
            throw new InvalidOperationException($"kac.tests: `cli xmldoc` exited {run.ExitCode}: {stderr}");

        return stdout;
    }

    internal sealed record Verb(string Name, IReadOnlyList<Option> Options);

    internal sealed record Option(string Long, string? Value, bool Required, string Description);
}
