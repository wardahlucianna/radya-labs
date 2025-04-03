using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.Timetable
{
    public class GetTimeTableByUserResult
    {
        public string Id { get; set; }
        public string Department { get; set; }
        public string Subject { get; set; }
        public string Grade { get; set; }
        public string Class { get; set; }
        public int TotalSession { get; set; }
    }
}
