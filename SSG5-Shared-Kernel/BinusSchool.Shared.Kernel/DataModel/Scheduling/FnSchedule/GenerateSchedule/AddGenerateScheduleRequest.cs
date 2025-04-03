using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule
{
    public class AddGenerateScheduleRequest
    {
        public string IdAcadicYears { get; set; }
        public string IdAsctimetable { get; set; }
        public List<GenerateScheduleGradeVM> Grades { get; set; }
        public string Type { get; set; }
    }

    public class GenerateScheduleLessonVM
    {
        public string WeekId { get; set; }
        public DateTime StartPeriode { get; set; }
        public DateTime EndPeriode { get; set; }
        public DateTime ScheduleDate { get; set; }
        public string ClassId { get; set; }
        public string LessonId { get; set; }
        public string VenueId { get; set; }
        public string VenueName { get; set; }
        public string TeacherName { get; set; }
        public string TeacherId { get; set; }
        public string IdSubject { get; set; }
        public string SubjectName { get; set; }
        public string SessionID { get; set; }
        public string IdSession { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string DaysofWeek { get; set; }
        public string IdHomeroom { get; set; }
        public string HomeroomName { get; set; }
    }

    public class GenerateScheduleStudentVM
    {
        public string StudentId { get; set; }
        public List<GenerateScheduleLessonVM> Lessons { get; set; }
    }

    public class GenerateScheduleGradeVM
    {
        public string GradeId { get; set; }
        public DateTime StartPeriod { get; set; }
        public DateTime EndPeriod { get; set; }
        public List<GenerateScheduleStudentVM> Students { get; set; }
    }

}
