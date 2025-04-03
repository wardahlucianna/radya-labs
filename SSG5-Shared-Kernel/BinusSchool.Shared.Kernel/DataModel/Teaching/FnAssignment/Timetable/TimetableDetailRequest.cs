using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.Timetable
{
    public class TimetableDetailRequest
    {
        public ICollection<string> IdTimetablePrefHeader { get; set; }
        public ICollection<TimetableDetailDetailRequest> DetailRequests { get; set; }
    }
    public class TimetableDetailDetailRequest
    {
        public string Action { get; set; }
        public CountAndLength OldValue { get; set; }
        public CountAndLength NewValue { get; set; }

    }

    public class CountAndLength
    {
        public int Count { get; set; }
        public int Length { get; set; }
    }
}
