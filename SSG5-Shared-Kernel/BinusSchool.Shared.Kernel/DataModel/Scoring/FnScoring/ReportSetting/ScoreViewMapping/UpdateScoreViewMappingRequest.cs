using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportSetting.ScoreViewMapping
{
    public class UpdateScoreViewMappingRequest
    {
        public string IdGrade { get; set; }
        public string IdReportScoreViewTemplate { get; set; }
        public List<UpdateScoreViewMapping_Subject> SubjectList { get; set; }
        public bool CurrentStatus { get; set; }
    }

    public class UpdateScoreViewMapping_Subject
    {
        public string Id { get; set; }
        public string OrderNo { get; set; }
    }
}
