using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class GetStudentCopyByGradeResult : ItemValueVm
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Gender Gender { get; set; }
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
    }
}
