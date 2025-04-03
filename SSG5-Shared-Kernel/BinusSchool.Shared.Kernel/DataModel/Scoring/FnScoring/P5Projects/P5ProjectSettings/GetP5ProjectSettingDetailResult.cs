using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.P5Projects.P5ProjectSettings
{
    public class GetP5ProjectSettingDetailResult
    {
        public string IdP5Project { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public ItemValueVm Semester { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm P5Theme { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<GetP5ProjectSettingDetailResult_ProjectOutcomes> ProjectOutcomes { get; set; }
        public List<GetP5ProjectSettingDetailResult_PIC> PIC { get; set; }
    }

    public class GetP5ProjectSettingDetailResult_ProjectOutcomes
    {
        public string IdP5ProjectOutcomes { get; set; }
        public string Dimension { get; set; }
        public string Element { get; set; }
        public string SubElement { get; set; }
        public string P5ProjectOutcomesDescription { get; set; }
        public string KurnasPhaseDesc { get; set; }
        public bool IsCanDelete { get; set; }
    }

    public class GetP5ProjectSettingDetailResult_PIC
    {
        public string IdBinusian { get; set; }
        public string Description { get; set; }
    }
}
