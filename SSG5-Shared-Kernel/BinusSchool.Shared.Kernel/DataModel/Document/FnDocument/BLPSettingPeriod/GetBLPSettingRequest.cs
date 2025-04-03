using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.BLPSettingPeriod
{
    public class GetBLPSettingRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdSurveyCategory { get; set; }
    }
}
