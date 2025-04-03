using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class GetStudentMultipleGradeResult : CodeWithIdVm
    {
        public string Name { get; set; }
        public string IdBinusian { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public CodeWithIdVm Homeroom { get; set; }
        public CodeWithIdVm StudentPhoto { get; set; }
    }
}
