using System.Diagnostics.CodeAnalysis;

namespace kac.core;

// The `update` engine. `Plan` names every file an update writes and deletes, and touches nothing. `Apply`
// carries it out. `docs/use/cli/update.md` says what each layer means.

/// <summary>
/// What the invocation was told. <c>From</c>, <c>Ref</c> and <c>Path</c> are null where <c>.corpus.yaml</c> is to
/// answer.
/// </summary>
public sealed record UpdateRequest
{
    public string? From { get; init; }
    public string? Ref { get; init; }
    public string? Path { get; init; }

    /// <summary>Write nothing, and exit non-zero if anything would change.</summary>
    public bool Check { get; init; }

    /// <summary>
    /// <c>cautious</c> or <c>full</c>, for this run only. Null where <c>update-policy:</c> is to answer.
    /// </summary>
    public string? Policy { get; init; }

    public string? AddType { get; init; }
    public string? DropType { get; init; }

    /// <summary>
    /// Also confirms <c>--drop-type</c>, which otherwise waits on an answer before it deletes anything.
    /// </summary>
    public bool Yes { get; init; }
}

public sealed record UpdateSettings(string From, string Policy);

/// <summary>Exactly one of the two is set.</summary>
public sealed record Settled(UpdateSettings? Settings, string? Problem)
{
    [MemberNotNullWhen(true, nameof(Problem))]
    [MemberNotNullWhen(false, nameof(Settings))]
    public bool Failed => Problem is not null;
}

/// <summary>
/// <c>Adopted</c> is null where the descriptor states no <c>types:</c>, so the corpus is held to everything and offered
/// nothing.
/// </summary>
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

/// <summary>
/// The types the corpus adopts after this run. <c>Types</c> is null where the descriptor states no <c>types:</c>, and
/// <c>Account</c> is null where no flag changed the list.
/// </summary>
public sealed record Adoption(
    IReadOnlyList<string>? Types,
    IReadOnlyList<string> Deleted,
    string? Account);

/// <summary>Exactly one of the two is set.</summary>
public sealed record AdoptionAsked(Adoption? Adoption, string? Problem)
{
    [MemberNotNullWhen(true, nameof(Problem))]
    [MemberNotNullWhen(false, nameof(Adoption))]
    public bool Failed => Problem is not null;
}

/// <summary>
/// Null wherever the file carries no id, which is every file that is not a record. Delegates, so a plan can state two
/// ids with no tree on disk. <c>Update.IdAt</c> is what a command passes.
/// </summary>
public sealed record RecordIds(Func<string, string?> Sent, Func<string, string?> Held);

/// <summary><c>Written</c> and <c>Seeded</c> are the files to copy, and the only lists with a source path.</summary>
/// <param name="Written">Overlay, whose authored halves differ.</param>
/// <param name="Seeded">Seed, absent from this corpus, or refreshed under <c>full</c>.</param>
/// <param name="Deleted">Named by a tombstone, and still here.</param>
/// <param name="Skipped">Named in <c>skip:</c>, with the reason the corpus gave.</param>
/// <param name="Unshared">Sits where the rules call overlay, and the template sends no such file.</param>
/// <param name="Offered">Types the template declares and this corpus has not adopted.</param>
/// <param name="Declined">Withheld for a type this corpus has not adopted.</param>
/// <param name="DeclinedCiSystems">The <c>ci:</c> system of each file withheld for being absent here.</param>
/// <param name="DeclinedPlugin">The shared half of a plugin tree this corpus reads from elsewhere.</param>
public sealed record UpdatePlan(
    IReadOnlyList<PlannedFile> Written,
    IReadOnlyList<PlannedFile> Seeded,
    IReadOnlyList<string> Deleted,
    IReadOnlyList<string> Skipped,
    IReadOnlyList<string> Unshared,
    IReadOnlyList<string> Offered,
    TemplateFaults Faults,
    int InStep,
    int Declined,
    IReadOnlyList<string> DeclinedCiSystems,
    int DeclinedPlugin)
{
    public IEnumerable<PlannedFile> Copies => Written.Concat(Seeded);

    /// <summary>
    /// What <c>--check</c> exits on. An unshared file fails it too, because it is a framework change in the wrong tree.
    /// </summary>
    public bool Changes => Written.Count > 0 || Seeded.Count > 0 || Deleted.Count > 0 || Unshared.Count > 0;
}

public static class Update
{
    /// <summary>
    /// Judged before the corpus is read, so a descriptor that does not parse cannot throw over a run this refuses.
    /// </summary>
    public static string? Contradiction(UpdateRequest request) =>
        request.AddType is not null && request.DropType is not null
            ? "update: --add-type and --drop-type change the same list. Ask for one."
            : null;

