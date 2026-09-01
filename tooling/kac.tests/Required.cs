using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit.Sdk;
using kac.core;

namespace kac.tests;

// Reading a value a test has already arranged to be there: a field of a fixture, or the half of a
// result the run it set up was going to produce. Each underlying member is nullable for a case the
// test has ruled out, so the read throws where it happened and says what it expected, rather than
// leaving a `NullReferenceException` several lines further on.
internal static class Required
{
    // The text of a property, named in the failure so the assertion says which field was missing.
    internal static string Text(this JsonElement element, string property) =>
        element.TryGetProperty(property, out var found) && found.GetString() is { } text
            ? text
            : throw new XunitException($"Expected a '{property}' holding text, and found {element}.");

    // The text of an element already reached, for an array whose items are strings.
    internal static string Text(this JsonElement element) =>
        element.GetString() ?? throw new XunitException($"Expected text, and found {element}.");

    // The answers a test drove `Asking.Resolve` to produce. Such a test has arranged a run that
    // succeeds, so a problem is the failure, and the message carries what the run said instead.
    internal static NewAnswers Resolved(this Asking.Answered answered) =>
        answered.Answers
        ?? throw new XunitException($"Expected answers, and the run refused: {answered.Problem}");

    // The template a test drove `TemplateSource` to fetch, on the same footing.
    internal static TemplateSource Fetched(this TemplateSource.Fetch fetch) =>
        fetch.Source
        ?? throw new XunitException($"Expected a template, and the fetch refused: {fetch.Problem}");

    // A document parsed from markdown a test wrote. `Doc.Parse` answers null for text it cannot read as
    // a record, and a test writing that text is asserting something else, so the failure says which
    // path was being parsed.
    internal static Doc Parsed(string rel, string text, Schema schema, bool requireFrontmatter = true) =>
        Doc.Parse(rel, text, schema, requireFrontmatter)
        ?? throw new XunitException($"'{rel}' did not parse as a record.");

    // The type a parsed document was read as. A test that fixed the folder has fixed the type with it.
    internal static TypeSchema TypeOf(this Doc doc) =>
        doc.Type ?? throw new XunitException($"'{doc.Rel}' was read as no type.");

    // A frontmatter scalar a test wrote into the document it is now asserting on.
    internal static string Scalar(this Doc doc, string field) =>
        doc.FrontScalar(field) ?? throw new XunitException($"'{doc.Rel}' carries no '{field}'.");

    // JSON a test wrote, or a command under test produced.
    internal static JsonNode Json(string text) =>
        JsonNode.Parse(text) ?? throw new XunitException("Expected JSON, and the text held null.");
}
