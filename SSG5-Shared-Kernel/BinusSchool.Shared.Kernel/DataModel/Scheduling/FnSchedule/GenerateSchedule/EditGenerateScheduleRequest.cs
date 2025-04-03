using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule
{
    public class EditGenerateScheduleRequest
    {

    }

    public class EditGenerateScheduleLessonVM : GenerateScheduleLessonVM
    {
        public string IdGenerateSchedule { get; set; }
    }

    public class EditGenerateScheduleStudentVM
    {
        public string StudentId { get; set; }
        public List<EditGenerateScheduleLessonVM> Lessons { get; set; }
    }

    public class EditGenerateScheduleGradeVM
    {
        public string GradeId { get; set; }
        public DateTime StartPeriod { get; set; }
        public DateTime EndPeriod { get; set; }
        public List<GenerateScheduleStudentVM> Students { get; set; }
    }
}
