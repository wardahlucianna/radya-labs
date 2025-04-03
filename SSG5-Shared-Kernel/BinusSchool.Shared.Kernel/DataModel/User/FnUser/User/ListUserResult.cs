using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnUser.User
{
    public class ListUserResult : CodeWithIdVm
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public bool IsActiveDirectory { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
    }
}
