using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.Filter
{
    public class GetSpecificGradeByPositionRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdUser { get; set; }
        public string PositionShortName { get; set; }
    }
}
