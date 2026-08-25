// The CLI surface: it wires Spectre.Console.Cli to `Commands` and finds the corpus each verb answers about.
// Every verb's logic is in kac.core, and `tooling/CLAUDE.md` says which file answers what.

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
    // with the commit. So the answer names the source it was built from as well as the release.
    config.SetApplicationVersion(Cli.Version);

    // Without this an option the verb does not declare is dropped in silence, and a mistyped flag
    // reads as a clean run.
    config.UseStrictParsing();

    config.AddCommand<NewCommand>("new")
        .WithDescription("Turn the folder you are in into a corpus.");
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
    config.AddCommand<UpdateCommand>("update")
        .WithDescription("Take a newer framework into this corpus, or adopt a type.");
});

// A bare `kac` names no verb, which is a usage error rather than a request for help. Spectre prints
// the help and exits 0; the exit code is what a script reads, so it stays 1.
if (args.Length == 0)
{
    app.Run(["--help"]);
    return 1;
}

// Spectre answers a parse failure with -1, which a shell reads as 255. No verb returns -1, so nothing
// else can be caught by this. `docs/cli/index.md` carries the exit codes.
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
    // The release the build stamped this assembly with, and the commit it came from. `--version` prints
    // it, and `new` holds itself against a template's `minimum-tool`.
    public static readonly string Version =
        typeof(Cli).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? "unknown";

    public static int InCorpus(KacSettings settings, Func<string, int> run)
    {
        if (settings.NoColor) Out.NoColor();

        var corpusRoot = Corpus.FindRoot(Directory.GetCurrentDirectory());
        if (corpusRoot is null)
        {
            Out.ErrMarkup("[red]kac: could not locate a corpus (no .corpus.yaml above the cwd).[/]");
            return 2;
        }

        // A corpus with no schema above it is a corpus this tool cannot judge, so it is declined here
        // rather than left to fail on the first file the loader opens.
        if (Schema.FindRoot(corpusRoot) is null)
        {
            Out.ErrMarkup($"[red]kac: {corpusRoot} has no .schema/ at or above it, so there is nothing to "
                          + "check the corpus against.[/]");
            return 1;
        }

        return run(corpusRoot);
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

// `new` is the one verb answering about a corpus that is not there yet, so it takes the working directory
// and never `Cli.InCorpus`. Finding a corpus is what stops it rather than what lets it run.
//
// Every answer has a flag, so nothing the command needs is reachable only by typing, and a run with no
// terminal exits rather than waiting. `docs/cli/new.md` carries the defaults and the order it asks in.
internal sealed class NewSettings : KacSettings
{
    [CommandOption("--name <NAME>")]
    [Description("What the corpus is called. Defaults to the name of this folder.")]
    public string? Name { get; init; }

    [CommandOption("--from <URL|PATH>")]
    [Description("The repository or folder serving the template. Defaults to the framework's own.")]
    public string From { get; init; } = Asking.DefaultFrom;

    [CommandOption("--ref <REF>")]
    [Description("The branch or tag to take the template from.")]
    public string? Ref { get; init; }

    [CommandOption("--path <PATH>")]
    [Description("The folder inside that repository holding manifest.yaml, where it is not at the root.")]
    public string? Path { get; init; }

    [CommandOption("--types <TYPES>")]
    [Description("The types to adopt, comma-separated, or 'all'.")]
    public string? Types { get; init; }

    [CommandOption("--publishing <TARGET>")]
    [Description("Where the corpus is published: github, azure-devops-wiki, mkdocs or none.")]
    public string? Publishing { get; init; }

    [CommandOption("--ci <SYSTEM>")]
    [Description("What builds the corpus: github, azure-devops or none.")]
    public string? Ci { get; init; }

    [CommandOption("--yes")]
    [Description("Take the default for every answer not given, and ask nothing.")]
    public bool Yes { get; init; }
}

internal sealed class NewCommand : Command<NewSettings>
{
    protected override int Execute(CommandContext context, NewSettings settings, CancellationToken token)
    {
        if (settings.NoColor) Out.NoColor();

        var request = new NewRequest
        {
            Name = settings.Name,
            Types = settings.Types,
            Publishing = settings.Publishing,
            Ci = settings.Ci,
            From = settings.From,
            Ref = settings.Ref,
            Path = settings.Path,
            Yes = settings.Yes
        };

        return Commands.New(Directory.GetCurrentDirectory(), request, Cli.Version,
            DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"));
    }
}

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
// narrows what is written and never what is read; `docs/cli/export.md` says why the corpus is
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

// `update` takes a newer framework into a corpus that already has one, and is where a corpus adopts a
// type or gives one up. Everything it writes stays in the working tree and nothing is committed, which is
// what lets it be liberal where a tool without that safety net would have to be timid.
//
// It asks nothing, because an update runs in a pipeline as often as it runs under somebody watching.
// `docs/cli/update.md` carries the defaults and what each layer means.
internal sealed class UpdateSettings : KacSettings
{
    [CommandOption("--from <URL|PATH>")]
    [Description("The repository or folder serving the template. Defaults to upstream.url in .corpus.yaml.")]
    public string? From { get; init; }

    [CommandOption("--ref <REF>")]
    [Description("The branch or tag to take the template from. Defaults to upstream.ref.")]
    public string? Ref { get; init; }

    [CommandOption("--path <PATH>")]
    [Description("The folder inside that repository holding manifest.yaml. Defaults to upstream.path.")]
    public string? Path { get; init; }

    [CommandOption("--check")]
    [Description("Report what would change and write nothing. Fails where anything would.")]
    public bool Check { get; init; }

    [CommandOption("--policy <POLICY>")]
    [Description("How far this run goes: cautious or full. Defaults to update-policy in .corpus.yaml.")]
    public string? Policy { get; init; }

    [CommandOption("--add-type <TYPE>")]
    [Description("Adopt a type the template declares, and write its schema, root page and template.")]
    public string? AddType { get; init; }

    [CommandOption("--drop-type <TYPE>")]
    [Description("Give up a type. Refused where its folder still holds records.")]
    public string? DropType { get; init; }

    [CommandOption("--yes")]
    [Description("Never wait on a credential prompt, for a run with nobody at the keyboard.")]
    public bool Yes { get; init; }
}

internal sealed class UpdateCommand : Command<UpdateSettings>
{
    protected override int Execute(CommandContext context, UpdateSettings settings, CancellationToken token) =>
        Cli.InCorpus(settings, corpus => Commands.Update(corpus, new UpdateRequest
        {
            From = settings.From,
            Ref = settings.Ref,
            Path = settings.Path,
            Check = settings.Check,
            Policy = settings.Policy,
            AddType = settings.AddType,
            DropType = settings.DropType,
            Yes = settings.Yes
        }, Cli.Version, DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd")));
}
