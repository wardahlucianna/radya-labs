using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportSetting.ViewTemplate
{
    public class GetScoreViewTemplateRequest : CollectionRequest
    {
        public string IdSchool { set; get; }
        public bool? Status { set; get; }
    }
}
