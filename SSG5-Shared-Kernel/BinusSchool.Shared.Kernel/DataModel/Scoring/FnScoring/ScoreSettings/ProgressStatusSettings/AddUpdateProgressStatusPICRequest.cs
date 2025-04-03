using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ProgressStatusSettings
{
    public class AddUpdateProgressStatusPICRequest
    {
        public string IdAcademicYear { set; get; }
        public List<string> Levels { set; get; }
        public string? IdTeacherPosition { set; get; }
        public string? IdRole { set; get; }
        public bool EnableProgressStatus { set; get; }
        public bool EnableNationalStatus { set; get; }
        public bool EnableAgreement { set; get; }
        public bool EnableHideReportCard { set; get; }
    }
}
