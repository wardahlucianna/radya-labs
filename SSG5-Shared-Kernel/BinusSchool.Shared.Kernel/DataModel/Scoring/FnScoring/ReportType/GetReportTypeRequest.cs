using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportType
{
    public class GetReportTypeRequest : CollectionRequest
    {
        public string IdSchool { set; get; }
    }
}
