using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.User.FnAuth.LoginTransaction
{
    public class AddLoginTransactionRequest
    {
        public string IdUser { get; set; }
        public string IpAddress { get; set; }
        public bool? SignInWithActiveDirectory { get; set; }
        public string Action { get; set; }      // "login" or "logout"
    }
}
