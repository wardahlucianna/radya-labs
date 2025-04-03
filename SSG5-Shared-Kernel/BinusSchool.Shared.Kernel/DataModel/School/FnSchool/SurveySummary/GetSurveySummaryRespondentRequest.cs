using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchool.SurveySummary
{
    public class GetSurveySummaryRespondentRequest : CollectionSchoolRequest
    {
        public string Id { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public SurveyTemplateType Type { get; set; }
    }
}
