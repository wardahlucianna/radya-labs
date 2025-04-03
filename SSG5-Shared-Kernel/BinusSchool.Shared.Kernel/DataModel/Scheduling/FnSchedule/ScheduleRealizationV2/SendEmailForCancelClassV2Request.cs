using System;
using BinusSchool.Common.Model;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2
{
    public class SendEmailForCancelClassV2Request
    {
        public List<SendEmailDataScheduleRealizationV2> DataScheduleRealizations { get; set; }
    }

    public class SendEmailDataScheduleRealizationV2
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
        public string IdLesson { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
    }
}
