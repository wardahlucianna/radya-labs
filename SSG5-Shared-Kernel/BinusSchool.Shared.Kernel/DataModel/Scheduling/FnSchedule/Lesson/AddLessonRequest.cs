using System.Collections.Generic;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson
{
    public class AddLessonRequest
    {
        public string IdAcadyear { get; set; }
        public int Semester { get; set; }
        public string SemesterValue { get; set; }
        public string IdGrade { get; set; }
        public string IdSubject { get; set; }
        public string ClassIdFormat { get; set; }
        public string ClassIdExample { get; set; }
        public IEnumerable<LessonTeacherAndHomeroom> Lessons { get; set; }
    }

    public class LessonTeacherAndHomeroom
    {
        public string ClassIdGenerated { get; set; }
        public int TotalPerWeek { get; set; }
        public string IdWeekVarian { get; set; }
        public IEnumerable<LessonTeacher> Teachers { get; set; }
        public IEnumerable<LessonHomeroom> Homerooms { get; set; }
    }

    public class LessonTeacher
    {
        public string IdTeacher { get; set; }
        public bool HasAttendance { get; set; }
        public bool HasScore { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsClassDiary { get; set; }
        public bool IsLessonPlan { get; set; }
    }

    public class LessonHomeroom
    {
        public string IdHomeroom { get; set; }
        public string Homeroom { get; set; }
        public IEnumerable<string> IdPathways { get; set; }
    }
}
