using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition
{
    public class GetStudentRolePositionResult 
    {
        public string IdStudent { get; set; }
        public string IdHomeroomStudent { get; set; }
        public ItemValueVmWithOrderNumber Level { get; set; }
        public ItemValueVmWithOrderNumber Grade { get; set; }
        public GetSubjectHomeroom Homeroom { get; set; }
    }
}