    public static Settled Settle(UpdateRequest request, CorpusDescriptor descriptor)
    {
        var from = request.From ?? descriptor.UpstreamUrl;
        if (from is null)
            return new Settled(null, "update: this corpus names no template, so there is nothing to take. "
                                     + "Pass --from, or set upstream.url in .corpus.yaml.");

        var policy = request.Policy ?? descriptor.UpdatePolicy;
        if (!CorpusDescriptor.Policies.Contains(policy, StringComparer.Ordinal))
            return new Settled(null, $"update: '{policy}' is not an update policy. It is "
                                     + $"{string.Join(" or ", CorpusDescriptor.Policies)}.");

        return new Settled(new UpdateSettings(from, policy), null);
    }

    /// <summary>
    /// Null where nothing was withheld. Names each system once, in the order <c>CiSystem.All</c> gives, and never the
    /// kind of file.
    /// </summary>
    public static string? CiWithheld(UpdatePlan plan)
    {
        if (plan.DeclinedCiSystems.Count == 0) return null;

        var systems = CiSystem.All.Where(s => plan.DeclinedCiSystems.Contains(s, StringComparer.Ordinal));
        return $"update: withheld {Counted.Of(plan.DeclinedCiSystems.Count, "file")} for "
               + $"{string.Join(" and ", systems)} that this corpus does not hold. Which system builds it is not an "
               + "update's to decide.";
    }

    /// <summary>
    /// Decided from listings, not a filesystem. <c>same</c> answers whether two copies agree, and <c>ids</c> what
    /// record a file is, because only a record may move. <c>readInPlace</c> means the corpus sits inside its template's
    /// repository, so a file whose destination is its source is shared, not copied.
    /// </summary>
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
        var declinedCi = new List<string>();
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
                    declinedCi.Add(ci);
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

    /// <summary>
    /// Rules in their written order, because only the order says which rule sent a file:
    /// <c>.plugin/.claude-plugin/plugin.json</c> is a seed carved out ahead of the overlay around it.
    /// </summary>
    private static Manifest AsReceived(Manifest manifest) => new()
    {
        Rules = [.. manifest.Rules.Select(r => new ManifestRule([.. Manifest.Destinations(r)], r.Layer))]
    };

    /// <summary>
    /// A relative path resolves against the corpus root, never the working directory. Anything that is not a folder is
    /// returned as given, which is what a URL is.
    /// </summary>
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

