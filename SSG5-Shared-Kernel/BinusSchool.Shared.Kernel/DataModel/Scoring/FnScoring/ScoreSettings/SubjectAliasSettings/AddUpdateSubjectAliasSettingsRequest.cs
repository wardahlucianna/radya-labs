using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.SubjectAliasSettings
{
    public class AddUpdateSubjectAliasSettingsRequest
    {
        public string IdSubjectAlias { get; set; }
        public string Alias { get; set; }
        public string NationalReportAlias { get; set; }
        public string IdSubject { get; set; }
    }
}
