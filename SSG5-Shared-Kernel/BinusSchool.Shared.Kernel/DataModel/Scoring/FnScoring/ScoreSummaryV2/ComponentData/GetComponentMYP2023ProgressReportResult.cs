using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    public class GetComponentMYP2023ProgressReportResult
    {
        public string SubjectName { get; set; }
        public string Teacher { get; set; }
        public GetComponentMYP2023ProgressReportResult_AssesmentScore AssesmentScore { get; set; }
        public GetComponentMYP2023ProgressReportResult_Attendance Attendance { get; set; }
    }

    public class GetComponentMYP2023ProgressReportResult_AssesmentScore
    {
        public List<GetComponentMYP2023ProgressReportResult_Header> Header { get; set; }
        public List<GetComponentMYP2023ProgressReportResult_AssesmentScore_Body> Body { get; set; }
    }

    public class GetComponentMYP2023ProgressReportResult_Attendance
    {
        public List<GetComponentMYP2023ProgressReportResult_Header> Header { get; set; }
        public List<GetComponentMYP2023ProgressReportResult_Attendance_Body> Body { get; set; }
    }

    public class GetComponentMYP2023ProgressReportResult_Header
    {
        public string Id { get; set; }
        public string Description { get; set; }
    }

    public class GetComponentMYP2023ProgressReportResult_AssesmentScore_Body
    {
        public int No { get; set; }
        public string Date { get; set; }
        public string Type { get; set; }
        public string Assesment { get; set; }
        public List<GetComponentMYP2023ProgressReportResult_AssesmentScore_BodyScore> Score { get; set; }
    }

    public class GetComponentMYP2023ProgressReportResult_AssesmentScore_BodyScore
    {
        public string Id { get; set; }
        public string Score { get; set; }
    }

    public class GetComponentMYP2023ProgressReportResult_Attendance_Body
    {
        public string Id { get; set; }
        public string Value { get; set; }
    }


    //public class GetComponentIB2023SubjectScoreResult_ComponentData
    //{
    //    public string IdSubject { set; get; }
    //    public string SubjectName { set; get; }
    //    public string IdSubjectLevel { set; get; }
    //    public string IdLesson { set; get; }
    //    public int Semester { set; get; }
    //    public string IdComponent { set; get; }
    //    public string ComponentShortDesc { set; get; }
    //    public string ComponentLongDesc { set; get; }
    //    public bool ShowGradingAsScore { set; get; }
    //    public int OrderNoComponent { set; get; }
    //    public decimal? Score { set; get; }
    //    public string? Grading { set; get; }
    //}
}
