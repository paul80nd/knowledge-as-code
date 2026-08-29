namespace kac.core;

// The `update` engine: what taking a newer framework into a corpus that already has one comes to.
//
// Deciding and doing stay apart, as they do either side of `New` and of `GeneratedFiles`. `Plan` names
// every file an update writes and every file it deletes, and touches nothing. `Apply` carries that plan
// out. So an update is decidable from two listings and a manifest, with no template on disk and no
// network, which is what lets the unit tests ask about a corpus nobody wrote.
//
// `docs/cli/update.md` is the reference for what each layer means and why a seed is left alone.

// What the invocation was told. `From`, `Ref` and `Path` are null where `.corpus.yaml` is to answer.
public sealed record UpdateRequest
{
    public string? From { get; init; }
    public string? Ref { get; init; }
    public string? Path { get; init; }

    // Compute the plan, print it, write nothing, and exit non-zero if anything would change.
    public bool Check { get; init; }

    // `cautious` or `full`, for this run only. Null where `update-policy:` is to answer.
    public string? Policy { get; init; }

    // The type to adopt, or the one to give up. Each is a run of its own.
    public string? AddType { get; init; }
    public string? DropType { get; init; }

    // Reaches the clone, and the one question an update asks: giving up a type leaves the pages that
    // name it holding a dead link, and `--drop-type` waits on an answer before it deletes anything.
    public bool Yes { get; init; }
}

// Which types the template declares, which this corpus holds, and the predicate telling a declined
// type's files from the rest.
//
// `Adopted` is null where the descriptor states no `types:`. That corpus has not declared, so it is held
// to everything and offered nothing.
public sealed record UpdateTypes(
    IReadOnlyList<string> Declared,
    IReadOnlyList<string>? Adopted,
    Func<string, bool> Declines)
{
    public IReadOnlyList<string> Offered =>
        Adopted is null
            ? []
            :
            [
                .. Declared.Where(t => !Adopted.Contains(t, StringComparer.Ordinal))
                    .OrderBy(t => t, StringComparer.Ordinal)
            ];
}

// What `--add-type` and `--drop-type` come to: the types this corpus holds afterwards, the files a drop
// takes with it, and the line saying what happened. `Problem` is set instead where the run cannot go on,
// and is then the only field worth reading.
public sealed record Adoption(
    IReadOnlyList<string>? Types,
    IReadOnlyList<string> Deleted,
    string? Account,
    string? Problem);

// What an update comes to. `Written` and `Seeded` are the files to copy, and are the only lists carrying
// where each file is read from. Everything else names a path in the corpus.
public sealed record UpdatePlan(
    IReadOnlyList<PlannedFile> Written, // overlay, whose authored halves differ
    IReadOnlyList<PlannedFile> Seeded,  // seed, absent from this corpus, or refreshed under `full`
    IReadOnlyList<string> Deleted,      // named by a tombstone, and still here
    IReadOnlyList<string> Skipped,      // named in `skip:`, with the reason the corpus gave
    IReadOnlyList<string> Unshared,     // sits where the rules call overlay, and the template sends no such file
    IReadOnlyList<string> Offered,      // types the template declares and this corpus has not adopted
    IReadOnlyList<string> Unclassified,
    IReadOnlyList<string> UnknownCi,
    int InStep,
    int Declined,       // withheld for a type this corpus has not adopted
    int DeclinedCi,     // a starter for a system that does not build this corpus
    int DeclinedPlugin) // the shared half of a plugin tree this corpus reads from elsewhere
{
    public IEnumerable<PlannedFile> Copies => Written.Concat(Seeded);

    // Whether this corpus is out of step with its framework. What `--check` exits on. An unshared file is
    // not something an update writes, and it fails a check all the same: it is a framework change made in
    // the wrong tree, and it would reach no other corpus.
    public bool Changes => Written.Count > 0 || Seeded.Count > 0 || Deleted.Count > 0 || Unshared.Count > 0;

    // The same two faults `NewPlan.TemplateIsUnsound` names, and stopped for the same reason.
    public bool TemplateIsUnsound => Unclassified.Count > 0 || UnknownCi.Count > 0;
}

