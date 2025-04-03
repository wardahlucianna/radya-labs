using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnUser.Role
{
    public class RoleDetailResult : CodeWithIdVm
    {
        public CodeWithIdVm RoleGroup { get; set; }
        public bool IsArrangeUsernameFormat { get; set; }
        public string UsernameFormat { get; set; }
        public string UsernameExample { get; set; }
        public List<RolePositionResult> RolePositions { get; set; }
        public List<string> PermissionIds { get; set; }
    }

    public class RolePositionResult
    {
        public string Id { get; set; }
        public CodeWithIdVm TeacherPosition { get; set; }
        public List<string> PermissionIds { get; set; }
    }
}
