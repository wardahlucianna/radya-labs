﻿using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.PmBenchmarkSettings
{
    public class GetPmBenchmarkMappingSettingsRequest : CollectionRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string PeriodCode { get; set; }
        public string ComponentName { get; set; }
        public bool GetAllId { get; set; }
    }
}
