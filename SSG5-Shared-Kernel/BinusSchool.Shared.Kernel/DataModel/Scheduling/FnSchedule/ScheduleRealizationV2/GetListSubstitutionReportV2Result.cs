using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2
{
    public class GetListSubstitutionReportV2Result
    {
        // public IEnumerable<string> Ids { get; set; }
        public DateTime Date { get; set; }
        public string ClassID { get; set; }
        public string SessionID { get; set; }
        public TimeSpan SessionStartTime { get; set; }
        public TimeSpan SessionEndTime { get; set; }
        public List<DataListTeacherV2> DataTeachers {get; set; }
        public List<DataListSubtituteTeacherV2> DataSubtituteTeachers {get; set; }
        public string IdVenue { get; set; }
        public string VenueName { get; set; }
        public string IdVenueOld { get; set; }
        public string VenueNameOld { get; set; }
        public ItemValueVm ChangeVenue { get; set; }
        public string EntryStatusBy { get; set; }
        public DateTime? EntryStatusDate { get; set; }
        public bool IsCancelClass { get; set; }
        public bool IsSendEmail { get; set; }
        public string NotesForSubtitutions { get; set; }
        public string Status { get; set; }
        public string IdHomeroom { get; set; }
    }

    public class DataListTeacherV2 : ItemValueVm
    {

    }

    public class DataListSubtituteTeacherV2 : CodeWithIdVm
    {

    }
}
