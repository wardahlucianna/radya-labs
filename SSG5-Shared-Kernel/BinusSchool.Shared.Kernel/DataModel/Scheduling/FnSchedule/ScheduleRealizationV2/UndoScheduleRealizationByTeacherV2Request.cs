using System;
using BinusSchool.Common.Model;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2
{
    public class UndoScheduleRealizationByTeacherV2Request
    {
        public List<UndoScheduleRealizationByTeacherV2> DataScheduleRealizations { get; set; }
    }

    public class UndoScheduleRealizationByTeacherV2
    {
        public List<string> Ids { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SessionID { get; set; }
        public string ClassID { get; set; }
        public string DaysOfWeek { get; set; }
        public string IdUserTeacher { get; set; }
        public string IdLesson { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdDay { get; set; }
    }
}
