using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentExitForm
{
    public class GetParentByChildResult
    {
        public string IdHomeroomStudent { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public List<FamilyOfStudent> FamilyOfStudents { get; set; }
    }

    public class FamilyOfStudent
    {
        public string Iduser { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string ParentRole { get; set; }
    }
}
