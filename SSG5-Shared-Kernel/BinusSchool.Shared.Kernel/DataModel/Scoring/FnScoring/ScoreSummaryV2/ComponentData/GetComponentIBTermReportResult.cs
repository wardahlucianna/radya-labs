using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    public class GetComponentIBTermReportResult
    {
        public List<GetComponentIBTermReportResult_Header> Header { get; set; }
        public List<GetComponentIBTermReportResult_Body> Body { get; set; }
        public string SubjectName { get; set; }
    }

    public class GetComponentIBTermReportResult_Header
    {
        public string Period { get; set; }
        public List<ItemValueVm> Component { get; set; }
    }

    public class GetComponentIBTermReportResult_Body
    {
        public string Subject { get; set; }
        public ItemValueVm Teacher { get; set; }
        public List<GetComponentIBTermReportResult_Body_Score> Score { get; set; }
    }

    public class GetComponentIBTermReportResult_Body_Score
    {
        public string IdComponent { get; set; }
        public string Score { get; set; }
    }
}
