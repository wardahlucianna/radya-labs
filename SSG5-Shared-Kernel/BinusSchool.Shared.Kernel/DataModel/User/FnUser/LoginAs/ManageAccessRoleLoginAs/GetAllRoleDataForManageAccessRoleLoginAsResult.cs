using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.User.FnUser.LoginAs.ManageAccessRoleLoginAs
{
    public class GetAllRoleDataForManageAccessRoleLoginAsResult
    {
        public string IdSchool { get; set; }
        public string IdRoleGroup { get; set; }
        public string RoleGroupCode { get; set; }
        public string RoleGroupDesc { get; set; }
        public string IdRole { get; set; }
        public string RoleCode { get; set; }
        public string RoleDesc { get; set; }
        public string IdAuthorizedRole { get; set; }
        public string AuthorizedRoleCode { get; set; }
        public string AuthorizedRoleDesc { get; set; }
        public string AuthorizedRoleGroupCode { get; set; }
        public string AuthorizedRoleGroupDesc { get; set; }
    }
}
