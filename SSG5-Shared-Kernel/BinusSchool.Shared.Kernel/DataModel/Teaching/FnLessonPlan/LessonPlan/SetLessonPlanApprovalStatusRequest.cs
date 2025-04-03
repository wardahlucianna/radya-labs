namespace BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan
{
    public class SetLessonPlanApprovalStatusRequest
    {
        public string IdUser { get; set; }
        public string IdLessonPlanApproval { get; set; }
        public string IdLessonPlanDocument { get; set; }
        public bool IsApproved { get; set; }
        public string Reason { get; set; }
    }
}
