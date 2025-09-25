#pragma warning disable IDE0130 // Namespace does not match folder structure

#nullable enable

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace System;

/// <summary>
/// Provides extension methods for <see cref="DateTime"/> and <see cref="DateTimeOffset"/> to enhance date and time manipulation operations.
/// </summary>
[ExcludeFromCodeCoverage]
#if PUBLIC_EXTENSIONS
public
#endif
 static class DateTimeExtensions
{
    /// <summary>
    /// Converts a <see cref="DateTime"/> to the specified timezone.
    /// Handles different <see cref="DateTimeKind"/> values appropriately, treating unspecified dates as UTC.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTime"/> to convert.</param>
    /// <param name="timeZoneInfo">The target timezone to convert to.</param>
    /// <returns>A <see cref="DateTime"/> representing the same moment in time in the specified timezone.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="timeZoneInfo"/> is null.</exception>
    public static DateTime ToTimeZone(this DateTime dateTime, TimeZoneInfo timeZoneInfo)
    {
        ArgumentNullException.ThrowIfNull(timeZoneInfo);

        return dateTime.Kind switch
        {
            DateTimeKind.Local => TimeZoneInfo.ConvertTime(dateTime, timeZoneInfo),
            DateTimeKind.Utc => TimeZoneInfo.ConvertTimeFromUtc(dateTime, timeZoneInfo),
            DateTimeKind.Unspecified => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc), timeZoneInfo),
            _ => TimeZoneInfo.ConvertTime(dateTime, timeZoneInfo)
        };
    }

    /// <summary>
    /// Converts a <see cref="DateTimeOffset"/> to the specified timezone.
    /// </summary>
    /// <param name="dateTimeOffset">The <see cref="DateTimeOffset"/> to convert.</param>
    /// <param name="timeZoneInfo">The target timezone to convert to.</param>
    /// <returns>A <see cref="DateTimeOffset"/> representing the same moment in time in the specified timezone.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="timeZoneInfo"/> is null.</exception>
    public static DateTimeOffset ToTimeZone(this DateTimeOffset dateTimeOffset, TimeZoneInfo timeZoneInfo)
    {
        ArgumentNullException.ThrowIfNull(timeZoneInfo);

        return TimeZoneInfo.ConvertTime(dateTimeOffset, timeZoneInfo);
    }

    /// <summary>
    /// Gets the week number of the year for the specified <see cref="DateTime"/> according to the given calendar rules.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTime"/> to get the week number for.</param>
    /// <param name="firstDayOfWeek">The first day of the week. Defaults to <see cref="DayOfWeek.Sunday"/>.</param>
    /// <param name="weekRule">The calendar week rule that defines the first week of the year. Defaults to <see cref="CalendarWeekRule.FirstFourDayWeek"/>.</param>
    /// <returns>An integer representing the week number of the year (1-based).</returns>
    public static int WeekNumber(
        this DateTime dateTime,
        DayOfWeek firstDayOfWeek = DayOfWeek.Sunday,
        CalendarWeekRule weekRule = CalendarWeekRule.FirstFourDayWeek)
    {
        Calendar calendar = CultureInfo.InvariantCulture.Calendar;
        return calendar.GetWeekOfYear(dateTime, weekRule, firstDayOfWeek);
    }

    /// <summary>
    /// Gets the week number of the year for the specified <see cref="DateTimeOffset"/> according to the given calendar rules.
    /// Uses the <see cref="DateTime"/> component of the <see cref="DateTimeOffset"/> for the calculation.
    /// </summary>
    /// <param name="dateTimeOffset">The <see cref="DateTimeOffset"/> to get the week number for.</param>
    /// <param name="firstDayOfWeek">The first day of the week. Defaults to <see cref="DayOfWeek.Sunday"/>.</param>
    /// <param name="weekRule">The calendar week rule that defines the first week of the year. Defaults to <see cref="CalendarWeekRule.FirstFourDayWeek"/>.</param>
    /// <returns>An integer representing the week number of the year (1-based).</returns>
    public static int WeekNumber(
        this DateTimeOffset dateTimeOffset,
        DayOfWeek firstDayOfWeek = DayOfWeek.Sunday,
        CalendarWeekRule weekRule = CalendarWeekRule.FirstFourDayWeek)
    {
        return WeekNumber(dateTimeOffset.DateTime, firstDayOfWeek, weekRule);
    }
}
