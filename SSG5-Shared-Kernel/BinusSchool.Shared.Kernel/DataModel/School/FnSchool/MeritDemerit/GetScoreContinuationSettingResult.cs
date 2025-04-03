using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.MeritDemerit
{
    public class GetScoreContinuationSettingResult : CodeWithIdVm
    {
        public string AcademicYear { get; set; }
        public string IdAcademicYear { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string IdGrade { get; set; }
        public bool IsPointSystem { get; set; }
        public int? Score { get; set; }
        public ItemValueVm Option { get; set; }
        public ItemValueVm Every { get; set; }
        public ItemValueVm Operation { get; set; }
    }
}
