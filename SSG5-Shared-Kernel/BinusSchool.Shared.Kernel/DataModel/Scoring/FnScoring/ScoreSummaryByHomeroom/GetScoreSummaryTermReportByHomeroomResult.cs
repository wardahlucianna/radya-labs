using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using NPOI.OpenXmlFormats.Dml;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryByHomeroom
{
    public class GetScoreSummaryTermReportByHomeroomResult
    {
        public List<GetScoreSummaryTermReportByHomeroomResult_Header> Header { get; set; }
        public List<GetScoreSummaryTermReportByHomeroomResult_Body> Body { get; set; }

    }
    
    public class GetScoreSummaryTermReportByHomeroomResult_Header
    {
        public ItemValueVm Subject { get; set; }
        public List<ItemValueVm> Component { get; set; }
    }

    public class GetScoreSummaryTermReportByHomeroomResult_Body
    {
        public ItemValueVm Homeroom { get; set; }
        public ItemValueVm Student { get; set; } 
        public List<GetScoreSummaryTermReportByHomeroomResult_Body_Component> Components { get; set; }
    }

    public class GetScoreSummaryTermReportByHomeroomResult_Body_Component
    {
        public string IdSubject { get; set; }
        public List<GetScoreSummaryTermReportByHomeroomResult_Body_Component_Score> Score { get; set; }
    }

    public class GetScoreSummaryTermReportByHomeroomResult_Body_Component_Score
    {
        public string Id { get; set; }
        public string Score { get; set; }
    }

    public class GetScoreSummaryTermReportByHomeroomResult_ComponentVm
    {
        public string IdHomeroom { get; set; }
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdLesson { get; set; }
        public string IdSubject { get; set; }
        public string SubjectName { get; set; }
        public string IdSubComponentCounter { get; set; }
        public string SubjectId { get; set; }
        public string IdSubComponent { get; set; }
        public string IdComponent { get; set; }
        public string ComponentDescription { get; set; }
        public int? ComponentOrderNumber { get; set; }
        public string IdScoreOption { get; set; }
        public bool SubjectScoreSettingShowGradingAsScore { get; set; }
        public bool ComponentShowGradingAsScore { get;set; }
        public string? Score { get; set; }
        public string? SubjectNameReplacement { get; set; }
        public string? Classroom { get; set; }
    }
    
    public class GetScoreSummaryTermReportByHomeroomResult_HeaderInformation
    {
        public string School { get; set; }
        public string AcademicYear { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Term { get; set; }
    }
}
