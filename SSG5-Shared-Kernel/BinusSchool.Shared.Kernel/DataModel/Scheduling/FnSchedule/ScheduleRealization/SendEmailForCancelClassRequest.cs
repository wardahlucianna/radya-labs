using System;
using BinusSchool.Common.Model;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization
{
    public class SendEmailForCancelClassRequest
    {
        public List<SendEmailDataScheduleRealization> DataScheduleRealizations { get; set; }
    }

    public class SendEmailDataScheduleRealization
    {
        public List<string> Ids { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string DaysOfWeek { get; set; }
        public string SessionID { get; set; }
        public TimeSpan SessionStartTime { get; set; }
        public TimeSpan SessionEndTime { get; set; }
        public string ClassID { get; set; }
        public List<string> IdUserTeacher { get; set; }
        public List<string> IdUserSubtituteTeacher { get; set; }
        public string IdRegularVenue { get; set; }
        public string IdChangeVenue { get; set; }
        public bool IsCancel { get; set; }
        public bool IsSendEmail { get; set; }
        public bool IsByDate { get; set; }
    }
}
