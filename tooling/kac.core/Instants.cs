using System.Globalization;

namespace kac.core;

// Shared so a rule written as an `expr:` and one written in C# measure the same way. See
// https://paul80nd.github.io/knowledge-as-code/design/expressions/ for what `span()` and `days()` promise.
public static class Instants
{
    // Null where `timestamp-format` would refuse the value.
    public static DateTimeOffset? Read(string? value) =>
        value is not null
        && DateTimeOffset.TryParseExact(value, "yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var moment)
            ? moment
            : null;

    // Null where `date-format` would refuse the value.
    public static DateOnly? ReadDay(string? value) =>
        value is not null
        && DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var day)
            ? day
            : null;

    // Zero where either value is absent or not a date, and where the second is the earlier: each is another
    // check's to report, so a caller guards on them. A same-day gap is zero too.
    public static int Days(string? from, string? to) =>
        ReadDay(from) is { } start && ReadDay(to) is { } end && end >= start
            ? end.DayNumber - start.DayNumber
            : 0;

    // The time between two moments as an ISO 8601 duration in hours, minutes and seconds, never days, which ISO 8601
    // gives no fixed length. Empty where either value is absent or not a moment, or where the second is earlier.
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
