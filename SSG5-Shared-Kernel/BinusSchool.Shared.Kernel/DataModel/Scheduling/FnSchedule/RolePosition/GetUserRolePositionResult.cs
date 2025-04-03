using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition
{
    public class GetUserRolePositionResult
    {
        public string IdUserRolePositions { get; set; }
        public string IdUser { get; set; }
        public string IdUserChild { get; set; }
        public string IdHomeroomStudent { get; set; }
        public ItemValueVmWithOrderNumber Level { get; set; }
        public ItemValueVmWithOrderNumber Grade { get; set; }
        public UserRolePositionHomeroom Homeroom { get; set; }
        public string Role { get; set; }
    }

    public class UserRolePositionHomeroom : ItemValueVm
    {
        public int? Semester { get; set; }
    }
}
