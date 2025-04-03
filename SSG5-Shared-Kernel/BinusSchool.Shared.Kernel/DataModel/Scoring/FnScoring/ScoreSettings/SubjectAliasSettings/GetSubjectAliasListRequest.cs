using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.SubjectAliasSettings
{
    public class GetSubjectAliasListRequest : CollectionRequest
    {
        public string IdGrade { get; set; }
    }
}
