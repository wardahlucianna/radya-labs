using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class GetSummaryDetailUnsubmittedByLevelByRangeRequest : CollectionSchoolRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string IdHomeroom { get; set; }
    }
}
