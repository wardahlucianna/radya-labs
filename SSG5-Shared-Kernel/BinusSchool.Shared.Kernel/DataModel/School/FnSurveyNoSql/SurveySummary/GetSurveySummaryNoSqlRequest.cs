using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSurveyNoSql.SurveySummary
{
    public class GetSurveySummaryNoSqlRequest
    {
        public string IdPublishSurvey { get; set; }
        public int StartIndex { get; set; }
        public int Lenght { get; set; }
    }
}
