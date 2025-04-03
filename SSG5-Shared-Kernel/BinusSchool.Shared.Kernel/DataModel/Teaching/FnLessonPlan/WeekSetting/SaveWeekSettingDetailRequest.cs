using System;
using System.Collections.Generic;

namespace BinusSchool.Data.Model.Teaching.FnLessonPlan.WeekSetting
{
    public class SaveWeekSettingDetailRequest
    {
        public string IdWeekSetting { get; set; }
        public List<WeekSettingDetail> WeekSettingDetails { get; set; }
    }

    public class WeekSettingDetail 
    {
        public string IdWeekSettingDetail { get; set; }
        public DateTime? DeadlineDate { get; set; }
        public int WeekNumber { get; set; }
        public bool Status { get; set; }
    }
}