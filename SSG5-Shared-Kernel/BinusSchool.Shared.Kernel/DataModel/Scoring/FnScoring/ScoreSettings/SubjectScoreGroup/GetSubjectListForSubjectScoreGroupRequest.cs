using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.SubjectScoreGroup
{
    public class GetSubjectListForSubjectScoreGroupRequest : CollectionRequest
    {
        public string? IdSubjectDPGroup { set; get; }
        public string IdAcademicYear { set; get; }
        public List<string> IdLevels { set; get; }
        public List<string?> IdGrades { set; get; }
    }
}
