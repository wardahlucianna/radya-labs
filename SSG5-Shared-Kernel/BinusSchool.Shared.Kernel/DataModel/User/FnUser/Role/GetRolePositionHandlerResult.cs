using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnUser.Role
{
    public class GetRolePositionHandlerResult : CodeWithIdVm
    {
        public string IdPosition { get; set; }
        public string IdTeacherPosition { get; set; }
    }
}
