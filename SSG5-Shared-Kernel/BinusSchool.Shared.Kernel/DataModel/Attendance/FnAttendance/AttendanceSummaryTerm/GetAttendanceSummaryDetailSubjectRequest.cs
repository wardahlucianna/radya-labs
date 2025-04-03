using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDetailSubjectRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public string SelectedPosition { get; set; }
        public string IdUser { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdSubject { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
