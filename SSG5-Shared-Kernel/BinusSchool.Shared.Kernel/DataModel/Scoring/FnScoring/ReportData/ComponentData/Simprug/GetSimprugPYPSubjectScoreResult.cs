﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData.Simprug
{
    public class GetSimprugPYPSubjectScoreResult
    {
        public string HtmlOutput { get; set; }     
        public List<string> GenerateStatus { get; set; }
        public int CurrPage { get; set; }
    }
}
