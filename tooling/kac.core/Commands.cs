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

        var read = TemplateSource.Read("new", request.From, request.Ref, request.Path, Path.GetTempPath(),
            prompt: asker is not null);
        if (read.Problem is { } unreachable) return Fail(unreachable);
        using var template = read.Source!;

        var manifestFile = Path.Combine(template.Root, Manifest.FileName);
        if (!File.Exists(manifestFile))
            return Fail($"new: {request.From} holds no {Manifest.FileName}, so there is no template to "
                        + "read. --path names the folder holding it, where it is not at the root.");

        var manifest = Manifest.LoadFrom(manifestFile);
        if (kac.core.New.TooOldFor(manifest.MinimumTool, toolVersion, "new") is { } tooOld) return Fail(tooOld);

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

    // Both halves of `NewPlan.TemplateIsUnsound`, each naming what the template did rather than the
    // count of it, because the fix is upstream and needs the paths.
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

    // A path the descriptor states, resolved against the corpus root. Every verb answers the same wherever
    // inside a corpus it is run, and a path resolved against the working directory would not: it would find
    // one folder from the root and another from a folder below it.
    //
    // A path the platform cannot parse comes back as it was given. The caller reads that as a folder that is
    // not there, which is the message it was going to print anyway.
    private static string Rooted(string corpusRoot, string path)
    {
        try { return Path.GetFullPath(path, corpusRoot); }
        catch (ArgumentException) { return path; }
    }

    // Assemble the plugin from the export and the `.plugin/` tree. Two trees in, one directory out, and
    // the corpus is never loaded bar its descriptor, which says where the plugin tree is read from: what
    // a bundle has to decide beyond that is a fact about the export it was handed.
    public static int Bundle(string corpusRoot)
    {
        var descriptor = CorpusDescriptor.Load(corpusRoot);
        var own = Bundler.Read(Path.Combine(corpusRoot, Bundler.SourceDir));
        var pluginTree = own;

        if (descriptor.PluginFrom is { } from)
        {
            // Resolved against the corpus root and never against the working directory, so `bundle`
            // assembles the same plugin wherever inside the corpus it is run. `Update.TemplatePath`
            // is the wrong resolver here: it hands back a path it cannot resolve, which is right for
            // a `--from` that may be a URL and would leave this one reading a folder beside the caller.
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

    // What a regeneration wrote. Shared with `update`, which ends by regenerating, so an update reports the files it
    // rebuilt in the words `generate` uses for the same work.
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

    // Take a newer framework into a corpus that already has one.
    //
    // The order mirrors `New`: everything that can stop the run is settled before anything is written.
    // Nothing is committed, which is the whole safety model and the reason `update` can be liberal.
    // `docs/cli/update.md` argues each step. `today` is passed in rather than read here, so a golden can
    // pin what the descriptor says.
    public static int Update(string corpusRoot, UpdateRequest request, string toolVersion, string today)
    {
        if (request.AddType is not null && request.DropType is not null)
            return Fail("update: --add-type and --drop-type change the same list. ask for one.");

        if (CorpusDescriptor.RenamedKeyInUse(corpusRoot) is { } renamed) return Fail(renamed);

        var descriptor = CorpusDescriptor.Load(corpusRoot);

        var from = request.From ?? descriptor.UpstreamUrl;
        if (from is null)
            return Fail("update: this corpus names no template, so there is nothing to take. pass --from, "
                        + "or set upstream.url in .corpus.yaml.");

        var policy = request.Policy ?? descriptor.UpdatePolicy;
        if (!CorpusDescriptor.Policies.Contains(policy, StringComparer.Ordinal))
            return Fail($"update: '{policy}' is not an update policy. it is "
                        + $"{string.Join(" or ", CorpusDescriptor.Policies)}.");

        // A clean tree is what makes everything this writes distinguishable from everything the person
        // wrote. A tree git could not answer for is not reported as clean, and is not stopped either.
        // `--check` writes nothing, so it runs over a tree in any state.
        if (!request.Check && Git.Dirty(corpusRoot) is true)
            return Fail("update: this repository holds uncommitted changes. commit or stash them first, "
                        + "so that what `update` writes reads as a diff of its own.");

        var read = TemplateSource.Read("update", kac.core.Update.TemplatePath(from, corpusRoot),
            request.Ref ?? descriptor.UpstreamRef, request.Path ?? descriptor.UpstreamPath,
            Path.GetTempPath(), prompt: Out.Interactive && !request.Yes);
        if (read.Problem is { } unreachable) return Fail(unreachable);
        using var template = read.Source!;

        var manifestFile = Path.Combine(template.Root, Manifest.FileName);
        if (!File.Exists(manifestFile))
            return Fail($"update: {from} holds no {Manifest.FileName}, so there is no template to read. "
                        + "--path names the folder holding it, where it is not at the root.");

        var manifest = Manifest.LoadFrom(manifestFile);
        if (kac.core.New.TooOldFor(manifest.MinimumTool, toolVersion, "update") is { } tooOld)
            return Fail(tooOld);

        var schema = Schema.Load(Schema.FindRoot(template.Root) ?? template.Root);
        var declared = schema.ByFolder.Keys.OrderBy(k => k, StringComparer.Ordinal).ToList();

        var corpusFiles = kac.core.Update.Listing(corpusRoot);
        var adoption = kac.core.Update.Adopt(corpusFiles, descriptor, schema, declared,
            request.AddType, request.DropType);
        if (adoption.Problem is { } refused) return Fail(refused);

        var types = new UpdateTypes(declared, adoption.Types,
            kac.core.New.DeclinesTypes(schema, adoption.Types ?? declared));

        var plan = kac.core.Update.Plan(template.Files(), corpusFiles, manifest, descriptor, types, policy,
            kac.core.Update.ReadInPlace(template.Root, corpusRoot),
            file => kac.core.Update.Same(template.Root, corpusRoot, file));

        // A type being given up takes its own files with it, and they are deletions like any other. So
        // they join the plan rather than being carried beside it, and `--check` reports them too.
        if (adoption.Deleted.Count > 0)
            plan = plan with
            {
                Deleted = [.. plan.Deleted.Concat(adoption.Deleted)
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)]
            };

        if (plan.TemplateIsUnsound) return Unsound(plan, from);

        var taken = from + At(template.Commit);
        if (request.Check) return ReportCheck(plan, taken, request.DropType);

        kac.core.Update.Apply(plan, template.Root, corpusRoot);
        CorpusDescriptor.Stamp(corpusRoot, manifest.Version, today, template.Commit);

        // Only where a flag asked for it. `types:` is the corpus's own list, and rewriting it on a run
        // that changed nothing would reformat a block somebody laid out by hand.
        if (adoption.Account is not null && adoption.Types is { } adopted)
            CorpusDescriptor.SetTypes(corpusRoot, adopted);

        return ReportUpdate(plan, adoption, corpusRoot, taken, manifest.Version, today, request.DropType);
    }

    // A template this tool cannot read the whole of, in either of the two ways. Each names what the
    // template did rather than the count of it, because the fix is upstream and needs the paths.
    private static int Unsound(UpdatePlan plan, string from)
    {
        if (plan.Unclassified.Count > 0)
        {
            Stop($"update: {from} has a manifest that does not place its own tree. these files match no rule:");
            foreach (var path in plan.Unclassified) Out.ErrLine($"  {path}");
        }

        if (plan.UnknownCi.Count > 0)
            Stop($"update: {from} serves {string.Join(" and ", plan.UnknownCi)}, which this tool cannot "
                 + $"offer. it offers {string.Join(", ", CiSystem.All)}.");

        return 1;
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

    // What `--check` found: nothing, or the files that would change. The same discipline as
    // `generate --check`, and for the same reason. A pipeline says whether a corpus has fallen behind,
    // and never pushes.
    private static int ReportCheck(UpdatePlan plan, string taken, string? dropped)
    {
        Out.Line($"update: comparing this corpus against {taken}.");
        Aside(plan, dropped);

        if (!plan.Changes)
        {
            Out.Line($"update: in step, {plan.InStep} file(s) compared.");
            return 0;
        }

        Stop(HasWrites(plan)
            ? "this corpus is behind its framework. these would change:"
            : "this corpus is out of step with its framework:");
        Listed("WRITE, framework files this corpus holds differently", plan.Written.Select(f => f.To));
        Listed("SEED, files the corpus has none of", plan.Seeded.Select(f => f.To));
        Listed("DELETE, files the template has retired", plan.Deleted);
        Listed("UNSHARED, framework files this corpus holds and the template does not send",
            plan.Unshared);
        Out.ErrLine(plan.Unshared.Count > 0 && !HasWrites(plan)
            ? "an unshared file is a framework change made in the wrong tree. move it upstream, or say "
              + "the corpus owns it with a skip: entry. one under .plugin/ is a copy left behind by "
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

    // What the update did. Every file it touched, then the tally and what the descriptor now records.
    private static int ReportUpdate(UpdatePlan plan, Adoption adoption, string corpusRoot, string taken,
        int templateVersion, string today, string? dropped)
    {
        Out.Line($"update: taking the framework from {taken}.");
        foreach (var file in plan.Written) Out.Markup(Wrote(file.To));
        foreach (var file in plan.Seeded) Out.Markup(Wrote(file.To) + "  [grey](new)[/]");
        foreach (var rel in plan.Deleted) Out.Markup($"  [red]deleted[/]  {rel.EscapeMarkup()}");

        Aside(plan, dropped);
        if (adoption.Account is { } changed) Account(changed);

        // An update writes nothing for one of these, so saying so is the whole of what the tool can do.
        if (plan.Unshared.Count > 0)
        {
            Note($"update: {plan.Unshared.Count} file(s) sit where the framework's rules apply, and the "
                 + "template sends nothing to them:");
            foreach (var rel in plan.Unshared) Out.Line($"  {rel}");
        }

        Account($"update: wrote {plan.Written.Count}, seeded {plan.Seeded.Count}, deleted "
                + $"{plan.Deleted.Count}; {plan.InStep} already in step. recorded in .corpus.yaml as "
                + $"template version {templateVersion}, taken {today}.");
        Account("update: nothing is committed. `git diff` is the review step, and `git checkout` on a "
                + "file is how to decline one.");

        // Every overlay page may carry a generated block built from this corpus's own types, so the
        // copies above are only right once rebuilt against what this corpus holds. Regenerating here
        // makes a passing `generate --check` the update's postcondition.
        return Regenerate(corpusRoot);
    }

    // What the run stepped over and what it could still take. Neither is a change, and both are worth a
    // line: a skipped file is one nothing will refresh, and an unadopted type is one nothing will offer
    // again unless this says so.
    //
    // A type this run has just given up is left out. The corpus could indeed take it back, and saying so
    // beside the deletions reads as the tool arguing with the decision it was handed.
    private static void Aside(UpdatePlan plan, string? dropped)
    {
        if (plan.Skipped.Count > 0)
        {
            Account($"update: stepped over {plan.Skipped.Count} file(s) that .corpus.yaml claims:");
            foreach (var p in plan.Skipped) Out.Line($"  {p}");
        }

        if (plan.Declined > 0)
            Account($"update: withheld {plan.Declined} file(s) for types this corpus has not adopted.");

        if (plan.DeclinedCi > 0)
            Account($"update: withheld {plan.DeclinedCi} continuous integration starter(s) this corpus "
                    + "does not hold. which system builds it is not an update's to decide.");

        if (plan.DeclinedPlugin > 0)
            Account($"update: withheld {plan.DeclinedPlugin} plugin file(s) this corpus reads from "
                    + "elsewhere. .corpus.yaml says where under plugin.from.");

        var offered = plan.Offered.Where(t => !t.Equals(dropped, StringComparison.Ordinal)).ToList();
        if (offered.Count > 0)
            Account($"update: this template also declares {string.Join(", ", offered)}. take one with "
                    + "kac update --add-type <name>.");
    }

    // The corpus is loaded here rather than by the caller, because an update has just replaced the schema. A schema
    // this corpus cannot yet read is the one failure worth surviving: the files are already in place, and saying so is
    // more use than a stack trace over a half-finished tree.
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
                + "run kac validate to see what the corpus is missing, then kac generate.");
            return 1;
        }
    }
}
