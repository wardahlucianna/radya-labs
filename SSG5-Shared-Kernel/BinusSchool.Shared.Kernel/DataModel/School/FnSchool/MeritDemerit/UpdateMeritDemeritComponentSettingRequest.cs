using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.MeritDemerit
{
    public class UpdateMeritDemeritComponentSettingRequest
    {
        public string IdSchool { get; set; }
        public List<MeritDemeritCompSetting> MeritDemeritComponentSetting { get; set; }

    }

    public class MeritDemeritCompSetting 
    {
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public bool IsPointSystem { get; set; }
        public bool IsDemeritSystem { get; set; }
    }
}
