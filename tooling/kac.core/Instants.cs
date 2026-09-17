using System.Globalization;

namespace kac.core;

// Reading a moment a record states, and measuring between two of them. Shared so that a rule written
// as an `expr:` and a rule written in C# answer the same question the same way.
//
// See https://paul80nd.github.io/knowledge-as-code/design/expressions/ for what `span()` and `days()`
// promise the rule that calls them.
public static class Instants
{
    // Null where the value is not the one shape `timestamp-format` admits, so a value that check has
    // already refused reaches no rule as a moment.
    public static DateTimeOffset? Read(string? value) =>
        value is not null
        && DateTimeOffset.TryParseExact(value, "yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var moment)
            ? moment
            : null;

    // Null where the value is not the one shape `date-format` admits, so a value that check has already
    // refused reaches no rule as a day.
    public static DateOnly? ReadDay(string? value) =>
        value is not null
        && DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var day)
            ? day
            : null;

    // The whole days from the first day to the second, so a rule compares a gap against a threshold a
    // schema states. Days, where Span answers in hours: these are date fields, and a date has no time
    // of day to lose.
    //
    // Zero where either value is absent or is not a date, and where the second is the earlier. Each of
    // those is another check's to report, and a caller guards on them rather than reporting the same
    // fault twice. A same-day gap is zero too, which no caller needs to tell apart from those.
    public static int Days(string? from, string? to) =>
        ReadDay(from) is { } start && ReadDay(to) is { } end && end >= start
            ? end.DayNumber - start.DayNumber
            : 0;

    // The time between two moments, as an ISO 8601 duration in hours, minutes and seconds. Never days:
    // ISO 8601 gives a day no fixed length, so a caller comparing the text would take one spelling of a
    // span and refuse the other.
    //
    // Empty where either value is absent or is not a moment, and where the second is the earlier. Each
    // of those is another check's to report, and a caller guards on them rather than reporting the same
    // fault twice.
    public static string Span(string? from, string? to)
    {
        if (Read(from) is not { } start || Read(to) is not { } end || end < start) return "";
        if (end == start) return "PT0S";

        var gap = end - start;
        var span = "PT";
        if ((int)gap.TotalHours > 0) span += $"{(int)gap.TotalHours}H";
        if (gap.Minutes > 0) span += $"{gap.Minutes}M";
        if (gap.Seconds > 0) span += $"{gap.Seconds}S";

        return span;
    }
}
