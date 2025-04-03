using BinusSchool.Common.Model;


namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class GetStudentRequest : CollectionRequest
    {
        public string IdStudent { get; set; }
        public bool ForApproval { get; set; }
    }
}
