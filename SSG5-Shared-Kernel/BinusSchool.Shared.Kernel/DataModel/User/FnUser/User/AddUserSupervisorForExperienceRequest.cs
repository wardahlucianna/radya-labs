using System.Collections.Generic;

namespace BinusSchool.Data.Model.User.FnUser.User
{
    public class AddUserSupervisorForExperienceRequest
    {
        public string IdUser { get; set; }
        public string IdUserRole { get; set; }
        public string Password { get; set; }
        public string IdSchool { get; set; }
        public bool IsActiveDirectory { get; set; }
        // public string DisplayName { get; set; }
        public string Email { get; set; }
        public string IdRole { get; set; }
    }
}
