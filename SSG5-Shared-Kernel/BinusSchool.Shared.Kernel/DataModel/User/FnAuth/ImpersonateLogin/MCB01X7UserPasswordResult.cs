using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnAuth.UserPassword;

namespace BinusSchool.Data.Model.User.FnAuth.ImpersonateLogin
{
    public class MCB01X7UserPasswordResult : AuthUserToken
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public bool IsUserActiveDirectory { get; set; }
        public IEnumerable<UserRoleResult> Roles { get; set; }
        public IEnumerable<SchoolResult> Schools { get; set; }
        public bool IsBlock { get; set; }
        public string BlockingMessage { get; set; }
        public string ImpersonatorIdUser { get; set; }
    }
    public class UserRoleResult : NameValueVm
    {
        public NameValueVm RoleGroup { get; set; }
        public bool CanOpenTeacherTracking { get; set; }
    }
}