    /// <summary>
    /// Where the corpus filed a seeded record, or null. Searched by id within the type folder only, and compared
    /// case-insensitively as <c>id-unique</c> compares, so the guard is as wide as that rule.
    /// </summary>
    private static string? Filed(string from, string to, IReadOnlySet<string> corpusFiles, RecordIds ids)
    {
        // Ahead of the read, so a seed at the root is not parsed on every `--check`.
        var cut = to.IndexOf('/', StringComparison.Ordinal);
        if (cut <= 0) return null;

        if (ids.Sent(from) is not { } id) return null;

        return RecordsUnder(corpusFiles, to[..cut])
            .FirstOrDefault(rel => id.Equals(ids.Held(rel), StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Read through the destinations <c>to:</c> sends each pattern to, because the file is in the corpus and no longer
    /// in the template.
    /// </summary>
    private static bool Tombstoned(Manifest manifest, string rel) =>
        manifest.Rules
            .Where(r => r.Layer == Manifest.Removed)
            .SelectMany(Manifest.Destinations)
            .Any(pattern => Glob.IsMatch(rel, pattern));

    private static string Owned(string rel, string? reason) => reason is null ? rel : $"{rel}  ({reason})";

    /// <summary>
    /// Commits and stages nothing. <c>declined</c> and <c>owner</c> reach the seed pages alone. <c>SeedLinks</c> says
    /// why. Returns the pages it took a link out of, and the records it wrote <c>owner</c> into.
    /// </summary>
    public static Receipt Apply(UpdatePlan plan, string templateRoot, string corpusRoot, DeclinedNames declined,
        string owner)
    {
        var received = SeedLinks.ReceiveAll(plan.Copies, templateRoot, corpusRoot, declined, owner);

        foreach (var rel in plan.Deleted) File.Delete(Path.Combine(corpusRoot, rel));
        return received;
    }

    /// <summary>
    /// Giving up a type refuses where its folder holds records, because deleting a record is deleting knowledge. With
    /// neither flag, returns the types the corpus already holds.
    /// </summary>
    public static AdoptionAsked Adopt(IReadOnlySet<string> corpusFiles, CorpusDescriptor descriptor, Schema schema,
        IReadOnlyList<string> declared, string? add, string? drop)
    {
        var held = descriptor.Types;
        if (add is null && drop is null) return Accept(new Adoption(held, [], null));

        if (held is null)
            return Refuse("update: this corpus states no `types:`, so there is no list to change. Declare "
                          + "what it has adopted first.");

        if (add is not null)
        {
            var adding = Adding(add);
            if (adding.Count == 0)
                return Refuse("update: --add-type names no type. Name one, or several separated by commas.");

            if (adding.Where(t => !declared.Contains(t, StringComparer.Ordinal)).ToList() is { Count: > 0 } unknown)
                return Refuse($"update: this template declares no type called "
                              + $"{string.Join(" or ", unknown.Select(t => $"'{t}'"))}. It declares "
                              + $"{string.Join(", ", declared)}.");

            if (adding.Where(t => held.Contains(t, StringComparer.Ordinal)).ToList() is { Count: > 0 } already)
                return Refuse(adding.Count == 1
                    ? $"update: '{already[0]}' is already adopted, so there is nothing to add."
                    : $"update: this corpus already adopts {Series(already)}. Name only the types it does not "
                      + "adopt.");

            return Accept(new Adoption(
                [.. held.Concat(adding).OrderBy(t => t, StringComparer.Ordinal)], [],
                adding.Count == 1
                    ? $"update: adopted {adding[0]}. Its schema, root page and template arrived with this run."
                    : $"update: adopted {Series(adding)}. Their schemas, root pages and templates arrived with "
                      + "this run."));
        }

        // Unreachable with `drop` unset. Asked again so the compiler knows.
        if (drop is not { } dropping) return Accept(new Adoption(held, [], null));

        if (!held.Contains(dropping, StringComparer.Ordinal))
            return Refuse($"update: '{dropping}' is not adopted, so there is nothing to give up.");

        if (!schema.ByFolder.TryGetValue(dropping, out var type))
            return Refuse($"update: this template declares no type called '{dropping}', so there is no "
                          + "folder to read. Take the name out of `types:` by hand.");

        var folder = type.Folder;
        var records = RecordsUnder(corpusFiles, folder);
        if (records.Count > 0)
            return Refuse($"update: {folder}/ holds {Counted.Of(records.Count, "record")}, and deleting a record is "
                          + $"deleting knowledge. Delete {(records.Count == 1 ? "it" : "them")} yourself and run "
                          + $"this again, or leave '{dropping}' adopted.");

        return Accept(new Adoption(
            [.. held.Where(t => !t.Equals(dropping, StringComparison.Ordinal))],
            FilesOf(corpusFiles, type, dropping),
            $"update: gave up {dropping}. Its schema, its page and {folder}/ went with it."));

        static AdoptionAsked Accept(Adoption adoption) => new(adoption, null);
        static AdoptionAsked Refuse(string message) => new(null, message);
    }

    /// <summary>
    /// The types <c>--add-type</c> names, comma-separated as <c>new --types</c> takes them, each once and sorted.
    /// </summary>
    public static IReadOnlyList<string> Adding(string add) =>
    [
        .. add.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
    ];

    private static string Series(IReadOnlyList<string> items) =>
        items.Count == 1 ? items[0] : string.Join(", ", items.Take(items.Count - 1)) + " and " + items[^1];

    /// <summary>
    /// Read off the listing, not loaded, so the count is right even where the schema no longer loads. An underscore
    /// marks a type folder's two non-record files.
    /// </summary>
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

    /// <summary>
    /// LF-normalised and compared on the authored half. See <c>Markers.Authored</c>. The template's side is unlinked
    /// first, so <c>full</c> holds a corpus to what <c>new</c> would have written. A seeded record is compared under
    /// the owner the corpus gave it, so the owner alone never makes one differ.
    /// </summary>
    internal static bool Same(string templateRoot, string corpusRoot, PlannedFile file,
        DeclinedNames declined)
    {
        var sent = Files.ReadLf(Path.Combine(templateRoot, file.From));
        var held = Files.ReadLf(Path.Combine(corpusRoot, file.To));
        if (SeedLinks.Reaches(file)) sent = SeedLinks.Unlinked(sent, declined);
        if (SeedOwner.Reaches(file) && SeedOwner.Held(held) is { } owner) sent = SeedOwner.Owned(sent, owner);
        return Markers.Authored(sent) == Markers.Authored(held);
    }

    /// <summary>
    /// Read with the document parser, so <c>update</c> asks the same question <c>validate</c> does. Frontmatter that
    /// does not parse carries no id here.
    /// </summary>
    internal static string? IdAt(string root, string rel, Schema schema)
    {
        if (!rel.EndsWith(".md", StringComparison.Ordinal)) return null;

        var path = Path.Combine(root, rel);
        return File.Exists(path) ? Doc.Parse(rel, Files.ReadLf(path), schema)?.FrontScalar("id") : null;
    }

    /// <summary><c>GitFiles</c> falls back to a walk where git cannot answer.</summary>
    internal static IReadOnlySet<string> Listing(string root) =>
        new HashSet<string>(GitFiles.Tracked(root) ?? GitFiles.Walk(root, "*", ".git"), StringComparer.Ordinal);
}
