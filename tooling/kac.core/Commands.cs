using System.Text.Json;
using Spectre.Console;

// Subcommands: the orchestration behind each CLI verb. The entrypoint, tooling/kac/Program.cs, only wires
// Spectre.Console.Cli to these. All the work lives here and in the rest of kac.core.

namespace kac.core;

public static class Commands
{
    public static int Validate(string corpusRoot, bool json)
    {
        var corpus = Corpus.Load(corpusRoot);
        var findings = Validator.CheckAll(corpus);
        return Report(findings, corpus.Docs.Count, corpus.Templates.Count, corpus.SkippedNoFrontmatter, json);
    }

    // The corpus is loaded whole, whatever `type` names.
    public static int Export(string corpusRoot, string? type)
    {
        var corpus = Corpus.Load(corpusRoot);

        if (type is not null && corpus.Adopted.All(t => t.Key != type))
            return Fail($"export: this corpus has not adopted a type called '{type}'. "
                        + $"It holds {string.Join(", ", corpus.Adopted.Select(t => t.Key))}.");

        var unknown = corpus.Descriptor.ExportExclude
            .Where(e => !CorpusDescriptor.Excludable.Contains(e, StringComparer.Ordinal)).ToList();
        if (unknown.Count > 0)
            return Fail($"export: .corpus.yaml excludes {string.Join(", ", unknown)}, which an export cannot "
                        + $"act on. It excludes {string.Join(" or ", CorpusDescriptor.Excludable)}.");

        var commit = Git.Head(corpusRoot);
        var dirty = Git.Dirty(corpusRoot);
        var publishing = Publishing.For(corpus.Descriptor, commit);
        var now = DateTime.UtcNow;

        var plan = Exporter.Plan(corpus, publishing, type,
            new ExportRun(now.ToString("yyyy-MM-ddTHH:mm:ssZ"), DateOnly.FromDateTime(now), commit, dirty));

        var written = Exporter.Write(corpusRoot, plan);
        foreach (var path in written) Out.Markup(Wrote(path));

        // What the export cannot say is worth saying here, where someone is watching. Neither state is an error. A
        // corpus may publish nowhere, and an export built from a dirty tree is still an export. It is only one that
        // cannot be rebuilt from the commit it names.
        if (publishing is null)
            Note(
                "export: no published links. This corpus states no publishing target the tool can address, "
                + "or no bases for one. Records carry their paths and no URLs.");
        if (dirty is true)
            Note(
                "export: built from a dirty working tree, and the manifest says so. The commit it names "
                + "does not reproduce it.");

        // Named rather than counted. A record left behind is invisible in the output, so this run is the last place
        // anyone sees which ones they were.
        if (plan.Withheld.Count > 0)
            Note(
                $"export: withheld {string.Join(", ", plan.Withheld)}: .corpus.yaml excludes "
                + $"{string.Join(" and ", corpus.Descriptor.ExportExclude)}.");

        // The same reasoning for a cross-reference the export could not read. The export carries what a link names, and
        // a link naming a record rather than a term inside it leaves nothing to carry. So this run states the omission,
        // where an author can act on it, rather than leaving a reader to guess.
        if (plan.Unread.Count > 0)
        {
            Note(
                $"export: {plan.Unread.Count} cross-reference(s) name a record and no part inside it, so "
                + "nothing was carried for them:");
            foreach (var u in plan.Unread) Out.Line($"  {u}");
            Note("export: point each link at the part it means, as '<file>.md#<anchor>'.");
        }

        // An empty type list is a statement of what this corpus has, and not a failure. Either it adopted no type that
        // declares an export, or it withheld everything the types it did adopt would have carried. The line above says
        // which, where it was the second.
        Account(plan.Types.Count == 0
            ? $"export: wrote {written.Count} file(s); no type contributed a record."
            : $"export: wrote {written.Count} file(s) for {string.Join(", ", plan.Types.Select(t => t.Type))}.");
        return 0;
    }

