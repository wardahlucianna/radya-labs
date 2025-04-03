using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnUser.Role
{
    public class GetRoleMobileResult : CodeWithIdVm
    {
        public List<MobileRolePositionResult> RolePositions { get; set; }
        public List<MobilePermissionResult> PermissionIds { get; set; }
    }

    public class MobileRolePositionResult
    {
        public string Id { get; set; }
        public CodeWithIdVm TeacherPosition { get; set; }
        public List<MobilePermissionResult> PermissionIds { get; set; }
    }

    public class MobilePermissionResult
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string TypeVal { get; set; }
        public string Description { get; set; }
        public int FeatureOrderNumber { get; set; }
        public List<MobilePermissionResult> Childs { get; set; }
    }
}
