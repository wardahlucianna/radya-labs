﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData
{
    public class GetScoreCompetenciesSubjectsRequest
    {
        public string ReportScoreTemplate { get; set; }
        public string ReportScoreTable { get; set; }
        //public bool IsExaminable { get; set; }
        public string IdStudent { get; set; }
        public string IdGrade { get; set; }
        public int Semester { get; set; }
        public string IdPeriod { get; set; }
    }
}
