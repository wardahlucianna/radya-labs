using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.UpdateScore
{
    public class GetStudentScoreForApprovalRequest
    {
        public string IdStudent { set; get; }
        public string IdSubComponentCounter { get; set; }
    }
}
