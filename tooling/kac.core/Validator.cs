using YamlDotNet.RepresentationModel;

namespace kac.core;

public static class Validator
{
    /// <summary>
    /// The whole of <c>validate</c>, so no caller runs a subset and believes it ran the lot. <c>today</c> is the day a
    /// rule written against <c>today()</c> judges by (see <c>docs/reference/expressions.md</c>). A null
    /// <c>standings</c> reports no import's freshness, rather than every import as current.
    /// </summary>
    public static List<Finding> CheckAll(
        LoadedCorpus corpus, DateOnly today, IReadOnlyList<ImportStanding>? standings = null)
    {
        var (schema, tree) = (corpus.Schema, corpus.Tree);
        var findings = new List<Finding>();

        // The schema first, because it decides how every document below is read.
        SchemaChecks.Check(schema, findings);

        var fillable = Fillable(corpus);

        foreach (var doc in corpus.Docs)
            CheckDocument(doc, schema, tree, findings, today, fillable: fillable);

        // A file the corpus does not hold is skipped. type-setup reports a missing page, and a framework
        // document is one a corpus may not have taken.
        foreach (var file in GeneratedFiles.Blocks(corpus.Adopted))
        {
            if (!file.MarkersRequired) continue;
            if (!tree.Exists(file.Path)) continue;

            CheckGeneratedBlocks(file.Path, tree.Read(file.Path), file.Blocks, file.FromFramework, findings);
        }

        // A type's page is not a record. It carries no frontmatter, so it takes the link checks alone.
        foreach (var (_, t) in schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            if (string.IsNullOrEmpty(t.Page)) continue;
            // Absence is type-setup's to report, not this pass's.
            if (!tree.Exists(t.Page)) continue;

            var page = Doc.Parse(t.Page, tree.Read(t.Page), schema, requireFrontmatter: false);
            if (page is null) continue;

            // A page is forked and a forked file is never compared against upstream, so nothing else
            // reports frontmatter left on one by a type that became a folder.
            if (page.FrontStartLine > 0)
                findings.Add(new Finding(t.Page, page.FrontStartLine, Sev.Error, new CheckId("page-frontmatter"),
                    "the page carries frontmatter: it describes the records beneath it and is not one, so it has "
                    + $"no id, tier or status of its own. Move what it holds into '{t.Folder}/' "
                    + "as a record, and delete the block."));

            LinkChecks.Check(page, schema, tree, new Report(page.Rel, findings));
        }

        // A template is checked here rather than discovered as a record: it has no id, and must not answer to
        // id-unique or to a reciprocal edge. Kept, because `CheckCorpusEnums` reads what a template states.
        List<Doc> templates = [];
        foreach (var rel in corpus.Templates)
        {
            var template = Doc.Parse(rel, tree.Read(rel), schema);
            if (template is null)
                findings.Add(new Finding(rel, null, Sev.Error, new CheckId("template-fields"),
                    "the template carries no frontmatter: a document copied from it starts with none."));
            else
            {
                templates.Add(template);
                CheckDocument(template, schema, tree, findings, today, DocKind.Template, fillable);
            }
        }

        CheckFrameworkDocs(schema, tree, corpus.Docs, findings);

        // The index is handed on rather than built again. A second one would be free to answer differently.
        var byId = CheckCorpus(schema, corpus.Docs, findings, corpus.Imports);

        CheckCorpusRules(schema, corpus.Docs, byId, tree, Versions(corpus.Descriptor),
            corpus.Descriptor.ReposBase, findings);
        CheckMinRecords(corpus.Docs, findings);
        CheckTypeSetup(schema, tree, corpus.Descriptor, findings);
        CheckShortcode(schema, corpus.Descriptor, findings);
        CheckTargets(corpus.Descriptor, findings);
        CheckAreas(corpus.Descriptor, findings);
        CheckCorpusEnums(schema, corpus.Docs, templates, corpus.Descriptor, findings);
        CheckImports(corpus.Imports, findings);
        CheckFreshness(standings ?? [], findings);

        return findings;
    }

