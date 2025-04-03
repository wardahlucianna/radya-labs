using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnAuth.UserPassword
{
    public class UserPasswordResult : AuthUserToken
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public bool IsUserActiveDirectory { get; set; }
        public IEnumerable<UserRoleResult> Roles { get; set; }
        public IEnumerable<SchoolResult> Schools { get; set; }
        public bool IsBlock { get; set; }
        public string BlockingMessage { get; set; }
    }

    public class UserRoleResult : NameValueVm
    {
        public NameValueWithCodeVm RoleGroup { get; set; }
        public bool CanOpenTeacherTracking { get; set; }
    }
}
