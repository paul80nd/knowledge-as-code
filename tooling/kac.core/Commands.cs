using System.Text.Json;
using Spectre.Console;

// The orchestration behind each CLI verb. tooling/kac/Program.cs only wires Spectre.Console.Cli to these.

namespace kac.core;

public static class Commands
{
    /// <summary>
    /// Check the corpus and print the findings. <paramref name="today"/> is the day every record is judged
    /// against, and <paramref name="registry"/> is where each import's source is read from. Both come from
    /// the caller so a test can fix the day and answer for the registry without a network.
    /// </summary>
    public static int Validate(string corpusRoot, bool json, DateOnly today, Registry registry)
    {
        var corpus = Corpus.Load(corpusRoot);
        var findings = Validator.CheckAll(corpus, today, Standings(corpus.Descriptor, registry));
        return Report(findings, corpus.Docs.Count, corpus.Templates.Count, corpus.SkippedNoFrontmatter, json);
    }

    /// <summary>
    /// Null for a corpus consuming nothing, so no source is read. The one place <c>validate</c> leaves the working
    /// tree: an unreachable registry reports <c>import-unreachable</c> rather than the lock as current.
    /// </summary>
    private static IReadOnlyList<ImportStanding>? Standings(CorpusDescriptor descriptor, Registry registry) =>
        descriptor.Consumes.Count == 0 ? null : Freshness.Read(descriptor.Consumes, registry);