    /// <param name="fillable">
    /// The folders an id written here may point into: the types this corpus adopted, plus the types its
    /// imports supply. Null says the caller does not know, and every obligation is then reported.
    /// </param>
    public static void CheckDocument(Doc d, Schema schema, Tree tree, List<Finding> f, DateOnly today,
        DocKind kind = DocKind.Record, IReadOnlySet<string>? fillable = null)
    {
        var report = new Report(d.Rel, f);

        if (d.Type is null)
        {
            report.Err(new CheckId("type"), $"folder '{d.Folder}' has no schema.");
            return;
        }

        var t = d.Type;

        if (d.Front is null)
        {
            report.Err(new CheckId("frontmatter-parses"), "frontmatter is not a valid YAML mapping.");
            return;
        }

        var present = new Dictionary<string, YamlNode>();
        foreach (var kv in d.Front.Children)
            present[((YamlScalarNode)kv.Key).Value ?? ""] = kv.Value;

        if (kind == DocKind.Template) CheckTemplateFields(d, t, report);
        else
            foreach (var k in d.FrontKeys)
                if (!t.KnownKeys.Contains(k))
                    report.Err(new CheckId("unknown-key"), $"unknown frontmatter key '{k}'.", d.FrontStartLine);

        // A derived field is declared, so `unknown-key` and `template-fields` pass it. A written value is
        // still a second answer nothing reconciles. Asked of a template too, which teaches every copy the key.
        foreach (var k in d.FrontKeys)
            if (t.Fields.GetValueOrDefault(k)?.From is { } from)
                report.Err(new CheckId("derived-key"),
                    $"'{k}' is derived from the record's {from} and is not written in frontmatter. "
                    + "Delete the line, and file the record in the folder you want it to name.",
                    d.FrontStartLine);

        // See TypeSchema.DeriveKeyOrderEdges for why the constraint is a pair set rather than a total order.
        CheckKeyOrder(d, t, report);

        // Not asked of a template: every value in one is bare or a placeholder, by design.
        if (kind == DocKind.Record)
            foreach (var spec in t.DeclaredFields)
            {
                var req = spec.Required || RequiredWhenHolds(spec.RequiredWhenCondition, present);
                var absent = !present.ContainsKey(spec.Name) || ValueChecks.IsAbsent(present[spec.Name], spec);
                if (req && absent && !Unfillable(spec, fillable))
                {
                    var why = spec.Required ? "" : $" (required when {spec.RequiredWhen})";
                    report.Err(new CheckId("required-field"),
                        $"missing required field '{spec.Name}'{why}.", d.FrontStartLine);
                }
            }

        foreach (var (name, node) in present)
        {
            var spec = schema.EffectiveField(t, name);
            // Unknown key already reported.
            if (spec is null) continue;

            ValueChecks.Check(name, node, spec, kind, d.FrontStartLine, report);
        }

        // Against the singular name the type declares, which is what frontmatter carries. `id-prefix` below
        // checks the id prefix declared beside it.
        if (present.TryGetValue("type", out var typeNode) && Yaml.Raw(typeNode) is { } type && type != t.TypeName)
            report.Err(new CheckId("type-matches-folder"),
                $"type '{type}' does not match the '{t.Key}' folder's type '{t.TypeName}'.",
                Yaml.LineOf(typeNode, d.FrontStartLine));

        if (present.TryGetValue("tier", out var tierNode) && Yaml.Raw(tierNode) is { } tier && tier != t.Tier)
            report.Err(new CheckId("tier-matches-type"),
                $"tier '{tier}' does not match the '{t.TypeName}' type tier '{t.Tier}'.",
                Yaml.LineOf(tierNode, d.FrontStartLine));

        // Not asked of a template. `svc-{{slug}}` is the instruction to allocate an id, and `_template.md`
        // matches no type's filename pattern.
        if (kind == DocKind.Record)
        {
            if (present.TryGetValue("id", out var idNode) && Yaml.Raw(idNode) is { } id)
                IdChecks.Check(id, Yaml.LineOf(idNode, d.FrontStartLine), d.Rel, t, report);

            IdChecks.CheckFilename(d.Rel, t, report);
        }

        CheckH1(d, report);
        CheckIdentity(d, t, present, report);

        foreach (var sec in t.RequiredSections)
            if (!d.Sections.Any(s => string.Equals(s.Title, sec, StringComparison.OrdinalIgnoreCase)))
                report.Err(new CheckId("required-section"), $"missing required section '## {sec}'.");

        // A required section left as a bare heading reads as finished to everyone but the reader who needed
        // it. A section the schema never declared is the author's own, and the section a type keeps its parts
        // in is left to the checks that read them.
        if (kind == DocKind.Record)
            foreach (var s in d.Sections.Where(s => !Md.HasContent(d.Text.AsSpan()[s.BodyStart..s.BodyEnd])))
            {
                if (t.Parts is { } parts
                    && string.Equals(s.Title, parts.Section, StringComparison.OrdinalIgnoreCase)) continue;

                if (t.RequiredSections.Contains(s.Title, StringComparer.OrdinalIgnoreCase))
                    report.Err(new CheckId("empty-section"),
                        $"required section '## {s.Title}' has nothing under it.", s.Line);
                else if (t.OptionalSections.Contains(s.Title, StringComparer.OrdinalIgnoreCase))
                    report.Err(new CheckId("empty-section"),
                        $"section '## {s.Title}' has nothing under it. Write it or delete the heading.", s.Line);
            }

        // Reported once, naming the first: an unfinished copy holds a dozen.
        if (kind == DocKind.Record) CheckPlaceholders(d, report);

        // Not asked of a template, whose clause rows demonstrate the shape with `{{ID}}` where the id goes.
        if (kind == DocKind.Record) PartChecks.Check(d, t, report);

        // Asked of a template too, which would teach a wrong notation to every record copied from it.
        PartChecks.CheckNotation(d, report);

        LinkChecks.Check(d, schema, tree, report, kind);

        // Not asked of a template, where both halves are examples.
        if (kind == DocKind.Record) CheckMirrorsSection(d, t, schema, report);

        // Held off a template for the same reason.
        if (kind == DocKind.Record) CheckMirrorsCitations(d, t, schema, report);

        // A template is exempt from every type rule, including one a type declares tomorrow: its guidance
        // prose would answer several of them wrongly.
        if (kind == DocKind.Record)
            CheckRules(d, t, report, today);
    }

    /// <summary>
    /// <c>Markers.SpliceBlock</c> returns the text untouched when a marker is missing, so a file that loses one stops
    /// being generated into and <c>generate --check</c> agrees it is fresh. Nothing else can notice. <c>README.md</c>
    /// is the exception and answers for itself. See <c>BlockFile.MarkersRequired</c>. <paramref name="fromFramework"/>
    /// adds the command that restores the page.
    /// </summary>
    public static void CheckGeneratedBlocks(string rel, string text, IEnumerable<string> names, bool fromFramework,
        List<Finding> f)
    {
        var restore = fromFramework ? " `kac update` writes the framework's copy of this page, which has them." : "";
        foreach (var name in names)
        {
            var begin = text.IndexOf(Markers.Begin(name), StringComparison.Ordinal);
            var end = text.IndexOf(Markers.End(name), StringComparison.Ordinal);
            if (begin < 0 || end < 0)
                f.Add(new Finding(rel, null, Sev.Error, new CheckId("generated-block"),
                    $"the '{name}' block is missing its "
                    + (begin < 0 && end < 0 ? "markers" : begin < 0 ? "BEGIN marker" : "END marker")
                    + ": `kac generate` writes between them and leaves the page alone without both." + restore));
            else if (end < begin)
                f.Add(new Finding(rel, null, Sev.Error, new CheckId("generated-block"),
                    $"the '{name}' block's END marker comes before its BEGIN marker."));
        }
    }

    /// <summary>
    /// A declared type with nothing behind it is valid; half of one is not. A folder needs its page and template, and a
    /// page its folder. A folder counts as present when it holds tracked files, so a fresh clone answers the same.
    /// </summary>
    private static void CheckTypeSetup(Schema schema, Tree tree, CorpusDescriptor descriptor, List<Finding> f)
    {
        CheckAdoption(schema, tree, descriptor, f);

        foreach (var (key, t) in schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            var at = $".schema/{key}.yaml";
            var pageExists = !string.IsNullOrEmpty(t.Page) && tree.Exists(t.Page);

            if (!tree.HasFolder(t.Folder))
            {
                if (pageExists)
                    f.Add(new Finding(at, null, Sev.Error, new CheckId("type-setup"),
                        $"type '{key}' has {t.Page} but no '{t.Folder}/': a type is set up as both or neither."));
                continue;
            }

            var missing = new List<string>();
            if (!pageExists) missing.Add(string.IsNullOrEmpty(t.Page) ? $"{key}.md" : t.Page);

            // The template is asked for with `OnDisk`, as `Corpus.DiscoverTemplates` asks for it. `Tree`
            // says why that is the right question for this one file.
            var template = $"{t.Folder}/{Artefact.Template}";
            if (!tree.OnDisk(template)) missing.Add(template);
            if (missing.Count > 0)
                f.Add(new Finding(at, null, Sev.Error, new CheckId("type-setup"),
                    $"type '{key}' has a '{t.Folder}/' folder but is not fully set up. "
                    + $"Add {string.Join(", ", missing)}."));
        }
    }

