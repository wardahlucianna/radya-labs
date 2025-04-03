using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2
{
    public class ENS2NotificationRequest
    {
        public string IdUser { get; set; }
        public List<Ens4NotificationUnsubmited> Pending { get; set; }
    }
}
