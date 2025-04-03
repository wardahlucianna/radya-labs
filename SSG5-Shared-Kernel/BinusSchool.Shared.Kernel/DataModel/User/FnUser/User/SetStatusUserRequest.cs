using System.Collections.Generic;

namespace BinusSchool.Data.Model.User.FnUser.User
{
    public class SetStatusUserRequest
    {
        public IEnumerable<string> Ids { get; set; }
        public bool IsActive { get; set; }
    }
}
