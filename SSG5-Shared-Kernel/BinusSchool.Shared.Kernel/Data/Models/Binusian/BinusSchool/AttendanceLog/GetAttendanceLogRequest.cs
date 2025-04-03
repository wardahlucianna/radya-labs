using System.Collections.Generic;

namespace BinusSchool.Data.Models.Binusian.BinusSchool.AttendanceLog
{
    public class GetAttendanceLogRequest
    {
        public string IdSchool { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int StartHour { get; set; }
        public int StartMinutes { get; set; }
        public int EndHour { get; set; }
        public int EndMinutes { get; set; }
        public IEnumerable<string> ListStudent { get; set; }
    }
}
