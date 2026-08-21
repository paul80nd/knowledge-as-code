// kac — the knowledge-as-code validator and generator.
//
// One tool, several subcommands, sharing a schema-loading and markdown-parsing core:
//
//   validate   check the corpus against .schema/*.yaml
//   generate   regenerate _index.md and the generated blocks in <type>.md
//   export     write the corpus to .dist/export/ as data a consumer reads instead of cloning
//   bundle     assemble that export and the .plugin/ tree into an installable plugin
//   checks     list every check the validator implements
//   mechanism  enforce the portability manifest: check the shared layers against a
//              reference corpus, or sync them from one
//
// This file is only the CLI surface: it wires Spectre.Console.Cli to Commands and finds the corpus each
// verb answers about. Every subcommand's logic lives in the kac.core project this one references — one class per
// file, named for what it holds. Four carry the substance: Schema.cs loads .schema/*.yaml, Document.cs
// parses a record, Validator.cs holds the checks, Generator.cs builds the generated blocks.
//
// Spectre.Console.Cli is the parser because Spectre.Console is where a .NET tool goes for a prompt, and a
// tool that asks a question and parses a command line should carry one library rather than two.
//
// The tool is deliberately free of type-specific rules: everything it enforces is
// read from the YAML schema, so adding a type is adding a YAML file, not editing C#.
// See tooling/features/checks.md for what fails versus warns and how each check maps to the schema.

using System.ComponentModel;
using System.Reflection;
using kac.core;
using Spectre.Console.Cli;

var app = new CommandApp();
app.Configure(config =>
{
    // What the help text and every usage error call the tool. It is the packed `ToolCommandName`, which is
    // what a corpus types, and is stated here rather than left to be inferred from the assembly's name.
    config.SetApplicationName("kac");

    // `--version` exists only once a version is set, and the informational one is what the build stamps
    // with the commit — so the answer names the source it was built from as well as the release.
    config.SetApplicationVersion(
        typeof(Cli).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? "unknown");

    // Without this an option the verb does not declare is dropped in silence, and a mistyped flag
    // reads as a clean run.
    config.UseStrictParsing();

    config.AddCommand<ValidateCommand>("validate")
        .WithDescription("Check the corpus against .schema/*.yaml.");
    config.AddCommand<GenerateCommand>("generate")
        .WithDescription("Regenerate _index.md and the generated blocks in <type>.md.");
    config.AddCommand<ExportCommand>("export")
        .WithDescription("Write the corpus to .dist/export/ as a versioned export.");
    config.AddCommand<BundleCommand>("bundle")
        .WithDescription("Assemble the export and .plugin/ into a plugin under .dist/plugin/.");
    config.AddCommand<ChecksCommand>("checks")
        .WithDescription("List every check the validator implements.");
    config.AddCommand<MechanismCommand>("mechanism")
        .WithDescription(
            "Enforce the portability manifest: compare the shared layers against a reference, or take them from one.");
});

// A bare `kac` names no verb, which is a usage error rather than a request for help. Spectre prints
// the help and exits 0; the exit code is what a script reads, so it stays 1.
if (args.Length == 0)
{
    app.Run(["--help"]);
    return 1;
}

// Spectre answers a parse failure with -1, which a shell reads as 255. A usage error has always exited
// 1 here, beside a corpus error at 1 and no corpus at 2, and that is the table in tooling/README.md.
// No verb returns -1, so nothing else can be caught by this.
var exit = app.Run(args);
return exit == -1 ? 1 : exit;

// Run a verb against the corpus the working directory sits in, or decline having said why. Every
// verb needs one, and nothing else does: `--version` and `--help` are answered by the parser, from
// wherever the command was typed. Resolving inside a verb's Execute rather than before the parse is
// what lets an installed `kac` say which version it is without standing in a corpus first.
//
// Colour is settled here too, because this is the one step every verb takes before it writes anything.
internal static class Cli
{
    public static int InCorpus(KacSettings settings, Func<string, int> run)
    {
        if (settings.NoColor) Out.NoColor();

        var corpusRoot = FindCorpusRoot(Directory.GetCurrentDirectory());
        if (corpusRoot is null)
        {
            Out.ErrMarkup("[red]kac: could not locate a corpus (no .schema above the cwd).[/]");
            return 2;
        }

        return run(corpusRoot);
    }

    // The corpus this command runs against: the nearest folder above the working directory carrying a
    // `.schema/`. Every subcommand is answered from there, so where the tool's own files sit says nothing
    // about which corpus it reads.
    private static string? FindCorpusRoot(string start)
    {
        var dir = new DirectoryInfo(start);
        while (dir is not null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, ".schema")))
                return dir.FullName;
            dir = dir.Parent;
        }

        return null;
    }
}

