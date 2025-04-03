using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnUser.LoginAs.ManageAccessRoleLoginAs
{
    public class GetListAccessRoleLoginAsResult : CodeWithIdVm
    {
        public GetListAccessRoleLoginAsResult_AccessRole AccessRole { get; set; }
    }

    public class GetListAccessRoleLoginAsResult_AccessRole
    {
        public string Description { get; set; }
        public int AccessCount { get; set; }
    }
}
