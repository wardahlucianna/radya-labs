using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.Attendance
{
    public class GetEventAttendanceResult : ItemValueVm
    {
        public string Name { get; set; }
        public DateTimeRange Date { get; set; }
        public int UnsaveAbsence { get; set; }
        public int UnexcuseAbsence { get; set; }
        public int TotalStudent { get; set; }
        public AuditableResult Audit { get; set; }
    }
}
