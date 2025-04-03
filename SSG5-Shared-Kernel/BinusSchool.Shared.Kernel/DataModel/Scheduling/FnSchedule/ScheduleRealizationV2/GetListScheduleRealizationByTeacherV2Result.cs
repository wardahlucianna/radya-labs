using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2
{
    public class GetListScheduleRealizationByTeacherV2Result
    {
        public string IdScheduleRealization { get; set; }
        public IEnumerable<string> Ids { get; set; }
        public string ClassID { get; set; }
        public string DaysOfWeek { get; set; }
        public string SessionID { get; set; }
        public TimeSpan SessionStartTime { get; set; }
        public TimeSpan SessionEndTime { get; set; }
        public SubtituteTeacher DataSubtituteTeachers {get; set; }
        public string IdVenue { get; set; }
        public string VenueName { get; set; }
        public string IdVenueOld { get; set; }
        public string VenueNameOld { get; set; }
        public ItemValueVm ChangeVenue { get; set; }
        public string EntryStatusBy { get; set; }
        public DateTime? EntryStatusDate { get; set; }
        public string Status { get; set; }
        public bool CanEnableDisable { get; set; }
        public bool IsCancelClass { get; set; }
        public bool IsSendEmail { get; set; }
        public bool CanPrint { get; set; }
        public bool IsSetScheduleRealization { get; set; }
        public string IdHomeroom { get; set; }
        public string IdLesson { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdDay { get; set; }
        public bool CanModified { get; set; }
    }
}
