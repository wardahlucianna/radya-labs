using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2
{
    public class GetScoreSummaryTabSettingResult
    {
        public string IdSummaryTab { get; set; }
        public string SummaryTabDesc { get; set; }
        public GetScoreSummaryTabSettingResult_PdfDownload PdfDownload { set; get; }
        public bool ShowExcelDownload { set; get; }
    }

    public class GetScoreSummaryTabSettingResult_PdfDownload
    {
        public string IdHomeroomStudent { get; set; }
        public string IdReportType { get; set; }
        public string IdPeriod { set; get; }
    }

    public class GetScoreSummaryTabSettingResult_Detail
    {
        public string IdGrade { set; get; }
        public string IdPeriod { set; get; }
        public string Term { set; get; }
        public string IdHomeroomStudent { get; set; }
        public string? IdReportType { get; set; }
        public string IdScoreSummaryTab { get; set; }
        public string ScoreSummaryTabDesc { get; set; }
        public int OrderNumberTab { get; set; }
        public bool IsExportExcel { get; set; }
        public string IdScoreSummaryTabSection { get; set; }
        //public string? IdSubjectType { get; set; }
        //public string? SubjectTypeDesc { get; set; }
        public string ScoreTabTemplate { get; set; }
        public string? Content { get; set; }
        public string? MobileContent { get; set; }
        public string? WidthColumn { get; set; }
        public bool ShowTeacherName { get; set; }
        public bool ShowTotal { get; set; }
        public bool ShowSubjectLevel { get; set; }
        public int OrderNumberTabSection { get; set; }
        public bool HideInSemesterOne { get; set; }
    }

    public class GetScoreSummaryTabSettingResult_BlockPeriod
    {
        public bool IsUserParentOrStudent { set; get; }
        public bool IsBlockPeriodTermOne { set; get; }
        public bool IsBlockPeriodTermTwo { set; get; }
        public bool IsBlockPeriodTermThree { set; get; }
        public bool IsBlockPeriodTermFour { set; get; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
