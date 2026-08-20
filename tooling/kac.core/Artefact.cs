// ---------------------------------------------------------------------------
// The reserved `_` prefix
// ---------------------------------------------------------------------------

namespace kac.core;

// `_` means framework artefact rather than knowledge record. Inside a type folder that is the
// generated index and the template a contributor copies; at the corpus root it is the scaffolding
// directories. The prefix is what the tool tests rather than the names below, so the rule holds for
// anything a corpus adds under it, and a reader scanning a folder finds the framework's files
// grouped above the records in any listing — `_` sorts ahead of letters whether or not the listing
// folds case.
//
// The corollary is that a knowledge record may not begin with `_`. Nothing reports that, because
// there is nothing to report: a file taking the prefix is read as an artefact, and is never
// discovered as a record in the first place.
public static class Artefact
{
    private const char Prefix = '_';

    // The index `kac generate` writes into every collection type's folder.
    public const string Index = "_index.md";

    // The template a contributor copies. Written by hand, and held to the schema it teaches: it is not a
    // record, so it answers to none of the questions that need an id or a filename, but every other
    // defect in it is inherited by every document copied from it. See Validator.CheckTemplateFields.
    public const string Template = "_template.md";

    // Whether any segment of a repo-relative path takes the prefix. Asked of the whole path rather
    // than the filename so that a reserved directory carries its meaning down to everything inside it.
    public static bool IsReserved(string rel) =>
        rel.Replace('\\', '/').Split('/').Any(p => p.StartsWith(Prefix));
}
