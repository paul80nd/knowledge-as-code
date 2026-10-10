using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit.Sdk;
using kac.core;

namespace kac.tests;

/// <summary>
/// Reads a value a test has already arranged to be there. The read throws where it happened and says what it expected,
/// rather than leaving a <c>NullReferenceException</c> several lines further on.
/// </summary>
internal static class Required
{
    /// <summary>
    /// For a case about something else, where any day would do. A named day, so a rule written against <c>today()</c>
    /// cannot make an unrelated case start failing on a date nobody chose.
    /// </summary>
    internal static readonly DateOnly Today = new(2026, 6, 15);

    internal static string Text(this JsonElement element, string property) =>
        element.TryGetProperty(property, out var found) && found.GetString() is { } text
            ? text
            : throw new XunitException($"Expected a '{property}' holding text, and found {element}.");

    internal static string Text(this JsonElement element) =>
        element.GetString() ?? throw new XunitException($"Expected text, and found {element}.");

    internal static NewAnswers Resolved(this Asking.Answered answered) =>
        answered.Answers
        ?? throw new XunitException($"Expected answers, and the run refused: {answered.Problem}");

    internal static TemplateSource Fetched(this TemplateSource.Fetch fetch) =>
        fetch.Source
        ?? throw new XunitException($"Expected a template, and the fetch refused: {fetch.Problem}");

    internal static Doc Parsed(string rel, string text, Schema schema, bool requireFrontmatter = true) =>
        Doc.Parse(rel, text, schema, requireFrontmatter)
        ?? throw new XunitException($"'{rel}' did not parse as a record.");

    internal static TypeSchema TypeOf(this Doc doc) =>
        doc.Type ?? throw new XunitException($"'{doc.Rel}' was read as no type.");

    internal static string Scalar(this Doc doc, string field) =>
        doc.FrontScalar(field) ?? throw new XunitException($"'{doc.Rel}' carries no '{field}'.");

    internal static ExportManifest Manifest(string json) =>
        Exporter.ReadManifest(json)
        ?? throw new XunitException("The text did not read as an export manifest.");

    internal static JsonNode Json(string text) =>
        JsonNode.Parse(text) ?? throw new XunitException("Expected JSON, and the text held null.");
}
