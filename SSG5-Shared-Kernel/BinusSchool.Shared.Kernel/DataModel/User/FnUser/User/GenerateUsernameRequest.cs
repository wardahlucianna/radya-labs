using System.Collections.Generic;

namespace BinusSchool.Data.Model.User.FnUser.User
{
    public class GenerateUsernameRequest
    {
        public string IdRole { get; set; }
        public List<string> BinusianIds { get; set; }
    }
}
