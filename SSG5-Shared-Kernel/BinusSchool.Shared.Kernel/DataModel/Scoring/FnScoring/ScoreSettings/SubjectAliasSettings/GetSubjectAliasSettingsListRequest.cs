using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.SubjectAliasSettings
{
    public class GetSubjectAliasSettingsListRequest
    {
        public string IdAcademicYear { get; set; }
        public string? IdLevel { get; set; }
        public string? IdGrade { get; set; }
        public string? IdSubject { get; set; }
    }
}
