using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnUser.Role;

namespace BinusSchool.Data.Model.User.FnUser.MenuAndPermission
{
    public class FeatureUserMenuAndPermissionResult : CodeWithIdVm
    {
        public string Action { get; set; }
        public string Controller { get; set; }
        public string Icon { get; set; }
        public string Submenus { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
    }
}
