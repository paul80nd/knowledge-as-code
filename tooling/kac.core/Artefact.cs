namespace kac.core;

// `_` marks a framework artefact. Inside a type folder that is the generated index and the template a
// contributor copies; at the corpus root it is the scaffolding directories. The tool tests the prefix
// and not the names below, so the rule holds for anything a corpus adds under it. A reader scanning a
// folder finds the framework's files grouped above the records, because `_` sorts ahead of letters
// whether or not the listing folds case.
//
// A knowledge record therefore never begins with `_`. Nothing reports it, because there is nothing to
// report: a file taking the prefix is read as an artefact and is never discovered as a record.
public static class Artefact
{
    private const char Prefix = '_';

    // The index `kac generate` writes into every collection type's folder.
    public const string Index = "_index.md";

    // The template a contributor copies. Written by hand, and held to the schema it teaches. A template
    // is not a record, so the questions needing an id or a filename pass it by. Every other defect in it
    // reaches every document copied from it. See Validator.CheckTemplateFields.
    public const string Template = "_template.md";

    // Whether any segment of a repo-relative path takes the prefix. Asked of the whole path, so a
    // reserved directory carries its meaning down to everything inside it.
    public static bool IsReserved(string rel) =>
        rel.Replace('\\', '/').Split('/').Any(p => p.StartsWith(Prefix));
}