public static class Update
{
    // What taking this template into this corpus comes to, decided from listings rather than from a
    // filesystem.
    //
    // `manifest` is the template's, because the boundary arrives with the files it describes: a corpus on
    // an older manifest would otherwise resolve the new tree by the old rules and silently skip whatever
    // those rules had not yet heard of.
    //
    // `policy` is `cautious` or `full`, already resolved between the descriptor and `--policy`. `same`
    // answers whether two copies of a file say the same thing, and `Same` below is what a command passes.
    //
    // A corpus naming `plugin.from` in its descriptor reads the plugin tree from somewhere else, so the
    // shared half of that tree is withheld rather than copied in. Its own manifest is a seed and still
    // arrives: the manifest names the plugin and is the corpus's to write.
    //
    // `readInPlace` is true where the corpus sits inside the repository serving its template. A file
    // whose destination is its source is then shared with that corpus rather than copied into it, which
    // is the arrangement `.schema/` and the travelling skills are in: the tool walks up for them, so the
    // corpus below holds none of its own and is missing nothing. `ReadInPlace` below decides it.
    public static UpdatePlan Plan(IReadOnlySet<string> templateFiles, IReadOnlySet<string> corpusFiles,
        Manifest manifest, CorpusDescriptor descriptor, UpdateTypes types, string policy,
        bool readInPlace, Func<PlannedFile, bool> same)
    {
        var owned = descriptor.Skipped.ToDictionary(s => s.Path, s => s.Reason, StringComparer.Ordinal);
        var full = policy.Equals(CorpusDescriptor.Full, StringComparison.Ordinal);

        var sent = new HashSet<string>(StringComparer.Ordinal);
        var written = new List<PlannedFile>();
        var seeded = new List<PlannedFile>();
        var skipped = new List<string>();
        var unclassified = new List<string>();
        var unknownCi = new List<string>();
        var inStep = 0;
        var declined = 0;
        var declinedCi = 0;
        var declinedPlugin = 0;

        foreach (var from in templateFiles.OrderBy(f => f, StringComparer.Ordinal))
        {
            if (manifest.Place(from) is not { } placement)
            {
                unclassified.Add(from);
                continue;
            }

            // A tombstone names a file the template no longer holds, so nothing here would ever match
            // one. The corpus-side loop below is what acts on `removed`.
            if (placement.Layer is Manifest.Withheld or Manifest.Removed) continue;

            // Ahead of `sent`, and deliberately. A corpus that adopted `plugin.from` while still holding
            // the copies it used to keep would otherwise carry them forever: `Merge` gives a corpus's own
            // file priority, so the leftovers win over the shared tree and no later change upstream ever
            // reaches that corpus's bundle. Left out of `sent`, each one surfaces below as a file the
            // corpus holds that nothing sends to.
            if (descriptor.PluginFrom is not null
                && placement.Layer == Manifest.Overlay
                && Bundler.InSourceTree(placement.Path))
            {
                declinedPlugin++;
                continue;
            }

            sent.Add(placement.Path);
            if (readInPlace && placement.Path.Equals(from, StringComparison.Ordinal)) continue;

            if (types.Declines(placement.Path))
            {
                declined++;
                continue;
            }

            // Written only where this corpus already holds one. `New.Plan` argues why a starter belongs
            // to the repository rather than to the framework, and an update may not introduce what `new`
            // was careful not to send.
            if (placement.Ci is { } ci)
            {
                if (!CiSystem.All.Contains(ci, StringComparer.Ordinal))
                {
                    if (!unknownCi.Contains(ci, StringComparer.Ordinal)) unknownCi.Add(ci);
                    continue;
                }

                if (!corpusFiles.Contains(placement.Path))
                {
                    declinedCi++;
                    continue;
                }
            }

            if (owned.TryGetValue(placement.Path, out var reason))
            {
                skipped.Add(Owned(placement.Path, reason));
                continue;
            }

            var here = corpusFiles.Contains(placement.Path);
            var file = new PlannedFile(from, placement.Path, placement.Layer);

            // A seed is the corpus's own words from the moment it lands, so `cautious` asks only whether
            // the corpus has one. `full` holds it to the template and hands the reconciliation to the
            // diff.
            if (placement.Layer == Manifest.Seed && here && !full)
            {
                inStep++;
                continue;
            }

            if (here && same(file))
            {
                inStep++;
                continue;
            }

            (placement.Layer == Manifest.Seed ? seeded : written).Add(file);
        }

        var deleted = new List<string>();
        foreach (var rel in corpusFiles.OrderBy(r => r, StringComparer.Ordinal))
        {
            if (!Tombstoned(manifest, rel)) continue;
            if (owned.TryGetValue(rel, out var reason)) skipped.Add(Owned(rel, reason));
            else deleted.Add(rel);
        }

        // The other direction, and the one nothing else covers. A file the corpus keeps where the rules
        // call the area overlay, that no template file was sent to, is a framework change written in the
        // wrong tree: it reaches no other corpus, and nothing in this one reads as though it is missing.
        var received = AsReceived(manifest);
        var unshared = corpusFiles
            .Where(rel => !sent.Contains(rel))
            .Where(rel => received.Resolve(rel) == Manifest.Overlay)
            .OrderBy(rel => rel, StringComparer.Ordinal)
            .ToList();

        skipped.Sort(StringComparer.Ordinal);
        unknownCi.Sort(StringComparer.Ordinal);
        return new UpdatePlan(written, seeded, deleted, skipped, unshared, types.Offered, unclassified,
            unknownCi, inStep, declined, declinedCi, declinedPlugin);
    }

