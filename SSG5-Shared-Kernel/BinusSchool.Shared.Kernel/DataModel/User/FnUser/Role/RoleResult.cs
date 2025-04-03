using System;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnUser.Role
{
    public class RoleResult : CodeWithIdVm
    {
        public CodeWithIdVm RoleGroup { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool CanDeleted { get; set; }
    }
}
