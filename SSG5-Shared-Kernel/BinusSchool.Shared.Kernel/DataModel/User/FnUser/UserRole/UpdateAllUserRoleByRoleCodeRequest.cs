using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.User.FnUser.UserRole
{
    public class UpdateAllUserRoleByRoleCodeRequest
    {
        public string RoleCode { set; get; }
        public string IdSchool { set; get; }
        public List<AllUserRoleUpdateVm> UserList { set; get; }
    }
    public class AllUserRoleUpdateVm 
    {
        public string IdUser { set; get; }      

    }

}
