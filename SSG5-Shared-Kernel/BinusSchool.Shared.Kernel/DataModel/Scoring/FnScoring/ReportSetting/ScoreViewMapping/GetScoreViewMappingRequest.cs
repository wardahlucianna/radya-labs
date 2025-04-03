using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportSetting.ScoreViewMapping
{
    public class GetScoreViewMappingRequest : CollectionRequest
    {
        public string IdSchool { set; get; }
        public string IdAcademicYear { set; get; }
        public string IdLevel { set; get; }
        public string IdGrade { set; get; }
        public string IdSubject { set; get; }     
    }
}
