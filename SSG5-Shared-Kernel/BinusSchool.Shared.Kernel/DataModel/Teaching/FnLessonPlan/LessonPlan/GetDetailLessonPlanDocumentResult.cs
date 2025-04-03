using System;

namespace BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan
{
    public class GetDetailLessonPlanDocumentResult
    {
        public string Id { get; set; }
        public string AcademicYear { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Term { get; set; }
        public string IdSubject { get; set; }
        public string Subject { get; set; }
        public string SubjectLevel { get; set; }
        public string Periode { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string PathFile { get; set; }
        public string Filename { get; set; }
        public string Status { get; set; }
        public string RespondedBy { get; set; }
        public DateTime? RespondedDate { get; set; }
        public string Reason { get; set; }
        public int ApprovalState { get; set; }
        public string NextApproval { get; set; }
        public string IdLessonPlanApproval { get; set; }
        public string IdUser { get; set; }
        public bool DisplayNote { get; set; }
    }
}
