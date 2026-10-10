using System.Diagnostics.CodeAnalysis;

namespace kac.core;

// The `update` engine. `Plan` names every file an update writes and deletes, and touches nothing. `Apply`
// carries it out. `docs/cli/update.md` says what each layer means.

// What the invocation was told. `From`, `Ref` and `Path` are null where `.corpus.yaml` is to answer.
public sealed record UpdateRequest
{
    public string? From { get; init; }
    public string? Ref { get; init; }
    public string? Path { get; init; }

    // Write nothing, and exit non-zero if anything would change.
    public bool Check { get; init; }

    // `cautious` or `full`, for this run only. Null where `update-policy:` is to answer.
    public string? Policy { get; init; }

    public string? AddType { get; init; }
    public string? DropType { get; init; }

    // Also confirms `--drop-type`, which otherwise waits on an answer before it deletes anything.
    public bool Yes { get; init; }
}

public sealed record UpdateSettings(string From, string Policy);

// Exactly one of the two is set.
public sealed record Settled(UpdateSettings? Settings, string? Problem)
{
    [MemberNotNullWhen(true, nameof(Problem))]
    [MemberNotNullWhen(false, nameof(Settings))]
    public bool Failed => Problem is not null;
}

// `Adopted` is null where the descriptor states no `types:`, so the corpus is held to everything and offered
// nothing.
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

// `Problem` is set where the run cannot go on, and is then the only field worth reading.
public sealed record Adoption(
    IReadOnlyList<string>? Types,
    IReadOnlyList<string> Deleted,
    string? Account,
    string? Problem);

// Null wherever the file carries no id, which is every file that is not a record. Delegates, so a plan can
// state two ids with no tree on disk. `Update.IdAt` is what a command passes.
public sealed record RecordIds(Func<string, string?> Sent, Func<string, string?> Held);

// `Written` and `Seeded` are the files to copy, and the only lists with a source path.
public sealed record UpdatePlan(
    IReadOnlyList<PlannedFile> Written, // overlay, whose authored halves differ
    IReadOnlyList<PlannedFile> Seeded,  // seed, absent from this corpus, or refreshed under `full`
    IReadOnlyList<string> Deleted,      // named by a tombstone, and still here
    IReadOnlyList<string> Skipped,      // named in `skip:`, with the reason the corpus gave
    IReadOnlyList<string> Unshared,     // sits where the rules call overlay, and the template sends no such file
    IReadOnlyList<string> Offered,      // types the template declares and this corpus has not adopted
    TemplateFaults Faults,
    int InStep,
    int Declined,       // withheld for a type this corpus has not adopted
    int DeclinedCi,     // a starter for a system that does not build this corpus
    int DeclinedPlugin) // the shared half of a plugin tree this corpus reads from elsewhere
{
    public IEnumerable<PlannedFile> Copies => Written.Concat(Seeded);

    // What `--check` exits on. An unshared file fails it too, because it is a framework change in the wrong tree.
    public bool Changes => Written.Count > 0 || Seeded.Count > 0 || Deleted.Count > 0 || Unshared.Count > 0;
}

public static class Update
{
    // Judged before the corpus is read, so a descriptor that does not parse cannot throw over a run this refuses.
    public static string? Contradiction(UpdateRequest request) =>
        request.AddType is not null && request.DropType is not null
            ? "update: --add-type and --drop-type change the same list. ask for one."
            : null;

    public static Settled Settle(UpdateRequest request, CorpusDescriptor descriptor)
    {
        var from = request.From ?? descriptor.UpstreamUrl;
        if (from is null)
            return new Settled(null, "update: this corpus names no template, so there is nothing to take. "
                                     + "pass --from, or set upstream.url in .corpus.yaml.");

        var policy = request.Policy ?? descriptor.UpdatePolicy;
        if (!CorpusDescriptor.Policies.Contains(policy, StringComparer.Ordinal))
            return new Settled(null, $"update: '{policy}' is not an update policy. it is "
                                     + $"{string.Join(" or ", CorpusDescriptor.Policies)}.");

        return new Settled(new UpdateSettings(from, policy), null);
    }

    // Decided from listings, not a filesystem. `same` answers whether two copies agree, and `ids` what record a file
    // is, because only a record may move. `readInPlace` means the corpus sits inside its template's repository, so a
    // file whose destination is its source is shared, not copied.
    public static UpdatePlan Plan(IReadOnlySet<string> templateFiles, IReadOnlySet<string> corpusFiles,
        Manifest manifest, CorpusDescriptor descriptor, UpdateTypes types, string policy,
        bool readInPlace, Func<PlannedFile, bool> same, RecordIds ids)
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

            // A tombstone matches nothing the template holds. The corpus-side loop below acts on `removed`.
            if (placement.Layer is Manifest.Withheld or Manifest.Removed) continue;

            // Ahead of `sent`, so a corpus that adopted `plugin.from` while holding old copies sees each one
            // reported as unshared. `Merge` would otherwise let the leftovers win over the shared tree.
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

            // Written only where the corpus already holds one, because `new` does not send a starter. See `New.Plan`.
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

            // A seeded record filed under a category folder is that record, moved, and in step under either policy. A
            // second copy would fail `id-unique`, and the template's bytes a folder down would break its `../` links.
            if (!here && placement.Layer == Manifest.Seed
                      && Filed(from, placement.Path, corpusFiles, ids) is { } filed)
            {
                sent.Add(filed);
                inStep++;
                continue;
            }

            var file = new PlannedFile(from, placement.Path, placement.Layer);

            // A seed is the corpus's own once it lands, so `cautious` asks only whether it is there.
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