    // Turn the folder this was run in into a corpus.
    //
    // The one verb answering about a corpus that is not there yet, so it takes the working directory
    // rather than a corpus root, and refuses where the others require one.
    //
    // The order is the whole design: everything that can fail is settled before the first question, so
    // that nobody answers six of them and is then told the URL was unreachable. `docs/cli/new.md` argues
    // each step. `today` is passed in rather than read here, so a golden can pin what the descriptor says.
    public static int New(string dir, NewRequest request, string toolVersion, string today)
    {
        var ground = kac.core.New.Survey(dir);

        if (ground.Corpus is { } already)
            return Fail($"new: {already} is already a corpus, so there is nothing here to create. "
                        + "taking a newer framework into one is `kac update`.");

        // A dirty tree stops the run, so that what `new` writes is legible as a diff against what was
        // there. A tree git could not answer for is not reported as clean, and is not stopped either.
        if (ground.Dirty is true)
            return Fail("new: this repository holds uncommitted changes. commit or stash them first, so "
                        + "that what `new` writes reads as a diff of its own.");

        // Nobody to ask, in the two ways that happen: `--yes` answered everything in advance, and a run
        // with no terminal has nobody at the keyboard. What each comes to differs, and `--yes` decides.
        var asker = request.Yes || !Out.Interactive ? null : new ConsoleAsker();

        if (!ground.Repository && Initialise(dir, request, asker) is { } refused) return refused;

        if (ground.Holds.Count > 0)
        {
            Note($"new: this folder already holds {string.Join(", ", ground.Holds)}. without a committed "
                 + "baseline there is nothing to tell those from the files about to arrive.");
            if (asker is not null && !asker.Confirm("Create the corpus here anyway?")) return Cancelled();
        }

        var read = TemplateSource.Read(request.From, request.Ref, request.Path, Path.GetTempPath(),
            prompt: asker is not null);
        if (read.Problem is { } unreachable) return Fail(unreachable);
        using var template = read.Source!;

        var manifestFile = Path.Combine(template.Root, Manifest.FileName);
        if (!File.Exists(manifestFile))
            return Fail($"new: {request.From} holds no {Manifest.FileName}, so there is no template to "
                        + "read. --path names the folder holding it, where it is not at the root.");

        var manifest = Manifest.LoadFrom(manifestFile);
        if (kac.core.New.TooOldFor(manifest.MinimumTool, toolVersion) is { } tooOld) return Fail(tooOld);

        // The schema the template serves, which is the one account of what types there are to adopt.
        var schema = Schema.Load(Schema.FindRoot(template.Root) ?? template.Root);
        var declared = schema.ByFolder.Keys.OrderBy(k => k, StringComparer.Ordinal).ToList();

        var answered = Asking.Resolve(request, new DirectoryInfo(dir).Name, declared,
            Git.Run(dir, "remote get-url origin")?.Trim(), asker);
        if (answered.Problem is { } unanswered) return Fail(unanswered);
        var answers = answered.Answers!;

        var upstream = new Upstream(request.From, request.Path, request.Ref, template.Commit,
            manifest.Version, today);

        Summarise(dir, answers, upstream, declared.Count);
        if (asker is not null && !asker.Confirm("Create it?")) return Cancelled();

        var plan = kac.core.New.Plan(template.Files(), manifest, answers, upstream,
            kac.core.New.DeclinesTypes(schema, answers.Types));
        if (plan.TemplateIsUnsound) return Unsound(plan, request.From);

        foreach (var path in kac.core.New.Apply(plan, template.Root, dir)) Out.Markup(Wrote(path));

        // Named where a reader can act on it, counted where they cannot. A declined type is a decision
        // just made and needs no list; a starter withheld is one file, and which one is the whole fact.
        if (plan.DeclinedTypes.Count > 0)
            Account($"new: {plan.DeclinedTypes.Count} file(s) withheld for the types this corpus declined.");
        if (plan.DeclinedCi.Count > 0)
            Account($"new: did not write {string.Join(", ", plan.DeclinedCi)}: this corpus is built by "
                    + $"{answers.Ci}.");

        Account($"new: wrote {plan.Copied.Count + plan.Composed.Count} file(s) for {answers.Name}, taken "
                + $"from {request.From}{At(template.Commit)}.");

        // Generation writes the `_index.md` files and the generated blocks that validation then checks,
        // so it goes first. Staging goes last, so everything the command did is visible in one place.
        Generate(dir, check: false);
        var validated = Validate(dir, json: false);
        Stage(dir);

        if (validated == 0) return 0;

        // Two different faults end here, and the message has to say which. A corpus taking every type and
        // failing is a defect upstream, and the person who just ran the command should not be left
        // thinking they caused it. A corpus that declined types is meeting the cross-references on the
        // pages it did receive, and those pages are seeds it owns.
        Stop(plan.DeclinedTypes.Count > 0
            ? "new: the corpus this created does not validate. a page it received links to a type this "
              + "corpus declined. those pages are yours from here, so edit the links out. the files are "
              + "written and staged."
            : "new: the corpus this created does not validate. that is a defect in the template or in the "
              + "tool, and not in anything you answered. the files are written and staged.");
        return 1;
    }

