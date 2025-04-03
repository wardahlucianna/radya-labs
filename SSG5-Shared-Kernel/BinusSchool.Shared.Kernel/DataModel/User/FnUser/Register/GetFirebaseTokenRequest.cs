using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.User.FnUser.Register
{
    public class GetFirebaseTokenRequest
    {
        public List<string> IdUserRecipient { get; set; }
        public List<AppPlatform> AppPlatform { get; set; }
    }
}
