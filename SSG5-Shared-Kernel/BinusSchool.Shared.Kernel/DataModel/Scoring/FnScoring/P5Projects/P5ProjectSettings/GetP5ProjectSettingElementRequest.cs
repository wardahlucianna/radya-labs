using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.P5Projects.P5ProjectSettings
{
    public class GetP5ProjectSettingElementRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public string IdP5Dimension { get; set; }
    }
}
