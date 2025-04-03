using System;
using BinusSchool.Common.Model;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization
{
    public class SaveScheduleRealizationRequest
    {
        public List<DataScheduleRealization> DataScheduleRealizations { get; set; }
    }

    public class DataScheduleRealization
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
    }
}
