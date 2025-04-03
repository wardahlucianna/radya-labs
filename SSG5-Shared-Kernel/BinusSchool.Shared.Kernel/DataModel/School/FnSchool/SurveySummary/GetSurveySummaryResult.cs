using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchool.SurveySummary
{
    public class GetSurveySummaryResult : ItemValueVm
    {
        public string AcademicYear { get; set; }
        public string SurveyName { get; set; }
        public SurveyTemplateType SurveyTemplateTypeEnum { get; set; }
        public string SurveyTemplateType { get; set; }
        public int Semester { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double TotalRespondent { get; set; }
        public List<string> Role { get; set; }


    }
}
