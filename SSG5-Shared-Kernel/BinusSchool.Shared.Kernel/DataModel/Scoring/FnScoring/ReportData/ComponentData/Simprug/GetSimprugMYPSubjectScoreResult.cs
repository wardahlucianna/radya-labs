﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData.Simprug
{
    public class GetSimprugMYPSubjectScoreResult
    {
        public string HtmlOutput { get; set; }
        public int TotalSubjectProgressionReq { get; set; }
        public List<string> GenerateStatus { get; set; }
    }
}
