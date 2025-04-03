using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2
{
    public class GetClassIdByHomeroomResult
    {
        public string ClassId { get; set; }
        public GetClassIdSession Session { get; set; }
        public int Submitted { get; set; }
        public int Unsubmitted { get; set; }
        public int Pending { get; set; }
        public int TotalStudent { get; set; }
        public DateTime? LastSavedAt { get; set; }
        public string LastSavedBy { get; set; }
    }

    public class GetClassIdSession
    {
        public string Id { get; set; }
        public string SessionId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
