using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceRecap
{
    public class GetDetailAttendanceRecapRequest : CollectionRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdStudent { get; set; }
        public string IdHomeroom { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
    }
}
