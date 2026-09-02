using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using kac.core;
using Xunit.Sdk;

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

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    // A command page opens `# `verb` what running it does`, and the overview's table is built from both halves.
    [GeneratedRegex(@"^# `(?<verb>[a-z]+)` (?<does>.+)$")]
    private static partial Regex PageHeading();

    // The first heading of a page, under which a missing marker pair is inserted.
    [GeneratedRegex(@"^# .*$", RegexOptions.Multiline)]
    private static partial Regex Heading();

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
            return new List<string>
            {
                Generator.Escape($"[`{page}`]({page}.md)"),
                Generator.Escape(char.ToUpperInvariant(does[0]) + does[1..] + ".")
            };
        }).ToList();

        return Generator.RenderTable(["Command", "What it does"], rows);
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

        var usage = $"```text\n{invocation}\n```";
        if (verb.Options.Count == 0) return usage;

        var rows = verb.Options
            .Select(o => new List<string>
            {
                Generator.Escape(o.Value is null ? $"`--{o.Long}`" : $"`--{o.Long} <{o.Value}>`"),
                Generator.Escape(o.Description)
            })
            .ToList();

        return $"{usage}\n\n{Generator.RenderTable(["Option", "What it does"], rows)}";
    }

    // The page carrying `body` in the block called `name`. `Generator.SpliceBlock` writes it, so a page of
    // the documentation and a page a corpus holds are filled by one piece of code.
    //
    // A page that has never carried the block is given an empty pair of markers under its first heading,
    // which is what a command page added to `docs/cli/` arrives as.
    //
    // A block that opens and never closes stops the run instead. `SpliceBlock` hands back a page it could
    // not splice untouched, and an untouched page is what the caller reads as up to date, so the block
    // would freeze where it stood. Writing a second pair above it is worse again: the orphan and the
    // content under it stay on the page, and `Generator.Authored` reads everything past an unmatched
    // marker as prose somebody wrote. Which of the two markers went is a question for whoever deleted one.
    internal static string Replaced(string page, string name, string body)
    {
        var begin = Generator.Begin(name);
        var end = Generator.End(name);
        var from = page.IndexOf(begin, StringComparison.Ordinal);

        if (from >= 0 && page.IndexOf(end, from, StringComparison.Ordinal) < 0)
            throw new XunitException(
                $"kac.tests: the block '{name}' opens and never closes. Put its '{end}' line back, or "
                + $"delete its '{begin}' line and let the block be written again.");

        if (from < 0)
        {
            var heading = Heading().Match(page);
            if (!heading.Success)
                throw new XunitException(
                    $"kac.tests: the page carrying '{name}' has no heading to put a generated block under.");

            var at = heading.Index + heading.Length;
            page = page[..at] + $"\n\n{begin}\n{end}" + page[at..];
        }

        return Generator.SpliceBlock(page, name, body);
    }

    private static IReadOnlyList<Verb> Read()
    {
        var xml = XDocument.Parse(XmlDoc());
        var root = xml.Root ?? throw new XunitException("The XML documentation for `kac` holds no root element.");

        return
        [
            .. root.Elements("Command")
                .Select(c => new Verb(
                    c.Value("Name"),
                    c.Element("Parameters")?.Elements("Option")
                        .Select(o => new Option(
                            o.Value("Long"),
                            o.Value("Value") is "NULL" ? null : o.Value("Value"),
                            o.Value("Required") is "true",
                            OneLine(o.Element("Description")?.Value)))
                        .OrderBy(o => o.Long, StringComparer.Ordinal).ToList()
                    ?? []))
        ];
    }

    // The value of an attribute the generated XML always carries. A null means that file's shape moved,
    // and naming the attribute says which one to go looking for.
    private static string Value(this XElement element, string attribute) =>
        element.Attribute(attribute)?.Value
        ?? throw new XunitException($"<{element.Name}> carries no '{attribute}' attribute.");

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
        }) ?? throw new XunitException("could not start kac to read its XML documentation.");

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
