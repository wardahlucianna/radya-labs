using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition 
{
    public class GetUserSubjectByEmailRecepientResult : GetSubjectByUserResult
    {
        public string IdUser { get; set; }
        public string IdTeacherPosition { get; set; }
    }
}
