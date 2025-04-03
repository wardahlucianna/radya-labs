using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnUser.User
{
    public class GetUserBySchoolAndRoleRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public string IdRole { get; set; }
    }
}
