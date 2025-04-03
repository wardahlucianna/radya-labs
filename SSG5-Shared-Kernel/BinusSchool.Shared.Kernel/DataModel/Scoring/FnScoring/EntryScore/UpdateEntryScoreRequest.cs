using System;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scoring.FnScoring.EntryScore
{
    public class UpdateEntryScoreRequest
    {
        public string IdApprovalWorkflow { set; get; }
        //public string IdUserActionNext { set; get; }
        public List<UpdateEntryScore> UpdateEntryScoreList { set; get; }
        public bool IsInsertWithNewCounter { set; get; }
        public newCounterVm InsertWithNewCounter { set; get; }
        public List<updateMaxRawScoreVm> UpdateMaxRawScoreList { set; get; }

    }
    public class UpdateEntryScore
    {
        public string IdTransactionScore { set; get; } //untuk ngedraft
        public string IdScore { set; get; } //update sore
        public string IdStudent { set; get; }
        public string IdComponent { set; get; }
        public string IdSubComponent { set; get; }
        public string IdSubComponentCounter { set; get; }
        public string TextScore { set; get; }
        public decimal? MaxRawScore { set; get; }
        public decimal? RawScore { set; get; }

        public string OldTextScore { set; get; }
        public decimal? OldMaxRawScore { set; get; }
        public decimal? OldRawScore { set; get; }
        public bool NeedApproval { set; get; }
        public bool CodeNeedApproval { set; get; }
        public ApprovalStatus Status { set; get; }
        public decimal SubComponentMaxScoreLength { set; get; }
        //public string PredictedGrade { set; get; }


    }

    public class newCounterVm
    {
        public string IdTeacherPosition { set; get; }
        public string IdCounter { set; get; }
        public string LongDesc { set; get; }
        public string IdCounterCategory { set; get; }
        public decimal MaxRawScore { set; get; }
        public decimal Weight { set; get; }
        public string IdLesson { set; get; }
        public DateTime DateCounter { set; get; }
        public List<string> IdSubComponentList { set; get; }
    }

    public class updateMaxRawScoreVm
    {
        public string IdSubComponentCounter { set; get; }
        public decimal MaxRawScore { set; get; }
    }

    public class rekalkulasiStudentScoreVm
    {
        public string IdSubjectScore { set; get; }
        public string IdComponent { set; get; }
        public string IdSubComponent { set; get; }
        public string IdSubComponentCounter { set; get; }
        public string IdSubjectScoreSetting { set; get; }
        public decimal Weight { set; get; }
        public decimal Score { set; get; }
        public bool IsAvg { set; get; }
        public bool isProrateForSubjectScore { set; get; }
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
