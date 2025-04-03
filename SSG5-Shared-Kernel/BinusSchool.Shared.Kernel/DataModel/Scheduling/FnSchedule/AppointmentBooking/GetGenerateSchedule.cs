using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetGenerateSchedule
    {
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Description { get; set; }
        public bool IsFixBreak { get; set; }
        public bool IsFlexibleBreak { get; set; }
        public int Quota { get; set; }
        public int Duration { get; set; }
        public string IdBreakFlexible { get; set; }
        public string NameBreakFlexible { get; set; }
    }
}
