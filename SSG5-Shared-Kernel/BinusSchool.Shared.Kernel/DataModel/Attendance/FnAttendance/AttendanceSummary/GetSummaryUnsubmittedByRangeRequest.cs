using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class GetSummaryUnsubmittedByRangeRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string IdUser { get; set; }
        public string SelectedPosition { get; set; }
    }
}
