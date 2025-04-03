using System;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scoring.FnScoring.UpdateScore
{
    public class ApprovalUpdateScoreRequest
    {
        public string IdApprovalWorkflow { set; get; }
        public string IdSchool { set; get; }
        public ApprovalAction Action { set; get; }
        public string Comment { set; get; }
        public ApprovalUpdateScoreStudent ApprovalUpdateScoreStudent { set; get; }
    }
    public class ApprovalUpdateScoreStudent
    {
        public string IdTransactionScore { set; get; }
        public string IdScore { set; get; }
        public string IdStudent { set; get; }
        public string IdComponent { set; get; }
        public string IdSubComponent { set; get; }
        public string TextScore { set; get; }
        public decimal? MaxRawScore { set; get; }
        public decimal? RawScore { set; get; }
        public decimal SubComponentMaxScoreLength { set; get; }
    }

    public class ApproveRekalkulasiStudentScoreVm
    {
        public string IdSubjectScore { set; get; }
        public string IdComponent { set; get; }
        public string IdSubComponent { set; get; }
        public string IdSubComponentCounter { set; get; }
        public string IdSubjectScoreSetting { set; get; }
        public decimal Weight { set; get; }
        public decimal Score { set; get; }
        public bool IsAvg { set; get; }
        public bool IsProrateScore { set; get; }
        public List<SubjectScoreLegendVm> SubjectScoreLegendList { set; get; }
        public string IdSubject { set; get; }
        public string IdSubjectMappingSubjectLevel { set; get; }
    }

    public class SubjectScoreLegendVm
    {
        public decimal Min { set; get; }
        public decimal Max { set; get; }
        public string Grade { set; get; }
    }

}
