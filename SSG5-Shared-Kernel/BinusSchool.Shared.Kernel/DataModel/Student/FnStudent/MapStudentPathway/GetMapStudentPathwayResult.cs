using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.MapStudentPathway
{
    public class GetMapStudentPathwayResult : CodeWithIdVm
    {
        public Gender Gender { get; set; }
        //public Religion Religion { get; set; }
        //public string Gender { get; set; }
        public string IdStudentGrade { get; set; }
        public string Religion { get; set; }
        public bool IsActive { get; set; }
        public ItemValueVm Pathway { get; set; }
        public ItemValueVm PathwayNextAcademicYear { get; set; }
        public ItemValueVm LastPathway { get; set; }
        public string LastHomeroom { get; set; }
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
    }
}
