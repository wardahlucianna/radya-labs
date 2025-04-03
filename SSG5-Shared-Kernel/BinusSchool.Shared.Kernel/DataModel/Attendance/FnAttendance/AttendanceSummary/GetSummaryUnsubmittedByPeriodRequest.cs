using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class GetSummaryUnsubmittedByPeriodRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public int Semester { get; set; }
        public string IdUser { get; set; }
        public string SelectedPosition { get; set; }
    }
}
