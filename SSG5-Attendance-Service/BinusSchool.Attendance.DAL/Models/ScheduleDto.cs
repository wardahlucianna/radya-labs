using System;

namespace BinusSchool.Persistence.AttendanceDb.Models
{
    public class ScheduleDto
    {
        public string IdSchedule { get; set; }
        public DateTime ScheduleDate { get; set; }
        public string IdLesson { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdSubject { get; set; }
        public string IdSession { get; set; }
        public string SessionID { get; set; }
        public string IdDay { get; set; }
    }
}
