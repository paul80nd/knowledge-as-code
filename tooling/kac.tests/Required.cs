using System.Text.Json;
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
}
