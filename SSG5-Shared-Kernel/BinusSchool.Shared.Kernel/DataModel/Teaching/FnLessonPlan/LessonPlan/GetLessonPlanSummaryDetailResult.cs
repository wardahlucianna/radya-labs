using System;
using System.Collections.Generic;

namespace BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan
{
    public class LessonPlanSummaryDetailUploaded
    {
        public string IdLessonPlan { get; set; }
        public string IdUser { get; set; }
        public string TeacherName { get; set; }
        public DateTime? UploadDate { get; set; }
        public DateTime? DeadlineDate { get; set; }
        public string Status { get; set; }
        public string IdClass { get; set; }
        public string IdLesson { get; set; }
    }
    public class LessonPlanSummaryDetailNotUploaded
    {
        public string IdLessonPlan { get; set; }
        public string IdUser { get; set; }
        public string TeacherName { get; set; }
        public DateTime? DeadlineDate { get; set; }
        public string Status { get; set; }
        public string IdClass { get; set; }
        public string IdLesson { get; set; }
    }
    public class GetLessonPlanSummaryDetailResult
    {
        public List<LessonPlanSummaryDetailUploaded> Uploaded { get; set; }
        public List<LessonPlanSummaryDetailNotUploaded> NotUploaded { get; set; }
    }
}
