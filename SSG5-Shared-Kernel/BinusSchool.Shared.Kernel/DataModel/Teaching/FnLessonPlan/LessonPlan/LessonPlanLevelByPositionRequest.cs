using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan
{
    public class LessonPlanLevelByPositionRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdUser { get; set; }
        public string SelectedPosition { get; set; }
    }
}
