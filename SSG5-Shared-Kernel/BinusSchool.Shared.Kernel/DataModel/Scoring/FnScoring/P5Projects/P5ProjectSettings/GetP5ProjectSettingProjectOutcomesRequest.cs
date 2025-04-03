using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.P5Projects.P5ProjectSettings
{
    public class GetP5ProjectSettingProjectOutcomesRequest
    {
        public string IdSchool { get; set; }
        public List<string> IdGrade { get; set; }
    }
}
