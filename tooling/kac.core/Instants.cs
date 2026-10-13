using System.Globalization;

namespace kac.core;

/// <summary>
/// Shared so a rule written as an <c>expr:</c> and one written in C# measure the same way. What <c>span()</c> and
/// <c>days()</c> promise is at https://paul80nd.github.io/knowledge-as-code/reference/expressions/.
/// </summary>
public static class Instants
{
    /// <summary>Null where <c>timestamp-format</c> would refuse the value.</summary>
    public static DateTimeOffset? Read(string? value) =>
        value is not null
        && DateTimeOffset.TryParseExact(value, "yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var moment)
            ? moment
            : null;

    /// <summary>Null where <c>date-format</c> would refuse the value.</summary>
    public static DateOnly? ReadDay(string? value) =>
        value is not null
        && DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var day)
            ? day
            : null;

    /// <summary>
    /// Zero where either value is absent or not a date, and where the second is the earlier: each is another check's to
    /// report, so a caller guards on them. A same-day gap is zero too.
    /// </summary>
    public static int Days(string? from, string? to) =>
        ReadDay(from) is { } start && ReadDay(to) is { } end && end >= start
            ? end.DayNumber - start.DayNumber
            : 0;

    /// <summary>
    /// The time between two moments as an ISO 8601 duration in hours, minutes and seconds, never days, which ISO 8601
    /// gives no fixed length. Empty where either value is absent or not a moment, or where the second is earlier.
    /// </summary>
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
