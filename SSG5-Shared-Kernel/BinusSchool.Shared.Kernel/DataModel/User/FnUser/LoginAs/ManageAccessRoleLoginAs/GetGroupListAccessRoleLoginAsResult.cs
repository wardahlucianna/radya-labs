using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnUser.LoginAs.ManageAccessRoleLoginAs
{
    public class GetGroupListAccessRoleLoginAsResult
    {
        public CodeWithIdVm RoleGroup { get; set; }
        public List<GetGroupListAccessRoleLoginAsResult_Role> RoleList { get; set; }
        public int CountRoleList { get; set; }
    }

    public class GetGroupListAccessRoleLoginAsResult_Role : CodeWithIdVm
    {
        public bool IsRoleChecked { get; set; }
    }
}
