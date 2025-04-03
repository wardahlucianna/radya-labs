using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnUser.LoginAs.ImpersonateUser
{
    public class CheckUserNamePasswordImpersonateUserLoginAsResult : AuthUserToken
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public bool IsUserActiveDirectory { get; set; }
        public IEnumerable<GetDetailImpersonateUserResult_UserRoleResult> Roles { get; set; }
        public IEnumerable<GetDetailImpersonateUserResult_SchoolResult> Schools { get; set; }
        public bool IsBlock { get; set; }
        public string BlockingMessage { get; set; }
        public string ImpersonatorIdUser { get; set; }
    }

    public class GetDetailImpersonateUserResult_UserRoleResult : NameValueVm
    {
        public NameValueVm RoleGroup { get; set; }
        public bool CanOpenTeacherTracking { get; set; }
    }

    public class GetDetailImpersonateUserResult_SchoolResult : NameValueVm
    {
        public string Logo { get; set; }
    }
}
