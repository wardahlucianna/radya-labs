using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2
{
    public class ENS4NotificationRequest
    {
        public string IdUser { get; set; }
        public List<Ens4NotificationUnsubmited> Unsubmited { get; set; }
    }

    public class Ens4NotificationUnsubmited
    {
        public string IdLesson { get; set; }
        public string Homeroom { get; set; }
        public DateTime Datetime { get; set; }
        public string Date { get; set; }
        public string ClassId { get; set; }
        public string IdTeacher { get; set; }
        public string Teacher { get; set; }
        public int TotalStudent { get; set; }
        public string Session { get; set; }
    }
}
