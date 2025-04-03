namespace BinusSchool.Scheduling.Kernel.Abstractions;

public interface IMachineDateTime
{
    DateTime ServerTime { get; }
}

public class MachineDateTime : IMachineDateTime
{
    public DateTime ServerTime => DateTimeUtil.ServerTime;
}

public static class DateTimeUtil
{
    private const int OffsetHour = 7;

    /// <summary>
    /// Server using UTC+7
    /// </summary>
    public static DateTime ServerTime => DateTime.UtcNow.AddHours(OffsetHour);

    public static bool IsIntersect(DateTimeRange source, DateTimeRange compare)
    {
        return IsIntersect(source.Start, source.End, compare.Start, compare.End);
    }

    private static bool IsIntersect(DateTime sourceStart, DateTime sourceEnd, DateTime compareStart,
        DateTime compareEnd)
    {
        if (sourceStart > sourceEnd || compareStart > compareEnd)
            throw new ArgumentOutOfRangeException();

        if (sourceStart == sourceEnd || compareStart == compareEnd)
            return false; // No actual date range

        if (sourceStart == compareStart || sourceEnd == compareEnd)
            return true; // If any set is the same time, then by default there must be some overlap. 

        if (sourceStart < compareStart)
        {
            if (sourceEnd > compareStart && sourceEnd < compareEnd)
                return true;

            if (sourceEnd > compareEnd)
                return true;
        }
        else
        {
            if (compareEnd > sourceStart && compareEnd < sourceEnd)
                return true;

            if (compareEnd > sourceEnd)
                return true;
        }

        return false;
    }

    private static IEnumerable<(DateTime start, DateTime end)> ToEachDay(DateTime from, DateTime until)
    {
        for (var day = from; day.Date <= until.Date; day = day.AddDays(1))
        {
            yield return (day, day.Add(-from.TimeOfDay).Add(until.TimeOfDay));
        }
    }

    public static IEnumerable<DateTime> ToEachMonth(DateTime from, DateTime until)
    {
        // set start-date to start of month
        from = new DateTime(from.Year, from.Month, 1);
        // set end-date to end of month
        until = new DateTime(until.Year, until.Month, DateTime.DaysInMonth(until.Year, until.Month));

        return Enumerable.Range(0, int.MaxValue).Select(e => from.AddMonths(e)).TakeWhile(e => e <= until);
    }

    public static int GetTotalDayBetweenRange(params DateTimeRange[] ranges)
    {
        return ranges.SelectMany(x => ToEachDay(x.Start, x.End)).Count();
    }
}