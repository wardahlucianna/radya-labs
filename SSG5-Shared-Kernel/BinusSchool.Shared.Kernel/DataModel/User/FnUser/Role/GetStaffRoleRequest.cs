using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.User.FnUser.Role
{
    public class GetStaffRoleRequest : CollectionSchoolRequest
    {
        public bool? HasPosition { get; set; }
    }
}
