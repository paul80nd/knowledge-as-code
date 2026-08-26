using System.Text;
using kac.core;

// What every type publishes, written out and pinned.
//
// A type's `export.version:` is moved by hand, so nothing but a diff stands between a line whose keys
// moved and a consumer still reading it against the number that did not move. This is that diff.
//
// Whether a shape is the right one is not the question here. What this catches is a shape changing
// quietly: edit a `line:`, a `fields:` or a fidelity and this fails, which is where the version beside
// it gets moved. Update the block below in the same commit. `docs/cli/export.md` says which edits
// oblige the number to move and which leave it where it is.

namespace kac.tests;

public class ExportShapeTests
{
    private const string Pinned =
        """
        glossary@1
          fields: id, title, narrows, status, review-by
          sections: Scope=full
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
        policies@2
          fields: id, title, category, status, review-by
          sections: Purpose=summary, Scope=full, Exceptions=full
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
            var export = t.Export!;
            shapes.AppendLine($"{t.Key}@{export.Version}");
            shapes.AppendLine($"  fields: {string.Join(", ", export.Fields)}");
            var sections = export.Sections.Select(s => $"{s.Section}={s.Fidelity}");
            shapes.AppendLine($"  sections: {string.Join(", ", sections)}");

            if (!export.PartsDeclared) continue;

            shapes.AppendLine($"  parts: {export.Parts}");
            foreach (var (key, source) in export.Line) shapes.AppendLine($"    {key}: {source}");
        }

        return shapes.ToString().TrimEnd();
    }
}
