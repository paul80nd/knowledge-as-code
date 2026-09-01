using System.Text.Json;
using Xunit.Sdk;

namespace kac.tests;

// Reading a field a fixture is asserted to hold. `GetString` answers null for a property that is absent
// or JSON null, so a test naming a field it expects reports the field it did not find rather than a
// `NullReferenceException` several lines later.
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
}
