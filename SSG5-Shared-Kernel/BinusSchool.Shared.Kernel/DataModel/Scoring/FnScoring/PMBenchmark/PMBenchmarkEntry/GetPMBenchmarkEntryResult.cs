using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Data.Model.Scoring.FnScoring.PMBenchmark.PMBenchmarkEntry
{
    public class GetPMBenchmarkEntryResult
    {
        public bool InPeriodEntry { set; get; }
        public bool IsLocked { get; set; }
        public DateTime? PeriodEntryStartDate { set; get; }
        public DateTime? PeriodEntryEndDate { set; get; }
        public GetPMBenchmarkEntry_Counter Counter { set; get; }
        public List<GetPMBenchmarkEntry_Header> Header { set; get; }
        public List<GetPMBenchmarkEntry_Body> Body { set; get; }
        public List<GetPMBenchMarkEntry_ScoreOptionList_InputType> OptionList { set; get; }
    }

    public class GetPMBenchmarkEntry_Counter
    {
        public int TotalCounter { set; get; }
        public int SubmittedScore { set; get; }
        public int UnsubmittedScore { set; get; }
    }

    public class GetPMBenchmarkEntry_Header
    {
        public string IdAssessmentComponentSetting { set; get; }
        public string Description { set; get; }
    }
    public class GetPMBenchmarkEntry_Body
    {
        public NameValueVm Student { set; get; }
        public List<GetPMBenchmarkEntry_Body_Score> ScoreList { set; get; }
    }
    public class GetPMBenchmarkEntry_Body_Score
    {
        public string AssessmentType { set; get; }
        public string IdScoreOption { set; get; }
        public string IdAssessmentComponentSetting { set; get; }
        public string IdAssessmentScore { set; get; }
        public decimal? Score { set; get; }
        public string IdUpdateTransaction { set; get; }
        public bool? StatusScore { set; get; }
        public GetPMBenchmarkEntry_Body_Score_option ScoreOption { set; get; }

    }
    public class GetPMBenchmarkEntry_Body_Score_option
    {
        public decimal minScoreLength { set; get; }
        public decimal maxScoreLength { set; get; }
    }

    public class GetPMBenchMarkEntry_ScoreOptionList
    {
        public List<GetPMBenchMarkEntry_ScoreOptionList_Radio> Radio { set; get; }
        public List<GetPMBenchMarkEntry_ScoreOptionList_Option> Option { set; get; }
    }
    public class GetPMBenchMarkEntry_ScoreOptionList_Radio
    {
        public string idScoreOptionDetail { set; get; }
        public string Description { set; get; }
    }
    public class GetPMBenchMarkEntry_ScoreOptionList_Option
    {
        public string idScoreOptionDetail { set; get; }
        public string Description { set; get; }
    }
    public class GetPMBenchMarkEntry_ScoreOptionList_InputType
    {
        public string IdScoreOption { set; get; }
        public string InputType { set; get; }
        public List<GetPMBenchMarkEntry_ScoreOptionList_InputType_detail> ScoreOptionDetail { set; get; }
    }
    public class GetPMBenchMarkEntry_ScoreOptionList_InputType_detail
    {
        public string IdScoreOptionDetail { set; get; }
        public short Key { set; get; }
        public string Description { set; get; }
    }


}
