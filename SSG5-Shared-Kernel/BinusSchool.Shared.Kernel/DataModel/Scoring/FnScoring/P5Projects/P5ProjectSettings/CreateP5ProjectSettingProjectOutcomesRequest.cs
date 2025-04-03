using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.P5Projects.P5ProjectSettings
{
    public class CreateP5ProjectSettingProjectOutcomesRequest
    {
        public List<string> IdGrade { get; set; }
        public string IdDimension { get; set; }
        public string IdElement { get; set; }
        public string IdSubElement { get; set; }
        public string ProjectOutcomesDescription { get; set; }
    }
}
