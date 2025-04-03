using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.User.FnUser.Role
{
    public class AddRoleRequest : CodeVm
    {

        public string IdSchool { get; set; }
        public string IdRoleGroup { get; set; }
        public bool IsArrangeUsernameFormat { get; set; }
        public string UsernameFormat { get; set; }
        public string UsernameExample { get; set; }
        public List<PositionRequest> Positions { get; set; }
        public List<string> WebPermissionIds { get; set; }
        public List<MobilePermissionRequest> MobilePermissionIds { get; set; }
    }

    public class PositionRequest
    {
        public string IdTeacherPosition { get; set; }
        public List<string> WebPermissionIds { get; set; }
        public List<MobilePermissionRequest> MobilePermissionIds { get; set; }
    }

    public class MobilePermissionRequest
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public int FeatureOrderNumber { get; set; }
    }
}