    /// <summary>
    /// Write the corpus to <c>.dist/export/</c>. <paramref name="now"/>, <paramref name="commit"/> and
    /// <paramref name="dirty"/> are stamped into the manifest as given. <paramref name="commit"/> is null where
    /// git could not answer, and <paramref name="dirty"/> is null where the tree could not be asked, which the
    /// manifest records as unknown rather than clean.
    /// </summary>
    public static int Export(string corpusRoot, string? type, DateTime now, string? commit, bool? dirty)
    {
        var corpus = Corpus.Load(corpusRoot);
        var (inherited, missing) = Inherited.Read(corpusRoot, corpus.Descriptor.Consumes);
        var publishing = Publishing.For(corpus.Descriptor, commit);

        var plan = Exporter.Plan(corpus, publishing, type,
            new ExportRun(now.ToString("yyyy-MM-ddTHH:mm:ssZ"), DateOnly.FromDateTime(now), commit, dirty),
            inherited, missing);

        if (plan.Refused.Count > 0)
        {
            foreach (var reason in plan.Refused) Stop($"export: {reason}");
            return 1;
        }

        var written = Exporter.Write(corpusRoot, plan);
        foreach (var path in written) Out.Markup(Wrote(path));

        if (publishing is null)
            Note(
                "export: no published links. This corpus states no publishing target the tool can address, "
                + "or no base the target can join to. Records carry their paths and no URLs.");
        if (dirty is true)
            Note(
                "export: built from a dirty working tree, and the manifest says so. The commit it names "
                + "does not reproduce it.");

        // Named rather than counted: a record left behind is invisible in the output.
        if (plan.Withheld.Count > 0)
            Note(
                $"export: withheld {string.Join(", ", plan.Withheld)}: .corpus.yaml excludes "
                + $"{string.Join(" and ", corpus.Descriptor.ExportExclude)}.");

        if (plan.Unread.Count > 0)
        {
            Note(
                $"export: {plan.Unread.Count} cross-reference(s) name a record and no part inside it, so "
                + "nothing was carried for them:");
            foreach (var u in plan.Unread) Out.Line($"  {u}");
            Note("export: point each link at the part it means, as '<file>.md#<anchor>'.");
        }

        if (inherited.Count > 0)
            Note($"export: carried {string.Join(", ", inherited.Select(c => $"{c.Corpus ?? c.Shortcode} "
                                                                            + $"{c.ContentVersion}"))}, which "
                 + "this corpus consumes. Their records travel merged with its own.");

        Account(plan.Types.Count == 0
            ? $"export: wrote {written.Count} file(s); no type contributed a record."
            : $"export: wrote {written.Count} file(s) for {string.Join(", ", plan.Types.Select(t => t.Type))}.");
        return 0;
    }

    /// <summary>
    /// Takes the working directory rather than a corpus root, because the corpus is not there yet. Everything that can
    /// fail is settled before the first question. <c>docs/use/cli/new.md</c> argues each step. <c>today</c> is passed
    /// in so a golden can pin what the descriptor says.
    /// </summary>
    public static int New(string dir, NewRequest request, string toolVersion, string today)
    {
        var ground = kac.core.New.Survey(dir);

        if (ground.Corpus is { } already)
            return Fail($"new: {already} is already a corpus, so there is nothing here to create. "
                        + "Taking a newer framework into one is `kac update`.");

        // An unknown tree goes on, as a clean one does.
        if (ground.Status is WorkingTree.Dirty)
            return Fail("new: this repository holds uncommitted changes. Commit or stash them first, so "
                        + "that what `new` writes reads as a diff of its own.");

        var asker = Asking.Asks(request.Yes, Out.Interactive) ? new ConsoleAsker() : null;

        if (!ground.Repository && Initialise(dir, request, asker) is { } refused) return refused;

        if (ground.Holds.Count > 0)
        {
            Note($"new: this folder already holds {string.Join(", ", ground.Holds)}. Without a committed "
                 + "baseline there is nothing to tell those from the files about to arrive.");
            if (Asking.OrDefault(asker, "Create the corpus here anyway?") is Consent.Withheld)
                return Cancelled("new");
        }

        using var take = TemplateSource.Take("new", request.From, request.From, request.Ref, request.Path,
            Path.GetTempPath(), prompt: asker is not null, toolVersion);
        if (take.Problem is { } unreadable) return Fail(unreadable);

        var answered = Asking.Resolve(request, new DirectoryInfo(dir).Name, take.Declared,
            Git.Run(dir, "remote", "get-url", "origin")?.Trim(), asker);
        if (answered.Failed) return Fail(answered.Problem);
        var answers = answered.Answers;

        var upstream = new Upstream(request.From, request.Path, request.Ref, take.Commit,
            take.Manifest.Version, today);

        Summarise(dir, answers, upstream, take.Declared.Count);
        if (Asking.OrDefault(asker, "Create it?") is Consent.Withheld) return Cancelled("new");

        var plan = kac.core.New.Plan(take.Files(), take.Manifest, answers, upstream,
            kac.core.New.DeclinesTypes(take.Schema, answers.Types));
        if (plan.Faults.Unsound) return StopUnsound("new", request.From, plan.Faults);

        var declined = SeedLinks.Declined(take.Schema, answers.Types);
        var (written, unlinked) = kac.core.New.Apply(plan, take.Root, dir, declined);
        foreach (var path in written) Out.Markup(Wrote(path));

        if (plan.DeclinedTypes.Count > 0)
            Account($"new: {plan.DeclinedTypes.Count} file(s) withheld for the types this corpus declined.");
        if (plan.DeclinedCi.Count > 0)
            Account($"new: did not write {string.Join(", ", plan.DeclinedCi)}: this corpus is built by "
                    + $"{answers.Ci}.");

        Account($"new: wrote {plan.Copied.Count + plan.Composed.Count} file(s) for {answers.Name}, taken "
                + $"from {request.From}{At(take.Commit)}.");
        Unlinked("new", unlinked);

        // Generation writes what validation then checks, so it goes first. Staging goes last.
        Generate(dir, check: false);
        var validated = Validate(dir, json: false, DateOnly.ParseExact(today, "yyyy-MM-dd"), Unreachable);
        Stage(dir);

        if (validated == 0) return 0;

        Stop("new: the corpus this created does not validate. That is a defect in the template or in the "
             + "tool, and not in anything you answered. The files are written and staged.");
        return 1;
    }

    /// <summary>
    /// The template declares no <c>consumes:</c>. Should one appear, every import reports <c>import-unreachable</c>
    /// rather than <c>new</c> fetching from a network nobody asked it to use.
    /// </summary>
    private static readonly Registry Unreachable = new(
        _ => new Fetched(null, 0, "new does not read a registry."),
        new FolderFeed(_ => null, _ => null));

    /// <summary>
    /// Discovery reads the git listing, so the choice is between running <c>git init</c> and cancelling.
    /// </summary>
    private static int? Initialise(string dir, NewRequest request, IAsker? asker) =>
        Asking.OrRefuse(asker, request.Yes, $"{dir} is not a git repository. Run `git init` here?") switch
        {
            Consent.Unattended => Fail("new: this folder is not a git repository, and there is no terminal to ask on. "
                                       + "Run `git init` first, or pass --yes to have it run."),
            Consent.Withheld => Cancelled("new"),
            _ => Git.Run(dir, "init", "-q") is null
                ? Fail("new: `git init` failed. The tool reads the git listing to find what a corpus holds, "
                       + "so a corpus git cannot see is not one worth writing.")
                : null
        };

    /// <summary>
    /// Asked rather than refused: the pages naming the type are the corpus's own words, and only the person running
    /// this knows what else points at it. A no ends the whole run, not the drop alone.
    /// </summary>
    private static int? Relinquish(string dropping, UpdateRequest request)
    {
        var asker = Asking.Asks(request.Yes, Out.Interactive) ? new ConsoleAsker() : null;

        Note($"update: giving up {dropping} deletes its page. Every page still linking to it is left "
             + "holding a dead link, and `kac validate` reports the ones it can reach.");
        Note("update: a reference it cannot parse is reported by nobody. Search the corpus for the name "
             + "as well, and fix what you find.");

        return Asking.OrRefuse(asker, request.Yes, $"Give up {dropping}?", fallback: false) switch
        {
            Consent.Unattended => Fail($"update: giving up {dropping} needs an answer, and there is no "
                                       + "terminal to ask on. Pass --yes to give it up anyway."),
            Consent.Withheld => Cancelled("update"),
            _ => null
        };
    }

    private static void Summarise(string dir, NewAnswers answers, Upstream upstream, int declared)
    {
        Out.Markup($"[grey]new:[/] creating [bold]{answers.Name.EscapeMarkup()}[/] in {dir.EscapeMarkup()}");

        var grid = Rows();
        Row(grid, "types", answers.Types.Count == declared
            ? $"all {declared}"
            : $"{string.Join(", ", answers.Types)} ({answers.Types.Count} of {declared})");
        Row(grid, "publishing", answers.PublishingTarget);
        Row(grid, "built by", answers.Ci);
        Row(grid, "template", upstream.Url + Ref(upstream.Ref) + At(upstream.Commit));
        Out.Write(grid);

        static void Row(Grid g, string label, string value) =>
            g.AddRow($"  [grey]{label}[/]", value.EscapeMarkup());

        static string Ref(string? name) => name is { Length: > 0 } r ? $" at {r}" : "";
    }

    /// <summary>
    /// A failure here is a note and not a stop: the files are written either way, and staging can be redone.
    /// </summary>
    private static void Stage(string dir)
    {
        if (Git.Run(dir, "add", "-A") is null)
            Note("new: `git add -A` did not run, so nothing is staged. The files are written.");
        else
            Account("new: staged. `git status` shows everything this wrote, and the first commit is yours.");
    }

    private static int StopUnsound(string verb, string from, TemplateFaults faults)
    {
        if (faults.Unclassified.Count > 0)
        {
            Stop($"{verb}: {from} has a manifest that does not place its own tree. These files match no rule:");
            foreach (var path in faults.Unclassified) Out.ErrLine($"  {path}");
        }

        if (faults.UnknownCi.Count > 0)
            Stop($"{verb}: {from} serves {string.Join(" and ", faults.UnknownCi)}, which this tool cannot offer. "
                 + $"It offers {string.Join(", ", CiSystem.All)}.");

        return 1;
    }

    /// <summary>The verb is the caller's, because <c>update</c> gives a type up through this too.</summary>
    private static int Cancelled(string verb)
    {
        Note($"{verb}: cancelled. Nothing was written.");
        return 1;
    }

    /// <summary>Nothing where the template was read from a folder.</summary>
    private static string At(string? commit) => commit is { Length: >= 7 } c ? $" at {c[..7]}" : "";

    /// <summary>
    /// Against the corpus root, so every verb answers the same from any folder inside the corpus. A path the platform
    /// cannot parse comes back as given, and the caller reports a folder that is not there.
    /// </summary>
    private static string Rooted(string corpusRoot, string path)
    {
        try { return Path.GetFullPath(path, corpusRoot); }
        catch (ArgumentException) { return path; }
    }

    /// <summary>The corpus is never loaded bar its descriptor, which says where the plugin tree is read from.</summary>
    public static int Bundle(string corpusRoot)
    {
        var descriptor = CorpusDescriptor.Load(corpusRoot);
        var own = Bundler.Read(Path.Combine(corpusRoot, Bundler.SourceDir));
        var pluginTree = own;

        if (descriptor.PluginFrom is { } from)
        {
            // Not `Update.TemplatePath`, which hands back a path it cannot resolve because a `--from` may be a URL.
            var shared = Bundler.Read(Rooted(corpusRoot, from));
            if (shared is null)
                return Fail($"bundle: .corpus.yaml reads the plugin tree from {from}, and there is no folder "
                            + "there. The path is relative to this corpus's root.");

            pluginTree = Bundler.Merge(shared, own ?? []);
            Note($"bundle: read the plugin tree from {from}, less the manifest, which names this plugin "
                 + $"and is {Bundler.SourceDir}/{Bundler.ManifestFile} here.");
        }

        if (pluginTree is null)
            return Fail($"bundle: no plugin tree at {Bundler.SourceDir}/. It is the source a plugin is built from, "
                        + "and it arrives with the mechanism.");

        var export = Bundler.Read(Path.Combine(corpusRoot, Dist.Export.Replace('/', Path.DirectorySeparatorChar)));
        if (export is null)
            return Fail($"bundle: no export at {Dist.Export}/. Run it first: kac export");

        var plan = Bundler.Plan(new BundleSource(pluginTree, export));

        if (plan.Problems.Count > 0)
        {
            foreach (var problem in plan.Problems) Stop($"bundle: {problem}");
            return 1;
        }

        var written = Bundler.Write(corpusRoot, plan);
        foreach (var path in written) Out.Markup(Wrote(path));

        // Named rather than counted: two corpora building one plugin name may drop different components.
        foreach (var t in plan.Trimmed)
            Note($"bundle: trimmed {t.Path}: {t.Reason}.");

        foreach (var warning in plan.Warnings) Note($"bundle: {warning}");

        Account(
            $"bundle: wrote {written.Count} file(s) to {Dist.Plugin}/ as {plan.PluginName} "
            + $"{plan.Version ?? "(no version)"}. {plan.Included.Count} component(s) included, "
            + $"{plan.Trimmed.Count} trimmed.");

        Out.Markup(
            $"[grey]bundle:[/] {Dist.Root}/ is a marketplace holding it. Install it from a path with:  "
            + $"[bold]claude plugin marketplace add ./{Dist.Root}[/]");
        return 0;
    }

    /// <summary>
    /// Reads the export alone, so the tree that was proved and the tree that is published cannot come apart.
    /// </summary>
    public static int Pack(string corpusRoot, string? repository)
    {
        var export = Bundler.Read(Path.Combine(corpusRoot, Dist.Export.Replace('/', Path.DirectorySeparatorChar)));
        if (export is null)
            return Fail($"pack: no export at {Dist.Export}/. Run it first: kac export");

        var plan = Packer.Plan(export, repository);

        if (plan.Problems.Count > 0)
        {
            foreach (var problem in plan.Problems) Stop($"pack: {problem}");
            return 1;
        }

        var written = Packer.Write(corpusRoot, plan);
        Out.Markup(Wrote(written));

        Account($"pack: sealed {plan.Entries.Count} file(s) as {plan.Id} {plan.Version}, cited as "
                + $"'{plan.Shortcode}:'.");

        // `docs/use/cli/pack.md` says what to do when a registry rejects the push.
        Note($"pack: {plan.Version} is content-version from .corpus.yaml. A registry never replaces a "
             + "published version.");
        return 0;
    }

    /// <summary>
    /// Fetch what <c>consumes:</c> declares into <c>.imports/</c>, from <paramref name="registry"/>.
    /// </summary>
    public static int Restore(string corpusRoot, Registry registry)
    {
        var descriptor = CorpusDescriptor.Load(corpusRoot);
        if (descriptor.Consumes.Count == 0)
        {
            Out.Line("this corpus consumes nothing, so there is nothing to restore.");
            return 0;
        }

        var plan = kac.core.Restore.Plan(descriptor.Consumes, registry,
            shortcode => kac.core.Restore.Installed(corpusRoot, shortcode));

        if (plan.Problems.Count > 0)
        {
            foreach (var problem in plan.Problems) Stop($"restore: {problem}");
            return 1;
        }

        var written = kac.core.Restore.Write(corpusRoot, plan);
        foreach (var path in written) Out.Markup(Wrote(path));

        // Written after the fetch, so a run that refused halfway leaves the lock from the last whole restore.
        var locked = CorpusDescriptor.SetResolved(corpusRoot,
            plan.Steps.ToDictionary(s => s.Corpus, s => s.Version, StringComparer.Ordinal));

        // Named rather than passed over: an entry the writer could not place would otherwise read as locked.
        var unwritten = plan.Steps.Select(s => s.Corpus)
            .Where(c => !locked.Contains(c, StringComparer.Ordinal)).ToList();
        if (unwritten.Count > 0)
            Note($"restore: could not write a resolved version for {string.Join(", ", unwritten)}. "
                 + "Write `resolved:` on each entry by hand, so the next run takes the same version.");

        foreach (var step in plan.Steps)
            Account($"restore: {step.Corpus} {step.Version} as '{step.Shortcode}:'."
                    + (step.Current ? " Already current." : ""));

        Account($"restore: {written.Count} fetched, {plan.Steps.Count - written.Count} already current. "
                + $"{locked.Count} resolved version(s) written to .corpus.yaml, and "
                + $"{kac.core.Restore.ImportsDir}/ is not committed.");
        return 0;
    }

    public static int Generate(string corpusRoot, bool check)
    {
        var corpus = Corpus.Load(corpusRoot);

        // `GeneratedFiles` owns which files and blocks there are, so `validate` checks the list this writes.
        var plan = GeneratedFiles.Plan(corpus.Schema, corpus.Adopted, corpus.Docs, corpus.Tree);

        if (check)
        {
            var stale = plan.Where(f => f.Stale).Select(f => f.Path).ToList();
            if (stale.Count == 0)
            {
                Out.Line($"generated files are up to date ({plan.Count} file(s)).");
                return 0;
            }

            Stop("generated files are stale. These differ from the schema/frontmatter:");
            foreach (var s in stale) Out.ErrLine($"  {s}");
            Out.ErrLine("run:  kac generate");
            return 1;
        }

        ReportWritten(plan, GeneratedFiles.Write(corpusRoot, plan));
        return 0;
    }

    /// <summary>Shared with <c>update</c>, which ends by regenerating.</summary>
    private static void ReportWritten(IReadOnlyList<GeneratedFiles.GeneratedFile> plan, List<string> written)
    {
        var created = plan.Where(f => f.State is GeneratedFiles.FileState.Missing).Select(f => f.Path)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var path in written)
            Out.Markup(Wrote(path) + (created.Contains(path) ? "  [grey](new)[/]" : ""));

        Out.Markup(written.Count == 0
            ? "generated files already up to date; nothing written."
            : $"updated [bold]{written.Count}[/] of {plan.Count} generated file(s).");
    }

    private static int Report(List<Finding> findings, int validated, int templates, int skipped, bool json)
    {
        var errors = findings.Count(f => f.Severity == Sev.Error);
        var warnings = findings.Count(f => f.Severity == Sev.Warning);
        var infos = findings.Count(f => f.Severity == Sev.Info);

        if (json)
        {
            // Through the source generator rather than reflection. See Json.cs.
            var report = new ValidateReport(
                new ValidateSummary(validated, templates, skipped, errors, warnings, infos),
                [
                    .. findings
                        .OrderBy(f => f.File).ThenBy(f => f.Line ?? 0)
                        .Select(f => new ValidateFinding(
                            f.File, f.Line, f.Severity.ToString().ToLowerInvariant(), f.Check.Value, f.Message))
                ]);

            Console.WriteLine(JsonSerializer.Serialize(report, KacJson.Relaxed.ValidateReport));
            return errors > 0 ? 1 : 0;
        }

        foreach (var grp in findings.GroupBy(f => f.File).OrderBy(g => g.Key))
        {
            var file = grp.Key.EscapeMarkup();
            Out.Markup($"[bold]{file}[/]");

            var grid = Rows();
            foreach (var f in grp.OrderBy(f => f.Line ?? 0))
            {
                // The location repeats the file name the heading above already gave, because `path:line` in one token
                // is the form a terminal offers to open.
                var at = f.Line is { } ln ? $"  [grey]({file}:{ln})[/]" : "";
                grid.AddRow(
                    new Markup(Tag(f.Severity)),
                    new Markup($"[grey][[{f.Check.Value.EscapeMarkup()}]][/]"),
                    new Markup(f.Message.EscapeMarkup() + at));
            }

            Out.Write(grid);
            Out.Line();
        }

        Out.Markup(
            $"validated {validated} document(s) and {templates} template(s), skipped {skipped} without "
            + $"frontmatter. {Tally(errors, Sev.Error)}, {Tally(warnings, Sev.Warning)}"
            + (infos > 0 ? $", {Tally(infos, Sev.Info)}" : ""));
        return errors > 0 ? 1 : 0;
    }

    public static int Checks(string corpusRoot, bool json)
    {
        // The only verb that reads the schema without loading the corpus, so it takes the walk itself.
        var schema = Schema.LoadNearest(corpusRoot);
        var catalogue = CheckCatalogue.For(schema);

        // The catalogue is always emitted. Whether the table matches it is a separate signal, on stderr and in
        // the exit code, and the test suite relies on that exit code.
        if (json)
        {
            var report = new ChecksReport(
            [
                .. catalogue.Select(c =>
                    new CheckInfo(c.Id.Value, c.Severity.ToString().ToLowerInvariant(), c.Summary))
            ]);
            Console.WriteLine(JsonSerializer.Serialize(report, KacJson.Relaxed.ChecksReport));
        }
        else
        {
            var grid = Rows();
            foreach (var c in catalogue)
                grid.AddRow(
                    new Markup(Tag(c.Severity)),
                    new Markup(c.Id.Value.EscapeMarkup()),
                    new Markup(c.Summary.EscapeMarkup()));

            Out.Write(grid);
            Out.Line();

            Out.Markup($"{catalogue.Count} checks: "
                       + $"{Tally(catalogue.Count(c => c.Severity == Sev.Error), Sev.Error)}, "
                       + $"{Tally(catalogue.Count(c => c.Severity == Sev.Warning), Sev.Warning)}, "
                       + $"{Tally(catalogue.Count(c => c.Severity == Sev.Info), Sev.Info)}.");
        }

        var problems = ChecksTable.Problems(schema);
        if (problems.Count == 0) return 0;

        Stop("checks: the reader-facing checks table is out of step with the catalogue:");
        foreach (var p in problems) Out.ErrLine($"  {p}");
        // Only the flag is named by a path: the row lives in a table this tool ships. `docs/maintaining/checks.md`
        // says where it sits.
        Out.ErrLine("'on-type-page:' sits beside the check in .schema/_checks.yaml.");
        return 1;
    }

    /// <summary><c>docs/use/cli/report.md</c> says what each report carries.</summary>
    public static int Report(string corpusRoot, string name, string toolVersion, string at, string? outPath)
    {
        if (!Reports.Names.Contains(name, StringComparer.Ordinal))
            return Fail($"report: there is no report called '{name}'. "
                        + $"There is {string.Join(" and ", Reports.Names)}.");

        var corpus = Corpus.Load(corpusRoot);

        if (outPath is not null && corpus.Descriptor.Name is null)
            return Fail("report: .corpus.yaml states no `corpus:`, so nothing was written. A report kept as a record "
                        + "cites the corpus by that name. Write `corpus:` in .corpus.yaml, or leave out --out to "
                        + "print the report.");

        var (inherited, notRestored) = Inherited.Read(corpusRoot, corpus.Descriptor.Consumes);
        if (notRestored.Count > 0)
            return Fail($"report: nothing is restored for {string.Join(", ", notRestored)}, which this corpus "
                        + "consumes and a report reads. Run kac restore.");

        var sources = Reports.Sources(corpus.Descriptor, inherited);
        var plan = Reports.Plan(name, corpus, inherited, ReportStamp.ForTool(toolVersion, at, sources));
        if (plan is null) return Fail($"report: '{name}' named no report this build can write.");

        if (outPath is null)
        {
            Out.Line(plan.Text);
            return 0;
        }

        // Relative to where the command was typed, as a shell's own redirection is, not to the corpus root.
        var full = Path.GetFullPath(outPath);

        // A finished report holds verdicts somebody wrote. `writing-a-report` merges them by hand, and refusing
        // here is what leaves something to merge.
        if (File.Exists(full))
            return Fail($"report: {outPath} already exists, so nothing was written. A report holds verdicts "
                        + "somebody wrote. Write this run somewhere else, and merge the two by hand.");

        var folder = Path.GetDirectoryName(full);
        if (folder is not null && !Directory.Exists(folder))
            return Fail($"report: there is nowhere to put {outPath}. Create the folder first.");

        File.WriteAllText(full, plan.Text);
        Out.Markup(Wrote(outPath));
        return 0;
    }

    /// <summary>
    /// Everything that can stop the run is settled before anything is written, as in <c>New</c>. Nothing is committed.
    /// <c>docs/use/cli/update.md</c> argues each step. <c>today</c> is passed in so a golden can pin it.
    /// </summary>
    public static int Update(string corpusRoot, UpdateRequest request, string toolVersion, string today)
    {
        if (kac.core.Update.Contradiction(request) is { } contradiction) return Fail(contradiction);

        if (CorpusDescriptor.RenamedKeyInUse(corpusRoot) is { } renamed) return Fail(renamed);

        var descriptor = CorpusDescriptor.Load(corpusRoot);

        var settled = kac.core.Update.Settle(request, descriptor);
        if (settled.Failed) return Fail(settled.Problem);
        var (from, policy) = settled.Settings;

        // `--check` writes nothing, so it runs over a tree in any state. An unknown tree goes on, as a clean
        // one does.
        if (!request.Check && Git.Status(corpusRoot) is WorkingTree.Dirty)
            return Fail("update: this repository holds uncommitted changes. Commit or stash them first, "
                        + "so that what `update` writes reads as a diff of its own.");

        using var take = TemplateSource.Take("update", kac.core.Update.TemplatePath(from, corpusRoot), from,
            request.Ref ?? descriptor.UpstreamRef, request.Path ?? descriptor.UpstreamPath,
            Path.GetTempPath(), prompt: Asking.Asks(request.Yes, Out.Interactive), toolVersion);
        if (take.Problem is { } unreadable) return Fail(unreadable);

        var corpusFiles = kac.core.Update.Listing(corpusRoot);
        var asked = kac.core.Update.Adopt(corpusFiles, descriptor, take.Schema, take.Declared,
            request.AddType, request.DropType);
        if (asked.Failed) return Fail(asked.Problem);
        var adoption = asked.Adoption;

        var types = new UpdateTypes(take.Declared, adoption.Types,
            kac.core.New.DeclinesTypes(take.Schema, adoption.Types ?? take.Declared));

        var declined = SeedLinks.Declined(take.Schema, adoption.Types ?? take.Declared);

        var plan = kac.core.Update.Plan(take.Files(), corpusFiles, take.Manifest, descriptor, types, policy,
            kac.core.Update.ReadInPlace(take.Root, corpusRoot),
            file => kac.core.Update.Same(take.Root, corpusRoot, file, declined),
            new RecordIds(
                rel => kac.core.Update.IdAt(take.Root, rel, take.Schema),
                rel => kac.core.Update.IdAt(corpusRoot, rel, take.Schema)));

        // Joined to the plan, so `--check` reports the deletions too.
        if (adoption.Deleted.Count > 0)
            plan = plan with
            {
                Deleted =
                [
                    .. plan.Deleted.Concat(adoption.Deleted)
                        .Distinct(StringComparer.Ordinal)
                        .Order(StringComparer.Ordinal)
                ]
            };

        if (plan.Faults.Unsound) return StopUnsound("update", from, plan.Faults);

        var origin = from + At(take.Commit);
        if (request.Check) return ReportCheck(plan, origin, request.DropType);

        if (request.DropType is { } dropping && Relinquish(dropping, request) is { } stopped)
            return stopped;

        var unlinked = kac.core.Update.Apply(plan, take.Root, corpusRoot, declined);
        CorpusDescriptor.Stamp(corpusRoot, take.Manifest.Version, today, take.Commit);

        // Only where a flag asked for it: `types:` is a block somebody laid out by hand.
        if (adoption.Account is not null && adoption.Types is { } adopted)
            CorpusDescriptor.SetTypes(corpusRoot, adopted);

        return ReportUpdate(plan, adoption, corpusRoot, origin, take.Manifest.Version, today,
            request.DropType, request.AddType, unlinked);
    }

    /// <summary>Only the message column wraps, so a check id is never split across lines.</summary>
    private static Grid Rows()
    {
        var grid = new Grid();
        grid.AddColumn(new GridColumn().NoWrap().PadLeft(2).PadRight(2));
        grid.AddColumn(new GridColumn().NoWrap().PadRight(2));
        grid.AddColumn(new GridColumn().PadRight(0));
        return grid;
    }

    /// <summary>The word carries the severity where colour cannot, into a pipe or under <c>--no-color</c>.</summary>
    private static (string Word, string Colour) Severity(Sev severity) => severity switch
    {
        Sev.Error => ("error", "red"),
        Sev.Warning => ("warning", "yellow"),
        Sev.Info => ("info", "grey"),

        // For a value cast in from outside the enum, or a severity added and not named here.
        _ => throw new InvalidOperationException($"no word for severity '{severity}'.")
    };

    private static string Tag(Sev severity)
    {
        var (word, colour) = Severity(severity);
        return $"[{colour}]{word}[/]";
    }

    /// <summary>Zero stays plain: a red zero reads as a failed run.</summary>
    private static string Tally(int count, Sev severity)
    {
        var (word, colour) = Severity(severity);

        var counted = severity == Sev.Info ? $"{count} {word}" : $"{count} {word}(s)";
        return count == 0 ? counted : $"[{colour}]{counted}[/]";
    }

    private static string Wrote(string path)
    {
        var cut = path.LastIndexOf('/') + 1;
        return cut == 0
            ? $"[grey]wrote[/] {path.EscapeMarkup()}"
            : $"[grey]wrote {path[..cut].EscapeMarkup()}[/]{path[cut..].EscapeMarkup()}";
    }

    private static void Note(string line) => Out.Markup(Prefix(line, "yellow"));

    /// <summary>Said rather than repaired, as with the pages an added type arrives among.</summary>
    private static void Unlinked(string verb, IReadOnlyList<UnlinkedPage> pages)
    {
        if (pages.Count == 0) return;

        Note($"{verb}: these lines name a type this corpus declined, and the link to it is gone. Read each one:");
        foreach (var page in pages)
        foreach (var line in page.Lines)
            Out.Line($"  {page.Path}:{line.Number}  {string.Join(", ", line.Types)}");
    }

    private static void Account(string line) => Out.Markup(Prefix(line, "grey"));

    private static string Prefix(string line, string colour)
    {
        var cut = line.IndexOf(':') + 1;
        return $"[{colour}]{line[..cut].EscapeMarkup()}[/]{line[cut..].EscapeMarkup()}";
    }

    private static int Fail(string message)
    {
        Stop(message);
        return 1;
    }

    private static void Stop(string line) => Out.ErrMarkup($"[red]{line.EscapeMarkup()}[/]");

    private static int ReportCheck(UpdatePlan plan, string origin, string? dropped)
    {
        Out.Line($"update: comparing this corpus against {origin}.");
        Aside(plan, dropped);

        if (!plan.Changes)
        {
            Out.Line($"update: in step, {plan.InStep} file(s) compared.");
            return 0;
        }

        Stop(HasWrites(plan)
            ? "this corpus is behind its framework. These would change:"
            : "this corpus is out of step with its framework:");
        Listed("WRITE, framework files this corpus holds differently", plan.Written.Select(f => f.To));
        Listed("SEED, files the corpus has none of", plan.Seeded.Select(f => f.To));
        Listed("DELETE, files the template has retired", plan.Deleted);
        Listed("UNSHARED, framework files this corpus holds and the template does not send",
            plan.Unshared);
        Out.ErrLine(plan.Unshared.Count > 0 && !HasWrites(plan)
            ? "an unshared file is a framework change made in the wrong tree. Move it upstream, or say "
              + "the corpus owns it with a skip: entry. One under .plugin/ is a copy left behind by "
              + "adopting plugin.from, and it wins over the shared tree until it is deleted."
            : "run:  kac update");
        return 1;

        static bool HasWrites(UpdatePlan p) => p.Written.Count > 0 || p.Seeded.Count > 0 || p.Deleted.Count > 0;

        static void Listed(string heading, IEnumerable<string> paths)
        {
            var found = paths.ToList();
            if (found.Count == 0) return;
            Out.ErrLine($"{heading}:");
            foreach (var p in found) Out.ErrLine($"  {p}");
        }
    }

    private static int ReportUpdate(UpdatePlan plan, Adoption adoption, string corpusRoot, string origin,
        int templateVersion, string today, string? dropped, string? added, IReadOnlyList<UnlinkedPage> unlinked)
    {
        Out.Line($"update: taking the framework from {origin}.");
        foreach (var file in plan.Written) Out.Markup(Wrote(file.To));
        foreach (var file in plan.Seeded) Out.Markup(Wrote(file.To) + "  [grey](new)[/]");
        foreach (var rel in plan.Deleted) Out.Markup($"  [red]deleted[/]  {rel.EscapeMarkup()}");

        Aside(plan, dropped);
        if (adoption.Account is { } changed) Account(changed);

        if (plan.Unshared.Count > 0)
        {
            Note($"update: {plan.Unshared.Count} file(s) sit where the framework's rules apply, and the "
                 + "template sends nothing to them:");
            foreach (var rel in plan.Unshared) Out.Line($"  {rel}");
        }

        Account($"update: wrote {plan.Written.Count}, seeded {plan.Seeded.Count}, deleted "
                + $"{plan.Deleted.Count}; {plan.InStep} already in step. Recorded in .corpus.yaml as "
                + $"template version {templateVersion}, taken {today}.");
        Account("update: nothing is committed. `git diff` is the review step, and `git checkout` on a "
                + "file is how to decline one.");

        // Said rather than repaired: the pages already here hold the corpus's own words.
        if (added is not null)
            Note($"update: {added} arrives linking to the types this corpus holds. The pages already "
                 + $"here name {added} without linking to it, and they are yours to change.");
        Unlinked("update", unlinked);

        // Overlay pages carry generated blocks built from this corpus's types, so a passing
        // `generate --check` is the update's postcondition.
        return Regenerate(corpusRoot);
    }

    /// <summary>A type this run has just given up is left out of the offer.</summary>
    private static void Aside(UpdatePlan plan, string? dropped)
    {
        if (plan.Skipped.Count > 0)
        {
            Account($"update: stepped over {plan.Skipped.Count} file(s) that .corpus.yaml claims:");
            foreach (var p in plan.Skipped) Out.Line($"  {p}");
        }

        if (plan.Declined > 0)
            Account($"update: withheld {plan.Declined} file(s) for types this corpus has not adopted.");

        if (kac.core.Update.CiWithheld(plan) is { } ci) Account(ci);

        if (plan.DeclinedPlugin > 0)
            Account($"update: withheld {plan.DeclinedPlugin} plugin file(s) this corpus reads from "
                    + "elsewhere. .corpus.yaml says where under plugin.from.");

        var offered = plan.Offered.Where(t => !t.Equals(dropped, StringComparison.Ordinal)).ToList();
        if (offered.Count > 0)
            Account($"update: this template also declares {string.Join(", ", offered)}. Take one with "
                    + "kac update --add-type <name>.");
    }

    /// <summary>
    /// Loaded here, after the schema was replaced. A schema this corpus cannot read yet is reported rather than thrown,
    /// because the files are already in place.
    /// </summary>
    private static int Regenerate(string corpusRoot)
    {
        try
        {
            var corpus = Corpus.Load(corpusRoot);
            var plan = GeneratedFiles.Plan(corpus.Schema, corpus.Adopted, corpus.Docs, corpus.Tree);
            ReportWritten(plan, GeneratedFiles.Write(corpusRoot, plan));
            return 0;
        }
        catch (Exception ex)
        {
            Out.ErrLine($"update: regeneration failed: {ex.Message}");
            Out.ErrLine(
                "update: the files are in place but the generated blocks were not rebuilt. "
                + "Run kac validate to see what the corpus is missing, then kac generate.");
            return 1;
        }
    }
}
