using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Teaching.FnLessonPlan.WeekSetting
{
    public class DateSetting
    {
        public string IdWeekSettingDetail { get; set; }
        public int WeekNumber { get; set; }
        public DateTime? DeadlineDate { get; set; }
        public bool Status { get; set; }
    }
    public class GetWeekSettingResponse : ItemValueVm
    {
        public string AcademicYear { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Term { get; set; }
        public string TotalWeek { get; set; }
        public string Method { get; set; }
        public string Status { get; set; }
        public bool IsDeletable { get; set; }
        public List<DateSetting> DateSettings { get; set; }
    }
}