using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.MeritDemerit
{
    public class UpdateMeritDemeritDisciplineAprovalRequest
    {
        public List<MeritDemeritApprovalSetting> MeritDemeritApprovalSetting { get; set; }
        public string IdAcademic { get; set; }
        public string IdLevel { get; set; }
       
    }

    public class MeritDemeritApprovalSetting 
    {
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdTeacherPositionApproval1 { get; set; }
        public string IdTeacherPositionApproval2 { get; set; }
        public string IdTeacherPositionApproval3 { get; set; }
    }
}
