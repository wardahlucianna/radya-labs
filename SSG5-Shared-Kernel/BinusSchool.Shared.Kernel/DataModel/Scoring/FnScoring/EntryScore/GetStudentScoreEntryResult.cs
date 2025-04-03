using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scoring.FnScoring.EntryScore
{
    public class GetStudentScoreEntryResult
    {
        public string IdStudent { set; get; }
        public string StudentName { set; get; }
        public List<StudentScoreVm> StudentScoreList { set; get; }
        public GetStudentScoreEntry_SubjectScore SubjectScore { set; get; }
        public string PredictedGrade { set; get; }
        public bool IsExitStudent { set; get; }
        
    }

    public class StudentScoreVm
    {
        public string IdComponent { set; get; }
        public bool AverageSubComponentScore { set; get; }
        public string IdSubComponent { set; get; }
        public bool AverageCounterScore { set; get; }
        public string InputType { set; get; }
        public decimal MinScoreLength { set; get; }
        public decimal MaxScoreLength { set; get; }
        public List<ScoreOptionDetailVm> ScoreOptionDetail { set; get; }
        public string IdSubComponentCounter { set; get; }
        public string SubComponentCounterLongDesc { get; set; }
        public string IdScore { set; get; }
        public decimal? SubComponentCounterScore { set; get; }
        public decimal SubComponentCounterWeight { set; get; }
        public bool ShowCodeOption { set; get; }
        public UpdateScoreVm UpdateScore { set; get; }
        //Samuel 15 October 2021 - Add
        public string SubComponentTextScore { get; set; }
        public decimal? RawScore { set; get; }
        public decimal? MaxRawScore { set; get; }
        public bool CanAddCounter { get; set; }
    }

    public class ScoreOptionDetailVm
    {
        public Int16 Key { set; get; }
        public string Grade { set; get; }
        public string Description { set; get; }

    }

    public class UpdateScoreVm 
    {
        public string IdTransactionScore { set; get; }
        public decimal? MaxRawScore { set; get; }
        public decimal? RawScore { set; get; }
        public decimal? UpdateScore { set; get; }
        public string TextScore { set; get; }
        public ApprovalStatus StatusAction { set; get; }
        public bool IsEditable { set; get; }
        public string ForPositionActionNext { set; get; }
        public string IdUserActionNext { set; get; }
        public string IdApprovalState { set; get; }
    }



    public class StudentCounterScoreVm
    {
        public string IdStudent { set; get; }
        public string IdSubComponentCounter { set; get; }       
        public decimal? MaxRawScore { set; get; }
        public decimal? RawScore { set; get; }
        public decimal? SubComponentCounterScore { set; get; }
        public string TextScore { set; get; }
        public string IdScore { set; get; }

    }

    public class StudentUpdateScoreVm
    {
        public string IdTransactionScore { set; get; }
        public string IdScore { set; get; }
        public string IdStudent { set; get; }
        public string IdSubComponentCounter { set; get; }
        public decimal? MaxRawScore { set; get; }
        public decimal? RawScore { set; get; }
        public decimal? UpdateScore { set; get; }
        public string TextScore { set; get; }
        public ApprovalStatus StatusAction { set; get; }
        public DateTime? ActionDate { set; get; }
        public string ForPositionActionNext { set; get; }
        public string IdUserActionNext { set; get; }
        public string IdApprovalState { set; get; }

    }
    public class GetStudentScoreEntry_SubjectScore 
    {             
        public decimal Score { set; get; }
        public string Grading { set; get; }
        public string ShowScore { set; get; }       
    }



}
