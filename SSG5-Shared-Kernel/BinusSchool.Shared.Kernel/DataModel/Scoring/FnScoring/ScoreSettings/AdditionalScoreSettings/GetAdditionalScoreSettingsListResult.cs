using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.AdditionalScoreSettings
{
    public class GetAdditionalScoreSettingsListResult : CodeWithIdVm
    {
        public CodeWithIdVm AcademicYear { set; get; }
        public int OrderNoAcademicYear { set; get; }
        public CodeWithIdVm Level { set; get; }
        public int OrderNoLevel { set; get; }
        public GetAdditionalScoreSettingsResult_GradeKeyVal Grade { set; get; }
        public int OrderNoGrade { set; get; }
        public decimal Key { set; get; }
        public string Category { set; get; }
        public bool ShowScoreAsNA { set; get; }
        public bool CurrentStatus { set; get; }
        public bool ShowActionDelete { set; get; }
    }

    public class GetAdditionalScoreSettingsResult_GradeKeyVal : CodeWithIdVm
    {
        public string TagHtml { set; get; }
    }
}
