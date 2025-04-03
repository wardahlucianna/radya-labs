using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class GetStudentByGradeResult : ItemValueVm
    {
        public string Grade { get; set; }
        public string StudentId { get; set; }
        public string FullName { get; set; }
        public Gender Gender { get; set; }
        public bool IsActive { get; set; }
        public CodeWithIdVm LastPathway { get; set; }
        public CodeWithIdVm CurrentPathway { get; set; }
    }
}