using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportSetting.ViewTemplate
{
    public class GetScoreViewTemplateDetailResult
    {
        public string IdReportScoreViewTemplate { set; get; }
        public string ShortDesc { set; get; }
        public string LongDesc { set; get; }
        public string Template { set; get; }
        public string Code { set; get; }
        public string IdSchool { set; get; }
        public bool CanEdit { set; get; }
        public bool CurrentStatus { set; get; }

    }
}