    // Offer `git init`, and answer with the refusal where there is one. Discovery reads the git listing,
    // so the choice is between running it and cancelling rather than carrying on without a repository.
    private static int? Initialise(string dir, NewRequest request, IAsker? asker)
    {
        if (asker is null && !request.Yes)
            return Fail("new: this folder is not a git repository, and there is no terminal to ask on. "
                        + "run `git init` first, or pass --yes to have it run.");

        if (asker is not null && !asker.Confirm($"{dir} is not a git repository. Run `git init` here?"))
            return Cancelled();

        return Git.Run(dir, "init -q") is null
            ? Fail("new: `git init` failed. the tool reads the git listing to find what a corpus holds, "
                   + "so a corpus git cannot see is not one worth writing.")
            : null;
    }

    // What the run is about to do, before the one question that can still stop it.
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

    // Everything the command did, in one place, before anybody commits it. A failure here is worth a note
    // and not a stop: the files are written either way, and staging is the part a person can redo.
    private static void Stage(string dir)
    {
        if (Git.Run(dir, "add -A") is null)
            Note("new: `git add -A` did not run, so nothing is staged. the files are written.");
        else
            Account("new: staged. `git status` shows everything this wrote, and the first commit is yours.");
    }

    // A template this tool cannot read the whole of. Its files are neither taken nor silently dropped,
    // because each state is a defect upstream and acting on half a manifest is how a corpus goes wrong
    // quietly.
    private static int Unsound(NewPlan plan, string from)
    {
        if (plan.Unclassified.Count > 0)
        {
            Stop($"new: {from} has a manifest that does not place its own tree. these files match no rule:");
            foreach (var path in plan.Unclassified) Out.ErrLine($"  {path}");
        }

        if (plan.UnknownCi.Count > 0)
            Stop($"new: {from} serves {string.Join(" and ", plan.UnknownCi)}, which this tool cannot offer. "
                 + $"it offers {string.Join(", ", CiSystem.All)}.");

        return 1;
    }

    private static int Cancelled()
    {
        Note("new: cancelled. nothing was written.");
        return 1;
    }

    // The commit a take resolved to, short, or nothing where the template was read from a folder.
    private static string At(string? commit) => commit is { Length: >= 7 } c ? $" at {c[..7]}" : "";

    // Assemble the plugin from the export and the `.plugin/` tree. Two trees in, one directory out, and
    // the corpus is never loaded: what a bundle has to decide is a fact about the export it was handed.
    public static int Bundle(string corpusRoot)
    {
        var pluginTree = Bundler.Read(Path.Combine(corpusRoot, Bundler.SourceDir));
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

        // Named rather than counted, as the export names what it withheld. A dropped component is invisible in the
        // output, and two corpora building one plugin name may drop different ones. So the run says which, beside the
        // `bundle.json` that will outlive it.
        foreach (var t in plan.Trimmed)
            Note($"bundle: trimmed {t.Path}: {t.Reason}.");

        foreach (var warning in plan.Warnings) Note($"bundle: {warning}");

        Account(
            $"bundle: wrote {written.Count} file(s) to {Dist.Plugin}/ as {plan.PluginName} "
            + $"{plan.Version ?? "(no version)"}. {plan.Included.Count} component(s) included, "
            + $"{plan.Trimmed.Count} trimmed.");

        // The command is the one part of this line anybody retypes, so it is the one part set in bold.
        Out.Markup(
            $"[grey]bundle:[/] {Dist.Root}/ is a marketplace holding it. Install it from a path with:  "
            + $"[bold]claude plugin marketplace add ./{Dist.Root}[/]");
        return 0;
    }

