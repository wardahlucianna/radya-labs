﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryByHomeroom
{
    public class GetScoreSummaryStatisticCategoryByHomeroomRequest
    {
        public string IdSchool { set; get; }
        public string IdAcademicYear { set; get; }
        public string IdLevel { set; get; }
    }
}
