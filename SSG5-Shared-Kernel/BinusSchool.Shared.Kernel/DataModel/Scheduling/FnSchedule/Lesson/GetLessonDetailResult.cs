using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson
{
    public class GetLessonDetailResult : DetailResult2
    {
        public CodeWithIdVm Acadyear { get; set; }
        public int Semester { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public string ClassIdFormat { get; set; }
        public string ClassIdExample { get; set; }
        public string ClassIdGenerated { get; set; }
        public int TotalPerWeek { get; set; }
        public CodeWithIdVm WeekVarian { get; set; }
        public IEnumerable<LessonTeacherDetail> Teachers { get; set; }
        public IEnumerable<LessonHomeroomDetail> Homerooms { get; set; }
    }

    public class LessonTeacherDetail
    {
        public NameValueVm Teacher { get; set; }
        public bool HasAttendance { get; set; }
        public bool HasScore { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsClassDiary { get; set; }
        public bool IsLessonPlan { get; set; }
    }

    public class LessonHomeroomDetail : CodeWithIdVm
    {
        public IEnumerable<CodeWithIdVm> Pathways { get; set; }
    }
}