    /// <summary>Not asked of a corpus that declares no <c>types:</c>, whose adoption is read off its folders.</summary>
    private static void CheckAdoption(Schema schema, Tree tree, CorpusDescriptor descriptor, List<Finding> f)
    {
        if (descriptor.Types is not { } declared) return;

        const string at = ".corpus.yaml";

        foreach (var name in declared.Where(n => !schema.ByFolder.ContainsKey(n)))
            f.Add(new Finding(at, null, Sev.Error, new CheckId("type-setup"),
                $"'{name}' is adopted here and no schema covers it: either '.schema/{name}.yaml' has not been synced "
                + "from upstream, or the name is wrong."));

        foreach (var (key, t) in schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            var stoodUp = Corpus.StoodUp(t, tree);
            var adopted = declared.Contains(key, StringComparer.Ordinal);

            if (adopted && !stoodUp)
                f.Add(new Finding(at, null, Sev.Error, new CheckId("type-setup"),
                    $"type '{key}' is adopted here and is not stood up. Add {t.Page} and its folder, or drop it from "
                    + "'types:' if it was not wanted."));

            if (!adopted && stoodUp)
                f.Add(new Finding(at, null, Sev.Error, new CheckId("type-setup"),
                    $"type '{key}' is stood up here and is not in 'types:'. Every generated list leaves it out while "
                    + "the corpus holds it. Adopt it, or delete what was built."));
        }
    }

    /// <summary>
    /// See docs/use/corpus-descriptor.md. Reported once, against the descriptor, because that is the file to edit.
    /// Asked of the types holding a record: an empty folder has no estate to derive a list from.
    /// </summary>
    private static void CheckCorpusEnums(Schema schema, IReadOnlyList<Doc> docs, IReadOnlyList<Doc> templates,
        CorpusDescriptor descriptor, List<Finding> f)
    {
        var types = docs.Select(d => d.Type).OfType<TypeSchema>()
            .DistinctBy(t => t.Key).OrderBy(t => t.Key, StringComparer.Ordinal).ToList();
        if (types.Count == 0) return;

        // A template is read for what the corpus owes a range for, and never for which types have arrived.
        var stated = docs.Concat(templates).ToList();

        foreach (var field in schema.Universal.Values)
            Ask(field, stated, "", "");

        foreach (var t in types)
        {
            var written = stated.Where(d => d.Type?.Key == t.Key).ToList();
            foreach (var field in t.Fields.OrderBy(kv => kv.Key, StringComparer.Ordinal).Select(kv => kv.Value))
                Ask(field, written, $" on a {t.TypeName}", $" {t.Page} says how to reach them.");
        }

        void Ask(FieldSpec root, IReadOnlyList<Doc> written, string on, string page)
        {
            var owed = Owed(root, written);
            foreach (var (path, field) in Reachable(root))
            {
                if (field.CorpusEnum is not { } name) continue;
                var said = $"'{path}'{on}";

                // Only this half waits for the field to be owed.
                // A range already written is checked as soon as it exists.
                if (!descriptor.Enums.TryGetValue(name, out var values) || values.Count == 0)
                {
                    if (owed)
                        Report($"{said} is judged against a list this corpus states, and it states none. Write "
                               + $"the values your estate uses under `enums:`, as `{name}: [a, b]`.{page}");
                    continue;
                }

                foreach (var value in values.Where(v => v != v.ToLowerInvariant()))
                    Report($"{said} is judged against `enums.{name}`, and that list carries '{value}'. An enum "
                           + "value is lower case, so no record can satisfy this one.");
            }

            return;

            void Report(string message) => f.Add(new Finding(
                Corpus.Descriptor, null, Sev.Error, new CheckId("corpus-enum-undeclared"), message));
        }
    }

    /// <summary>
    /// A required field owes a range from the type's first record. An optional one owes it from the first document that
    /// states it.
    /// </summary>
    private static bool Owed(FieldSpec root, IReadOnlyList<Doc> written) =>
        root.Required || root.RequiredWhen is not null
                      || written.Any(d => d.FrontNode(root.Name) is { } v && !ValueChecks.IsAbsent(v, root));

    /// <summary>
    /// A <c>shape:</c> is resolved into <c>Entry</c> at load, so its keys need no walk of their own. The path tells
    /// <c>interfaces.type</c> apart from the frontmatter key <c>type</c>.
    /// </summary>
    private static IEnumerable<(string Path, FieldSpec Field)> Reachable(FieldSpec field, string prefix = "")
    {
        var path = prefix + field.Name;
        return new[] { (path, field) }
            .Concat((field.Entry ?? []).SelectMany(key => Reachable(key, path + ".")));
    }

