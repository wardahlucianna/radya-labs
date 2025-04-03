using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.Attendance
{
    public class GetEventAttendanceRequest
    {
        public DateTime Date { get; set; }
        public string IdUser { get; set; }
        public string IdHomeroom { get; set; }
    }
}