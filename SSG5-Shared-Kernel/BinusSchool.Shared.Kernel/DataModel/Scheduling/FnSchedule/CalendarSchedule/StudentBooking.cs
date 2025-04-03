using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarSchedule
{
    public class StudentBooking
    {
        public string IdBinusian { set; get; }
        public string StudentName { set; get; }
        public string IdInvitationBooking { set; get; }
        public DateTime Date { set; get; }
        public TimeSpan Time { set; get; }
    }
}
