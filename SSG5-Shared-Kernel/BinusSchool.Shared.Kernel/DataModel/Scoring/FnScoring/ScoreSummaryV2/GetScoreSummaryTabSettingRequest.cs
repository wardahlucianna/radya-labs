using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2
{
    public class GetScoreSummaryTabSettingRequest
    {
        public string IdUser { get; set; }
        public string IdStudent { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdGrade { set; get; }
        public int Semester { set; get; }
    }
}
