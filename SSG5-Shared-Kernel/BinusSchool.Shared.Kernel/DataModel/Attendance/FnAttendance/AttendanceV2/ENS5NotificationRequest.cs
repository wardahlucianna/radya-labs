using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2
{
    public class ENS5NotificationRequest
    {
        public string IdUser { get; set; }
        public List<Ens5NotificationUnsubmited> Unsubmited { get; set; }
    }

    public class Ens5NotificationUnsubmited
    {
        public string IdHomeroom { get; set; }
        public string Homeroom { get; set; }
        public DateTime DateTime { get; set; }
        public string Date { get; set; }
        public string IdTeacher { get; set; }
        public string Teacher { get; set; }
        public int TotalStudent { get; set; }
    }
}
