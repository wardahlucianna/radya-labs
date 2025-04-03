using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryUnsubmitedRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public string SelectedPosition { get; set; }
        public string IdUser { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public string IdClassroom { get; set; }
        public string IdSubject { get; set; }
        public string ClassId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SchoolId { get; set; }
    }
}
