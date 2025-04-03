using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministrationV2
{
    public class ATD22NotificationModel
    {
       public string IdRecepient { get; set; }
       public string IdLevel { get; set; }
        public List<NotifATD22> ListCancel { get; set; }
    }

    public class NotifATD22
    {
        public string AcademicYear { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
        public string StudentName { get; set; }
        public string AttendanceCategory { get; set; }
        public string DetailStatus { get; set; }
        public string SubmittedDate { get; set; }
        public string CancelAttendance { get; set; }
    }
}