        // A file in an overlay area that no template file was sent to is a framework change in the wrong tree.
        var received = AsReceived(manifest);
        var unshared = corpusFiles
            .Where(rel => !sent.Contains(rel))
            .Where(rel => received.Resolve(rel) == Manifest.Overlay)
            .OrderBy(rel => rel, StringComparer.Ordinal)
            .ToList();

        skipped.Sort(StringComparer.Ordinal);
        return new UpdatePlan(written, seeded, deleted, skipped, unshared, types.Offered,
            TemplateFaults.Of(unclassified, unknownCi), inStep, declined, declinedCi, declinedPlugin);
    }

    // Rules in their written order, because only the order says which rule sent a file:
    // `.plugin/.claude-plugin/plugin.json` is a seed carved out ahead of the overlay around it.
    private static Manifest AsReceived(Manifest manifest) => new()
    {
        Rules = [.. manifest.Rules.Select(r => new ManifestRule([.. Manifest.Destinations(r)], r.Layer))]
    };

    // A relative path resolves against the corpus root, never the working directory. Anything that is not a
    // folder is returned as given, which is what a URL is.
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

    public static bool ReadInPlace(string templateRoot, string corpusRoot)
    {
        var template = Path.GetFullPath(templateRoot).TrimEnd(Path.DirectorySeparatorChar);
        var corpus = Path.GetFullPath(corpusRoot).TrimEnd(Path.DirectorySeparatorChar);
        return corpus.StartsWith(template + Path.DirectorySeparatorChar, StringComparison.Ordinal);
    }

    // Where the corpus filed a seeded record, or null. Searched by id within the type folder only, and compared
    // case-insensitively as `id-unique` compares, so the guard is as wide as that rule.
    private static string? Filed(string from, string to, IReadOnlySet<string> corpusFiles, RecordIds ids)
    {
        // Ahead of the read, so a seed at the root is not parsed on every `--check`.
        var cut = to.IndexOf('/', StringComparison.Ordinal);
        if (cut <= 0) return null;

        if (ids.Sent(from) is not { } id) return null;

        return RecordsUnder(corpusFiles, to[..cut])
            .FirstOrDefault(rel => id.Equals(ids.Held(rel), StringComparison.OrdinalIgnoreCase));
    }

    // Read through the destinations `to:` sends each pattern to, because the file is in the corpus and no longer
    // in the template.
    private static bool Tombstoned(Manifest manifest, string rel) =>
        manifest.Rules
            .Where(r => r.Layer == Manifest.Removed)
            .SelectMany(Manifest.Destinations)
            .Any(pattern => Glob.IsMatch(rel, pattern));

    private static string Owned(string rel, string? reason) => reason is null ? rel : $"{rel}  ({reason})";

    // Commits and stages nothing. `declined` reaches the seed pages alone. `SeedLinks` says why.
    public static void Apply(UpdatePlan plan, string templateRoot, string corpusRoot,
        IReadOnlySet<string> declined)
    {
        foreach (var file in plan.Copies) SeedLinks.Receive(file, templateRoot, corpusRoot, declined);

        foreach (var rel in plan.Deleted) File.Delete(Path.Combine(corpusRoot, rel));
    }

    // Giving up a type refuses where its folder holds records, because deleting a record is deleting knowledge.
    // With neither flag, returns the types the corpus already holds.
    public static Adoption Adopt(IReadOnlySet<string> corpusFiles, CorpusDescriptor descriptor, Schema schema,
        IReadOnlyList<string> declared, string? add, string? drop)
    {
        var held = descriptor.Types;
        if (add is null && drop is null) return new Adoption(held, [], null, null);

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

        // Unreachable with `drop` unset. Asked again so the compiler knows.
        if (drop is not { } dropping) return new Adoption(held, [], null, null);

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

    // Read off the listing, not loaded, so the count is right even where the schema no longer loads. An
    // underscore marks a type folder's two non-record files.
    public static IReadOnlyList<string> RecordsUnder(IReadOnlySet<string> corpusFiles, string folder) =>
    [
        .. corpusFiles
            .Where(rel => rel.StartsWith(folder + "/", StringComparison.Ordinal))
            .Where(rel => rel.EndsWith(".md", StringComparison.Ordinal))
            .Where(rel => !Path.GetFileName(rel).StartsWith('_'))
            .OrderBy(rel => rel, StringComparer.Ordinal)
    ];

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

    // LF-normalised and compared on the authored half. See `Markers.Authored`. The template's side is unlinked
    // first, so `full` holds a corpus to what `new` would have written.
    internal static bool Same(string templateRoot, string corpusRoot, PlannedFile file,
        IReadOnlySet<string> declined)
    {
        var sent = Files.ReadLf(Path.Combine(templateRoot, file.From));
        if (SeedLinks.Reaches(file)) sent = SeedLinks.Unlinked(sent, declined);
        return Markers.Authored(sent)
               == Markers.Authored(Files.ReadLf(Path.Combine(corpusRoot, file.To)));
    }

    // Read with the document parser, so `update` asks the same question `validate` does. Frontmatter that does
    // not parse carries no id here.
    internal static string? IdAt(string root, string rel, Schema schema)
    {
        if (!rel.EndsWith(".md", StringComparison.Ordinal)) return null;

        var path = Path.Combine(root, rel);
        return File.Exists(path) ? Doc.Parse(rel, Files.ReadLf(path), schema)?.FrontScalar("id") : null;
    }

    // `GitFiles` falls back to a walk where git cannot answer.
    internal static IReadOnlySet<string> Listing(string root) =>
        new HashSet<string>(GitFiles.Tracked(root) ?? GitFiles.Walk(root, "*", ".git"), StringComparer.Ordinal);
}
