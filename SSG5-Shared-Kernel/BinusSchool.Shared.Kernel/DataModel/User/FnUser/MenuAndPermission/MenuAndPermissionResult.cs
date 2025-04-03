using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnUser.MenuAndPermission
{
    public class MenuAndPermissionResult : CodeWithIdVm
    {
        public string IdParent { get; set; }
        public List<CodeWithIdVm> Permissions { get; set; }
        public List<MenuAndPermissionResult> Childs { get; set; }
    }
}
