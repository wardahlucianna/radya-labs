using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectivesScoreGrade
{
    public class GetElectivesScoreGradeResult : CodeWithIdVm
    {
        public string IdSchool { get; set; }
        public decimal MinScore { get; set; }
        public decimal MaxScore { get; set; }
        public string Grade { get; set; }
    }
}
