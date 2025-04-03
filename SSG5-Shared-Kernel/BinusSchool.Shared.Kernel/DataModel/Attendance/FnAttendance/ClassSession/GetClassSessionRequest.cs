using System;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.ClassSession
{
    public class GetClassSessionRequest
    {
        public string IdHomeroom { get; set; }
        public string IdUser { get; set; }
        public string IdSchool { get; set; }
        public DateTime Date { get; set; }
    }
}
