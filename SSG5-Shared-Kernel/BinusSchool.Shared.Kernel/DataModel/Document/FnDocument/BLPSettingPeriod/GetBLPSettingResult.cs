using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.BLPSettingPeriod
{
    public class GetBLPSettingResult : CodeWithIdVm
    {
        public string IdSurvey { get; set; }
        public string IdSurveyCategory { get; set; }
        public string SurveyCategoryName { get; set; }
        public string AcademicYear { get; set; }
        public int Semester { get; set; }
        public string Grade { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }

    }
}
