using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.Timetable
{
    public class TimetableResult
    {
        public string  Id { get; set; }
        public bool IsMerge { get; set; }
        public bool CanDelete { get; set; }
        public bool Status { get; set; }
    }
}
