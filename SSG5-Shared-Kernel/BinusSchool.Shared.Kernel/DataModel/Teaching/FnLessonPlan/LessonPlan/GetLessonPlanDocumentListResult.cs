using System;
using System.Collections.Generic;
namespace BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan
{
    public class LessonPlanDocument
    {
        public string IdLessonPlanDocument { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string Status { get; set; }
        public string UserCreate { get; set; }
    }
    public class GetLessonPlanDocumentListResult
    {
        public string IdLessonPlan { get; set; }
        public string IdUser { get; set; }
        public string TeacherName { get; set; }
        public string IdAcademicYear { get; set; }
        public string AcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string Level { get; set; }
        public string IdGrade { get; set; }
        public string Grade { get; set; }
        public int Semester { get; set; }
        public int WeekNumber { get; set; }
        public string IdPeriod { get; set; }
        public string Term { get; set; }
        public string Subject { get; set; }
        public string SubjectLevel { get; set; }
        public string Periode { get; set; }
        public DateTime? DeadlineDate { get; set; }
        public string Status { get; set; }
        public List<LessonPlanDocument> LessonPlanDocuments { get; set; }
        public bool CanUpload { get; set; }
        // public string IdClass { get; set; }
    }
}
