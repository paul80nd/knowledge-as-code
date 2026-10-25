// The clock, the environment, git and the network are read here and passed down, so a test calls a verb with a fixed
// day and a registry that answers from memory. `docs/maintaining/architecture.md` says which file answers what.

using System.ComponentModel;
using System.Reflection;
using System.Text;
using kac.core;
using Spectre.Console;
using Spectre.Console.Cli;

// Windows encodes stdout in the machine's install code page, which damages a clause citing `§9`; UTF-8 makes output
// agree with the corpus. The setter throws where the process has no console, and such a host takes the bytes as is.
if (OperatingSystem.IsWindows())
{
    try
    {
        Console.OutputEncoding = new UTF8Encoding(false);
    }
    catch (IOException)
    {
    }
}

var app = new CommandApp();
app.Configure(config =>
{
    config.SetApplicationName("kac");

    config.SetApplicationVersion(Cli.Version);

    // Without this an option the verb does not declare is dropped in silence, and a mistyped flag
    // reads as a clean run.
    config.UseStrictParsing();

    // Spectre writes an unhandled fault to stdout, so a caller redirecting a report into a file gets the
    // message inside the report. Every verb's own refusal goes to stderr through `Commands.Fail`, and
    // this joins them.
    config.SetExceptionHandler((ex, _) =>
    {
        Out.ErrMarkup($"[red]kac: {ex.Message.EscapeMarkup()}[/]");
        return 1;
    });

    config.AddCommand<NewCommand>("new")
        .WithDescription("Turn the folder you are in into a corpus.");
    config.AddCommand<ValidateCommand>("validate")
        .WithDescription("Check the corpus against .schema/*.yaml.");
    config.AddCommand<GenerateCommand>("generate")
        .WithDescription("Regenerate _index.md and the generated blocks in <type>.md.");
    config.AddCommand<RestoreCommand>("restore")
        .WithDescription("Fetch the corpora this one consumes into .imports/.");
    config.AddCommand<ExportCommand>("export")
        .WithDescription("Write the corpus to .dist/export/ as a versioned export.");
    config.AddCommand<BundleCommand>("bundle")
        .WithDescription("Assemble the export and .plugin/ into a plugin under .dist/plugin/.");
    config.AddCommand<PackCommand>("pack")
        .WithDescription("Seal the export into a versioned package under .dist/package/.");
    config.AddCommand<ChecksCommand>("checks")
        .WithDescription("List every check the validator implements.");
    config.AddCommand<ReportCommand>("report")
        .WithDescription("Print a report over this corpus and what it imports.");
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
// else can be caught by this. `docs/use/cli/index.md` carries the exit codes.
var exit = app.Run(args);
return exit == -1 ? 1 : exit;

/// <summary>
/// A verb finds its corpus inside its own Execute, not before the parse, so <c>--version</c> and <c>--help</c> answer
/// from any folder.
/// </summary>
internal static class Cli
{
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

        if (Schema.FindRoot(corpusRoot) is null)
        {
            Out.ErrMarkup($"[red]kac: {corpusRoot} has no .schema/ at or above it, so there is nothing to "
                          + "check the corpus against.[/]");
            return 1;
        }

        return run(corpusRoot);
    }

    /// <summary>The caller owns the client, because the timeout is the caller's to choose.</summary>
    public static Registry RegistryFor(string corpusRoot, HttpClient client) =>
        new(Registry.Over(client), Registry.OnDisk(corpusRoot));
}

internal class KacSettings : CommandSettings
{
    [CommandOption("--no-color")]
    [Description("Turn colour off. NO_COLOR in the environment does the same.")]
    public bool NoColor { get; init; }
}

internal sealed class CorpusSettings : KacSettings;

/// <summary>
/// <c>new</c> takes the working directory and never <c>Cli.InCorpus</c>: finding a corpus there is what stops it. Every
/// answer has a flag, so a run with no terminal exits rather than waits. <c>docs/use/cli/new.md</c> has the defaults.
/// </summary>
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
    [Description("Where the corpus is published: github, azure-devops, azure-devops-wiki, mkdocs or none.")]
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
    public override int Execute(CommandContext context, NewSettings settings, CancellationToken token)
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

/// <summary>
/// <c>validate</c> takes no paths: several of its checks ask about the shape of the corpus, and a subset cannot answer
/// them.
/// </summary>
internal sealed class ValidateSettings : KacSettings
{
    [CommandOption("--json")]
    [Description("Emit the summary and findings as JSON.")]
    public bool Json { get; init; }
}

internal sealed class ValidateCommand : Command<ValidateSettings>
{
    public override int Execute(CommandContext context, ValidateSettings settings, CancellationToken token) =>
        Cli.InCorpus(settings, corpus =>
        {
            // Shorter than the two minutes `restore` allows: this runs after every edit, and a source that never
            // answers costs each of its imports this wait before reporting `import-unreachable`.
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };

            var platform = Annotator.Detect(Environment.GetEnvironmentVariable);

            // One UTC day for the whole run, so every record is judged against the same day wherever it runs.
            return Commands.Validate(corpus, settings.Json, DateOnly.FromDateTime(DateTime.UtcNow),
                Cli.RegistryFor(corpus, client), Annotating(platform, corpus), Commenting(platform, client));
        });

    /// <summary>
    /// Null for a local run. A folder git cannot place takes an empty prefix, so its paths stay relative to the corpus.
    /// </summary>
    private static Annotator? Annotating(CiPlatform? platform, string corpus)
    {
        if (platform is not { } on) return null;

        var schemaRoot = Schema.FindRoot(corpus) ?? corpus;
        return new Annotator(on, Git.Prefix(corpus) ?? "", Git.Prefix(schemaRoot) ?? "");
    }

    /// <summary>
    /// Null for a run that is not for a pull request. Without a token the comment has nothing to send with, and says
    /// so when asked to post.
    /// </summary>
    private static PullRequestComment? Commenting(CiPlatform? platform, HttpClient client)
    {
        if (platform is not { } on) return null;

        var pullRequest = PullRequest.From(on, Environment.GetEnvironmentVariable, Payload);
        if (pullRequest is null) return null;

        var token = Environment.GetEnvironmentVariable(pullRequest.TokenVariable);
        return new PullRequestComment(pullRequest,
            token is { Length: > 0 } ? PullRequestComment.Over(client, on, token) : null);
    }

    /// <summary>The event payload, or null where it cannot be read, which makes the run no pull request's.</summary>
    private static string? Payload(string path)
    {
        try
        {
            return File.Exists(path) ? File.ReadAllText(path) : null;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }
}

internal sealed class GenerateSettings : KacSettings
{
    [CommandOption("--check")]
    [Description("Fail if a generated file is stale instead of writing it.")]
    public bool Check { get; init; }
}

internal sealed class GenerateCommand : Command<GenerateSettings>
{
    public override int Execute(CommandContext context, GenerateSettings settings, CancellationToken token) =>
        Cli.InCorpus(settings, corpus => Commands.Generate(corpus, settings.Check));
}

internal sealed class RestoreCommand : Command<CorpusSettings>
{
    public override int Execute(CommandContext context, CorpusSettings settings, CancellationToken token) =>
        Cli.InCorpus(settings, corpus =>
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
            return Commands.Restore(corpus, Cli.RegistryFor(corpus, client));
        });
}

