using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnAuth.UserPassword;

namespace BinusSchool.Data.Model.User.FnAuth.ImpersonateLogin
{
    public class ImpersonateLoginResult : AuthUser
    {
        public string IdUser { get; set; }
        public string Email { get; set; }
        public bool IsUserActiveDirectory { get; set; }
        public IEnumerable<ImpersonateLoginResult_UserRoleResult> Roles { get; set; }
        public IEnumerable<ImpersonateLoginResult_SchoolResult> Schools { get; set; }
    }

    public class ImpersonateLoginResult_UserRoleResult : NameValueVm
    {
        public NameValueWithCodeVm RoleGroup { get; set; }
        public bool CanOpenTeacherTracking { get; set; }
    }

    public class ImpersonateLoginResult_SchoolResult : NameValueVm
    {
        public string Logo { get; set; }
    }
}
