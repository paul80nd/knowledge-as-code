namespace kac.core;

// `_` marks a framework artefact: the generated index and template inside a type folder, and the scaffolding
// directories at the corpus root. The tool tests the prefix, not the names, so a file taking it is never discovered as
// a record.
public static class Artefact
{
    private const char Prefix = '_';

    // The index `kac generate` writes into every collection type's folder.
    public const string Index = "_index.md";

    // The template a contributor copies. It is held to the schema it teaches, though checks needing an id or a
    // filename pass it by. See Validator.CheckTemplateFields.
    public const string Template = "_template.md";

    // Whether any segment of a repo-relative path takes the prefix, so a reserved directory covers everything inside
    // it.
    public static bool IsReserved(string rel) =>
        rel.Replace('\\', '/').Split('/').Any(p => p.StartsWith(Prefix));
}