    public static int Generate(string corpusRoot, bool check)
    {
        var corpus = Corpus.Load(corpusRoot);

        // What every generated file should hold, beside what it holds now. `GeneratedFiles` owns which files those are
        // and which blocks each carries, so `validate` holds a corpus to the same list this writes.
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

    // What a regeneration wrote. Shared with `mechanism --sync`, which ends by regenerating, so a sync reports the
    // files it rebuilt in the words `generate` uses for the same work.
    //
    // A file the corpus did not hold is marked, because creating one changes what the corpus contains rather than what
    // a file inside it says. The tally names the whole plan beside the part of it that moved: a reader who sees one
    // file written wants to know it was one of forty.
    private static void ReportWritten(IReadOnlyList<GeneratedFiles.GeneratedFile> plan, List<string> written)
    {
        var created = plan.Where(f => f.Current is null).Select(f => f.Path).ToHashSet(StringComparer.Ordinal);

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

        if (json)
        {
            // Through the source generator rather than reflection. See Json.cs.
            var report = new ValidateReport(
                new ValidateSummary(validated, templates, skipped, errors, warnings),
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

        // Templates are counted apart from documents because they are checked apart from them. A reader who sees a
        // finding against a `_template.md` should find it accounted for in the tally. A reader who sees none should be
        // able to tell the templates were read rather than skipped.
        Out.Markup(
            $"validated {validated} document(s) and {templates} template(s), skipped {skipped} without "
            + $"frontmatter. {Tally(errors, Sev.Error)}, {Tally(warnings, Sev.Warning)}");
        return errors > 0 ? 1 : 0;
    }

    public static int Checks(string corpusRoot, bool json)
    {
        // The only verb that reads the schema without loading the corpus around it, so it takes the walk
        // itself. `Corpus.Load` is where every other verb gets the same answer.
        var schema = Schema.Load(Schema.FindRoot(corpusRoot) ?? corpusRoot);
        var catalogue = CheckCatalogue.For(schema);

        // The catalogue is always valid data, so emit it either way. Whether the reader-facing table is faithful to it
        // is a separate signal, reported to stderr and through the exit code below. The test suite relies on that exit
        // code. A new catalogue check with no table row and no explicit waiver exits non-zero here, and so does a row
        // naming a check that no longer exists.
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

            // Split by severity, because a reader comes to this list to learn how much of it fails a
            // build. The catalogue's own order says nothing about that.
            var errors = catalogue.Count(c => c.Severity == Sev.Error);
            Out.Markup($"{catalogue.Count} checks: {Tally(errors, Sev.Error)}, "
                       + $"{Tally(catalogue.Count - errors, Sev.Warning)}.");
        }

        var problems = Generator.ChecksTableProblems(schema);
        if (problems.Count == 0) return 0;

        Stop("checks: the reader-facing checks table is out of step with the catalogue:");
        foreach (var p in problems) Out.ErrLine($"  {p}");
        Out.ErrLine(
            "fix Generator.DocRows in tooling/kac.core/Generator.cs, or the check's 'on-type-page:' "
            + "in .schema/_checks.yaml.");
        return 1;
    }

    public static int Mechanism(string corpusRoot, bool check, bool sync, string? against)
    {
        if (check == sync)
            return Fail(check
                ? "mechanism: --check and --sync are the two halves of this command; ask for one."
                : "mechanism: specify --check to compare against a reference, or --sync to take from one.");

        var descriptor = CorpusDescriptor.Load(corpusRoot);

        // A descriptor still on a renamed key stops both halves. A check would report on a file it has
        // misread, and a sync would stamp beside a key it does not read.
        if (CorpusDescriptor.RenamedKeyInUse(corpusRoot) is { } renamed) return Fail(renamed);

        // A sync needs a declared upstream, and not just a directory it can read. `--against` says which copy of the
        // upstream to take from, a local checkout rather than the URL. `upstream.url` says that the corpus takes from
        // an upstream at all. The corpus at the head of the chain names none: changes leave it and none arrive, so a
        // sync has nowhere to run from.
        if (sync && descriptor.UpstreamUrl is null)
            return Fail("mechanism: this corpus names no upstream, so there is nothing to sync from. "
                        + "A corpus that takes from another records it in upstream.url in .corpus.yaml.");

        var reference = against ?? descriptor.UpstreamUrl;
        if (string.IsNullOrWhiteSpace(reference))
            return Fail("mechanism: no reference to compare against. Pass --against <path>, "
                        + "or set upstream.url in .corpus.yaml.");

        var refRoot = Path.GetFullPath(reference, corpusRoot);
        if (!Directory.Exists(refRoot))
            return Fail($"mechanism: reference corpus not found: {refRoot}");
        if (Path.GetFullPath(refRoot) == Path.GetFullPath(corpusRoot))
            return Fail("mechanism: the reference is this corpus itself. Nothing to compare.");

        var localFiles = MechanismCheck.ListFiles(corpusRoot);
        var refFiles = MechanismCheck.ListFiles(refRoot);

        // Check reads this corpus's manifest, because it reports whether this corpus is in step with the
        // boundary it believes in. Sync reads the reference's, because it takes that boundary down along
        // with the files the boundary describes.
        if (check)
            return ReportMechanism(
                MechanismCheck.Classify(localFiles, refFiles, Manifest.Load(corpusRoot), descriptor, Same),
                descriptor, refRoot);

        var manifest = Manifest.Load(refRoot);
        var plan = MechanismSync.Plan(localFiles, refFiles, manifest, descriptor,
            MechanismSync.DeclinedTypePaths(refRoot, descriptor), Same);

        var today = DateTime.Today.ToString("yyyy-MM-dd");
        MechanismSync.Apply(plan, corpusRoot, refRoot, manifest, today);
        return ReportSync(plan, corpusRoot, manifest.Version, reference, today);

        // Whether two copies of a file say the same thing. That is the one question either engine asks of the disk.
        // Passed in, so each engine decides from listings and a predicate rather than from a tree.
        bool Same(string rel) => MechanismCheck.Same(corpusRoot, refRoot, rel);
    }

    // The shape both listings take: how loud, what it is called, and what it says. Only the last column
    // wraps. The other two are short and fixed, and squeezing them would split a check id across lines.
    // A wrapped message keeps the hanging indent, so the column still reads down the page.
    private static Grid Rows()
    {
        var grid = new Grid();
        grid.AddColumn(new GridColumn().NoWrap().PadLeft(2).PadRight(2));
        grid.AddColumn(new GridColumn().NoWrap().PadRight(2));
        grid.AddColumn(new GridColumn().PadRight(0));
        return grid;
    }

    // The two things a severity carries into the output, decided in one place: what it is called, and
    // what colour says so. The word carries the severity where colour cannot, into a pipe or under
    // `--no-color`.
    private static (string Word, string Colour) Severity(Sev severity) =>
        severity == Sev.Error ? ("error", "red") : ("warning", "yellow");

    private static string Tag(Sev severity)
    {
        var (word, colour) = Severity(severity);
        return $"[{colour}]{word}[/]";
    }

    // A count of findings at one severity. Zero stays plain, because a reader glances at this line to
    // decide the run was clean and a red zero says the opposite.
    private static string Tally(int count, Sev severity)
    {
        var (word, colour) = Severity(severity);
        return count == 0 ? $"{count} {word}(s)" : $"[{colour}]{count} {word}(s)[/]";
    }

    // `wrote <path>`, with everything but the filename dimmed. A bundle writes a dozen paths sharing a
    // prefix, and the eye should land on the part that differs. One separator covers every platform:
    // the manifest and the corpus both hold their paths with forward slashes, and so does every path
    // written here.
    private static string Wrote(string path)
    {
        var cut = path.LastIndexOf('/') + 1;
        return cut == 0
            ? $"[grey]wrote[/] {path.EscapeMarkup()}"
            : $"[grey]wrote {path[..cut].EscapeMarkup()}[/]{path[cut..].EscapeMarkup()}";
    }

    // A verb's own remark about the run. The verb's name is coloured to say which kind it is.
    //
    // A note is what nothing else will say: a link that carried nothing, a component dropped, an export that cannot be
    // rebuilt from the commit it names. None of it is an error, and none of it shows in the artefact. Colour stops it
    // reading as part of the tally.
    private static void Note(string line) => Out.Markup(Prefix(line, "yellow"));

    // An account of what the run came to, and the last thing a verb says about its own work.
    private static void Account(string line) => Out.Markup(Prefix(line, "grey"));

    private static string Prefix(string line, string colour)
    {
        var cut = line.IndexOf(':') + 1;
        return $"[{colour}]{line[..cut].EscapeMarkup()}[/]{line[cut..].EscapeMarkup()}";
    }

    // A command stopping on something the caller asked for and cannot have. The message goes to stderr and the exit
    // code says so, as it does for every verb.
    private static int Fail(string message)
    {
        Stop(message);
        return 1;
    }

    // Why a command stopped, or the heading over a list of what stopped it. The whole line is coloured,
    // because it is the message rather than a remark sitting beside other output. Whatever the heading
    // names stays plain beneath it.
    private static void Stop(string line) => Out.ErrMarkup($"[red]{line.EscapeMarkup()}[/]");

    private static int ReportMechanism(MechanismReport report, CorpusDescriptor descriptor, string refRoot)
    {
        // Where the corpus says it stands, before what the comparison found. Three versions answering
        // three questions, so a reader can tell which one moved. See CorpusDescriptor for why an unstated
        // one is reported and not filled in.
        Out.Line(
            $"mechanism: content version {Stated(descriptor.ContentVersion)}, "
            + $"descriptor format {Stated(descriptor.DescriptorVersion)}, "
            + $"template version {Stated(descriptor.TemplateVersion)}.");
        Out.Line($"mechanism: comparing the synced layer against {refRoot}");
        Section("DRIFT, synced files differ from the reference", report.Drift);
        Section("MISSING LOCALLY, synced files in the reference but not here", report.MissingLocally);
        Section("MISSING UPSTREAM, synced files here but not in the reference", report.MissingUpstream);
        Section("UNCLASSIFIED, files matching no manifest rule", report.Unclassified);

        if (report.ResolvedDivergence.Count > 0)
        {
            Out.Line(
                "RESOLVED, accepted divergences that are now identical again (delete them from .corpus.yaml):");
            foreach (var p in report.ResolvedDivergence) Out.Line($"  {p}");
        }

        Out.Line(
            $"synced: {report.SyncedInStep} in step, {report.Drift.Count} drifted; "
            + $"forked: {report.ForkedShared} shared ({report.ForkedDiffer} differ, informational); "
            + $"accepted divergences: {report.AcceptedActive}.");

        // Held but not asked for: schema files for types this corpus did not adopt, or a fixture tree in a corpus whose
        // role declines the verification layer. Neither is drift, because nothing was compared. Say so anyway. No sync
        // will refresh these files, and the alternative is leaving the reader to find them stale later.
        if (report.DeclinedButHeld > 0)
            Out.Line(
                $"declined: {report.DeclinedButHeld} file(s) held here that this corpus's descriptor does not ask for. "
                + "They are not synced or compared; delete them, or adopt what they belong to.");

        if (report.Problems > 0)
        {
            Out.ErrLine($"mechanism check failed: {report.Problems} synced-layer problem(s) above.");
            return 1;
        }

        Out.Line("mechanism: synced layer in step.");
        return 0;

        static void Section(string heading, IReadOnlyList<string> paths)
        {
            if (paths.Count == 0) return;
            Out.ErrLine($"{heading}:");
            foreach (var p in paths) Out.ErrLine($"  {p}");
        }

        static string Stated(object? version) => version?.ToString() is { Length: > 0 } v ? v : "not declared";
    }

    private static int ReportSync(SyncPlan plan, string corpusRoot, int mechanismVersion, string reference,
        string today)
    {
        Out.Line($"mechanism: syncing the shared layers from {reference}");
        Section("UPDATED, brought down from the reference", plan.Updated);
        Section("SEEDED, the corpus's own from here on, copied because it had none", plan.Seeded);
        Section("SKIPPED, accepted divergences, left as they are", plan.Skipped);
        Section("HELD HERE, NOT UPSTREAM, shared files the reference does not have (sync never deletes)",
            plan.HeldHere);

        Out.Line(
            $"synced: {plan.Updated.Count} updated, {plan.InStep} already in step; seeded {plan.Seeded.Count}; "
            + $"skipped {plan.Skipped.Count}; declined {plan.Declined}. "
            + $"Recorded in .corpus.yaml as mechanism version {mechanismVersion}, taken {today}.");

        if (plan.ReferenceIsUnsound)
        {
            Out.ErrLine("UNCLASSIFIED, files in the reference matching no manifest rule, so not copied:");
            foreach (var p in plan.Unclassified) Out.ErrLine($"  {p}");
            Out.ErrLine(
                "mechanism sync: the reference's manifest does not resolve its own tree. Fix it there.");
            return 1;
        }

        // Every synced page may carry a generated block built from this corpus's own types, so the copies above are
        // only right once rebuilt against what this corpus holds. Regenerating here makes a passing `generate --check`
        // sync's postcondition.
        return Regenerate(corpusRoot);

        static void Section(string heading, IReadOnlyList<string> paths)
        {
            if (paths.Count == 0) return;
            Out.Line($"{heading}:");
            foreach (var p in paths) Out.Line($"  {p}");
        }
    }

    // The corpus is loaded here rather than by the caller, because a sync has just replaced the schema. A schema this
    // corpus cannot yet read is the one failure worth surviving: the files are already in place, and saying so is more
    // use than a stack trace over a half-finished tree.
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
            Out.ErrLine($"mechanism sync: regeneration failed: {ex.Message}");
            Out.ErrLine(
                "mechanism sync: the files are in place but the generated blocks were not rebuilt. "
                + "Run kac validate to see what the corpus is missing, then kac generate.");
            return 1;
        }
    }
}
