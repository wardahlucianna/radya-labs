using System.Collections.Generic;

namespace BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan
{
    public class AddLessonPlanApproverSettingRequest
    {
        public string IdSchool { get; set; }
        public List<ListLessonPlanApproverSetting> ListLessonPlanApproverSettings { get; set; }
    }

    public class ListLessonPlanApproverSetting
    {
        public string Id { get; set; }
        public string IdRole { get; set; }
        public string IdTeacherPosition { get; set; }
        public string IdStaff { get; set; }
    }
}
