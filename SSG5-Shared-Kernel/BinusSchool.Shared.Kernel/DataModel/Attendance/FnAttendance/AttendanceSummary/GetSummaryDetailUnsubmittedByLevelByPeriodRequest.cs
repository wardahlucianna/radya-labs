using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class GetSummaryDetailUnsubmittedByLevelByPeriodRequest : CollectionSchoolRequest
    {
        public int Semester { get; set; }
        public string IdHomeroom { get; set; }
    }
}
