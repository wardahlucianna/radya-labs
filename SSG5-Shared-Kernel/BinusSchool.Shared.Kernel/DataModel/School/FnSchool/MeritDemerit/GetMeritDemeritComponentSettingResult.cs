using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.MeritDemerit
{
    public class GetMeritDemeritComponentSettingResult : CodeWithIdVm
    {
        public string AcademicYear { get; set; }
        public string IdAcademicYear { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string IdGrade { get; set; }
        public string IdMeritDemeritCompSetting { get; set; }
        public bool IsPointSystem { get; set; }
        public bool IsDemeritSystem { get; set; }
    }
}
