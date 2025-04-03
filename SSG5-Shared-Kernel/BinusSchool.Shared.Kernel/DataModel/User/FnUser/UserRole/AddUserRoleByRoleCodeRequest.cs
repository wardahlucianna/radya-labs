using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.User.FnUser.UserRole
{
    public class AddUserRoleByRoleCodeRequest
    {
        public string IdUser { set; get; }
        public string RoleCode { set; get; }
        public string IdSchool { set; get; }

    }
}
