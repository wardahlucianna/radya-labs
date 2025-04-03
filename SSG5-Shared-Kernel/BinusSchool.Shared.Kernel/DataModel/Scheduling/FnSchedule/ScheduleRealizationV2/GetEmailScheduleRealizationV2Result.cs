using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2
{
    public class GetEmailScheduleRealizationV2Result
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
        public List<string> IdUserCc { get; set; }
        public List<string> IdUsers { get; set; }
        public string IdRegularVenue { get; set; }
        public string RegularVenueName { get; set; }
        public string IdChangeVenue { get; set; }
        public string ChangeVenueName { get; set; }
        public string NotesForSubtitutions { get; set; }
        public bool IsCancel { get; set; }
        public bool IsSendEmail { get; set; }
        public DateTime? DateIn { get; set; }
    }
}