    // The same rules read from the corpus's side: each rule's patterns rewritten to where its files land,
    // in the order they were written. A corpus file has to be sorted by the rule that sent it, and only
    // the order says which that was: `.plugin/.claude-plugin/plugin.json` is a seed carved out ahead of
    // the overlay claiming the folder around it.
    private static Manifest AsReceived(Manifest manifest) => new()
    {
        Rules = [.. manifest.Rules.Select(r => new ManifestRule([.. Manifest.Destinations(r)], r.Layer))]
    };

    // Where to read the template from, given what `--from` or `upstream.url` said.
    //
    // A relative path is resolved against the corpus root and never against the working directory. Every
    // other verb answers the same question wherever inside a corpus it is run, and one that did not would
    // work at the root and fail a folder down. `upstream.url` is a value the corpus wrote about itself, so
    // the corpus is what it is relative to.
    //
    // Anything that does not resolve to a folder is handed back exactly as it was given, which is what a
    // URL is. A path a platform cannot parse at all is the same case.
    public static string TemplatePath(string from, string corpusRoot)
    {
        try
        {
            var below = Path.GetFullPath(from, corpusRoot);
            return Directory.Exists(below) ? below : from;
        }
        catch (ArgumentException)
        {
            return from;
        }
    }

    // Whether the corpus sits inside the repository serving its template, which is what makes a file
    // authored at the template's root reachable from the corpus without a copy.
    public static bool ReadInPlace(string templateRoot, string corpusRoot)
    {
        var template = Path.GetFullPath(templateRoot).TrimEnd(Path.DirectorySeparatorChar);
        var corpus = Path.GetFullPath(corpusRoot).TrimEnd(Path.DirectorySeparatorChar);
        return corpus.StartsWith(template + Path.DirectorySeparatorChar, StringComparison.Ordinal);
    }

    // Whether a tombstone names this path. The rules are read from the corpus's side, through the
    // destinations `to:` sends each pattern to, because the file is still in the corpus and no longer in
    // the template.
    private static bool Tombstoned(Manifest manifest, string rel) =>
        manifest.Rules
            .Where(r => r.Layer == Manifest.Removed)
            .SelectMany(Manifest.Destinations)
            .Any(pattern => Glob.IsMatch(rel, pattern));

    // A skipped path beside the reason the corpus gave for owning it, which is written for whoever reads
    // the run rather than for the tool.
    private static string Owned(string rel, string? reason) => reason is null ? rel : $"{rel}  ({reason})";

    // Carry the plan out. The plan decided all of it, so this asks the template nothing beyond the bytes
    // and the mode of each file it was told to copy, and it commits and stages nothing. `declined` reaches
    // the seed pages alone, and `SeedLinks` says why.
    public static void Apply(UpdatePlan plan, string templateRoot, string corpusRoot,
        IReadOnlySet<string> declined)
    {
        foreach (var file in plan.Copies) SeedLinks.Receive(file, templateRoot, corpusRoot, declined);

        foreach (var rel in plan.Deleted) File.Delete(Path.Combine(corpusRoot, rel));
    }

