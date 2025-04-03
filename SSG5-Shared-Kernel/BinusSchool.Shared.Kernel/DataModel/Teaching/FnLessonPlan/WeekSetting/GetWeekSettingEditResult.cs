using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Teaching.FnLessonPlan.WeekSetting
{
    public class GetWeekSettingEditResult
    {
        public CodeWithIdVm AcademicYear { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public CodeWithIdVm Term { get; set; }

        public int TotalWeek { get; set; }
        public string Method { get; set; }
    }
}