    /// <summary>
    /// <c>.schema/_checks.yaml</c> argues the spelling under <c>shortcode</c>. Both faults are reported at once, which
    /// is why the prefix comparison ignores case.
    /// </summary>
    private static void CheckShortcode(Schema schema, CorpusDescriptor descriptor, List<Finding> f)
    {
        if (descriptor.Shortcode is not { } shortcode) return;

        const string at = ".corpus.yaml";

        if (CorpusDescriptor.ShortcodeFault(shortcode) is { } fault)
            f.Add(new Finding(at, null, Sev.Error, new CheckId("shortcode"),
                $"shortcode '{shortcode}' {fault}. A shortcode is "
                + $"{CorpusDescriptor.ShortcodeMin} to {CorpusDescriptor.ShortcodeMax} characters, opens on a "
                + "lower-case letter, and carries lower-case letters and digits after it."));

        // Read from the schema, so adopting a type takes its prefix and declining one frees it.
        foreach (var (key, t) in schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal))
            if (t.IdPrefix.Length > 0 && string.Equals(shortcode, t.IdPrefix, StringComparison.OrdinalIgnoreCase))
                f.Add(new Finding(at, null, Sev.Error, new CheckId("shortcode"),
                    $"shortcode '{shortcode}' is the id prefix of '{key}'. A citation opening '{t.IdPrefix}:' "
                    + "reads as that type rather than as this corpus. Pick a shorthand no type has taken."));
    }

    /// <summary><c>.schema/_checks.yaml</c> argues both vocabularies under <c>descriptor-target</c>.</summary>
    private static void CheckTargets(CorpusDescriptor descriptor, List<Finding> f)
    {
        Held("publishing-target", descriptor.PublishingTarget, Publishing.Targets, "publish to");
        Held("tracker.target", descriptor.TrackerTarget, Tracker.Stated, "file on");
        Held("framework.target", descriptor.FrameworkTarget, Tracker.Stated, "file on");

        return;

        void Held(string key, string? stated, IReadOnlyList<string> allowed, string verb)
        {
            if (string.IsNullOrWhiteSpace(stated)) return;
            if (allowed.Contains(stated, StringComparer.Ordinal)) return;

            f.Add(new Finding(".corpus.yaml", null, Sev.Error, new CheckId("descriptor-target"),
                $"{key} '{stated}' is not a target kac can {verb}. Write one of: "
                + $"{string.Join(", ", allowed)}."));
        }
    }

    /// <summary>
    /// An area is an Azure DevOps area path, so <c>github</c> and <c>none</c> have nowhere to put one. Held against the
    /// effective target, because a corpus publishing to Azure Repos states an area and no <c>tracker:</c>.
    /// <c>Tracker.For</c> drops an unusable area from the export, so this is what tells the corpus.
    /// </summary>
    private static void CheckAreas(CorpusDescriptor descriptor, List<Finding> f)
    {
        Divided("tracker", descriptor.TrackerArea, Tracker.Own(descriptor).Target);
        Divided("framework", descriptor.FrameworkArea, descriptor.FrameworkTarget);

        return;

        void Divided(string block, string? area, string target)
        {
            if (string.IsNullOrWhiteSpace(area)) return;
            if (Tracker.Areas.Contains(target, StringComparer.Ordinal)) return;

            f.Add(new Finding(".corpus.yaml", null, Sev.Error, new CheckId("descriptor-area"),
                $"{block}.area '{area}' names an area path, and {block} files on '{target}', which has "
                + $"no area paths. Only {string.Join(", ", Tracker.Areas)} does."));
        }
    }

    /// <summary>Globs <c>Tree.Match</c> answers. The framework's own glossary is also a record.</summary>
    private static readonly string[] FrameworkDocs =
        ["knowledge-as-code.md", "knowledge-as-code/*.md", "glossary/knowledge-as-code.md"];

    /// <summary>
    /// A framework document is shared byte-for-byte, so it names a type and never links to one: the link resolves only
    /// where the type was adopted. A generated block is exempt, being written against this corpus. <c>link-resolves</c>
    /// cannot catch it here, where every type page exists.
    /// </summary>
    private static void CheckFrameworkDocs(Schema schema, Tree tree, IEnumerable<Doc> docs, List<Finding> f)
    {
        // Already validated as records. A second link pass would report every dead link twice.
        var checkedAsRecords = new HashSet<string>(docs.Select(d => d.Rel), StringComparer.OrdinalIgnoreCase);

        var pages = schema.ByFolder.Values
            .Where(t => !string.IsNullOrEmpty(t.Page))
            .ToDictionary(t => "/" + t.Page[..^".md".Length], t => t.Key, StringComparer.OrdinalIgnoreCase);

        foreach (var rel in FrameworkDocs.SelectMany(tree.Match))
        {
            // Read with the generated blocks emptied. Those answer to `generate --check`.
            var doc = Doc.Parse(rel, Markers.Authored(tree.Read(rel)),
                schema, requireFrontmatter: false);
            if (doc is null) continue;

            if (!checkedAsRecords.Contains(rel)) LinkChecks.Check(doc, schema, tree, new Report(doc.Rel, f));

            foreach (var link in doc.Links)
            {
                var target = link.Target.Split('#')[0].TrimEnd('/');
                if (target.Length == 0 || LinkChecks.IsExternal(target)) continue;

                // Azure DevOps resolves a page with or without its extension, so both forms are the same
                // link and both are caught. Dropping the extension here lets one lookup answer for `/adrs`
                // and `/adrs.md` alike.
                if (target.EndsWith(".md", StringComparison.OrdinalIgnoreCase)) target = target[..^3];

                var slash = target.LastIndexOf('/');
                var page = slash > 0 ? target[..slash] : target;
                if (!pages.TryGetValue(page, out var type)) continue;

                // A path into a type's folder names a record, and every corpus deletes the records it inherits.
                f.Add(new Finding(rel, link.Line, Sev.Error, new CheckId("framework-names-types"), slash > 0
                    ? $"'{link.Target}' links to a record in '{type}' from a document every corpus shares. Those "
                      + "records are the first thing a corpus deletes, so the link dies even where the type is used."
                    : $"'{link.Target}' links to the '{type}' type from a document every corpus shares. Name the type "
                      + "instead: a corpus that has not adopted it reads a dead link, and one that has is no worse "
                      + "off."));
            }
        }
    }

    /// <summary>
    /// Returns which document each id names, first writer winning. The corpus rules read that index next.
    /// </summary>
    public static Dictionary<string, Doc> CheckCorpus(
        Schema schema, List<Doc> docs, List<Finding> f, ImportGraph? imports = null)
    {
        var byId = new Dictionary<string, Doc>(StringComparer.OrdinalIgnoreCase);
        foreach (var d in docs)
        {
            var id = d.FrontScalar("id");
            if (id is null) continue;
            if (byId.TryGetValue(id, out var other))
                f.Add(new Finding(d.Rel, d.FrontStartLine, Sev.Error, new CheckId("id-unique"),
                    $"id '{id}' is also used by {other.Rel}."));
            else
                byId[id] = d;
        }

        // Local and imported records behind one lookup. See Resolver.cs.
        var resolver = new Resolver(byId, imports ?? ImportGraph.None);

        foreach (var d in docs)
        foreach (var (text, line) in d.PartRefs)
        {
            var citation = Citation.Read(text);
            var landed = resolver.Resolve(citation);

            if (Spelled(citation, landed) is { } wrong)
            {
                f.Add(new Finding(d.Rel, line, Sev.Error, new CheckId("part-ref"), wrong));
                continue;
            }

            if (landed.How is Landing.Missing)
            {
                f.Add(new Finding(d.Rel, line, Sev.Error, new CheckId("part-ref"),
                    $"'{citation}' cites '{citation.Whole}', which does not exist."));
                continue;
            }

            // The import this cites has not been restored, and `import-restored` has already said so.
            if (landed.How is Landing.NotRestored) continue;

            // A scoped record cited whole, such as `eng:pol-VURM`, has no part to ask about.
            if (citation.Part is not { } partId) continue;

            var noun = landed.Noun(schema);
            if (noun is null)
                f.Add(new Finding(d.Rel, line, Sev.Error, new CheckId("part-ref"),
                    $"'{citation}' addresses a part of {landed.Where}, and its type offers none. Cite the "
                    + $"document as '{citation.Whole}'."));
            else if (!landed.Carries(partId))
                f.Add(new Finding(d.Rel, line, Sev.Error, new CheckId("part-ref"),
                    $"'{citation}' cites a {noun} '{partId}' that {landed.Where} does not carry."));
        }

        // Asked of every ref field, reciprocal or not. A literal the field admits is not an id and is skipped.
        foreach (var d in docs)
        {
            if (d.Type is null) continue;
            foreach (var name in d.Type.FieldOrder)
            {
                var spec = d.Type.Fields[name];
                if (spec.Refs.Count == 0 || !spec.DeclaresId) continue;

                var admitted = Admitted(spec, schema);
                foreach (var value in d.FrontList(name))
                {
                    if (spec.IsLiteral(value)) continue;

                    var citation = Citation.Read(value);
                    var landed = resolver.Resolve(citation);
                    var at = d.FrontStartLine;

                    if (Spelled(citation, landed) is { } wrong)
                    {
                        f.Add(new Finding(d.Rel, at, Sev.Error, new CheckId("ref-resolves"),
                            $"'{name}': {wrong}"));
                        continue;
                    }

                    if (landed.How is Landing.Missing)
                    {
                        f.Add(new Finding(d.Rel, at, Sev.Error, new CheckId("ref-resolves"),
                            $"'{name}' points at '{citation.Whole}', which does not exist."));
                        continue;
                    }

                    if (landed.How is Landing.NotRestored) continue;

                    if (citation.Part is { } partId)
                    {
                        var noun = landed.Noun(schema);
                        if (noun is null)
                        {
                            f.Add(new Finding(d.Rel, at, Sev.Error, new CheckId("ref-resolves"),
                                $"'{name}' points at a part of {landed.Where}, and its type offers none. "
                                + $"Name the document as '{citation.Whole}'."));
                            continue;
                        }

                        if (!landed.Carries(partId))
                        {
                            f.Add(new Finding(d.Rel, at, Sev.Error, new CheckId("ref-resolves"),
                                $"'{name}' points at a {noun} '{partId}' that {landed.Where} does not "
                                + "carry."));
                            continue;
                        }
                    }

                    var type = landed.Type(schema);
                    if (type is not null && !admitted.Contains(type))
                    {
                        // Named from the declaration: a type this corpus declined has no schema here
                        // to read a label from.
                        if (admitted.Count == 0)
                            f.Add(new Finding(d.Rel, at, Sev.Error, new CheckId("ref-resolves"),
                                $"'{name}' points at '{citation.Whole}', which is {WithArticle(type)}. The field "
                                + $"points at {Named(spec.Refs)}, which this corpus did not adopt."));
                        else
                            f.Add(new Finding(d.Rel, at, Sev.Error, new CheckId("ref-resolves"),
                                $"'{name}' points at '{citation.Whole}', which is {WithArticle(type)}, "
                                + $"not {OneOf(admitted)}."));

                        continue;
                    }

                    // Asked after the type, so a target of the wrong type is reported as that.
                    // The fallback noun is for a target whose type this corpus declined.
                    if (spec.PartRequired && citation.Part is null)
                    {
                        var noun = landed.Noun(schema) ?? "part";
                        f.Add(new Finding(d.Rel, at, Sev.Error, new CheckId("ref-resolves"),
                            $"'{name}' points at '{citation.Whole}' whole, and this field names a {noun}. Write "
                            + $"'{citation.Whole}.<{noun}>', one entry per {noun}. A bare id reads as every "
                            + $"{noun} covered."));
                    }
                }
            }
        }

        // Existence and type are `ref-resolves`'s. This asks only whether the target points back.
        foreach (var d in docs)
        {
            if (d.Type is null) continue;
            foreach (var name in d.Type.FieldOrder)
            {
                var spec = d.Type.Fields[name];
                if (spec.Reciprocal is null || spec.Refs.Count == 0) continue;
                var admitted = Admitted(spec, schema);
                foreach (var targetId in d.FrontList(name))
                {
                    // A scoped id is in another corpus, which has no field pointing back into this one.
                    var cited = Citation.Read(targetId);
                    if (cited.Scope is not null) continue;
                    if (!byId.TryGetValue(cited.Record, out var target)) continue;

                    // A target of the wrong type is already reported, and has no counterpart field to answer with.
                    if (!Admits(admitted, target)) continue;

                    var back = target.FrontList(spec.Reciprocal);
                    var selfId = d.FrontScalar("id");
                    if (!back.Any(b => string.Equals(b, selfId, StringComparison.OrdinalIgnoreCase)))
                        f.Add(new Finding(d.Rel, d.FrontStartLine, Sev.Error, new CheckId("reciprocal"),
                            $"'{name}: {targetId}' is not reciprocated: {target.Rel} must list '{spec.Reciprocal}: {selfId}'."));
                }
            }
        }

        return byId;
    }

    /// <summary>
    /// An error rather than a skip. A run that quietly checks less than CI lets a broken reference reach main.
    /// </summary>
    private static void CheckImports(ImportGraph imports, List<Finding> f)
    {
        foreach (var shortcode in imports.NotRestored)
            f.Add(new Finding(Corpus.Descriptor, null, Sev.Error, new CheckId("import-restored"),
                $"`consumes:` declares '{shortcode}:' and {Restore.ImportsDir}/{shortcode}/ holds no "
                + "export. Run `kac restore`."));

        foreach (var named in imports.Undeclared)
            f.Add(new Finding(Corpus.Descriptor, null, Sev.Error, new CheckId("import-restored"),
                $"`consumes:` declares {named} without a shortcode, so it files under no folder and no "
                + "citation can reach it. Run `kac restore`, which says which key is missing."));
    }

    /// <summary>
    /// Never an error. A version behind is not a broken corpus, and somebody else's release must not turn a downstream
    /// build red.
    /// </summary>
    private static void CheckFreshness(IReadOnlyList<ImportStanding> standings, List<Finding> f)
    {
        foreach (var s in standings)
            switch (s.How)
            {
                case Standing.Behind:
                    f.Add(new Finding(Corpus.Descriptor, null, Sev.Warning, new CheckId("import-behind"),
                        $"'{s.Shortcode}:' is locked at {s.Locked} and {s.Available} is published. "
                        + $"'{s.Range}' admits it, so `kac restore` takes it."));
                    break;

                case Standing.Capped:
                    f.Add(new Finding(Corpus.Descriptor, null, Sev.Info, new CheckId("import-capped"),
                        $"'{s.Shortcode}:' is locked at {s.Locked} and {s.Available} is published. "
                        + $"'{s.Range}' holds it back."));
                    break;

                case Standing.Unreachable:
                    f.Add(new Finding(Corpus.Descriptor, null, Sev.Info, new CheckId("import-unreachable"),
                        $"could not check what '{s.Shortcode}:' publishes, so {s.Locked} is unchecked "
                        + $"rather than current. {s.Problem}"));
                    break;
            }
    }

    /// <summary>
    /// The three ways a citation misspells a record that exists, and null where the spelling is right.
    /// </summary>
    private static string? Spelled(Citation citation, Landed landed) => landed.How switch
    {
        Landing.NeedsScope =>
            $"'{citation}' names a record this corpus imports rather than holds. Write it as "
            + $"'{citation.In(landed.Scope)}', so a reader can see which corpus owns it.",
        Landing.NeedsNoScope =>
            $"'{citation}' scopes a record this corpus holds itself. Write it as "
            + $"'{citation.In(null)}': two spellings of one id defeat every search anybody runs for it.",
        Landing.UnknownScope =>
            $"'{citation}' cites '{landed.Scope}:', and this corpus consumes nothing under that "
            + "shortcode. Declare it in `consumes:`, or correct the spelling.",
        _ => null
    };

    private static List<TypeSchema> Admitted(FieldSpec spec, Schema schema) =>
        [.. spec.Refs.Select(schema.ByFolder.GetValueOrDefault).OfType<TypeSchema>()];

    /// <summary>A document in a folder no type covers is admitted. <c>type</c> is the check that reports it.</summary>
    private static bool Admits(List<TypeSchema> admitted, Doc target) =>
        target.Type is null || admitted.Contains(target.Type);

    private static string Named(IReadOnlyList<string> refs) =>
        string.Join(", ", refs.Select(r => $"'{r}'"));

    private static string OneOf(List<TypeSchema> types)
    {
        var names = types.Select(WithArticle).ToList();
        return names.Count switch
        {
            1 => names[0],
            2 => $"{names[0]} or {names[1]}",
            _ => $"{string.Join(", ", names[..^1])}, or {names[^1]}"
        };
    }

    /// <summary>
    /// The content version this corpus reads under each corpus name: its own from the descriptor, and a consumed
    /// corpus's from the version restore resolved. An entry with no name or no version is left out.
    /// </summary>
    private static Dictionary<string, string> Versions(CorpusDescriptor descriptor)
    {
        var found = new Dictionary<string, string>(StringComparer.Ordinal);

        if (descriptor.Name is { Length: > 0 } name && descriptor.ContentVersion is { Length: > 0 } version)
            found[name] = version;

        foreach (var consumed in descriptor.Consumes)
            if (consumed is { Corpus: { Length: > 0 } from, Resolved: { Length: > 0 } locked })
                found[from] = locked;

        return found;
    }

    /// <summary>
    /// Driven from the schema, so a rule runs whether or not the corpus has stood its type up. A rule with no
    /// implementation is skipped in silence.
    /// </summary>
    private static void CheckCorpusRules(Schema schema, List<Doc> docs, Dictionary<string, Doc> byId,
        Tree tree, IReadOnlyDictionary<string, string> versions, string? reposBase, List<Finding> f)
    {
        foreach (var (_, t) in schema.ByFolder.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        foreach (var rule in t.Rules)
            if (CorpusRules.ByRuleId.TryGetValue(rule.Id, out var implementation))
                implementation.Check(new CorpusRuleContext(docs, byId, tree, t, rule, versions, reposBase,
                    (at, c, m, l) => Report(Sev.Error, at, c, m, l),
                    (at, c, m, l) => Report(Sev.Warning, at, c, m, l)));
        return;

        void Report(Sev severity, Doc at, CheckId check, string message, int? line)
            => f.Add(new Finding(at.Rel, line, severity, check, message));
    }

    /// <summary>
    /// Documents paired with the type they were read as, dropping those read as no type. <c>Where</c> cannot tell
    /// <c>GroupBy</c> that it settled the key, so the pair is made here, where the narrowing survives into it.
    /// </summary>
    private static IEnumerable<(Doc Doc, TypeSchema Type)> Typed(IEnumerable<Doc> docs)
    {
        foreach (var d in docs)
            if (d.Type is { } type)
                yield return (d, type);
    }

    /// <summary>
    /// A warning, and permanently one: a value newly introduced is below the floor on the day it is written.
    /// </summary>
    private static void CheckMinRecords(List<Doc> docs, List<Finding> f)
    {
        foreach (var group in Typed(docs).GroupBy(x => x.Type, x => x.Doc))
        {
            var type = group.Key;
            foreach (var name in type.FieldOrder)
            {
                if (type.Fields[name].MinRecords is not { } floor) continue;

                // Case-insensitive, and once per record however often the record lists the value.
                var carried = group
                    .Select(d => (Doc: d,
                        Values: d.FrontList(name).Distinct(StringComparer.OrdinalIgnoreCase).ToList()))
                    .ToList();
                var count = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var value in carried.SelectMany(c => c.Values))
                    count[value] = count.GetValueOrDefault(value) + 1;

                foreach (var (doc, values) in carried)
                foreach (var value in values.Where(v => count[v] < floor).Order(StringComparer.Ordinal))
                {
                    var carriers = Count(count[value], $"{type.TypeName} record", $"{type.TypeName} records");
                    f.Add(new Finding(doc.Rel, doc.FrontStartLine, Sev.Warning, new CheckId("min-records"),
                        $"'{name}: {value}' is carried by {carriers}: the schema asks for at least {floor}, "
                        + "because a value here is meant to group records. One that does not belongs in a field "
                        + "that is free to be unique."));
                }
            }
        }
    }

    /// <summary>
    /// Would a copy of this template pass its own frontmatter checks? A missing optional field is an editorial choice.
    /// A missing required field fails every copy. <c>required-when</c> is not asked, and <c>title</c> is admitted but
    /// never expected.
    /// </summary>
    private static void CheckTemplateFields(Doc d, TypeSchema t, Report report)
    {
        foreach (var k in d.FrontKeys.Where(k => !t.KnownKeys.Contains(k)))
            report.Err(new CheckId("template-fields"),
                $"'{k}' is not a field of the '{t.TypeName}' type: every document copied from this "
                + "template would fail unknown-key.", d.FrontStartLine);

        var carried = new HashSet<string>(d.FrontKeys, StringComparer.Ordinal);
        foreach (var spec in t.DeclaredFields.Where(spec => spec.Required && !carried.Contains(spec.Name)))
            report.Err(new CheckId("template-fields"),
                $"the template does not carry '{spec.Name}', which is required: every document copied "
                + "from it would fail required-field.", d.FrontStartLine);
    }

    private static void CheckPlaceholders(Doc d, Report report)
    {
        var left = Placeholder.Occurrences(d).ToList();
        if (left.Count == 0) return;

        var (token, line) = left[0];
        var rest = left.Count > 1 ? $" There are {left.Count - 1} more in this document." : "";
        report.Err(new CheckId("placeholder-left"),
            $"'{token}' is a placeholder the template left for you to fill in.{rest}", line);
    }

    private static void CheckKeyOrder(Doc d, TypeSchema t, Report report)
    {
        var pos = new Dictionary<string, int>();
        for (var i = 0; i < d.FrontKeys.Count; i++)
            if (!pos.ContainsKey(d.FrontKeys[i]))
                pos[d.FrontKeys[i]] = i;

        foreach (var (a, b) in t.KeyOrderEdges)
            if (pos.TryGetValue(a, out var pa) && pos.TryGetValue(b, out var pb) && pa > pb)
                report.Err(new CheckId("key-order"),
                    $"'{a}' must appear before '{b}' in the frontmatter.", d.FrontStartLine);
    }

    /// <summary>
    /// <c>CheckIdentity</c> depends on this having run first. With no H1 there is no identity line, and reporting both
    /// would count one fault twice.
    /// </summary>
    private static void CheckH1(Doc d, Report report)
    {
        if (d.H1 is null) report.Err(new CheckId("h1"), "document has no H1.", 1);
    }

    /// <summary>Each half is checked separately: a wrong type and a wrong id are different mistakes.</summary>
    private static void CheckIdentity(Doc d, TypeSchema t, Dictionary<string, YamlNode> present,
        Report report)
    {
        if (d.H1 is null) return;

        var id = present.TryGetValue("id", out var idNode) ? Yaml.Raw(idNode) : null;
        var status = present.TryGetValue("status", out var statusNode) ? Yaml.Raw(statusNode) : null;
        var expected = Expected(t, id, status);

        if (d.IdentitySpans is null)
        {
            report.Err(new CheckId("identity"), $"no identity line follows the H1. Add {expected}.", d.H1Line);
            return;
        }

        // Anything else is reported once, against the whole line.
        var colon = d.IdentitySpans[0].IndexOf(':');
        if (d.IdentitySpans.Count != 2 || colon <= 0)
        {
            report.Err(new CheckId("identity"),
                $"identity line is malformed. Write it as {expected}.", d.IdentityLine);
            return;
        }

        var gotType = d.IdentitySpans[0][..colon].Trim();
        var gotId = d.IdentitySpans[0][(colon + 1)..].Trim();
        var gotStatus = d.IdentitySpans[1].Trim();

        if (!string.Equals(gotType, t.DisplayName, StringComparison.Ordinal))
            report.Err(new CheckId("identity-type"),
                $"identity line says '{gotType}', but this is {WithArticle(t)}.", d.IdentityLine);

        // Against the frontmatter: `id-matches-filename` already ties the frontmatter to the file.
        if (id is not null && !string.Equals(gotId, id, StringComparison.Ordinal))
            report.Err(new CheckId("identity-id"),
                $"identity line id '{gotId}' does not match the document's id '{id}'.",
                d.IdentityLine);

        // Case-insensitive: frontmatter writes status lower case and the line upper case.
        if (status is not null && !string.Equals(gotStatus, status, StringComparison.OrdinalIgnoreCase))
            report.Err(new CheckId("identity-status"),
                $"identity line status '{gotStatus}' does not match the document's status '{status}'.",
                d.IdentityLine);
        else if (status is not null && !string.Equals(gotStatus, status.ToUpperInvariant(), StringComparison.Ordinal))
            report.Err(new CheckId("identity-status"), $"identity line status '{gotStatus}' must be upper-case: "
                                                       + $"`{status.ToUpperInvariant()}`.", d.IdentityLine);
    }

    private static string Expected(TypeSchema t, string? id, string? status) =>
        $"`{t.DisplayName}: {id ?? $"{t.IdPrefix}-…"}` `{status?.ToUpperInvariant() ?? "STATUS"}`";

    private static void CheckMirrorsSection(Doc d, TypeSchema t, Schema schema, Report report)
    {
        foreach (var spec in t.DeclaredFields)
        {
            if (spec.MirrorsSection is not { } section) continue;
            var refTypes = spec.Refs.Select(schema.ByFolder.GetValueOrDefault).OfType<TypeSchema>().ToList();
            if (refTypes.Count == 0) continue;

            var inFront = new HashSet<string>(d.FrontList(spec.Name), StringComparer.OrdinalIgnoreCase);
            var inSection = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var link in d.MirroredSectionLinks.GetValueOrDefault(section, []))
            {
                // At most one of the types can own the folder a link lands in, so the first answer is the answer.
                var id = refTypes.Select(rt => IdChecks.IdFromLink(link, d.Rel, rt))
                    .FirstOrDefault(x => x is not null);
                if (id is not null) inSection.Add(id);
            }

            // Both sets ignore case, and so must the comparison across them: `Except` builds a set of its
            // own and reaches for the default comparer unless handed one.
            foreach (var id in inFront.Except(inSection, StringComparer.OrdinalIgnoreCase))
                report.Err(new CheckId("related-matches-section"),
                    $"'{spec.Name}' lists '{id}' but it is not referenced in the '## {section}' section.",
                    d.FrontStartLine);
            foreach (var id in inSection.Except(inFront, StringComparer.OrdinalIgnoreCase))
                report.Err(new CheckId("related-matches-section"),
                    $"the '## {section}' section references '{id}' but '{spec.Name}' does not list it.",
                    d.FrontStartLine);
        }
    }

    private static void CheckMirrorsCitations(Doc d, TypeSchema t, Schema schema, Report report)
    {
        var declared =
            new List<(FieldSpec Spec, string Label, List<CitationFootnote> Lines, List<TypeSchema> Targets)>();
        foreach (var spec in t.DeclaredFields)
        {
            if (spec.MirrorsCitations is not { } label) continue;
            var targets = spec.Refs.Select(schema.ByFolder.GetValueOrDefault).OfType<TypeSchema>().ToList();
            if (targets.Count == 0) continue;
            declared.Add((spec, label, d.CitationFootnotes.GetValueOrDefault(label, []), targets));
        }

        // Decided once per line, because two fields spelling one label share a list. The comparer is held in a typed
        // local so the group key comes from the selector; passed inline, it widens the key to object.
        IEqualityComparer<List<CitationFootnote>> sameLines = ReferenceEqualityComparer.Instance;
        foreach (var group in declared.GroupBy(x => x.Lines, sameLines))
        {
            var label = group.First().Label;
            var targets = group.SelectMany(x => x.Targets).ToList();
            foreach (var footnote in group.Key)
            {
                // Until the line is italic, nothing else can be asked of it.
                if (!footnote.Italic)
                {
                    report.Err(new CheckId("mirrors-citations"),
                        $"this '{label}' line is not italic, so its marks show on the page. An "
                        + "emphasis mark needs a word against it at each end.", footnote.Line);
                    continue;
                }

                if (!footnote.ClosesSection)
                    report.Err(new CheckId("mirrors-citations"),
                        $"this '{label}' line stands in the middle of a section. Write it as the last "
                        + "thing under the heading it belongs to.", footnote.Line);
                if (!footnote.Citations.Any(citation => Gathered(targets, citation) is not null))
                    report.Err(new CheckId("mirrors-citations"),
                        $"this '{label}' line names nothing it could gather. Name what the section "
                        + "answers, or take the line off.", footnote.Line);
            }
        }

        foreach (var (spec, label, lines, targets) in declared)
        {
            var inFront = new HashSet<string>(d.FrontList(spec.Name), StringComparer.OrdinalIgnoreCase);
            var inBody = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Asked per field: `part-required:` is one field's declaration, so two fields sharing a label may
            // disagree about a line. A line naming a record whole stays out of the union.
            foreach (var footnote in lines)
            {
                var whole = new List<(TypeSchema Target, string Id)>();
                foreach (var text in footnote.Citations)
                {
                    if (Gathered(targets, text) is not { } target) continue;
                    var citation = Citation.Read(text);
                    if (spec.PartRequired && citation.Part is null) whole.Add((target, citation.Whole));
                    else inBody.Add(text);
                }

                // Once per line however many ids it names, and only once the line is italic.
                if (!footnote.Italic) continue;
                foreach (var byTarget in whole.GroupBy(x => x.Target))
                {
                    var noun = byTarget.Key.Parts?.Noun ?? "part";
                    var ids = byTarget.Select(x => x.Id)
                        .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                    var write = ids.Count == 1
                        ? $"Write '{ids[0]}.<{noun}>'"
                        : $"Write each as '<id>.<{noun}>'";

                    report.Err(new CheckId("mirrors-citations"),
                        $"this '{label}' line names {string.Join(", ", ids.Select(id => $"'{id}'"))} "
                        + $"whole, and '{spec.Name}' names a {noun}. {write}, one entry per {noun}. A "
                        + $"bare id reads as every {noun} covered.", footnote.Line);
                }
            }

            foreach (var id in inFront.Except(inBody, StringComparer.OrdinalIgnoreCase))
                report.Err(new CheckId("mirrors-citations"),
                    $"'{spec.Name}' lists '{id}' and no '{label}' line names it. Close the section that "
                    + "answers it with one, or take the id out of the field.", d.FrontStartLine);
            foreach (var id in inBody.Except(inFront, StringComparer.OrdinalIgnoreCase))
                report.Err(new CheckId("mirrors-citations"),
                    $"a '{label}' line names '{id}' and '{spec.Name}' does not list it.", d.FrontStartLine);
        }
    }

    /// <summary>
    /// Null where the citation belongs to none of the types, as a line citing another standard's rule does. That is
    /// prose rather than coverage.
    /// </summary>
    private static TypeSchema? Gathered(List<TypeSchema> targets, string citation)
    {
        var record = Citation.Read(citation).Record;
        return targets.FirstOrDefault(target => record.StartsWith(target.IdPrefix + "-", StringComparison.Ordinal));
    }

    /// <summary>
    /// An <c>expr:</c> rule is evaluated, and any other is looked up in <c>DocumentRules</c> by id.
    /// <c>docs/maintaining/checks.md</c> draws the line between them.
    /// </summary>
    private static void CheckRules(Doc d, TypeSchema t, Report report, DateOnly today)
    {
        // Built only where a rule asks something of the document.
        Facts? facts = null;

        foreach (var rule in t.Rules)
        {
            if (rule.IsExpression)
            {
                facts ??= new Facts(d, today);
                if (RuleExpr.Eval(rule.Compiled, facts)) continue;
                // Deliberately the rule's own id: the one place the two ids are the same string.
                var reported = new CheckId(rule.Id.Value);
                if (rule.Severity == Sev.Error) report.Err(reported, rule.Message, d.FrontStartLine);
                else report.Warn(reported, rule.Message, d.FrontStartLine);
                continue;
            }

            // A rule with neither an `expr:` nor an implementation is a statement of intent, skipped in silence.
            if (DocumentRules.ByRuleId.TryGetValue(rule.Id, out var implementation))
                implementation.Check(new RuleContext(d, t, rule, report));
        }
    }

    private static bool RequiredWhenHolds(RequiredWhen? condition, Dictionary<string, YamlNode> present)
        => condition is not null
           && present.TryGetValue(condition.Field, out var node)
           && condition.Holds(Yaml.Raw(node));

    /// <summary>Dropped in silence, as <c>ref:</c> resolution is. See Checks/SchemaChecks.cs.</summary>
    private static bool Unfillable(FieldSpec spec, IReadOnlySet<string>? fillable)
        => fillable is not null
           && spec.Refs.Count > 0
           && spec.AllowLiteral.Count == 0
           && !spec.Refs.Any(fillable.Contains);

    /// <summary>A type the corpus declined may still be supplied by an import.</summary>
    private static HashSet<string> Fillable(LoadedCorpus corpus)
    {
        var folders = corpus.Adopted.Select(t => t.Folder).ToHashSet(StringComparer.Ordinal);

        var supplied = corpus.Imports.Imports
            .SelectMany(i => i.Records)
            .Select(r => r.Type)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var t in corpus.Schema.ByFolder.Values)
            if (supplied.Contains(t.TypeName))
                folders.Add(t.Folder);

        return folders;
    }

    private static string Count(int n, string one, string many) => $"{n} {(n == 1 ? one : many)}";

    /// <summary>
    /// A label used for singular and plural alike is a mass noun and takes no article. A label in capitals takes the
    /// article its first letter's name wants: "an ADR", "an NFR".
    /// </summary>
    private static string WithArticle(TypeSchema t)
    {
        var name = t.DisplayName;
        if (name.Length == 0) return name;
        if (!string.IsNullOrEmpty(t.LabelPlural) && string.Equals(t.Label, t.LabelPlural, StringComparison.Ordinal))
            return name;

        var vowel = name.All(char.IsAsciiLetterUpper)
            ? "AEFHILMNORSX".Contains(name[0])
            : "AEIOU".Contains(char.ToUpperInvariant(name[0]));
        return vowel ? $"an {name}" : $"a {name}";
    }
}