    // What adopting or giving up a type comes to, decided from the listing and the template's schema.
    //
    // Adopting and giving up are asymmetric, and the asymmetry is the design. Adopting is the same
    // machinery as any other write, pointed at one type's files. Giving up refuses where the folder holds
    // records, because deleting a record is deleting knowledge and everything else in a corpus exists to
    // serve them.
    //
    // A run naming neither flag answers with the types the corpus already holds, so the caller has one
    // list to read whether or not it asked for a change.
    public static Adoption Adopt(IReadOnlySet<string> corpusFiles, CorpusDescriptor descriptor, Schema schema,
        IReadOnlyList<string> declared, string? add, string? drop)
    {
        var held = descriptor.Types;
        if (add is null && drop is null) return new Adoption(held, [], null, null);

        // Reading adoption off the folders is what a corpus that has declared nothing gets, and there is
        // no list there to add a name to. Declaring is the corpus's own step, and `validate` is what holds
        // it to the declaration afterwards.
        if (held is null)
            return Refuse("update: this corpus states no `types:`, so there is no list to change. declare "
                          + "what it has adopted first.");

        if (add is { } adding)
        {
            if (!declared.Contains(adding, StringComparer.Ordinal))
                return Refuse($"update: this template declares no type called '{adding}'. it declares "
                              + $"{string.Join(", ", declared)}.");

            if (held.Contains(adding, StringComparer.Ordinal))
                return Refuse($"update: '{adding}' is already adopted, so there is nothing to add.");

            return new Adoption(
                [.. held.Append(adding).OrderBy(t => t, StringComparer.Ordinal)], [],
                $"update: adopted {adding}. its schema, root page and template arrived with this run.", null);
        }

        var dropping = drop!;
        if (!held.Contains(dropping, StringComparer.Ordinal))
            return Refuse($"update: '{dropping}' is not adopted, so there is nothing to give up.");

        if (!schema.ByFolder.TryGetValue(dropping, out var type))
            return Refuse($"update: this template declares no type called '{dropping}', so there is no "
                          + "folder to read. take the name out of `types:` by hand.");

        var folder = type.Folder;
        var records = RecordsUnder(corpusFiles, folder);
        if (records.Count > 0)
            return Refuse($"update: {folder}/ holds {records.Count} record(s), and deleting a record is "
                          + $"deleting knowledge. delete them yourself and run this again, or leave "
                          + $"'{dropping}' adopted.");

        return new Adoption(
            [.. held.Where(t => !t.Equals(dropping, StringComparison.Ordinal))],
            FilesOf(corpusFiles, type, dropping),
            $"update: gave up {dropping}. its schema, its page and {folder}/ went with it.", null);

        static Adoption Refuse(string message) => new(null, [], null, message);
    }

    // The records a type's folder holds, which is the whole of what stops `--drop-type`. Read off the
    // listing rather than loaded as documents, so the count is right even in a corpus the schema no
    // longer loads. An underscore opens the two files a type folder holds that are not records.
    public static IReadOnlyList<string> RecordsUnder(IReadOnlySet<string> corpusFiles, string folder) =>
    [
        .. corpusFiles
            .Where(rel => rel.StartsWith(folder + "/", StringComparison.Ordinal))
            .Where(rel => rel.EndsWith(".md", StringComparison.Ordinal))
            .Where(rel => !Path.GetFileName(rel).StartsWith('_'))
            .OrderBy(rel => rel, StringComparer.Ordinal)
    ];

    // Everything a corpus holds for one type: the schema file the tool judges it by, the root page, and
    // the folder. What `--drop-type` deletes once it has established the folder holds no record.
    public static IReadOnlyList<string> FilesOf(IReadOnlySet<string> corpusFiles, TypeSchema type, string name)
    {
        var folder = type.Folder;
        var files = new List<string> { $".schema/{name}.yaml" };
        if (!string.IsNullOrEmpty(type.Page)) files.Add(type.Page);
        files.AddRange(corpusFiles.Where(rel => rel.StartsWith(folder + "/", StringComparison.Ordinal)));

        return
        [
            .. files.Where(corpusFiles.Contains)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(f => f, StringComparer.Ordinal)
        ];
    }

    // Whether the template's copy of a file and the corpus's copy say the same thing. LF-normalised, so a
    // working copy checked out with CRLF never reads as drift, and compared on the authored half alone.
    // See `Generator.Authored` for why an overlay page may hold a different table in each corpus and
    // still be in step.
    //
    // The template's side is unlinked before the comparison, so what a corpus is held to under `full` is
    // what `new` would have written. Compared against the template as authored, a corpus that declined
    // types would read as behind on every seed page it holds, and a full update would put back the links
    // its own `types:` says it cannot follow.
    internal static bool Same(string templateRoot, string corpusRoot, PlannedFile file,
        IReadOnlySet<string> declined)
    {
        var sent = Files.ReadLf(Path.Combine(templateRoot, file.From));
        if (SeedLinks.Reaches(file)) sent = SeedLinks.Unlinked(sent, declined);
        return Generator.Authored(sent)
               == Generator.Authored(Files.ReadLf(Path.Combine(corpusRoot, file.To)));
    }

    // Every tracked (and not-ignored) file a corpus holds, relative and forward-slashed. `GitFiles` falls
    // back to a walk where git cannot answer, which is a corpus outside version control.
    internal static IReadOnlySet<string> Listing(string root) =>
        new HashSet<string>(GitFiles.Tracked(root) ?? GitFiles.Walk(root, "*", ".git"), StringComparer.Ordinal);
}
