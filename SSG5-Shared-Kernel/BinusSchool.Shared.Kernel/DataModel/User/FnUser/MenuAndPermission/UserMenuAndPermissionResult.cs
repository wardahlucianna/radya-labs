using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnUser.MenuAndPermission
{
    public class UserMenuAndPermissionResult : CodeWithIdVm
    {
        public string IdParent { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Icon { get; set; }
        public string ParamUrl { get; set; }
        public List<CodeWithIdVm> Permissions { get; set; }
        public List<UserMenuAndPermissionResult> Childs { get; set; }
        public int OrderNumber { get; set; }
        public string Type { get; set; }
    }
}
