using System;

namespace LockedIn.BusinessObject.Common;

public static class VietnamTimeZoneHelper
{
    private static readonly TimeZoneInfo VietnamTimeZone = GetVietnamTimeZone();

    private static TimeZoneInfo GetVietnamTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        }
    }

    /// <summary>
    /// Combines a Vietnam local calendar date with a Vietnam local time span, and converts it to a UTC DateTime (DateTimeKind.Utc).
    /// </summary>
    public static DateTime CreateUtcFromVietnamDateAndTime(DateTime vietnamDate, TimeSpan vietnamTime)
    {
        var localUnspecified = new DateTime(
            vietnamDate.Year,
            vietnamDate.Month,
            vietnamDate.Day,
            vietnamTime.Hours,
            vietnamTime.Minutes,
            vietnamTime.Seconds,
            DateTimeKind.Unspecified
        );
        return TimeZoneInfo.ConvertTimeToUtc(localUnspecified, VietnamTimeZone);
    }

    /// <summary>
    /// Ensures that a DateTime is marked with DateTimeKind.Utc so JSON serializers format it with a 'Z' suffix.
    /// </summary>
    public static DateTime EnsureUtcKind(DateTime dt)
    {
        if (dt.Kind == DateTimeKind.Utc) return dt;
        return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
    }

    public static DateTime? EnsureUtcKind(DateTime? dt)
    {
        if (!dt.HasValue) return null;
        return EnsureUtcKind(dt.Value);
    }
}
