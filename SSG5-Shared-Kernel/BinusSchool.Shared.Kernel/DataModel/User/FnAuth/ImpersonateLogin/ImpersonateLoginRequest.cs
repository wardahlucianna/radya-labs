using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.User.FnAuth.ImpersonateLogin
{
    public class ImpersonateLoginRequest
    {
        public string ImpersonatorIdUser { get; set; }
        public string ImpersonatedUsername { get; set; }
        public string LoggedInIpAddress { get; set; }
    }
}