/// <summary><c>--type</c> narrows what is written and never what is read. See <c>docs/use/cli/export.md</c>.</summary>
internal sealed class ExportSettings : KacSettings
{
    [CommandOption("--type <TYPE>")]
    [Description("Export one type rather than every type that contributes.")]
    public string? Type { get; init; }
}

internal sealed class ExportCommand : Command<ExportSettings>
{
    public override int Execute(CommandContext context, ExportSettings settings, CancellationToken token) =>
        Cli.InCorpus(settings, corpus =>
            Commands.Export(corpus, settings.Type, DateTime.UtcNow, Git.Head(corpus), Git.Dirty(corpus)));
}

internal sealed class BundleCommand : Command<CorpusSettings>
{
    public override int Execute(CommandContext context, CorpusSettings settings, CancellationToken token) =>
        Cli.InCorpus(settings, Commands.Bundle);
}

internal sealed class PackSettings : KacSettings
{
    [CommandOption("--repository <URL>")]
    [Description("Where the corpus's source lives. Some registries refuse a package naming none.")]
    public string? Repository { get; init; }
}

internal sealed class PackCommand : Command<PackSettings>
{
    public override int Execute(CommandContext context, PackSettings settings, CancellationToken token) =>
        Cli.InCorpus(settings, corpus => Commands.Pack(corpus, settings.Repository));
}

internal sealed class ChecksSettings : KacSettings
{
    [CommandOption("--json")]
    [Description("Emit the check catalogue as JSON.")]
    public bool Json { get; init; }
}

internal sealed class ChecksCommand : Command<ChecksSettings>
{
    public override int Execute(CommandContext context, ChecksSettings settings, CancellationToken token) =>
        Cli.InCorpus(settings, corpus => Commands.Checks(corpus, settings.Json));
}

internal sealed class ReportSettings : KacSettings
{
    [CommandArgument(0, "<NAME>")]
    [Description("Which report to print.")]
    public string Name { get; init; } = "";

    [CommandOption("--out <PATH>")]
    [Description("Write the report to this file instead of printing it. Refuses a path something holds.")]
    public string? OutPath { get; init; }
}

internal sealed class ReportCommand : Command<ReportSettings>
{
    public override int Execute(CommandContext context, ReportSettings settings, CancellationToken token) =>
        Cli.InCorpus(settings, corpus => Commands.Report(corpus, settings.Name, Cli.Version,
            DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), settings.OutPath));
}

/// <summary>
/// <c>update</c> asks nothing, because it runs in a pipeline as often as under somebody watching.
/// <c>docs/use/cli/update.md</c> has the defaults and what each layer means.
/// </summary>
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
    public override int Execute(CommandContext context, UpdateSettings settings, CancellationToken token) =>
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
