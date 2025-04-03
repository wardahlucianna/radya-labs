using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.ApprovalAttendanceAdministration
{
    public class GetListAttendanceAdministrationApprovalRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public int Semester { get; set; }
        public string IdHomeroom { get; set; }
        public AttendanceCategory? AttendanceCategory { get; set; }
        public int? Status { get; set; }

    }
}
