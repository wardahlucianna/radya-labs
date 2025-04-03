using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData
{
    public class GetSpiritReportResult
    {
        public string HtmlOutput { get; set; }
    }

    public class GetSpiritReportResult_StudentComponentScore
    {
        public string IdAcademicYear { set; get; }
        public string IdSubject { set; get; }
        public string IdLesson { set; get; }
        public string IdGrade { set; get; }
        public string GradeDesc { set; get; }
        public int Semester { set; get; }
        public string Term { set; get; }
        public string IdSubjectScoreSetting { set; get; }
        public string IdComponent { set; get; }
        public string ComponentShortDesc { set; get; }
        public string ComponentLongDesc { set; get; }
        public bool ShowGradingAsScore { set; get; }
        public int OrderNoComponent { set; get; }
        public decimal? Score { set; get; }
        public string? Grading { set; get; }
    }
}
