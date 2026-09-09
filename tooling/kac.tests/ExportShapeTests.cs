using System.Text;
using kac.core;

// What every type publishes, written out and pinned.
//
// A type's `export.version:` is moved by hand, so nothing but a diff stands between a line whose keys
// moved and a consumer still reading it against the number that did not move. This is that diff.
//
// Whether a shape is the right one is not the question here. What this catches is a shape changing
// quietly: edit a `line:`, a `fields:` or a fidelity and this fails, which is where the version beside
// it gets moved. Update the block below in the same commit. `docs/design/export.md` says which edits
// oblige the number to move and which leave it where it is.

namespace kac.tests;

[Trait(Kind.Of, Kind.Repository)]
public class ExportShapeTests
{
    private const string Pinned =
        """
        adrs@1
          fields: id, title, status, decided-on, supersedes, superseded-by, related, tags
          sections:
            Context: summary
            Decision: full
            Consequences: full
        controls@1
          fields: id, title, status, verifies, mechanism, frequency, evidence, applies-to, tags
          sections:
            What it checks: full
            How it works: full
            Coverage and gaps: full
        deviations@1
          fields: id, title, status, departs-from, owner, accepted-on, review-by, closed-on, applies-to, tags
          sections:
            What we are doing instead: full
            Why we need it: full
            What compensates: full
            How it closes: full
            Scope: full
        fixes@1
          fields: id, title, status, symptom-keywords, applies-to, verified, review-by, tags
          sections:
            Symptom: full
            Cause: full
            Resolution: full
        glossary@1
          fields: id, title, narrows, status, review-by, tags
          sections:
            Scope: full
          parts: full
            id: part.id
            title: part.text
            definition: part.lead
            not: part.aside
            seeAlso: part.see-also
            type: record.type
            record: record.id
            part: part.key
            status: front.status
            reviewBy: front.review-by
            path: record.path
            anchor: part.anchor
        nfrs@1
          fields: id, title, status, applies-to, target, measured-by, constrained-by, review-by, tags
          sections:
            Target: full
            How it is measured: full
            If it is breached: full
            Constraints: full
        policies@2
          fields: id, title, category, status, review-by
          sections:
            Purpose: summary
            Scope: full
            Exceptions: full
          frameworks: frameworks.jsonl
          parts: full
            id: part.id
            clause: part.text
            level: part.level
            type: record.type
            record: record.id
            part: part.key
            status: front.status
            reviewBy: front.review-by
            path: record.path
            anchor: part.anchor
        processes@1
          fields: id, title, status, applies-to, last-rehearsed, rehearsal-frequency, tags
          sections:
            When to use this: full
            Prerequisites: full
        reports@2
          fields: id, title, status, generated, sources, verified, tags
        runbooks@1
          fields: id, title, status, severity, applies-to, last-rehearsed, rehearsal-frequency, requires-access, tags
          sections:
            Symptoms: full
        services@1
          fields: id, title, status, platform, criticality, repo, depends-on, data-stores, facets, tags
          sections:
            What it does: full
            Where it lives: full
            Dependencies: full
            Data: full
        standards@1
          fields: id, title, category, status, derived-from, implements, applies-to, review-by, tags
          sections:
            Summary: full
            Conformance checklist: full
          parts: full
            id: part.id
            title: part.text
            obligations: part.lead
            covers: part.citations.Covers
            seeAlso: part.see-also
            type: record.type
            record: record.id
            part: part.key
            status: front.status
            reviewBy: front.review-by
            path: record.path
            anchor: part.anchor
        tools@1
          fields: id, title, status, category, versions, licence, decided-in, replaces, successor, tags
          sections:
            What we use it for: full
            Status: full
            Licence and obligations: full
            Where it is used: full
        """;

    [Fact]
    public void Every_exported_type_publishes_the_shape_pinned_here()
        => Assert.Equal(Pinned, Declared(), ignoreLineEndingDifferences: true);

    // The schema's own account of what each type contributes, rendered so that a reader of the diff sees
    // the keys rather than a hash. Types in folder order, and everything else in the order it is
    // declared, because a consumer reads a line's keys in that order.
    private static string Declared()
    {
        var shapes = new StringBuilder();

        foreach (var t in Schema.Load(Repo.Root).ByFolder.Values
                     .Where(t => t.Export is not null)
                     .OrderBy(t => t.Key, StringComparer.Ordinal))
        {
            var export = t.DeclaredExport;
            shapes.AppendLine($"{t.Key}@{export.Version}");
            shapes.AppendLine($"  fields: {string.Join(", ", export.Fields)}");
            if (export.Sections.Count > 0) shapes.AppendLine("  sections:");
            foreach (var s in export.Sections) shapes.AppendLine($"    {s.Section}: {s.Fidelity}");

            if (export.Frameworks.Length > 0) shapes.AppendLine($"  frameworks: {export.Frameworks}");

            if (!export.PartsDeclared) continue;

            shapes.AppendLine($"  parts: {export.Parts}");
            foreach (var (key, source) in export.Line) shapes.AppendLine($"    {key}: {source}");
        }

        return shapes.ToString().TrimEnd();
    }
}
