using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnUser.User
{
    public class GetUserDetailResult
    {
        public string Id { get; set; }
        public bool IsActiveDirectory { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string Username { get; set; }
        public IEnumerable<UserRoleResult> Roles { get; set; }
    }
    public class UserRoleResult : CodeWithIdVm
    {
        public string Username { get; set; }
        public bool IsDefaultUsername { get; set; }
        public string UsernameFormat { get; set; }
        public CodeWithIdVm RoleGroup {get;set;}
    }
}
