using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendanceLongrun.Longrun.AttendanceSummaryTerm
{
    public class GetPrasentResult
    {
        public string IdStudent { get; set; }
        public string IdBinusian { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Grade { get; set; }
        public string Class { get; set; }
        public string ClassId { get; set; }
        public int Session { get; set; }
        public string Status { get; set; }
        public string Teacher { get; set; }
        public string Type { get; set; }
    }
}
