using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.Attendance
{
    public class GetMasterDataAttendanceRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public AttendanceCategory? AttendanceCategory  { get; set; }
        public AbsenceCategory? AbsenceCategory { get; set; }
    }
}
