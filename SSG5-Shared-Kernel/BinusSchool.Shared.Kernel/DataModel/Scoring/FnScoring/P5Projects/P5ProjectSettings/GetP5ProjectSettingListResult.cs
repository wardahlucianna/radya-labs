using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.P5Projects.P5ProjectSettings
{
    public class GetP5ProjectSettingListResult
    {
        public string IdP5Project { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public int Semester { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm P5Theme { get; set; }
        public string Title { get; set; }
    }
}
