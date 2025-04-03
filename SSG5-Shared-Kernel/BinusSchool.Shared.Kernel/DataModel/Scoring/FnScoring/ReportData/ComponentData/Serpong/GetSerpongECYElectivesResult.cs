using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData.Serpong
{
    public class GetSerpongECYElectivesResult
    {
        public string HtmlOutput { get; set; }
        public List<string> GenerateStatus { get; set; }
    }

    public class GetSerpongECYElectivesResult_ElectiveAttendance
    {
        public NameValueVm Extracurricular { get; set; }
        public string AttendancePercentage { get; set; }
        public string ScorePerformance { get; set; }
        public int Semester { get; set; }
    }
}
