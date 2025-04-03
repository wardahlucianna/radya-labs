using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnUser.LoginAs.ImpersonateUser
{
    public class ImpersonateUserLoginAsResult : AuthUserToken
    {
        public string IdUser { get; set; }
        public string Email { get; set; }
        public bool IsUserActiveDirectory { get; set; }
        public IEnumerable<ImpersonateUserLoginAsResult_UserRoleResult> Roles { get; set; }
        public IEnumerable<ImpersonateUserLoginAsResult_SchoolResult> Schools { get; set; }
        public bool IsBlock { get; set; }
        public string BlockingMessage { get; set; }
        public bool IsBlockAndStudentBlockingActive { get; set; }
    }

    public class ImpersonateUserLoginAsResult_UserRoleResult : CodeWithIdVm
    {
        public NameValueWithCodeVm RoleGroup { get; set; }
        public bool IsDefault { get; set; }
        public bool CanOpenTeacherTracking { get; set; }
    }

    public class ImpersonateUserLoginAsResult_SchoolResult : NameValueVm
    {
        public string Logo { get; set; }
    }
}
