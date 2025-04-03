using System;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan
{
    public class GetLessonPlanApprovalQueryResult
    {
        public string IdLessonPlanApproval { get; set; }
        public string IdLessonPlan { get; set; }
        public string IdPeriod { get; set; }
        public string IdSubject { get; set; }
        public string IdSubjectLevel { get; set; }
        public string IdWeekSettingDetail { get; set; }
        public CodeWithIdVm AcademicYear { get; set; }
        public CodeWithIdVm Level { get; set; } 
        public ItemValueVm Grade { get; set; }
        public ItemValueVm Period { get; set; }
        public ItemValueVm Subject { get; set; }
        public string SubjectLevel { get; set; }
        public string TeacherId { get; set; }
        public string TeacherName { get; set; }
        public string Periode { get; set; }
        public DateTime? DeadlineDate { get; set; }
    }
    
    public class GetLessonPlanApprovalResult
    {
        public string AcademicYear { get; set; }
        public string Level { get; set; } 
        public string Grade { get; set; }
        public string Period { get; set; }
        public string Subject { get; set; }
        public string SubjectLevel { get; set; }
        public string TeacherName { get; set; }
        public string Periode { get; set; }
        public DateTime? DeadlineDate { get; set; }
        public string IdLessonPlanApproval { get; set; }
        public string IdPeriod { get; set; }
        public string IdSubject { get; set; }
        public string IdSubjectLevel { get; set; }
        public string IdWeekSettingDetail { get; set; }
        public string IdLessonPlan { get; set; }
        public string LatestDocumentId { get; set; }
    }
}