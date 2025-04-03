using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson
{
    public class GetLessonByTeacherIDResult
    {
        public string AcademicYear { get; set; }
        public int Semester { get; set; }
        public string Grade { get; set; }
        public string Subject { get; set; }
        public string ClassId { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public string VerifiedBy { get; set; }
    }
}
