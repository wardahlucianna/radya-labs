using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularSummary
{
    public class GetExtracurricularSummaryByStudentResult
    {
        public NameValueVm Extracurricular { get; set; }
        public string ExtracurricularType { get; set; }
        public int Semester { get; set; }
        public bool IsPrimary { get; set; }
        public bool? IsClub { get; set; }
        public string AttendancePercentage { get; set; }
        public string ScorePerformance { get; set; }
        public string AttendancePercentageFinal { get; set; }
        public string ScorePerformanceFinal { get; set; }
        public bool ShowScoreRC { get; set; }
        public double AttendanceScore { get; set; }
    }
}
