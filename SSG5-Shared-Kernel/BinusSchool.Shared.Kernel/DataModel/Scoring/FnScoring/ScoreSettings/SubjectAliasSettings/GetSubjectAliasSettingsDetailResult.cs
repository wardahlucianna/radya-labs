using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.SubjectAliasSettings
{
    public class GetSubjectAliasSettingsDetailResult
    {
        public string IdSubjectAlias { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm Subject { get; set; }
        public string Alias { get; set; }
        public string NationalReportAlias { get; set; }
    }
}
