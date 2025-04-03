using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnUser.Role
{
    public class GetStaffRoleResult : CodeWithIdVm
    {
        public CodeWithIdVm RoleGroup { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool CanDeleted { get; set; }
    }
}
