using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class GetListParentStudentResult : CodeWithIdVm
    {
        public string ParentId { get; set; }
        public string ParentName { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public ItemValueVm ClassHomeroom { get; set; }
    }
}
