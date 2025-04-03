using System.Collections.Generic;

namespace BinusSchool.Data.Model.User.FnUser.User
{
    public class AddUserRequest
    {
        public string IdSchool { get; set; }
        public bool IsActiveDirectory { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public List<UserRoleRequest> Roles { get; set; }
    }

    public class UserRoleRequest
    {
        public string IdRole { get; set; }
        public string Username { get; set; }
        public bool IsDefaultUsername { get; set; }
    }
}
