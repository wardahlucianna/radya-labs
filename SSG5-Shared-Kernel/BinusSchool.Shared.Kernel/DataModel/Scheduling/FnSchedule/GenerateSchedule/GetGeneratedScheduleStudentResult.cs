using System.Collections.Generic;
using System.Linq;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule
{
    public class GetGeneratedScheduleStudentResult : ItemValueVm
    {
        public NameValueVm Student { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public CodeWithIdVm Homeroom { get; set; }
        public int TotalLesson => ClassIds?.Distinct()?.Count() ?? 0;
        public IEnumerable<LessonGeneratedScheduleVm> ClassIds { get; set; }
    }

    public class LessonGeneratedScheduleVm
    {
        public string ClassId { get; set; }
        public string  TeacherName { get; set; }
    }
}
