using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.Parent;

namespace BinusSchool.Data.Model.Student.FnStudent.ParentRole
{
    public class GetParentRoleResult : CodeWithIdVm
    {
        public string ParentRoleName { get; set; }
        public string ParentRoleNameEng { get; set; }
        public GetParentResult FamilyInformation { get; set; }
    }
}