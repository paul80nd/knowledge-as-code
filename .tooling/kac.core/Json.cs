using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace kac.core;
// ---------------------------------------------------------------------------
// JSON output models
//
// Every JSON document kac emits is a record serialized through the source generator
// (KacJson) — reflection-based serialization is disabled under AOT. To add a new document
// (e.g. `digest` or `drift` output): declare its record shape here and add one
// `[JsonSerializable(typeof(...))]` line to KacJson; no other plumbing is needed.
//
// Property names are PascalCase and emitted camelCase; `int?` line is written as `null`.
// ---------------------------------------------------------------------------

public record ValidateReport(ValidateSummary Summary, IReadOnlyList<ValidateFinding> Findings);

public record ValidateSummary(int Validated, int Templates, int Skipped, int Errors, int Warnings);

public record ValidateFinding(string File, int? Line, string Severity, string Check, string Message);

public record ChecksReport(IReadOnlyList<CheckInfo> Checks);

public record CheckInfo(string Check, string Severity, string Summary);

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
[JsonSerializable(typeof(ValidateReport))]
[JsonSerializable(typeof(ChecksReport))]
public partial class KacJson : JsonSerializerContext
{
    private static KacJson? _relaxed;

    // Shared context for CLI output: the source-generated metadata from Default, plus
    // relaxed escaping so quotes and dashes in messages stay human-readable rather than
    // becoming ' / —. Lazily initialised so it does not touch the generator's Default
    // during static construction (their init order across the partial is unspecified).
    public static KacJson Relaxed => _relaxed ??= new KacJson(new JsonSerializerOptions(Default.Options)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });
}
