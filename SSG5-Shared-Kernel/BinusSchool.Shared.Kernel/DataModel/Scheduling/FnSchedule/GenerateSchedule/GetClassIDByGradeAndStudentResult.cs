using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule
{
    public class GetClassIDByGradeAndStudentResult
    {
        public CodeWithIdVm Asctimetable { get; set; }

        public DateTime StartPeriod { get; set; }
        public DateTime EndPeriod { get; set; }
        public string ClassId { get; set; }
        public List<StudentVm> Students { get; set; }
    }

    public class WeekVm
    {
        public CodeWithIdVm Week { get; set; }
        public string IdWeek { get; set; }
        public CodeWithIdVm Teacher { get; set; }
        public SessionVm Session { get; set; }
        public CodeWithIdVm Venue { get; set; }
        public CodeWithIdVm Lesson { get; set; }
        public int Semester { get; set; }
    }

    public class WeekNewVm
    {
        public CodeWithIdVm Week { get; set; }
        public string IdWeek { get; set; }
       public List<WeekNewDetailVm> WeekNewDetails { get; set; }
    }

    public class WeekNewDetailVm
    {
        public CodeWithIdVm Teacher { get; set; }
        public SessionVm Session { get; set; }
        public CodeWithIdVm Venue { get; set; }
        public CodeWithIdVm Lesson { get; set; }
        public int Semester { get; set; }
    }

    public class SessionVm
    {
        public string Dayofweek { get; set; }
        public string Id { get; set; }
        public string SessionID { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public class LessonClassIDVM
    {
        public LessonClassIDVM()
        {
            Weeks = new List<WeekVm>();
            Weeks2 = new List<WeekVm>();
            WeeksNew = new List<WeekNewVm>();
        }
        public CodeWithIdVm Subject { get; set; }
        public CodeWithIdVm Homeroom { get; set; }
        public string ClassIdFormat { get; set; }
        public string IdLesson { get; set; }
        public List<WeekVm> Weeks { get; set; }
        public List<WeekVm> Weeks2 { get; set; }
        public List<WeekNewVm> WeeksNew { get; set; }
    }

    public class StudentVm
    {
        public StudentVm()
        {
            ClassIds = new List<LessonClassIDVM>();
        }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public GradeWithPeriodVm Grade { get; set; }
        public List<LessonClassIDVM> ClassIds { get; set; }
    }

    public class GradeWithPeriodVm : CodeWithIdVm
    {
        public List<PeriodVm> Periods { get; set; }
    }

    public class PeriodVm
    {
        public DateTime AttendanceStartDate { get; set; }
        public DateTime AttendanceEndDate { get; set; }
        public int Semester { get; set; }
    }

}
