
using System.Collections.Generic;

namespace BinusSchool.Data.Model.User.FnUser.Role
{
    public class GetRoleRequest : CollectionSchoolRequest
    {
        public bool? HasPosition { get; set; }
        public IEnumerable<string> IdRoleGroups { get; set; }

    }
}