// What every verb takes, whatever else it declares.
//
// `--no-color` sits here rather than on each verb because it is a fact about the terminal, not about
// the command. `NO_COLOR` in the environment asks for the same thing and is the cross-tool standard,
// which Spectre reads for itself. The flag is for a caller who cannot set a variable.
//
// It reaches a verb's own output and no further. The parser renders `--help` and `--version` before
// any verb runs, so `NO_COLOR` is what covers those. Either way colour goes and bold stays, which is
// what the standard asks for.
internal class KacSettings : CommandSettings
{
    [CommandOption("--no-color")]
    [Description("Turn colour off. NO_COLOR in the environment does the same.")]
    public bool NoColor { get; init; }
}

// A verb taking nothing but the corpus it stands in.
internal sealed class CorpusSettings : KacSettings;

// `validate` takes no paths: several of its checks ask about the shape of the corpus rather than
// about a document, and a subset cannot answer them.
internal sealed class ValidateSettings : KacSettings
{
    [CommandOption("--json")]
    [Description("Emit the summary and findings as JSON.")]
    public bool Json { get; init; }
}

internal sealed class ValidateCommand : Command<ValidateSettings>
{
    protected override int Execute(CommandContext context, ValidateSettings settings, CancellationToken token) =>
        Cli.InCorpus(settings, corpus => Commands.Validate(corpus, settings.Json));
}

internal sealed class GenerateSettings : KacSettings
{
    [CommandOption("--check")]
    [Description("Fail if a generated file is stale instead of writing it.")]
    public bool Check { get; init; }
}

internal sealed class GenerateCommand : Command<GenerateSettings>
{
    protected override int Execute(CommandContext context, GenerateSettings settings, CancellationToken token) =>
        Cli.InCorpus(settings, corpus => Commands.Generate(corpus, settings.Check));
}

// `export` writes the corpus to `.dist/export/` as data a consumer reads instead of cloning. `--type`
// narrows what is written and never what is read; `tooling/features/export.md` says why the corpus is
// loaded whole either way.
internal sealed class ExportSettings : KacSettings
{
    [CommandOption("--type <TYPE>")]
    [Description("Export one type rather than every type that contributes.")]
    public string? Type { get; init; }
}

internal sealed class ExportCommand : Command<ExportSettings>
{
    protected override int Execute(CommandContext context, ExportSettings settings, CancellationToken token) =>
        Cli.InCorpus(settings, corpus => Commands.Export(corpus, settings.Type));
}

// `bundle` assembles what `export` wrote, plus the `.plugin/` tree, into a plugin directory under
// `.dist/plugin/`. Two commands rather than one because they fail differently and are proved
// differently: an export is wrong about the corpus, and a bundle is wrong about what it shipped.
internal sealed class BundleCommand : Command<CorpusSettings>
{
    protected override int Execute(CommandContext context, CorpusSettings settings, CancellationToken token) =>
        Cli.InCorpus(settings, Commands.Bundle);
}

// `checks` is machinery before it is documentation: the test suite reads `checks --json` to assert
// every rule is exercised by a fixture, so a new rule cannot ship without a golden covering it.
internal sealed class ChecksSettings : KacSettings
{
    [CommandOption("--json")]
    [Description("Emit the check catalogue as JSON.")]
    public bool Json { get; init; }
}

internal sealed class ChecksCommand : Command<ChecksSettings>
{
    protected override int Execute(CommandContext context, ChecksSettings settings, CancellationToken token) =>
        Cli.InCorpus(settings, corpus => Commands.Checks(corpus, settings.Json));
}

// mechanism — enforce the portability manifest. `--check` compares this corpus's shared layers
// against a reference copy and reports drift, following the same discipline as `generate --check`:
// recompute, compare, name what is stale, exit non-zero, never write. `--sync` is the write half:
// it takes those layers from the reference, records what it took, and regenerates.
internal sealed class MechanismSettings : KacSettings
{
    [CommandOption("--check")]
    [Description("Compare the shared layers against a reference and report drift; never writes.")]
    public bool Check { get; init; }

    [CommandOption("--sync")]
    [Description("Take the shared layers from the reference, then record what it took in .corpus.yaml.")]
    public bool Sync { get; init; }

    [CommandOption("--against <PATH>")]
    [Description("Reference corpus (a path). Defaults to upstream.url in .corpus.yaml.")]
    public string? Against { get; init; }
}

internal sealed class MechanismCommand : Command<MechanismSettings>
{
    protected override int Execute(CommandContext context, MechanismSettings settings, CancellationToken token) =>
        Cli.InCorpus(settings, corpus => Commands.Mechanism(corpus, settings.Check, settings.Sync, settings.Against));
}
