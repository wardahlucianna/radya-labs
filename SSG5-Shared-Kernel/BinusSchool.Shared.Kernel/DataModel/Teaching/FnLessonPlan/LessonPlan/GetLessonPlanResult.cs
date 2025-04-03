using System;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan
{
    public class GetLessonPlanQueryResult
    {
        public string IdLessonPlan { get; set; }
        public string IdPeriod { get; set; }
        public string IdSubject { get; set; }
        public string IdSubjectLevel { get; set; }
        public string IdWeekSettingDetail { get; set; }
        public CodeWithIdVm AcademicYear { get; set; }
        public CodeWithIdVm Level { get; set; } 
        public ItemValueVm Grade { get; set; }
        public CodeWithIdVm Period { get; set; }
        public ItemValueVm Subject { get; set; }
        public string SubjectLevel { get; set; }
        public string Periode { get; set; }
        public DateTime? DeadlineDate { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string Status { get; set; }
        public bool CanUpload { get; set; }
    }
    
    public class GetLessonPlanResult
    {
        public string IdLessonPlan { get; set; }
        public string AcademicYear { get; set; }
        public string Level { get; set; } 
        public string Grade { get; set; }
        public string Term { get; set; }
        public string Subject { get; set; }
        public string SubjectLevel { get; set; }
        public string Periode { get; set; }
        public DateTime? DeadlineDate { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string Status { get; set; }
        public string IdPeriod { get; set; }
        public string IdSubject { get; set; }
        public string IdSubjectLevel { get; set; }
        public string IdWeekSettingDetail { get; set; }
        public string IdLesson { get; set; }
        public bool CanUpload { get; set; }
    }

    public class GetLessonPlanNotificationResult
    {
        public string IdUser { get; set; }
        public string IdLessonPlan { get; set; }
        public string IdLessonPlanApproval { get; set; }
        public string IdLessonPlanDocument { get; set; }
        public string IdLessonTeacher { get; set; }
        public string AcademicYear { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Term { get; set; }
        public string Subject { get; set; }
        public string SubjectLevel { get; set; }
        public string Periode { get; set; }
        public string DeadlineDate { get; set; }
        public string RequestDate { get; set; }
        public string SchoolName { get; set; }
        public string TeacherName { get; set; }
        public string ApprovalName { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public string IdPeriod { get; set; }
        public string IdSubject { get; set; }
        public string IdSubjectLevel { get; set; }
        public string IdWeekSettingDetail { get; set; }
        public string IdTeacher { get; set; }
        public bool CanUpload { get; set; }
        public string Link { get; set; }
    }

}
