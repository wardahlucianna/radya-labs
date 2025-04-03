using System;
using BinusSchool.Common.Model;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2
{
    public class SaveScheduleRealizationByTeacherV2Request
    {
        public List<DataScheduleRealizationByTeacherV2> DataScheduleRealizations { get; set; }
    }

    public class DataScheduleRealizationByTeacherV2
    {
        public List<string> Ids { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SessionID { get; set; }
        public string ClassID { get; set; }
        public string DaysOfWeek { get; set; }
        public string IdUserTeacher { get; set; }
        public string IdUserSubtituteTeacher { get; set; }
        public string IdRegularVenue { get; set; }
        public string IdChangeVenue { get; set; }
        public bool IsCancel { get; set; }
        public bool IsSendEmail { get; set; }
        public string NotesForSubtitutions { get; set; }
        public bool IsSubtituteChange { get; set; }
        public bool IsVenueChange { get; set; }
        public string IdLesson { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdDay { get; set; }
    }
}
