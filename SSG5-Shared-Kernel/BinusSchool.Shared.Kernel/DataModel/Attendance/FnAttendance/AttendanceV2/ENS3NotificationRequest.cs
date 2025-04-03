using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2
{
    public class ENS3NotificationRequest
    {
        public string IdUser { get; set; }
        public List<Ens3NotificationEvent> Event { get; set; }
    }

    public class Ens3NotificationEvent
    {
        public string IdEvent { get; set; }
        public string EventName { get; set; }
        public DateTime StartDatetime { get; set; }
        public DateTime EndDatetime { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string IdGrade { get; set; }
        public string IdTeacher { get; set; }
        public string Teacher { get; set; }
        public string AttendanceCheckName { get; set; }
        public TimeSpan AttendanceTimeSpan { get; set; }
        public string AttendanceTime { get; set; }
    }
}
