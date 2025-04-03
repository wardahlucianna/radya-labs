using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportTypeMapping
{
    public class GetListReportTypeMappingResult
    {
        public CodeWithIdVm AcademicYear { set; get; }
        //public CodeWithIdVm Level { set; get; }
        public List<CodeWithIdVm> Grade { set; get; }
        public ItemValueVm ReportType { set; get; }
        public bool Status { set; get; }
        public string IdReportTypeMapping { set; get; }
    }
}
