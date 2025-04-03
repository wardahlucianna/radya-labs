using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreApproval.ApprovalReportScore
{
    public class GetApprovalReportScoreRequest
    {
        public string IdUser { set; get; }
        public string IdSchool { set; get; }
        public string IdAcademicYear { set; get; }
        public string IdLevel { set; get; }
        public string IdGrade { set; get; }
        public string Term { set; get; }
        public string? IdSubject { set; get; }
        public int? ScoreStatus { set; get; }
        public int? ApprovalStatus { set; get; }
    }
}
