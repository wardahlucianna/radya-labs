using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.MapStudentGrade
{
    public class GetMapStudentGradeResult : CodeWithIdVm
    {
        public ItemValueVm AcademicYear { get; set; }
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
    }
}
