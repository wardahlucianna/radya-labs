﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryByHomeroom
{
    public class GetScoreSummaryStatisticByHomeroomRequest
    {
        public string IdAcademicYear { set; get; }
        public int Semester { set; get; }
        public string IdSchool { set; get; }
        public string IdLevel { set; get; }
        public string IdGrade { set; get; }
        public string IdScoreStatisticCategory { set; get; }
    }
}
