using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceRecap
{
    public class GetDataDetailAttendanceRecapResult : CodeWithIdVm
    {
        public DateTime? Date { get; set; }
        public string ClassId { get; set; }
        public string Session { get; set; }
        public string Homeroom { get; set; }
        public AbsenceCategory? AbsenceCategory { get; set; }
    }
}
