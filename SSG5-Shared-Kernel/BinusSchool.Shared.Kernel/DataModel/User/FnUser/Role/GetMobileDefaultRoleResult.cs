using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnUser.Role
{
    public class GetMobileDefaultRoleResult : CodeWithIdVm
    {
        public string IdParent { get; set; }
        public string Type { get; set; }
        public List<CodeWithIdVm> Permissions { get; set; }
        public List<GetMobileDefaultRoleResult> Childs { get; set; }
    }
}
