using System;
using BinusSchool.Common.Model;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2
{
    public class SaveScheduleRealizationV2Request
    {
        public List<DataScheduleRealizationV2> DataScheduleRealizations { get; set; }
    }

    public class DataScheduleRealizationV2
    {
        public List<string> Ids { get; set; }
        public DateTime Date { get; set; }
        public string SessionID { get; set; }
        public string ClassID { get; set; }
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
