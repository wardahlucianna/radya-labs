using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan
{
    public class UpdateLessonPlanSwitchTeacherRequest
    {
        public string IdLesson { get; set; }
        public string IdLessonTeacher { get; set; }
    }
}
