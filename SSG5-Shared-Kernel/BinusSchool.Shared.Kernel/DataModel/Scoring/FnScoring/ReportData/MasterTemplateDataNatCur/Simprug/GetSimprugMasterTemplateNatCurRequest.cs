﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateDataNatCur.Simprug
{
    public class GetSimprugMasterTemplateNatCurRequest
    {
        public string IdReportType { get; set; }
        public string IdReportTemplate { get; set; }
        public string TemplateName { get; set; }
        public string IdStudent { get; set; }
        public string IdSchool { get; set; }
        public string IdGrade { get; set; }
        public int Semester { get; set; }
        public string IdPeriod { get; set; }
    }
}
