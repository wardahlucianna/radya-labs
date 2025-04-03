
using System;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.EventAttendanceEntry
{
    public class GetEventAttendanceEntryRequest
    {
        public string IdEventCheck { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public string IdSubject { get; set; }
        public bool? IsSubmitted { get; set; }
        public DateTime Date { get; set; }
        public string IdEvent { get; set; }
        public string IdUser { get; set; }
        public string IdAcademicYear { get; set; }
    }
}
