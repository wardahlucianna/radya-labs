using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData
{
    public class GetP5ProjectScoreDataResult
    {
        public string IdP5Project { get; set; }
        public string ProjectTitle { get; set; }
        public string ProjectDesc { get; set; }
        public string IdP5Theme { get; set; }
        public string ThemeDesc { get; set; }
        public string IdStudent { get; set; }
        public List<GetP5ProjectScoreDataResult_Outcomes> Outcomes { get; set; }
    }

    public class GetP5ProjectScoreDataResult_Outcomes
    {
        public string IdDimension { get; set; }
        public string DimensionDesc { get; set; }
        public string IdElement { get; set; }
        public string ElementDesc { get; set; }
        public List<GetP5ProjectScoreDataResult_SubElement> SubElements { get; set; }
        public List<GetP5ProjectScoreDataResult_ProjectOutcomes> ProjectOutcomes { get; set; }
    }

    public class GetP5ProjectScoreDataResult_SubElement
    {
        public string IdSubElement { get; set; }
        public string SubElementDesc { get; set; }
    }

    public class GetP5ProjectScoreDataResult_ProjectOutcomes
    {
        public string IdProjectOutcomes { get; set; }
        public string ProjectOutcomesDesc { get; set; }
        public int Score { get; set; }
    }
}
