﻿namespace BinusSchool.Document.Kernel;

public class DateTimeRange
{
    public DateTimeRange() { }
        
    public DateTimeRange(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
    }

    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}