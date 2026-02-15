using JetBrains.Annotations;

namespace Jitzu.Core.Runtime.Extensions;

[UsedImplicitly]
public static class DateOnlyExtensions
{
    public static DateOnly Today()
    {
        return DateOnly.FromDateTime(DateTime.Today);
    }

    [UsedImplicitly]
    public static int DaysSince(this DateOnly left, DateOnly other)
    {
        return left.DayNumber - other.DayNumber;
    }
}