using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition
{
    public class GetUserRolePositionRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdSchool { get; set; }
        public string IdUser { get; set; }
        public List<GetUserRolePosition> UserRolePositions { get; set; }
    }

    public class GetUserRolePosition 
    {
        public string IdUserRolePositions {  get; set; } 
        public UserRolePersonalOptionRole Role {  get; set; } 
        public UserRolePersonalOptionType Option {  get; set; }
        public List<string> Departemens { get; set; }
        public List<string> TeacherPositions { get; set; }
        public List<string> Level { get; set; }
        public List<string> Homeroom { get; set; }
        public List<string> Personal { get; set; }
    }

}
