using System;

namespace BinusSchool.Persistence.AttendanceDb.Models
{
    public class PeriodDto : GradeDto
    {
        public string IdPeriod { get; set; }
        public string PeriodCode { get; set; }
        public string PeriodName { get; set; }
        public int PeriodOrder { get; set; }
        public DateTime PeriodStartDt { get; set; }
        public DateTime PeriodEndDt { get; set; }
        public DateTime PeriodAttendanceStartDt { get; set; }
        public DateTime PeriodAttendanceEndDt { get; set; }
        public int PeriodSemester { get; set; }
    }
}
