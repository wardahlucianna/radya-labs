using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.P5Projects.P5ProjectSettings
{
    public class GetP5ProjectSettingListRequest
    {
        public string IdAcademicYear { get; set; }
        public int? Semester { get; set; }
        public string IdGrade { get; set; }
        public string IdP5Theme { get; set; }
    }
}
