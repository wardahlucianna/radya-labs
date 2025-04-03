using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.User.FnUser.LoginAs.ImpersonateUser
{
    public class ImpersonateUserLoginAsRequest
    {
        public string IdSchool { get; set; }
        public string IdCurrentUser { get; set; }
        public string ImpresonatingUsername { get; set; }
        public string LoggedInIpAddress { get; set; }
    }
}
