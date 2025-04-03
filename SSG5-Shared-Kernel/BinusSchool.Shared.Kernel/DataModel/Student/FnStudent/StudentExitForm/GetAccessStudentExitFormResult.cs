using System.Collections.Generic;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentExitForm
{
    public class GetAccessStudentExitFormResult
    {
        public GetAccessStudentExitFormResult()
        {
            Childrens = new List<GetListStudentByParentModel>();
        }
        public bool IsAllowed { get; set; }
        public List<GetListStudentByParentModel> Childrens { get; set; }
    }

    public class GetListStudentByParentModel
    {
        public string IdHomeroomStudent { get; set; }
        public string IdStudent { get; set; }
        public string Name { get; set; }
    }
}
