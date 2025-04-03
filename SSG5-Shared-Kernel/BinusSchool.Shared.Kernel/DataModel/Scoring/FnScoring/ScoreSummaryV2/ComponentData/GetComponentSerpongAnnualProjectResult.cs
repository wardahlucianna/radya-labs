using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using BinusSchool.Common.Model;
using NPOI.OpenXmlFormats.Dml;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    public class GetComponentSerpongAnnualProjectResult
    {
        public string SubjectName { get; set; }
        public ItemValueVm Period { get; set; }
        public int Semester { get; set; }
        public GetComponentSerpongAnnualProjectResult_Subject SubjectScoreDescription { get; set; }
        public decimal? Score { get; set; }
        public string Grading { get; set; }
    }

    public class GetComponentSerpongAnnualProjectResult_Subject
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
