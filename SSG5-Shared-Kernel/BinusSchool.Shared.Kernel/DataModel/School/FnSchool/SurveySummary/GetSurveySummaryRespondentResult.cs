using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.SurveySummary
{
    public class GetSurveySummaryRespondentResult : ItemValueVm
    {
        public int OrderNumber { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
        public string Role { get; set; }
        public double Total { get; set; }
        public double TotalRespondent { get; set; } // total respondent telah mengisi survey
        public double TotalNotAnswer { get; set; }
        public double Percent { get; set; }
    }
}
